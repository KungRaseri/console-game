using Bogus;
using Game.Shared.Data.Models;
using Game.Core.Models;
using Game.Shared.Services;
using Game.Core.Utilities;

namespace Game.Core.Generators;

/// <summary>
/// Generates random items using Bogus and JSON data.
/// </summary>
public static class ItemGenerator
{
    private static readonly Faker<Item> ItemFaker = new Faker<Item>()
        .RuleFor(i => i.Id, f => Guid.NewGuid().ToString())
        .RuleFor(i => i.Name, f => f.Commerce.ProductName())
        .RuleFor(i => i.Description, f => f.Commerce.ProductDescription())
        .RuleFor(i => i.Price, f => f.Random.Int(5, 1000))
        .RuleFor(i => i.Rarity, f => f.PickRandom<ItemRarity>())
        .RuleFor(i => i.Type, f => f.PickRandom<ItemType>());

    /// <summary>
    /// Generate a single random item.
    /// </summary>
    public static Item Generate()
    {
        return ItemFaker.Generate();
    }

    /// <summary>
    /// Generate multiple random items.
    /// </summary>
    public static List<Item> Generate(int count)
    {
        return ItemFaker.Generate(count);
    }

    /// <summary>
    /// Generate items of a specific type.
    /// </summary>
    public static List<Item> GenerateByType(ItemType type, int count)
    {
        var typedFaker = new Faker<Item>()
            .RuleFor(i => i.Id, f => Guid.NewGuid().ToString())
            .RuleFor(i => i.Rarity, f => f.PickRandom<ItemRarity>())
            .RuleFor(i => i.Type, type)
            .RuleFor(i => i.IsTwoHanded, (f, item) => 
                item.Type == ItemType.Weapon && f.Random.Bool(0.3f)) // 30% chance for two-handed weapons
            .RuleFor(i => i.Name, (f, item) => GenerateNameForType(f, type, item))
            .RuleFor(i => i.Description, f => f.Commerce.ProductDescription())
            .RuleFor(i => i.Price, f => f.Random.Int(5, 1000))
            .RuleFor(i => i.BonusStrength, (f, item) => GenerateStatBonus(f, item, "Strength"))
            .RuleFor(i => i.BonusDexterity, (f, item) => GenerateStatBonus(f, item, "Dexterity"))
            .RuleFor(i => i.BonusConstitution, (f, item) => GenerateStatBonus(f, item, "Constitution"))
            .RuleFor(i => i.BonusIntelligence, (f, item) => GenerateStatBonus(f, item, "Intelligence"))
            .RuleFor(i => i.BonusWisdom, (f, item) => GenerateStatBonus(f, item, "Wisdom"))
            .RuleFor(i => i.BonusCharisma, (f, item) => GenerateStatBonus(f, item, "Charisma"));

        return typedFaker.Generate(count);
    }

    private static string GenerateNameForType(Faker f, ItemType type, Item item)
    {
        return type switch
        {
            // Weapons - use JSON data for more variety
            ItemType.Weapon => GenerateWeaponName(f, item),
            ItemType.Shield => $"{ApplyMetalMaterial(item, f) ?? "Iron"} Shield",
            ItemType.OffHand => $"{f.PickRandom("Tome", "Orb", "Crystal", "Focus")} {GetRandomEnchantmentSuffix(f, item)}",
            
            // Armor - use leather materials for light armor
            ItemType.Helmet => $"{ApplyLeatherMaterial(item, f) ?? "Leather"} Helmet",
            ItemType.Shoulders => $"{ApplyLeatherMaterial(item, f) ?? "Leather"} Shoulderpads",
            ItemType.Chest => $"{ApplyLeatherMaterial(item, f) ?? "Leather"} Chestpiece",
            ItemType.Bracers => $"{ApplyLeatherMaterial(item, f) ?? "Leather"} Bracers",
            ItemType.Gloves => $"{ApplyLeatherMaterial(item, f) ?? "Leather"} Gloves",
            ItemType.Belt => $"{ApplyLeatherMaterial(item, f) ?? "Leather"} Belt",
            ItemType.Legs => $"{ApplyLeatherMaterial(item, f) ?? "Leather"} Leggings",
            ItemType.Boots => $"{ApplyLeatherMaterial(item, f) ?? "Leather"} Boots",
            
            // Jewelry - use gemstones
            ItemType.Necklace => $"{ApplyGemstoneMaterial(item, f)} {f.PickRandom("Amulet", "Necklace", "Pendant")}",
            ItemType.Ring => $"{ApplyGemstoneMaterial(item, f)} {f.PickRandom("Ring", "Band", "Signet")}",
            
            // Other
            ItemType.Consumable => $"{f.PickRandom("Health", "Mana", "Stamina")} Potion",
            ItemType.QuestItem => $"{f.Lorem.Word()} {f.PickRandom("Key", "Scroll", "Crystal", "Relic")}",
            
            _ => f.Commerce.ProductName()
        };
    }
    
