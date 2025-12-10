using FluentAssertions;
using Game.Models;
using Game.Utilities;
using Xunit;

namespace Game.Tests.Utilities;

/// <summary>
/// Tests for SkillEffectCalculator utility class.
/// </summary>
public class SkillEffectCalculatorTests
{
    #region Physical Damage Tests

    [Fact]
    public void GetPhysicalDamageMultiplier_Should_Return_1_When_No_Skills()
    {
        // Arrange
        var character = new Character { LearnedSkills = new List<Skill>() };

        // Act
        var multiplier = SkillEffectCalculator.GetPhysicalDamageMultiplier(character);

        // Assert
        multiplier.Should().Be(1.0);
    }

    [Theory]
    [InlineData(1, 1.10)]
    [InlineData(2, 1.20)]
    [InlineData(3, 1.30)]
    [InlineData(5, 1.50)]
    public void GetPhysicalDamageMultiplier_Should_Increase_By_10_Percent_Per_Rank(int rank, double expected)
    {
        // Arrange
        var character = new Character
        {
            LearnedSkills = new List<Skill>
            {
                new Skill { Name = "Power Attack", CurrentRank = rank }
            }
        };

        // Act
        var multiplier = SkillEffectCalculator.GetPhysicalDamageMultiplier(character);

        // Assert
        multiplier.Should().BeApproximately(expected, 0.001);
    }

    [Fact]
    public void GetPhysicalDamageMultiplier_Should_Ignore_Other_Skills()
    {
        // Arrange
        var character = new Character
        {
            LearnedSkills = new List<Skill>
            {
                new Skill { Name = "Arcane Knowledge", CurrentRank = 5 },
                new Skill { Name = "Critical Strike", CurrentRank = 3 }
            }
        };

        // Act
        var multiplier = SkillEffectCalculator.GetPhysicalDamageMultiplier(character);

        // Assert
        multiplier.Should().Be(1.0);
    }

    #endregion

    #region Magic Damage Tests

    [Fact]
    public void GetMagicDamageMultiplier_Should_Return_1_When_No_Skills()
    {
        // Arrange
        var character = new Character { LearnedSkills = new List<Skill>() };

        // Act
        var multiplier = SkillEffectCalculator.GetMagicDamageMultiplier(character);

        // Assert
        multiplier.Should().Be(1.0);
    }

    [Theory]
    [InlineData(1, 1.10)]
    [InlineData(2, 1.20)]
    [InlineData(4, 1.40)]
    [InlineData(10, 2.00)]
    public void GetMagicDamageMultiplier_Should_Increase_By_10_Percent_Per_Rank(int rank, double expected)
    {
        // Arrange
        var character = new Character
        {
            LearnedSkills = new List<Skill>
            {
                new Skill { Name = "Arcane Knowledge", CurrentRank = rank }
            }
        };

        // Act
        var multiplier = SkillEffectCalculator.GetMagicDamageMultiplier(character);

        // Assert
        multiplier.Should().BeApproximately(expected, 0.001);
    }

    #endregion

    #region Critical Hit Tests

    [Fact]
    public void GetCriticalChanceBonus_Should_Return_0_When_No_Skills()
    {
        // Arrange
        var character = new Character { LearnedSkills = new List<Skill>() };

        // Act
        var bonus = SkillEffectCalculator.GetCriticalChanceBonus(character);

        // Assert
        bonus.Should().Be(0.0);
    }

    [Theory]
    [InlineData(1, 2.0)]
    [InlineData(2, 4.0)]
    [InlineData(5, 10.0)]
    [InlineData(10, 20.0)]
    public void GetCriticalChanceBonus_Should_Increase_By_2_Percent_Per_Rank(int rank, double expected)
    {
        // Arrange
        var character = new Character
        {
            LearnedSkills = new List<Skill>
            {
                new Skill { Name = "Critical Strike", CurrentRank = rank }
            }
        };

        // Act
        var bonus = SkillEffectCalculator.GetCriticalChanceBonus(character);

        // Assert
        bonus.Should().BeApproximately(expected, 0.001);
    }

    #endregion

    #region Physical Defense Tests

    [Fact]
    public void GetPhysicalDefenseMultiplier_Should_Return_1_When_No_Skills()
    {
        // Arrange
        var character = new Character { LearnedSkills = new List<Skill>() };

        // Act
        var multiplier = SkillEffectCalculator.GetPhysicalDefenseMultiplier(character);

        // Assert
        multiplier.Should().Be(1.0);
    }

