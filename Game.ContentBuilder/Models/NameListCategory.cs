
using System.Collections.ObjectModel;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Game.ContentBuilder.Models;

/// <summary>
/// Represents a single name component (e.g., title, first_name, surname, suffix) in v4 names.json
/// </summary>
public partial class NameComponent : ObservableObject
{
    [ObservableProperty]
    private string _value = string.Empty;

    [ObservableProperty]
    private int _rarityWeight;

    [ObservableProperty]
    private string? _gender;

    [ObservableProperty]
    private string? _preferredSocialClass;

    [ObservableProperty]
    private Dictionary<string, double>? _weightMultiplier;
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
}
