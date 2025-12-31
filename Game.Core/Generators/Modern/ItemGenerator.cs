using Game.Data.Services;
using Game.Shared.Models;
using Newtonsoft.Json.Linq;

namespace Game.Core.Generators.Modern;

public class ItemGenerator
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly Random _random;

    public ItemGenerator(GameDataCache dataCache, ReferenceResolverService referenceResolver)
    {
        _dataCache = dataCache ?? throw new ArgumentNullException(nameof(dataCache));
        _referenceResolver = referenceResolver ?? throw new ArgumentNullException(nameof(referenceResolver));
        _random = new Random();
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

    private Task<Item?> ConvertToItemAsync(JToken catalogItem, string category)
    {
        try
        {
            var item = new Item
            {
                Id = $"{category}:{GetStringProperty(catalogItem, "name")}",
                Name = GetStringProperty(catalogItem, "name") ?? "Unknown Item",
                Description = GetStringProperty(catalogItem, "description") ?? "No description available",
                Price = GetIntProperty(catalogItem, "value", 1) // JSON uses "value", model uses "Price"
            };

            // Resolve item type from category
            item.Type = category switch
            {
                "weapons" => ItemType.Weapon,
                "armor" => ItemType.Chest, // Default armor type
                "consumables" => ItemType.Consumable,
                "shields" => ItemType.Shield,
                _ => ItemType.Consumable // Default to Consumable
            };

            // Set rarity from rarityWeight or rarity field
            var rarityWeight = GetIntProperty(catalogItem, "rarityWeight", 0);
            if (rarityWeight > 0)
            {
                item.Rarity = ConvertWeightToRarity(rarityWeight);
            }
            else
            {
                // Try to get rarity string directly
                var rarityString = GetStringProperty(catalogItem, "rarity");
                if (!string.IsNullOrEmpty(rarityString))
                {
                    item.Rarity = Enum.TryParse<ItemRarity>(rarityString, true, out var rarity) 
                        ? rarity 
                        : ItemRarity.Common;
                }
            }

            return Task.FromResult<Item?>(item);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting catalog item to Item: {ex.Message}");
            return Task.FromResult<Item?>(null);
        }
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
        return weight switch
        {
            >= 100 => ItemRarity.Common,
            >= 50 => ItemRarity.Uncommon,
            >= 25 => ItemRarity.Rare,
            >= 10 => ItemRarity.Epic,
            _ => ItemRarity.Legendary
        };
    }
}