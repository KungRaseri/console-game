using FluentAssertions;
using Game.Shared.Models;

namespace Game.Tests.Generators;

public class QuestGeneratorTests
{
    public QuestGeneratorTests()
    {
        // Initialize GameDataService for tests
        var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Game.Data", "Data", "Json");
        if (Directory.Exists(dataPath))
        {
            GameDataService.Initialize(dataPath);
        }
    }

    [Fact]
    public void Generate_Should_Return_Valid_Quest()
    {
        // Act
        var quest = QuestGenerator.Generate();

        // Assert
        quest.Should().NotBeNull();
        quest.Id.Should().NotBeEmpty();
        quest.Title.Should().NotBeNullOrEmpty();
        quest.Description.Should().NotBeNullOrEmpty();
        quest.QuestType.Should().NotBeNullOrEmpty();
        quest.Difficulty.Should().NotBeNullOrEmpty();
        quest.GoldReward.Should().BeGreaterThan(0);
        quest.XpReward.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("fetch")]
    [InlineData("kill")]
    [InlineData("escort")]
    [InlineData("delivery")]
    [InlineData("investigate")]
    public void GenerateByType_Should_Return_Quest_With_Correct_Type(string questType)
    {
        // Act
        var quest = QuestGenerator.GenerateByType(questType);

        // Assert
        quest.Should().NotBeNull();
        quest.QuestType.Should().Be(questType);
        quest.Title.Should().NotBeNullOrEmpty();
        quest.Description.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("fetch", "easy")]
    [InlineData("kill", "medium")]
    [InlineData("escort", "hard")]
    public void GenerateByTypeAndDifficulty_Should_Return_Quest_With_Correct_Type_And_Difficulty(
        string questType,
        string difficulty)
    {
        // Act
        var quest = QuestGenerator.GenerateByTypeAndDifficulty(questType, difficulty);

        // Assert
        quest.Should().NotBeNull();
        quest.QuestType.Should().Be(questType);
        quest.Difficulty.Should().Be(difficulty);
        quest.Title.Should().NotBeNullOrEmpty();
        quest.Description.Should().NotBeNullOrEmpty();
        quest.Location.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Generated_Quest_Should_Have_Valid_Location()
    {
        // Act
        var quest = QuestGenerator.Generate();

        // Assert
        quest.Location.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Generated_Quest_Should_Have_Objectives()
    {
        // Act
        var quest = QuestGenerator.Generate();

        // Assert
        quest.Objectives.Should().NotBeEmpty();
        quest.ObjectiveProgress.Should().NotBeEmpty();
        quest.Objectives.Count.Should().Be(quest.ObjectiveProgress.Count);
    }

    [Fact]
    public void Generate_Multiple_Quests_Should_Have_Variety()
    {
        // Arrange
        var quests = new List<Quest>();

        // Act
        for (int i = 0; i < 20; i++)
        {
            quests.Add(QuestGenerator.Generate());
        }

        // Assert
        quests.Select(q => q.QuestType).Distinct().Count().Should().BeGreaterThan(1);
        quests.Select(q => q.Difficulty).Distinct().Count().Should().BeGreaterThan(1);
    }
}
