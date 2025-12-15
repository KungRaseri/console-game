namespace Game.Core.Models;

/// <summary>
/// Represents an enchantment that can be applied to an item.
/// </summary>
public class Enchantment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public EnchantmentRarity Rarity { get; set; } = EnchantmentRarity.Minor;
    
    // D20 Attribute Bonuses
    public int BonusStrength { get; set; } = 0;
    public int BonusDexterity { get; set; } = 0;
    public int BonusConstitution { get; set; } = 0;
    public int BonusIntelligence { get; set; } = 0;
    public int BonusWisdom { get; set; } = 0;
    public int BonusCharisma { get; set; } = 0;
    
    // Special effects (for display/future implementation)
    public string? SpecialEffect { get; set; }
    
    // Enchantment level (+1, +2, +3, etc.)
    public int Level { get; set; } = 1;
}

/// <summary>
/// Rarity levels for enchantments.
/// </summary>
public enum EnchantmentRarity
{
    Minor,
    Lesser,
    Greater,
    Superior,
    Legendary
}
