using RealmEngine.Data.Services;
using RealmEngine.Shared.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace RealmEngine.Core.Features.Progression.Services;

/// <summary>
/// Service for loading and accessing ability definitions from ability catalogs.
/// Loads from 4 catalog files: active, passive, reactive, ultimate.
/// </summary>
public class AbilityCatalogService
{
    private readonly GameDataCache _dataCache;
    private readonly ILogger<AbilityCatalogService> _logger;
    private readonly Dictionary<string, Ability> _abilities = new();
    private readonly Dictionary<string, List<string>> _abilitiesByClass = new();
    private bool _initialized;

    public AbilityCatalogService(GameDataCache dataCache, ILogger<AbilityCatalogService>? logger = null)
    {
        _dataCache = dataCache ?? throw new ArgumentNullException(nameof(dataCache));
        _logger = logger ?? NullLogger<AbilityCatalogService>.Instance;
    }

    /// <summary>
    /// Initialize by loading all ability catalogs.
    /// </summary>
    public Task InitializeAsync()
    {
        if (_initialized)
        {
            _logger.LogWarning("AbilityCatalogService already initialized");
            return Task.CompletedTask;
        }

        try
        {
            // Load all 4 ability catalogs
            var catalogTypes = new[] { "active", "passive", "reactive", "ultimate" };
            
            foreach (var catalogType in catalogTypes)
            {
                var catalogPath = $"abilities/{catalogType}/catalog.json";
                var catalogFile = _dataCache.GetFile(catalogPath);
                
                if (catalogFile == null)
                {
                    _logger.LogWarning("Failed to load {CatalogType} abilities catalog", catalogType);
                    continue;
                }

                ParseAbilityCatalog(catalogFile.JsonData, catalogType);
            }

            _initialized = true;
            _logger.LogInformation("AbilityCatalogService initialized with {Count} abilities", _abilities.Count);
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize AbilityCatalogService");
            throw;
        }
    }

    /// <summary>
    /// Get ability by ID.
    /// </summary>
    public Ability? GetAbility(string abilityId)
    {
        EnsureInitialized();
        return _abilities.GetValueOrDefault(abilityId);
    }

    /// <summary>
    /// Get all abilities.
    /// </summary>
    public IReadOnlyDictionary<string, Ability> GetAllAbilities()
    {
        EnsureInitialized();
        return _abilities;
    }

    /// <summary>
    /// Get abilities by activation type.
    /// </summary>
    public List<Ability> GetAbilitiesByType(AbilityTypeEnum type)
    {
        EnsureInitialized();
        return _abilities.Values.Where(a => a.Type == type).ToList();
    }

    /// <summary>
    /// Get abilities by tier (based on selectionWeight).
    /// </summary>
    public List<Ability> GetAbilitiesByTier(int tier)
    {
        EnsureInitialized();
        return _abilities.Values.Where(a => CalculateTier(a) == tier).ToList();
    }

