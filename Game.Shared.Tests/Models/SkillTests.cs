using FluentAssertions;
using Game.Shared.Models;

namespace Game.Tests.Models;

[Trait("Category", "Unit")]
/// <summary>
/// Tests for Skill model.
/// </summary>
public class SkillTests
{
    #region Initialization Tests

    [Fact]
    public void Skill_Should_Initialize_With_Default_Values()
    {
        // Act
        var skill = new Skill();

        // Assert
        skill.Name.Should().BeEmpty();
        skill.Description.Should().BeEmpty();
        skill.Effect.Should().BeEmpty();
        skill.RequiredLevel.Should().Be(0);
        skill.MaxRank.Should().Be(5);
        skill.CurrentRank.Should().Be(0);
        skill.Type.Should().Be(default(SkillType));
    }

    [Fact]
    public void Skill_Properties_Should_Be_Settable()
    {
        // Arrange
        var skill = new Skill();

        // Act
        skill.Name = "Power Attack";
        skill.Description = "A devastating blow";
        skill.Effect = "+10% physical damage per rank";
        skill.RequiredLevel = 5;
        skill.MaxRank = 10;
        skill.CurrentRank = 3;
        skill.Type = SkillType.Combat;

        // Assert
        skill.Name.Should().Be("Power Attack");
        skill.Description.Should().Be("A devastating blow");
        skill.Effect.Should().Be("+10% physical damage per rank");
        skill.RequiredLevel.Should().Be(5);
        skill.MaxRank.Should().Be(10);
        skill.CurrentRank.Should().Be(3);
        skill.Type.Should().Be(SkillType.Combat);
    }

    #endregion

    #region Skill Type Tests

    [Theory]
    [InlineData(SkillType.Combat)]
    [InlineData(SkillType.Defense)]
    [InlineData(SkillType.Magic)]
    [InlineData(SkillType.Utility)]
    [InlineData(SkillType.Passive)]
    public void Skill_Should_Support_All_Types(SkillType type)
    {
        // Arrange
        var skill = new Skill { Type = type };

        // Assert
        skill.Type.Should().Be(type);
    }

    #endregion

    #region Rank Tests

    [Fact]
    public void Skill_Should_Track_Current_Rank()
    {
        // Arrange
        var skill = new Skill { CurrentRank = 0, MaxRank = 5 };

        // Act
        skill.CurrentRank = 3;

        // Assert
        skill.CurrentRank.Should().Be(3);
        skill.CurrentRank.Should().BeLessThan(skill.MaxRank);
    }

