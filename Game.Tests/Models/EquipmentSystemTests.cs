using FluentAssertions;
using Game.Core.Models;
using Xunit;

namespace Game.Tests.Models;

/// <summary>
/// Tests for the 13-slot equipment system
/// </summary>
public class EquipmentSystemTests
{
    [Fact]
    public void Character_Should_Have_All_13_Equipment_Slots()
    {
        // Arrange & Act
        var character = new Character { Name = "Test", Level = 1 };

        // Assert - Weapon slots
        character.EquippedMainHand.Should().BeNull();
        character.EquippedOffHand.Should().BeNull();

        // Assert - Armor slots
        character.EquippedHelmet.Should().BeNull();
        character.EquippedShoulders.Should().BeNull();
        character.EquippedChest.Should().BeNull();
        character.EquippedBracers.Should().BeNull();
        character.EquippedGloves.Should().BeNull();
        character.EquippedBelt.Should().BeNull();
        character.EquippedLegs.Should().BeNull();
        character.EquippedBoots.Should().BeNull();

        // Assert - Jewelry slots
        character.EquippedNecklace.Should().BeNull();
        character.EquippedRing1.Should().BeNull();
        character.EquippedRing2.Should().BeNull();
    }

    [Fact]
    public void Character_Should_Equip_MainHand_Weapon()
    {
        // Arrange
        var character = new Character { Name = "Warrior", Level = 1 };
        var sword = new Item { Name = "Iron Sword", Type = ItemType.Weapon };

        // Act
        character.EquippedMainHand = sword;

        // Assert
        character.EquippedMainHand.Should().NotBeNull();
        character.EquippedMainHand.Name.Should().Be("Iron Sword");
        character.EquippedMainHand.Type.Should().Be(ItemType.Weapon);
    }

    [Fact]
    public void Character_Should_Equip_OffHand_Shield()
    {
        // Arrange
        var character = new Character { Name = "Knight", Level = 1 };
        var shield = new Item { Name = "Wooden Shield", Type = ItemType.Shield };

        // Act
        character.EquippedOffHand = shield;

        // Assert
        character.EquippedOffHand.Should().NotBeNull();
        character.EquippedOffHand.Name.Should().Be("Wooden Shield");
        character.EquippedOffHand.Type.Should().Be(ItemType.Shield);
    }

    [Fact]
    public void Character_Should_Equip_All_Armor_Pieces()
    {
        // Arrange
        var character = new Character { Name = "Paladin", Level = 1 };

        // Act
        character.EquippedHelmet = new Item { Name = "Iron Helm", Type = ItemType.Helmet };
        character.EquippedShoulders = new Item { Name = "Steel Pauldrons", Type = ItemType.Shoulders };
        character.EquippedChest = new Item { Name = "Plate Chestpiece", Type = ItemType.Chest };
        character.EquippedBracers = new Item { Name = "Leather Bracers", Type = ItemType.Bracers };
        character.EquippedGloves = new Item { Name = "Chain Gauntlets", Type = ItemType.Gloves };
        character.EquippedBelt = new Item { Name = "Studded Belt", Type = ItemType.Belt };
        character.EquippedLegs = new Item { Name = "Plate Greaves", Type = ItemType.Legs };
        character.EquippedBoots = new Item { Name = "Iron Boots", Type = ItemType.Boots };

        // Assert
        character.EquippedHelmet.Should().NotBeNull();
        character.EquippedShoulders.Should().NotBeNull();
        character.EquippedChest.Should().NotBeNull();
        character.EquippedBracers.Should().NotBeNull();
        character.EquippedGloves.Should().NotBeNull();
        character.EquippedBelt.Should().NotBeNull();
        character.EquippedLegs.Should().NotBeNull();
        character.EquippedBoots.Should().NotBeNull();
    }

    [Fact]
    public void Character_Should_Equip_Two_Different_Rings()
    {
        // Arrange
        var character = new Character { Name = "Mage", Level = 1 };
        var ring1 = new Item { Name = "Ring of Fire", Type = ItemType.Ring };
        var ring2 = new Item { Name = "Ring of Ice", Type = ItemType.Ring };

        // Act
        character.EquippedRing1 = ring1;
        character.EquippedRing2 = ring2;

        // Assert
        character.EquippedRing1.Should().NotBeNull();
        character.EquippedRing1.Name.Should().Be("Ring of Fire");
        character.EquippedRing2.Should().NotBeNull();
        character.EquippedRing2.Name.Should().Be("Ring of Ice");
    }

    [Fact]
    public void Character_Should_Equip_Necklace()
    {
        // Arrange
        var character = new Character { Name = "Priest", Level = 1 };
        var necklace = new Item { Name = "Holy Amulet", Type = ItemType.Necklace };

        // Act
        character.EquippedNecklace = necklace;

        // Assert
        character.EquippedNecklace.Should().NotBeNull();
        character.EquippedNecklace.Name.Should().Be("Holy Amulet");
        character.EquippedNecklace.Type.Should().Be(ItemType.Necklace);
    }

    [Fact]
    public void Character_Should_Unequip_Item_By_Setting_Null()
    {
        // Arrange
        var character = new Character { Name = "Ranger", Level = 1 };
        character.EquippedMainHand = new Item { Name = "Bow", Type = ItemType.Weapon };
        character.EquippedHelmet = new Item { Name = "Leather Hood", Type = ItemType.Helmet };

        // Act
        character.EquippedMainHand = null;
        character.EquippedHelmet = null;

        // Assert
        character.EquippedMainHand.Should().BeNull();
        character.EquippedHelmet.Should().BeNull();
    }

