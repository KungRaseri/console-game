using Bogus;
using Game.Data.Models;
using Game.Models;
using Game.Services;

namespace Game.Generators;

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
            ItemType.Shield => $"{GetRandomMaterial(ItemRarity.Common)} Shield",
            ItemType.OffHand => $"{f.PickRandom("Tome", "Orb", "Crystal", "Focus")} {GetRandomEnchantmentSuffix(f, item)}",
            
            // Armor - use JSON materials
            ItemType.Helmet => $"{GetRandomArmorMaterial(f, item)} Helmet",
            ItemType.Shoulders => $"{GetRandomArmorMaterial(f, item)} Shoulderpads",
            ItemType.Chest => $"{GetRandomArmorMaterial(f, item)} Chestpiece",
            ItemType.Bracers => $"{GetRandomArmorMaterial(f, item)} Bracers",
            ItemType.Gloves => $"{GetRandomArmorMaterial(f, item)} Gloves",
            ItemType.Belt => $"{GetRandomArmorMaterial(f, item)} Belt",
            ItemType.Legs => $"{GetRandomArmorMaterial(f, item)} Leggings",
            ItemType.Boots => $"{GetRandomArmorMaterial(f, item)} Boots",
            
            // Jewelry - use enchantment suffixes
            ItemType.Necklace => $"{f.PickRandom("Amulet", "Necklace", "Pendant")} {GetRandomEnchantmentSuffix(f, item)}",
            ItemType.Ring => $"{f.PickRandom("Ring", "Band", "Signet")} {GetRandomEnchantmentSuffix(f, item)}",
            
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
        
        // Sometimes add a prefix (30% chance) and apply traits
        if (f.Random.Bool(0.3f))
        {
            var prefixData = GetPrefixByRarity(item.Rarity);
            if (prefixData != null)
            {
                // Apply traits from prefix
                TraitApplicator.ApplyTraits(item, prefixData.Traits);
                return $"{prefixData.DisplayName} {weaponName}";
            }
        }
        
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
