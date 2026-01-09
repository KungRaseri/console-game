namespace RealmEngine.Core.Settings;

/// <summary>
/// Gameplay configuration settings
/// </summary>
public class GameplaySettings
{
    /// <summary>Gets or sets a value indicating whether cheat codes are enabled.</summary>
    public bool EnableCheats { get; set; } = false;
    /// <summary>Gets or sets a value indicating whether multiple save slots are allowed.</summary>
    public bool AllowMultipleSaves { get; set; } = true;
    /// <summary>Gets or sets a value indicating whether permanent death (hardcore mode) is enabled.</summary>
    public bool PermanentDeath { get; set; } = false;
    /// <summary>Gets or sets the battle speed setting (Slow, Normal, Fast).</summary>
    public string BattleSpeed { get; set; } = "Normal";
    /// <summary>Gets or sets a value indicating whether enemy stats are shown during combat.</summary>
    public bool ShowEnemyStats { get; set; } = true;
    /// <summary>Gets or sets a value indicating whether random encounters are enabled.</summary>
    public bool RandomEncounters { get; set; } = true;
    /// <summary>Gets or sets the random encounter rate (0.0 to 1.0).</summary>
    public double EncounterRate { get; set; } = 0.3;
}