    [Theory]
    [InlineData(1, 1.05)]
    [InlineData(2, 1.10)]
    [InlineData(5, 1.25)]
    [InlineData(10, 1.50)]
    public void GetPhysicalDefenseMultiplier_Should_Increase_By_5_Percent_Per_Rank(int rank, double expected)
    {
        // Arrange
        var character = new Character
        {
            LearnedSkills = new List<Skill>
            {
                new Skill { Name = "Iron Skin", CurrentRank = rank }
            }
        };

        // Act
        var multiplier = SkillEffectCalculator.GetPhysicalDefenseMultiplier(character);

        // Assert
        multiplier.Should().BeApproximately(expected, 0.001);
    }

    #endregion

    #region Dodge Chance Tests

    [Fact]
    public void GetDodgeChanceBonus_Should_Return_0_When_No_Skills()
    {
        // Arrange
        var character = new Character { LearnedSkills = new List<Skill>() };

        // Act
        var bonus = SkillEffectCalculator.GetDodgeChanceBonus(character);

        // Assert
        bonus.Should().Be(0.0);
    }

    [Theory]
    [InlineData(1, 3.0)]
    [InlineData(2, 6.0)]
    [InlineData(5, 15.0)]
    [InlineData(10, 30.0)]
    public void GetDodgeChanceBonus_Should_Increase_By_3_Percent_Per_Rank(int rank, double expected)
    {
        // Arrange
        var character = new Character
        {
            LearnedSkills = new List<Skill>
            {
                new Skill { Name = "Quick Reflexes", CurrentRank = rank }
            }
        };

        // Act
        var bonus = SkillEffectCalculator.GetDodgeChanceBonus(character);

        // Assert
        bonus.Should().BeApproximately(expected, 0.001);
    }

    #endregion

    #region Rare Item Find Tests

    [Fact]
    public void GetRareItemFindBonus_Should_Return_0_When_No_Skills()
    {
        // Arrange
        var character = new Character { LearnedSkills = new List<Skill>() };

        // Act
        var bonus = SkillEffectCalculator.GetRareItemFindBonus(character);

        // Assert
        bonus.Should().Be(0.0);
    }

    [Theory]
    [InlineData(1, 10.0)]
    [InlineData(2, 20.0)]
    [InlineData(5, 50.0)]
    [InlineData(10, 100.0)]
    public void GetRareItemFindBonus_Should_Increase_By_10_Percent_Per_Rank(int rank, double expected)
    {
        // Arrange
        var character = new Character
        {
            LearnedSkills = new List<Skill>
            {
                new Skill { Name = "Treasure Hunter", CurrentRank = rank }
            }
        };

        // Act
        var bonus = SkillEffectCalculator.GetRareItemFindBonus(character);

        // Assert
        bonus.Should().BeApproximately(expected, 0.001);
    }

    #endregion

    #region Max Mana Tests

    [Fact]
    public void GetMaxManaMultiplier_Should_Return_1_When_No_Skills()
    {
        // Arrange
        var character = new Character { LearnedSkills = new List<Skill>() };

        // Act
        var multiplier = SkillEffectCalculator.GetMaxManaMultiplier(character);

        // Assert
        multiplier.Should().Be(1.0);
    }

    [Theory]
    [InlineData(1, 1.10)]
    [InlineData(2, 1.20)]
    [InlineData(5, 1.50)]
    [InlineData(10, 2.00)]
    public void GetMaxManaMultiplier_Should_Increase_By_10_Percent_Per_Rank(int rank, double expected)
    {
        // Arrange
        var character = new Character
        {
            LearnedSkills = new List<Skill>
            {
                new Skill { Name = "Mana Efficiency", CurrentRank = rank }
            }
        };

        // Act
        var multiplier = SkillEffectCalculator.GetMaxManaMultiplier(character);

        // Assert
        multiplier.Should().BeApproximately(expected, 0.001);
    }

    #endregion

    #region Health Regeneration Tests

