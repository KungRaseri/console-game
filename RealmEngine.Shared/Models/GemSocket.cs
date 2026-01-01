namespace Game.Shared.Models;

/// <summary>
/// Represents a gem socket in an item (player-customizable enhancement).
/// Part of the Hybrid Enhancement System v1.0.
/// </summary>
public class GemSocket
{
    /// <summary>
    /// Color/type of gem this socket accepts (Red, Blue, Green, Yellow, White, Prismatic).
    /// </summary>
    public GemColor Color { get; set; }

    /// <summary>
    /// The gem currently socketed (null if empty).
    /// </summary>
    public Gem? Gem { get; set; }

    /// <summary>
    /// Whether this socket is locked and cannot be modified.
    /// </summary>
    public bool IsLocked { get; set; } = false;

    /// <summary>
    /// Get a display string for this socket.
    /// </summary>
    public string GetDisplayName()
    {
        if (Gem != null)
        {
            return $"[{Color} Socket: {Gem.Name}]";
        }
        else if (IsLocked)
        {
            return $"[{Color} Socket: Locked]";
        }
        else
        {
            return $"[{Color} Socket: Empty]";
        }
    }
}
