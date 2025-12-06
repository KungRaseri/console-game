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
    
    // Equipment set
    public string? SetName { get; set; }
    
    // Weapon properties
    public bool IsTwoHanded { get; set; } = false;
    
    // Enchantments applied to this item
    public List<Enchantment> Enchantments { get; set; } = new();
    
    // Upgrade level (+1, +2, +3, etc.)
    public int UpgradeLevel { get; set; } = 0;
    
    // D20 Attribute Bonuses - bonuses provided by this item
    public int BonusStrength { get; set; } = 0;
    public int BonusDexterity { get; set; } = 0;
    public int BonusConstitution { get; set; } = 0;
    public int BonusIntelligence { get; set; } = 0;
    public int BonusWisdom { get; set; } = 0;
    public int BonusCharisma { get; set; } = 0;
    
    // Legacy properties for backward compatibility
    public int BonusDefense { get => BonusConstitution; set => BonusConstitution = value; }
    public int BonusAgility { get => BonusDexterity; set => BonusDexterity = value; }
    public int BonusVitality { get => BonusConstitution; set => BonusConstitution = value; }

    /// <summary>
    /// Get the total strength bonus including base bonus, enchantments, and upgrade level.
    /// </summary>
    public int GetTotalBonusStrength()
    {
        int total = BonusStrength;
        total += Enchantments.Sum(e => e.BonusStrength);
        total += UpgradeLevel * 2; // Each upgrade level adds +2 to all stats
        return total;
    }

    /// <summary>
    /// Get the total dexterity bonus including base bonus, enchantments, and upgrade level.
    /// </summary>
    public int GetTotalBonusDexterity()
    {
        int total = BonusDexterity;
        total += Enchantments.Sum(e => e.BonusDexterity);
        total += UpgradeLevel * 2;
        return total;
    }

    /// <summary>
    /// Get the total constitution bonus including base bonus, enchantments, and upgrade level.
    /// </summary>
    public int GetTotalBonusConstitution()
    {
        int total = BonusConstitution;
        total += Enchantments.Sum(e => e.BonusConstitution);
        total += UpgradeLevel * 2;
        return total;
    }

    /// <summary>
    /// Get the total intelligence bonus including base bonus, enchantments, and upgrade level.
    /// </summary>
    public int GetTotalBonusIntelligence()
    {
        int total = BonusIntelligence;
        total += Enchantments.Sum(e => e.BonusIntelligence);
        total += UpgradeLevel * 2;
        return total;
    }

    /// <summary>
    /// Get the total wisdom bonus including base bonus, enchantments, and upgrade level.
    /// </summary>
    public int GetTotalBonusWisdom()
    {
        int total = BonusWisdom;
        total += Enchantments.Sum(e => e.BonusWisdom);
        total += UpgradeLevel * 2;
        return total;
    }

    /// <summary>
    /// Get the total charisma bonus including base bonus, enchantments, and upgrade level.
    /// </summary>
    public int GetTotalBonusCharisma()
    {
        int total = BonusCharisma;
        total += Enchantments.Sum(e => e.BonusCharisma);
        total += UpgradeLevel * 2;
        return total;
    }

    // Legacy methods for backward compatibility
    /// <summary>
    /// Get the total defense bonus (maps to Constitution).
    /// </summary>
    public int GetTotalBonusDefense()
    {
        return GetTotalBonusConstitution();
    }

    /// <summary>
    /// Get the total agility bonus (maps to Dexterity).
    /// </summary>
    public int GetTotalBonusAgility()
    {
        return GetTotalBonusDexterity();
    }

    /// <summary>
    /// Get the total vitality bonus (maps to Constitution).
    /// </summary>
    public int GetTotalBonusVitality()
    {
        return GetTotalBonusConstitution();
    }

    /// <summary>
    /// Get the display name for this item including upgrade level and enchantments.
    /// </summary>
    public string GetDisplayName()
    {
        var nameParts = new List<string>();
        
        // Add upgrade level prefix
        if (UpgradeLevel > 0)
        {
            nameParts.Add($"+{UpgradeLevel}");
        }
        
        // Add base name
        nameParts.Add(Name);
        
        // Add enchantment suffixes
        foreach (var enchantment in Enchantments)
        {
            nameParts.Add($"({enchantment.Name})");
        }
        
        return string.Join(" ", nameParts);
    }
}
