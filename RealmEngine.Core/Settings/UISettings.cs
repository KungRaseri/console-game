namespace RealmEngine.Core.Settings;

/// <summary>
/// User interface configuration settings
/// </summary>
public class UISettings
{
    /// <summary>Gets or sets the color scheme name.</summary>
    public string ColorScheme { get; set; } = "Default";
    /// <summary>Gets or sets a value indicating whether tutorial messages are shown.</summary>
    public bool ShowTutorials { get; set; } = true;
    /// <summary>Gets or sets the animation speed (Slow, Normal, Fast, Instant).</summary>
    public string AnimationSpeed { get; set; } = "Normal";
    /// <summary>Gets or sets a value indicating whether health bars are displayed.</summary>
    public bool ShowHealthBars { get; set; } = true;
    /// <summary>Gets or sets a value indicating whether damage numbers are displayed.</summary>
    public bool ShowDamageNumbers { get; set; } = true;
    /// <summary>Gets or sets a value indicating whether a confirmation prompt is shown on exit.</summary>
    public bool ConfirmOnExit { get; set; } = true;
    /// <summary>Gets or sets the number of items displayed per page in lists.</summary>
    public int PageSize { get; set; } = 10;
}