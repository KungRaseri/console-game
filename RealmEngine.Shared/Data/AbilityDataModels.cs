using RealmEngine.Shared.Models;
using System.Text.Json.Serialization;

namespace RealmEngine.Shared.Data.Models;

// Data models for ability v4.0 JSON structure.
// Abilities use a dual-file system:
// - abilities_names.json: Pattern-based generation with components
// - abilities_catalog.json: Base ability definitions with traits

#region Abilities Names Data (Pattern Generation)

/// <summary>
/// Root model for abilities_names.json - pattern-based ability name generation
/// </summary>
public class AbilityNamesData
{
    /// <summary>Gets or sets the metadata.</summary>
    [JsonPropertyName("metadata")]
    public AbilityNamesMetadata Metadata { get; set; } = new();

    /// <summary>Gets or sets the components dictionary.</summary>
    [JsonPropertyName("components")]
    public Dictionary<string, List<AbilityComponent>> Components { get; set; } = new();

    /// <summary>Gets or sets the patterns.</summary>
    [JsonPropertyName("patterns")]
    public List<AbilityPattern> Patterns { get; set; } = new();
}

/// <summary>
/// Metadata for abilities_names.json
/// </summary>
public class AbilityNamesMetadata
{
    /// <summary>Gets or sets the description.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the version.</summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = "4.0";

    /// <summary>Gets or sets the last updated date.</summary>
    [JsonPropertyName("lastUpdated")]
    public string LastUpdated { get; set; } = string.Empty;

    /// <summary>Gets or sets the type.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "pattern_generation";

    /// <summary>Gets or sets a value indicating whether traits are supported.</summary>
    [JsonPropertyName("supportsTraits")]
    public bool SupportsTraits { get; set; } = true;

    /// <summary>Gets or sets the component keys.</summary>
    [JsonPropertyName("componentKeys")]
    public List<string> ComponentKeys { get; set; } = new() { "prefix", "base", "suffix" };

    /// <summary>Gets or sets the pattern tokens.</summary>
    [JsonPropertyName("patternTokens")]
    public List<string> PatternTokens { get; set; } = new() { "base", "prefix", "suffix" };

    /// <summary>Gets or sets the total patterns count.</summary>
    [JsonPropertyName("totalPatterns")]
    public int TotalPatterns { get; set; }

    /// <summary>Gets or sets the rarity system.</summary>
    [JsonPropertyName("raritySystem")]
    public string RaritySystem { get; set; } = "weight-based";

    /// <summary>Gets or sets the notes.</summary>
    [JsonPropertyName("notes")]
    public List<string> Notes { get; set; } = new();
}

/// <summary>
/// Component for procedural ability name generation (prefix, base, or suffix)
/// </summary>
public class AbilityComponent
{
    /// <summary>Gets or sets the value.</summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    /// <summary>Gets or sets the rarity weight.</summary>
    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; } = 10;

    /// <summary>Gets or sets the traits in JSON format.</summary>
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
    /// <summary>Gets or sets the pattern.</summary>
    [JsonPropertyName("pattern")]
    public string Pattern { get; set; } = string.Empty;

    /// <summary>Gets or sets the weight.</summary>
    [JsonPropertyName("weight")]
    public int Weight { get; set; } = 10;

    /// <summary>Gets or sets the example.</summary>
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
    /// <summary>Gets or sets the metadata.</summary>
    [JsonPropertyName("metadata")]
    public AbilityCatalogMetadata Metadata { get; set; } = new();

    /// <summary>Gets or sets the ability types dictionary.</summary>
    [JsonPropertyName("ability_types")]
    public Dictionary<string, AbilityType> AbilityTypes { get; set; } = new();
}

/// <summary>
/// Metadata for abilities_catalog.json
/// </summary>
public class AbilityCatalogMetadata
{
    /// <summary>Gets or sets the description.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the version.</summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = "4.0";

    /// <summary>Gets or sets the last updated date.</summary>
    [JsonPropertyName("lastUpdated")]
    public string LastUpdated { get; set; } = string.Empty;

    /// <summary>Gets or sets the type.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "ability_catalog";

    /// <summary>Gets or sets a value indicating whether traits are supported.</summary>
    [JsonPropertyName("supportsTraits")]
    public bool SupportsTraits { get; set; } = true;

    /// <summary>Gets or sets the total ability types count.</summary>
    [JsonPropertyName("totalAbilityTypes")]
    public int TotalAbilityTypes { get; set; }

    /// <summary>Gets or sets the total abilities count.</summary>
    [JsonPropertyName("totalAbilities")]
    public int TotalAbilities { get; set; }

    /// <summary>Gets or sets the usage.</summary>
    [JsonPropertyName("usage")]
    public string? Usage { get; set; }
}

/// <summary>
/// Ability type grouping (offensive, defensive, control, utility, legendary)
/// Contains type-level traits that apply to all abilities of this type
/// </summary>
public class AbilityType
{
    /// <summary>Gets or sets the type-level traits in JSON format.</summary>
    [JsonPropertyName("traits")]
    public Dictionary<string, JsonTraitValue>? TypeJsonTraits { get; set; }

    /// <summary>Gets or sets the items.</summary>
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
    /// <summary>Gets or sets the name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the rarity weight.</summary>
    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; } = 10;

    // Optional base stats (not all abilities will have these)
    /// <summary>Gets or sets the base damage.</summary>
    [JsonPropertyName("baseDamage")]
    public string? BaseDamage { get; set; }

    /// <summary>Gets or sets the cooldown.</summary>
    [JsonPropertyName("cooldown")]
    public int? Cooldown { get; set; }

    /// <summary>Gets or sets the range.</summary>
    [JsonPropertyName("range")]
    public int? Range { get; set; }

    /// <summary>Gets or sets the traits in JSON format.</summary>
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
