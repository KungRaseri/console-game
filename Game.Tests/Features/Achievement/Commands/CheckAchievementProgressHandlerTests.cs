using FluentAssertions;
using Game.Features.Achievement.Commands;
using Game.Features.Achievement.Services;
using Game.Features.SaveLoad;
using Moq;

namespace Game.Tests.Features.Achievement.Commands;

public class CheckAchievementProgressHandlerTests
{
    private readonly Mock<AchievementService> _mockAchievementService;
    private readonly CheckAchievementProgressHandler _handler;

    public CheckAchievementProgressHandlerTests()
    {
        _mockAchievementService = new Mock<AchievementService>(
            Mock.Of<SaveGameService>(),
            Mock.Of<Shared.UI.IConsoleUI>());
        
        _handler = new CheckAchievementProgressHandler(_mockAchievementService.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Achievements_From_Service()
    {
        // Arrange
        var expectedAchievements = new List<Game.Models.Achievement>
        {
            new() { Id = "ACH_001", Title = "Test Achievement 1", IsUnlocked = true },
            new() { Id = "ACH_002", Title = "Test Achievement 2", IsUnlocked = false }
        };

        _mockAchievementService
            .Setup(s => s.CheckAllAchievementsAsync())
            .ReturnsAsync(expectedAchievements);

        var command = new CheckAchievementProgressCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedAchievements);
        _mockAchievementService.Verify(s => s.CheckAllAchievementsAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Achievements()
    {
        // Arrange
        _mockAchievementService
            .Setup(s => s.CheckAllAchievementsAsync())
            .ReturnsAsync(new List<Game.Models.Achievement>());

        var command = new CheckAchievementProgressCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Call_Service_With_CancellationToken()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        _mockAchievementService
            .Setup(s => s.CheckAllAchievementsAsync())
            .ReturnsAsync(new List<Game.Models.Achievement>());

        var command = new CheckAchievementProgressCommand();

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _mockAchievementService.Verify(s => s.CheckAllAchievementsAsync(), Times.Once);
    }
}
