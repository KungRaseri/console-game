using RealmEngine.Data.Services;
using RealmEngine.Shared.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;

namespace RealmEngine.Core.Generators.Modern;

/// <summary>
/// Generates organizations (guilds, factions, shops, businesses).
/// </summary>
public class OrganizationGenerator
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly Random _random;
    private readonly ILogger<OrganizationGenerator> _logger;

    /// <summary>
    /// Initializes a new instance of the OrganizationGenerator class.
    /// </summary>
    /// <param name="dataCache">The game data cache for accessing organization catalog files.</param>
    /// <param name="referenceResolver">The reference resolver for resolving JSON references.</param>
    /// <param name="logger">Logger for this generator.</param>
    public OrganizationGenerator(GameDataCache dataCache, ReferenceResolverService referenceResolver, ILogger<OrganizationGenerator> logger)
    {
        _dataCache = dataCache ?? throw new ArgumentNullException(nameof(dataCache));
        _referenceResolver = referenceResolver ?? throw new ArgumentNullException(nameof(referenceResolver));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = new Random();
    }

    /// <summary>
    /// Generates organizations from a specific type (guilds, factions, shops, businesses).
    /// </summary>
    /// <param name="organizationType">The organization type to generate.</param>
    /// <param name="count">The number of organizations to generate (default: 5).</param>
    /// <param name="hydrate">If true, populates resolved MemberObjects and InventoryObjects properties (default: true).</param>
    public async Task<List<Organization>> GenerateOrganizationsAsync(string organizationType, int count = 5, bool hydrate = true)
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
            var namesPath = catalogPath.Replace("catalog.json", "names.json");
            var namesFile = _dataCache.FileExists(namesPath) ? _dataCache.GetFile(namesPath) : null;

            for (int i = 0; i < count; i++)
            {
                var randomOrg = GetRandomWeightedItem(items);
                if (randomOrg != null)
                {
                    var organization = await ConvertToOrganizationAsync(randomOrg, organizationType, namesFile);
                    if (organization != null)
                    {
                        if (hydrate)
                        {
                            await HydrateOrganizationAsync(organization);
                        }
                        result.Add(organization);
                    }
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating organizations");
            return new List<Organization>();
        }
    }

    /// <summary>
    /// Generates a specific organization by name.
    /// </summary>
    /// <param name="organizationType">The organization type to search in.</param>
    /// <param name="organizationName">The name of the organization to generate.</param>
    /// <param name="hydrate">If true, populates resolved MemberObjects and InventoryObjects properties (default: true).</param>
    public async Task<Organization?> GenerateOrganizationByNameAsync(string organizationType, string organizationName, bool hydrate = true)
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

            var namesPath = catalogPath.Replace("catalog.json", "names.json");
            var namesFile = _dataCache.FileExists(namesPath) ? _dataCache.GetFile(namesPath) : null;
            var organization = await ConvertToOrganizationAsync(item, organizationType, namesFile);
            if (organization != null && hydrate)
            {
                await HydrateOrganizationAsync(organization);
            }
            return organization;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating organization by name");
            return null;
        }
    }

    /// <summary>
    /// Generates a shop with inventory.
    /// </summary>
    /// <param name="shopType">The shop type to generate.</param>
    /// <param name="inventorySize">The size of the shop's inventory (default: 20).</param>
    /// <param name="hydrate">If true, populates resolved MemberObjects and InventoryObjects properties (default: true).</param>
    public async Task<Organization> GenerateShopAsync(string shopType, int inventorySize = 20, bool hydrate = true)
    {
        var shop = await GenerateOrganizationByNameAsync("shops", shopType, hydrate);
        
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
            "armorer" => ["armor"],
            "general store" => ["consumables", "materials"],
            "alchemist" => ["consumables"],
            _ => ["weapons", "armor", "consumables"]
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
            _logger.LogError(ex, "Error converting organization");
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

    /// <summary>
    /// Hydrates an organization by resolving reference IDs to full objects.
    /// Populates MemberObjects and InventoryObjects properties.
    /// </summary>
    /// <param name="organization">The organization to hydrate.</param>
    private async Task HydrateOrganizationAsync(Organization organization)
    {
        // Resolve members
        if (organization.Members != null && organization.Members.Any())
        {
            var memberObjects = new List<NPC>();
            foreach (var refId in organization.Members)
            {
                try
                {
                    var npcJson = await _referenceResolver.ResolveToObjectAsync(refId);
                    if (npcJson != null)
                    {
                        var npc = npcJson.ToObject<NPC>();
                        if (npc != null)
                        {
                            memberObjects.Add(npc);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to resolve member NPC '{RefId}'", refId);
                }
            }
            organization.MemberObjects = memberObjects;
        }

        // Resolve inventory
        if (organization.Inventory != null && organization.Inventory.Any())
        {
            var inventoryObjects = new List<Item>();
            foreach (var refId in organization.Inventory)
            {
                try
                {
                    var itemJson = await _referenceResolver.ResolveToObjectAsync(refId);
                    if (itemJson != null)
                    {
                        var item = itemJson.ToObject<Item>();
                        if (item != null)
                        {
                            inventoryObjects.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to resolve inventory item '{RefId}'", refId);
                }
            }
            organization.InventoryObjects = inventoryObjects;
        }
    }
}
