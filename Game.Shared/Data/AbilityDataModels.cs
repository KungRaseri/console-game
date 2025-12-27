using Game.Shared.Models;
using System.Text.Json.Serialization;

namespace Game.Shared.Data.Models;

/// <summary>
/// Data models for ability v4.0 JSON structure.
/// Abilities use a dual-file system:
/// - abilities_names.json: Pattern-based generation with components
/// - abilities_catalog.json: Base ability definitions with traits
/// </summary>

#region Abilities Names Data (Pattern Generation)

/// <summary>
/// Root model for abilities_names.json - pattern-based ability name generation
/// </summary>
public class AbilityNamesData
{
    [JsonPropertyName("metadata")]
    public AbilityNamesMetadata Metadata { get; set; } = new();

    [JsonPropertyName("components")]
    public Dictionary<string, List<AbilityComponent>> Components { get; set; } = new();

    [JsonPropertyName("patterns")]
    public List<AbilityPattern> Patterns { get; set; } = new();
}

/// <summary>
/// Metadata for abilities_names.json
/// </summary>
public class AbilityNamesMetadata
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = "4.0";

    [JsonPropertyName("lastUpdated")]
    public string LastUpdated { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "pattern_generation";

    [JsonPropertyName("supportsTraits")]
    public bool SupportsTraits { get; set; } = true;

    [JsonPropertyName("componentKeys")]
    public List<string> ComponentKeys { get; set; } = new() { "prefix", "base", "suffix" };

    [JsonPropertyName("patternTokens")]
    public List<string> PatternTokens { get; set; } = new() { "base", "prefix", "suffix" };

    [JsonPropertyName("totalPatterns")]
    public int TotalPatterns { get; set; }

    [JsonPropertyName("raritySystem")]
    public string RaritySystem { get; set; } = "weight-based";

    [JsonPropertyName("notes")]
    public List<string> Notes { get; set; } = new();
}

/// <summary>
/// Component for procedural ability name generation (prefix, base, or suffix)
/// </summary>
public class AbilityComponent
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; } = 10;

    [JsonPropertyName("traits")]
    public Dictionary<string, JsonTraitValue>? JsonTraits { get; set; }

    /// <summary>
    /// Convert JSON traits to TraitValue dictionary.
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, TraitValue> Traits
    {
        get
        {
            if (JsonTraits == null) return new();
            var result = new Dictionary<string, TraitValue>();
            foreach (var kvp in JsonTraits)
            {
                result[kvp.Key] = kvp.Value.ToTraitValue();
            }
            return result;
        }
    }
}

/// <summary>
/// Pattern template for combining components into ability names
/// </summary>
public class AbilityPattern
{
    [JsonPropertyName("pattern")]
    public string Pattern { get; set; } = string.Empty;

    [JsonPropertyName("weight")]
    public int Weight { get; set; } = 10;

    [JsonPropertyName("example")]
    public string? Example { get; set; }
}

#endregion

#region Abilities Catalog Data (Base Definitions)

/// <summary>
/// Root model for abilities_catalog.json - base ability definitions
/// </summary>
public class AbilityCatalogData
{
    [JsonPropertyName("metadata")]
    public AbilityCatalogMetadata Metadata { get; set; } = new();

    [JsonPropertyName("ability_types")]
    public Dictionary<string, AbilityType> AbilityTypes { get; set; } = new();
}

/// <summary>
/// Metadata for abilities_catalog.json
/// </summary>
public class AbilityCatalogMetadata
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = "4.0";

    [JsonPropertyName("lastUpdated")]
    public string LastUpdated { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "ability_catalog";

    [JsonPropertyName("supportsTraits")]
    public bool SupportsTraits { get; set; } = true;

    [JsonPropertyName("totalAbilityTypes")]
    public int TotalAbilityTypes { get; set; }

    [JsonPropertyName("totalAbilities")]
    public int TotalAbilities { get; set; }

    [JsonPropertyName("usage")]
    public string? Usage { get; set; }
}

/// <summary>
/// Ability type grouping (offensive, defensive, control, utility, legendary)
/// Contains type-level traits that apply to all abilities of this type
/// </summary>
public class AbilityType
{
    [JsonPropertyName("traits")]
    public Dictionary<string, JsonTraitValue>? TypeJsonTraits { get; set; }

    [JsonPropertyName("items")]
    public List<AbilityItem> Items { get; set; } = new();

    /// <summary>
    /// Type-level traits that cascade to all items
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, TraitValue> TypeTraits
    {
        get
        {
            if (TypeJsonTraits == null) return new();
            var result = new Dictionary<string, TraitValue>();
            foreach (var kvp in TypeJsonTraits)
            {
                result[kvp.Key] = kvp.Value.ToTraitValue();
            }
            return result;
        }
    }
}

/// <summary>
/// Individual ability definition with stats and traits
/// </summary>
public class AbilityItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; } = 10;

    // Optional base stats (not all abilities will have these)
    [JsonPropertyName("baseDamage")]
    public string? BaseDamage { get; set; }

    [JsonPropertyName("cooldown")]
    public int? Cooldown { get; set; }

    [JsonPropertyName("range")]
    public int? Range { get; set; }

    [JsonPropertyName("traits")]
    public Dictionary<string, JsonTraitValue>? JsonTraits { get; set; }

    /// <summary>
    /// Ability-specific traits (combined with type-level traits at runtime)
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, TraitValue> Traits
    {
        get
        {
            if (JsonTraits == null) return new();
            var result = new Dictionary<string, TraitValue>();
            foreach (var kvp in JsonTraits)
            {
                result[kvp.Key] = kvp.Value.ToTraitValue();
            }
            return result;
        }
    }
}

#endregion
