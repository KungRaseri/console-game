using FluentAssertions;
using Game.Features.Achievement.Commands;
using Game.Features.Achievement.Services;
using Moq;

namespace Game.Tests.Features.Achievement.Commands;

public class UnlockAchievementHandlerTests
{
    private readonly Mock<AchievementService> _mockAchievementService;
    private readonly UnlockAchievementHandler _handler;

    public UnlockAchievementHandlerTests()
    {
        _mockAchievementService = new Mock<AchievementService>(
            Mock.Of<Shared.Services.GameStateService>(),
            Mock.Of<Shared.Services.GameDataService>(),
            Mock.Of<Shared.UI.IConsoleUI>());
        
        _handler = new UnlockAchievementHandler(_mockAchievementService.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Success_When_Achievement_Unlocked()
    {
        // Arrange
        var achievementId = "ACH_001";
        var unlockedAchievement = new Game.Models.Achievement
        {
            Id = achievementId,
            Title = "Test Achievement",
            IsUnlocked = true
        };

        _mockAchievementService
            .Setup(s => s.UnlockAchievementAsync(achievementId))
            .ReturnsAsync(unlockedAchievement);

        var command = new UnlockAchievementCommand(achievementId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Achievement.Should().NotBeNull();
        result.Achievement.Should().Be(unlockedAchievement);
        result.Achievement!.Id.Should().Be(achievementId);
        _mockAchievementService.Verify(s => s.UnlockAchievementAsync(achievementId), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Achievement_Not_Found()
    {
        // Arrange
        var achievementId = "INVALID_ID";

        _mockAchievementService
            .Setup(s => s.UnlockAchievementAsync(achievementId))
            .ReturnsAsync((Game.Models.Achievement?)null);

        var command = new UnlockAchievementCommand(achievementId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Achievement.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Call_Service_With_Correct_AchievementId()
    {
        // Arrange
        var achievementId = "ACH_TEST";
        
        _mockAchievementService
            .Setup(s => s.UnlockAchievementAsync(achievementId))
            .ReturnsAsync(new Game.Models.Achievement { Id = achievementId });

        var command = new UnlockAchievementCommand(achievementId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockAchievementService.Verify(
            s => s.UnlockAchievementAsync(achievementId), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Achievement_With_Unlocked_Status()
    {
        // Arrange
        var achievementId = "ACH_002";
        var achievement = new Game.Models.Achievement
        {
            Id = achievementId,
            Title = "Unlocked Achievement",
            IsUnlocked = true,
            UnlockedAt = DateTime.UtcNow
        };

        _mockAchievementService
            .Setup(s => s.UnlockAchievementAsync(achievementId))
            .ReturnsAsync(achievement);

        var command = new UnlockAchievementCommand(achievementId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Achievement!.IsUnlocked.Should().BeTrue();
        result.Achievement.UnlockedAt.Should().NotBeNull();
    }
}
