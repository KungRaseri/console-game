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

    public OrganizationGenerator(GameDataCache dataCache, ReferenceResolverService referenceResolver)
    {
        _dataCache = dataCache ?? throw new ArgumentNullException(nameof(dataCache));
        _referenceResolver = referenceResolver ?? throw new ArgumentNullException(nameof(referenceResolver));
        _random = new Random();
    }

    /// <summary>
    /// Generates organizations from a specific type (guilds, factions, shops, businesses).
    /// </summary>
    public Task<List<Organization>> GenerateOrganizationsAsync(string organizationType, int count = 5)
    {
        try
        {
            var catalogPath = $"organizations/{organizationType}/catalog.json";
            var catalogFile = _dataCache.GetFile(catalogPath);
            
            if (catalogFile?.JsonData == null)
            {
                return Task.FromResult(new List<Organization>());
            }

            var catalog = catalogFile.JsonData;
            var items = GetItemsFromCatalog(catalog);
            
            if (items == null || !items.Any())
            {
                return Task.FromResult(new List<Organization>());
            }

            var result = new List<Organization>();
            var namesFile = _dataCache.GetFile(catalogPath.Replace("catalog.json", "names.json"));

            for (int i = 0; i < count; i++)
            {
                var randomOrg = GetRandomWeightedItem(items);
                if (randomOrg != null)
                {
                    var organization = ConvertToOrganization(randomOrg, organizationType, namesFile);
                    if (organization != null)
                    {
                        result.Add(organization);
                    }
                }
            }

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating organizations: {ex.Message}");
            return Task.FromResult(new List<Organization>());
        }
    }

    /// <summary>
    /// Generates a specific organization by name.
    /// </summary>
    public Task<Organization?> GenerateOrganizationByNameAsync(string organizationType, string organizationName)
    {
        try
        {
            var catalogPath = $"organizations/{organizationType}/catalog.json";
            var catalogFile = _dataCache.GetFile(catalogPath);
            
            if (catalogFile?.JsonData == null)
            {
                return Task.FromResult<Organization?>(null);
            }

            var catalog = catalogFile.JsonData;
            var items = GetItemsFromCatalog(catalog);
            var item = items?.FirstOrDefault(i => i["name"]?.ToString() == organizationName);

            if (item == null)
            {
                return Task.FromResult<Organization?>(null);
            }

            var namesFile = _dataCache.GetFile(catalogPath.Replace("catalog.json", "names.json"));
            return Task.FromResult(ConvertToOrganization(item, organizationType, namesFile));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating organization by name: {ex.Message}");
            return Task.FromResult<Organization?>(null);
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

    private Organization? ConvertToOrganization(JToken orgData, string organizationType, CachedJsonFile? namesFile)
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
                Reputation = orgData["reputation"]?.Value<int>() ?? _random.Next(0, 100),
                Wealth = orgData["wealth"]?.Value<int>() ?? _random.Next(1000, 100000)
            };

            // Parse leader
            organization.Leader = orgData["leader"]?.ToString();

            // Parse members
            if (orgData["members"] is JArray members)
            {
                organization.Members = members.Select(m => m.ToString()).ToList();
            }

            // Parse services
            if (orgData["services"] is JArray services)
            {
                organization.Services = services.Select(s => s.ToString()).ToList();
            }

            // Parse inventory (for shops)
            if (orgData["inventory"] is JArray inventory)
            {
                organization.Inventory = inventory.Select(i => i.ToString()).ToList();
            }

            return organization;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting organization: {ex.Message}");
            return null;
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
        if (catalog["items"] is JArray itemsArray)
        {
            return itemsArray;
        }

        if (catalog is JArray rootArray)
        {
            return rootArray;
        }

        return Enumerable.Empty<JToken>();
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
