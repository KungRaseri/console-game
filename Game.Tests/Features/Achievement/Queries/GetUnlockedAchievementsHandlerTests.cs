using FluentAssertions;
using Game.Features.Achievement.Queries;
using Game.Features.Achievement.Services;
using Game.Features.SaveLoad;
using Moq;

namespace Game.Tests.Features.Achievement.Queries;

public class GetUnlockedAchievementsHandlerTests
{
    private readonly Mock<AchievementService> _mockAchievementService;
    private readonly GetUnlockedAchievementsHandler _handler;

    public GetUnlockedAchievementsHandlerTests()
    {
        _mockAchievementService = new Mock<AchievementService>(
            Mock.Of<SaveGameService>(),
            Mock.Of<Shared.UI.IConsoleUI>());
        
        _handler = new GetUnlockedAchievementsHandler(_mockAchievementService.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Unlocked_Achievements()
    {
        // Arrange
        var unlockedAchievements = new List<Game.Models.Achievement>
        {
            new() { Id = "ACH_001", Title = "First Blood", IsUnlocked = true },
            new() { Id = "ACH_002", Title = "Level 10", IsUnlocked = true },
            new() { Id = "ACH_003", Title = "Rich", IsUnlocked = true }
        };

        _mockAchievementService
            .Setup(s => s.GetUnlockedAchievementsAsync())
            .ReturnsAsync(unlockedAchievements);

        var query = new GetUnlockedAchievementsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(unlockedAchievements);
        result.Should().OnlyContain(a => a.IsUnlocked);
        _mockAchievementService.Verify(s => s.GetUnlockedAchievementsAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Unlocked_Achievements()
    {
        // Arrange
        _mockAchievementService
            .Setup(s => s.GetUnlockedAchievementsAsync())
            .ReturnsAsync(new List<Game.Models.Achievement>());

        var query = new GetUnlockedAchievementsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Call_Service_Once()
    {
        // Arrange
        var achievements = new List<Game.Models.Achievement>
        {
            new() { Id = "ACH_001", IsUnlocked = true }
        };

        _mockAchievementService
            .Setup(s => s.GetUnlockedAchievementsAsync())
            .ReturnsAsync(achievements);

        var query = new GetUnlockedAchievementsQuery();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockAchievementService.Verify(s => s.GetUnlockedAchievementsAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Unlocked_With_Details()
    {
        // Arrange
        var unlockTime = DateTime.UtcNow;
        var achievements = new List<Game.Models.Achievement>
        {
            new() 
            { 
                Id = "ACH_001", 
                Title = "Achievement 1",
                Description = "Test description",
                IsUnlocked = true,
                UnlockedAt = unlockTime,
                Points = 10
            }
        };

        _mockAchievementService
            .Setup(s => s.GetUnlockedAchievementsAsync())
            .ReturnsAsync(achievements);

        var query = new GetUnlockedAchievementsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var achievement = result.First();
        achievement.Id.Should().Be("ACH_001");
        achievement.Title.Should().Be("Achievement 1");
        achievement.Description.Should().Be("Test description");
        achievement.IsUnlocked.Should().BeTrue();
        achievement.UnlockedAt.Should().Be(unlockTime);
        achievement.Points.Should().Be(10);
    }
}