    /// <summary>
    /// Generate a weapon name using JSON data and apply traits.
    /// </summary>
    private static string GenerateWeaponName(Faker f, Item item)
    {
        var data = GameDataService.Instance;
        var weaponType = f.PickRandom("swords", "axes", "bows", "daggers", "spears", "maces", "staves");
        
        var weaponList = weaponType switch
        {
            "swords" => data.WeaponNames.Swords,
            "axes" => data.WeaponNames.Axes,
            "bows" => data.WeaponNames.Bows,
            "daggers" => data.WeaponNames.Daggers,
            "spears" => data.WeaponNames.Spears,
            "maces" => data.WeaponNames.Maces,
            "staves" => data.WeaponNames.Staves,
            _ => data.WeaponNames.Swords
        };
        
        var weaponName = GameDataService.GetRandom(weaponList);
        
        // Apply material traits based on weapon type
        string? materialPrefix = null;
        if (weaponType == "bows" || weaponType == "staves")
        {
            // Bows and staves use wood materials
            materialPrefix = ApplyWoodMaterial(item, f);
        }
        else
        {
            // Other weapons use metal materials
            materialPrefix = ApplyMetalMaterial(item, f);
        }
        
        // Sometimes add a prefix (30% chance) and apply traits
        string? prefixName = null;
        if (f.Random.Bool(0.3f))
        {
            var prefixData = GetPrefixByRarity(item.Rarity);
            if (prefixData != null)
            {
                // Apply traits from prefix
                TraitApplicator.ApplyTraits(item, prefixData.Traits);
                prefixName = prefixData.DisplayName;
            }
        }
        
        // Combine material + prefix + weapon name
        if (materialPrefix != null && prefixName != null)
            return $"{prefixName} {materialPrefix} {weaponName}";
        else if (materialPrefix != null)
            return $"{materialPrefix} {weaponName}";
        else if (prefixName != null)
            return $"{prefixName} {weaponName}";
        else
            return weaponName;
    }
    
    /// <summary>
    /// Get a random armor material based on rarity and apply traits.
    /// </summary>
    private static string GetRandomArmorMaterial(Faker f, Item item)
    {
        var data = GameDataService.Instance;
        
        var materials = item.Rarity switch
        {
            ItemRarity.Common => data.ArmorMaterials.Common,
            ItemRarity.Uncommon => data.ArmorMaterials.Uncommon,
            ItemRarity.Rare => data.ArmorMaterials.Rare,
            ItemRarity.Epic => data.ArmorMaterials.Epic,
            ItemRarity.Legendary => data.ArmorMaterials.Legendary,
            _ => data.ArmorMaterials.Common
        };
        
        if (materials.Count > 0)
        {
            var materialData = GameDataService.GetRandom(materials.Values.ToList());
            TraitApplicator.ApplyTraits(item, materialData.Traits);
            return materialData.DisplayName;
        }
        
        return "Cloth";
    }
    
