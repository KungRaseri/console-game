namespace RealmEngine.Core.Settings;

/// <summary>
/// Core game configuration settings
/// </summary>
public class GameSettings
{
    /// <summary>Gets or sets the game version.</summary>
    public string Version { get; set; } = "1.0.0";
    /// <summary>Gets or sets the default difficulty level (Easy, Normal, Hard).</summary>
    public string DefaultDifficulty { get; set; } = "Normal";
    /// <summary>Gets or sets a value indicating whether auto-save is enabled.</summary>
    public bool AutoSave { get; set; } = true;
    /// <summary>Gets or sets the auto-save interval in seconds.</summary>
    public int AutoSaveIntervalSeconds { get; set; } = 300;
    /// <summary>Gets or sets the maximum number of save slots.</summary>
    public int MaxSaveSlots { get; set; } = 10;
    /// <summary>Gets or sets the starting gold amount for new characters.</summary>
    public int StartingGold { get; set; } = 100;
    /// <summary>Gets or sets the starting health points for new characters.</summary>
    public int StartingHealth { get; set; } = 100;
    /// <summary>Gets or sets the starting mana points for new characters.</summary>
    public int StartingMana { get; set; } = 50;
    /// <summary>Gets or sets the maximum character level.</summary>
    public int MaxLevel { get; set; } = 100;
    /// <summary>Gets or sets the experience points required per level.</summary>
    public int ExperiencePerLevel { get; set; } = 100;
}