using FluentAssertions;
using Game.Core.Features.Quest.Services;
using Game.Core.Features.SaveLoad;
using Game.Core.Models;
using Moq;

namespace Game.Tests.Features.Quests.Services;

public class QuestServiceTests
{
    private readonly Mock<ISaveGameService> _mockSaveGameService;
    private readonly Mock<MainQuestService> _mockMainQuestService;
    private readonly QuestService _questService;
    private readonly SaveGame _testSaveGame;

    public QuestServiceTests()
    {
        _mockSaveGameService = new Mock<ISaveGameService>();
        _mockMainQuestService = new Mock<MainQuestService>();
        _questService = new QuestService(_mockSaveGameService.Object, _mockMainQuestService.Object);

        _testSaveGame = new SaveGame
        {
            Id = "test-save",
            PlayerName = "TestHero",
            ActiveQuests = new List<Quest>(),
            CompletedQuests = new List<Quest>(),
            QuestsCompleted = 0
        };
    }

    [Fact]
    public async Task StartQuestAsync_Should_Return_Error_When_No_Active_Save()
    {
        // Arrange
        _mockSaveGameService.Setup(s => s.GetCurrentSave()).Returns((SaveGame?)null);

        // Act
        var result = await _questService.StartQuestAsync("quest-1");

        // Assert
        result.Success.Should().BeFalse();
        result.Quest.Should().BeNull();
        result.ErrorMessage.Should().Be("No active save game");
    }

    [Fact]
    public async Task StartQuestAsync_Should_Start_Quest_Successfully()
    {
        // Arrange
        var quest = new Quest
        {
            Id = "quest-1",
            Title = "First Quest",
            Description = "Complete the first quest",
            Prerequisites = new List<string>(),
            Objectives = new Dictionary<string, int> { { "kill_enemies", 10 } }
        };

        _mockSaveGameService.Setup(s => s.GetCurrentSave()).Returns(_testSaveGame);
        _mockMainQuestService.Setup(m => m.GetQuestByIdAsync("quest-1")).ReturnsAsync(quest);

        // Act
        var result = await _questService.StartQuestAsync("quest-1");

        // Assert
        result.Success.Should().BeTrue();
        result.Quest.Should().NotBeNull();
        result.Quest!.Id.Should().Be("quest-1");
        result.Quest.IsActive.Should().BeTrue();

        _testSaveGame.ActiveQuests.Should().ContainSingle();
        _mockSaveGameService.Verify(s => s.SaveGame(_testSaveGame), Times.Once);
    }

    [Fact]
    public async Task CompleteQuestAsync_Should_Complete_Quest_Successfully()
    {
        // Arrange
        var quest = new Quest
        {
            Id = "quest-1",
            Title = "Test Quest",
            IsActive = true,
            Objectives = new Dictionary<string, int> { { "kill_enemies", 10 } },
            ObjectiveProgress = new Dictionary<string, int> { { "kill_enemies", 10 } }
        };

        _testSaveGame.ActiveQuests.Add(quest);
        _mockSaveGameService.Setup(s => s.GetCurrentSave()).Returns(_testSaveGame);

        // Act
        var result = await _questService.CompleteQuestAsync("quest-1");

        // Assert
        result.Success.Should().BeTrue();
        result.Quest.Should().NotBeNull();
        result.Quest!.IsCompleted.Should().BeTrue();

        _testSaveGame.ActiveQuests.Should().BeEmpty();
        _testSaveGame.CompletedQuests.Should().ContainSingle();
        _testSaveGame.QuestsCompleted.Should().Be(1);
    }

    [Fact]
    public async Task GetActiveQuestsAsync_Should_Return_All_Active_Quests()
    {
        // Arrange
        var quest1 = new Quest { Id = "quest-1", Title = "Quest 1", IsActive = true };
        var quest2 = new Quest { Id = "quest-2", Title = "Quest 2", IsActive = true };

        _testSaveGame.ActiveQuests.AddRange(new[] { quest1, quest2 });
        _mockSaveGameService.Setup(s => s.GetCurrentSave()).Returns(_testSaveGame);

        // Act
        var result = await _questService.GetActiveQuestsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(q => q.Id == "quest-1");
        result.Should().Contain(q => q.Id == "quest-2");
    }
}

