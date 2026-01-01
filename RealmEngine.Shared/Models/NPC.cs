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

    // Trait system
    public Dictionary<string, TraitValue> Traits { get; } = new();
}
