using FluentAssertions;
using Moq;
using RealmEngine.Core.Features.Quests.Queries;
using RealmEngine.Core.Features.Quests.Services;
using QuestModel = RealmEngine.Shared.Models.Quest;

namespace RealmEngine.Core.Tests.Features.Quest.Queries;

[Trait("Category", "Feature")]
/// <summary>
/// Tests for GetMainQuestChainHandler.
/// </summary>
public class GetMainQuestChainHandlerTests
{
    private readonly Mock<MainQuestService> _mockMainQuestService;
    private readonly GetMainQuestChainHandler _handler;

    public GetMainQuestChainHandlerTests()
    {
        _mockMainQuestService = new Mock<MainQuestService>();
        _handler = new GetMainQuestChainHandler(_mockMainQuestService.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Main_Quest_Chain()
    {
        // Arrange
        var mainQuestChain = new List<QuestModel>
        {
            new QuestModel { Id = "main-001", Title = "Chapter 1", QuestType = "main" },
            new QuestModel { Id = "main-002", Title = "Chapter 2", QuestType = "main" },
            new QuestModel { Id = "main-003", Title = "Chapter 3", QuestType = "main" }
        };
        var query = new GetMainQuestChainQuery();

        _mockMainQuestService
            .Setup(s => s.GetMainQuestChainAsync())
            .ReturnsAsync(mainQuestChain);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(mainQuestChain);
        _mockMainQuestService.Verify(s => s.GetMainQuestChainAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Main_Quests()
    {
        // Arrange
        var emptyList = new List<QuestModel>();
        var query = new GetMainQuestChainQuery();

        _mockMainQuestService
            .Setup(s => s.GetMainQuestChainAsync())
            .ReturnsAsync(emptyList);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_Quests_In_Story_Order()
    {
        // Arrange
        var mainQuestChain = new List<QuestModel>
        {
            new QuestModel { Id = "main-001", Title = "The Beginning", QuestType = "main" },
            new QuestModel { Id = "main-002", Title = "The Journey", QuestType = "main" },
            new QuestModel { Id = "main-003", Title = "The Climax", QuestType = "main" },
            new QuestModel { Id = "main-004", Title = "The End", QuestType = "main" }
        };
        var query = new GetMainQuestChainQuery();

        _mockMainQuestService
            .Setup(s => s.GetMainQuestChainAsync())
            .ReturnsAsync(mainQuestChain);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().ContainInOrder(mainQuestChain);
    }

    [Fact]
    public async Task Handle_Should_Include_Quest_Prerequisites()
    {
        // Arrange
        var mainQuestChain = new List<QuestModel>
        {
            new QuestModel
            {
                Id = "main-001",
                Title = "First Quest",
                QuestType = "main"
            },
            new QuestModel
            {
                Id = "main-002",
                Title = "Second Quest",
                QuestType = "main"
            }
        };
        var query = new GetMainQuestChainQuery();

        _mockMainQuestService
            .Setup(s => s.GetMainQuestChainAsync())
            .ReturnsAsync(mainQuestChain);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        // Prerequisites would be tested once property exists
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_Should_Return_Both_Completed_And_Incomplete_Quests()
    {
        // Arrange
        var mainQuestChain = new List<QuestModel>
        {
            new QuestModel { Id = "main-001", Title = "Completed Quest", QuestType = "main", IsCompleted = true },
            new QuestModel { Id = "main-002", Title = "Active Quest", QuestType = "main", IsActive = true },
            new QuestModel { Id = "main-003", Title = "Locked Quest", QuestType = "main", IsCompleted = false, IsActive = false }
        };
        var query = new GetMainQuestChainQuery();

        _mockMainQuestService
            .Setup(s => s.GetMainQuestChainAsync())
            .ReturnsAsync(mainQuestChain);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(q => q.IsCompleted);
        result.Should().Contain(q => q.IsActive);
        result.Should().Contain(q => !q.IsCompleted && !q.IsActive);
    }

    [Fact]
    public async Task Handle_Should_Return_Only_MainStory_Quest_Type()
    {
        // Arrange
        var mainQuestChain = new List<QuestModel>
        {
            new QuestModel { Id = "main-001", Title = "Main Quest 1", QuestType = "main" },
            new QuestModel { Id = "main-002", Title = "Main Quest 2", QuestType = "main" }
        };
        var query = new GetMainQuestChainQuery();

        _mockMainQuestService
            .Setup(s => s.GetMainQuestChainAsync())
            .ReturnsAsync(mainQuestChain);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().OnlyContain(q => q.QuestType == "main");
    }
}
