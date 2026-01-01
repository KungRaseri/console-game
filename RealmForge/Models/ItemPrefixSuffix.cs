using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RealmForge.Models;

/// <summary>
/// Represents a trait on an item (e.g., damageBonus, durability)
/// </summary>
public partial class ItemTrait : ObservableObject
{
    [ObservableProperty]
    private string _key = string.Empty;

    [ObservableProperty]
    private object _value = 0;

    [ObservableProperty]
    private string _type = "number";

    public ItemTrait() { }

    public ItemTrait(string key, object value, string type)
    {
        Key = key;
        Value = value;
        Type = type;
    }
}

/// <summary>
/// Represents an item prefix or suffix
/// </summary>
public partial class ItemPrefixSuffix : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _rarity = "common";

    [ObservableProperty]
    private ObservableCollection<ItemTrait> _traits = new();

    public ItemPrefixSuffix() { }

    public ItemPrefixSuffix(string name, string displayName, string rarity)
    {
        Name = name;
        DisplayName = displayName;
        Rarity = rarity;
    }
}

/// <summary>
/// Root structure for weapon_prefixes.json and similar files
/// </summary>
public class ItemPrefixSuffixData
{
    public Dictionary<string, Dictionary<string, ItemPrefixRaw>>? Common { get; set; }
    public Dictionary<string, Dictionary<string, ItemPrefixRaw>>? Uncommon { get; set; }
    public Dictionary<string, Dictionary<string, ItemPrefixRaw>>? Rare { get; set; }
    public Dictionary<string, Dictionary<string, ItemPrefixRaw>>? Epic { get; set; }
    public Dictionary<string, Dictionary<string, ItemPrefixRaw>>? Legendary { get; set; }
}

/// <summary>
/// Raw JSON structure for a single prefix/suffix entry
/// </summary>
public class ItemPrefixRaw
{
    public string? DisplayName { get; set; }
    public Dictionary<string, TraitValue>? Traits { get; set; }
}

/// <summary>
/// Raw JSON structure for a trait value
/// </summary>
public class TraitValue
{
    public object? Value { get; set; }
    public string? Type { get; set; }
}
