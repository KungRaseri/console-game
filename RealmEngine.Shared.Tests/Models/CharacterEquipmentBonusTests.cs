using FluentAssertions;
using RealmEngine.Shared.Models;

namespace RealmEngine.Shared.Tests.Models;

[Trait("Category", "Unit")]
/// <summary>
/// Additional comprehensive tests for Character model focusing on equipment bonuses and derived stats.
/// </summary>
public class CharacterEquipmentBonusTests
{
    #region Total Strength Tests

    [Fact]
    public void GetTotalStrength_Should_Return_Base_Strength_With_No_Equipment()
    {
        // Arrange
        var character = new Character { Strength = 15 };

        // Act
        var totalStrength = character.GetTotalStrength();

        // Assert
        totalStrength.Should().Be(15);
    }

    [Fact]
    public void GetTotalStrength_Should_Include_Equipment_Bonuses()
    {
        // Arrange
        var character = new Character { Strength = 10 };
        character.EquippedMainHand = new Item
        {
            Name = "Mighty Sword",
            BonusStrength = 5
        };
        character.EquippedChest = new Item
        {
            Name = "Warrior Plate",
            BonusStrength = 3
        };

        // Act
        var totalStrength = character.GetTotalStrength();

        // Assert
        totalStrength.Should().Be(18); // 10 + 5 + 3
    }

    [Fact]
    public void GetTotalStrength_Should_Include_Enchantment_Bonuses()
    {
        // Arrange
        var character = new Character { Strength = 10 };
        character.EquippedHelmet = new Item
        {
            Name = "Enchanted Helm",
            Enchantments = new List<Enchantment>
            {
                new Enchantment { Name = "Strength +2", BonusStrength = 2 }
            }
        };

        // Act
        var totalStrength = character.GetTotalStrength();

        // Assert
        totalStrength.Should().Be(12); // 10 + 2
    }

    [Fact]
    public void GetTotalStrength_Should_Include_All_Equipment_Slots()
    {
        // Arrange
        var character = new Character { Strength = 10 };
        character.EquippedMainHand = new Item { BonusStrength = 1 };
        character.EquippedOffHand = new Item { BonusStrength = 1 };
        character.EquippedHelmet = new Item { BonusStrength = 1 };
        character.EquippedShoulders = new Item { BonusStrength = 1 };
        character.EquippedChest = new Item { BonusStrength = 1 };
        character.EquippedBracers = new Item { BonusStrength = 1 };
        character.EquippedGloves = new Item { BonusStrength = 1 };
        character.EquippedBelt = new Item { BonusStrength = 1 };
        character.EquippedLegs = new Item { BonusStrength = 1 };
        character.EquippedBoots = new Item { BonusStrength = 1 };
        character.EquippedNecklace = new Item { BonusStrength = 1 };
        character.EquippedRing1 = new Item { BonusStrength = 1 };
        character.EquippedRing2 = new Item { BonusStrength = 1 };

        // Act
        var totalStrength = character.GetTotalStrength();

        // Assert
        totalStrength.Should().Be(23); // 10 + 13 (all slots)
    }

    #endregion

    #region Total Dexterity Tests

    [Fact]
    public void GetTotalDexterity_Should_Return_Base_Dexterity_With_No_Equipment()
    {
        // Arrange
        var character = new Character { Dexterity = 12 };

        // Act
        var totalDexterity = character.GetTotalDexterity();

        // Assert
        totalDexterity.Should().Be(12);
    }

    [Fact]
    public void GetTotalDexterity_Should_Include_Equipment_Bonuses()
    {
        // Arrange
        var character = new Character { Dexterity = 10 };
        character.EquippedGloves = new Item { BonusDexterity = 4 };
        character.EquippedBoots = new Item { BonusDexterity = 3 };

        // Act
        var totalDexterity = character.GetTotalDexterity();

        // Assert
        totalDexterity.Should().Be(17); // 10 + 4 + 3
    }

    #endregion

    #region Total Constitution Tests

    [Fact]
    public void GetTotalConstitution_Should_Return_Base_Constitution_With_No_Equipment()
    {
        // Arrange
        var character = new Character { Constitution = 14 };

        // Act
        var totalConstitution = character.GetTotalConstitution();

        // Assert
        totalConstitution.Should().Be(14);
    }

