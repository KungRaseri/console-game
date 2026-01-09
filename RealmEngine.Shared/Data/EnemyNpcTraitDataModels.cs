using RealmEngine.Shared.Models;
using System.Text.Json.Serialization;

namespace RealmEngine.Shared.Data.Models;

/// <summary>
/// Enhanced data models for enemy traits from JSON.
/// </summary>

/// <summary>
/// Enemy prefix with traits (Dire, Ancient, Elder, etc.).
/// </summary>
public class EnemyPrefixTraitData
{
    /// <summary>Gets or sets the display name.</summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the traits in JSON format.</summary>
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

/// <summary>
/// NPC occupation with traits (Blacksmith, Merchant, etc.).
/// </summary>
public class OccupationTraitData
{
    /// <summary>Gets or sets the display name.</summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the traits in JSON format.</summary>
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

/// <summary>
/// Enemy prefix data structure by rarity tier.
/// </summary>
public class EnemyPrefixData
{
    /// <summary>Gets or sets the common prefixes.</summary>
    public Dictionary<string, EnemyPrefixTraitData> Common { get; set; } = new();
    
    /// <summary>Gets or sets the uncommon prefixes.</summary>
    public Dictionary<string, EnemyPrefixTraitData> Uncommon { get; set; } = new();
    
    /// <summary>Gets or sets the rare prefixes.</summary>
    public Dictionary<string, EnemyPrefixTraitData> Rare { get; set; } = new();
    
    /// <summary>Gets or sets the elite prefixes.</summary>
    public Dictionary<string, EnemyPrefixTraitData> Elite { get; set; } = new();
    
    /// <summary>Gets or sets the boss prefixes.</summary>
    public Dictionary<string, EnemyPrefixTraitData> Boss { get; set; } = new();
}

/// <summary>
/// Occupation data structure by category.
/// </summary>
public class OccupationData
{
    /// <summary>Gets or sets the merchant occupations.</summary>
    public Dictionary<string, OccupationTraitData> Merchants { get; set; } = new();
    
    /// <summary>Gets or sets the craftsmen occupations.</summary>
    public Dictionary<string, OccupationTraitData> Craftsmen { get; set; } = new();
    
    /// <summary>Gets or sets the professional occupations.</summary>
    public Dictionary<string, OccupationTraitData> Professionals { get; set; } = new();
    
    /// <summary>Gets or sets the service occupations.</summary>
    public Dictionary<string, OccupationTraitData> Service { get; set; } = new();
    
    /// <summary>Gets or sets the nobility occupations.</summary>
    public Dictionary<string, OccupationTraitData> Nobility { get; set; } = new();
    
    /// <summary>Gets or sets the religious occupations.</summary>
    public Dictionary<string, OccupationTraitData> Religious { get; set; } = new();
    
    /// <summary>Gets or sets the adventurer occupations.</summary>
    public Dictionary<string, OccupationTraitData> Adventurers { get; set; } = new();
    
    /// <summary>Gets or sets the magical occupations.</summary>
    public Dictionary<string, OccupationTraitData> Magical { get; set; } = new();
    
    /// <summary>Gets or sets the criminal occupations.</summary>
    public Dictionary<string, OccupationTraitData> Criminal { get; set; } = new();
    
    /// <summary>Gets or sets the common occupations.</summary>
    public Dictionary<string, OccupationTraitData> Common { get; set; } = new();
}

/// <summary>
/// Dragon color trait data.
/// </summary>
public class DragonColorTraitData
{
    /// <summary>Gets or sets the display name.</summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the traits in JSON format.</summary>
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

/// <summary>
/// Dragon color data - flat dictionary (no tiers, just 13 colors).
/// </summary>
public class DragonColorData : Dictionary<string, DragonColorTraitData>
{
}

/// <summary>
/// Dialogue personality trait data.
/// </summary>
public class DialogueTraitData
{
    /// <summary>Gets or sets the display name.</summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the traits in JSON format.</summary>
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

/// <summary>
/// Dialogue trait data - flat dictionary of personality archetypes.
/// </summary>
public class DialogueTraitsData : Dictionary<string, DialogueTraitData>
{
}
