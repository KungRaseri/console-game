using FluentAssertions;
using Game.Core.Generators.Modern;
using Game.Data.Services;
using Xunit;

namespace Game.Core.Tests.Generators;

[Trait("Category", "Generator")]
public class QuestGeneratorTests
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly QuestGenerator _generator;

    public QuestGeneratorTests()
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "Game.Data", "Data", "Json");
        _dataCache = new GameDataCache(basePath);
        _referenceResolver = new ReferenceResolverService(_dataCache);
        _generator = new QuestGenerator(_dataCache, _referenceResolver);
    }

    [Fact]
    public async Task Should_Generate_Quests_From_Type()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var quests = await _generator.GenerateQuestsAsync("kill", 3);

        // Assert
        quests.Should().NotBeNull();
        quests.Should().HaveCount(3);
        quests.Should().AllSatisfy(quest =>
        {
            quest.Title.Should().NotBeNullOrEmpty();
            quest.Id.Should().Contain("kill:");
        });
    }

    [Theory]
    [InlineData("kill")]
    [InlineData("fetch")]
    [InlineData("escort")]
    [InlineData("investigate")]
    [InlineData("delivery")]
    public async Task Should_Generate_Quests_From_Different_Types(string questType)
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var quests = await _generator.GenerateQuestsAsync(questType, 2);

        // Assert
        quests.Should().NotBeNull();
        quests.Should().HaveCountGreaterThan(0);
        quests.Should().AllSatisfy(quest =>
        {
            quest.Id.Should().Contain($"{questType}:");
            quest.QuestType.Should().Be(questType);
        });
    }

    [Fact]
    public async Task Should_Generate_Quest_By_Name()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var quest = await _generator.GenerateQuestByNameAsync("kill", "SlayBeasts");

        // Assert
        quest.Should().NotBeNull();
        quest!.Id.Should().Contain("kill:");
        quest.Title.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Should_Return_Null_For_Non_Existent_Quest()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var quest = await _generator.GenerateQuestByNameAsync("kill", "NonExistentQuest");

        // Assert
        quest.Should().BeNull();
    }

    [Fact]
    public async Task Should_Generate_Quests_With_Valid_Properties()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var quests = await _generator.GenerateQuestsAsync("fetch", 10);

        // Assert
        quests.Should().AllSatisfy(quest =>
        {
            quest.Title.Should().NotBeNullOrEmpty();
            quest.Description.Should().NotBeNullOrEmpty();
            quest.GoldReward.Should().BeGreaterThan(0);
            quest.XpReward.Should().BeGreaterThan(0);
            quest.Difficulty.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task Should_Generate_Quests_With_Objectives()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var quests = await _generator.GenerateQuestsAsync("kill", 5);

        // Assert
        quests.Should().AllSatisfy(quest =>
        {
            quest.Objectives.Should().NotBeNull();
            quest.Objectives.Should().NotBeEmpty();
            quest.ObjectiveProgress.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task Should_Generate_Quests_With_Appropriate_Rewards()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var quests = await _generator.GenerateQuestsAsync("escort", 10);

        // Assert
        quests.Should().AllSatisfy(quest =>
        {
            quest.GoldReward.Should().BeInRange(1, 10000);
            quest.XpReward.Should().BeInRange(1, 10000);
        });
    }

    [Fact]
    public async Task Should_Handle_Empty_Quest_Type_Gracefully()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var quests = await _generator.GenerateQuestsAsync("nonexistenttype", 3);

        // Assert
        quests.Should().NotBeNull();
        quests.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_Generate_Quests_With_Proper_Id_Format()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var quests = await _generator.GenerateQuestsAsync("investigate", 5);

        // Assert
        quests.Should().AllSatisfy(quest =>
        {
            quest.Id.Should().MatchRegex(@"^investigate:.+$");
        });
    }

    [Fact]
    public async Task Should_Generate_Quests_Not_Active_Or_Completed_Initially()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var quests = await _generator.GenerateQuestsAsync("delivery", 10);

        // Assert
        quests.Should().AllSatisfy(quest =>
        {
            quest.IsActive.Should().BeFalse();
            quest.IsCompleted.Should().BeFalse();
        });
    }

    [Fact]
    public async Task Should_Generate_Different_Quests_From_Same_Type()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var quests = await _generator.GenerateQuestsAsync("fetch", 20);

        // Assert
        var uniqueTitles = quests.Select(q => q.Title).Distinct().ToList();
        uniqueTitles.Should().HaveCountGreaterThan(1, "should generate variety of quests");
    }

    [Fact]
    public async Task Should_Set_Quest_Type_From_Catalog()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var killQuests = await _generator.GenerateQuestsAsync("kill", 5);
        var fetchQuests = await _generator.GenerateQuestsAsync("fetch", 5);

        // Assert
        killQuests.Should().AllSatisfy(q => q.QuestType.Should().Be("kill"));
        fetchQuests.Should().AllSatisfy(q => q.QuestType.Should().Be("fetch"));
    }
}