    /// <summary>
    /// Get a random material from the general materials list.
    /// </summary>
    private static string GetRandomMaterial(ItemRarity rarity)
    {
        var data = GameDataService.Instance;
        
        return rarity switch
        {
            ItemRarity.Common or ItemRarity.Uncommon => GameDataService.GetRandom(data.Materials.Natural),
            ItemRarity.Rare or ItemRarity.Epic => GameDataService.GetRandom(data.Materials.Metals),
            ItemRarity.Legendary => GameDataService.GetRandom(data.Materials.Precious),
            _ => GameDataService.GetRandom(data.Materials.Natural)
        };
    }
    
    /// <summary>
    /// Get a random enchantment suffix and apply traits.
    /// </summary>
    private static string GetRandomEnchantmentSuffix(Faker f, Item item)
    {
        var data = GameDataService.Instance;
        var category = f.PickRandom("power", "protection", "wisdom", "agility", "magic", "fire", "ice", "lightning", "life", "death");
        
        var suffixDict = category switch
        {
            "power" => data.EnchantmentSuffixes.Power,
            "protection" => data.EnchantmentSuffixes.Protection,
            "wisdom" => data.EnchantmentSuffixes.Wisdom,
            "agility" => data.EnchantmentSuffixes.Agility,
            "magic" => data.EnchantmentSuffixes.Magic,
            "fire" => data.EnchantmentSuffixes.Fire,
            "ice" => data.EnchantmentSuffixes.Ice,
            "lightning" => data.EnchantmentSuffixes.Lightning,
            "life" => data.EnchantmentSuffixes.Life,
            "death" => data.EnchantmentSuffixes.Death,
            _ => data.EnchantmentSuffixes.Power
        };
        
        if (suffixDict.Count > 0)
        {
            var suffixData = GameDataService.GetRandom(suffixDict.Values.ToList());
            TraitApplicator.ApplyTraits(item, suffixData.Traits);
            return suffixData.DisplayName;
        }
        
        return "of Power";
    }
    
    /// <summary>
    /// Apply metal material traits to an item and return the material name.
    /// </summary>
    private static string? ApplyMetalMaterial(Item item, Faker f)
    {
        var data = GameDataService.Instance;
        
        if (data.Metals.Count == 0)
            return null;
        
        // Pick appropriate metal based on rarity
        var metalOptions = data.Metals.Values.ToList();
        
        // Filter by rarity - legendary items get legendary materials, etc.
        var appropriateMetals = item.Rarity switch
        {
            ItemRarity.Common => metalOptions.Where(m => !m.Traits.ContainsKey("legendary")).Take(2).ToList(), // Iron, Bronze
            ItemRarity.Uncommon => metalOptions.Where(m => !m.Traits.ContainsKey("legendary")).Skip(1).Take(3).ToList(), // Steel, Silver, Bronze
            ItemRarity.Rare => metalOptions.Where(m => !m.Traits.ContainsKey("legendary")).Skip(2).Take(4).ToList(), // Silver, Mithril, Electrum, etc.
            ItemRarity.Epic => metalOptions.Where(m => !m.Traits.ContainsKey("cursed") || f.Random.Bool(0.3f)).Skip(3).ToList(), // Higher tier metals
            ItemRarity.Legendary => metalOptions.Where(m => m.Traits.ContainsKey("legendary") || f.Random.Bool(0.5f)).ToList(), // Legendary materials
            _ => metalOptions.Take(2).ToList()
        };
        
        if (appropriateMetals.Count == 0)
            appropriateMetals = metalOptions.Take(3).ToList(); // Fallback
        
        var metal = f.PickRandom(appropriateMetals);
        TraitApplicator.ApplyTraits(item, metal.Traits);
        
        return metal.DisplayName;
    }
    
