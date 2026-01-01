using RealmEngine.Data.Services;
using RealmEngine.Shared.Models;
using Newtonsoft.Json.Linq;

namespace RealmEngine.Core.Generators.Modern;

/// <summary>
/// Generates Ability instances from abilities catalog JSON files.
/// Supports different ability types (passive, active, offensive, defensive, support) and rarity-based selection.
/// </summary>
public class AbilityGenerator
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of the AbilityGenerator class.
    /// </summary>
    /// <param name="dataCache">The game data cache for accessing ability catalog files.</param>
    /// <param name="referenceResolver">The reference resolver for resolving JSON references.</param>
    public AbilityGenerator(GameDataCache dataCache, ReferenceResolverService referenceResolver)
    {
        _dataCache = dataCache ?? throw new ArgumentNullException(nameof(dataCache));
        _referenceResolver = referenceResolver ?? throw new ArgumentNullException(nameof(referenceResolver));
        _random = new Random();
    }

    /// <summary>
    /// Generates a list of random abilities from a specific category and subcategory.
    /// </summary>
    /// <param name="category">The ability category (e.g., "active", "passive").</param>
    /// <param name="subcategory">The ability subcategory (e.g., "offensive", "defensive", "support").</param>
    /// <param name="count">The number of abilities to generate (default: 5).</param>
    /// <returns>A list of generated Ability instances.</returns>
    public Task<List<Ability>> GenerateAbilitiesAsync(string category, string subcategory, int count = 5)
    {
        try
        {
            var catalogFile = _dataCache.GetFile($"abilities/{category}/{subcategory}/catalog.json");
            if (catalogFile?.JsonData == null)
            {
                return Task.FromResult(new List<Ability>());
            }

            var catalog = catalogFile.JsonData;
            var items = GetItemsFromCatalog(catalog);
            
            if (items == null || !items.Any())
            {
                return Task.FromResult(new List<Ability>());
            }

            var result = new List<Ability>();

            for (int i = 0; i < count; i++)
            {
                var randomAbility = GetRandomWeightedItem(items);
                if (randomAbility != null)
                {
                    var ability = ConvertToAbility(randomAbility, category, subcategory);
                    if (ability != null)
                    {
                        result.Add(ability);
                    }
                }
            }

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating abilities for category {category}/{subcategory}: {ex.Message}");
            return Task.FromResult(new List<Ability>());
        }
    }

    /// <summary>
    /// Generates a specific ability by name from a category and subcategory.
    /// </summary>
    /// <param name="category">The ability category to search in.</param>
    /// <param name="subcategory">The ability subcategory to search in.</param>
    /// <param name="abilityName">The name of the ability to generate.</param>
    /// <returns>The generated Ability instance, or null if not found.</returns>
    public Task<Ability?> GenerateAbilityByNameAsync(string category, string subcategory, string abilityName)
    {
        try
        {
            var catalogFile = _dataCache.GetFile($"abilities/{category}/{subcategory}/catalog.json");
            if (catalogFile?.JsonData == null)
            {
                return Task.FromResult<Ability?>(null);
            }

            var catalog = catalogFile.JsonData;
            var items = GetItemsFromCatalog(catalog);
            
            var catalogAbility = items?.FirstOrDefault(a => 
                string.Equals(GetStringProperty(a, "name"), abilityName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(GetStringProperty(a, "displayName"), abilityName, StringComparison.OrdinalIgnoreCase));

            if (catalogAbility != null)
            {
                return Task.FromResult(ConvertToAbility(catalogAbility, category, subcategory));
            }

            return Task.FromResult<Ability?>(null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating ability {abilityName} from category {category}/{subcategory}: {ex.Message}");
            return Task.FromResult<Ability?>(null);
        }
    }

    private static IEnumerable<JToken>? GetItemsFromCatalog(JToken catalog)
    {
        var allItems = new List<JToken>();
        
        // Handle hierarchical structure: ability_types -> specific type -> items
        foreach (var property in catalog.Children<JProperty>())
        {
            if (property.Name == "metadata") continue;
            
            // This is a type category (ability_types, etc)
            var typeCategory = property.Value;
            if (typeCategory is JObject typeCategoryObj)
            {
                foreach (var subType in typeCategoryObj.Children<JProperty>())
                {
                    if (subType.Name == "metadata") continue;
                    
                    // This is a specific type with items array
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

    private Ability? ConvertToAbility(JToken catalogAbility, string category, string subcategory)
    {
        try
        {
            var name = GetStringProperty(catalogAbility, "name") ?? GetStringProperty(catalogAbility, "displayName") ?? "unknown-ability";
            var ability = new Ability
            {
                Id = $"{category}/{subcategory}:{name}",
                Name = name,
                DisplayName = GetStringProperty(catalogAbility, "displayName") ?? name,
                Description = GetStringProperty(catalogAbility, "description") ?? "An ability",
                RarityWeight = GetIntProperty(catalogAbility, "rarityWeight", 1),
                BaseDamage = GetStringProperty(catalogAbility, "baseDamage"),
                Cooldown = GetIntProperty(catalogAbility, "cooldown", 0),
                Range = GetIntProperty(catalogAbility, "range", null),
                ManaCost = GetIntPropertyFromTraits(catalogAbility, "manaCost", 0),
                Duration = GetIntPropertyFromTraits(catalogAbility, "duration", null),
                IsPassive = category == "passive",
                RequiredLevel = GetIntProperty(catalogAbility, "requiredLevel", 1),
                Type = MapAbilityType(category, subcategory)
            };

            // Parse traits dictionary
            var traits = catalogAbility["traits"];
            if (traits != null && traits is JObject traitsObj)
            {
                foreach (var trait in traitsObj.Properties())
                {
                    var traitValue = trait.Value;
                    if (traitValue is JObject traitObj && traitObj["value"] != null)
                    {
                        ability.Traits[trait.Name] = ExtractTraitValue(traitObj["value"]!);
                    }
                    else
                    {
                        ability.Traits[trait.Name] = traitValue?.ToString() ?? "";
                    }
                }
            }

            return ability;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting catalog ability to Ability: {ex.Message}");
            return null;
        }
    }

    private static object ExtractTraitValue(JToken valueToken)
    {
        return valueToken.Type switch
        {
            JTokenType.String => valueToken.Value<string>() ?? "",
            JTokenType.Integer => valueToken.Value<int>(),
            JTokenType.Float => valueToken.Value<double>(),
            JTokenType.Boolean => valueToken.Value<bool>(),
            _ => valueToken.ToString()
        };
    }

    private static AbilityTypeEnum MapAbilityType(string category, string subcategory)
    {
        return category.ToLower() switch
        {
            "active" => subcategory.ToLower() switch
            {
                "offensive" => AbilityTypeEnum.Offensive,
                "defensive" => AbilityTypeEnum.Defensive,
                "support" => AbilityTypeEnum.Support,
                "utility" => AbilityTypeEnum.Utility,
                "mobility" => AbilityTypeEnum.Mobility,
                "summon" => AbilityTypeEnum.Summon,
                "control" => AbilityTypeEnum.Crowd_Control,
                _ => AbilityTypeEnum.Offensive
            },
            "passive" => AbilityTypeEnum.Passive,
            "reactive" => AbilityTypeEnum.Defensive,
            "ultimate" => AbilityTypeEnum.Offensive,
            _ => AbilityTypeEnum.Offensive
        };
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

    private static int GetIntProperty(JToken obj, string propertyName, int? defaultValue)
    {
        try
        {
            var value = obj[propertyName];
            if (value != null)
            {
                return value.Value<int>();
            }
            return defaultValue ?? 0;
        }
        catch
        {
            return defaultValue ?? 0;
        }
    }

    private static int GetIntPropertyFromTraits(JToken obj, string propertyName, int? defaultValue)
    {
        try
        {
            var traits = obj["traits"];
            if (traits != null && traits[propertyName] != null)
            {
                var trait = traits[propertyName];
                if (trait is JObject traitObj && traitObj["value"] != null)
                {
                    return traitObj["value"]!.Value<int>();
                }
            }
            return defaultValue ?? 0;
        }
        catch
        {
            return defaultValue ?? 0;
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
}
