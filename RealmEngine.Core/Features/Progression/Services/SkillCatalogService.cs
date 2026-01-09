using RealmEngine.Data.Services;
using RealmEngine.Shared.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace RealmEngine.Core.Features.Progression.Services;

/// <summary>
/// Service for loading and accessing skill definitions from skills/catalog.json.
/// Provides skill metadata, XP costs, effects, and XP actions.
/// </summary>
public class SkillCatalogService
{
    private readonly GameDataCache _dataCache;
    private readonly ILogger<SkillCatalogService> _logger;
    private readonly Dictionary<string, SkillDefinition> _skillDefinitions = new();
    private bool _initialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="SkillCatalogService"/> class.
    /// </summary>
    /// <param name="dataCache">The game data cache.</param>
    /// <param name="logger">Optional logger instance.</param>
    public SkillCatalogService(GameDataCache dataCache, ILogger<SkillCatalogService>? logger = null)
    {
        _dataCache = dataCache ?? throw new ArgumentNullException(nameof(dataCache));
        _logger = logger ?? NullLogger<SkillCatalogService>.Instance;
    }

    /// <summary>
    /// Initialize skill catalog by loading from JSON.
    /// </summary>
    public Task InitializeAsync()
    {
        if (_initialized)
        {
            _logger.LogWarning("SkillCatalogService already initialized");
            return Task.CompletedTask;
        }

        try
        {
            var catalogPath = "skills/catalog.json";
            var catalogFile = _dataCache.GetFile(catalogPath);
            
            if (catalogFile == null)
            {
                throw new InvalidOperationException($"Failed to load skills catalog from {catalogPath}");
            }

            ParseSkillCatalog(catalogFile.JsonData);
            
            _initialized = true;
            _logger.LogInformation("SkillCatalogService initialized with {Count} skills", _skillDefinitions.Count);
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize SkillCatalogService");
            throw;
        }
    }

    /// <summary>
    /// Get skill definition by ID.
    /// </summary>
    public virtual SkillDefinition? GetSkillDefinition(string skillId)
    {
        EnsureInitialized();
        return _skillDefinitions.GetValueOrDefault(skillId);
    }

    /// <summary>
    /// Get all skill definitions.
    /// </summary>
    public IReadOnlyDictionary<string, SkillDefinition> GetAllSkills()
    {
        EnsureInitialized();
        return _skillDefinitions;
    }

