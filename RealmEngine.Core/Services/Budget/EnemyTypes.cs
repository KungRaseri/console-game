using Newtonsoft.Json;

namespace RealmEngine.Core.Services.Budget;

/// <summary>
/// Enemy type configuration loaded from enemies/enemy-types.json.
/// Defines default material pools and budget modifiers per enemy type.
/// </summary>
public class EnemyTypes
{
    [JsonProperty("metadata")]
    public EnemyTypeMetadata? Metadata { get; set; }

    [JsonProperty("types")]
    public Dictionary<string, EnemyTypeConfig> Types { get; set; } = new();
}

public class EnemyTypeMetadata
{
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("version")]
    public string Version { get; set; } = string.Empty;

    [JsonProperty("lastUpdated")]
    public string LastUpdated { get; set; } = string.Empty;

    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;
}

public class EnemyTypeConfig
{
    [JsonProperty("materialPool")]
    public string MaterialPool { get; set; } = "default";

    [JsonProperty("budgetMultiplier")]
    public double BudgetMultiplier { get; set; } = 1.0;

    [JsonProperty("materialPercentage")]
    public double? MaterialPercentage { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;
}
