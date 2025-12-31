namespace Game.Shared.Models;

/// <summary>
/// Gem colors/types for the socket system.
/// Part of the Hybrid Enhancement System v1.0.
/// </summary>
public enum GemColor
{
    /// <summary>
    /// Red gems - typically provide offensive bonuses (damage, crit, attack speed).
    /// </summary>
    Red,

    /// <summary>
    /// Blue gems - typically provide defensive bonuses (armor, resistance, health).
    /// </summary>
    Blue,

    /// <summary>
    /// Green gems - typically provide utility bonuses (movement speed, resource regen, cooldown).
    /// </summary>
    Green,

    /// <summary>
    /// Yellow gems - typically provide magical bonuses (spell power, mana, magic penetration).
    /// </summary>
    Yellow,

    /// <summary>
    /// White gems - versatile gems that can fit any socket color.
    /// </summary>
    White,

    /// <summary>
    /// Prismatic gems - rare gems that provide bonuses from multiple categories.
    /// Can fit any socket.
    /// </summary>
    Prismatic
}
