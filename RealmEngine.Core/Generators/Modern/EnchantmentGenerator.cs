using RealmEngine.Data.Services;
using RealmEngine.Shared.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace RealmEngine.Core.Generators.Modern;

/// <summary>
/// Generates enchantments using the v4.1 positional pattern system.
/// Part of the Hybrid Enhancement System v1.0.
/// </summary>
public class EnchantmentGenerator
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly Random _random;
    private readonly ILogger<EnchantmentGenerator> _logger;
    private readonly NameComposer _nameComposer;

    public EnchantmentGenerator(GameDataCache dataCache, ReferenceResolverService referenceResolver, ILogger<EnchantmentGenerator> logger)
    {
        _dataCache = dataCache ?? throw new ArgumentNullException(nameof(dataCache));
        _referenceResolver = referenceResolver ?? throw new ArgumentNullException(nameof(referenceResolver));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = new Random();
        
        // Create a NullLogger for NameComposer (it doesn't need logging for our use case)
        _nameComposer = new NameComposer(NullLogger<NameComposer>.Instance);
    }

    /// <summary>
    /// Generate a single enchantment from a reference.
    /// Uses positional pattern system to determine prefix/suffix placement.
    /// </summary>
    public Task<Enchantment?> GenerateEnchantmentAsync(string reference)
    {
        try
        {
            var namesPath = "items/enchantments/names.json";
            if (!_dataCache.FileExists(namesPath)) return Task.FromResult<Enchantment?>(null);
            
            var namesFile = _dataCache.GetFile(namesPath);
            if (namesFile?.JsonData == null)
            {
                return Task.FromResult<Enchantment?>(null);
            }

            // Get patterns
            var patterns = namesFile.JsonData["patterns"];
            if (patterns == null || !patterns.Any())
            {
                return Task.FromResult<Enchantment?>(null);
            }

            // Select random pattern
            var pattern = GetRandomWeightedPattern(patterns);
            if (pattern == null)
            {
                return Task.FromResult<Enchantment?>(null);
            }

            var patternString = GetStringProperty(pattern, "pattern");
            if (string.IsNullOrEmpty(patternString))
            {
                return Task.FromResult<Enchantment?>(null);
            }

            // Get components
            var components = namesFile.JsonData["components"];
            if (components == null)
            {
                return Task.FromResult<Enchantment?>(null);
            }

            // Use NameComposer to resolve pattern with {base} placeholder
            var (name, baseName, prefixes, suffixes) = _nameComposer.ComposeNameWithComponents(patternString, components);
            
            // Determine position based on whether name has prefixes or suffixes
            EnchantmentPosition position;
            JToken? componentData = null;
            
            if (prefixes.Any())
            {
                // Pattern has tokens before {base} - this is a prefix enchantment
                position = EnchantmentPosition.Prefix;
                // Get the component data for the prefix
                var prefixToken = prefixes.First().Token;
                var prefixValue = prefixes.First().Value;
                componentData = FindComponentByValue(components[prefixToken], prefixValue);
            }
            else if (suffixes.Any())
            {
                // Pattern has tokens after {base} - this is a suffix enchantment
                position = EnchantmentPosition.Suffix;
                // Get the component data for the suffix
                var suffixToken = suffixes.First().Token;
                var suffixValue = suffixes.First().Value;
                componentData = FindComponentByValue(components[suffixToken], suffixValue);
            }
            else
            {
                // Fallback - shouldn't happen with valid patterns
                return Task.FromResult<Enchantment?>(null);
            }

            if (componentData == null)
            {
                return Task.FromResult<Enchantment?>(null);
            }

            // Get the actual enchantment name (without the {base} placeholder)
            var enchantmentName = prefixes.Any() ? prefixes.First().Value : suffixes.First().Value;
            
            // Build enchantment from component
            return Task.FromResult<Enchantment?>(BuildEnchantment(componentData, enchantmentName, position));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating enchantment");
            return Task.FromResult<Enchantment?>(null);
        }
    }
    
    /// <summary>
    /// Find a component by its value.
    /// </summary>
    private JToken? FindComponentByValue(JToken? componentsArray, string value)
    {
        if (componentsArray == null) return null;
        
        foreach (var component in componentsArray.Children())
        {
            if (GetStringProperty(component, "value") == value)
            {
                return component;
            }
        }
        
        return null;
    }

    /// <summary>
    /// Generate multiple enchantments.
    /// </summary>
    public async Task<List<Enchantment>> GenerateEnchantmentsAsync(int count)
    {
        var result = new List<Enchantment>();
        
        for (int i = 0; i < count; i++)
        {
            var enchantment = await GenerateEnchantmentAsync("@items/enchantments:*");
            if (enchantment != null)
            {
                result.Add(enchantment);
            }
        }
        
        return result;
    }

    /// <summary>
    /// Build an Enchantment model from a component.
    /// </summary>
    private Enchantment BuildEnchantment(JToken component, string name, EnchantmentPosition position)
    {
        var enchantment = new Enchantment
        {
            Name = name,
            RarityWeight = GetIntProperty(component, "rarityWeight", 50),
            Position = position,
            Description = "Enchantment"
        };

        // Set rarity based on weight
        enchantment.Rarity = ConvertWeightToEnchantmentRarity(enchantment.RarityWeight);

        // Apply traits from component
        var traits = component["traits"];
        if (traits != null)
        {
            foreach (var traitProp in traits.Children<JProperty>())
            {
                var traitName = traitProp.Name;
                var traitData = traitProp.Value;
                
                var traitValue = new TraitValue
                {
                    Value = traitData["value"]?.ToObject<object>(),
                    Type = ParseTraitType(traitData["type"]?.ToString() ?? "number")
                };
                
                enchantment.Traits[traitName] = traitValue;
            }
        }

        return enchantment;
    }

    /// <summary>
    /// Convert rarity weight to enchantment rarity enum.
    /// </summary>
    private static EnchantmentRarity ConvertWeightToEnchantmentRarity(int weight)
    {
        return weight switch
        {
            < 40 => EnchantmentRarity.Minor,
            < 80 => EnchantmentRarity.Lesser,
            < 120 => EnchantmentRarity.Greater,
            < 180 => EnchantmentRarity.Superior,
            _ => EnchantmentRarity.Legendary
        };
    }

    /// <summary>
    /// Parse trait type string to TraitType enum.
    /// </summary>
    private static TraitType ParseTraitType(string typeString)
    {
        return typeString?.ToLower() switch
        {
            "number" => TraitType.Number,
            "string" => TraitType.String,
            "boolean" => TraitType.Boolean,
            "stringarray" => TraitType.StringArray,
            "numberarray" => TraitType.NumberArray,
            _ => TraitType.Number
        };
    }

    /// <summary>
    /// Get a random weighted pattern.
    /// </summary>
    private JToken? GetRandomWeightedPattern(JToken patterns)
    {
        var patternList = patterns.Children().ToList();
        if (!patternList.Any()) return null;

        var totalWeight = patternList.Sum(p => GetIntProperty(p, "rarityWeight", 1));
        var randomValue = _random.Next(1, totalWeight + 1);

        int currentWeight = 0;
        foreach (var pattern in patternList)
        {
            currentWeight += GetIntProperty(pattern, "rarityWeight", 1);
            if (randomValue <= currentWeight)
            {
                return pattern;
            }
        }

        return patternList.First();
    }

    /// <summary>
    /// Get a random weighted item from a collection.
    /// </summary>
    private JToken? GetRandomWeightedItem(JToken items)
    {
        var itemList = items.Children().ToList();
        if (!itemList.Any()) return null;

        var totalWeight = itemList.Sum(item => GetIntProperty(item, "rarityWeight", 1));
        var randomValue = _random.Next(1, totalWeight + 1);

        int currentWeight = 0;
        foreach (var item in itemList)
        {
            currentWeight += GetIntProperty(item, "rarityWeight", 1);
            if (randomValue <= currentWeight)
            {
                return item;
            }
        }

        return itemList.First();
    }

    private static int GetIntProperty(JToken obj, string propertyName, int defaultValue)
    {
        try
        {
            var value = obj[propertyName];
            return value != null ? value.Value<int>() : defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    private static string? GetStringProperty(JToken obj, string propertyName)
    {
        try
        {
            var value = obj[propertyName];
            return value?.Value<string>();
        }
        catch
        {
            return null;
        }
    }
}
