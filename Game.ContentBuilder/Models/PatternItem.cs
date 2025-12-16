using CommunityToolkit.Mvvm.ComponentModel;

namespace Game.ContentBuilder.Models;

/// <summary>
/// Represents a pattern with auto-generated example
/// </summary>
public partial class PatternItem : ObservableObject
{
    [ObservableProperty]
    private string _pattern = string.Empty;

    [ObservableProperty]
    private string _example = string.Empty;

    public PatternItem(string pattern, string example)
    {
        Pattern = pattern;
        Example = example;
    }
}
