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
    public virtual async Task<List<Location>> GenerateLocationsAsync(string locationType, int count = 5, bool hydrate = true)
    {
        try
        {
            // Validate location type
            if (string.IsNullOrWhiteSpace(locationType))
            {
                _logger.LogWarning("Empty location type provided to GenerateLocationsAsync");
                return new List<Location>();
            }

            var catalogPath = locationType switch
            {
                "towns" => "world/locations/towns/catalog.json",
                "dungeons" => "world/locations/dungeons/catalog.json",
                "wilderness" => "world/locations/wilderness/catalog.json",
                "environments" => "world/environments/catalog.json",
                "regions" => "world/regions/catalog.json",
                _ => $"world/locations/{locationType}/catalog.json" // Assume subdirectory under locations
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
            // Validate parameters
            if (string.IsNullOrWhiteSpace(locationType))
            {
                _logger.LogWarning("Empty location type provided to GenerateLocationByNameAsync");
                return null;
            }

            var catalogPath = locationType switch
            {
                "towns" => "world/locations/towns/catalog.json",
                "dungeons" => "world/locations/dungeons/catalog.json",
                "wilderness" => "world/locations/wilderness/catalog.json",
                "environments" => "world/environments/catalog.json",
                "regions" => "world/regions/catalog.json",
                _ => $"world/locations/{locationType}/catalog.json" // Assume subdirectory under locations
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
                Level = ParseLevel(locationData),
                DangerRating = locationData["dangerRating"]?.Value<int>() ?? ParseDangerRating(locationData),
                HasShop = DetermineHasShop(locationType, locationData),
                HasInn = DetermineHasInn(locationType, locationData),
                IsSafeZone = DetermineIsSafeZone(locationType, locationData)
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

            // Parse and resolve enemy references (from "enemies" or "enemyTypes")
            if (locationData["enemies"] is JArray enemies)
            {
                location.Enemies = await ResolveReferencesAsync(enemies);
            }
            else if (locationData["enemyTypes"] is JArray enemyTypes)
            {
                location.Enemies = await ResolveReferencesAsync(enemyTypes);
            }

            // Parse and resolve loot/resource references (from "loot", "resources", or "rewards")
            if (locationData["loot"] is JArray loot)
            {
                location.Loot = await ResolveReferencesAsync(loot);
            }
            else if (locationData["resources"] is JArray resources)
            {
                location.Loot = await ResolveReferencesAsync(resources);
            }
            else if (locationData["rewards"] is JArray rewards)
            {
                location.Loot = await ResolveReferencesAsync(rewards);
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

    /// <summary>
    /// Parses the level from location data, supporting "level" field or "recommendedLevel" range string.
    /// </summary>
    private int ParseLevel(JToken locationData)
    {
        // Check for direct "level" field
        if (locationData["level"] is JValue levelValue)
        {
            return levelValue.Value<int>();
        }

        // Check for "recommendedLevel" field (e.g., "1-5", "10-15")
        if (locationData["recommendedLevel"] is JValue recommendedLevel)
        {
            var levelStr = recommendedLevel.ToString();
            if (levelStr.Contains("-"))
            {
                var parts = levelStr.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0], out var minLevel))
                {
                    // Use the minimum level from the range
                    return minLevel;
                }
            }
            else if (int.TryParse(levelStr, out var singleLevel))
            {
                return singleLevel;
            }
        }

        // Default random level if not specified
        return _random.Next(1, 20);
    }

    /// <summary>
    /// Parses the danger rating from location data, inferring from difficulty if not specified.
    /// </summary>
    private int ParseDangerRating(JToken locationData)
    {
        // Check for direct "dangerRating" field
        if (locationData["dangerRating"] is JValue dangerRatingValue)
        {
            return dangerRatingValue.Value<int>();
        }

        // Infer from difficulty field
        if (locationData["difficulty"] is JValue difficultyValue)
        {
            var difficulty = difficultyValue.ToString().ToLower();
            return difficulty switch
            {
                "easy" or "low" => 2,
                "medium" or "moderate" => 5,
                "hard" or "high" => 8,
                "deadly" or "extreme" => 10,
                _ => _random.Next(1, 10)
            };
        }

        // Default random danger rating
        return _random.Next(1, 10);
    }

    /// <summary>
    /// Determines if a location has a shop based on type and data.
    /// </summary>
    private bool DetermineHasShop(string locationType, JToken locationData)
    {
        // Check explicit shop flag in data
        if (locationData["hasShop"] is JValue hasShopValue)
        {
            return hasShopValue.Value<bool>();
        }

        // Infer from location type
        return locationType.ToLower() switch
        {
            "towns" => true,        // Towns always have shops
            "dungeons" => false,    // Dungeons never have shops
            "wilderness" => false,  // Wilderness never has shops
            _ => false
        };
    }

    /// <summary>
    /// Determines if a location has an inn based on type and data.
    /// </summary>
    private bool DetermineHasInn(string locationType, JToken locationData)
    {
        // Check explicit inn flag in data
        if (locationData["hasInn"] is JValue hasInnValue)
        {
            return hasInnValue.Value<bool>();
        }

        // Infer from location type
        return locationType.ToLower() switch
        {
            "towns" => true,        // Towns always have inns
            "dungeons" => false,    // Dungeons never have inns
            "wilderness" => false,  // Wilderness never has inns
            _ => false
        };
    }

    /// <summary>
    /// Generates a random enemy appropriate for the given location.
    /// Filters enemies by location type and danger level.
    /// </summary>
    /// <param name="location">The location to generate an enemy for.</param>
    /// <param name="enemyGenerator">The enemy generator service.</param>
    /// <returns>An appropriate enemy for the location, or null if generation fails.</returns>
    public async Task<Enemy?> GenerateLocationAppropriateEnemyAsync(Location location, EnemyGenerator enemyGenerator)
    {
        try
        {
            // Determine enemy category based on location type
            var enemyCategory = location.Type.ToLower() switch
            {
                "dungeons" => GetDungeonEnemyCategory(location),
                "wilderness" => GetWildernessEnemyCategory(location),
                "towns" => "humanoids",  // Towns have guards/criminals if combat happens
                _ => "beasts"  // Default fallback
            };

            // Generate enemies from the appropriate category
            var enemies = await enemyGenerator.GenerateEnemiesAsync(enemyCategory, count: 5, hydrate: true);
            
            // Filter by danger level (use location level as guide)
            var appropriateEnemies = enemies
                .Where(e => Math.Abs(e.Level - location.Level) <= 2)  // Within ±2 levels
                .ToList();

            // If no appropriate enemies, return any from the category
            if (!appropriateEnemies.Any())
            {
                appropriateEnemies = enemies;
            }

            // Return random appropriate enemy
            return appropriateEnemies.Any() 
                ? appropriateEnemies[_random.Next(appropriateEnemies.Count)] 
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating location-appropriate enemy for {LocationName}", location.Name);
            return null;
        }
    }

    /// <summary>
    /// Generates location-appropriate loot with rarity based on danger level.
    /// Higher danger locations yield better loot.
    /// </summary>
    /// <param name="location">The location to generate loot for.</param>
    /// <returns>Loot information including gold and potential item rarity.</returns>
    public LocationLootResult GenerateLocationLoot(Location location)
    {
        try
        {
            var dangerRating = location.DangerRating;
            var locationType = location.Type.ToLower();

            // Base gold rewards scale with danger
            var baseGold = dangerRating * 5;
            var goldAmount = _random.Next(baseGold, baseGold * 3);

            // XP rewards also scale with danger
            var baseXP = dangerRating * 3;
            var xpAmount = _random.Next(baseXP, baseXP * 2);

            // Item drop chance and rarity based on location type and danger
            var itemDropChance = locationType switch
            {
                "dungeons" => 0.5 + (dangerRating * 0.05),  // 50-100% in dungeons
                "wilderness" => 0.3 + (dangerRating * 0.03), // 30-60% in wilderness
                "towns" => 0.1,                               // 10% in towns
                _ => 0.25
            };

            // Determine item rarity based on danger rating
            ItemRarity? suggestedRarity = null;
            if (_random.NextDouble() < itemDropChance)
            {
                suggestedRarity = dangerRating switch
                {
                    <= 2 => ItemRarity.Common,
                    <= 4 => _random.Next(100) < 70 ? ItemRarity.Common : ItemRarity.Uncommon,
                    <= 6 => _random.Next(100) < 50 ? ItemRarity.Uncommon : ItemRarity.Rare,
                    <= 8 => _random.Next(100) < 60 ? ItemRarity.Rare : ItemRarity.Epic,
                    _ => _random.Next(100) < 70 ? ItemRarity.Epic : ItemRarity.Legendary
                };
            }

            return new LocationLootResult
            {
                GoldAmount = goldAmount,
                ExperienceAmount = xpAmount,
                ShouldDropItem = suggestedRarity.HasValue,
                SuggestedItemRarity = suggestedRarity,
                ItemCategory = GetLocationItemCategory(locationType)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating location loot for {LocationName}", location.Name);
            return new LocationLootResult
            {
                GoldAmount = 10,
                ExperienceAmount = 15,
                ShouldDropItem = false
            };
        }
    }

    /// <summary>
    /// Determines the appropriate item category for a location type.
    /// </summary>
    private string GetLocationItemCategory(string locationType)
    {
        return locationType switch
        {
            "dungeons" => "weapons",    // Dungeons drop weapons
            "wilderness" => "materials", // Wilderness drops materials/resources
            "towns" => "consumables",    // Towns drop consumables
            _ => "materials"
        };
    }

    /// <summary>
    /// Determines the appropriate enemy category for a dungeon location.
    /// </summary>
    private string GetDungeonEnemyCategory(Location location)
    {
        // Check location features/metadata for hints
        var features = location.Features.Select(f => f.ToLower()).ToList();
        var name = location.Name.ToLower();

        if (features.Any(f => f.Contains("undead")) || name.Contains("crypt") || name.Contains("tomb"))
            return "undead";
        
        if (features.Any(f => f.Contains("demon")) || name.Contains("abyss") || name.Contains("hell"))
            return "demons";
        
        if (features.Any(f => f.Contains("goblin")) || name.Contains("goblin"))
            return "humanoids";  // Goblins are in humanoids

        // Default dungeon enemies
        return location.DangerRating >= 7 ? "demons" : "undead";
    }

    /// <summary>
    /// Determines the appropriate enemy category for a wilderness location.
    /// </summary>
    private string GetWildernessEnemyCategory(Location location)
    {
        // Check location features/metadata for hints
        var features = location.Features.Select(f => f.ToLower()).ToList();
        var name = location.Name.ToLower();

        if (features.Any(f => f.Contains("forest")) || name.Contains("forest") || name.Contains("woods"))
            return "beasts";  // Wolves, bears
        
        if (features.Any(f => f.Contains("mountain")) || name.Contains("mountain") || name.Contains("peak"))
            return "beasts";  // Mountain creatures
        
        if (features.Any(f => f.Contains("swamp")) || name.Contains("swamp") || name.Contains("marsh"))
            return "undead";  // Swamp creatures, zombies
        
        if (features.Any(f => f.Contains("bandit")) || name.Contains("camp") || name.Contains("hideout"))
            return "humanoids";  // Bandits

        // Default wilderness enemies
        return "beasts";
    }

    /// <summary>
    /// Determines if a location is a safe zone (no random combat) based on type and data.
    /// </summary>
    private bool DetermineIsSafeZone(string locationType, JToken locationData)
    {
        // Check explicit safeZone flag in data
        if (locationData["isSafeZone"] is JValue isSafeValue)
        {
            return isSafeValue.Value<bool>();
        }

        // Infer from location type
        return locationType.ToLower() switch
        {
            "towns" => true,        // Towns are safe zones
            "dungeons" => false,    // Dungeons are dangerous
            "wilderness" => false,  // Wilderness is dangerous
            _ => false
        };
    }
}
