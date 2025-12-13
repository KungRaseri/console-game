using FluentAssertions;
using Game.Features.Exploration;
using Game.Features.Exploration.Queries;
using MediatR;
using Moq;

namespace Game.Tests.Features.Exploration.Queries;

public class GetKnownLocationsQueryHandlerTests
{
    private readonly Mock<ExplorationService> _mockExplorationService;
    private readonly GetKnownLocationsQueryHandler _handler;

    public GetKnownLocationsQueryHandlerTests()
    {
        _mockExplorationService = new Mock<ExplorationService>(
            Mock.Of<IMediator>(),
            Mock.Of<Shared.Services.GameStateService>(),
            Mock.Of<Game.Features.SaveLoad.SaveGameService>(),
            Mock.Of<Shared.UI.IConsoleUI>());
        
        _handler = new GetKnownLocationsQueryHandler(_mockExplorationService.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Success_With_Known_Locations()
    {
        // Arrange
        var expectedLocations = new List<string>
        {
            "Hub Town",
            "Dark Forest",
            "Ancient Ruins"
        };
        
        _mockExplorationService
            .Setup(s => s.GetKnownLocations())
            .Returns(expectedLocations);

        var query = new GetKnownLocationsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Locations.Should().NotBeNull();
        result.Locations.Should().HaveCount(3);
        result.Locations.Should().ContainInOrder(expectedLocations);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Locations_Known()
    {
        // Arrange
        _mockExplorationService
            .Setup(s => s.GetKnownLocations())
            .Returns(new List<string>());

        var query = new GetKnownLocationsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Locations.Should().NotBeNull();
        result.Locations.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_Only_Hub_Town_For_New_Game()
    {
        // Arrange
        _mockExplorationService
            .Setup(s => s.GetKnownLocations())
            .Returns(new List<string> { "Hub Town" });

        var query = new GetKnownLocationsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Locations.Should().HaveCount(1);
        result.Locations.Should().Contain("Hub Town");
    }

    [Fact]
    public async Task Handle_Should_Return_All_Locations_For_Completed_Game()
    {
        // Arrange
        var allLocations = new List<string>
        {
            "Hub Town",
            "Dark Forest",
            "Ancient Ruins",
            "Dragon's Lair",
            "Underground Caverns",
            "Mountain Peak",
            "Coastal Village",
            "Cursed Graveyard"
        };
        
        _mockExplorationService
            .Setup(s => s.GetKnownLocations())
            .Returns(allLocations);

        var query = new GetKnownLocationsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Locations.Should().HaveCount(8);
        result.Locations.Should().ContainInOrder(allLocations);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Service_Throws_Exception()
    {
        // Arrange
        var exceptionMessage = "Failed to load locations";
        _mockExplorationService
            .Setup(s => s.GetKnownLocations())
            .Throws(new InvalidOperationException(exceptionMessage));

        var query = new GetKnownLocationsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Locations.Should().BeNull();
        result.ErrorMessage.Should().Contain(exceptionMessage);
    }

    [Fact]
    public async Task Handle_Should_Call_ExplorationService_GetKnownLocations()
    {
        // Arrange
        _mockExplorationService
            .Setup(s => s.GetKnownLocations())
            .Returns(new List<string>());

        var query = new GetKnownLocationsQuery();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockExplorationService.Verify(s => s.GetKnownLocations(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Preserve_Location_Order_From_Service()
    {
        // Arrange
        var orderedLocations = new List<string>
        {
            "Coastal Village",
            "Ancient Ruins",
            "Hub Town",
            "Dark Forest"
        };
        
        _mockExplorationService
            .Setup(s => s.GetKnownLocations())
            .Returns(orderedLocations);

        var query = new GetKnownLocationsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Locations.Should().ContainInOrder(orderedLocations);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(8)]
    public async Task Handle_Should_Return_Varying_Number_Of_Locations(int locationCount)
    {
        // Arrange
        var locations = Enumerable.Range(1, locationCount)
            .Select(i => $"Location {i}")
            .ToList();
        
        _mockExplorationService
            .Setup(s => s.GetKnownLocations())
            .Returns(locations);

        var query = new GetKnownLocationsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Locations.Should().HaveCount(locationCount);
    }

    [Fact]
    public async Task Handle_Should_Return_Locations_Without_Duplicates()
    {
        // Arrange
        var uniqueLocations = new List<string>
        {
            "Hub Town",
            "Dark Forest",
            "Ancient Ruins",
            "Dragon's Lair"
        };
        
        _mockExplorationService
            .Setup(s => s.GetKnownLocations())
            .Returns(uniqueLocations);

        var query = new GetKnownLocationsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Locations.Should().OnlyHaveUniqueItems();
    }
}
