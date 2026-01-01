using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RealmForge.Models;

/// <summary>
/// Model for a component group (e.g., "material", "quality", "descriptive")
/// Used in pattern generation JSON files
/// </summary>
public partial class ComponentGroup : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> components = new();
}
