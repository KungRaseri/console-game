using FluentAssertions;
using Game.Features.Exploration.Queries;
using Game.Shared.Services;
using Moq;

namespace Game.Tests.Features.Exploration.Queries;

public class GetCurrentLocationQueryHandlerTests
{
    private readonly Mock<GameStateService> _mockGameState;
    private readonly GetCurrentLocationQueryHandler _handler;

    public GetCurrentLocationQueryHandlerTests()
    {
        _mockGameState = new Mock<GameStateService>(
            Mock.Of<Game.Models.Character>());
        
        _handler = new GetCurrentLocationQueryHandler(_mockGameState.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Success_With_Current_Location()
    {
        // Arrange
        var expectedLocation = "Dragon's Lair";
        _mockGameState.SetupGet(s => s.CurrentLocation).Returns(expectedLocation);

        var query = new GetCurrentLocationQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.CurrentLocation.Should().Be(expectedLocation);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Return_Hub_Town_As_Starting_Location()
    {
        // Arrange
        _mockGameState.SetupGet(s => s.CurrentLocation).Returns("Hub Town");

        var query = new GetCurrentLocationQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.CurrentLocation.Should().Be("Hub Town");
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_String_When_No_Location_Set()
    {
        // Arrange
        _mockGameState.SetupGet(s => s.CurrentLocation).Returns(string.Empty);

        var query = new GetCurrentLocationQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.CurrentLocation.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_GameState_Throws_Exception()
    {
        // Arrange
        var exceptionMessage = "Database connection failed";
        _mockGameState.SetupGet(s => s.CurrentLocation)
            .Throws(new InvalidOperationException(exceptionMessage));

        var query = new GetCurrentLocationQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.CurrentLocation.Should().BeNull();
        result.ErrorMessage.Should().Contain(exceptionMessage);
    }

    [Fact]
    public async Task Handle_Should_Access_GameState_CurrentLocation_Property()
    {
        // Arrange
        _mockGameState.SetupGet(s => s.CurrentLocation).Returns("Ancient Ruins");

        var query = new GetCurrentLocationQuery();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockGameState.VerifyGet(s => s.CurrentLocation, Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Different_Locations_On_Multiple_Calls()
    {
        // Arrange
        var query = new GetCurrentLocationQuery();
        
        _mockGameState.SetupGet(s => s.CurrentLocation).Returns("Dark Forest");
        var result1 = await _handler.Handle(query, CancellationToken.None);

        _mockGameState.SetupGet(s => s.CurrentLocation).Returns("Mountain Peak");
        var result2 = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result1.CurrentLocation.Should().Be("Dark Forest");
        result2.CurrentLocation.Should().Be("Mountain Peak");
    }

    [Theory]
    [InlineData("Hub Town")]
    [InlineData("Dark Forest")]
    [InlineData("Ancient Ruins")]
    [InlineData("Dragon's Lair")]
    [InlineData("Underground Caverns")]
    public async Task Handle_Should_Return_Any_Valid_Location_Name(string locationName)
    {
        // Arrange
        _mockGameState.SetupGet(s => s.CurrentLocation).Returns(locationName);

        var query = new GetCurrentLocationQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.CurrentLocation.Should().Be(locationName);
    }
}
