using RealmEngine.Data.Services;
using RealmEngine.Shared.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;

namespace RealmEngine.Core.Generators.Modern;

/// <summary>
/// Generates world locations (towns, dungeons, wilderness, environments, regions).
/// </summary>
public class LocationGenerator
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly Random _random;
    private readonly ILogger<LocationGenerator> _logger;

    /// <summary>
    /// Initializes a new instance of the LocationGenerator class.
    /// </summary>
    /// <param name="dataCache">The game data cache for accessing location catalog files.</param>
    /// <param name="referenceResolver">The reference resolver for resolving JSON references.</param>
    /// <param name="logger">Logger for this generator.</param>
    public LocationGenerator(GameDataCache dataCache, ReferenceResolverService referenceResolver, ILogger<LocationGenerator> logger)
    {
        _dataCache = dataCache ?? throw new ArgumentNullException(nameof(dataCache));
        _referenceResolver = referenceResolver ?? throw new ArgumentNullException(nameof(referenceResolver));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = new Random();
    }

    /// <summary>
    /// Generates locations from a specific type (towns, dungeons, wilderness, environments, regions).
    /// </summary>
    /// <param name="locationType">The location type to generate.</param>
    /// <param name="count">The number of locations to generate (default: 5).</param>
    /// <param name="hydrate">If true, populates resolved NpcObjects, EnemyObjects, and LootObjects properties (default: true).</param>
    public async Task<List<Location>> GenerateLocationsAsync(string locationType, int count = 5, bool hydrate = true)
    {
        try
        {
            var catalogPath = locationType switch
            {
                "towns" => "world/locations/towns/catalog.json",
                "dungeons" => "world/locations/dungeons/catalog.json",
                "wilderness" => "world/locations/wilderness/catalog.json",
                "environments" => "world/environments/catalog.json",
                "regions" => "world/regions/catalog.json",
                _ => $"world/{locationType}/catalog.json"
            };

            var catalogFile = _dataCache.GetFile(catalogPath);
            if (catalogFile?.JsonData == null)
            {
                return new List<Location>();
            }

            var catalog = catalogFile.JsonData;
            var items = GetItemsFromCatalog(catalog);
            
            if (items == null || !items.Any())
            {
                return new List<Location>();
            }

            var result = new List<Location>();
            var namesPath = catalogPath.Replace("catalog.json", "names.json");
            var namesFile = _dataCache.FileExists(namesPath) ? _dataCache.GetFile(namesPath) : null;

            for (int i = 0; i < count; i++)
            {
                var randomLocation = GetRandomWeightedItem(items);
                if (randomLocation != null)
                {
                    var location = await ConvertToLocationAsync(randomLocation, locationType, namesFile);
                    if (location != null)
                    {
                        if (hydrate)
                        {
                            await HydrateLocationAsync(location);
                        }
                        result.Add(location);
                    }
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating locations");
            return new List<Location>();
        }
    }

    /// <summary>
    /// Generates a specific location by name.
    /// </summary>
    /// <param name="locationType">The location type to search in.</param>
    /// <param name="locationName">The name of the location to generate.</param>
    /// <param name="hydrate">If true, populates resolved NpcObjects, EnemyObjects, and LootObjects properties (default: true).</param>
    public async Task<Location?> GenerateLocationByNameAsync(string locationType, string locationName, bool hydrate = true)
    {
        try
        {
            var catalogPath = locationType switch
            {
                "towns" => "world/locations/towns/catalog.json",
                "dungeons" => "world/locations/dungeons/catalog.json",
                "wilderness" => "world/locations/wilderness/catalog.json",
                "environments" => "world/environments/catalog.json",
                "regions" => "world/regions/catalog.json",
                _ => $"world/{locationType}/catalog.json"
            };

            var catalogFile = _dataCache.GetFile(catalogPath);
            if (catalogFile?.JsonData == null)
            {
                return null;
            }

            var catalog = catalogFile.JsonData;
            var items = GetItemsFromCatalog(catalog);
            var item = items?.FirstOrDefault(i => i["name"]?.ToString() == locationName);

            if (item == null)
            {
                return null;
            }

            var namesPath = catalogPath.Replace("catalog.json", "names.json");
            var namesFile = _dataCache.FileExists(namesPath) ? _dataCache.GetFile(namesPath) : null;
            var location = await ConvertToLocationAsync(item, locationType, namesFile);
            if (location != null && hydrate)
            {
                await HydrateLocationAsync(location);
            }
            return location;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating location by name");
            return null;
        }
    }

    private async Task<Location?> ConvertToLocationAsync(JToken locationData, string locationType, CachedJsonFile? namesFile)
    {
        try
        {
            var name = namesFile != null 
                ? GenerateName(namesFile, locationData) 
                : locationData["name"]?.ToString() ?? "Unknown Location";

            var location = new Location
            {
                Id = $"{locationType}:{name.ToLower().Replace(" ", "-")}",
                Name = name,
                Description = locationData["description"]?.ToString() ?? "A mysterious location.",
                Type = locationType,
                Level = locationData["level"]?.Value<int>() ?? _random.Next(1, 20),
                DangerRating = locationData["dangerRating"]?.Value<int>() ?? _random.Next(1, 10)
            };

            // Parse features
            if (locationData["features"] is JArray features)
            {
                location.Features = features.Select(f => f.ToString()).ToList();
            }

            // Parse and resolve NPC references
            if (locationData["npcs"] is JArray npcs)
            {
                location.Npcs = await ResolveReferencesAsync(npcs);
            }

            // Parse and resolve enemy references
            if (locationData["enemies"] is JArray enemies)
            {
                location.Enemies = await ResolveReferencesAsync(enemies);
            }

            // Parse and resolve loot/resource references
            if (locationData["loot"] is JArray loot)
            {
                location.Loot = await ResolveReferencesAsync(loot);
            }
            else if (locationData["resources"] is JArray resources)
            {
                location.Loot = await ResolveReferencesAsync(resources);
            }

            // Parse parent region
            location.ParentRegion = locationData["region"]?.ToString();

            return location;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting location");
            return null;
        }
    }

    /// <summary>
    /// Resolves an array of references (handles both direct strings and @references)
    /// </summary>
    private async Task<List<string>> ResolveReferencesAsync(JArray references)
    {
        var resolved = new List<string>();
        foreach (var reference in references)
        {
            var refString = reference.ToString();
            if (refString.StartsWith("@"))
            {
                var result = await _referenceResolver.ResolveAsync(refString);
                if (result is string resolvedId)
                {
                    resolved.Add(resolvedId);
                }
            }
            else
            {
                resolved.Add(refString);
            }
        }
        return resolved;
    }

    private string GenerateName(CachedJsonFile namesFile, JToken locationData)
    {
        try
        {
            if (namesFile.JsonData == null) return locationData["name"]?.ToString() ?? "Unknown";

            var patterns = namesFile.JsonData["patterns"] as JArray;
            if (patterns == null || !patterns.Any()) return locationData["name"]?.ToString() ?? "Unknown";

            var pattern = GetRandomWeightedPattern(patterns);
            if (pattern == null) return locationData["name"]?.ToString() ?? "Unknown";

            var formatString = pattern["format"]?.ToString() ?? "{base}";
            var components = namesFile.JsonData["components"] as JObject;

            return ReplaceTokens(formatString, components);
        }
        catch
        {
            return locationData["name"]?.ToString() ?? "Unknown";
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
        
        // Handle hierarchical structure: outposts_types -> outposts -> items
        foreach (var property in catalog.Children<JProperty>())
        {
            if (property.Name == "metadata") continue;
            
            // This is a type category (outposts_types, villages_types, etc)
            var typeCategory = property.Value;
            if (typeCategory is JObject typeCategoryObj)
            {
                foreach (var subType in typeCategoryObj.Children<JProperty>())
                {
                    if (subType.Name == "metadata") continue;
                    
                    // This is a specific type (outposts, villages, etc)
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
    /// Hydrates a location by resolving reference IDs to full objects.
    /// Populates NpcObjects, EnemyObjects, and LootObjects properties.
    /// </summary>
    /// <param name="location">The location to hydrate.</param>
    private async Task HydrateLocationAsync(Location location)
    {
        // Resolve NPCs
        if (location.Npcs != null && location.Npcs.Any())
        {
            var npcObjects = new List<NPC>();
            foreach (var refId in location.Npcs)
            {
                try
                {
                    var npcJson = await _referenceResolver.ResolveToObjectAsync(refId);
                    if (npcJson != null)
                    {
                        var npc = npcJson.ToObject<NPC>();
                        if (npc != null)
                        {
                            npcObjects.Add(npc);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to resolve NPC '{RefId}'", refId);
                }
            }
            location.NpcObjects = npcObjects;
        }

        // Resolve enemies
        if (location.Enemies != null && location.Enemies.Any())
        {
            var enemyObjects = new List<Enemy>();
            foreach (var refId in location.Enemies)
            {
                try
                {
                    var enemyJson = await _referenceResolver.ResolveToObjectAsync(refId);
                    if (enemyJson != null)
                    {
                        var enemy = enemyJson.ToObject<Enemy>();
                        if (enemy != null)
                        {
                            enemyObjects.Add(enemy);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to resolve enemy '{RefId}'", refId);
                }
            }
            location.EnemyObjects = enemyObjects;
        }

        // Resolve loot/resources
        if (location.Loot != null && location.Loot.Any())
        {
            var lootObjects = new List<Item>();
            foreach (var refId in location.Loot)
            {
                try
                {
                    var itemJson = await _referenceResolver.ResolveToObjectAsync(refId);
                    if (itemJson != null)
                    {
                        var item = itemJson.ToObject<Item>();
                        if (item != null)
                        {
                            lootObjects.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to resolve loot item '{RefId}'", refId);
                }
            }
            location.LootObjects = lootObjects;
        }
    }
}