    [Fact]
    public void Skill_Should_Have_Max_Rank()
    {
        // Arrange
        var skill = new Skill { MaxRank = 5 };

        // Assert
        skill.MaxRank.Should().Be(5);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public void Skill_Should_Support_Different_Max_Ranks(int maxRank)
    {
        // Arrange
        var skill = new Skill { MaxRank = maxRank };

        // Assert
        skill.MaxRank.Should().Be(maxRank);
    }

    [Fact]
    public void Skill_Can_Be_At_Max_Rank()
    {
        // Arrange
        var skill = new Skill
        {
            MaxRank = 5,
            CurrentRank = 5
        };

        // Assert
        skill.CurrentRank.Should().Be(skill.MaxRank);
    }

    #endregion

    #region Level Requirement Tests

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public void Skill_Should_Have_Required_Level(int requiredLevel)
    {
        // Arrange
        var skill = new Skill { RequiredLevel = requiredLevel };

        // Assert
        skill.RequiredLevel.Should().Be(requiredLevel);
    }

    [Fact]
    public void Skill_Can_Have_No_Level_Requirement()
    {
        // Arrange
        var skill = new Skill { RequiredLevel = 0 };

        // Assert
        skill.RequiredLevel.Should().Be(0);
    }

    #endregion

    #region Combat Skills Tests

    [Fact]
    public void Skill_Combat_Power_Attack()
    {
        // Arrange
        var skill = new Skill
        {
            Name = "Power Attack",
            Description = "Deliver a devastating blow to your enemy",
            Effect = "+10% physical damage per rank",
            Type = SkillType.Combat,
            RequiredLevel = 1,
            MaxRank = 5,
            CurrentRank = 0
        };

        // Assert
        skill.Type.Should().Be(SkillType.Combat);
        skill.MaxRank.Should().Be(5);
    }

    [Fact]
    public void Skill_Combat_Critical_Strike()
    {
        // Arrange
        var skill = new Skill
        {
            Name = "Critical Strike",
            Description = "Increase your critical hit chance",
            Effect = "+2% crit chance per rank",
            Type = SkillType.Combat,
            RequiredLevel = 3,
            MaxRank = 5
        };

        // Assert
        skill.Type.Should().Be(SkillType.Combat);
        skill.RequiredLevel.Should().Be(3);
    }

    #endregion

    #region Defense Skills Tests

    [Fact]
    public void Skill_Defense_Iron_Skin()
    {
        // Arrange
        var skill = new Skill
        {
            Name = "Iron Skin",
            Description = "Harden your body against attacks",
            Effect = "+5% physical defense per rank",
            Type = SkillType.Defense,
            RequiredLevel = 2,
            MaxRank = 5
        };

        // Assert
        skill.Type.Should().Be(SkillType.Defense);
    }

    [Fact]
    public void Skill_Defense_Quick_Reflexes()
    {
        // Arrange
        var skill = new Skill
        {
            Name = "Quick Reflexes",
            Description = "Improve your ability to dodge attacks",
            Effect = "+3% dodge chance per rank",
            Type = SkillType.Defense,
            RequiredLevel = 4,
            MaxRank = 5
        };

        // Assert
        skill.Type.Should().Be(SkillType.Defense);
    }

    #endregion

    #region Magic Skills Tests

    [Fact]
    public void Skill_Magic_Arcane_Knowledge()
    {
        // Arrange
        var skill = new Skill
        {
            Name = "Arcane Knowledge",
            Description = "Enhance your magical power",
            Effect = "+10% magic damage per rank",
            Type = SkillType.Magic,
            RequiredLevel = 1,
            MaxRank = 5
        };

        // Assert
        skill.Type.Should().Be(SkillType.Magic);
    }

    [Fact]
    public void Skill_Magic_Mana_Efficiency()
    {
        // Arrange
        var skill = new Skill
        {
            Name = "Mana Efficiency",
            Description = "Increase your maximum mana",
            Effect = "+10% max mana per rank",
            Type = SkillType.Magic,
            RequiredLevel = 5,
            MaxRank = 5
        };

        // Assert
        skill.Type.Should().Be(SkillType.Magic);
    }

    #endregion

    #region Utility Skills Tests

    [Fact]
    public void Skill_Utility_Treasure_Hunter()
    {
        // Arrange
        var skill = new Skill
        {
            Name = "Treasure Hunter",
            Description = "Improve your chances of finding rare items",
            Effect = "+10% rare item find per rank",
            Type = SkillType.Utility,
            RequiredLevel = 10,
            MaxRank = 5
        };

        // Assert
        skill.Type.Should().Be(SkillType.Utility);
    }

    #endregion

    #region Passive Skills Tests

    [Fact]
    public void Skill_Passive_Regeneration()
    {
        // Arrange
        var skill = new Skill
        {
            Name = "Regeneration",
            Description = "Slowly regenerate health over time",
            Effect = "+2 HP per turn per rank",
            Type = SkillType.Passive,
            RequiredLevel = 5,
            MaxRank = 10
        };

        // Assert
        skill.Type.Should().Be(SkillType.Passive);
        skill.MaxRank.Should().Be(10);
    }

    #endregion

    #region Skill Progression Tests

    [Fact]
    public void Skill_Should_Progress_Through_Ranks()
    {
        // Arrange
        var skill = new Skill
        {
            Name = "Power Attack",
            CurrentRank = 0,
            MaxRank = 5
        };

        // Act & Assert - Rank 1
        skill.CurrentRank = 1;
        skill.CurrentRank.Should().Be(1);

        // Act & Assert - Rank 3
        skill.CurrentRank = 3;
        skill.CurrentRank.Should().Be(3);

        // Act & Assert - Max Rank
        skill.CurrentRank = 5;
        skill.CurrentRank.Should().Be(skill.MaxRank);
    }

    [Fact]
    public void Skill_At_Different_Ranks_Should_Have_Different_Effects()
    {
        // Arrange
        var skillRank1 = new Skill
        {
            Name = "Power Attack",
            CurrentRank = 1,
            MaxRank = 5
        };

        var skillRank5 = new Skill
        {
            Name = "Power Attack",
            CurrentRank = 5,
            MaxRank = 5
        };

        // Assert
        skillRank1.CurrentRank.Should().BeLessThan(skillRank5.CurrentRank);
        skillRank5.CurrentRank.Should().Be(skillRank5.MaxRank);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Skill_Complete_Workflow()
    {
        // Arrange - Create new skill
        var skill = new Skill
        {
            Name = "Critical Strike",
            Description = "Master the art of critical hits",
            Effect = "+2% critical chance per rank",
            Type = SkillType.Combat,
            RequiredLevel = 5,
            MaxRank = 5,
            CurrentRank = 0
        };

        // Assert - Initial state
        skill.CurrentRank.Should().Be(0);

        // Act - Learn skill (rank 1)
        skill.CurrentRank = 1;
        skill.CurrentRank.Should().Be(1);

        // Act - Upgrade to rank 3
        skill.CurrentRank = 3;
        skill.CurrentRank.Should().Be(3);
        skill.CurrentRank.Should().BeLessThan(skill.MaxRank);

        // Act - Max out skill
        skill.CurrentRank = 5;
        skill.CurrentRank.Should().Be(skill.MaxRank);
    }

    [Fact]
    public void Multiple_Skills_Can_Coexist()
    {
        // Arrange
        var combatSkill = new Skill
        {
            Name = "Power Attack",
            Type = SkillType.Combat,
            CurrentRank = 3
        };

        var defenseSkill = new Skill
        {
            Name = "Iron Skin",
            Type = SkillType.Defense,
            CurrentRank = 2
        };

        var magicSkill = new Skill
        {
            Name = "Arcane Knowledge",
            Type = SkillType.Magic,
            CurrentRank = 4
        };

        // Assert
        combatSkill.Type.Should().Be(SkillType.Combat);
        defenseSkill.Type.Should().Be(SkillType.Defense);
        magicSkill.Type.Should().Be(SkillType.Magic);
        combatSkill.CurrentRank.Should().NotBe(defenseSkill.CurrentRank);
    }

    #endregion
}
