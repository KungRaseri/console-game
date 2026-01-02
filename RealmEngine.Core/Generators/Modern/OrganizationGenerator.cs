using RealmEngine.Data.Services;
using RealmEngine.Shared.Models;
using Newtonsoft.Json.Linq;

namespace RealmEngine.Core.Generators.Modern;

/// <summary>
/// Generates organizations (guilds, factions, shops, businesses).
/// </summary>
public class OrganizationGenerator
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of the OrganizationGenerator class.
    /// </summary>
    /// <param name="dataCache">The game data cache for accessing organization catalog files.</param>
    /// <param name="referenceResolver">The reference resolver for resolving JSON references.</param>
    public OrganizationGenerator(GameDataCache dataCache, ReferenceResolverService referenceResolver)
    {
        _dataCache = dataCache ?? throw new ArgumentNullException(nameof(dataCache));
        _referenceResolver = referenceResolver ?? throw new ArgumentNullException(nameof(referenceResolver));
        _random = new Random();
    }

    /// <summary>
    /// Generates organizations from a specific type (guilds, factions, shops, businesses).
    /// </summary>
    public async Task<List<Organization>> GenerateOrganizationsAsync(string organizationType, int count = 5)
    {
        try
        {
            var catalogPath = $"organizations/{organizationType}/catalog.json";
            var catalogFile = _dataCache.GetFile(catalogPath);
            
            if (catalogFile?.JsonData == null)
            {
                return new List<Organization>();
            }

            var catalog = catalogFile.JsonData;
            var items = GetItemsFromCatalog(catalog);
            
            if (items == null || !items.Any())
            {
                return new List<Organization>();
            }

            var result = new List<Organization>();
            var namesFile = _dataCache.GetFile(catalogPath.Replace("catalog.json", "names.json"));

            for (int i = 0; i < count; i++)
            {
                var randomOrg = GetRandomWeightedItem(items);
                if (randomOrg != null)
                {
                    var organization = await ConvertToOrganizationAsync(randomOrg, organizationType, namesFile);
                    if (organization != null)
                    {
                        result.Add(organization);
                    }
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating organizations: {ex.Message}");
            return new List<Organization>();
        }
    }

    /// <summary>
    /// Generates a specific organization by name.
    /// </summary>
    public async Task<Organization?> GenerateOrganizationByNameAsync(string organizationType, string organizationName)
    {
        try
        {
            var catalogPath = $"organizations/{organizationType}/catalog.json";
            var catalogFile = _dataCache.GetFile(catalogPath);
            
            if (catalogFile?.JsonData == null)
            {
                return null;
            }

            var catalog = catalogFile.JsonData;
            var items = GetItemsFromCatalog(catalog);
            var item = items?.FirstOrDefault(i => i["name"]?.ToString() == organizationName);

            if (item == null)
            {
                return null;
            }

            var namesFile = _dataCache.GetFile(catalogPath.Replace("catalog.json", "names.json"));
            return await ConvertToOrganizationAsync(item, organizationType, namesFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating organization by name: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Generates a shop with inventory.
    /// </summary>
    public async Task<Organization> GenerateShopAsync(string shopType, int inventorySize = 20)
    {
        var shop = await GenerateOrganizationByNameAsync("shops", shopType);
        
        if (shop == null)
        {
            shop = new Organization
            {
                Id = $"shops:{shopType}",
                Name = $"Generic {shopType}",
                Description = $"A typical {shopType} shop.",
                Type = "shops"
            };
        }

        // Generate inventory for shop
        shop.Inventory = GenerateShopInventory(shopType, inventorySize);
        shop.Prices = GenerateShopPrices(shop.Inventory);

        return shop;
    }

    private List<string> GenerateShopInventory(string shopType, int size)
    {
        var inventory = new List<string>();
        
        // Determine appropriate item categories based on shop type
        var categories = shopType.ToLower() switch
        {
            "weaponsmith" => new[] { "weapons" },
            "armorer" => new[] { "armor" },
            "general store" => new[] { "consumables", "materials" },
            "alchemist" => new[] { "consumables" },
            _ => new[] { "weapons", "armor", "consumables" }
        };

        foreach (var category in categories)
        {
            var itemsPerCategory = size / categories.Length;
            for (int i = 0; i < itemsPerCategory; i++)
            {
                inventory.Add($"@items/{category}:*");
            }
        }

        return inventory;
    }

    private Dictionary<string, int> GenerateShopPrices(List<string> inventory)
    {
        var prices = new Dictionary<string, int>();
        
        foreach (var item in inventory)
        {
            var basePrice = _random.Next(10, 1000);
            prices[item] = basePrice;
        }

        return prices;
    }

    private async Task<Organization?> ConvertToOrganizationAsync(JToken orgData, string organizationType, CachedJsonFile? namesFile)
    {
        try
        {
            var name = namesFile != null 
                ? GenerateName(namesFile, orgData) 
                : orgData["name"]?.ToString() ?? "Unknown Organization";

            var organization = new Organization
            {
                Id = $"{organizationType}:{name.ToLower().Replace(" ", "-")}",
                Name = name,
                Description = orgData["description"]?.ToString() ?? "A mysterious organization.",
                Type = organizationType,
                Reputation = TryGetIntValue(orgData, "reputation", _random.Next(0, 100)),
                Wealth = TryGetIntValue(orgData, "wealth", _random.Next(1000, 100000))
            };

            // Parse leader
            organization.Leader = orgData["leader"]?.ToString();

            // Resolve member references
            if (orgData["members"] is JArray members)
            {
                organization.Members = await ResolveReferencesAsync(members);
            }

            // Resolve service references
            if (orgData["services"] is JArray services)
            {
                organization.Services = await ResolveReferencesAsync(services);
            }

            // Resolve inventory references (for shops)
            if (orgData["inventory"] is JArray inventory)
            {
                organization.Inventory = await ResolveReferencesAsync(inventory);
            }

            return organization;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting organization: {ex.Message}");
            return null;
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

    private static int TryGetIntValue(JToken obj, string propertyName, int defaultValue)
    {
        try
        {
            var value = obj[propertyName];
            if (value == null) return defaultValue;
            
            // Try parsing as int first
            if (value.Type == JTokenType.Integer)
            {
                return value.Value<int>();
            }
            
            // If it's a string, try parsing
            var stringValue = value.ToString();
            if (int.TryParse(stringValue, out var intValue))
            {
                return intValue;
            }
            
            // For string reputation values like "neutral", "lawful", return default
            return defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    private string GenerateName(CachedJsonFile namesFile, JToken orgData)
    {
        try
        {
            if (namesFile.JsonData == null) return orgData["name"]?.ToString() ?? "Unknown";

            var patterns = namesFile.JsonData["patterns"] as JArray;
            if (patterns == null || !patterns.Any()) return orgData["name"]?.ToString() ?? "Unknown";

            var pattern = GetRandomWeightedPattern(patterns);
            if (pattern == null) return orgData["name"]?.ToString() ?? "Unknown";

            var formatString = pattern["format"]?.ToString() ?? "{base}";
            var components = namesFile.JsonData["components"] as JObject;

            return ReplaceTokens(formatString, components);
        }
        catch
        {
            return orgData["name"]?.ToString() ?? "Unknown";
        }
    }

    private string ReplaceTokens(string format, JObject? components)
    {
        var result = format;
        
        if (components == null) return result;

        foreach (var component in components)
        {
            var token = $"{{{component.Key}}}";
            if (result.Contains(token) && component.Value is JArray array && array.Any())
            {
                var randomValue = array[_random.Next(array.Count)].ToString();
                result = result.Replace(token, randomValue);
            }
        }

        return result;
    }

    private IEnumerable<JToken> GetItemsFromCatalog(JToken catalog)
    {
        var allItems = new List<JToken>();
        
        // Handle hierarchical structure: guild_types -> warrior_guilds/mage_guilds -> items
        foreach (var property in catalog.Children<JProperty>())
        {
            if (property.Name == "metadata") continue;
            
            // This is a type category (guild_types, faction_types, etc)
            var typeCategory = property.Value;
            if (typeCategory is JObject typeCategoryObj)
            {
                foreach (var subType in typeCategoryObj.Children<JProperty>())
                {
                    if (subType.Name == "metadata") continue;
                    
                    // This is a specific type (warrior_guilds, mage_guilds, etc)
                    var items = subType.Value["items"];
                    if (items != null && items.HasValues)
                    {
                        allItems.AddRange(items.Children());
                    }
                }
            }
        }
        
        // Fallback: Try direct items array
        if (!allItems.Any() && catalog["items"] is JArray itemsArray)
        {
            return itemsArray;
        }

        // Fallback: Try root level if not wrapped
        if (!allItems.Any() && catalog is JArray rootArray)
        {
            return rootArray;
        }

        return allItems;
    }

    private JToken? GetRandomWeightedItem(IEnumerable<JToken> items)
    {
        var itemsList = items.ToList();
        if (!itemsList.Any()) return null;

        var totalWeight = itemsList.Sum(i => i["rarityWeight"]?.Value<int>() ?? 1);
        var randomValue = _random.Next(totalWeight);
        var currentWeight = 0;

        foreach (var item in itemsList)
        {
            currentWeight += item["rarityWeight"]?.Value<int>() ?? 1;
            if (randomValue < currentWeight)
            {
                return item;
            }
        }

        return itemsList.Last();
    }

    private JToken? GetRandomWeightedPattern(JArray patterns)
    {
        if (!patterns.Any()) return null;

        var totalWeight = patterns.Sum(p => p["rarityWeight"]?.Value<int>() ?? 1);
        var randomValue = _random.Next(totalWeight);
        var currentWeight = 0;

        foreach (var pattern in patterns)
        {
            currentWeight += pattern["rarityWeight"]?.Value<int>() ?? 1;
            if (randomValue < currentWeight)
            {
                return pattern;
            }
        }

        return patterns.Last();
    }
}
