using RealmEngine.Data.Services;
using RealmEngine.Shared.Models;
using Newtonsoft.Json.Linq;

namespace RealmEngine.Core.Generators.Modern;

/// <summary>
/// Generates Enemy instances from enemies catalog JSON files.
/// Supports hierarchical categorization (e.g., beasts/wolves, undead/skeletons) and weighted random selection.
/// </summary>
public class EnemyGenerator
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of the EnemyGenerator class.
    /// </summary>
    /// <param name="dataCache">The game data cache for accessing enemy catalog files.</param>
    /// <param name="referenceResolver">The reference resolver for resolving JSON references.</param>
    public EnemyGenerator(GameDataCache dataCache, ReferenceResolverService referenceResolver)
    {
        _dataCache = dataCache ?? throw new ArgumentNullException(nameof(dataCache));
        _referenceResolver = referenceResolver ?? throw new ArgumentNullException(nameof(referenceResolver));
        _random = new Random();
    }

    /// <summary>
    /// Generates a list of random enemies from a specific category.
    /// </summary>
    /// <param name="category">The enemy category (e.g., "beasts", "undead", "dragons").</param>
    /// <param name="count">The number of enemies to generate (default: 5).</param>
    /// <param name="hydrate">If true, populates resolved Abilities and LootTable properties (default: true).</param>
    /// <returns>A list of generated Enemy instances.</returns>
    public async Task<List<Enemy>> GenerateEnemiesAsync(string category, int count = 5, bool hydrate = true)
    {
        try
        {
            var catalogFile = _dataCache.GetFile($"enemies/{category}/catalog.json");
            if (catalogFile?.JsonData == null)
            {
                return new List<Enemy>();
            }

            var catalog = catalogFile.JsonData;
            var items = GetItemsFromCatalog(catalog);
            
            if (items == null || !items.Any())
            {
                return new List<Enemy>();
            }

            var result = new List<Enemy>();

            for (int i = 0; i < count; i++)
            {
                var randomEnemy = GetRandomWeightedItem(items);
                if (randomEnemy != null)
                {
                    var enemy = await ConvertToEnemyAsync(randomEnemy, category);
                    if (enemy != null)
                    {
                        if (hydrate)
                        {
                            await HydrateEnemyAsync(enemy);
                        }
                        result.Add(enemy);
                    }
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating enemies for category {category}: {ex.Message}");
            return new List<Enemy>();
        }
    }

    /// <summary>
    /// Generates a specific enemy by name from a category.
    /// </summary>
    /// <param name="category">The enemy category to search in.</param>
    /// <param name="enemyName">The name of the enemy to generate.</param>
    /// <param name="hydrate">If true, populates resolved Abilities and LootTable properties (default: true).</param>
    /// <returns>The generated Enemy instance, or null if not found.</returns>
    public async Task<Enemy?> GenerateEnemyByNameAsync(string category, string enemyName, bool hydrate = true)
    {
        try
        {
            var catalogFile = _dataCache.GetFile($"enemies/{category}/catalog.json");
            if (catalogFile?.JsonData == null)
            {
                return null;
            }

            var catalog = catalogFile.JsonData;
            var items = GetItemsFromCatalog(catalog);
            
            var catalogEnemy = items?.FirstOrDefault(e => 
                string.Equals(GetStringProperty(e, "name"), enemyName, StringComparison.OrdinalIgnoreCase));

            if (catalogEnemy != null)
            {
                var enemy = await ConvertToEnemyAsync(catalogEnemy, category);
                if (enemy != null && hydrate)
                {
                    await HydrateEnemyAsync(enemy);
                }
                return enemy;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating enemy {enemyName} from category {category}: {ex.Message}");
            return null;
        }
    }

    private static IEnumerable<JToken>? GetItemsFromCatalog(JToken catalog)
    {
        var allItems = new List<JToken>();
        
        // Handle hierarchical structure: beast_types -> wolves/bears/etc -> items
        foreach (var property in catalog.Children<JProperty>())
        {
            if (property.Name == "metadata") continue;
            
            // This is a type category (beast_types, undead_types, etc)
            var typeCategory = property.Value;
            if (typeCategory is JObject typeCategoryObj)
            {
                foreach (var subType in typeCategoryObj.Children<JProperty>())
                {
                    if (subType.Name == "metadata") continue;
                    
                    // This is a specific type (wolves, bears, etc)
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

    private async Task<Enemy?> ConvertToEnemyAsync(JToken catalogEnemy, string category)
    {
        try
        {
            var enemy = new Enemy
            {
                Id = $"{category}:{GetStringProperty(catalogEnemy, "name")}",
                Name = GetStringProperty(catalogEnemy, "name") ?? "Unknown Enemy",
                Description = GetStringProperty(catalogEnemy, "description") ?? "A mysterious creature",
                Health = GetIntProperty(catalogEnemy, "health", 50),
                MaxHealth = GetIntProperty(catalogEnemy, "health", 50),
                Level = GetIntProperty(catalogEnemy, "level", 1),
                
                // Map stats from catalog
                Strength = GetIntProperty(catalogEnemy, "strength", 10),
                Dexterity = GetIntProperty(catalogEnemy, "dexterity", 10),
                Constitution = GetIntProperty(catalogEnemy, "constitution", 10),
                Intelligence = GetIntProperty(catalogEnemy, "intelligence", 10),
                Wisdom = GetIntProperty(catalogEnemy, "wisdom", 10),
                Charisma = GetIntProperty(catalogEnemy, "charisma", 10),
                
                // Map damage properties
                BasePhysicalDamage = GetIntProperty(catalogEnemy, "attack", 5),
                BaseMagicDamage = GetIntProperty(catalogEnemy, "magicAttack", 0),
                
                // Map rewards
                XPReward = GetIntProperty(catalogEnemy, "xp", 25),
                GoldReward = GetIntProperty(catalogEnemy, "gold", 10)
            };

            // Resolve ability references
            var abilities = GetStringArrayProperty(catalogEnemy, "abilities");
            if (abilities?.Any() == true)
            {
                var resolvedAbilities = new List<string>();
                foreach (var ability in abilities)
                {
                    if (ability.StartsWith("@"))
                    {
                        var resolved = await _referenceResolver.ResolveAsync(ability);
                        if (resolved is string resolvedId)
                        {
                            resolvedAbilities.Add(resolvedId);
                        }
                    }
                    else
                    {
                        resolvedAbilities.Add(ability);
                    }
                }
                enemy.AbilityIds = resolvedAbilities;
            }

            // Resolve loot table references
            var loot = GetStringArrayProperty(catalogEnemy, "loot");
            if (loot?.Any() == true)
            {
                var resolvedLoot = new List<string>();
                foreach (var lootRef in loot)
                {
                    if (lootRef.StartsWith("@"))
                    {
                        var resolved = await _referenceResolver.ResolveAsync(lootRef);
                        if (resolved is string resolvedId)
                        {
                            resolvedLoot.Add(resolvedId);
                        }
                    }
                    else
                    {
                        resolvedLoot.Add(lootRef);
                    }
                }
                enemy.LootTableIds = resolvedLoot;
            }

            return enemy;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting catalog enemy to Enemy: {ex.Message}");
            return null;
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

    private static string[]? GetStringArrayProperty(JToken obj, string propertyName)
    {
        try
        {
            var value = obj[propertyName];
            if (value == null) return null;

            if (value is JArray array)
            {
                return array.Select(x => x?.Value<string>()).Where(x => x != null).ToArray()!;
            }

            if (value.Type == JTokenType.String)
            {
                // Handle space-separated string format
                var stringValue = value.Value<string>();
                return stringValue?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Hydrates an enemy by resolving reference IDs to full objects.
    /// Populates Abilities and LootTable properties.
    /// </summary>
    /// <param name="enemy">The enemy to hydrate.</param>
    private async Task HydrateEnemyAsync(Enemy enemy)
    {
        // Resolve abilities
        if (enemy.AbilityIds != null && enemy.AbilityIds.Any())
        {
            var abilities = new List<Ability>();
            foreach (var refId in enemy.AbilityIds)
            {
                try
                {
                    var abilityJson = await _referenceResolver.ResolveToObjectAsync(refId);
                    if (abilityJson != null)
                    {
                        var ability = abilityJson.ToObject<Ability>();
                        if (ability != null)
                        {
                            abilities.Add(ability);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to resolve ability '{refId}': {ex.Message}");
                }
            }
            enemy.Abilities = abilities;
        }

        // Resolve loot table
        if (enemy.LootTableIds != null && enemy.LootTableIds.Any())
        {
            var lootTable = new List<Item>();
            foreach (var refId in enemy.LootTableIds)
            {
                try
                {
                    var itemJson = await _referenceResolver.ResolveToObjectAsync(refId);
                    if (itemJson != null)
                    {
                        var item = itemJson.ToObject<Item>();
                        if (item != null)
                        {
                            lootTable.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to resolve loot item '{refId}': {ex.Message}");
                }
            }
            enemy.LootTable = lootTable;
        }
    }
}