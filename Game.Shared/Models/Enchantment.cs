namespace Game.Shared.Models;

/// <summary>
/// Represents an enchantment that can be applied to an item.
/// Part of the Hybrid Enhancement System v1.0.
/// </summary>
public class Enchantment : ITraitable
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public EnchantmentRarity Rarity { get; set; } = EnchantmentRarity.Minor;

    // Enhancement System v1.0 - Trait-based bonuses
    public Dictionary<string, TraitValue> Traits { get; set; } = new();
    
    // Position in the name (prefix or suffix)
    public EnchantmentPosition Position { get; set; } = EnchantmentPosition.Suffix;
    
    // Rarity weight for procedural generation
    public int RarityWeight { get; set; } = 50;

    // D20 Attribute Bonuses (legacy - consider migrating to Traits)
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
/// Position of enchantment in item name.
/// </summary>
public enum EnchantmentPosition
{
    Prefix,  // e.g., "Flaming" in "Flaming Sword"
    Suffix   // e.g., "of Fire" in "Sword of Fire"
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
