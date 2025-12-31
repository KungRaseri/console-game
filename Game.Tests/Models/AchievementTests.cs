using FluentAssertions;
using Game.Shared.Models;

namespace Game.Tests.Models;

/// <summary>
/// Tests for Achievement model.
/// </summary>
public class AchievementTests
{
    #region Initialization Tests

    [Fact]
    public void Achievement_Should_Initialize_With_Default_Values()
    {
        // Act
        var achievement = new Achievement();

        // Assert
        achievement.Id.Should().BeEmpty();
        achievement.Title.Should().BeEmpty();
        achievement.Description.Should().BeEmpty();
        achievement.Icon.Should().Be("🏆");
        achievement.Points.Should().Be(0);
        achievement.IsSecret.Should().BeFalse();
        achievement.IsUnlocked.Should().BeFalse();
        achievement.UnlockedAt.Should().BeNull();
        achievement.Criteria.Should().NotBeNull();
    }

    [Fact]
    public void Achievement_Properties_Should_Be_Settable()
    {
        // Arrange
        var achievement = new Achievement();

        // Act
        achievement.Id = "ach_001";
        achievement.Title = "Dragon Slayer";
        achievement.Description = "Defeat 100 dragons";
        achievement.Icon = "🐉";
        achievement.Category = AchievementCategory.Combat;
        achievement.Points = 50;
        achievement.IsSecret = true;

        // Assert
        achievement.Id.Should().Be("ach_001");
        achievement.Title.Should().Be("Dragon Slayer");
        achievement.Description.Should().Be("Defeat 100 dragons");
        achievement.Icon.Should().Be("🐉");
        achievement.Category.Should().Be(AchievementCategory.Combat);
        achievement.Points.Should().Be(50);
        achievement.IsSecret.Should().BeTrue();
    }

    #endregion

    #region Category Tests

    [Theory]
    [InlineData(AchievementCategory.Combat)]
    [InlineData(AchievementCategory.Exploration)]
    [InlineData(AchievementCategory.Quests)]
    [InlineData(AchievementCategory.Survival)]
    [InlineData(AchievementCategory.Mastery)]
    [InlineData(AchievementCategory.Secret)]
    public void Achievement_Should_Support_All_Categories(AchievementCategory category)
    {
        // Arrange
        var achievement = new Achievement { Category = category };

        // Assert
        achievement.Category.Should().Be(category);
    }

    #endregion

    #region Unlock Status Tests

    [Fact]
    public void Achievement_Should_Track_Unlock_Status()
    {
        // Arrange
        var achievement = new Achievement();

        // Act
        achievement.IsUnlocked = true;
        achievement.UnlockedAt = DateTime.UtcNow;

        // Assert
        achievement.IsUnlocked.Should().BeTrue();
        achievement.UnlockedAt.Should().NotBeNull();
        achievement.UnlockedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Achievement_Should_Start_Locked()
    {
        // Arrange
        var achievement = new Achievement();

        // Assert
        achievement.IsUnlocked.Should().BeFalse();
        achievement.UnlockedAt.Should().BeNull();
    }

    #endregion

    #region Criteria Tests

    [Fact]
    public void AchievementCriteria_Should_Initialize()
    {
        // Act
        var criteria = new AchievementCriteria();

        // Assert
        criteria.Type.Should().Be(default(AchievementType));
        criteria.RequiredValue.Should().Be(0);
        criteria.RequiredId.Should().BeNull();
    }

    [Fact]
    public void AchievementCriteria_Should_Set_Type_And_Value()
    {
        // Arrange
        var criteria = new AchievementCriteria();

        // Act
        criteria.Type = AchievementType.DefeatEnemies;
        criteria.RequiredValue = 100;

        // Assert
        criteria.Type.Should().Be(AchievementType.DefeatEnemies);
        criteria.RequiredValue.Should().Be(100);
    }

    [Fact]
    public void AchievementCriteria_Should_Support_RequiredId()
    {
        // Arrange
        var criteria = new AchievementCriteria();

        // Act
        criteria.Type = AchievementType.CompleteQuest;
        criteria.RequiredId = "quest_main_01";

        // Assert
        criteria.RequiredId.Should().Be("quest_main_01");
    }

    [Theory]
    [InlineData(AchievementType.CompleteQuest)]
    [InlineData(AchievementType.DefeatEnemies)]
    [InlineData(AchievementType.ReachLevel)]
    [InlineData(AchievementType.CollectGold)]
    [InlineData(AchievementType.SurviveTime)]
    [InlineData(AchievementType.CompleteGame)]
    [InlineData(AchievementType.CompleteDifficulty)]
    [InlineData(AchievementType.Deathless)]
    public void AchievementCriteria_Should_Support_All_Types(AchievementType type)
    {
        // Arrange
        var criteria = new AchievementCriteria { Type = type };

        // Assert
        criteria.Type.Should().Be(type);
    }

    #endregion

    #region Points Tests

    [Theory]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(100)]
    public void Achievement_Should_Have_Different_Point_Values(int points)
    {
        // Arrange
        var achievement = new Achievement { Points = points };

        // Assert
        achievement.Points.Should().Be(points);
    }

