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
    /// Gets or sets the collection of sockets available on this item, organized by socket type.
    /// Sockets are player-customizable after generation.
    /// Key = SocketType, Value = List of sockets for that type.
    /// </summary>
    public Dictionary<SocketType, List<Socket>> Sockets { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the total rarity weight calculated from base item, material, enchantments, and sockets.
    /// </summary>
    public int TotalRarityWeight { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets the base item name before enhancements are applied (e.g., \"Longsword\").
    /// </summary>
    public string BaseName { get; set; } = string.Empty;
    
    /// <summary>
    /// Ordered list of prefix components (quality, material, enchantments, etc.) that appear before the base name.
    /// Each component preserves its token identifier and display value.
    /// </summary>
    public List<NameComponent> Prefixes { get; set; } = new();
    
    /// <summary>
    /// Ordered list of suffix components (enchantments, sockets, etc.) that appear after the base name.
    /// Each component preserves its token identifier and display value.
    /// </summary>
    public List<NameComponent> Suffixes { get; set; } = new();
    /// <summary>
    /// Gets or sets the upgrade level of this item (+1, +2, +3, etc.).
    /// Higher upgrade levels increase attribute bonuses.
    /// </summary>
    public int UpgradeLevel { get; set; } = 0;

    /// <summary>
    /// Collection of enchantment reference IDs (v4.1 format) that can be applied to this item.
    /// ⚠️ HYBRID PATTERN: Both EnchantmentIds (templates) and Enchantments (resolved) exist.
    /// </summary>
    /// <remarks>
    /// <para><strong>✅ HOW TO RESOLVE - Use ReferenceResolverService:</strong></para>
    /// <code>
    /// // C# - Apply enchantments during item generation
    /// var resolver = new ReferenceResolverService(dataCache);
    /// var enchantments = new List&lt;ItemEnhancement&gt;();
    /// foreach (var refId in item.EnchantmentIds)
    /// {
    ///     var enchantJson = await resolver.ResolveToObjectAsync(refId);
    ///     var enchantment = enchantJson.ToObject&lt;ItemEnhancement&gt;();
    ///     enchantments.Add(enchantment);
    /// }
    /// item.Enchantments = enchantments; // Store resolved enchantments
    /// item.Name = GenerateEnchantedName(item.BaseName, enchantments);
    /// </code>
    /// <code>
    /// // GDScript - Apply enchantments in Godot
    /// var resolver = ReferenceResolverService.new(data_cache)
    /// var enchantments = []
    /// for ref_id in item.EnchantmentIds:
    ///     var enchant_data = await resolver.ResolveToObjectAsync(ref_id)
    ///     enchantments.append(enchant_data)
    /// item.enchantments = enchantments
    /// </code>
    /// <para><strong>⚠️ Hybrid Pattern Explained:</strong></para>
    /// <list type="bullet">
    /// <item><description><strong>EnchantmentIds</strong> = Template references from item catalog</description></item>
    /// <item><description><strong>Enchantments</strong> = Resolved enhancement objects baked into item</description></item>
    /// <item><description>IDs used during generation, then resolved objects stored</description></item>
    /// <item><description>At runtime, use Enchantments list (already resolved)</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// Example enchantment reference IDs:
    /// <code>
    /// [
    ///   "@items/enchantments/elemental:fire",
    ///   "@items/enchantments/attribute:strength-boost"
    /// ]
    /// </code>
    /// </example>
    public List<string> EnchantmentIds { get; set; } = new();
    
    /// <summary>
    /// Collection of material reference IDs (v4.1 format) this item can be crafted from.
    /// ⚠️ HYBRID PATTERN: Materials resolve to Material property string at generation time.
    /// </summary>
    /// <remarks>
    /// <para><strong>✅ HOW TO RESOLVE - Use ReferenceResolverService:</strong></para>
    /// <code>
    /// // C# - Apply material during item generation
    /// var resolver = new ReferenceResolverService(dataCache);
    /// if (item.MaterialIds.Any())
    /// {
    ///     var randomMaterialRefId = item.MaterialIds.PickRandom();
    ///     var materialJson = await resolver.ResolveToObjectAsync(randomMaterialRefId);
    ///     var material = materialJson.ToObject&lt;Material&gt;();
    ///     item.Material = material.Name; // Store resolved name
    ///     item.Name = $"{material.Name} {item.BaseName}";
    /// }
    /// </code>
    /// <code>
    /// // GDScript - Apply material in Godot
    /// var resolver = ReferenceResolverService.new(data_cache)
    /// if item.MaterialIds.size() > 0:
    ///     var mat_ref_id = item.MaterialIds.pick_random()
    ///     var mat_data = await resolver.ResolveToObjectAsync(mat_ref_id)
    ///     item.material = mat_data.name
    ///     item.name = mat_data.name + " " + item.base_name
    /// </code>
    /// <para><strong>⚠️ Hybrid Pattern:</strong></para>
    /// <list type="bullet">
    /// <item><description><strong>MaterialIds</strong> = Template references from item catalog</description></item>
    /// <item><description><strong>Material</strong> = Resolved material name string ("Iron", "Steel")</description></item>
    /// <item><description>IDs used during generation, then resolved name stored</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// Example material reference IDs:
    /// <code>
    /// [
    ///   "@items/materials/metals:iron",
    ///   "@items/materials/metals:steel",
    ///   "@items/materials/metals:mithril"
    /// ]
    /// </code>
    /// </example>
    public List<string> MaterialIds { get; set; } = new();
    
    /// <summary>
    /// Collection of item reference IDs (v4.1 format) required for crafting recipes or upgrades.
    /// Each ID is a JSON reference like "@items/materials/metals:iron-ingot".
    /// </summary>
    /// <remarks>
    /// <para><strong>✅ HOW TO RESOLVE - Use ReferenceResolverService:</strong></para>
    /// <code>
    /// // C# - Check crafting requirements
    /// var resolver = new ReferenceResolverService(dataCache);
    /// var requiredItems = new List&lt;Item&gt;();
    /// foreach (var refId in item.RequiredItemIds)
    /// {
    ///     var itemJson = await resolver.ResolveToObjectAsync(refId);
    ///     var requiredItem = itemJson.ToObject&lt;Item&gt;();
    ///     requiredItems.Add(requiredItem);
    /// }
    /// bool canCraft = requiredItems.All(req => player.Inventory.Contains(req.Name));
    /// if (canCraft)
    /// {
    ///     CraftItem(item);
    ///     player.Inventory.RemoveRange(requiredItems);
    /// }
    /// </code>
    /// <code>
    /// // GDScript - Verify crafting materials in Godot
    /// var resolver = ReferenceResolverService.new(data_cache)
    /// var can_craft = true
    /// for ref_id in item.RequiredItemIds:
    ///     var required_item = await resolver.ResolveToObjectAsync(ref_id)
    ///     if not player.inventory.has_item(required_item.name):
    ///         can_craft = false
    ///         break
    /// if can_craft:
    ///     craft_item(item)
    /// </code>
    /// </remarks>
    /// <example>
    /// Example required item reference IDs:
    /// <code>
    /// [
    ///   "@items/materials/metals:iron-ingot",
    ///   "@items/materials/leather:thick-leather"
    /// ]
    /// </code>
    /// </example>
    public List<string> RequiredItemIds { get; set; } = new();

    /// <summary>
    /// Fully resolved Item objects required for crafting this item.
    /// Populated by ItemGenerator.GenerateAsync() when hydrating templates.
    /// Not serialized to JSON (template IDs stored in RequiredItemIds instead).
    /// </summary>
    /// <remarks>
    /// <para><strong>For Runtime Use:</strong></para>
    /// <list type="bullet">
    /// <item><description>Use this property to check if player has crafting materials</description></item>
    /// <item><description>Already resolved - no need to call ReferenceResolverService</description></item>
    /// <item><description>Null if item loaded from template without hydration</description></item>
    /// </list>
    /// </remarks>
    [System.Text.Json.Serialization.JsonIgnore]
    public List<Item> RequiredItems { get; set; } = new();

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

        // 4. Add socket content traits (additive for numeric, last one wins for text)
        foreach (var socketList in Sockets.Values)
        {
            foreach (var socket in socketList)
            {
                if (socket.Content != null)
                {
                    foreach (var trait in socket.Content.Traits)
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
        }

        return mergedTraits;
    }

    /// <summary>
    /// Gets the value of a specific prefix component by token name.
    /// </summary>
    /// <param name="token">The token name to search for (e.g., "quality", "material").</param>
    /// <returns>The component value if found, otherwise null.</returns>
    public string? GetPrefixValue(string token)
    {
        return Prefixes.FirstOrDefault(p => p.Token == token)?.Value;
    }
    
    /// <summary>
    /// Gets the value of a specific suffix component by token name.
    /// </summary>
    /// <param name="token">The token name to search for.</param>
    /// <returns>The component value if found, otherwise null.</returns>
    public string? GetSuffixValue(string token)
    {
        return Suffixes.FirstOrDefault(s => s.Token == token)?.Value;
    }

    /// <summary>
    /// Rebuilds the full item name from naming components in the correct order.
    /// Format: [Prefixes] [BaseName] [Suffixes]
    /// </summary>
    /// <returns>The properly composed item name.</returns>
    public string ComposeNameFromComponents()
    {
        var parts = new List<string>();
        
        // Add all prefixes in order
        parts.AddRange(Prefixes.Select(p => p.Value));
        
        // Add base name
        if (!string.IsNullOrWhiteSpace(BaseName)) parts.Add(BaseName);
        
        // Add all suffixes in order
        parts.AddRange(Suffixes.Select(s => s.Value));
        
        return string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
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
    
    /// <summary>
    /// Get rich socket information for all socket types on this item.
    /// Useful for Godot UI display.
    /// </summary>
    public List<SocketInfo> GetSocketsInfo()
    {
        return Sockets
            .Where(kvp => kvp.Value.Any())
            .Select(kvp => new SocketInfo
            {
                Type = kvp.Key,
                Sockets = kvp.Value,
                FilledCount = kvp.Value.Count(s => s.Content != null),
                TotalCount = kvp.Value.Count
            })
            .OrderBy(info => info.Type)
            .ToList();
    }
    
    /// <summary>
    /// Get a display string showing all socket types and their fill status.
    /// Example: "Gem: 1/2 | Essence: 0/1 | Rune: 3/3"
    /// </summary>
    public string GetSocketsDisplayText()
    {
        var infos = GetSocketsInfo();
        if (!infos.Any()) return string.Empty;
        
        return string.Join(" | ", infos.Select(info => info.DisplayText));
    }
}
