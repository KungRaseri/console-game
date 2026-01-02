namespace RealmEngine.Shared.Models;

/// <summary>
/// Represents an item in the game.
/// Supports the Hybrid Enhancement System v1.0 with materials, enchantments, and gem sockets.
/// </summary>
public class Item : ITraitable
{
    /// <summary>
    /// Gets or sets the unique identifier for this item.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Gets or sets the display name of the item (may include enhancements).
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the descriptive text for the item.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the market value of the item in gold.
    /// </summary>
    public int Price { get; set; }
    
    /// <summary>
    /// Gets or sets the item rarity (Common, Uncommon, Rare, Epic, Legendary, Mythic).
    /// </summary>
    public ItemRarity Rarity { get; set; } = ItemRarity.Common;
    
    /// <summary>
    /// Gets or sets the item type/category (Weapon, Armor, Consumable, Quest, Material, etc.).
    /// </summary>
    public ItemType Type { get; set; } = ItemType.Consumable;

    /// <summary>
    /// Gets or sets the trait system dictionary for dynamic properties defined in JSON.
    /// Implements ITraitable interface.
    /// </summary>
    public Dictionary<string, TraitValue> Traits { get; set; } = new();

    /// <summary>
    /// Gets or sets the equipment set name this item belongs to (if any).
    /// Items in a set grant bonuses when multiple pieces are equipped.
    /// </summary>
    public string? SetName { get; set; }

    /// <summary>
    /// Gets or sets whether this weapon requires both hands to wield.
    /// Two-handed weapons cannot be used with shields.
    /// </summary>
    public bool IsTwoHanded { get; set; } = false;

    // Enhancement System v1.0 (Hybrid Model)
    // ========================================
    
    /// <summary>
    /// Gets or sets the material this item is crafted from (e.g., \"iron\", \"steel\", \"mithril\").
    /// Materials are baked into the item at generation time.
    /// </summary>
    public string? Material { get; set; }
    
    /// <summary>
    /// Gets or sets the traits provided by this item's material.
    /// Materials contribute to the item's overall power level.
    /// </summary>
    public Dictionary<string, TraitValue> MaterialTraits { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the collection of enchantments applied to this item.
    /// Enchantments are baked into the item at generation time.
    /// </summary>
    public List<Enchantment> Enchantments { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the collection of gem sockets available on this item.
    /// Gem sockets are player-customizable after generation.
    /// </summary>
    public List<GemSocket> GemSockets { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the total rarity weight calculated from base item, material, enchantments, and sockets.
    /// </summary>
    public int TotalRarityWeight { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets the base item name before enhancements are applied (e.g., \"Longsword\").
    /// </summary>
    public string BaseName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the upgrade level of this item (+1, +2, +3, etc.).
    /// Higher upgrade levels increase attribute bonuses.
    /// </summary>
    public int UpgradeLevel { get; set; } = 0;

    /// <summary>
    /// Gets or sets the collection of enchantment IDs resolved from @enchantments references in JSON data.
    /// </summary>
    public List<string> EnchantmentIds { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the collection of material IDs resolved from @materials references in JSON data.
    /// </summary>
    public List<string> MaterialIds { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the collection of required item IDs for crafting or upgrades.
    /// </summary>
    public List<string> RequiredItemIds { get; set; } = new();

    /// <summary>
    /// Gets or sets the Strength attribute bonus provided by this item.
    /// </summary>
    public int BonusStrength { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets the Dexterity attribute bonus provided by this item.
    /// </summary>
    public int BonusDexterity { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets the Constitution attribute bonus provided by this item.
    /// </summary>
    public int BonusConstitution { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets the Intelligence attribute bonus provided by this item.
    /// </summary>
    public int BonusIntelligence { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets the Wisdom attribute bonus provided by this item.
    /// </summary>
    public int BonusWisdom { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets the Charisma attribute bonus provided by this item.
    /// </summary>
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