    /// <summary>
    /// Apply wood material traits to an item and return the material name.
    /// </summary>
    private static string? ApplyWoodMaterial(Item item, Faker f)
    {
        var data = GameDataService.Instance;
        
        if (data.Woods.Count == 0)
            return null;
        
        // Pick appropriate wood based on rarity
        var woodOptions = data.Woods.Values.ToList();
        
        var appropriateWoods = item.Rarity switch
        {
            ItemRarity.Common => woodOptions.Where(w => !w.Traits.ContainsKey("legendary")).Take(2).ToList(), // Oak, Ash
            ItemRarity.Uncommon => woodOptions.Where(w => !w.Traits.ContainsKey("legendary")).Skip(1).Take(3).ToList(), // Ash, Yew, Maple
            ItemRarity.Rare => woodOptions.Where(w => !w.Traits.ContainsKey("legendary")).Skip(3).Take(4).ToList(), // Ironwood, Ebony, etc.
            ItemRarity.Epic => woodOptions.Where(w => !w.Traits.ContainsKey("shadowBlend") || f.Random.Bool(0.3f)).Skip(4).ToList(),
            ItemRarity.Legendary => woodOptions.Where(w => w.Traits.ContainsKey("legendary") || f.Random.Bool(0.5f)).ToList(),
            _ => woodOptions.Take(2).ToList()
        };
        
        if (appropriateWoods.Count == 0)
            appropriateWoods = woodOptions.Take(3).ToList(); // Fallback
        
        var wood = f.PickRandom(appropriateWoods);
        TraitApplicator.ApplyTraits(item, wood.Traits);
        
        return wood.DisplayName;
    }
    
    /// <summary>
    /// Apply leather material traits to an item and return the material name.
    /// </summary>
    private static string? ApplyLeatherMaterial(Item item, Faker f)
    {
        var data = GameDataService.Instance;
        
        if (data.Leathers.Count == 0)
            return null;
        
        // Pick appropriate leather based on rarity
        var leatherOptions = data.Leathers.Values.ToList();
        
        var appropriateLeathers = item.Rarity switch
        {
            ItemRarity.Common => leatherOptions.Where(l => !l.Traits.ContainsKey("legendary")).Take(2).ToList(), // Hide, Leather
            ItemRarity.Uncommon => leatherOptions.Where(l => !l.Traits.ContainsKey("legendary")).Skip(1).Take(3).ToList(), // Leather, Studded, Hardened
            ItemRarity.Rare => leatherOptions.Where(l => !l.Traits.ContainsKey("legendary")).Skip(3).Take(3).ToList(), // Drake, Shadow, Chimera
            ItemRarity.Epic => leatherOptions.Where(l => !l.Traits.ContainsKey("cursed") || f.Random.Bool(0.3f)).Skip(4).ToList(),
            ItemRarity.Legendary => leatherOptions.Where(l => l.Traits.ContainsKey("legendary") || f.Random.Bool(0.5f)).ToList(),
            _ => leatherOptions.Take(2).ToList()
        };
        
        if (appropriateLeathers.Count == 0)
            appropriateLeathers = leatherOptions.Take(3).ToList(); // Fallback
        
        var leather = f.PickRandom(appropriateLeathers);
        TraitApplicator.ApplyTraits(item, leather.Traits);
        
        return leather.DisplayName;
    }
    
    /// <summary>
    /// Apply gemstone material traits to an item and return the material name.
    /// </summary>
    private static string? ApplyGemstoneMaterial(Item item, Faker f)
    {
        var data = GameDataService.Instance;
        
        if (data.Gemstones.Count == 0)
            return null;
        
        // Pick appropriate gemstone based on rarity
        var gemOptions = data.Gemstones.Values.ToList();
        
        var appropriateGems = item.Rarity switch
        {
            ItemRarity.Common => gemOptions.Where(g => !g.Traits.ContainsKey("legendary")).Take(3).ToList(), // Ruby, Topaz, Obsidian
            ItemRarity.Uncommon => gemOptions.Where(g => !g.Traits.ContainsKey("legendary")).Skip(1).Take(4).ToList(), // Sapphire, Amethyst, etc.
            ItemRarity.Rare => gemOptions.Where(g => !g.Traits.ContainsKey("legendary")).Skip(3).Take(4).ToList(), // Diamond, Emerald, Opal, etc.
            ItemRarity.Epic => gemOptions.Where(g => !g.Traits.ContainsKey("cursed") || f.Random.Bool(0.3f)).Skip(5).ToList(),
            ItemRarity.Legendary => gemOptions.Where(g => g.Traits.ContainsKey("legendary") || g.Traits.ContainsKey("artifact")).ToList(),
            _ => gemOptions.Take(3).ToList()
        };
        
        if (appropriateGems.Count == 0)
            appropriateGems = gemOptions.Take(3).ToList(); // Fallback
        
        var gem = f.PickRandom(appropriateGems);
        TraitApplicator.ApplyTraits(item, gem.Traits);
        
        return gem.DisplayName;
    }
    
