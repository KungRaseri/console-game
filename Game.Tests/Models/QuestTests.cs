using FluentAssertions;
using Game.Models;
using Xunit;

namespace Game.Tests.Models;

/// <summary>
/// Tests for Quest model.
/// </summary>
public class QuestTests
{
    #region Initialization Tests

    [Fact]
    public void Quest_Should_Initialize_With_Default_Values()
    {
        // Act
        var quest = new Quest();

        // Assert
        quest.Id.Should().NotBeEmpty();
        quest.Title.Should().BeEmpty();
        quest.Description.Should().BeEmpty();
        quest.QuestType.Should().BeEmpty();
        quest.Difficulty.Should().BeEmpty();
        quest.Type.Should().Be("side");
        quest.IsActive.Should().BeFalse();
        quest.IsCompleted.Should().BeFalse();
        quest.Progress.Should().Be(0);
        quest.Quantity.Should().Be(1);
        quest.TimeLimit.Should().Be(0);
        quest.ApocalypseBonusMinutes.Should().Be(0);
    }

    [Fact]
    public void Quest_Should_Generate_Unique_Ids()
    {
        // Act
        var quest1 = new Quest();
        var quest2 = new Quest();

        // Assert
        quest1.Id.Should().NotBe(quest2.Id);
    }

    [Fact]
    public void Quest_Properties_Should_Be_Settable()
    {
        // Arrange
        var quest = new Quest();

        // Act
        quest.Title = "Dragon Slayer";
        quest.Description = "Defeat the ancient dragon";
        quest.QuestType = "kill";
        quest.Difficulty = "hard";
        quest.Type = "main";
        quest.GoldReward = 500;
        quest.XpReward = 1000;

        // Assert
        quest.Title.Should().Be("Dragon Slayer");
        quest.Description.Should().Be("Defeat the ancient dragon");
        quest.QuestType.Should().Be("kill");
        quest.Difficulty.Should().Be("hard");
        quest.Type.Should().Be("main");
        quest.GoldReward.Should().Be(500);
        quest.XpReward.Should().Be(1000);
    }

    #endregion

    #region Quest Types Tests

    [Theory]
    [InlineData("kill")]
    [InlineData("fetch")]
    [InlineData("escort")]
    [InlineData("investigate")]
    [InlineData("delivery")]
    public void Quest_Should_Support_Different_Quest_Types(string questType)
    {
        // Arrange
        var quest = new Quest { QuestType = questType };

        // Assert
        quest.QuestType.Should().Be(questType);
    }

    [Theory]
    [InlineData("easy")]
    [InlineData("medium")]
    [InlineData("hard")]
    public void Quest_Should_Support_Different_Difficulties(string difficulty)
    {
        // Arrange
        var quest = new Quest { Difficulty = difficulty };

        // Assert
        quest.Difficulty.Should().Be(difficulty);
    }

    [Theory]
    [InlineData("main")]
    [InlineData("side")]
    [InlineData("legendary")]
    public void Quest_Should_Support_Different_Types(string type)
    {
        // Arrange
        var quest = new Quest { Type = type };

        // Assert
        quest.Type.Should().Be(type);
    }

    #endregion

    #region Quest Objectives Tests

    [Fact]
    public void Quest_Should_Track_Objectives()
    {
        // Arrange
        var quest = new Quest();

        // Act
        quest.Objectives["Kill Goblins"] = 10;
        quest.Objectives["Collect Items"] = 5;
        quest.ObjectiveProgress["Kill Goblins"] = 3;

        // Assert
        quest.Objectives.Should().HaveCount(2);
        quest.Objectives["Kill Goblins"].Should().Be(10);
        quest.ObjectiveProgress["Kill Goblins"].Should().Be(3);
    }

    [Fact]
    public void Quest_Should_Have_Prerequisites()
    {
        // Arrange
        var quest = new Quest();

        // Act
        quest.Prerequisites.Add("quest_001");
        quest.Prerequisites.Add("quest_002");

        // Assert
        quest.Prerequisites.Should().HaveCount(2);
        quest.Prerequisites.Should().Contain(new[] { "quest_001", "quest_002" });
    }

    [Fact]
    public void Quest_Should_Track_Progress()
    {
        // Arrange
        var quest = new Quest
        {
            TargetName = "Goblin",
            Quantity = 10,
            Progress = 0
        };

        // Act
        quest.Progress = 7;

        // Assert
        quest.Progress.Should().Be(7);
        quest.Progress.Should().BeLessThan(quest.Quantity);
    }

    #endregion

    #region Quest Status Tests