    /// <summary>
    /// Get starting abilities for a class (tier 1 abilities).
    /// </summary>
    public List<Ability> GetStartingAbilities(string className)
    {
        EnsureInitialized();
        
        // Get tier 1 abilities for this class
        return _abilities.Values
            .Where(a => CalculateTier(a) == 1)
            .Where(a => a.AllowedClasses.Count == 0 || 
                       a.AllowedClasses.Any(c => c.Equals(className, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    /// <summary>
    /// Get abilities unlockable at a given level.
    /// </summary>
    public List<Ability> GetUnlockableAbilities(string className, int level)
    {
        EnsureInitialized();
        
        return _abilities.Values
            .Where(a => a.RequiredLevel <= level)
            .Where(a => a.AllowedClasses.Count == 0 || 
                       a.AllowedClasses.Any(c => c.Equals(className, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    /// <summary>
    /// Calculate ability tier from selectionWeight.
    /// Tier 1: less than 50, Tier 2: 50-99, Tier 3: 100-199, Tier 4: 200-399, Tier 5: 400 or more
    /// </summary>
    public int CalculateTier(Ability ability)
    {
        // Ultimates are always tier 5
        if (ability.Id.Contains("ultimate/", StringComparison.OrdinalIgnoreCase))
        {
            return 5;
        }

        var weight = ability.RarityWeight;
        
        if (weight < 50) return 1;
        if (weight < 100) return 2;
        if (weight < 200) return 3;
        if (weight < 400) return 4;
        return 5;
    }

    /// <summary>
    /// Calculate required level to unlock an ability based on tier.
    /// Tier 1 = Level 1, Tier 2 = Level 5, Tier 3 = Level 10, Tier 4 = Level 15, Tier 5 = Level 20
    /// </summary>
    public int GetRequiredLevelForTier(int tier)
    {
        return tier switch
        {
            1 => 1,
            2 => 5,
            3 => 10,
            4 => 15,
            5 => 20,
            _ => 1
        };
    }

    private void ParseAbilityCatalog(JObject catalog, string catalogType)
    {
        var types = catalog["types"] as JObject;
        if (types == null)
        {
            _logger.LogWarning("{CatalogType} catalog missing 'types' property", catalogType);
            return;
        }

        foreach (var typeProp in types.Properties())
        {
            var subcategory = typeProp.Name;
            var typeData = typeProp.Value as JObject;
            if (typeData == null) continue;

            var items = typeData["items"] as JArray;
            if (items == null) continue;

            foreach (var item in items)
            {
                try
                {
                    var ability = ParseAbility(item, catalogType, subcategory);
                    if (ability != null)
                    {
                        var abilityId = $"{catalogType}/{subcategory}:{ability.Name}".ToLower();
                        ability.Id = abilityId;
                        _abilities[abilityId] = ability;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse ability: {Item}", item);
                }
            }
        }
    }

    private Ability? ParseAbility(JToken item, string category, string subcategory)
    {
        var name = item["name"]?.ToString() ?? item["displayName"]?.ToString();
        if (string.IsNullOrEmpty(name))
        {
            _logger.LogWarning("Ability missing name, skipping");
            return null;
        }

        var ability = new Ability
        {
            Id = "", // Will be set by caller
            Name = name,
            DisplayName = item["displayName"]?.ToString() ?? name,
            Description = item["description"]?.ToString() ?? "",
            RarityWeight = item["rarityWeight"]?.Value<int>() ?? 1,
            BaseDamage = item["baseDamage"]?.ToString(),
            IsPassive = category.Equals("passive", StringComparison.OrdinalIgnoreCase),
            Type = MapAbilityType(category, subcategory)
        };

        // Parse traits
        var traits = item["traits"] as JObject;
        if (traits != null)
        {
            foreach (var trait in traits.Properties())
            {
                var traitValue = trait.Value;
                if (traitValue is JObject traitObj && traitObj["value"] != null)
                {
                    var value = traitObj["value"];
                    
                    // Extract specific traits
                    if (trait.Name == "manaCost" && value != null)
                    {
                        ability.ManaCost = value.Value<int>();
                    }
                    else if (trait.Name == "cooldown" && value != null)
                    {
                        ability.Cooldown = value.Value<int>();
                    }
                    else if (trait.Name == "range" && value != null)
                    {
                        ability.Range = value.Value<int>();
                    }
                    else if (trait.Name == "duration" && value != null)
                    {
                        ability.Duration = value.Value<int>();
                    }
                    
                    // Store all traits in dictionary
                    if (value != null)
                    {
                        ability.Traits[trait.Name] = ExtractTraitValue(value);
                    }
                }
            }
        }

        // Calculate required level from tier
        var tier = CalculateTierFromWeight(ability.RarityWeight, category);
        ability.RequiredLevel = GetRequiredLevelForTier(tier);

        return ability;
    }

    private int CalculateTierFromWeight(int weight, string category)
    {
        if (category.Equals("ultimate", StringComparison.OrdinalIgnoreCase))
        {
            return 5;
        }

        if (weight < 50) return 1;
        if (weight < 100) return 2;
        if (weight < 200) return 3;
        if (weight < 400) return 4;
        return 5;
    }

    private object ExtractTraitValue(JToken value)
    {
        switch (value.Type)
        {
            case JTokenType.Integer:
                return value.Value<int>();
            case JTokenType.Float:
                return value.Value<double>();
            case JTokenType.String:
                return value.Value<string>() ?? "";
            case JTokenType.Boolean:
                return value.Value<bool>();
            case JTokenType.Array:
                return value.ToObject<List<object>>() ?? new List<object>();
            case JTokenType.Object:
                return value.ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>();
            default:
                return value.ToString();
        }
    }

    private AbilityTypeEnum MapAbilityType(string category, string subcategory)
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

    private void EnsureInitialized()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("AbilityCatalogService not initialized. Call InitializeAsync() first.");
        }
    }
}
