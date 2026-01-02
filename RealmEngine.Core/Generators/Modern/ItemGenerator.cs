using RealmEngine.Data.Services;
using RealmEngine.Shared.Models;
using Newtonsoft.Json.Linq;

namespace RealmEngine.Core.Generators.Modern;

/// <summary>
/// Generates items using the Hybrid Enhancement System v1.0.
/// Supports materials (baked), enchantments (baked), and gem sockets (player customizable).
/// </summary>
public class ItemGenerator
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly Random _random;
    private EnchantmentGenerator? _enchantmentGenerator;

    public ItemGenerator(GameDataCache dataCache, ReferenceResolverService referenceResolver)
    {
        _dataCache = dataCache ?? throw new ArgumentNullException(nameof(dataCache));
        _referenceResolver = referenceResolver ?? throw new ArgumentNullException(nameof(referenceResolver));
        _random = new Random();
    }

    /// <summary>
    /// Gets or creates the enchantment generator (lazy initialization).
    /// </summary>
    private EnchantmentGenerator EnchantmentGenerator
    {
        get
        {
            _enchantmentGenerator ??= new EnchantmentGenerator(_dataCache, _referenceResolver);
            return _enchantmentGenerator;
        }
    }

    public async Task<List<Item>> GenerateItemsAsync(string category, int count = 10)
    {
        try
        {
            var catalogFile = _dataCache.GetFile($"items/{category}/catalog.json");
            if (catalogFile?.JsonData == null)
            {
                return new List<Item>();
            }

            var catalog = catalogFile.JsonData;
            var items = GetItemsFromCatalog(catalog);
            
            if (items == null || !items.Any())
            {
                return new List<Item>();
            }

            var result = new List<Item>();

            for (int i = 0; i < count; i++)
            {
                var randomItem = GetRandomWeightedItem(items);
                if (randomItem != null)
                {
                    var item = await ConvertToItemAsync(randomItem, category);
                    if (item != null)
                    {
                        result.Add(item);
                    }
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating items for category {category}: {ex.Message}");
            return new List<Item>();
        }
    }

    public async Task<Item?> GenerateItemByNameAsync(string category, string itemName)
    {
        try
        {
            var catalogFile = _dataCache.GetFile($"items/{category}/catalog.json");
            if (catalogFile?.JsonData == null)
            {
                return null;
            }

            var catalog = catalogFile.JsonData;
            var items = GetItemsFromCatalog(catalog);
            
            var catalogItem = items?.FirstOrDefault(i => 
                string.Equals(GetStringProperty(i, "name"), itemName, StringComparison.OrdinalIgnoreCase));

            if (catalogItem != null)
            {
                return await ConvertToItemAsync(catalogItem, category);
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating item {itemName} from category {category}: {ex.Message}");
            return null;
        }
    }

    private static IEnumerable<JToken>? GetItemsFromCatalog(JToken catalog)
    {
        var allItems = new List<JToken>();
        
        // Try different possible structures
        if (catalog["items"] != null)
        {
            return catalog["items"]?.Children();
        }
        
        // For hierarchical catalogs like weapon_types, armor_types, etc.
        // Look for type containers (weapon_types, armor_types, etc.)
        foreach (var property in catalog.Children<JProperty>())
        {
            // Skip metadata
            if (property.Name == "metadata") continue;
            
            // Check if this property has nested items arrays
            var typeContainer = property.Value;
            if (typeContainer is JObject typeObj)
            {
                // Look through each weapon/armor type (swords, axes, etc.)
                foreach (var typeProperty in typeObj.Children<JProperty>())
                {
                    var items = typeProperty.Value["items"];
                    if (items != null)
                    {
                        foreach (var item in items.Children())
                        {
                            allItems.Add(item);
                        }
                    }
                }
            }
        }

        return allItems.Any() ? allItems : null;
    }

    private async Task<Item?> ConvertToItemAsync(JToken catalogItem, string category)
    {
        try
        {
            var baseName = GetStringProperty(catalogItem, "name") ?? "Unknown Item";
            
            var item = new Item
            {
                Id = $"{category}:{baseName}",
                BaseName = baseName,
                Name = baseName, // Will be updated after enhancements
                Description = GetStringProperty(catalogItem, "description") ?? "No description available",
                Price = GetIntProperty(catalogItem, "value", 1)
            };

            // Resolve item type from category
            item.Type = category switch
            {
                "weapons" => ItemType.Weapon,
                "armor" => ItemType.Chest,
                "consumables" => ItemType.Consumable,
                "shields" => ItemType.Shield,
                _ => ItemType.Consumable
            };

            // Base rarity weight from catalog
            var baseRarityWeight = GetIntProperty(catalogItem, "rarityWeight", 50);
            item.TotalRarityWeight = baseRarityWeight;

            // Resolve enchantment references
            if (catalogItem["enchantments"] is JArray enchantments)
            {
                item.EnchantmentIds = await ResolveReferencesAsync(enchantments);
            }

            // Resolve material references
            if (catalogItem["materials"] is JArray materials)
            {
                item.MaterialIds = await ResolveReferencesAsync(materials);
            }

            // Resolve required item references
            if (catalogItem["requiredItems"] is JArray requiredItems)
            {
                item.RequiredItemIds = await ResolveReferencesAsync(requiredItems);
            }

            // Apply traits from catalog
            await ApplyTraitsFromCatalogAsync(item, catalogItem);

            // Apply enhancements from names.json pattern (v4.2)
            await ApplyEnhancementsFromPatternAsync(item, category);

            // Calculate final rarity from total weight
            item.Rarity = ConvertWeightToRarity(item.TotalRarityWeight);

            // Update item name with enhancements
            item.Name = BuildEnhancedName(item);

            return item;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting catalog item to Item: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Apply enhancements (materials, enchantments, gem sockets) from names.json pattern.
    /// </summary>
    private async Task ApplyEnhancementsFromPatternAsync(Item item, string category)
    {
        try
        {
            var namesFile = _dataCache.GetFile($"items/{category}/names.json");
            if (namesFile?.JsonData == null) return;

            var patterns = namesFile.JsonData["patterns"];
            if (patterns == null) return;

            // Select a random pattern
            var pattern = GetRandomWeightedPattern(patterns);
            if (pattern == null) return;

            // Apply material if pattern has materialRef
            var materialRef = GetStringProperty(pattern, "materialRef");
            if (!string.IsNullOrEmpty(materialRef))
            {
                await SelectMaterialAsync(item, materialRef);
            }

            // Apply enchantments from enchantmentSlots
            var enchantmentSlots = pattern["enchantmentSlots"];
            if (enchantmentSlots != null && enchantmentSlots.Any())
            {
                await GenerateEnchantmentsAsync(item, enchantmentSlots);
            }

            // Generate gem sockets from gemSocketCount
            var gemSocketCount = pattern["gemSocketCount"];
            if (gemSocketCount != null)
            {
                GenerateGemSockets(item, gemSocketCount);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying enhancements: {ex.Message}");
        }
    }

    private async Task<List<string>> ResolveReferencesAsync(JArray? referenceArray)
    {
        var resolvedIds = new List<string>();
        if (referenceArray == null) return resolvedIds;

        foreach (var item in referenceArray)
        {
            var reference = item.ToString();
            
            if (reference.StartsWith("@"))
            {
                var resolvedId = await _referenceResolver.ResolveAsync(reference);
                if (resolvedId != null)
                {
                    resolvedIds.Add(resolvedId.ToString() ?? string.Empty);
                }
            }
        }

        return resolvedIds;
    }

    /// <summary>
    /// Select and apply a material to the item.
    /// </summary>
    private async Task SelectMaterialAsync(Item item, string materialReference)
    {
        try
        {
            // Use reference resolver to get materials
            var materialsObj = await _referenceResolver.ResolveAsync(materialReference);
            if (materialsObj == null) return;

            // Cast to JArray
            var materials = materialsObj as JArray;
            if (materials == null || !materials.Any()) return;

            // Select random material by weight
            var material = GetRandomWeightedItem(materials);
            if (material == null) return;

            item.Material = GetStringProperty(material, "name");
            
            // Add material rarity weight to total
            var materialWeight = GetIntProperty(material, "rarityWeight", 0);
            item.TotalRarityWeight += materialWeight;

            // Apply material traits
            var traits = material["traits"];
            if (traits != null && traits.Type == JTokenType.Object)
            {
                foreach (var traitProp in traits.Children<JProperty>())
                {
                    var traitName = traitProp.Name;
                    var traitValue = traitProp.Value;
                    
                    // Handle both simple values and structured trait objects
                    object? value;
                    TraitType type;
                    
                    if (traitValue.Type == JTokenType.Object && traitValue["value"] != null)
                    {
                        // Structured trait: { "value": 100, "type": "number" }
                        value = traitValue["value"]?.ToObject<object>();
                        type = ParseTraitType(traitValue["type"]?.ToString() ?? "number");
                    }
                    else
                    {
                        // Simple trait: just a value (100, 1.5, "metal", etc.)
                        value = traitValue.ToObject<object>();
                        type = ParseTraitType(traitValue.Type.ToString());
                    }
                    
                    item.MaterialTraits[traitName] = new TraitValue
                    {
                        Value = value,
                        Type = type
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error selecting material: {ex.Message}");
        }
    }

    /// <summary>
    /// Generate enchantments for the item based on enchantmentSlots.
    /// </summary>
    private async Task GenerateEnchantmentsAsync(Item item, JToken enchantmentSlots)
    {
        try
        {
            foreach (var slot in enchantmentSlots.Children())
            {
                var position = GetStringProperty(slot, "position");
                var reference = GetStringProperty(slot, "reference");
                
                if (string.IsNullOrEmpty(reference)) continue;

                var enchantment = await EnchantmentGenerator.GenerateEnchantmentAsync(reference);
                if (enchantment != null)
                {
                    // Set position from slot
                    enchantment.Position = position?.ToLower() == "prefix" 
                        ? EnchantmentPosition.Prefix 
                        : EnchantmentPosition.Suffix;
                    
                    item.Enchantments.Add(enchantment);
                    
                    // Add enchantment weight to total
                    item.TotalRarityWeight += enchantment.RarityWeight;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating enchantments: {ex.Message}");
        }
    }

    /// <summary>
    /// Generate gem sockets for the item.
    /// </summary>
    private void GenerateGemSockets(Item item, JToken gemSocketCount)
    {
        try
        {
            var min = GetIntProperty(gemSocketCount, "min", 0);
            var max = GetIntProperty(gemSocketCount, "max", 0);
            
            if (max == 0) return;

            var socketCount = _random.Next(min, max + 1);
            
            for (int i = 0; i < socketCount; i++)
            {
                var socket = new GemSocket
                {
                    Color = GetRandomGemColor(),
                    Gem = null, // Empty socket
                    IsLocked = false
                };
                
                item.GemSockets.Add(socket);
                
                // Each socket adds 10 to rarity weight
                item.TotalRarityWeight += 10;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating gem sockets: {ex.Message}");
        }
    }

    /// <summary>
    /// Build the enhanced item name from all components.
    /// </summary>
    private string BuildEnhancedName(Item item)
    {
        var nameParts = new List<string>();

        // Prefix enchantments
        var prefixEnchantments = item.Enchantments
            .Where(e => e.Position == EnchantmentPosition.Prefix)
            .ToList();
        
        foreach (var enchantment in prefixEnchantments)
        {
            nameParts.Add(enchantment.Name);
        }

        // Material
        if (!string.IsNullOrEmpty(item.Material))
        {
            nameParts.Add(item.Material);
        }

        // Base name
        nameParts.Add(item.BaseName);

        // Suffix enchantments
        var suffixEnchantments = item.Enchantments
            .Where(e => e.Position == EnchantmentPosition.Suffix)
            .ToList();
        
        foreach (var enchantment in suffixEnchantments)
        {
            nameParts.Add(enchantment.Name);
        }

        // Gem socket indicator
        if (item.GemSockets.Any())
        {
            var filledSockets = item.GemSockets.Count(s => s.Gem != null);
            var totalSockets = item.GemSockets.Count;
            if (totalSockets > 0)
            {
                nameParts.Add($"[{filledSockets}/{totalSockets} Sockets]");
            }
        }

        return string.Join(" ", nameParts);
    }

    /// <summary>
    /// Apply traits from catalog item to the item model.
    /// </summary>
    private Task ApplyTraitsFromCatalogAsync(Item item, JToken catalogItem)
    {
        try
        {
            var traits = catalogItem["traits"];
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
                    
                    item.Traits[traitName] = traitValue;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying traits: {ex.Message}");
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Get a random gem color for socket generation.
    /// </summary>
    private GemColor GetRandomGemColor()
    {
        var colors = Enum.GetValues<GemColor>();
        return colors[_random.Next(colors.Length)];
    }

    /// <summary>
    /// Parse trait type string to TraitType enum.
    /// </summary>
    private static TraitType ParseTraitType(string typeString)
    {
        return typeString?.ToLower() switch
        {
            "number" or "integer" or "float" => TraitType.Number,
            "string" => TraitType.String,
            "boolean" => TraitType.Boolean,
            "stringarray" or "array" => TraitType.StringArray,
            "numberarray" => TraitType.NumberArray,
            _ => TraitType.Number
        };
    }

    /// <summary>
    /// Get a random weighted pattern from the patterns array.
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

    private JToken? GetRandomWeightedItem(IEnumerable<JToken> items)
    {
        var itemList = items.ToList();
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

    private static double GetDoubleProperty(JToken obj, string propertyName, double defaultValue)
    {
        try
        {
            var value = obj[propertyName];
            return value != null ? value.Value<double>() : defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    private static bool GetBoolProperty(JToken obj, string propertyName, bool defaultValue)
    {
        try
        {
            var value = obj[propertyName];
            return value != null ? value.Value<bool>() : defaultValue;
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

    private static ItemRarity ConvertWeightToRarity(int weight)
    {
        // v4.2 Rarity Weight System (from ITEM_ENHANCEMENT_SYSTEM.md)
        // Common: < 50
        // Uncommon: 50-99
        // Rare: 100-199
        // Epic: 200-349
        // Legendary: 350+
        return weight switch
        {
            < 50 => ItemRarity.Common,
            < 100 => ItemRarity.Uncommon,
            < 200 => ItemRarity.Rare,
            < 350 => ItemRarity.Epic,
            _ => ItemRarity.Legendary
        };
    }
}