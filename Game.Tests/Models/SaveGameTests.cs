using FluentAssertions;
using Game.Core.Models;

namespace Game.Tests.Models;

public class SaveGameTests
{
    [Fact]
    public void SaveGame_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var saveGame = new SaveGame();

        // Assert
        saveGame.Id.Should().NotBeNullOrEmpty();
        saveGame.PlayerName.Should().Be(string.Empty);
        saveGame.SaveDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        saveGame.Character.Should().NotBeNull();
        saveGame.Character.Inventory.Should().NotBeNull().And.BeEmpty(); // Inventory is in Character now
        saveGame.PlayTimeMinutes.Should().Be(0);
    }

    [Fact]
    public void SaveGame_Should_Generate_Unique_Ids()
    {
        // Arrange & Act
        var save1 = new SaveGame();
        var save2 = new SaveGame();

        // Assert
        save1.Id.Should().NotBe(save2.Id);
    }

    [Fact]
    public void SaveGame_Properties_Should_Be_Settable()
    {
        // Arrange
        var saveGame = new SaveGame();
        var character = new Character { Name = "Hero", Level = 5 };
        var inventory = new List<Item> 
        { 
            new Item { Name = "Sword" },
            new Item { Name = "Potion" }
        };

        // Act
        saveGame.PlayerName = "TestPlayer";
        saveGame.Character = character;
        saveGame.Character.Inventory = inventory; // Inventory is in Character now
        saveGame.PlayTimeMinutes = 120;

        // Assert
        saveGame.PlayerName.Should().Be("TestPlayer");
        saveGame.Character.Name.Should().Be("Hero");
        saveGame.Character.Level.Should().Be(5);
        saveGame.Character.Inventory.Should().HaveCount(2); // Inventory is in Character now
        saveGame.PlayTimeMinutes.Should().Be(120);
    }

    [Fact]
    public void SaveGame_SaveDate_Can_Be_Set()
    {
        // Arrange
        var saveGame = new SaveGame();
        var customDate = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        saveGame.SaveDate = customDate;

        // Assert
        saveGame.SaveDate.Should().Be(customDate);
    }

    [Fact]
    public void SaveGame_Inventory_Can_Be_Modified()
    {
        // Arrange
        var saveGame = new SaveGame();

        // Act
        saveGame.Character.Inventory.Add(new Item { Name = "Sword" });
        saveGame.Character.Inventory.Add(new Item { Name = "Shield" });
        saveGame.Character.Inventory.Add(new Item { Name = "Potion" });

        // Assert
        saveGame.Character.Inventory.Should().HaveCount(3);
        saveGame.Character.Inventory.Should().Contain(i => i.Name == "Sword");
        saveGame.Character.Inventory.Should().Contain(i => i.Name == "Shield");
        saveGame.Character.Inventory.Should().Contain(i => i.Name == "Potion");
    }

    [Fact]
    public void GetSummary_Should_Return_Formatted_String()
    {
        // Arrange
        var saveGame = new SaveGame
        {
            PlayerName = "Aragorn",
            Character = new Character
            {
                Level = 25,
                ClassName = "Ranger"
            },
            PlayTimeMinutes = 137, // 2h 17m
            ActiveQuests = new List<Quest>
            {
                new Quest { Title = "Quest 1" },
                new Quest { Title = "Quest 2" }
            }
        };

        // Act
        var summary = saveGame.GetSummary();

        // Assert
        summary.Should().Be("Aragorn - Level 25 Ranger - 2h 17m - 2 active quests");
    }

    [Fact]
    public void GetSummary_Should_Handle_Zero_Playtime()
    {
        // Arrange
        var saveGame = new SaveGame
        {
            PlayerName = "Newbie",
            Character = new Character { Level = 1, ClassName = "Warrior" },
            PlayTimeMinutes = 0
        };

        // Act
        var summary = saveGame.GetSummary();

        // Assert
        summary.Should().Be("Newbie - Level 1 Warrior - 0h 0m - 0 active quests");
    }

    [Fact]
    public void GetCompletionPercentage_Should_Return_Zero_For_New_Game()
    {
        // Arrange
        var saveGame = new SaveGame();

        // Act
        var completion = saveGame.GetCompletionPercentage();

        // Assert
        completion.Should().Be(0);
    }

    [Fact]
    public void GetCompletionPercentage_Should_Calculate_Quest_Completion()
    {
        // Arrange
        var saveGame = new SaveGame
        {
            CompletedQuests = new List<Quest>
            {
                new Quest { Title = "Q1" },
                new Quest { Title = "Q2" }
            },
            ActiveQuests = new List<Quest>
            {
                new Quest { Title = "Q3" }
            },
            FailedQuests = new List<Quest>
            {
                new Quest { Title = "Q4" }
            }
        };
        // 2 completed out of 4 total = 50% * 40 weight = 20% from quests
        // 0 achievements = 0% * 30 weight = 0%
        // 0 locations = 0% * 30 weight = 0%
        // Total: 20% out of 70% possible = 28.57%

        // Act
        var completion = saveGame.GetCompletionPercentage();

        // Assert
        completion.Should().BeApproximately(28.57, 0.01);
    }

    [Fact]
    public void GetCompletionPercentage_Should_Calculate_Location_Discovery()
    {
        // Arrange
        var saveGame = new SaveGame
        {
            VisitedLocations = new List<string> { "Town", "Cave" },
            DiscoveredLocations = new List<string> { "Castle", "Mountain" }
        };
        // 2 visited out of 4 total = 50% * 30 weight = 15% from locations
        // 0 achievements = 0% * 30 weight = 0%
        // Total: 15% out of 60% possible (30 locations + 30 achievements) = 25%

        // Act
        var completion = saveGame.GetCompletionPercentage();

        // Assert
        completion.Should().Be(25);
    }

    [Fact]
    public void GetCompletionPercentage_Should_Calculate_Achievement_Completion()
    {
        // Arrange
        var saveGame = new SaveGame
        {
            UnlockedAchievements = new List<string>
            {
                "A1", "A2", "A3", "A4", "A5",
                "A6", "A7", "A8", "A9", "A10"
            }
        };
        // 10 out of 50 achievements = 20% * 30 weight = 6%
        // Total: 6% out of 30% possible = 20%

        // Act
        var completion = saveGame.GetCompletionPercentage();

        // Assert
        completion.Should().Be(20);
    }

    [Fact]
    public void GetCompletionPercentage_Should_Calculate_Complete_Game()
    {
        // Arrange
        var saveGame = new SaveGame
        {
            // All quests completed
            CompletedQuests = new List<Quest> { new Quest(), new Quest(), new Quest(), new Quest() },
            ActiveQuests = new List<Quest>(),
            FailedQuests = new List<Quest>(),
            // All locations visited
            VisitedLocations = new List<string> { "L1", "L2", "L3", "L4", "L5" },
            DiscoveredLocations = new List<string>(),
            // All achievements
            UnlockedAchievements = Enumerable.Range(1, 50).Select(i => $"A{i}").ToList()
        };

        // Act
        var completion = saveGame.GetCompletionPercentage();

        // Assert
        completion.Should().Be(100);
    }

    [Fact]
    public void SaveGame_Should_Support_Difficulty_Settings()
    {
        // Arrange
        var saveGame = new SaveGame();

        // Act
        saveGame.DifficultyLevel = "Hard";
        saveGame.IronmanMode = true;
        saveGame.PermadeathMode = true;
        saveGame.ApocalypseMode = true;

        // Assert
        saveGame.DifficultyLevel.Should().Be("Hard");
        saveGame.IronmanMode.Should().BeTrue();
        saveGame.PermadeathMode.Should().BeTrue();
        saveGame.ApocalypseMode.Should().BeTrue();
    }

    [Fact]
    public void SaveGame_Should_Track_Combat_Statistics()
    {
        // Arrange
        var saveGame = new SaveGame();

        // Act
        saveGame.TotalEnemiesDefeated = 100;
        saveGame.EnemiesDefeatedByType["dragon"] = 5;
        saveGame.EnemiesDefeatedByType["goblin"] = 50;
        saveGame.LegendaryEnemiesDefeated.Add(new Enemy { Name = "Ancient Dragon" });

        // Assert
        saveGame.TotalEnemiesDefeated.Should().Be(100);
        saveGame.EnemiesDefeatedByType.Should().HaveCount(2);
        saveGame.LegendaryEnemiesDefeated.Should().HaveCount(1);
    }

    [Fact]
    public void SaveGame_Should_Track_Quest_Statistics()
    {
        // Arrange
        var saveGame = new SaveGame();

        // Act
        saveGame.QuestsCompleted = 25;
        saveGame.QuestsFailed = 3;
        saveGame.ActiveQuests.Add(new Quest { Title = "Main Quest" });
        saveGame.CompletedQuests.Add(new Quest { Title = "Side Quest" });

        // Assert
        saveGame.QuestsCompleted.Should().Be(25);
        saveGame.QuestsFailed.Should().Be(3);
        saveGame.ActiveQuests.Should().HaveCount(1);
        saveGame.CompletedQuests.Should().HaveCount(1);
    }

    [Fact]
    public void SaveGame_Should_Track_NPC_Relationships()
    {
        // Arrange
        var saveGame = new SaveGame();
        var npc = new NPC { Id = "npc1", Name = "Merchant" };

        // Act
        saveGame.KnownNPCs.Add(npc);
        saveGame.NPCRelationships["npc1"] = 75;

        // Assert
        saveGame.KnownNPCs.Should().HaveCount(1);
        saveGame.NPCRelationships["npc1"].Should().Be(75);
    }

    [Fact]
    public void SaveGame_Should_Support_Death_Tracking()
    {
        // Arrange
        var saveGame = new SaveGame();
        var deathDate = new DateTime(2025, 12, 11, 0, 0, 0, DateTimeKind.Utc);

        // Act
        saveGame.DeathCount = 5;
        saveGame.LastDeathLocation = "Dark Cave";
        saveGame.LastDeathDate = deathDate;
        saveGame.DroppedItemsAtLocations["Dark Cave"] = new List<Item>
        {
            new Item { Name = "Lost Sword" }
        };

        // Assert
        saveGame.DeathCount.Should().Be(5);
        saveGame.LastDeathLocation.Should().Be("Dark Cave");
        saveGame.LastDeathDate.Should().Be(deathDate);
        saveGame.DroppedItemsAtLocations["Dark Cave"].Should().HaveCount(1);
    }

    [Fact]
    public void SaveGame_Should_Support_Apocalypse_Mode()
    {
        // Arrange
        var saveGame = new SaveGame();
        var startTime = new DateTime(2025, 12, 11, 0, 0, 0, DateTimeKind.Utc);

        // Act
        saveGame.ApocalypseMode = true;
        saveGame.ApocalypseStartTime = startTime;
        saveGame.ApocalypseBonusMinutes = 15;

        // Assert
        saveGame.ApocalypseMode.Should().BeTrue();
        saveGame.ApocalypseStartTime.Should().Be(startTime);
        saveGame.ApocalypseBonusMinutes.Should().Be(15);
    }
}
