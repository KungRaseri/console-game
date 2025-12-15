using FluentAssertions;
using Game.Core.Models;
using Xunit;

namespace Game.Tests.Models;

/// <summary>
/// Comprehensive tests for SetBonus model.
/// Target: 0% -> 100% coverage.
/// </summary>
public class SetBonusTests
{
    #region Initialization Tests

    [Fact]
    public void SetBonus_Should_Initialize_With_Default_Values()
    {
        // Act
        var bonus = new SetBonus();

        // Assert
        bonus.PiecesRequired.Should().Be(0);
        bonus.Description.Should().BeEmpty();
        bonus.BonusStrength.Should().Be(0);
        bonus.BonusDexterity.Should().Be(0);
        bonus.BonusConstitution.Should().Be(0);
        bonus.BonusIntelligence.Should().Be(0);
        bonus.BonusWisdom.Should().Be(0);
        bonus.BonusCharisma.Should().Be(0);
        bonus.SpecialEffect.Should().BeNull();
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void SetBonus_Should_Allow_PiecesRequired_Assignment()
    {
        // Arrange
        var bonus = new SetBonus();

        // Act
        bonus.PiecesRequired = 4;

        // Assert
        bonus.PiecesRequired.Should().Be(4);
    }

    [Fact]
    public void SetBonus_Should_Allow_Description_Assignment()
    {
        // Arrange
        var bonus = new SetBonus();

        // Act
        bonus.Description = "Enhanced Defense";

        // Assert
        bonus.Description.Should().Be("Enhanced Defense");
    }

    [Fact]
    public void SetBonus_Should_Allow_Strength_Bonus()
    {
        // Arrange
        var bonus = new SetBonus();

        // Act
        bonus.BonusStrength = 5;

        // Assert
        bonus.BonusStrength.Should().Be(5);
    }

    [Fact]
    public void SetBonus_Should_Allow_Dexterity_Bonus()
    {
        // Arrange
        var bonus = new SetBonus();

        // Act
        bonus.BonusDexterity = 3;

        // Assert
        bonus.BonusDexterity.Should().Be(3);
    }

    [Fact]
    public void SetBonus_Should_Allow_Constitution_Bonus()
    {
        // Arrange
        var bonus = new SetBonus();

        // Act
        bonus.BonusConstitution = 4;

        // Assert
        bonus.BonusConstitution.Should().Be(4);
    }

    [Fact]
    public void SetBonus_Should_Allow_Intelligence_Bonus()
    {
        // Arrange
        var bonus = new SetBonus();

        // Act
        bonus.BonusIntelligence = 6;

        // Assert
        bonus.BonusIntelligence.Should().Be(6);
    }

    [Fact]
    public void SetBonus_Should_Allow_Wisdom_Bonus()
    {
        // Arrange
        var bonus = new SetBonus();

        // Act
        bonus.BonusWisdom = 2;

        // Assert
        bonus.BonusWisdom.Should().Be(2);
    }

    [Fact]
    public void SetBonus_Should_Allow_Charisma_Bonus()
    {
        // Arrange
        var bonus = new SetBonus();

        // Act
        bonus.BonusCharisma = 7;

        // Assert
        bonus.BonusCharisma.Should().Be(7);
    }

    [Fact]
    public void SetBonus_Should_Allow_SpecialEffect_Assignment()
    {
        // Arrange
        var bonus = new SetBonus();

        // Act
        bonus.SpecialEffect = "FireAura";

        // Assert
        bonus.SpecialEffect.Should().Be("FireAura");
    }

    [Fact]
    public void SetBonus_Should_Support_Multiple_Attribute_Bonuses()
    {
        // Arrange
        var bonus = new SetBonus();

        // Act
        bonus.BonusStrength = 3;
        bonus.BonusDexterity = 2;
        bonus.BonusIntelligence = 5;

        // Assert
        bonus.BonusStrength.Should().Be(3);
        bonus.BonusDexterity.Should().Be(2);
        bonus.BonusIntelligence.Should().Be(5);
        bonus.BonusConstitution.Should().Be(0);
        bonus.BonusWisdom.Should().Be(0);
        bonus.BonusCharisma.Should().Be(0);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void SetBonus_Should_Create_Two_Piece_Bonus()
    {
        // Arrange & Act
        var bonus = new SetBonus
        {
            PiecesRequired = 2,
            Description = "+10 Defense",
            BonusConstitution = 2
        };

        // Assert
        bonus.PiecesRequired.Should().Be(2);
        bonus.Description.Should().Be("+10 Defense");
        bonus.BonusConstitution.Should().Be(2);
    }

    [Fact]
    public void SetBonus_Should_Create_Four_Piece_Bonus()
    {
        // Arrange & Act
        var bonus = new SetBonus
        {
            PiecesRequired = 4,
            Description = "+20% Fire Resistance",
            BonusIntelligence = 4,
            SpecialEffect = "FireResistance"
        };

        // Assert
        bonus.PiecesRequired.Should().Be(4);
        bonus.Description.Should().Be("+20% Fire Resistance");
        bonus.BonusIntelligence.Should().Be(4);
        bonus.SpecialEffect.Should().Be("FireResistance");
    }

    [Fact]
    public void SetBonus_Should_Create_Six_Piece_Bonus()
    {
        // Arrange & Act
        var bonus = new SetBonus
        {
            PiecesRequired = 6,
            Description = "+50 Strength and Legendary Aura",
            BonusStrength = 10,
            BonusDexterity = 5,
            BonusConstitution = 5,
            SpecialEffect = "LegendaryAura"
        };

        // Assert
        bonus.PiecesRequired.Should().Be(6);
        bonus.BonusStrength.Should().Be(10);
        bonus.BonusDexterity.Should().Be(5);
        bonus.BonusConstitution.Should().Be(5);
        bonus.SpecialEffect.Should().Be("LegendaryAura");
    }

    [Fact]
    public void SetBonus_Should_Support_Zero_Attribute_Bonuses()
    {
        // Arrange & Act
        var bonus = new SetBonus
        {
            PiecesRequired = 3,
            Description = "Special effect only",
            SpecialEffect = "GlowingEffect"
        };

        // Assert
        bonus.BonusStrength.Should().Be(0);
        bonus.BonusDexterity.Should().Be(0);
        bonus.BonusConstitution.Should().Be(0);
        bonus.BonusIntelligence.Should().Be(0);
        bonus.BonusWisdom.Should().Be(0);
        bonus.BonusCharisma.Should().Be(0);
        bonus.SpecialEffect.Should().Be("GlowingEffect");
    }

    [Fact]
    public void SetBonus_Should_Support_All_Six_Attributes()
    {
        // Arrange & Act
        var bonus = new SetBonus
        {
            PiecesRequired = 8,
            Description = "Complete Set - All Attributes",
            BonusStrength = 5,
            BonusDexterity = 5,
            BonusConstitution = 5,
            BonusIntelligence = 5,
            BonusWisdom = 5,
            BonusCharisma = 5
        };

        // Assert
        bonus.BonusStrength.Should().Be(5);
        bonus.BonusDexterity.Should().Be(5);
        bonus.BonusConstitution.Should().Be(5);
        bonus.BonusIntelligence.Should().Be(5);
        bonus.BonusWisdom.Should().Be(5);
        bonus.BonusCharisma.Should().Be(5);
    }

    [Fact]
    public void SetBonus_Should_Allow_Negative_Bonuses()
    {
        // Arrange & Act
        var bonus = new SetBonus
        {
            PiecesRequired = 2,
            Description = "Cursed set piece",
            BonusStrength = -2,
            BonusWisdom = -1
        };

        // Assert
        bonus.BonusStrength.Should().Be(-2);
        bonus.BonusWisdom.Should().Be(-1);
    }

    [Fact]
    public void SetBonus_Should_Allow_High_Pieces_Required()
    {
        // Arrange & Act
        var bonus = new SetBonus { PiecesRequired = 10 };

        // Assert
        bonus.PiecesRequired.Should().Be(10);
    }

    [Fact]
    public void SetBonus_Should_Allow_Null_Special_Effect()
    {
        // Arrange & Act
        var bonus = new SetBonus
        {
            PiecesRequired = 2,
            Description = "Basic bonus",
            SpecialEffect = null
        };

        // Assert
        bonus.SpecialEffect.Should().BeNull();
    }

    #endregion
}
