using FluentAssertions;
using Moq;
using RealmEngine.Core.Features.Exploration;
using RealmEngine.Core.Features.Exploration.Queries;

namespace RealmEngine.Core.Tests.Features.Exploration.Queries;

[Trait("Category", "Feature")]
/// <summary>
/// Tests for GetKnownLocationsQueryHandler.
/// </summary>
public class GetKnownLocationsQueryHandlerTests
{
    private readonly Mock<ExplorationService> _mockExplorationService;
    private readonly GetKnownLocationsQueryHandler _handler;

    public GetKnownLocationsQueryHandlerTests()
    {
        _mockExplorationService = new Mock<ExplorationService>();
        _handler = new GetKnownLocationsQueryHandler(_mockExplorationService.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Known_Locations()
    {
        // Arrange
        var knownLocations = new List<string>
        {
            "Starting Village",
            "Dark Forest",
            "Ancient Ruins",
            "Mountain Peak"
        };
        var query = new GetKnownLocationsQuery();

        _mockExplorationService
            .Setup(s => s.GetKnownLocations())
            .Returns(knownLocations);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Locations.Should().NotBeNull();
        result.Locations.Should().BeEquivalentTo(knownLocations);
        result.ErrorMessage.Should().BeNull();
        _mockExplorationService.Verify(s => s.GetKnownLocations(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Locations_Known()
    {
        // Arrange
        var emptyList = new List<string>();
        var query = new GetKnownLocationsQuery();

        _mockExplorationService
            .Setup(s => s.GetKnownLocations())
            .Returns(emptyList);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Locations.Should().NotBeNull();
        result.Locations.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_Single_Location()
    {
        // Arrange
        var singleLocation = new List<string> { "Starting Village" };
        var query = new GetKnownLocationsQuery();

        _mockExplorationService
            .Setup(s => s.GetKnownLocations())
            .Returns(singleLocation);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Locations.Should().ContainSingle();
        result.Locations!.First().Should().Be("Starting Village");
    }

    [Fact]
    public async Task Handle_Should_Preserve_Location_Order()
    {
        // Arrange
        var orderedLocations = new List<string>
        {
            "First Location",
            "Second Location",
            "Third Location"
        };
        var query = new GetKnownLocationsQuery();

        _mockExplorationService
            .Setup(s => s.GetKnownLocations())
            .Returns(orderedLocations);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Locations.Should().ContainInOrder(orderedLocations);
    }

    [Fact]
    public async Task Handle_Should_Return_Large_List_Of_Locations()
    {
        // Arrange
        var manyLocations = Enumerable.Range(1, 100)
            .Select(i => $"Location {i}")
            .ToList();
        var query = new GetKnownLocationsQuery();

        _mockExplorationService
            .Setup(s => s.GetKnownLocations())
            .Returns(manyLocations);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Locations.Should().HaveCount(100);
    }

    [Fact]
    public async Task Handle_Should_Handle_Exception_Gracefully()
    {
        // Arrange
        var query = new GetKnownLocationsQuery();
        var expectedException = new InvalidOperationException("Database error");

        _mockExplorationService
            .Setup(s => s.GetKnownLocations())
            .Throws(expectedException);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Locations.Should().BeNull();
        result.ErrorMessage.Should().Be(expectedException.Message);
    }

    [Fact]
    public async Task Handle_Should_Return_Readonly_List()
    {
        // Arrange
        var locations = new List<string> { "Location 1", "Location 2" };
        var query = new GetKnownLocationsQuery();

        _mockExplorationService
            .Setup(s => s.GetKnownLocations())
            .Returns(locations);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Locations.Should().BeAssignableTo<IReadOnlyList<string>>();
    }

    [Fact]
    public async Task Handle_Should_Not_Modify_Exploration_Service_State()
    {
        // Arrange
        var locations = new List<string> { "Test Location" };
        var query = new GetKnownLocationsQuery();

        _mockExplorationService
            .Setup(s => s.GetKnownLocations())
            .Returns(locations);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert - Verify only read operation
        _mockExplorationService.Verify(s => s.GetKnownLocations(), Times.Once);
        _mockExplorationService.VerifyNoOtherCalls();
    }
}
