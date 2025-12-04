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
            .RuleFor(i => i.Type, type);

        return typedFaker.Generate(count);
    }

    private static string GenerateNameForType(Faker f, ItemType type)
    {
        return type switch
        {
            ItemType.Weapon => $"{f.PickRandom("Iron", "Steel", "Mythril", "Dragon")} {f.PickRandom("Sword", "Axe", "Bow", "Dagger")}",
            ItemType.Armor => $"{f.PickRandom("Leather", "Chain", "Plate", "Dragon")} {f.PickRandom("Helmet", "Chest", "Legs", "Boots")}",
            ItemType.Consumable => $"{f.PickRandom("Health", "Mana", "Stamina")} Potion",
            ItemType.Accessory => $"{f.PickRandom("Ring", "Amulet", "Bracelet")} of {f.PickRandom("Strength", "Wisdom", "Agility")}",
            ItemType.QuestItem => $"{f.Lorem.Word()} {f.PickRandom("Key", "Scroll", "Crystal", "Relic")}",
            _ => f.Commerce.ProductName()
        };
    }
}
