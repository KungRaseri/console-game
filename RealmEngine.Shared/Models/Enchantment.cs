namespace RealmEngine.Shared.Models;

/// <summary>
/// Represents an enchantment that can be applied to an item.
/// Part of the Hybrid Enhancement System v1.0.
/// </summary>
public class Enchantment : ITraitable
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the description.</summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the rarity.</summary>
    public EnchantmentRarity Rarity { get; set; } = EnchantmentRarity.Minor;

    /// <summary>Gets or sets the trait-based bonuses.</summary>
    public Dictionary<string, TraitValue> Traits { get; set; } = new();
    
    /// <summary>Gets or sets the position in the name (prefix or suffix).</summary>
    public EnchantmentPosition Position { get; set; } = EnchantmentPosition.Suffix;
    
    /// <summary>Gets or sets the rarity weight for procedural generation.</summary>
    public int RarityWeight { get; set; } = 50;

    /// <summary>Gets or sets the bonus strength.</summary>
    public int BonusStrength { get; set; } = 0;
    
    /// <summary>Gets or sets the bonus dexterity.</summary>
    public int BonusDexterity { get; set; } = 0;
    
    /// <summary>Gets or sets the bonus constitution.</summary>
    public int BonusConstitution { get; set; } = 0;
    
    /// <summary>Gets or sets the bonus intelligence.</summary>
    public int BonusIntelligence { get; set; } = 0;
    
    /// <summary>Gets or sets the bonus wisdom.</summary>
    public int BonusWisdom { get; set; } = 0;
    
    /// <summary>Gets or sets the bonus charisma.</summary>
    public int BonusCharisma { get; set; } = 0;

    /// <summary>Gets or sets the special effect.</summary>
    public string? SpecialEffect { get; set; }

    /// <summary>Gets or sets the enchantment level.</summary>
    public int Level { get; set; } = 1;
}

/// <summary>
/// Position of enchantment in item name.
/// </summary>
public enum EnchantmentPosition
{
    /// <summary>Prefix position (e.g., "Flaming" in "Flaming Sword").</summary>
    Prefix,
    /// <summary>Suffix position (e.g., "of Fire" in "Sword of Fire").</summary>
    Suffix
}

/// <summary>
/// Rarity levels for enchantments.
/// </summary>
public enum EnchantmentRarity
{
    /// <summary>Minor enchantment.</summary>
    Minor,
    /// <summary>Lesser enchantment.</summary>
    Lesser,
    /// <summary>Greater enchantment.</summary>
    Greater,
    /// <summary>Superior enchantment.</summary>
    Superior,
    /// <summary>Legendary enchantment.</summary>
    Legendary
}
