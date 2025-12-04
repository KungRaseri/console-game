namespace Game.Models;

/// <summary>
/// Represents an item in the game.
/// </summary>
public class Item
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Price { get; set; }
    public ItemRarity Rarity { get; set; } = ItemRarity.Common;
    public ItemType Type { get; set; } = ItemType.Consumable;
}