    [Fact]
    public void Character_Should_Replace_Equipment_When_Equipping_Same_Slot()
    {
        // Arrange
        var character = new Character { Name = "Thief", Level = 1 };
        var oldSword = new Item { Name = "Rusty Dagger", Type = ItemType.Weapon };
        var newSword = new Item { Name = "Steel Dagger", Type = ItemType.Weapon };

        character.EquippedMainHand = oldSword;

        // Act
        character.EquippedMainHand = newSword;

        // Assert
        character.EquippedMainHand.Should().NotBeNull();
        character.EquippedMainHand.Name.Should().Be("Steel Dagger");
        character.EquippedMainHand.Should().NotBeSameAs(oldSword);
    }

    [Theory]
    [InlineData(ItemType.Weapon)]
    [InlineData(ItemType.Shield)]
    [InlineData(ItemType.OffHand)]
    [InlineData(ItemType.Helmet)]
    [InlineData(ItemType.Shoulders)]
    [InlineData(ItemType.Chest)]
    [InlineData(ItemType.Bracers)]
    [InlineData(ItemType.Gloves)]
    [InlineData(ItemType.Belt)]
    [InlineData(ItemType.Legs)]
    [InlineData(ItemType.Boots)]
    [InlineData(ItemType.Necklace)]
    [InlineData(ItemType.Ring)]
    public void ItemType_Should_Include_All_Equipment_Types(ItemType type)
    {
        // Arrange & Act
        var item = new Item { Name = "Test Item", Type = type };

        // Assert
        item.Type.Should().Be(type);
        Enum.IsDefined(typeof(ItemType), type).Should().BeTrue();
    }

    [Fact]
    public void ItemType_Should_Have_15_Total_Types()
    {
        // Arrange & Act
        var allTypes = Enum.GetValues<ItemType>();

        // Assert
        allTypes.Should().HaveCount(15);
        allTypes.Should().Contain(new[]
        {
            ItemType.Consumable,
            ItemType.Weapon,
            ItemType.Shield,
            ItemType.OffHand,
            ItemType.Helmet,
            ItemType.Shoulders,
            ItemType.Chest,
            ItemType.Bracers,
            ItemType.Gloves,
            ItemType.Belt,
            ItemType.Legs,
            ItemType.Boots,
            ItemType.Necklace,
            ItemType.Ring,
            ItemType.QuestItem
        });
    }

    [Fact]
    public void Character_Should_Support_Full_Equipment_Set()
    {
        // Arrange
        var character = new Character { Name = "Fully Equipped Hero", Level = 10 };

        // Act - Equip full set
        character.EquippedMainHand = new Item { Name = "Legendary Sword", Type = ItemType.Weapon };
        character.EquippedOffHand = new Item { Name = "Dragon Shield", Type = ItemType.Shield };
        character.EquippedHelmet = new Item { Name = "Crown of Kings", Type = ItemType.Helmet };
        character.EquippedShoulders = new Item { Name = "Titan Pauldrons", Type = ItemType.Shoulders };
        character.EquippedChest = new Item { Name = "Dragonscale Plate", Type = ItemType.Chest };
        character.EquippedBracers = new Item { Name = "Mithril Bracers", Type = ItemType.Bracers };
        character.EquippedGloves = new Item { Name = "Gauntlets of Might", Type = ItemType.Gloves };
        character.EquippedBelt = new Item { Name = "Belt of Giants", Type = ItemType.Belt };
        character.EquippedLegs = new Item { Name = "Adamantite Greaves", Type = ItemType.Legs };
        character.EquippedBoots = new Item { Name = "Boots of Speed", Type = ItemType.Boots };
        character.EquippedNecklace = new Item { Name = "Amulet of Power", Type = ItemType.Necklace };
        character.EquippedRing1 = new Item { Name = "Ring of Strength", Type = ItemType.Ring };
        character.EquippedRing2 = new Item { Name = "Ring of Wisdom", Type = ItemType.Ring };

        // Assert - All slots filled
        character.EquippedMainHand.Should().NotBeNull();
        character.EquippedOffHand.Should().NotBeNull();
        character.EquippedHelmet.Should().NotBeNull();
        character.EquippedShoulders.Should().NotBeNull();
        character.EquippedChest.Should().NotBeNull();
        character.EquippedBracers.Should().NotBeNull();
        character.EquippedGloves.Should().NotBeNull();
        character.EquippedBelt.Should().NotBeNull();
        character.EquippedLegs.Should().NotBeNull();
        character.EquippedBoots.Should().NotBeNull();
        character.EquippedNecklace.Should().NotBeNull();
        character.EquippedRing1.Should().NotBeNull();
        character.EquippedRing2.Should().NotBeNull();

        // Assert - 13 total equipped items
        var equippedCount = 0;
        if (character.EquippedMainHand != null) equippedCount++;
        if (character.EquippedOffHand != null) equippedCount++;
        if (character.EquippedHelmet != null) equippedCount++;
        if (character.EquippedShoulders != null) equippedCount++;
        if (character.EquippedChest != null) equippedCount++;
        if (character.EquippedBracers != null) equippedCount++;
        if (character.EquippedGloves != null) equippedCount++;
        if (character.EquippedBelt != null) equippedCount++;
        if (character.EquippedLegs != null) equippedCount++;
        if (character.EquippedBoots != null) equippedCount++;
        if (character.EquippedNecklace != null) equippedCount++;
        if (character.EquippedRing1 != null) equippedCount++;
        if (character.EquippedRing2 != null) equippedCount++;

        equippedCount.Should().Be(13);
    }
}
