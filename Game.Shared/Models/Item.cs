namespace Game.Shared.Models;

/// <summary>
/// Represents an item in the game.
/// </summary>
public class Item : ITraitable
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Price { get; set; }
    public ItemRarity Rarity { get; set; } = ItemRarity.Common;
    public ItemType Type { get; set; } = ItemType.Consumable;

    // Trait system - flexible properties defined in JSON
    public Dictionary<string, TraitValue> Traits { get; set; } = new();

    // Equipment set
    public string? SetName { get; set; }

    // Weapon properties
    public bool IsTwoHanded { get; set; } = false;

    // Enhancement System v1.0 (Hybrid Model)
    // ========================================
    
    // Material System (Baked into item at generation)
    public string? Material { get; set; } // e.g., "iron", "steel", "mithril"
    public Dictionary<string, TraitValue> MaterialTraits { get; set; } = new(); // Traits from material
    
    // Enchantment System (Baked into item at generation)
    public List<Enchantment> Enchantments { get; set; } = new();
    
    // Gem Socket System (Player customizable after generation)
    public List<GemSocket> GemSockets { get; set; } = new();
    
    // Rarity Weight System
    public int TotalRarityWeight { get; set; } = 0; // Sum of base + material + pattern + enchantments + sockets
    public string BaseName { get; set; } = string.Empty; // Base item name before enhancements (e.g., "Longsword")

    // Upgrade level (+1, +2, +3, etc.)
    public int UpgradeLevel { get; set; } = 0;

    // D20 Attribute Bonuses - bonuses provided by this item
    public int BonusStrength { get; set; } = 0;
    public int BonusDexterity { get; set; } = 0;
    public int BonusConstitution { get; set; } = 0;
    public int BonusIntelligence { get; set; } = 0;
    public int BonusWisdom { get; set; } = 0;
    public int BonusCharisma { get; set; } = 0;

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

    /// <summary>
    /// Get all traits merged from base item, material, enchantments, and gems.
    /// Follows trait merging rules from ITEM_ENHANCEMENT_SYSTEM.md.
    /// </summary>
    public Dictionary<string, TraitValue> GetTotalTraits()
    {
        var mergedTraits = new Dictionary<string, TraitValue>();

        // 1. Start with base item traits
        foreach (var trait in Traits)
        {
            mergedTraits[trait.Key] = trait.Value;
        }

        // 2. Add material traits (additive for numeric, override for text)
        foreach (var trait in MaterialTraits)
        {
            if (mergedTraits.ContainsKey(trait.Key))
            {
                // Merge existing trait
                var existing = mergedTraits[trait.Key];
                if (existing.Type == TraitType.Number && trait.Value.Type == TraitType.Number)
                {
                    // Numeric: add values
                    var sum = existing.AsDouble() + trait.Value.AsDouble();
                    mergedTraits[trait.Key] = new TraitValue(sum, TraitType.Number);
                }
                else
                {
                    // Text/Boolean: material overrides
                    mergedTraits[trait.Key] = trait.Value;
                }
            }
            else
            {
                // New trait from material
                mergedTraits[trait.Key] = trait.Value;
            }
        }

        // 3. Add enchantment traits (additive for numeric, last one wins for text)
        foreach (var enchantment in Enchantments)
        {
            foreach (var trait in enchantment.Traits)
            {
                if (mergedTraits.ContainsKey(trait.Key))
                {
                    var existing = mergedTraits[trait.Key];
                    if (existing.Type == TraitType.Number && trait.Value.Type == TraitType.Number)
                    {
                        var sum = existing.AsDouble() + trait.Value.AsDouble();
                        mergedTraits[trait.Key] = new TraitValue(sum, TraitType.Number);
                    }
                    else
                    {
                        mergedTraits[trait.Key] = trait.Value;
                    }
                }
                else
                {
                    mergedTraits[trait.Key] = trait.Value;
                }
            }
        }

        // 4. Add gem socket traits (additive for numeric, last one wins for text)
        foreach (var socket in GemSockets)
        {
            if (socket.Gem != null)
            {
                foreach (var trait in socket.Gem.Traits)
                {
                    if (mergedTraits.ContainsKey(trait.Key))
                    {
                        var existing = mergedTraits[trait.Key];
                        if (existing.Type == TraitType.Number && trait.Value.Type == TraitType.Number)
                        {
                            var sum = existing.AsDouble() + trait.Value.AsDouble();
                            mergedTraits[trait.Key] = new TraitValue(sum, TraitType.Number);
                        }
                        else
                        {
                            mergedTraits[trait.Key] = trait.Value;
                        }
                    }
                    else
                    {
                        mergedTraits[trait.Key] = trait.Value;
                    }
                }
            }
        }

        return mergedTraits;
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