    /// <summary>
    /// Get skills by category.
    /// </summary>
    public List<SkillDefinition> GetSkillsByCategory(string category)
    {
        EnsureInitialized();
        return _skillDefinitions.Values
            .Where(s => s.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Calculate XP required to reach next rank.
    /// Formula: baseXPCost + (currentRank * baseXPCost * costMultiplier)
    /// </summary>
    public int CalculateXPToNextRank(string skillId, int currentRank)
    {
        var skillDef = GetSkillDefinition(skillId);
        if (skillDef == null)
        {
            _logger.LogWarning("Unknown skill ID: {SkillId}", skillId);
            return 100; // Default fallback
        }

        if (currentRank >= skillDef.MaxRank)
        {
            return int.MaxValue; // Already at max rank
        }

        return skillDef.BaseXPCost + (int)(currentRank * skillDef.BaseXPCost * skillDef.CostMultiplier);
    }

    private void ParseSkillCatalog(JObject catalog)
    {
        var skillTypes = catalog["skill_types"] as JObject;
        if (skillTypes == null)
        {
            _logger.LogError("skill_types not found in catalog");
            return;
        }

        foreach (var skillType in skillTypes.Properties())
        {
            var category = skillType.Name;
            var categoryData = skillType.Value as JObject;
            if (categoryData == null) continue;

            var governingAttribute = categoryData["governingAttribute"]?.ToString() ?? "none";
            var items = categoryData["items"] as JArray;
            if (items == null) continue;

            foreach (var item in items)
            {
                try
                {
                    var skillDef = ParseSkillDefinition(item, category, governingAttribute);
                    if (skillDef != null)
                    {
                        _skillDefinitions[skillDef.SkillId] = skillDef;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse skill: {Item}", item);
                }
            }
        }
    }

    private SkillDefinition? ParseSkillDefinition(JToken item, string category, string defaultGoverningAttribute)
    {
        var slug = item["slug"]?.ToString();
        if (string.IsNullOrEmpty(slug))
        {
            _logger.LogWarning("Skill missing slug, skipping");
            return null;
        }

        var traits = item["traits"] as JObject;
        if (traits == null)
        {
            _logger.LogWarning("Skill {Slug} missing traits, skipping", slug);
            return null;
        }

        var skillDef = new SkillDefinition
        {
            SkillId = slug,
            Name = item["name"]?.ToString() ?? slug,
            DisplayName = item["displayName"]?.ToString() ?? item["name"]?.ToString() ?? slug,
            Description = item["description"]?.ToString() ?? "",
            Category = category,
            BaseXPCost = GetTraitValueInt(traits, "baseXPCost", 100),
            CostMultiplier = GetTraitValueDouble(traits, "costMultiplier", 0.5),
            MaxRank = GetTraitValueInt(traits, "maxRank", 100),
            GoverningAttribute = GetTraitValueString(traits, "governingAttribute", defaultGoverningAttribute)
        };

        // Parse effects
        var effects = GetTraitArray(traits, "effects");
        if (effects != null)
        {
            foreach (var effect in effects)
            {
                var effectType = effect["effectType"]?.ToString();
                var effectValue = effect["effectValue"]?.Value<double>() ?? 0;
                var appliesTo = effect["appliesTo"]?.ToString() ?? "general";

                if (!string.IsNullOrEmpty(effectType))
                {
                    skillDef.Effects.Add(new SkillEffect
                    {
                        EffectType = effectType,
                        EffectValue = effectValue,
                        AppliesTo = appliesTo
                    });
                }
            }
        }

        // Parse XP actions
        var xpActions = GetTraitArray(traits, "xpActions");
        if (xpActions != null)
        {
            foreach (var action in xpActions)
            {
                var actionName = action["action"]?.ToString();
                var xpAmount = action["xpAmount"]?.Value<int>() ?? 0;

                if (!string.IsNullOrEmpty(actionName))
                {
                    skillDef.XPActions[actionName] = xpAmount;
                }
            }
        }

        return skillDef;
    }

    private int GetTraitValueInt(JObject traits, string name, int defaultValue)
    {
        var trait = traits[name] as JObject;
        return trait?["value"]?.Value<int>() ?? defaultValue;
    }

    private double GetTraitValueDouble(JObject traits, string name, double defaultValue)
    {
        var trait = traits[name] as JObject;
        return trait?["value"]?.Value<double>() ?? defaultValue;
    }

    private string GetTraitValueString(JObject traits, string name, string defaultValue)
    {
        var trait = traits[name] as JObject;
        return trait?["value"]?.ToString() ?? defaultValue;
    }

    private JArray? GetTraitArray(JObject traits, string name)
    {
        var trait = traits[name] as JObject;
        return trait?["value"] as JArray;
    }

    private void EnsureInitialized()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("SkillCatalogService not initialized. Call InitializeAsync() first.");
        }
    }
}

/// <summary>
/// Defines a skill's properties from catalog JSON.
/// </summary>
public class SkillDefinition
{
    /// <summary>Gets or sets the unique skill identifier.</summary>
    public required string SkillId { get; set; }
    /// <summary>Gets or sets the internal skill name.</summary>
    public required string Name { get; set; }
    /// <summary>Gets or sets the display name shown to players.</summary>
    public required string DisplayName { get; set; }
    /// <summary>Gets or sets the skill description.</summary>
    public required string Description { get; set; }
    /// <summary>Gets or sets the skill category (combat, magic, stealth, etc.).</summary>
    public required string Category { get; set; }
    /// <summary>Gets or sets the base XP cost for rank 1.</summary>
    public int BaseXPCost { get; set; } = 100;
    /// <summary>Gets or sets the multiplier applied to XP cost per rank.</summary>
    public double CostMultiplier { get; set; } = 0.5;
    /// <summary>Gets or sets the maximum achievable rank.</summary>
    public int MaxRank { get; set; } = 100;
    /// <summary>Gets or sets the governing attribute (strength, dexterity, etc.).</summary>
    public string GoverningAttribute { get; set; } = "none";
    /// <summary>Gets or sets the list of skill effects.</summary>
    public List<SkillEffect> Effects { get; set; } = new();
    /// <summary>Gets or sets the XP awards for specific actions.</summary>
    public Dictionary<string, int> XPActions { get; set; } = new();
}

/// <summary>
/// Represents a skill effect (damage bonus, speed increase, etc.).
/// </summary>
public class SkillEffect
{
    /// <summary>Gets or sets the effect type (damage, speed, etc.).</summary>
    public required string EffectType { get; set; }
    /// <summary>Gets or sets the numeric effect value.</summary>
    public double EffectValue { get; set; }
    /// <summary>Gets or sets what the effect applies to (general, weapon, spell, etc.).</summary>
    public string AppliesTo { get; set; } = "general";
}
