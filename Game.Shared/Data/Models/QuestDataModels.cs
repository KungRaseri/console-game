using Game.Shared.Models;
using System.Text.Json.Serialization;

namespace Game.Shared.Data.Models;

/// <summary>
/// Data models for quest templates from JSON.
/// </summary>

// Quest template with traits
public class QuestTemplateTraitData
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

// Quest difficulty tier structure
public class QuestDifficultyTierData
{
    public Dictionary<string, QuestTemplateTraitData> Easy { get; set; } = new();
    public Dictionary<string, QuestTemplateTraitData> Medium { get; set; } = new();
    public Dictionary<string, QuestTemplateTraitData> Hard { get; set; } = new();
}

// Quest templates organized by type
public class QuestTemplatesData
{
    public QuestDifficultyTierData Kill { get; set; } = new();
    public QuestDifficultyTierData Fetch { get; set; } = new();
    public QuestDifficultyTierData Escort { get; set; } = new();
    public QuestDifficultyTierData Investigate { get; set; } = new();
    public QuestDifficultyTierData Delivery { get; set; } = new();
}
