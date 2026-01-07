using FluentAssertions;
using RealmEngine.Shared.Models;

namespace RealmEngine.Shared.Tests.Models;

[Trait("Category", "Unit")]
public class LevelUpTests
{
    [Fact]
    public void GainExperience_Should_Trigger_Level_Up_At_Threshold()
    {
        // Arrange
        var character = new Character { Level = 1, Experience = 0 };

        // Act
        character.GainExperience(100); // Level 1 needs 100 XP

        // Assert
        character.Level.Should().Be(2);
        character.PendingLevelUps.Should().HaveCount(1);
        character.PendingLevelUps[0].NewLevel.Should().Be(2);
    }

    [Fact]
    public void LevelUp_Should_Award_Attribute_Points()
    {
        // Arrange
        var character = new Character { Level = 1, Experience = 0 };
        int initialPoints = character.UnspentAttributePoints;

        // Act
        character.GainExperience(100);

        // Assert
        character.UnspentAttributePoints.Should().Be(initialPoints + 3); // 3 points per level
    }

    [Fact]
    public void LevelUp_Should_Award_Skill_Points()
    {
        // Arrange
        var character = new Character { Level = 1, Experience = 0 };
        int initialPoints = character.UnspentSkillPoints;

        // Act
        character.GainExperience(100);

        // Assert
        character.UnspentSkillPoints.Should().Be(initialPoints + 1); // 1 point per level
    }

    [Fact]
    public void LevelUp_Should_Award_Bonus_Points_At_Level_5()
    {
        // Arrange
        var character = new Character { Level = 4, Experience = 0 };
        int initialAttrPoints = character.UnspentAttributePoints;
        int initialSkillPoints = character.UnspentSkillPoints;

        // Act
        character.GainExperience(400); // Level up to 5

        // Assert
        character.Level.Should().Be(5);
        character.UnspentAttributePoints.Should().Be(initialAttrPoints + 5); // 3 + 2 bonus
        character.UnspentSkillPoints.Should().Be(initialSkillPoints + 2); // 1 + 1 bonus
    }

    [Fact]
    public void LevelUp_Should_Fully_Heal_Character()
    {
        // Arrange
        var character = new Character
        {
            Level = 1,
            Experience = 0,
            Health = 50,
            MaxHealth = 100,
            Mana = 25,
            MaxMana = 50
        };

        // Act
        character.GainExperience(100);

        // Assert
        character.Health.Should().Be(character.MaxHealth);
        character.Mana.Should().Be(character.MaxMana);
    }

    [Fact]
    public void Multiple_Level_Ups_Should_Queue_Correctly()
    {
        // Arrange
        var character = new Character { Level = 1, Experience = 0 };

        // Act - Gain enough XP to go from 1 to 3
        character.GainExperience(100 + 200); // 100 for lvl 2, 200 for lvl 3

        // Assert
        character.Level.Should().Be(3);
        character.PendingLevelUps.Should().HaveCount(2);
        character.PendingLevelUps[0].NewLevel.Should().Be(2);
        character.PendingLevelUps[1].NewLevel.Should().Be(3);
    }

    [Fact]
    public void AttributePointAllocation_Should_Track_Total_Points()
    {
        // Arrange
        var allocation = new AttributePointAllocation();

        // Act
        allocation.StrengthPoints = 2;
        allocation.DexterityPoints = 1;
        allocation.ConstitutionPoints = 3;

        // Assert
        allocation.TotalPointsAllocated.Should().Be(6);
    }

    [Fact]
    public void AttributePointAllocation_Reset_Should_Clear_All_Points()
    {
        // Arrange
        var allocation = new AttributePointAllocation
        {
            StrengthPoints = 2,
            DexterityPoints = 1,
            ConstitutionPoints = 3,
            IntelligencePoints = 1,
            WisdomPoints = 2,
            CharismaPoints = 1
        };

        // Act
        allocation.Reset();

        // Assert
        allocation.TotalPointsAllocated.Should().Be(0);
        allocation.StrengthPoints.Should().Be(0);
        allocation.DexterityPoints.Should().Be(0);
        allocation.ConstitutionPoints.Should().Be(0);
        allocation.IntelligencePoints.Should().Be(0);
        allocation.WisdomPoints.Should().Be(0);
        allocation.CharismaPoints.Should().Be(0);
    }

    [Fact]
    public void Skill_Can_Be_Upgraded_To_Max_Rank()
    {
        // Arrange
        var skill = new Skill
        {
            Name = "Power Attack",
            MaxRank = 5,
            CurrentRank = 0
        };

        // Act
        skill.CurrentRank = 5;

        // Assert
        skill.CurrentRank.Should().Be(5);
        skill.CurrentRank.Should().BeLessThanOrEqualTo(skill.MaxRank);
    }

    [Fact]
    public void LevelUpInfo_Should_Track_Points_Gained()
    {
        // Arrange & Act
        var levelUpInfo = new LevelUpInfo
        {
            NewLevel = 5,
            AttributePointsGained = 5, // Includes bonus
            SkillPointsGained = 2,
            IsProcessed = false
        };

        // Assert
        levelUpInfo.NewLevel.Should().Be(5);
        levelUpInfo.AttributePointsGained.Should().Be(5);
        levelUpInfo.SkillPointsGained.Should().Be(2);
        levelUpInfo.IsProcessed.Should().BeFalse();
    }

    [Fact]
    public void Character_Can_Learn_Multiple_Skills()
    {
        // Arrange
        var character = new Character();

        // Act
        character.Skills["power-attack"] = new CharacterSkill { SkillId = "power-attack", CurrentRank = 3 };
        character.Skills["iron-skin"] = new CharacterSkill { SkillId = "iron-skin", CurrentRank = 2 };
        character.Skills["critical-strike"] = new CharacterSkill { SkillId = "critical-strike", CurrentRank = 1 };

        // Assert
        character.Skills.Should().HaveCount(3);
        character.Skills.Should().ContainKey("power-attack");
        character.Skills.Should().ContainKey("iron-skin");
        character.Skills.Should().ContainKey("critical-strike");
    }
}
