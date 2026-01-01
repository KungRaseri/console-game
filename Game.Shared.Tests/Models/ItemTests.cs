using FluentAssertions;
using Game.Shared.Models;

namespace Game.Tests.Models;

[Trait("Category", "Unit")]
public class ItemTests
{
    [Fact]
    public void Item_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var item = new Item();

        // Assert
        item.Id.Should().NotBeNullOrEmpty();
        item.Name.Should().Be(string.Empty);
        item.Description.Should().Be(string.Empty);
        item.Price.Should().Be(0);
        item.Rarity.Should().Be(ItemRarity.Common);
        item.Type.Should().Be(ItemType.Consumable);
    }

    [Fact]
    public void Item_Should_Generate_Unique_Ids()
    {
        // Arrange & Act
        var item1 = new Item();
        var item2 = new Item();

        // Assert
        item1.Id.Should().NotBe(item2.Id);
    }

    [Fact]
    public void Item_Properties_Should_Be_Settable()
    {
        // Arrange
        var item = new Item();

        // Act
        item.Name = "Health Potion";
        item.Description = "Restores 50 HP";
        item.Price = 25;
        item.Rarity = ItemRarity.Rare;
        item.Type = ItemType.Consumable;

        // Assert
        item.Name.Should().Be("Health Potion");
        item.Description.Should().Be("Restores 50 HP");
        item.Price.Should().Be(25);
        item.Rarity.Should().Be(ItemRarity.Rare);
        item.Type.Should().Be(ItemType.Consumable);
    }

    [Theory]
    [InlineData(ItemRarity.Common)]
    [InlineData(ItemRarity.Uncommon)]
    [InlineData(ItemRarity.Rare)]
    [InlineData(ItemRarity.Epic)]
    [InlineData(ItemRarity.Legendary)]
    public void Item_Should_Accept_All_Rarity_Levels(ItemRarity rarity)
    {
        // Arrange & Act
        var item = new Item { Rarity = rarity };

        // Assert
        item.Rarity.Should().Be(rarity);
    }

    [Theory]
    [InlineData(ItemType.Weapon)]
    [InlineData(ItemType.Chest)]
    [InlineData(ItemType.Consumable)]
    [InlineData(ItemType.QuestItem)]
    [InlineData(ItemType.Ring)]
    public void Item_Should_Accept_All_itemTypes(ItemType type)
    {
        // Arrange & Act
        var item = new Item { Type = type };

        // Assert
        item.Type.Should().Be(type);
    }

    // GetTotalBonus* Calculation Tests
    [Fact]
    public void GetTotalBonusStrength_Should_Return_Base_Bonus_When_No_Enchantments_Or_Upgrades()
    {
        var item = new Item { BonusStrength = 5 };
        item.GetTotalBonusStrength().Should().Be(5);
    }

    [Fact]
    public void GetTotalBonusStrength_Should_Include_Upgrade_Level_Bonus()
    {
        var item = new Item { BonusStrength = 3, UpgradeLevel = 2 };
        item.GetTotalBonusStrength().Should().Be(7); // 3 + (2*2)
    }

    [Fact]
    public void GetTotalBonusStrength_Should_Include_Enchantment_Bonuses()
    {
        var item = new Item
        {
            BonusStrength = 2,
            Enchantments = new List<Enchantment>
            {
                new Enchantment { BonusStrength = 3 },
                new Enchantment { BonusStrength = 5 }
            }
        };
        item.GetTotalBonusStrength().Should().Be(10); // 2 + 3 + 5
    }

    [Fact]
    public void GetTotalBonusStrength_Should_Combine_All_Sources()
    {
        var item = new Item
        {
            BonusStrength = 5,
            UpgradeLevel = 3,
            Enchantments = new List<Enchantment>
            {
                new Enchantment { BonusStrength = 4 },
                new Enchantment { BonusStrength = 2 }
            }
        };
        item.GetTotalBonusStrength().Should().Be(17); // 5 + 6 + 4 + 2
    }

    [Fact]
    public void GetTotalBonusDexterity_Should_Combine_All_Sources()
    {
        var item = new Item
        {
            BonusDexterity = 3,
            UpgradeLevel = 2,
            Enchantments = new List<Enchantment> { new Enchantment { BonusDexterity = 5 } }
        };
        item.GetTotalBonusDexterity().Should().Be(12); // 3 + 4 + 5
    }

    [Fact]
    public void GetTotalBonusConstitution_Should_Combine_All_Sources()
    {
        var item = new Item
        {
            BonusConstitution = 6,
            UpgradeLevel = 1,
            Enchantments = new List<Enchantment>
            {
                new Enchantment { BonusConstitution = 3 },
                new Enchantment { BonusConstitution = 1 }
            }
        };
        item.GetTotalBonusConstitution().Should().Be(12); // 6 + 2 + 3 + 1
    }

    [Fact]
    public void GetTotalBonusIntelligence_Should_Combine_All_Sources()
    {
        var item = new Item
        {
            BonusIntelligence = 4,
            UpgradeLevel = 5,
            Enchantments = new List<Enchantment> { new Enchantment { BonusIntelligence = 7 } }
        };
        item.GetTotalBonusIntelligence().Should().Be(21); // 4 + 10 + 7
    }

    [Fact]
    public void GetTotalBonusWisdom_Should_Combine_All_Sources()
    {
        var item = new Item
        {
            BonusWisdom = 5,
            UpgradeLevel = 3,
            Enchantments = new List<Enchantment>
            {
                new Enchantment { BonusWisdom = 2 },
                new Enchantment { BonusWisdom = 4 }
            }
        };
        item.GetTotalBonusWisdom().Should().Be(17); // 5 + 6 + 2 + 4
    }

    [Fact]
    public void GetTotalBonusCharisma_Should_Combine_All_Sources()
    {
        var item = new Item
        {
            BonusCharisma = 3,
            UpgradeLevel = 4,
            Enchantments = new List<Enchantment> { new Enchantment { BonusCharisma = 6 } }
        };
        item.GetTotalBonusCharisma().Should().Be(17); // 3 + 8 + 6
    }

    // GetDisplayName Tests
    [Fact]
    public void GetDisplayName_Should_Return_Name_When_No_Upgrades_Or_Enchantments()
    {
        var item = new Item { Name = "Iron Sword" };
        item.GetDisplayName().Should().Be("Iron Sword");
    }

    [Fact]
    public void GetDisplayName_Should_Include_Upgrade_Level_Prefix()
    {
        var item = new Item { Name = "Steel Helmet", UpgradeLevel = 3 };
        item.GetDisplayName().Should().Be("+3 Steel Helmet");
    }

    [Fact]
    public void GetDisplayName_Should_Include_Enchantment_Suffixes()
    {
        var item = new Item
        {
            Name = "Leather Boots",
            Enchantments = new List<Enchantment>
            {
                new Enchantment { Name = "Swiftness" },
                new Enchantment { Name = "Stamina" }
            }
        };
        item.GetDisplayName().Should().Be("Leather Boots (Swiftness) (Stamina)");
    }

    [Fact]
    public void GetDisplayName_Should_Combine_Upgrade_And_Enchantments()
    {
        var item = new Item
        {
            Name = "Mythril Armor",
            UpgradeLevel = 5,
            Enchantments = new List<Enchantment>
            {
                new Enchantment { Name = "Fire Resistance" },
                new Enchantment { Name = "Strength" }
            }
        };
        item.GetDisplayName().Should().Be("+5 Mythril Armor (Fire Resistance) (Strength)");
    }

    // Theory Tests
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    public void Upgrade_Levels_Should_Add_Two_Per_Level(int upgradeLevel)
    {
        var item = new Item
        {
            BonusStrength = 5,
            BonusDexterity = 3,
            UpgradeLevel = upgradeLevel
        };
        item.GetTotalBonusStrength().Should().Be(5 + (upgradeLevel * 2));
        item.GetTotalBonusDexterity().Should().Be(3 + (upgradeLevel * 2));
    }

    [Fact]
    public void All_Bonus_Methods_Should_Handle_Empty_Enchantments()
    {
        var item = new Item
        {
            BonusStrength = 10,
            BonusDexterity = 8,
            BonusConstitution = 6,
            BonusIntelligence = 4,
            BonusWisdom = 2,
            BonusCharisma = 1
        };
        item.GetTotalBonusStrength().Should().Be(10);
        item.GetTotalBonusDexterity().Should().Be(8);
        item.GetTotalBonusConstitution().Should().Be(6);
        item.GetTotalBonusIntelligence().Should().Be(4);
        item.GetTotalBonusWisdom().Should().Be(2);
        item.GetTotalBonusCharisma().Should().Be(1);
    }
}
