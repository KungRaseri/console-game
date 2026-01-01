namespace RealmEngine.Shared.Models;

/// <summary>
/// Represents a gem that can be socketed into items (player-customizable enhancement).
/// Part of the Hybrid Enhancement System v1.0.
/// </summary>
public class Gem : ITraitable
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Color/type of this gem (determines which sockets it can fit).
    /// </summary>
    public GemColor Color { get; set; }
    
    /// <summary>
    /// Rarity of the gem.
    /// </summary>
    public ItemRarity Rarity { get; set; } = ItemRarity.Common;
    
    /// <summary>
    /// Market value of this gem.
    /// </summary>
    public int Price { get; set; }
    
    /// <summary>
    /// Traits provided by this gem when socketed.
    /// </summary>
    public Dictionary<string, TraitValue> Traits { get; set; } = new();
    
    /// <summary>
    /// Rarity weight for procedural generation.
    /// </summary>
    public int RarityWeight { get; set; } = 50;

    /// <summary>
    /// Get a display string showing gem stats.
    /// </summary>
    public string GetDisplayName()
    {
        var traitsSummary = string.Join(", ", Traits.Select(t => $"{t.Key}: {t.Value.AsString()}"));
        return $"{Name} ({Color} Gem) - {traitsSummary}";
    }
}
