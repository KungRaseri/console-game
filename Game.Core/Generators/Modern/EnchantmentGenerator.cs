using Game.Data.Services;
using Game.Shared.Models;
using Newtonsoft.Json.Linq;

namespace Game.Core.Generators.Modern;

/// <summary>
/// Generates enchantments using the v4.2 prefix/suffix system.
/// Part of the Hybrid Enhancement System v1.0.
/// </summary>
public class EnchantmentGenerator
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly Random _random;

    public EnchantmentGenerator(GameDataCache dataCache, ReferenceResolverService referenceResolver)
    {
        _dataCache = dataCache ?? throw new ArgumentNullException(nameof(dataCache));
        _referenceResolver = referenceResolver ?? throw new ArgumentNullException(nameof(referenceResolver));
        _random = new Random();
    }

    /// <summary>
    /// Generate a single enchantment from a reference.
    /// </summary>
    public Task<Enchantment?> GenerateEnchantmentAsync(string reference)
    {
        try
        {
            var namesFile = _dataCache.GetFile("items/enchantments/names.json");
            if (namesFile?.JsonData == null)
            {
                return Task.FromResult<Enchantment?>(null);
            }

            // Get patterns that match the reference filter
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

            // Get position from pattern
            var position = GetStringProperty(pattern, "position");
            var format = GetStringProperty(pattern, "format");
            
            if (string.IsNullOrEmpty(format))
            {
                return Task.FromResult<Enchantment?>(null);
            }

            // Resolve components from format (e.g., "{element_prefix}" -> element_prefix component)
            var componentName = format.Trim('{', '}');
            var components = namesFile.JsonData["components"]?[componentName];
            
            if (components == null || !components.Any())
            {
                return Task.FromResult<Enchantment?>(null);
            }

            // Select random component by weight
            var component = GetRandomWeightedItem(components);
            if (component == null)
            {
                return Task.FromResult<Enchantment?>(null);
            }

            // Build enchantment from component
            return Task.FromResult<Enchantment?>(BuildEnchantment(component, position));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating enchantment: {ex.Message}");
            return Task.FromResult<Enchantment?>(null);
        }
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
    private Enchantment BuildEnchantment(JToken component, string? position)
    {
        var enchantment = new Enchantment
        {
            Name = GetStringProperty(component, "value") ?? "Unknown Enchantment",
            RarityWeight = GetIntProperty(component, "rarityWeight", 50),
            Position = position?.ToLower() == "prefix" 
                ? EnchantmentPosition.Prefix 
                : EnchantmentPosition.Suffix,
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