    [Fact]
    public void GetTotalConstitution_Should_Include_Equipment_Bonuses()
    {
        // Arrange
        var character = new Character { Constitution = 10 };
        character.EquippedChest = new Item { BonusConstitution = 5 };
        character.EquippedBelt = new Item { BonusConstitution = 2 };

        // Act
        var totalConstitution = character.GetTotalConstitution();

        // Assert
        totalConstitution.Should().Be(17); // 10 + 5 + 2
    }

    #endregion

    #region Total Intelligence Tests

    [Fact]
    public void GetTotalIntelligence_Should_Return_Base_Intelligence_With_No_Equipment()
    {
        // Arrange
        var character = new Character { Intelligence = 16 };

        // Act
        var totalIntelligence = character.GetTotalIntelligence();

        // Assert
        totalIntelligence.Should().Be(16);
    }

    [Fact]
    public void GetTotalIntelligence_Should_Include_Equipment_Bonuses()
    {
        // Arrange
        var character = new Character { Intelligence = 10 };
        character.EquippedMainHand = new Item { Name = "Staff of Wisdom", BonusIntelligence = 6 };
        character.EquippedHelmet = new Item { Name = "Thinking Cap", BonusIntelligence = 3 };

        // Act
        var totalIntelligence = character.GetTotalIntelligence();

        // Assert
        totalIntelligence.Should().Be(19); // 10 + 6 + 3
    }

    #endregion

    #region Total Wisdom Tests

    [Fact]
    public void GetTotalWisdom_Should_Return_Base_Wisdom_With_No_Equipment()
    {
        // Arrange
        var character = new Character { Wisdom = 13 };

        // Act
        var totalWisdom = character.GetTotalWisdom();

        // Assert
        totalWisdom.Should().Be(13);
    }

    [Fact]
    public void GetTotalWisdom_Should_Include_Equipment_Bonuses()
    {
        // Arrange
        var character = new Character { Wisdom = 10 };
        character.EquippedNecklace = new Item { BonusWisdom = 4 };
        character.EquippedRing1 = new Item { BonusWisdom = 2 };

        // Act
        var totalWisdom = character.GetTotalWisdom();

        // Assert
        totalWisdom.Should().Be(16); // 10 + 4 + 2
    }

    #endregion

    #region Total Charisma Tests

    [Fact]
    public void GetTotalCharisma_Should_Return_Base_Charisma_With_No_Equipment()
    {
        // Arrange
        var character = new Character { Charisma = 11 };

        // Act
        var totalCharisma = character.GetTotalCharisma();

        // Assert
        totalCharisma.Should().Be(11);
    }

    [Fact]
    public void GetTotalCharisma_Should_Include_Equipment_Bonuses()
    {
        // Arrange
        var character = new Character { Charisma = 10 };
        character.EquippedNecklace = new Item { BonusCharisma = 3 };
        character.EquippedRing1 = new Item { BonusCharisma = 2 };
        character.EquippedRing2 = new Item { BonusCharisma = 2 };

        // Act
        var totalCharisma = character.GetTotalCharisma();

        // Assert
        totalCharisma.Should().Be(17); // 10 + 3 + 2 + 2
    }

    #endregion

    #region Shop Discount Tests

    [Fact]
    public void GetShopDiscount_Should_Calculate_Based_On_Charisma()
    {
        // Arrange
        var character = new Character { Charisma = 20 };

        // Act
        var discount = character.GetShopDiscount();

        // Assert
        discount.Should().Be(20.0); // 20 * 1.0 = 20% discount
    }

    [Theory]
    [InlineData(10, 10.0)]
    [InlineData(15, 15.0)]
    [InlineData(25, 25.0)]
    public void GetShopDiscount_Should_Scale_With_Charisma(int charisma, double expectedDiscount)
    {
        // Arrange
        var character = new Character { Charisma = charisma };

        // Act
        var discount = character.GetShopDiscount();

        // Assert
        discount.Should().BeApproximately(expectedDiscount, 0.1);
    }

    #endregion

    #region Rare Item Chance Tests

    [Fact]
    public void GetRareItemChance_Should_Calculate_Based_On_Charisma()
    {
        // Arrange
        var character = new Character { Charisma = 15 };

        // Act
        var chance = character.GetRareItemChance();

        // Assert
        chance.Should().Be(7.5); // 15 * 0.5 = 7.5% rare find
    }

    [Theory]
    [InlineData(10, 5.0)]
    [InlineData(20, 10.0)]
    [InlineData(30, 15.0)]
    public void GetRareItemChance_Should_Scale_With_Charisma(int charisma, double expectedChance)
    {
        // Arrange
        var character = new Character { Charisma = charisma };

        // Act
        var chance = character.GetRareItemChance();

        // Assert
        chance.Should().BeApproximately(expectedChance, 0.1);
    }

