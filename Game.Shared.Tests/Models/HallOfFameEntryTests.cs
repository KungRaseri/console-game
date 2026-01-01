using FluentAssertions;
using Game.Shared.Models;

namespace Game.Shared.Tests.Models;

[Trait("Category", "Unit")]
/// <summary>
/// Comprehensive tests for HallOfFameEntry model.
/// Target: 0% -> 100% coverage.
/// </summary>
public class HallOfFameEntryTests
{
    #region Initialization Tests

    [Fact]
    public void HallOfFameEntry_Should_Initialize_With_Default_Values()
    {
        // Act
        var entry = new HallOfFameEntry();

        // Assert
        entry.Id.Should().NotBeEmpty();
        entry.CharacterName.Should().BeEmpty();
        entry.ClassName.Should().BeEmpty();
        entry.Level.Should().Be(0);
        entry.PlayTimeMinutes.Should().Be(0);
        entry.TotalEnemiesDefeated.Should().Be(0);
        entry.QuestsCompleted.Should().Be(0);
        entry.DeathCount.Should().Be(0);
        entry.DeathReason.Should().Be("Unknown");
        entry.DeathLocation.Should().Be("Unknown");
        entry.DeathDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        entry.AchievementsUnlocked.Should().Be(0);
        entry.IsPermadeath.Should().BeFalse();
        entry.DifficultyLevel.Should().Be("Normal");
    }

