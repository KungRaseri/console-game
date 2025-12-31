namespace Game.Shared.Models;

/// <summary>
/// Item types.
/// </summary>
public enum ItemType
{
    Consumable,

    // Weapons & Off-hand
    Weapon,
    Shield,
    // Orbs, tomes, etc.
    OffHand,

    // Armor pieces
    Helmet,
    Shoulders,
    Chest,
    Bracers,
    Gloves,
    Belt,
    Legs,
    Boots,

    // Jewelry
    Necklace,
    Ring,

    // Special
    QuestItem
}