    #endregion

    #region Secret Achievements Tests

    [Fact]
    public void Achievement_Can_Be_Secret()
    {
        // Arrange
        var achievement = new Achievement
        {
            Title = "Hidden Master",
            IsSecret = true
        };

        // Assert
        achievement.IsSecret.Should().BeTrue();
    }

    [Fact]
    public void Achievement_Can_Be_Public()
    {
        // Arrange
        var achievement = new Achievement
        {
            Title = "Novice Explorer",
            IsSecret = false
        };

        // Assert
        achievement.IsSecret.Should().BeFalse();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Achievement_Combat_Complete_Workflow()
    {
        // Arrange
        var achievement = new Achievement
        {
            Id = "combat_master",
            Title = "Combat Master",
            Description = "Defeat 1000 enemies",
            Icon = "⚔️",
            Category = AchievementCategory.Combat,
            Points = 100,
            IsSecret = false,
            Criteria = new AchievementCriteria
            {
                Type = AchievementType.DefeatEnemies,
                RequiredValue = 1000
            }
        };

        // Assert - Before unlock
        achievement.IsUnlocked.Should().BeFalse();
        achievement.UnlockedAt.Should().BeNull();

        // Act - Unlock
        achievement.IsUnlocked = true;
        achievement.UnlockedAt = DateTime.UtcNow;

        // Assert - After unlock
        achievement.IsUnlocked.Should().BeTrue();
        achievement.UnlockedAt.Should().NotBeNull();
        achievement.Points.Should().Be(100);
    }

    [Fact]
    public void Achievement_Quest_Complete_Workflow()
    {
        // Arrange
        var achievement = new Achievement
        {
            Id = "legendary_hero",
            Title = "Legendary Hero",
            Description = "Complete the legendary quest chain",
            Icon = "🌟",
            Category = AchievementCategory.Quests,
            Points = 250,
            IsSecret = true,
            Criteria = new AchievementCriteria
            {
                Type = AchievementType.CompleteQuest,
                RequiredId = "legendary_quest_final"
            }
        };

        // Assert
        achievement.IsSecret.Should().BeTrue();
        achievement.Category.Should().Be(AchievementCategory.Quests);
        achievement.Criteria.RequiredId.Should().Be("legendary_quest_final");
    }

    [Fact]
    public void Achievement_Level_Based_Workflow()
    {
        // Arrange
        var achievement = new Achievement
        {
            Id = "max_level",
            Title = "Max Level",
            Description = "Reach level 100",
            Category = AchievementCategory.Mastery,
            Points = 200,
            Criteria = new AchievementCriteria
            {
                Type = AchievementType.ReachLevel,
                RequiredValue = 100
            }
        };

        // Assert
        achievement.Criteria.Type.Should().Be(AchievementType.ReachLevel);
        achievement.Criteria.RequiredValue.Should().Be(100);
    }

    [Fact]
    public void Achievement_Survival_Workflow()
    {
        // Arrange
        var achievement = new Achievement
        {
            Id = "survivor",
            Title = "Survivor",
            Description = "Survive for 100 days",
            Category = AchievementCategory.Survival,
            Points = 150,
            Criteria = new AchievementCriteria
            {
                Type = AchievementType.SurviveTime,
                RequiredValue = 100
            }
        };

        // Assert
        achievement.Category.Should().Be(AchievementCategory.Survival);
        achievement.Criteria.Type.Should().Be(AchievementType.SurviveTime);
    }

    [Fact]
    public void Achievement_Deathless_Workflow()
    {
        // Arrange
        var achievement = new Achievement
        {
            Id = "deathless_run",
            Title = "Deathless",
            Description = "Complete the game without dying",
            Icon = "💀",
            Category = AchievementCategory.Mastery,
            Points = 500,
            IsSecret = true,
            Criteria = new AchievementCriteria
            {
                Type = AchievementType.Deathless,
                RequiredValue = 1
            }
        };

        // Assert
        achievement.Points.Should().Be(500);
        achievement.IsSecret.Should().BeTrue();
        achievement.Criteria.Type.Should().Be(AchievementType.Deathless);
    }

    [Fact]
    public void Achievement_With_Custom_Icon()
    {
        // Arrange
        var achievement = new Achievement
        {
            Title = "Gold Hoarder",
            Icon = "💰",
            Criteria = new AchievementCriteria
            {
                Type = AchievementType.CollectGold,
                RequiredValue = 100000
            }
        };

        // Assert
        achievement.Icon.Should().Be("💰");
    }

    #endregion
}
