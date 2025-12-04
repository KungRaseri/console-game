namespace Game.Settings;

/// <summary>
/// Core game configuration settings
/// </summary>
public class GameSettings
{
    public string Version { get; set; } = "1.0.0";
    public string DefaultDifficulty { get; set; } = "Normal";
    public bool AutoSave { get; set; } = true;
    public int AutoSaveIntervalSeconds { get; set; } = 300;
    public int MaxSaveSlots { get; set; } = 10;
    public int StartingGold { get; set; } = 100;
    public int StartingHealth { get; set; } = 100;
    public int StartingMana { get; set; } = 50;
    public int MaxLevel { get; set; } = 100;
    public int ExperiencePerLevel { get; set; } = 100;
}