    #endregion

    #region Magic Resistance Tests

    [Fact]
    public void GetMagicResistance_Should_Calculate_Based_On_Wisdom()
    {
        // Arrange
        var character = new Character { Wisdom = 15 };

        // Act
        var resistance = character.GetMagicResistance();

        // Assert
        resistance.Should().Be(12.0); // 15 * 0.8
    }

    [Theory]
    [InlineData(10, 8.0)]
    [InlineData(20, 16.0)]
    [InlineData(25, 20.0)]
    public void GetMagicResistance_Should_Scale_With_Wisdom(int wisdom, double expectedResistance)
    {
        // Arrange
        var character = new Character { Wisdom = wisdom };

        // Act
        var resistance = character.GetMagicResistance();

        // Assert
        resistance.Should().BeApproximately(expectedResistance, 0.1);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Character_Should_Calculate_Correct_Stats_With_Full_Equipment_Set()
    {
        // Arrange - Warrior build with strength-focused equipment
        var character = new Character
        {
            Name = "Warrior",
            Strength = 15,
            Dexterity = 10,
            Constitution = 14,
            Intelligence = 8,
            Wisdom = 10,
            Charisma = 12
        };

        // Equip full set
        character.EquippedMainHand = new Item { Name = "Great Sword", BonusStrength = 5, BonusDexterity = 2 };
        character.EquippedOffHand = new Item { Name = "Steel Shield", BonusConstitution = 3 };
        character.EquippedHelmet = new Item { Name = "Warrior Helm", BonusStrength = 2, BonusConstitution = 2 };
        character.EquippedChest = new Item { Name = "Plate Mail", BonusConstitution = 5 };
        character.EquippedGloves = new Item { Name = "Gauntlets", BonusStrength = 1 };
        character.EquippedBoots = new Item { Name = "Heavy Boots", BonusConstitution = 1 };

        // Act
        var totalStr = character.GetTotalStrength();
        var totalDex = character.GetTotalDexterity();
        var totalCon = character.GetTotalConstitution();

        // Assert
        totalStr.Should().Be(23); // 15 + 5 + 2 + 1
        totalDex.Should().Be(12); // 10 + 2
        totalCon.Should().Be(25); // 14 + 3 + 2 + 5 + 1
    }

    [Fact]
    public void Character_Should_Calculate_Correct_Stats_With_Mage_Equipment()
    {
        // Arrange - Mage build with intelligence/wisdom-focused equipment
        var character = new Character
        {
            Name = "Mage",
            Intelligence = 18,
            Wisdom = 16,
            Charisma = 14
        };

        character.EquippedMainHand = new Item { Name = "Arcane Staff", BonusIntelligence = 6, BonusWisdom = 3 };
        character.EquippedHelmet = new Item { Name = "Wizard Hat", BonusIntelligence = 3 };
        character.EquippedChest = new Item { Name = "Robe of the Magi", BonusIntelligence = 4, BonusWisdom = 4 };
        character.EquippedNecklace = new Item { Name = "Amulet of Knowledge", BonusIntelligence = 2, BonusWisdom = 2 };

        // Act
        var totalInt = character.GetTotalIntelligence();
        var totalWis = character.GetTotalWisdom();

        // Assert
        totalInt.Should().Be(33); // 18 + 6 + 3 + 4 + 2
        totalWis.Should().Be(25); // 16 + 3 + 4 + 2
    }

    [Fact]
    public void Character_Total_Stats_Should_Benefit_From_Equipment()
    {
        // Arrange
        var character = new Character
        {
            Strength = 10,
            Dexterity = 10,
            Constitution = 10,
            Level = 5
        };

        // Before equipment
        var baseStrength = character.GetTotalStrength();
        var baseDexterity = character.GetTotalDexterity();

        // Add equipment
        character.EquippedMainHand = new Item { BonusStrength = 10 };
        character.EquippedBoots = new Item { BonusDexterity = 10 };

        // After equipment
        var boostedStrength = character.GetTotalStrength();
        var boostedDexterity = character.GetTotalDexterity();

        // Assert
        boostedStrength.Should().Be(20); // 10 + 10
        boostedDexterity.Should().Be(20); // 10 + 10
        boostedStrength.Should().BeGreaterThan(baseStrength);
        boostedDexterity.Should().BeGreaterThan(baseDexterity);
    }

    #endregion
}
