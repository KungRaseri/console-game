namespace Game.Models;

/// <summary>
/// Represents a saved game state.
/// </summary>
public class SaveGame
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PlayerName { get; set; } = string.Empty;
    public DateTime SaveDate { get; set; } = DateTime.Now;
    public Character Character { get; set; } = new();
    public List<Item> Inventory { get; set; } = new();
    public int PlayTimeMinutes { get; set; }
}
