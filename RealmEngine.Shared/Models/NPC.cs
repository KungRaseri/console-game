namespace RealmEngine.Shared.Models;

/// <summary>
/// Represents an NPC (Non-Player Character) in the game.
/// </summary>
public class NPC : ITraitable
{
    /// <summary>
    /// Gets or sets the unique identifier for this NPC.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Gets or sets the display name of the NPC.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the age of the NPC in years.
    /// </summary>
    public int Age { get; set; }
    
    /// <summary>
    /// Gets or sets the NPC's profession or role (e.g., "Blacksmith", "Merchant", "Guard").
    /// </summary>
    public string Occupation { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the amount of gold this NPC possesses.
    /// </summary>
    public int Gold { get; set; }
    
    /// <summary>
    /// Gets or sets the default dialogue text for this NPC.
    /// </summary>
    public string Dialogue { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets whether this NPC is friendly to the player.
    /// Hostile NPCs may initiate combat.
    /// </summary>
    public bool IsFriendly { get; set; } = true;

    /// <summary>
    /// Gets or sets the collection of dialogue IDs resolved from @dialogue references in JSON data.
    /// </summary>
    public List<string> DialogueIds { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the collection of ability IDs resolved from @abilities references in JSON data.
    /// </summary>
    public List<string> AbilityIds { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the collection of inventory item IDs resolved from @items references in JSON data.
    /// </summary>
    public List<string> InventoryIds { get; set; } = new();

    /// <summary>
    /// Gets or sets the trait system dictionary for dynamic properties.
    /// Implements ITraitable interface.
    /// </summary>
    public Dictionary<string, TraitValue> Traits { get; set; } = new();
}
