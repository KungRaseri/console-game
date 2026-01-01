using CommunityToolkit.Mvvm.ComponentModel;

namespace RealmForge.Models;

/// <summary>
/// Represents a single token in a pattern (component, reference, or plain text)
/// </summary>
public partial class PatternToken : ObservableObject
{
    [ObservableProperty]
    private string _value = string.Empty;

    [ObservableProperty]
    private PatternTokenType _type;

    /// <summary>
    /// Whether this is a base token (cannot be deleted)
    /// </summary>
    public bool IsBase => Type == PatternTokenType.Component && Value.Equals("base", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Display text for the badge
    /// </summary>
    public string DisplayText => Type switch
    {
        PatternTokenType.Component => $"{{{Value}}}",
        PatternTokenType.Reference => Value,
        PatternTokenType.PlainText => Value,
        _ => Value
    };

    /// <summary>
    /// Color for the badge based on type
    /// </summary>
    public string BadgeColor => Type switch
    {
        PatternTokenType.Component => "#4CAF50",  // Green
        PatternTokenType.Reference => "#2196F3",  // Blue
        PatternTokenType.PlainText => "#9E9E9E",  // Gray
        _ => "#9E9E9E"
    };
}

public enum PatternTokenType
{
    Component,   // {base}, {prefix}, {suffix}
    Reference,   // @materialRef/weapon
    PlainText    // Plain text (future: for literal words between tokens)
}
