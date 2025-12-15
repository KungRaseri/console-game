namespace Game.Core.Settings;

/// <summary>
/// Gameplay configuration settings
/// </summary>
public class GameplaySettings
{
    public bool EnableCheats { get; set; } = false;
    public bool AllowMultipleSaves { get; set; } = true;
    public bool PermanentDeath { get; set; } = false;
    public string BattleSpeed { get; set; } = "Normal";
    public bool ShowEnemyStats { get; set; } = true;
    public bool RandomEncounters { get; set; } = true;
    public double EncounterRate { get; set; } = 0.3;
}
