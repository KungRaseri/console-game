
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Game.ContentBuilder.Models;

/// <summary>
/// Base class for all name components with common properties
/// </summary>
[JsonConverter(typeof(NameComponentConverter))]
public abstract partial class NameComponentBase : ObservableObject
{
    /// <summary>
    /// The display value of the component (e.g., "Sir", "Rusty", "Dragon")
    /// </summary>
    [ObservableProperty]
    [JsonProperty("value")]
    private string _value = string.Empty;

    /// <summary>
    /// Rarity weight for random selection (common: 50-100, uncommon: 20-49, rare: 10-19, epic: 5-9, legendary: 1-4)
    /// </summary>
    [ObservableProperty]
    [JsonProperty("rarityWeight")]
    private int _rarityWeight;

    /// <summary>
    /// Fallback for truly unknown properties
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JToken>? AdditionalProperties { get; set; }
}

/// <summary>
/// NPC name component with gender, social class, and weight multipliers
/// </summary>
public partial class NpcNameComponent : NameComponentBase
{
    [ObservableProperty]
    [JsonProperty("gender")]
    private string? _gender;

    [ObservableProperty]
    [JsonProperty("preferredSocialClass")]
    private string? _preferredSocialClass;

    [ObservableProperty]
    [JsonProperty("weightMultiplier")]
    private Dictionary<string, double>? _weightMultiplier;
}

/// <summary>
/// Item name component with trait system
/// </summary>
public partial class ItemNameComponent : NameComponentBase
{
    [ObservableProperty]
    [JsonIgnore]
    private ObservableCollection<ComponentTrait> _traits = new();

    [JsonProperty("traits")]
    public Dictionary<string, JObject>? TraitsJson { get; set; }
}

/// <summary>
/// Represents the v4 metadata block for names.json
/// </summary>
public partial class NameListMetadata : ObservableObject
{
    [ObservableProperty]
    private string _type = string.Empty;

    [ObservableProperty]
    private string _version = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _lastModified = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _componentKeys = new();

    [ObservableProperty]
    private ObservableCollection<string> _patternTokens = new();

    [ObservableProperty]
    private string _raritySystem = string.Empty;

    [ObservableProperty]
    private bool _supportsSoftFiltering;

    [ObservableProperty]
    private ObservableCollection<string> _notes = new();

    /// <summary>
    /// Text representation of notes for TextBox binding
    /// </summary>
    public string NotesText
    {
        get => string.Join(Environment.NewLine, Notes);
        set
        {
            Notes.Clear();
            if (!string.IsNullOrWhiteSpace(value))
            {
                var lines = value.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    Notes.Add(line.Trim());
                }
            }
            OnPropertyChanged(nameof(NotesText));
        }
    }
}

/// <summary>
/// Represents a component trait with name, value, and type
/// </summary>
public partial class ComponentTrait : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _value = string.Empty;

    [ObservableProperty]
    private string _type = "number"; // number, boolean, string
}
