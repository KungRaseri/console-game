using FluentAssertions;
using Game.Core.Generators.Modern;
using Game.Data.Services;
using Xunit;

namespace RealmEngine.Core.Tests.Generators;

[Trait("Category", "Generator")]
public class NpcGeneratorTests
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly NpcGenerator _generator;

    public NpcGeneratorTests()
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "Game.Data", "Data", "Json");
        _dataCache = new GameDataCache(basePath);
        _referenceResolver = new ReferenceResolverService(_dataCache);
        _generator = new NpcGenerator(_dataCache, _referenceResolver);
    }

    [Fact]
    public async Task Should_Generate_Npcs_From_Category()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var npcs = await _generator.GenerateNpcsAsync("merchants", 5);

        // Assert
        npcs.Should().NotBeNull();
        npcs.Should().HaveCount(5);
        npcs.Should().AllSatisfy(npc =>
        {
            npc.Name.Should().NotBeNullOrEmpty();
            npc.Id.Should().Contain("merchants:");
        });
    }

    [Theory]
    [InlineData("merchants")]
    [InlineData("common")]
    [InlineData("military")]
    public async Task Should_Generate_Npcs_From_Different_Categories(string category)
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var npcs = await _generator.GenerateNpcsAsync(category, 3);

        // Assert
        npcs.Should().NotBeNull();
        npcs.Should().HaveCountGreaterThan(0);
        npcs.Should().AllSatisfy(npc =>
        {
            npc.Id.Should().Contain($"{category}:");
        });
    }

    [Fact]
    public async Task Should_Generate_Npc_By_Name()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var npc = await _generator.GenerateNpcByNameAsync("merchants", "General Merchant");

        // Assert
        npc.Should().NotBeNull();
        npc!.Name.Should().Be("General Merchant");
        npc.Id.Should().Contain("merchants:");
    }

    [Fact]
    public async Task Should_Return_Null_For_Non_Existent_Npc()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var npc = await _generator.GenerateNpcByNameAsync("merchants", "NonExistentNPC");

        // Assert
        npc.Should().BeNull();
    }

    [Fact]
    public async Task Should_Generate_Npcs_With_Valid_Properties()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var npcs = await _generator.GenerateNpcsAsync("merchants", 10);

        // Assert
        npcs.Should().AllSatisfy(npc =>
        {
            npc.Name.Should().NotBeNullOrEmpty();
            npc.Age.Should().BeInRange(1, 150);
            npc.Occupation.Should().NotBeNullOrEmpty();
            npc.Gold.Should().BeGreaterThanOrEqualTo(0);
            npc.Dialogue.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task Should_Generate_Npcs_With_Reasonable_Ages()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var npcs = await _generator.GenerateNpcsAsync("common", 20);

        // Assert
        npcs.Should().AllSatisfy(npc =>
        {
            npc.Age.Should().BeGreaterThan(0);
            npc.Age.Should().BeLessThan(150);
        });
    }

    [Fact]
    public async Task Should_Handle_Empty_Category_Gracefully()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var npcs = await _generator.GenerateNpcsAsync("nonexistentcategory", 5);

        // Assert
        npcs.Should().NotBeNull();
        npcs.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_Generate_Different_Npcs_From_Same_Category()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var npcs1 = await _generator.GenerateNpcsAsync("common", 20);
        var npcs2 = await _generator.GenerateNpcsAsync("common", 20);

        // Assert
        var uniqueNames = npcs1.Select(n => n.Name).Distinct().ToList();
        uniqueNames.Should().HaveCountGreaterThan(1, "should generate variety of NPCs");
    }

    [Fact]
    public async Task Should_Set_Proper_Npc_Id_Format()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var npcs = await _generator.GenerateNpcsAsync("merchants", 5);

        // Assert
        npcs.Should().AllSatisfy(npc =>
        {
            npc.Id.Should().MatchRegex(@"^merchants:.+$");
        });
    }

    [Fact]
    public async Task Should_Generate_Npcs_With_Gold()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var npcs = await _generator.GenerateNpcsAsync("merchants", 10);

        // Assert
        npcs.Should().AllSatisfy(npc =>
        {
            npc.Gold.Should().BeGreaterThanOrEqualTo(0);
        });
    }

    [Fact]
    public async Task Should_Generate_Friendly_Npcs_By_Default()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var npcs = await _generator.GenerateNpcsAsync("merchants", 10);

        // Assert
        var friendlyCount = npcs.Count(n => n.IsFriendly);
        friendlyCount.Should().BeGreaterThan(0, "most merchants should be friendly");
    }
}
