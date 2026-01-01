using FluentAssertions;
using Game.Shared.Models;

namespace RealmEngine.Shared.Tests.Models;

[Trait("Category", "Unit")]
/// <summary>
/// Comprehensive tests for Enchantment model.
/// Target: 91.6% -> 100% coverage.
/// </summary>
public class EnchantmentTests
{
    #region Initialization Tests

    [Fact]
    public void Enchantment_Should_Initialize_With_Default_Values()
    {
        // Act
        var enchantment = new Enchantment();

        // Assert
        enchantment.Id.Should().NotBeEmpty();
        enchantment.Name.Should().BeEmpty();
        enchantment.Description.Should().BeEmpty();
        enchantment.Rarity.Should().Be(EnchantmentRarity.Minor);
        enchantment.Level.Should().Be(1);
        enchantment.BonusStrength.Should().Be(0);
        enchantment.BonusDexterity.Should().Be(0);
        enchantment.BonusConstitution.Should().Be(0);
        enchantment.BonusIntelligence.Should().Be(0);
        enchantment.BonusWisdom.Should().Be(0);
        enchantment.BonusCharisma.Should().Be(0);
        enchantment.SpecialEffect.Should().BeNull();
    }

    [Fact]
    public void Enchantment_Should_Generate_Unique_Ids()
    {
        // Act
        var enchantment1 = new Enchantment();
        var enchantment2 = new Enchantment();

        // Assert
        enchantment1.Id.Should().NotBe(enchantment2.Id);
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void Enchantment_Should_Allow_Name_Assignment()
    {
        // Arrange
        var enchantment = new Enchantment();

        // Act
        enchantment.Name = "Flaming";

        // Assert
        enchantment.Name.Should().Be("Flaming");
    }

    [Fact]
    public void Enchantment_Should_Allow_Description_Assignment()
    {
        // Arrange
        var enchantment = new Enchantment();

        // Act
        enchantment.Description = "Adds fire damage";

        // Assert
        enchantment.Description.Should().Be("Adds fire damage");
    }

    [Fact]
    public void Enchantment_Should_Allow_Level_Assignment()
    {
        // Arrange
        var enchantment = new Enchantment();

        // Act
        enchantment.Level = 5;

        // Assert
        enchantment.Level.Should().Be(5);
    }

    [Fact]
    public void Enchantment_Should_Allow_SpecialEffect_Assignment()
    {
        // Arrange
        var enchantment = new Enchantment();

        // Act
        enchantment.SpecialEffect = "FireAura";

        // Assert
        enchantment.SpecialEffect.Should().Be("FireAura");
    }

    #endregion

    #region Rarity Tests

    [Theory]
    [InlineData(EnchantmentRarity.Minor)]
    [InlineData(EnchantmentRarity.Lesser)]
    [InlineData(EnchantmentRarity.Greater)]
    [InlineData(EnchantmentRarity.Superior)]
    [InlineData(EnchantmentRarity.Legendary)]
    public void Enchantment_Should_Support_All_Rarities(EnchantmentRarity rarity)
    {
        // Arrange
        var enchantment = new Enchantment();

        // Act
        enchantment.Rarity = rarity;

        // Assert
        enchantment.Rarity.Should().Be(rarity);
    }

    [Fact]
    public void EnchantmentRarity_Should_Have_Five_Tiers()
    {
        // Assert
        var rarities = Enum.GetValues<EnchantmentRarity>();
        rarities.Should().HaveCount(5);
        rarities.Should().Contain(new[]
        {
            EnchantmentRarity.Minor,
            EnchantmentRarity.Lesser,
            EnchantmentRarity.Greater,
            EnchantmentRarity.Superior,
            EnchantmentRarity.Legendary
        });
    }

    #endregion

    #region Attribute Bonus Tests

    [Fact]
    public void Enchantment_Should_Allow_Strength_Bonus()
    {
        // Arrange
        var enchantment = new Enchantment();

        // Act
        enchantment.BonusStrength = 5;

        // Assert
        enchantment.BonusStrength.Should().Be(5);
    }

    [Fact]
    public void Enchantment_Should_Allow_Dexterity_Bonus()
    {
        // Arrange
        var enchantment = new Enchantment();

        // Act
        enchantment.BonusDexterity = 3;

        // Assert
        enchantment.BonusDexterity.Should().Be(3);
    }

    [Fact]
    public void Enchantment_Should_Allow_Constitution_Bonus()
    {
        // Arrange
        var enchantment = new Enchantment();

        // Act
        enchantment.BonusConstitution = 4;

        // Assert
        enchantment.BonusConstitution.Should().Be(4);
    }

    [Fact]
    public void Enchantment_Should_Allow_Intelligence_Bonus()
    {
        // Arrange
        var enchantment = new Enchantment();

        // Act
        enchantment.BonusIntelligence = 6;

        // Assert
        enchantment.BonusIntelligence.Should().Be(6);
    }

    [Fact]
    public void Enchantment_Should_Allow_Wisdom_Bonus()
    {
        // Arrange
        var enchantment = new Enchantment();

        // Act
        enchantment.BonusWisdom = 2;

        // Assert
        enchantment.BonusWisdom.Should().Be(2);
    }

    [Fact]
    public void Enchantment_Should_Allow_Charisma_Bonus()
    {
        // Arrange
        var enchantment = new Enchantment();

        // Act
        enchantment.BonusCharisma = 7;

        // Assert
        enchantment.BonusCharisma.Should().Be(7);
    }

    [Fact]
    public void Enchantment_Should_Allow_Negative_Bonuses()
    {
        // Arrange
        var enchantment = new Enchantment();

        // Act
        enchantment.BonusStrength = -2;
        enchantment.BonusDexterity = -1;

        // Assert
        enchantment.BonusStrength.Should().Be(-2);
        enchantment.BonusDexterity.Should().Be(-1);
    }

    [Fact]
    public void Enchantment_Should_Support_Multiple_Attribute_Bonuses()
    {
        // Arrange
        var enchantment = new Enchantment();

        // Act
        enchantment.BonusStrength = 3;
        enchantment.BonusDexterity = 2;
        enchantment.BonusIntelligence = 5;

        // Assert
        enchantment.BonusStrength.Should().Be(3);
        enchantment.BonusDexterity.Should().Be(2);
        enchantment.BonusIntelligence.Should().Be(5);
        enchantment.BonusConstitution.Should().Be(0);
        enchantment.BonusWisdom.Should().Be(0);
        enchantment.BonusCharisma.Should().Be(0);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Enchantment_Should_Create_Complete_Flaming_Enchantment()
    {
        // Arrange & Act
        var enchantment = new Enchantment
        {
            Name = "Flaming",
            Description = "Deals additional fire damage",
            Rarity = EnchantmentRarity.Greater,
            Level = 3,
            BonusStrength = 2,
            BonusIntelligence = 3,
            SpecialEffect = "FireDamage"
        };

        // Assert
        enchantment.Name.Should().Be("Flaming");
        enchantment.Description.Should().Be("Deals additional fire damage");
        enchantment.Rarity.Should().Be(EnchantmentRarity.Greater);
        enchantment.Level.Should().Be(3);
        enchantment.BonusStrength.Should().Be(2);
        enchantment.BonusIntelligence.Should().Be(3);
        enchantment.SpecialEffect.Should().Be("FireDamage");
    }

    [Fact]
    public void Enchantment_Should_Create_Complete_Protection_Enchantment()
    {
        // Arrange & Act
        var enchantment = new Enchantment
        {
            Name = "Protection",
            Description = "Increases defensive capabilities",
            Rarity = EnchantmentRarity.Superior,
            Level = 4,
            BonusConstitution = 5,
            BonusWisdom = 2,
            SpecialEffect = "Shield"
        };

        // Assert
        enchantment.Name.Should().Be("Protection");
        enchantment.Rarity.Should().Be(EnchantmentRarity.Superior);
        enchantment.Level.Should().Be(4);
        enchantment.BonusConstitution.Should().Be(5);
        enchantment.BonusWisdom.Should().Be(2);
    }

    [Fact]
    public void Enchantment_Should_Create_Legendary_Enchantment()
    {
        // Arrange & Act
        var enchantment = new Enchantment
        {
            Name = "Divine Power",
            Description = "Grants immense power",
            Rarity = EnchantmentRarity.Legendary,
            Level = 10,
            BonusStrength = 10,
            BonusDexterity = 10,
            BonusConstitution = 10,
            BonusIntelligence = 10,
            BonusWisdom = 10,
            BonusCharisma = 10,
            SpecialEffect = "GlowingAura"
        };

        // Assert
        enchantment.Rarity.Should().Be(EnchantmentRarity.Legendary);
        enchantment.Level.Should().Be(10);
        enchantment.BonusStrength.Should().Be(10);
        enchantment.BonusDexterity.Should().Be(10);
        enchantment.BonusConstitution.Should().Be(10);
        enchantment.BonusIntelligence.Should().Be(10);
        enchantment.BonusWisdom.Should().Be(10);
        enchantment.BonusCharisma.Should().Be(10);
    }

    [Fact]
    public void Enchantment_Should_Support_Zero_Bonuses()
    {
        // Arrange & Act
        var enchantment = new Enchantment
        {
            Name = "Placeholder",
            Description = "No bonuses",
            Rarity = EnchantmentRarity.Minor
        };

        // Assert
        enchantment.BonusStrength.Should().Be(0);
        enchantment.BonusDexterity.Should().Be(0);
        enchantment.BonusConstitution.Should().Be(0);
        enchantment.BonusIntelligence.Should().Be(0);
        enchantment.BonusWisdom.Should().Be(0);
        enchantment.BonusCharisma.Should().Be(0);
    }

    [Fact]
    public void Enchantment_Should_Support_High_Level_Values()
    {
        // Arrange & Act
        var enchantment = new Enchantment { Level = 100 };

        // Assert
        enchantment.Level.Should().Be(100);
    }

    [Fact]
    public void Enchantment_Should_Allow_Empty_Special_Effect()
    {
        // Arrange & Act
        var enchantment = new Enchantment
        {
            Name = "Simple Bonus",
            SpecialEffect = null
        };

        // Assert
        enchantment.SpecialEffect.Should().BeNull();
    }

    #endregion
}
