using Newtonsoft.Json;

namespace RealmEngine.Core.Services.Budget;

/// <summary>
/// Material pool configuration loaded from general/material-pools.json.
/// Defines which materials can drop from which enemy types.
/// </summary>
public class MaterialPools
{
    /// <summary>Gets or sets the material pools metadata.</summary>
    [JsonProperty("metadata")]
    public MaterialPoolMetadata? Metadata { get; set; }

    /// <summary>Gets or sets the dictionary of material pools by pool name.</summary>
    [JsonProperty("pools")]
    public Dictionary<string, MaterialPool> Pools { get; set; } = new();
}

/// <summary>
/// Metadata for material pools configuration.
/// </summary>
public class MaterialPoolMetadata
{
    /// <summary>Gets or sets the configuration description.</summary>
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the configuration version.</summary>
    [JsonProperty("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>Gets or sets the last updated timestamp.</summary>
    [JsonProperty("lastUpdated")]
    public string LastUpdated { get; set; } = string.Empty;

    /// <summary>Gets or sets the configuration type.</summary>
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Defines a pool of materials that can be selected from.
/// </summary>
public class MaterialPool
{
    /// <summary>Gets or sets the pool description.</summary>
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the list of metal material references.</summary>
    [JsonProperty("metals")]
    public List<MaterialReference> Metals { get; set; } = new();
}

/// <summary>
/// Reference to a material with a selection weight.
/// </summary>
public class MaterialReference
{
    /// <summary>Gets or sets the reference to the material catalog item.</summary>
    [JsonProperty("materialRef")]
    public string MaterialRef { get; set; } = string.Empty;

    /// <summary>Gets or sets the selection weight for random selection.</summary>
    [JsonProperty("selectionWeight")]
    public int SelectionWeight { get; set; }
}
