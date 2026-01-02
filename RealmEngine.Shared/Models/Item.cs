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
    /// Collection of enchantment IDs that can be applied to this item during generation.
    /// These are resolved from @enchantments JSON references and baked into item at creation.
    /// </summary>
    /// <remarks>
    /// <para><strong>Resolution Pattern (C#):</strong></para>
    /// <code>
    /// // Apply enchantments during item generation
    /// var enchantments = await enchantmentRepository.GetByIdsAsync(item.EnchantmentIds);
    /// item.Enchantments = enchantments.Select(e => ApplyEnchantment(e, item)).ToList();
    /// item.Name = GenerateEnchantedName(item.BaseName, enchantments);
    /// </code>
    /// <para><strong>Resolution Pattern (GDScript/Godot):</strong></para>
    /// <code>
    /// # Generate enchanted item
    /// var enchantments = []
    /// for enchantment_id in item.EnchantmentIds:
    ///     var enchantment = await enchantment_service.get_by_id(enchantment_id)
    ///     enchantments.append(enchantment)
    /// item.apply_enchantments(enchantments)
    /// </code>
    /// <para><strong>Why IDs instead of objects?</strong></para>
    /// <list type="bullet">
    /// <item><description>Template references - points to enchantment catalog</description></item>
    /// <item><description>Generation-time resolution - enchantments applied when item created</description></item>
    /// <item><description>Hybrid pattern - IDs + resolved Enchantments list both exist</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// Example IDs: ["@enchantments/elemental:fire", "@enchantments/attribute:strength-boost"]
    /// </example>
    public List<string> EnchantmentIds { get; set; } = new();
    
    /// <summary>
    /// Collection of material IDs that this item can be crafted from.
    /// These are resolved from @materials JSON references during item generation.
    /// </summary>
    /// <remarks>
    /// <para><strong>Resolution Pattern (C#):</strong></para>
    /// <code>
    /// // Apply material during crafting
    /// var materials = await materialRepository.GetByIdsAsync(item.MaterialIds);
    /// var selectedMaterial = materials.RandomElement();
    /// item.Material = selectedMaterial.Name;
    /// item.MaterialTraits = selectedMaterial.Traits;
    /// item.Name = $"{selectedMaterial.Name} {item.BaseName}";
    /// </code>
    /// <para><strong>Resolution Pattern (GDScript/Godot):</strong></para>
    /// <code>
    /// # Craft item with material
    /// var material_id = item.MaterialIds.pick_random()
    /// var material = await material_service.get_by_id(material_id)
    /// item.material = material.name
    /// item.apply_material_traits(material.traits)
    /// </code>
    /// </remarks>
    /// <example>
    /// Example IDs: ["@materials/metals:iron", "@materials/metals:steel", "@materials/metals:mithril"]
    /// </example>
    public List<string> MaterialIds { get; set; } = new();
    
    /// <summary>
    /// Collection of item IDs required for crafting recipes or item upgrades.
    /// These are resolved from @items JSON references when checking craft requirements.
    /// </summary>
    /// <remarks>
    /// <para><strong>Resolution Pattern (C#):</strong></para>
    /// <code>
    /// // Check if player can craft item
    /// var requiredItems = await itemRepository.GetByIdsAsync(item.RequiredItemIds);
    /// bool canCraft = requiredItems.All(req => player.Inventory.Contains(req));
    /// if (canCraft)
    /// {
    ///     CraftItem(item, requiredItems);
    ///     player.Inventory.RemoveRange(requiredItems);
    /// }
    /// </code>
    /// <para><strong>Resolution Pattern (GDScript/Godot):</strong></para>
    /// <code>
    /// # Verify crafting materials
    /// var can_craft = true
    /// for required_id in item.RequiredItemIds:
    ///     if not player.inventory.has_item(required_id):
    ///         can_craft = false
    ///         break
    /// if can_craft:
    ///     craft_item(item)
    /// </code>
    /// </remarks>
    /// <example>
    /// Example IDs: ["@items/materials/metals:iron-ingot", "@items/materials/leather:thick-leather"]
    /// </example>
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