    [Fact]
    public void GetHealthRegeneration_Should_Return_0_When_No_Skills()
    {
        // Arrange
        var character = new Character { LearnedSkills = new List<Skill>() };

        // Act
        var regen = SkillEffectCalculator.GetHealthRegeneration(character);

        // Assert
        regen.Should().Be(0);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 4)]
    [InlineData(5, 10)]
    [InlineData(10, 20)]
    public void GetHealthRegeneration_Should_Increase_By_2_HP_Per_Rank(int rank, int expected)
    {
        // Arrange
        var character = new Character
        {
            LearnedSkills = new List<Skill>
            {
                new Skill { Name = "Regeneration", CurrentRank = rank }
            }
        };

        // Act
        var regen = SkillEffectCalculator.GetHealthRegeneration(character);

        // Assert
        regen.Should().Be(expected);
    }

    #endregion

    #region Apply Regeneration Tests

    [Fact]
    public void ApplyRegeneration_Should_Return_0_When_No_Regeneration_Skill()
    {
        // Arrange
        var character = new Character
        {
            Health = 50,
            MaxHealth = 100,
            LearnedSkills = new List<Skill>()
        };

        // Act
        var healed = SkillEffectCalculator.ApplyRegeneration(character);

        // Assert
        healed.Should().Be(0);
        character.Health.Should().Be(50);
    }

    [Fact]
    public void ApplyRegeneration_Should_Heal_Character()
    {
        // Arrange
        var character = new Character
        {
            Health = 50,
            MaxHealth = 100,
            LearnedSkills = new List<Skill>
            {
                new Skill { Name = "Regeneration", CurrentRank = 3 } // 6 HP regen
            }
        };

        // Act
        var healed = SkillEffectCalculator.ApplyRegeneration(character);

        // Assert
        healed.Should().Be(6);
        character.Health.Should().Be(56);
    }

    [Fact]
    public void ApplyRegeneration_Should_Not_Exceed_Max_Health()
    {
        // Arrange
        var character = new Character
        {
            Health = 98,
            MaxHealth = 100,
            LearnedSkills = new List<Skill>
            {
                new Skill { Name = "Regeneration", CurrentRank = 5 } // 10 HP regen
            }
        };

        // Act
        var healed = SkillEffectCalculator.ApplyRegeneration(character);

        // Assert
        healed.Should().Be(2); // Only healed 2 to reach max
        character.Health.Should().Be(100);
    }

    [Fact]
    public void ApplyRegeneration_Should_Do_Nothing_When_At_Max_Health()
    {
        // Arrange
        var character = new Character
        {
            Health = 100,
            MaxHealth = 100,
            LearnedSkills = new List<Skill>
            {
                new Skill { Name = "Regeneration", CurrentRank = 5 }
            }
        };

        // Act
        var healed = SkillEffectCalculator.ApplyRegeneration(character);

        // Assert
        healed.Should().Be(0);
        character.Health.Should().Be(100);
    }

    #endregion

    #region Multiple Skills Tests

    [Fact]
    public void Character_With_Multiple_Skills_Should_Calculate_All_Bonuses()
    {
        // Arrange
        var character = new Character
        {
            LearnedSkills = new List<Skill>
            {
                new Skill { Name = "Power Attack", CurrentRank = 3 },
                new Skill { Name = "Arcane Knowledge", CurrentRank = 2 },
                new Skill { Name = "Critical Strike", CurrentRank = 5 },
                new Skill { Name = "Iron Skin", CurrentRank = 4 },
                new Skill { Name = "Quick Reflexes", CurrentRank = 2 },
                new Skill { Name = "Treasure Hunter", CurrentRank = 1 },
                new Skill { Name = "Mana Efficiency", CurrentRank = 3 },
                new Skill { Name = "Regeneration", CurrentRank = 5 }
            }
        };

        // Act & Assert
        SkillEffectCalculator.GetPhysicalDamageMultiplier(character).Should().BeApproximately(1.30, 0.001);
        SkillEffectCalculator.GetMagicDamageMultiplier(character).Should().BeApproximately(1.20, 0.001);
        SkillEffectCalculator.GetCriticalChanceBonus(character).Should().BeApproximately(10.0, 0.001);
        SkillEffectCalculator.GetPhysicalDefenseMultiplier(character).Should().BeApproximately(1.20, 0.001);
        SkillEffectCalculator.GetDodgeChanceBonus(character).Should().BeApproximately(6.0, 0.001);
        SkillEffectCalculator.GetRareItemFindBonus(character).Should().BeApproximately(10.0, 0.001);
        SkillEffectCalculator.GetMaxManaMultiplier(character).Should().BeApproximately(1.30, 0.001);
        SkillEffectCalculator.GetHealthRegeneration(character).Should().Be(10);
    }

    #endregion
}