    /// <summary>
    /// Get weapon prefix by rarity and return trait data.
    /// </summary>
    private static WeaponPrefixTraitData? GetPrefixByRarity(ItemRarity rarity)
    {
        var data = GameDataService.Instance;
        
        var prefixes = rarity switch
        {
            ItemRarity.Common => data.WeaponPrefixes.Common,
            ItemRarity.Uncommon => data.WeaponPrefixes.Uncommon,
            ItemRarity.Rare => data.WeaponPrefixes.Rare,
            ItemRarity.Epic => data.WeaponPrefixes.Epic,
            ItemRarity.Legendary => data.WeaponPrefixes.Legendary,
            _ => data.WeaponPrefixes.Common
        };
        
        return prefixes.Count > 0 ? GameDataService.GetRandom(prefixes.Values.ToList()) : null;
    }

    /// <summary>
    /// Generate stat bonuses for an item based on its type and rarity.
    /// </summary>
    private static int GenerateStatBonus(Faker f, Item item, string statType)
    {
        // Consumables and quest items don't give stat bonuses
        if (item.Type == ItemType.Consumable || item.Type == ItemType.QuestItem)
            return 0;

        // Base bonus range based on rarity
        int minBonus = item.Rarity switch
        {
            ItemRarity.Common => 0,
            ItemRarity.Uncommon => 1,
            ItemRarity.Rare => 2,
            ItemRarity.Epic => 5,
            ItemRarity.Legendary => 10,
            _ => 0
        };

        int maxBonus = item.Rarity switch
        {
            ItemRarity.Common => 2,
            ItemRarity.Uncommon => 5,
            ItemRarity.Rare => 10,
            ItemRarity.Epic => 20,
            ItemRarity.Legendary => 50,
            _ => 2
        };

        // Item type affects which stats it prefers
        var primaryStat = GetPrimaryStatForItemType(item.Type);
        
        // If this is the primary stat for this item type, give higher bonuses
        if (primaryStat == statType)
        {
            return f.Random.Int(minBonus, maxBonus);
        }
        
        // Otherwise, only occasionally give bonuses (30% chance)
        if (f.Random.Bool(0.3f))
        {
            return f.Random.Int(0, maxBonus / 2);
        }
        
        return 0;
    }

    /// <summary>
    /// Get the primary stat for an item type.
    /// </summary>
    private static string GetPrimaryStatForItemType(ItemType type)
    {
        return type switch
        {
            // Weapons - Strength (melee) or Intelligence (magic)
            ItemType.Weapon => "Strength",
            
            // Shields and heavy armor - Constitution
            ItemType.Shield => "Constitution",
            ItemType.Helmet => "Constitution",
            ItemType.Shoulders => "Constitution",
            ItemType.Chest => "Constitution",
            ItemType.Bracers => "Constitution",
            ItemType.Belt => "Constitution",
            ItemType.Legs => "Constitution",
            
            // Light armor - Dexterity
            ItemType.Gloves => "Dexterity",
            ItemType.Boots => "Dexterity",
            
            // Off-hand items - Intelligence or Wisdom
            ItemType.OffHand => "Wisdom",
            
            // Jewelry - Mixed stats (can roll any)
            ItemType.Necklace => "Strength",
            ItemType.Ring => "Charisma",
            
            _ => "Constitution"
        };
    }
}
