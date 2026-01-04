using Newtonsoft.Json;

namespace RealmEngine.Core.Services.Budget;

/// <summary>
/// Material pool configuration loaded from general/material-pools.json.
/// Defines which materials can drop from which enemy types.
/// </summary>
public class MaterialPools
{
    [JsonProperty("metadata")]
    public MaterialPoolMetadata? Metadata { get; set; }

    [JsonProperty("pools")]
    public Dictionary<string, MaterialPool> Pools { get; set; } = new();
}

public class MaterialPoolMetadata
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

public class MaterialPool
{
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("metals")]
    public List<MaterialReference> Metals { get; set; } = new();
}

public class MaterialReference
{
    [JsonProperty("materialRef")]
    public string MaterialRef { get; set; } = string.Empty;

    [JsonProperty("selectionWeight")]
    public int SelectionWeight { get; set; }
}
