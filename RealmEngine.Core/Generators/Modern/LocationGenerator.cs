using RealmEngine.Data.Services;
using RealmEngine.Shared.Models;
using Newtonsoft.Json.Linq;

namespace RealmEngine.Core.Generators.Modern;

/// <summary>
/// Generates world locations (towns, dungeons, wilderness, environments, regions).
/// </summary>
public class LocationGenerator
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly Random _random;

    public LocationGenerator(GameDataCache dataCache, ReferenceResolverService referenceResolver)
    {
        _dataCache = dataCache ?? throw new ArgumentNullException(nameof(dataCache));
        _referenceResolver = referenceResolver ?? throw new ArgumentNullException(nameof(referenceResolver));
        _random = new Random();
    }

    /// <summary>
    /// Generates locations from a specific type (towns, dungeons, wilderness, environments, regions).
    /// </summary>
    public Task<List<Location>> GenerateLocationsAsync(string locationType, int count = 5)
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
                return Task.FromResult(new List<Location>());
            }

            var catalog = catalogFile.JsonData;
            var items = GetItemsFromCatalog(catalog);
            
            if (items == null || !items.Any())
            {
                return Task.FromResult(new List<Location>());
            }

            var result = new List<Location>();
            var namesFile = _dataCache.GetFile(catalogPath.Replace("catalog.json", "names.json"));

            for (int i = 0; i < count; i++)
            {
                var randomLocation = GetRandomWeightedItem(items);
                if (randomLocation != null)
                {
                    var location = ConvertToLocation(randomLocation, locationType, namesFile);
                    if (location != null)
                    {
                        result.Add(location);
                    }
                }
            }

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating locations: {ex.Message}");
            return Task.FromResult(new List<Location>());
        }
    }

    /// <summary>
    /// Generates a specific location by name.
    /// </summary>
    public Task<Location?> GenerateLocationByNameAsync(string locationType, string locationName)
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
                return Task.FromResult<Location?>(null);
            }

            var catalog = catalogFile.JsonData;
            var items = GetItemsFromCatalog(catalog);
            var item = items?.FirstOrDefault(i => i["name"]?.ToString() == locationName);

            if (item == null)
            {
                return Task.FromResult<Location?>(null);
            }

            var namesFile = _dataCache.GetFile(catalogPath.Replace("catalog.json", "names.json"));
            return Task.FromResult(ConvertToLocation(item, locationType, namesFile));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating location by name: {ex.Message}");
            return Task.FromResult<Location?>(null);
        }
    }

    private Location? ConvertToLocation(JToken locationData, string locationType, CachedJsonFile? namesFile)
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

            // Parse NPCs (if any)
            if (locationData["npcs"] is JArray npcs)
            {
                location.Npcs = npcs.Select(n => n.ToString()).ToList();
            }

            // Parse enemies (if any)
            if (locationData["enemies"] is JArray enemies)
            {
                location.Enemies = enemies.Select(e => e.ToString()).ToList();
            }

            // Parse loot (if any)
            if (locationData["loot"] is JArray loot)
            {
                location.Loot = loot.Select(l => l.ToString()).ToList();
            }

            // Parse parent region
            location.ParentRegion = locationData["region"]?.ToString();

            return location;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting location: {ex.Message}");
            return null;
        }
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
}