    [Fact]
    public void Quest_Status_Should_Track_IsActive()
    {
        // Arrange
        var quest = new Quest();

        // Act
        quest.IsActive = true;

        // Assert
        quest.IsActive.Should().BeTrue();
        quest.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void Quest_Status_Should_Track_IsCompleted()
    {
        // Arrange
        var quest = new Quest { IsActive = true };

        // Act
        quest.IsCompleted = true;

        // Assert
        quest.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void Quest_Can_Be_Neither_Active_Nor_Completed()
    {
        // Arrange
        var quest = new Quest();

        // Assert
        quest.IsActive.Should().BeFalse();
        quest.IsCompleted.Should().BeFalse();
    }

    #endregion

    #region Quest Rewards Tests

    [Fact]
    public void Quest_Should_Have_Gold_And_XP_Rewards()
    {
        // Arrange
        var quest = new Quest
        {
            GoldReward = 100,
            XpReward = 250
        };

        // Assert
        quest.GoldReward.Should().Be(100);
        quest.XpReward.Should().Be(250);
    }

    [Fact]
    public void Quest_Should_Support_Item_Rewards()
    {
        // Arrange
        var quest = new Quest();

        // Act
        quest.ItemRewards.Add("Legendary Sword");
        quest.ItemRewards.Add("Health Potion");

        // Assert
        quest.ItemRewards.Should().HaveCount(2);
        quest.ItemRewards.Should().Contain("Legendary Sword");
    }

    [Fact]
    public void Quest_Should_Support_Apocalypse_Bonus()
    {
        // Arrange
        var quest = new Quest { ApocalypseBonusMinutes = 30 };

        // Assert
        quest.ApocalypseBonusMinutes.Should().Be(30);
    }

    [Fact]
    public void Quest_Can_Have_No_Item_Rewards()
    {
        // Arrange
        var quest = new Quest();

        // Assert
        quest.ItemRewards.Should().BeEmpty();
    }

    #endregion

    #region Time Limit Tests

    [Fact]
    public void IsExpired_Should_Return_False_When_No_Time_Limit()
    {
        // Arrange
        var quest = new Quest
        {
            TimeLimit = 0,
            StartTime = DateTime.Now
        };

        // Act
        var isExpired = quest.IsExpired();

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_Should_Return_False_When_No_Start_Time()
    {
        // Arrange
        var quest = new Quest
        {
            TimeLimit = 24,
            StartTime = null
        };

        // Act
        var isExpired = quest.IsExpired();

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_Should_Return_False_When_Within_Time_Limit()
    {
        // Arrange
        var quest = new Quest
        {
            TimeLimit = 24,
            StartTime = DateTime.Now.AddHours(-12)
        };

        // Act
        var isExpired = quest.IsExpired();

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_Should_Return_True_When_Time_Limit_Exceeded()
    {
        // Arrange
        var quest = new Quest
        {
            TimeLimit = 24,
            StartTime = DateTime.Now.AddHours(-25)
        };

        // Act
        var isExpired = quest.IsExpired();

        // Assert
        isExpired.Should().BeTrue();
    }

    [Fact]
    public void GetTimeRemaining_Should_Return_Message_When_No_Time_Limit()
    {
        // Arrange
        var quest = new Quest { TimeLimit = 0 };

        // Act
        var timeRemaining = quest.GetTimeRemaining();

        // Assert
        timeRemaining.Should().Be("No time limit");
    }

    [Fact]
    public void GetTimeRemaining_Should_Return_Message_When_No_Start_Time()
    {
        // Arrange
        var quest = new Quest { TimeLimit = 24, StartTime = null };

        // Act
        var timeRemaining = quest.GetTimeRemaining();

        // Assert
        timeRemaining.Should().Be("No time limit");
    }

    [Fact]
    public void GetTimeRemaining_Should_Return_Expired_When_Time_Exceeded()
    {
        // Arrange
        var quest = new Quest
        {
            TimeLimit = 1,
            StartTime = DateTime.Now.AddHours(-2)
        };

        // Act
        var timeRemaining = quest.GetTimeRemaining();

        // Assert
        timeRemaining.Should().Be("Expired");
    }

    [Fact]
    public void GetTimeRemaining_Should_Show_Minutes_When_Less_Than_Hour()
    {
        // Arrange
        var quest = new Quest
        {
            TimeLimit = 1,
            StartTime = DateTime.Now.AddMinutes(-30)
        };

        // Act
        var timeRemaining = quest.GetTimeRemaining();

        // Assert
        timeRemaining.Should().Contain("minutes");
    }

    [Fact]
    public void GetTimeRemaining_Should_Show_Days_And_Hours_When_More_Than_Day()
    {
        // Arrange
        var quest = new Quest
        {
            TimeLimit = 72, // 3 days
            StartTime = DateTime.Now.AddHours(-10)
        };

        // Act
        var timeRemaining = quest.GetTimeRemaining();

        // Assert
        timeRemaining.Should().Contain("days");
        timeRemaining.Should().Contain("hours");
    }

    #endregion

    #region Quest Giver Tests

    [Fact]
    public void Quest_Should_Track_Quest_Giver()
    {
        // Arrange
        var quest = new Quest
        {
            QuestGiverId = "npc_001",
            QuestGiverName = "Wise Elder"
        };

        // Assert
        quest.QuestGiverId.Should().Be("npc_001");
        quest.QuestGiverName.Should().Be("Wise Elder");
    }

    #endregion

    #region Target Tests

    [Fact]
    public void Quest_Should_Track_Target_Information()
    {
        // Arrange
        var quest = new Quest
        {
            TargetType = "dragon",
            TargetName = "Ancient Red Dragon",
            Quantity = 1,
            Location = "Dragon's Lair"
        };

        // Assert
        quest.TargetType.Should().Be("dragon");
        quest.TargetName.Should().Be("Ancient Red Dragon");
        quest.Quantity.Should().Be(1);
        quest.Location.Should().Be("Dragon's Lair");
    }

    [Fact]
    public void Quest_Should_Support_Multiple_Quantity_Targets()
    {
        // Arrange
        var quest = new Quest
        {
            TargetType = "goblin",
            TargetName = "Goblin Scout",
            Quantity = 25
        };

        // Assert
        quest.Quantity.Should().Be(25);
    }

    #endregion

    #region Traits Tests

    [Fact]
    public void Quest_Should_Implement_ITraitable()
    {
        // Arrange
        var quest = new Quest();

        // Assert
        quest.Should().BeAssignableTo<ITraitable>();
        quest.Traits.Should().NotBeNull();
    }

    [Fact]
    public void Quest_Traits_Should_Be_Modifiable()
    {
        // Arrange
        var quest = new Quest();

        // Act
        quest.Traits["questBonus"] = new TraitValue(1.5, TraitType.Number);
        quest.Traits["category"] = new TraitValue("epic", TraitType.String);

        // Assert
        quest.Traits.Should().HaveCount(2);
        quest.Traits["questBonus"].AsDouble().Should().BeApproximately(1.5, 0.001);
        quest.Traits["category"].AsString().Should().Be("epic");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Quest_Should_Support_Complete_Quest_Workflow()
    {
        // Arrange
        var quest = new Quest
        {
            Title = "Hunt the Beast",
            Description = "Slay 10 wolves",
            QuestType = "kill",
            Difficulty = "easy",
            Type = "side",
            TargetType = "beast",
            TargetName = "Wolf",
            Quantity = 10,
            GoldReward = 50,
            XpReward = 100,
            TimeLimit = 24,
            IsActive = false,
            IsCompleted = false,
            Progress = 0
        };

        // Act - Start quest
        quest.IsActive = true;
        quest.StartTime = DateTime.Now;

        // Act - Make progress
        quest.Progress = 5;

        // Assert - Mid-quest
        quest.IsActive.Should().BeTrue();
        quest.IsCompleted.Should().BeFalse();
        quest.Progress.Should().Be(5);
        quest.IsExpired().Should().BeFalse();

        // Act - Complete quest
        quest.Progress = 10;
        quest.IsCompleted = true;

        // Assert - Completed
        quest.Progress.Should().Be(quest.Quantity);
        quest.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void Quest_Should_Handle_Complex_Objectives()
    {
        // Arrange
        var quest = new Quest
        {
            Title = "The Grand Adventure",
            Type = "main"
        };

        // Act
        quest.Objectives["DefeatBoss"] = 1;
        quest.Objectives["CollectItems"] = 5;
        quest.Objectives["TalkToNPC"] = 3;

        quest.ObjectiveProgress["DefeatBoss"] = 0;
        quest.ObjectiveProgress["CollectItems"] = 3;
        quest.ObjectiveProgress["TalkToNPC"] = 3;

        // Assert
        quest.Objectives.Should().HaveCount(3);
        quest.ObjectiveProgress["CollectItems"].Should().Be(3);
        quest.ObjectiveProgress["TalkToNPC"].Should().Be(quest.Objectives["TalkToNPC"]);
    }

    #endregion
}
