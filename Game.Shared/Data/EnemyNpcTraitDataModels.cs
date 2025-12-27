using Game.Shared.Models;
using System.Text.Json.Serialization;

namespace Game.Shared.Data.Models;

/// <summary>
/// Enhanced data models for enemy traits from JSON.
/// </summary>

// Enemy prefix with traits (Dire, Ancient, Elder, etc.)
public class EnemyPrefixTraitData
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("traits")]
    public Dictionary<string, JsonTraitValue> JsonTraits { get; set; } = new();

    /// <summary>
    /// Convert JSON traits to TraitValue dictionary.
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, TraitValue> Traits
    {
        get
        {
            var result = new Dictionary<string, TraitValue>();
            foreach (var kvp in JsonTraits)
            {
                result[kvp.Key] = kvp.Value.ToTraitValue();
            }
            return result;
        }
    }
}

// NPC occupation with traits (Blacksmith, Merchant, etc.)
public class OccupationTraitData
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("traits")]
    public Dictionary<string, JsonTraitValue> JsonTraits { get; set; } = new();

    /// <summary>
    /// Convert JSON traits to TraitValue dictionary.
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, TraitValue> Traits
    {
        get
        {
            var result = new Dictionary<string, TraitValue>();
            foreach (var kvp in JsonTraits)
            {
                result[kvp.Key] = kvp.Value.ToTraitValue();
            }
            return result;
        }
    }
}

// Enemy prefix data structure
public class EnemyPrefixData
{
    public Dictionary<string, EnemyPrefixTraitData> Common { get; set; } = new();
    public Dictionary<string, EnemyPrefixTraitData> Uncommon { get; set; } = new();
    public Dictionary<string, EnemyPrefixTraitData> Rare { get; set; } = new();
    public Dictionary<string, EnemyPrefixTraitData> Elite { get; set; } = new();
    public Dictionary<string, EnemyPrefixTraitData> Boss { get; set; } = new();
}

// Occupation data structure
public class OccupationData
{
    public Dictionary<string, OccupationTraitData> Merchants { get; set; } = new();
    public Dictionary<string, OccupationTraitData> Craftsmen { get; set; } = new();
    public Dictionary<string, OccupationTraitData> Professionals { get; set; } = new();
    public Dictionary<string, OccupationTraitData> Service { get; set; } = new();
    public Dictionary<string, OccupationTraitData> Nobility { get; set; } = new();
    public Dictionary<string, OccupationTraitData> Religious { get; set; } = new();
    public Dictionary<string, OccupationTraitData> Adventurers { get; set; } = new();
    public Dictionary<string, OccupationTraitData> Magical { get; set; } = new();
    public Dictionary<string, OccupationTraitData> Criminal { get; set; } = new();
    public Dictionary<string, OccupationTraitData> Common { get; set; } = new();
}

// Dragon color trait data
public class DragonColorTraitData
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("traits")]
    public Dictionary<string, JsonTraitValue> JsonTraits { get; set; } = new();

    /// <summary>
    /// Convert JSON traits to TraitValue dictionary.
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, TraitValue> Traits
    {
        get
        {
            var result = new Dictionary<string, TraitValue>();
            foreach (var kvp in JsonTraits)
            {
                result[kvp.Key] = kvp.Value.ToTraitValue();
            }
            return result;
        }
    }
}

// Dragon color data - flat dictionary (no tiers, just 13 colors)
public class DragonColorData : Dictionary<string, DragonColorTraitData>
{
}

// Dialogue personality trait data
public class DialogueTraitData
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("traits")]
    public Dictionary<string, JsonTraitValue> JsonTraits { get; set; } = new();

    /// <summary>
    /// Convert JSON traits to TraitValue dictionary.
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, TraitValue> Traits
    {
        get
        {
            var result = new Dictionary<string, TraitValue>();
            foreach (var kvp in JsonTraits)
            {
                result[kvp.Key] = kvp.Value.ToTraitValue();
            }
            return result;
        }
    }
}

// Dialogue trait data - flat dictionary of personality archetypes
public class DialogueTraitsData : Dictionary<string, DialogueTraitData>
{
}
