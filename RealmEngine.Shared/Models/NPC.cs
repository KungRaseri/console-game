namespace RealmEngine.Shared.Models;

/// <summary>
/// Represents an NPC (Non-Player Character) in the game.
/// </summary>
public class NPC : ITraitable
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Occupation { get; set; } = string.Empty;
    public int Gold { get; set; }
    public string Dialogue { get; set; } = string.Empty;
    public bool IsFriendly { get; set; } = true;

    // Reference collections for resolved @references
    public List<string> DialogueIds { get; set; } = new();
    public List<string> AbilityIds { get; set; } = new();
    public List<string> InventoryIds { get; set; } = new();

    // Trait system
    public Dictionary<string, TraitValue> Traits { get; } = new();
}
