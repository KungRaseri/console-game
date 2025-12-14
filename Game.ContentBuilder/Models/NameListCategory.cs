using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Game.ContentBuilder.Models;

/// <summary>
/// Represents a category of names with an editable list
/// Used for weapon_names.json structure: { "category": ["name1", "name2", ...] }
/// </summary>
public partial class NameListCategory : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _names = new();

    public NameListCategory(string name)
    {
        Name = name;
    }

    public NameListCategory(string name, IEnumerable<string> names) : this(name)
    {
        Names = new ObservableCollection<string>(names);
    }
}
