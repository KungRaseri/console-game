using RealmEngine.Shared.Models;
using System.Text.Json.Serialization;

namespace RealmEngine.Shared.Data.Models;

/// <summary>
/// Data models for quest templates from JSON.
/// </summary>

/// <summary>
/// Quest template with traits.
/// </summary>
public class QuestTemplateTraitData
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
/// Quest difficulty tier structure.
/// </summary>
public class QuestDifficultyTierData
{
    /// <summary>Gets or sets the easy quest templates.</summary>
    public Dictionary<string, QuestTemplateTraitData> Easy { get; set; } = new();
    
    /// <summary>Gets or sets the medium quest templates.</summary>
    public Dictionary<string, QuestTemplateTraitData> Medium { get; set; } = new();
    
    /// <summary>Gets or sets the hard quest templates.</summary>
    public Dictionary<string, QuestTemplateTraitData> Hard { get; set; } = new();
}

/// <summary>
/// Quest templates organized by type.
/// </summary>
public class QuestTemplatesData
{
    /// <summary>Gets or sets the kill quest templates.</summary>
    public QuestDifficultyTierData Kill { get; set; } = new();
    
    /// <summary>Gets or sets the fetch quest templates.</summary>
    public QuestDifficultyTierData Fetch { get; set; } = new();
    
    /// <summary>Gets or sets the escort quest templates.</summary>
    public QuestDifficultyTierData Escort { get; set; } = new();
    
    /// <summary>Gets or sets the investigate quest templates.</summary>
    public QuestDifficultyTierData Investigate { get; set; } = new();
    
    /// <summary>Gets or sets the delivery quest templates.</summary>
    public QuestDifficultyTierData Delivery { get; set; } = new();
}
