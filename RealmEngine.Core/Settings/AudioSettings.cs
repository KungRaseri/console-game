namespace RealmEngine.Core.Settings;

/// <summary>
/// Audio configuration settings
/// </summary>
public class AudioSettings
{
    /// <summary>Gets or sets the master volume level (0.0 to 1.0).</summary>
    public double MasterVolume { get; set; } = 0.8;
    /// <summary>Gets or sets the music volume level (0.0 to 1.0).</summary>
    public double MusicVolume { get; set; } = 0.7;
    /// <summary>Gets or sets the sound effects volume level (0.0 to 1.0).</summary>
    public double SfxVolume { get; set; } = 0.9;
    /// <summary>Gets or sets a value indicating whether all audio is muted.</summary>
    public bool Muted { get; set; } = false;
    /// <summary>Gets or sets a value indicating whether background music is enabled.</summary>
    public bool EnableBackgroundMusic { get; set; } = true;
    /// <summary>Gets or sets a value indicating whether sound effects are enabled.</summary>
    public bool EnableSoundEffects { get; set; } = true;
}