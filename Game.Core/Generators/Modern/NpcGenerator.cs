using Game.Data.Services;
using Game.Shared.Models;
using Newtonsoft.Json.Linq;

namespace Game.Core.Generators.Modern;

/// <summary>
/// Generates NPC (Non-Player Character) instances from npcs catalog JSON files.
/// Supports different NPC roles (merchants, quest givers, companions) and procedural name generation.
/// </summary>
public class NpcGenerator
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of the NpcGenerator class.
    /// </summary>
    /// <param name="dataCache">The game data cache for accessing NPC catalog files.</param>
    /// <param name="referenceResolver">The reference resolver for resolving JSON references.</param>
    public NpcGenerator(GameDataCache dataCache, ReferenceResolverService referenceResolver)
    {
        _dataCache = dataCache ?? throw new ArgumentNullException(nameof(dataCache));
        _referenceResolver = referenceResolver ?? throw new ArgumentNullException(nameof(referenceResolver));
        _random = new Random();
    }

    public async Task<List<NPC>> GenerateNpcsAsync(string category, int count = 5)
    {
        try
        {
            var catalogFile = _dataCache.GetFile($"npcs/{category}/catalog.json");
            if (catalogFile?.JsonData == null)
            {
                return new List<NPC>();
            }

            var catalog = catalogFile.JsonData;
            var items = GetItemsFromCatalog(catalog);
            
            if (items == null || !items.Any())
            {
                return new List<NPC>();
            }

            var result = new List<NPC>();

            for (int i = 0; i < count; i++)
            {
                var randomNpc = GetRandomWeightedItem(items);
                if (randomNpc != null)
                {
                    var npc = await ConvertToNpcAsync(randomNpc, category);
                    if (npc != null)
                    {
                        result.Add(npc);
                    }
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating NPCs for category {category}: {ex.Message}");
            return new List<NPC>();
        }
    }

    public async Task<NPC?> GenerateNpcByNameAsync(string category, string npcName)
    {
        try
        {
            var catalogFile = _dataCache.GetFile($"npcs/{category}/catalog.json");
            if (catalogFile?.JsonData == null)
            {
                return null;
            }

            var catalog = catalogFile.JsonData;
            var items = GetItemsFromCatalog(catalog);
            
            var catalogNpc = items?.FirstOrDefault(n => 
                string.Equals(GetStringProperty(n, "name"), npcName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(GetStringProperty(n, "displayName"), npcName, StringComparison.OrdinalIgnoreCase));

            if (catalogNpc != null)
            {
                return await ConvertToNpcAsync(catalogNpc, category);
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating NPC {npcName} from category {category}: {ex.Message}");
            return null;
        }
    }

    private static IEnumerable<JToken>? GetItemsFromCatalog(JToken catalog)
    {
        var allItems = new List<JToken>();
        
        // Handle hierarchical structure: occupation_types -> general_traders/etc -> items
        foreach (var property in catalog.Children<JProperty>())
        {
            if (property.Name == "metadata" || property.Name == "background_types") continue;
            
            // This is a type category (occupation_types, etc)
            var typeCategory = property.Value;
            if (typeCategory is JObject typeCategoryObj)
            {
                foreach (var subType in typeCategoryObj.Children<JProperty>())
                {
                    if (subType.Name == "metadata") continue;
                    
                    // This is a specific type (general_traders, etc)
                    var items = subType.Value["items"];
                    if (items != null && items.HasValues)
                    {
                        allItems.AddRange(items.Children());
                    }
                }
            }
        }
        
        return allItems.Any() ? allItems : null;
    }

    private Task<NPC?> ConvertToNpcAsync(JToken catalogNpc, string category)
    {
        try
        {
            var npc = new NPC
            {
                Id = $"{category}:{GetStringProperty(catalogNpc, "name") ?? GetStringProperty(catalogNpc, "displayName")}",
                Name = GetStringProperty(catalogNpc, "displayName") ?? GetStringProperty(catalogNpc, "name") ?? "Unknown NPC",
                Age = GetIntProperty(catalogNpc, "age", _random.Next(20, 70)),
                Occupation = GetStringProperty(catalogNpc, "occupation") ?? GetStringProperty(catalogNpc, "socialClass") ?? "Citizen",
                Gold = GetIntProperty(catalogNpc, "baseGold", _random.Next(10, 100)),
                Dialogue = GetStringProperty(catalogNpc, "greeting") ?? GetStringProperty(catalogNpc, "description") ?? "Hello there!",
                IsFriendly = GetBoolProperty(catalogNpc, "isFriendly", !GetBoolProperty(catalogNpc, "isHostile", false))
            };

            return Task.FromResult<NPC?>(npc);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting catalog NPC to NPC: {ex.Message}");
            return Task.FromResult<NPC?>(null);
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
            if (value != null)
            {
                // Handle dice notation like "5d10"
                var stringValue = value.Value<string>();
                if (stringValue?.Contains('d') == true)
                {
                    var parts = stringValue.Split('d');
                    if (parts.Length == 2 && int.TryParse(parts[0], out var rolls) && int.TryParse(parts[1], out var sides))
                    {
                        var random = new Random();
                        var total = 0;
                        for (int i = 0; i < rolls; i++)
                        {
                            total += random.Next(1, sides + 1);
                        }
                        return total;
                    }
                }
                return value.Value<int>();
            }
            return defaultValue;
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
}
