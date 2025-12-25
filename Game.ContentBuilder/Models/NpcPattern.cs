using System.Collections.ObjectModel;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Game.ContentBuilder.Models;

/// <summary>
/// Base class for all name patterns with common properties
/// </summary>
[JsonConverter(typeof(NamePatternConverter))]
public abstract partial class NamePatternBase : ObservableObject
{
    /// <summary>
    /// The pattern template (e.g., "{title} {first_name}" or "prefix + material + base")
    /// Can be named "template" or "pattern" depending on the file
    /// </summary>
    [ObservableProperty]
    private string _patternTemplate = string.Empty;

    /// <summary>
    /// Rarity weight or selection weight (can be "rarityWeight" or "weight" in JSON)
    /// </summary>
    [ObservableProperty]
    private int _weight;

    /// <summary>
    /// Description or example of the pattern output
    /// </summary>
    [ObservableProperty]
    private string? _description;

    /// <summary>
    /// Generated example names (not persisted, computed at runtime)
    /// </summary>
    [ObservableProperty]
    [JsonIgnore]
    private string _generatedExamples = string.Empty;

    /// <summary>
    /// Whether this pattern is readonly (cannot be edited or deleted)
    /// Used for default patterns like {base}
    /// </summary>
    [ObservableProperty]
    [JsonIgnore]
    private bool _isReadOnly = false;

    /// <summary>
    /// Token-based representation of the pattern (for badge UI)
    /// Not persisted - computed from PatternTemplate on load
    /// </summary>
    [ObservableProperty]
    [JsonIgnore]
    private ObservableCollection<PatternToken> _tokens = new();

    /// <summary>
    /// Weight percentage relative to total (for visualization)
    /// Not persisted - computed at runtime
    /// </summary>
    [ObservableProperty]
    [JsonIgnore]
    private double _weightPercentage;

    /// <summary>
    /// Fallback for truly unknown properties
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JToken>? AdditionalProperties { get; set; }
}

/// <summary>
/// NPC name pattern with social class requirements and title flags
/// </summary>
public partial class NpcNamePattern : NamePatternBase
{
    [ObservableProperty]
    [JsonProperty("socialClass")]
    private ObservableCollection<string>? _socialClass;

    [ObservableProperty]
    [JsonProperty("excludeTitles")]
    private bool? _excludeTitles;

    [ObservableProperty]
    [JsonProperty("requiresTitle")]
    private bool? _requiresTitle;
}

/// <summary>
/// Item name pattern (uses only base properties)
/// </summary>
public partial class ItemNamePattern : NamePatternBase
{
    // Item patterns only use base properties (pattern, weight, example)
}
