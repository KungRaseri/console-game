using FluentAssertions;
using Moq;
using RealmEngine.Core.Features.Achievements.Commands;
using RealmEngine.Core.Features.Achievements.Services;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Tests.Features.Achievement.Commands;

[Trait("Category", "Feature")]
/// <summary>
/// Tests for UnlockAchievementHandler - validates manual achievement unlocking.
/// </summary>
public class UnlockAchievementHandlerTests
{
    private readonly Mock<AchievementService> _mockAchievementService;
    private readonly UnlockAchievementHandler _handler;

    public UnlockAchievementHandlerTests()
    {
        _mockAchievementService = new Mock<AchievementService>();
        _handler = new UnlockAchievementHandler(_mockAchievementService.Object);
    }

    [Fact]
    public async Task Handle_Should_Unlock_Achievement_Successfully()
    {
        // Arrange
        var achievement = new RealmEngine.Shared.Models.Achievement
        {
            Id = "first-quest",
            Title = "First Quest Complete",
            Description = "Complete your first quest",
            Points = 10,
            IsUnlocked = true,
            UnlockedAt = DateTime.Now
        };

        _mockAchievementService
            .Setup(x => x.UnlockAchievementAsync("first-quest"))
            .ReturnsAsync(achievement);

        var command = new UnlockAchievementCommand("first-quest");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Achievement.Should().NotBeNull();
        result.Achievement!.Id.Should().Be("first-quest");
        result.Achievement.IsUnlocked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Achievement_Not_Found()
    {
        // Arrange
        _mockAchievementService
            .Setup(x => x.UnlockAchievementAsync("invalid-id"))
            .ReturnsAsync((RealmEngine.Shared.Models.Achievement?)null);

        var command = new UnlockAchievementCommand("invalid-id");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Achievement.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Already_Unlocked()
    {
        // Arrange
        _mockAchievementService
            .Setup(x => x.UnlockAchievementAsync("already-unlocked"))
            .ReturnsAsync((RealmEngine.Shared.Models.Achievement?)null);

        var command = new UnlockAchievementCommand("already-unlocked");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Achievement.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Call_UnlockAchievementAsync_With_Correct_Id()
    {
        // Arrange
        var achievementId = "test-achievement";
        _mockAchievementService
            .Setup(x => x.UnlockAchievementAsync(achievementId))
            .ReturnsAsync((RealmEngine.Shared.Models.Achievement?)null);

        var command = new UnlockAchievementCommand(achievementId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockAchievementService.Verify(
            x => x.UnlockAchievementAsync(achievementId),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Preserve_Achievement_Data()
    {
        // Arrange
        var unlockTime = DateTime.Now;
        var achievement = new RealmEngine.Shared.Models.Achievement
        {
            Id = "legendary-kill",
            Title = "Legendary Hunter",
            Description = "Defeat a legendary enemy",
            Points = 100,
            Category = AchievementCategory.Combat,
            IsUnlocked = true,
            UnlockedAt = unlockTime
        };

        _mockAchievementService
            .Setup(x => x.UnlockAchievementAsync("legendary-kill"))
            .ReturnsAsync(achievement);

        var command = new UnlockAchievementCommand("legendary-kill");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Achievement.Should().NotBeNull();
        result.Achievement!.Title.Should().Be("Legendary Hunter");
        result.Achievement.Description.Should().Be("Defeat a legendary enemy");
        result.Achievement.Points.Should().Be(100);
        result.Achievement.Category.Should().Be(AchievementCategory.Combat);
        result.Achievement.UnlockedAt.Should().Be(unlockTime);
    }

    [Theory]
    [InlineData("first-quest")]
    [InlineData("level-10")]
    [InlineData("gold-collector")]
    [InlineData("survivor")]
    public async Task Handle_Should_Work_With_Different_Achievement_Ids(string achievementId)
    {
        // Arrange
        var achievement = new RealmEngine.Shared.Models.Achievement
        {
            Id = achievementId,
            Title = $"Achievement {achievementId}",
            IsUnlocked = true
        };

        _mockAchievementService
            .Setup(x => x.UnlockAchievementAsync(achievementId))
            .ReturnsAsync(achievement);

        var command = new UnlockAchievementCommand(achievementId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Achievement!.Id.Should().Be(achievementId);
    }
}