    [Fact]
    public void HallOfFameEntry_Should_Generate_Unique_Ids()
    {
        // Act
        var entry1 = new HallOfFameEntry();
        var entry2 = new HallOfFameEntry();

        // Assert
        entry1.Id.Should().NotBe(entry2.Id);
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void HallOfFameEntry_Should_Allow_Character_Name_Assignment()
    {
        // Arrange
        var entry = new HallOfFameEntry();

        // Act
        entry.CharacterName = "Aragorn";

        // Assert
        entry.CharacterName.Should().Be("Aragorn");
    }

    [Fact]
    public void HallOfFameEntry_Should_Allow_Class_Name_Assignment()
    {
        // Arrange
        var entry = new HallOfFameEntry();

        // Act
        entry.ClassName = "Warrior";

        // Assert
        entry.ClassName.Should().Be("Warrior");
    }

    [Fact]
    public void HallOfFameEntry_Should_Allow_Level_Assignment()
    {
        // Arrange
        var entry = new HallOfFameEntry();

        // Act
        entry.Level = 42;

        // Assert
        entry.Level.Should().Be(42);
    }

    [Fact]
    public void HallOfFameEntry_Should_Allow_PlayTime_Assignment()
    {
        // Arrange
        var entry = new HallOfFameEntry();

        // Act
        entry.PlayTimeMinutes = 1234;

        // Assert
        entry.PlayTimeMinutes.Should().Be(1234);
    }

    [Fact]
    public void HallOfFameEntry_Should_Allow_Death_Details_Assignment()
    {
        // Arrange
        var entry = new HallOfFameEntry();
        var deathDate = new DateTime(2025, 12, 11, 0, 0, 0, DateTimeKind.Utc);

        // Act
        entry.DeathReason = "Slain by Dragon";
        entry.DeathLocation = "Dark Cave";
        entry.DeathDate = deathDate;
        entry.DeathCount = 5;

        // Assert
        entry.DeathReason.Should().Be("Slain by Dragon");
        entry.DeathLocation.Should().Be("Dark Cave");
        entry.DeathDate.Should().Be(deathDate);
        entry.DeathCount.Should().Be(5);
    }

    [Fact]
    public void HallOfFameEntry_Should_Allow_Statistics_Assignment()
    {
        // Arrange
        var entry = new HallOfFameEntry();

        // Act
        entry.TotalEnemiesDefeated = 150;
        entry.QuestsCompleted = 25;
        entry.AchievementsUnlocked = 10;

        // Assert
        entry.TotalEnemiesDefeated.Should().Be(150);
        entry.QuestsCompleted.Should().Be(25);
        entry.AchievementsUnlocked.Should().Be(10);
    }

    [Fact]
    public void HallOfFameEntry_Should_Allow_Permadeath_Flag()
    {
        // Arrange
        var entry = new HallOfFameEntry();

        // Act
        entry.IsPermadeath = true;

        // Assert
        entry.IsPermadeath.Should().BeTrue();
    }

    [Fact]
    public void HallOfFameEntry_Should_Allow_Difficulty_Assignment()
    {
        // Arrange
        var entry = new HallOfFameEntry();

        // Act
        entry.DifficultyLevel = "Hard";

        // Assert
        entry.DifficultyLevel.Should().Be("Hard");
    }

    #endregion

    #region GetFameScore Tests

    [Fact]
    public void GetFameScore_Should_Return_Zero_For_Default_Entry()
    {
        // Arrange
        var entry = new HallOfFameEntry();

        // Act
        var score = entry.GetFameScore();

        // Assert
        score.Should().Be(0);
    }

    [Fact]
    public void GetFameScore_Should_Calculate_From_Level()
    {
        // Arrange
        var entry = new HallOfFameEntry { Level = 10 };

        // Act
        entry.CalculateFameScore();
        var score = entry.GetFameScore();

        // Assert - 10 * 100 = 1000
        score.Should().Be(1000);
    }

    [Fact]
    public void GetFameScore_Should_Include_Quests_Completed()
    {
        // Arrange
        var entry = new HallOfFameEntry
        {
            Level = 5,
            QuestsCompleted = 10
        };

        // Act
        entry.CalculateFameScore();
        var score = entry.GetFameScore();

        // Assert - (5 * 100) + (10 * 50) = 500 + 500 = 1000
        score.Should().Be(1000);
    }

    [Fact]
    public void GetFameScore_Should_Include_Enemies_Defeated()
    {
        // Arrange
        var entry = new HallOfFameEntry
        {
            Level = 5,
            TotalEnemiesDefeated = 100
        };

        // Act
        entry.CalculateFameScore();
        var score = entry.GetFameScore();

        // Assert - (5 * 100) + (100 * 5) = 500 + 500 = 1000
        score.Should().Be(1000);
    }

    [Fact]
    public void GetFameScore_Should_Include_Achievements()
    {
        // Arrange
        var entry = new HallOfFameEntry
        {
            Level = 5,
            AchievementsUnlocked = 3
        };

        // Act
        entry.CalculateFameScore();
        var score = entry.GetFameScore();

        // Assert - (5 * 100) + (3 * 200) = 500 + 600 = 1100
        score.Should().Be(1100);
    }

    [Fact]
    public void GetFameScore_Should_Double_For_Permadeath()
    {
        // Arrange
        var entry = new HallOfFameEntry
        {
            Level = 10,
            IsPermadeath = true
        };

        // Act
        entry.CalculateFameScore();
        var score = entry.GetFameScore();

        // Assert - (10 * 100) * 2 = 2000
        score.Should().Be(2000);
    }

    [Fact]
    public void GetFameScore_Should_Not_Double_For_Non_Permadeath()
    {
        // Arrange
        var entry = new HallOfFameEntry
        {
            Level = 10,
            IsPermadeath = false
        };

        // Act
        entry.CalculateFameScore();
        var score = entry.GetFameScore();

        // Assert - (10 * 100) = 1000 (not doubled)
        score.Should().Be(1000);
    }

    [Fact]
    public void GetFameScore_Should_Calculate_Complete_Score()
    {
        // Arrange
        var entry = new HallOfFameEntry
        {
            Level = 20,              // 20 * 100 = 2000
            QuestsCompleted = 15,    // 15 * 50 = 750
            TotalEnemiesDefeated = 200, // 200 * 5 = 1000
            AchievementsUnlocked = 5,   // 5 * 200 = 1000
            IsPermadeath = false
        };
        // Total: 2000 + 750 + 1000 + 1000 = 4750

        // Act
        entry.CalculateFameScore();
        var score = entry.GetFameScore();

        // Assert
        score.Should().Be(4750);
    }

    [Fact]
    public void GetFameScore_Should_Calculate_Complete_Score_With_Permadeath()
    {
        // Arrange
        var entry = new HallOfFameEntry
        {
            Level = 20,              // 20 * 100 = 2000
            QuestsCompleted = 15,    // 15 * 50 = 750
            TotalEnemiesDefeated = 200, // 200 * 5 = 1000
            AchievementsUnlocked = 5,   // 5 * 200 = 1000
            IsPermadeath = true         // * 2
        };
        // Total: (2000 + 750 + 1000 + 1000) * 2 = 9500

        // Act
        entry.CalculateFameScore();
        var score = entry.GetFameScore();

        // Assert
        score.Should().Be(9500);
    }

    #endregion

    #region GetPlaytimeFormatted Tests

    [Fact]
    public void GetPlaytimeFormatted_Should_Return_Zero_Time()
    {
        // Arrange
        var entry = new HallOfFameEntry { PlayTimeMinutes = 0 };

        // Act
        var formatted = entry.GetPlaytimeFormatted();

        // Assert
        formatted.Should().Be("0h 0m");
    }

    [Fact]
    public void GetPlaytimeFormatted_Should_Format_Only_Minutes()
    {
        // Arrange
        var entry = new HallOfFameEntry { PlayTimeMinutes = 45 };

        // Act
        var formatted = entry.GetPlaytimeFormatted();

        // Assert
        formatted.Should().Be("0h 45m");
    }

    [Fact]
    public void GetPlaytimeFormatted_Should_Format_Only_Hours()
    {
        // Arrange
        var entry = new HallOfFameEntry { PlayTimeMinutes = 120 };

        // Act
        var formatted = entry.GetPlaytimeFormatted();

        // Assert
        formatted.Should().Be("2h 0m");
    }

    [Fact]
    public void GetPlaytimeFormatted_Should_Format_Hours_And_Minutes()
    {
        // Arrange
        var entry = new HallOfFameEntry { PlayTimeMinutes = 137 };

        // Act
        var formatted = entry.GetPlaytimeFormatted();

        // Assert - 137 minutes = 2h 17m
        formatted.Should().Be("2h 17m");
    }

    [Fact]
    public void GetPlaytimeFormatted_Should_Format_Large_Playtime()
    {
        // Arrange
        var entry = new HallOfFameEntry { PlayTimeMinutes = 12345 };

        // Act
        var formatted = entry.GetPlaytimeFormatted();

        // Assert - 12345 minutes = 205h 45m
        formatted.Should().Be("205h 45m");
    }

    [Fact]
    public void GetPlaytimeFormatted_Should_Handle_Exactly_One_Hour()
    {
        // Arrange
        var entry = new HallOfFameEntry { PlayTimeMinutes = 60 };

        // Act
        var formatted = entry.GetPlaytimeFormatted();

        // Assert
        formatted.Should().Be("1h 0m");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void HallOfFameEntry_Should_Create_Complete_Normal_Death()
    {
        // Arrange & Act
        var entry = new HallOfFameEntry
        {
            CharacterName = "Thorin",
            ClassName = "Warrior",
            Level = 15,
            PlayTimeMinutes = 540, // 9 hours
            TotalEnemiesDefeated = 89,
            QuestsCompleted = 12,
            DeathCount = 3,
            DeathReason = "Overwhelmed by Goblin Horde",
            DeathLocation = "Mountain Pass",
            AchievementsUnlocked = 7,
            IsPermadeath = false,
            DifficultyLevel = "Hard"
        };

        // Assert
        entry.CharacterName.Should().Be("Thorin");
        entry.Level.Should().Be(15);
        entry.CalculateFameScore();
        entry.GetFameScore().Should().Be(3945); // (15*100 + 12*50 + 89*5 + 7*200) = 1500+600+445+1400 = 3945
        entry.GetPlaytimeFormatted().Should().Be("9h 0m");
    }

    [Fact]
    public void HallOfFameEntry_Should_Create_Complete_Permadeath_Entry()
    {
        // Arrange & Act
        var entry = new HallOfFameEntry
        {
            CharacterName = "Gandalf",
            ClassName = "Mage",
            Level = 50,
            PlayTimeMinutes = 3000, // 50 hours
            TotalEnemiesDefeated = 500,
            QuestsCompleted = 75,
            DeathCount = 0,
            DeathReason = "Fell into the abyss",
            DeathLocation = "Khazad-dûm",
            AchievementsUnlocked = 25,
            IsPermadeath = true,
            DifficultyLevel = "Permadeath"
        };

        // Assert
        entry.IsPermadeath.Should().BeTrue();
        entry.CalculateFameScore();
        entry.GetFameScore().Should().Be(32500); // (50*100 + 75*50 + 500*5 + 25*200) * 2 = 16250 * 2 = 32500
        entry.GetPlaytimeFormatted().Should().Be("50h 0m");
        entry.DeathCount.Should().Be(0); // First death in permadeath
    }

    [Fact]
    public void HallOfFameEntry_Should_Track_Multiple_Death_Run()
    {
        // Arrange & Act
        var entry = new HallOfFameEntry
        {
            CharacterName = "Legolas",
            ClassName = "Ranger",
            Level = 25,
            DeathCount = 7,
            DifficultyLevel = "Normal"
        };

        // Assert
        entry.DeathCount.Should().Be(7);
        entry.DifficultyLevel.Should().Be("Normal");
    }

    [Fact]
    public void HallOfFameEntry_Should_Allow_Zero_Statistics()
    {
        // Arrange & Act
        var entry = new HallOfFameEntry
        {
            CharacterName = "Newbie",
            Level = 1,
            TotalEnemiesDefeated = 0,
            QuestsCompleted = 0,
            AchievementsUnlocked = 0
        };

        // Assert
        entry.CalculateFameScore();
        entry.GetFameScore().Should().Be(100); // Just level * 100
    }

    #endregion
}
