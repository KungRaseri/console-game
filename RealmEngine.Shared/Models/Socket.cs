namespace RealmEngine.Shared.Models;

/// <summary>
/// Represents a socket slot in an item that can hold socketable content.
/// No subtype constraints - any gem fits any gem socket, any essence fits any essence socket, etc.
/// </summary>
public class Socket
{
    /// <summary>
    /// Type of content this socket accepts (Gem, Essence, Rune, Crystal, or Orb).
    /// </summary>
    public SocketType Type { get; set; }
    
    /// <summary>
    /// The socketable item currently in this socket (null if empty).
    /// </summary>
    public ISocketable? Content { get; set; }
    
    /// <summary>
    /// Whether this socket is locked and cannot be modified.
    /// </summary>
    public bool IsLocked { get; set; } = false;
    
    /// <summary>
    /// Check if this socket can accept a given socketable item.
    /// </summary>
    public bool CanAccept(ISocketable item)
    {
        if (IsLocked) return false;
        if (Content != null) return false; // Already filled
        return item.SocketType == Type;
    }
    
    /// <summary>
    /// Get a display string for this socket.
    /// </summary>
    public string GetDisplayName()
    {
        if (Content != null)
        {
            return $"[{Type} Socket: {Content.Name}]";
        }
        else if (IsLocked)
        {
            return $"[{Type} Socket: Locked]";
        }
        else
        {
            return $"[{Type} Socket: Empty]";
        }
    }
}
