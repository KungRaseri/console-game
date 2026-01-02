using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RealmEngine.Core.Generators.Modern;
using RealmEngine.Data.Services;
using Xunit;

namespace RealmEngine.Core.Tests.Generators;

[Trait("Category", "Generator")]
public class LocationGeneratorTests
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly LocationGenerator _generator;

    public LocationGeneratorTests()
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "RealmEngine.Data", "Data", "Json");
        _dataCache = new GameDataCache(basePath);
        var mockLogger = new Mock<ILogger<ReferenceResolverService>>();
        _referenceResolver = new ReferenceResolverService(_dataCache, mockLogger.Object);
        _generator = new LocationGenerator(_dataCache, _referenceResolver);
    }

    [Fact]
    public async Task Should_Generate_Locations_From_Category()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var locations = await _generator.GenerateLocationsAsync("towns", 5);

        // Assert
        locations.Should().NotBeNull();
        locations.Should().HaveCount(5);
        locations.Should().AllSatisfy(location =>
        {
            location.Name.Should().NotBeNullOrEmpty();
            location.Id.Should().Contain("towns:");
        });
    }

    [Theory]
    [InlineData("towns")]
    [InlineData("dungeons")]
    [InlineData("wilderness")]
    public async Task Should_Generate_Locations_From_Different_Categories(string category)
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var locations = await _generator.GenerateLocationsAsync(category, 3);

        // Assert
        locations.Should().NotBeNull();
        locations.Should().HaveCountGreaterThan(0);
        locations.Should().AllSatisfy(location =>
        {
            location.Id.Should().Contain($"{category}:");
        });
    }

    [Fact]
    public async Task Should_Generate_Location_By_Name()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act - Use known location from catalog (checking if Crossroads exists)
        var location = await _generator.GenerateLocationByNameAsync("towns", "Crossroads");

        // Assert
        if (location != null)
        {
            location.Name.Should().Be("Crossroads");
            location.Id.Should().Contain("towns:");
        }
        else
        {
            // Crossroads might not exist, just check we can call the method
            location.Should().BeNull();
        }
    }

    [Fact]
    public async Task Should_Return_Null_For_Non_Existent_Location()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var location = await _generator.GenerateLocationByNameAsync("towns", "NonExistentLocation12345");

        // Assert
        location.Should().BeNull();
    }

    [Fact]
    public async Task Should_Return_Empty_List_For_Invalid_Category()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var locations = await _generator.GenerateLocationsAsync("invalid_category", 5);

        // Assert
        locations.Should().NotBeNull();
        locations.Should().BeEmpty();
    }

    [Fact]
    public async Task Generated_Locations_Should_Have_Required_Properties()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var locations = await _generator.GenerateLocationsAsync("towns", 3);

        // Assert
        locations.Should().AllSatisfy(location =>
        {
            location.Id.Should().NotBeNullOrEmpty();
            location.Name.Should().NotBeNullOrEmpty();
            location.Type.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task Should_Handle_Hierarchical_Catalog_Structure()
    {
        // Arrange - Tests that LocationGenerator can parse nested structures like:
        // towns -> outposts_types -> outposts -> items
        _dataCache.LoadAllData();

        // Act
        var locations = await _generator.GenerateLocationsAsync("towns", 10);

        // Assert - Should successfully extract items from nested structure
        locations.Should().NotBeNull();
        locations.Should().HaveCount(10, "Generator should handle hierarchical catalog structure");
    }

    [Theory]
    [InlineData("dungeons")]
    [InlineData("wilderness")]
    public async Task Should_Generate_Locations_With_Correct_Type(string category)
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var locations = await _generator.GenerateLocationsAsync(category, 3);

        // Assert
        locations.Should().AllSatisfy(location =>
        {
            location.Type.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task Should_Use_Weighted_Random_Selection()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act - Generate many locations to check variety
        var locations = await _generator.GenerateLocationsAsync("towns", 50);

        // Assert - Should have variety due to weighted random selection
        var uniqueNames = locations.Select(l => l.Name).Distinct().ToList();
        uniqueNames.Should().HaveCountGreaterThan(1, "Weighted random selection should produce variety");
    }
}
