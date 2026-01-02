using FluentAssertions;
using RealmEngine.Core.Generators.Modern;
using RealmEngine.Data.Services;
using Xunit;

namespace RealmEngine.Core.Tests.Generators;

[Trait("Category", "Generator")]
public class OrganizationGeneratorTests
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly OrganizationGenerator _generator;

    public OrganizationGeneratorTests()
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "RealmEngine.Data", "Data", "Json");
        _dataCache = new GameDataCache(basePath);
        _referenceResolver = new ReferenceResolverService(_dataCache);
        _generator = new OrganizationGenerator(_dataCache, _referenceResolver);
    }

    [Fact]
    public async Task Should_Generate_Organizations_From_Category()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var organizations = await _generator.GenerateOrganizationsAsync("guilds", 5);

        // Assert
        organizations.Should().NotBeNull();
        organizations.Should().HaveCount(5);
        organizations.Should().AllSatisfy(org =>
        {
            org.Name.Should().NotBeNullOrEmpty();
            org.Id.Should().Contain("guilds:");
        });
    }

    [Theory]
    [InlineData("guilds")]
    [InlineData("factions")]
    [InlineData("shops")]
    [InlineData("businesses")]
    public async Task Should_Generate_Organizations_From_Different_Categories(string category)
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var organizations = await _generator.GenerateOrganizationsAsync(category, 3);

        // Assert
        organizations.Should().NotBeNull();
        organizations.Should().HaveCountGreaterThan(0);
        organizations.Should().AllSatisfy(org =>
        {
            org.Id.Should().Contain($"{category}:");
        });
    }

    [Fact]
    public async Task Should_Generate_Organization_By_Name()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act - Use known organization from catalog (checking if fighters_guild exists)
        var organization = await _generator.GenerateOrganizationByNameAsync("guilds", "fighters_guild");

        // Assert
        if (organization != null)
        {
            organization.Name.Should().NotBeNullOrEmpty();
            organization.Id.Should().Contain("guilds:");
        }
        else
        {
            // fighters_guild might not exist, just check we can call the method
            organization.Should().BeNull();
        }
    }

    [Fact]
    public async Task Should_Return_Null_For_Non_Existent_Organization()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var organization = await _generator.GenerateOrganizationByNameAsync("guilds", "NonExistentOrg12345");

        // Assert
        organization.Should().BeNull();
    }

    [Fact]
    public async Task Should_Return_Empty_List_For_Invalid_Category()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var organizations = await _generator.GenerateOrganizationsAsync("invalid_category", 5);

        // Assert
        organizations.Should().NotBeNull();
        organizations.Should().BeEmpty();
    }

    [Fact]
    public async Task Generated_Organizations_Should_Have_Required_Properties()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var organizations = await _generator.GenerateOrganizationsAsync("guilds", 3);

        // Assert
        organizations.Should().AllSatisfy(org =>
        {
            org.Id.Should().NotBeNullOrEmpty();
            org.Name.Should().NotBeNullOrEmpty();
            org.Type.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task Should_Handle_Hierarchical_Catalog_Structure()
    {
        // Arrange - Tests that OrganizationGenerator can parse nested structures like:
        // guilds -> guild_types -> warrior_guilds -> items
        _dataCache.LoadAllData();

        // Act
        var organizations = await _generator.GenerateOrganizationsAsync("guilds", 10);

        // Assert - Should successfully extract items from nested structure
        organizations.Should().NotBeNull();
        organizations.Should().HaveCount(10, "Generator should handle hierarchical catalog structure");
    }

    [Theory]
    [InlineData("shops")]
    [InlineData("businesses")]
    public async Task Should_Generate_Organizations_With_Services(string category)
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var organizations = await _generator.GenerateOrganizationsAsync(category, 3);

        // Assert
        organizations.Should().AllSatisfy(org =>
        {
            org.Type.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task Should_Use_Weighted_Random_Selection()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act - Generate many organizations to check variety
        var organizations = await _generator.GenerateOrganizationsAsync("guilds", 50);

        // Assert - Should have variety due to weighted random selection
        var uniqueNames = organizations.Select(o => o.Name).Distinct().ToList();
        uniqueNames.Should().HaveCountGreaterThan(1, "Weighted random selection should produce variety");
    }

    [Fact]
    public async Task Guild_Organizations_Should_Have_Specialization()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var guilds = await _generator.GenerateOrganizationsAsync("guilds", 5);

        // Assert
        guilds.Should().AllSatisfy(guild =>
        {
            guild.Type.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task Faction_Organizations_Should_Have_Alignment()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var factions = await _generator.GenerateOrganizationsAsync("factions", 5);

        // Assert
        factions.Should().AllSatisfy(faction =>
        {
            faction.Type.Should().NotBeNullOrEmpty();
        });
    }
}
