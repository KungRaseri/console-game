using Game.Models;
using System.Text.Json.Serialization;

namespace Game.Data.Models;

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

// Enhanced enemy prefix data structure
public class EnemyPrefixDataEnhanced
{
    public Dictionary<string, EnemyPrefixTraitData> Common { get; set; } = new();
    public Dictionary<string, EnemyPrefixTraitData> Uncommon { get; set; } = new();
    public Dictionary<string, EnemyPrefixTraitData> Rare { get; set; } = new();
    public Dictionary<string, EnemyPrefixTraitData> Elite { get; set; } = new();
    public Dictionary<string, EnemyPrefixTraitData> Boss { get; set; } = new();
}

// Enhanced occupation data structure
public class OccupationDataEnhanced
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
