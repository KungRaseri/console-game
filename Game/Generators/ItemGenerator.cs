using Bogus;
using Game.Models;

namespace Game.Generators;

/// <summary>
/// Generates random items using Bogus.
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
            .RuleFor(i => i.Name, f => GenerateNameForType(f, type))
            .RuleFor(i => i.Description, f => f.Commerce.ProductDescription())
            .RuleFor(i => i.Price, f => f.Random.Int(5, 1000))
            .RuleFor(i => i.Rarity, f => f.PickRandom<ItemRarity>())
            .RuleFor(i => i.Type, type)
            .RuleFor(i => i.IsTwoHanded, (f, item) => 
                item.Type == ItemType.Weapon && f.Random.Bool(0.3f)) // 30% chance for two-handed weapons
            .RuleFor(i => i.BonusStrength, (f, item) => GenerateStatBonus(f, item, "Strength"))
            .RuleFor(i => i.BonusDexterity, (f, item) => GenerateStatBonus(f, item, "Dexterity"))
            .RuleFor(i => i.BonusConstitution, (f, item) => GenerateStatBonus(f, item, "Constitution"))
            .RuleFor(i => i.BonusIntelligence, (f, item) => GenerateStatBonus(f, item, "Intelligence"))
            .RuleFor(i => i.BonusWisdom, (f, item) => GenerateStatBonus(f, item, "Wisdom"))
            .RuleFor(i => i.BonusCharisma, (f, item) => GenerateStatBonus(f, item, "Charisma"));

        return typedFaker.Generate(count);
    }

    private static string GenerateNameForType(Faker f, ItemType type)
    {
        return type switch
        {
            // Weapons
            ItemType.Weapon => $"{f.PickRandom("Iron", "Steel", "Mythril", "Dragon")} {f.PickRandom("Sword", "Axe", "Bow", "Dagger", "Spear", "Mace")}",
            ItemType.Shield => $"{f.PickRandom("Wooden", "Iron", "Steel", "Tower", "Kite")} Shield",
            ItemType.OffHand => $"{f.PickRandom("Tome", "Orb", "Crystal", "Focus")} of {f.PickRandom("Power", "Wisdom", "Light")}",
            
            // Armor
            ItemType.Helmet => $"{f.PickRandom("Leather", "Chain", "Plate", "Dragon")} Helmet",
            ItemType.Shoulders => $"{f.PickRandom("Leather", "Chain", "Plate", "Scaled")} Shoulderpads",
            ItemType.Chest => $"{f.PickRandom("Leather", "Chain", "Plate", "Dragon")} Chestpiece",
            ItemType.Bracers => $"{f.PickRandom("Leather", "Chain", "Steel", "Mithril")} Bracers",
            ItemType.Gloves => $"{f.PickRandom("Leather", "Chain", "Plate", "Silk")} Gloves",
            ItemType.Belt => $"{f.PickRandom("Leather", "Studded", "Chain", "Plate")} Belt",
            ItemType.Legs => $"{f.PickRandom("Leather", "Chain", "Plate", "Dragon")} Leggings",
            ItemType.Boots => $"{f.PickRandom("Leather", "Chain", "Plate", "Dragon")} Boots",
            
            // Jewelry
            ItemType.Necklace => $"{f.PickRandom("Amulet", "Necklace", "Pendant")} of {f.PickRandom("Strength", "Wisdom", "Agility", "Power")}",
            ItemType.Ring => $"{f.PickRandom("Ring", "Band", "Signet")} of {f.PickRandom("Strength", "Wisdom", "Agility", "Protection", "Power")}",
            
            // Other
            ItemType.Consumable => $"{f.PickRandom("Health", "Mana", "Stamina")} Potion",
            ItemType.QuestItem => $"{f.Lorem.Word()} {f.PickRandom("Key", "Scroll", "Crystal", "Relic")}",
            
            _ => f.Commerce.ProductName()
        };
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
