namespace Game.Core.Settings;

/// <summary>
/// Audio configuration settings
/// </summary>
public class AudioSettings
{
    public double MasterVolume { get; set; } = 0.8;
    public double MusicVolume { get; set; } = 0.7;
    public double SfxVolume { get; set; } = 0.9;
    public bool Muted { get; set; } = false;
    public bool EnableBackgroundMusic { get; set; } = true;
    public bool EnableSoundEffects { get; set; } = true;
}