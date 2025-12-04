namespace Game.Settings;

/// <summary>
/// User interface configuration settings
/// </summary>
public class UISettings
{
    public string ColorScheme { get; set; } = "Default";
    public bool ShowTutorials { get; set; } = true;
    public string AnimationSpeed { get; set; } = "Normal";
    public bool ShowHealthBars { get; set; } = true;
    public bool ShowDamageNumbers { get; set; } = true;
    public bool ConfirmOnExit { get; set; } = true;
    public int PageSize { get; set; } = 10;
}
