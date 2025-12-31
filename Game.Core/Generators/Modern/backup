using Game.Data.Services;
using Game.Shared.Data.Models;
using Game.Shared.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Game.Core.Generators.Modern;

/// <summary>
/// Generates Ability instances from abilities catalogs using GameDataCache.
/// </summary>
public class AbilityGenerator
{
    private readonly GameDataCache _dataCache;
    private readonly ILogger<AbilityGenerator> _logger;
    private readonly Dictionary<string, AbilityCatalogData> _catalogCache = new();

    public AbilityGenerator(GameDataCache dataCache, ILogger<AbilityGenerator> logger)
    {
        _dataCache = dataCache ?? throw new ArgumentNullException(nameof(dataCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Loads an ability catalog from JSON.
    /// Path format: "abilities/active/offensive/catalog.json"
    /// </summary>
    private AbilityCatalogData LoadCatalog(string catalogPath)
    {
        if (_catalogCache.TryGetValue(catalogPath, out var cached))
            return cached;

        var cachedFile = _dataCache.GetFile(catalogPath);
        if (cachedFile == null)
        {
            _logger.LogError("Failed to load {Path} from cache", catalogPath);
            throw new InvalidOperationException($"Ability catalog not found: {catalogPath}");
        }

        try
        {
            var catalogData = JsonConvert.DeserializeObject<AbilityCatalogData>(cachedFile.JsonData.ToString());
            if (catalogData == null)
            {
                _logger.LogError("Failed to deserialize {Path}", catalogPath);
                throw new InvalidOperationException($"Failed to deserialize ability catalog: {catalogPath}");
            }

            _catalogCache[catalogPath] = catalogData;
            _logger.LogInformation("Loaded ability catalog: {Path}", catalogPath);
            return catalogData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing {Path}", catalogPath);
            throw;
        }
    }

    /// <summary>
    /// Gets ability by ID format "active/offensive:basic-attack".
    /// </summary>
    public Ability? GetAbilityById(string abilityId)
    {
        // Parse ID: "active/offensive:basic-attack" -> category: "active/offensive", name: "basic-attack"
        var parts = abilityId.Split(':', 2);
        if (parts.Length != 2)
        {
            _logger.LogWarning("Invalid ability ID format: {Id}", abilityId);
            return null;
        }

        var categoryPath = parts[0]; // "active/offensive"
        var abilityName = parts[1];   // "basic-attack"

        // Build catalog path: "abilities/active/offensive/catalog.json"
        var catalogPath = $"abilities/{categoryPath}/catalog.json";

        try
        {
            var catalog = LoadCatalog(catalogPath);

            // Find ability in catalog
            foreach (var (typeKey, abilityType) in catalog.AbilityTypes)
            {
                var abilityData = abilityType.Items.FirstOrDefault(a => 
                    a.Name.Equals(abilityName, StringComparison.OrdinalIgnoreCase));

                if (abilityData != null)
                {
                    return MapToAbility(abilityData, categoryPath, typeKey);
                }
            }

            _logger.LogWarning("Ability not found: {Id}", abilityId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading ability: {Id}", abilityId);
            return null;
        }
    }

    /// <summary>
    /// Gets all abilities from a specific category (e.g., "active/offensive").
    /// </summary>
    public List<Ability> GetAbilitiesByCategory(string categoryPath)
    {
        var catalogPath = $"abilities/{categoryPath}/catalog.json";

        try
        {
            var catalog = LoadCatalog(catalogPath);
            var abilities = new List<Ability>();

            foreach (var (typeKey, abilityType) in catalog.AbilityTypes)
            {
                foreach (var abilityData in abilityType.Items)
                {
                    var ability = MapToAbility(abilityData, categoryPath, typeKey);
                    abilities.Add(ability);
                }
            }

            _logger.LogInformation("Found {Count} abilities in {Category}", abilities.Count, categoryPath);
            return abilities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading abilities from {Category}", categoryPath);
            return new List<Ability>();
        }
    }

    /// <summary>
    /// Gets multiple abilities by their IDs.
    /// </summary>
    public List<Ability> GetAbilitiesByIds(IEnumerable<string> abilityIds)
    {
        var abilities = new List<Ability>();

        foreach (var id in abilityIds)
        {
            var ability = GetAbilityById(id);
            if (ability != null)
            {
                abilities.Add(ability);
            }
        }

        return abilities;
    }

    /// <summary>
    /// Maps AbilityItem to Ability model.
    /// </summary>
    private Ability MapToAbility(AbilityItem data, string categoryPath, string typeKey)
    {
        var ability = new Ability
        {
            Id = $"{categoryPath}:{data.Name}",
            Name = data.Name,
            DisplayName = data.DisplayName,
            Description = data.Description,
            RarityWeight = data.RarityWeight,
            BaseDamage = data.BaseDamage,
            Cooldown = data.Cooldown ?? 0,
            Range = data.Range,
            ManaCost = 0, // Not in JSON yet
            Duration = null, // Not in JSON yet
            IsPassive = categoryPath.Contains("passive"),
            RequiredLevel = 1, // Not in JSON yet
            Traits = data.Traits.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value),
            AllowedClasses = new List<string>() // Not in JSON yet
        };

        // Parse type from category path (last segment)
        ability.Type = ParseAbilityType(categoryPath);

        return ability;
    }

    /// <summary>
    /// Parses ability type from category path (e.g., "active/offensive" -> Offensive).
    /// </summary>
    private AbilityTypeEnum ParseAbilityType(string categoryPath)
    {
        // categoryPath: "active/offensive", "passive", "active/support", etc.
        var parts = categoryPath.Split('/');
        var typeString = parts[^1]; // Last part is the type

        if (Enum.TryParse<AbilityTypeEnum>(typeString, ignoreCase: true, out var abilityType))
        {
            return abilityType;
        }

        _logger.LogWarning("Unknown ability type: {Type}, defaulting to Offensive", typeString);
        return AbilityTypeEnum.Offensive;
    }
}
