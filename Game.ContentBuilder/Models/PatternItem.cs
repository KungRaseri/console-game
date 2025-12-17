using CommunityToolkit.Mvvm.ComponentModel;
using Game.ContentBuilder.Services;

namespace Game.ContentBuilder.Models;

/// <summary>
/// Represents a pattern with auto-generated example and validation
/// </summary>
public partial class PatternItem : ObservableObject
{
    [ObservableProperty]
    private string _pattern = string.Empty;

    [ObservableProperty]
    private string _example = string.Empty;

    [ObservableProperty]
    private ValidationResult? _validationResult;

    /// <summary>
    /// Material Design icon for validation status
    /// </summary>
    public string ValidationIcon => ValidationResult != null 
        ? PatternValidator.GetValidationIcon(ValidationResult.Level)
        : "HelpCircle";

    /// <summary>
    /// Color name for validation status
    /// </summary>
    public string ValidationColor => ValidationResult != null
        ? PatternValidator.GetValidationColor(ValidationResult.Level)
        : "Gray";

    public PatternItem(string pattern, string example)
    {
        Pattern = pattern;
        Example = example;
    }

    public PatternItem(string pattern, string example, ValidationResult? validationResult)
    {
        Pattern = pattern;
        Example = example;
        ValidationResult = validationResult;
    }
}
