using FluentAssertions;
using Game.Data.Services;

namespace RealmEngine.Core.Tests.Basic;

[Trait("Category", "Integration")]
public class ReferenceResolverServiceTests
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;

    public ReferenceResolverServiceTests()
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "Game.Data", "Data", "Json");
        _dataCache = new GameDataCache(basePath);
        _referenceResolver = new ReferenceResolverService(_dataCache);
    }

    [Theory]
    [InlineData("@classes/cleric:Priest")]
    [InlineData("@items/weapons/swords:Longsword")]
    public void Should_Resolve_Valid_References(string reference)
    {
        // Act
        var result = _referenceResolver.Resolve(reference);

        // Assert - Should not throw and should return something (even if null due to missing data)
        // The main point is that the reference resolver works without exceptions
        result.Should().NotBeNull($"Reference {reference} should resolve to some value");
    }

    [Theory]
    [InlineData("@abilities/active/offensive:NonExistentAbility?")]
    [InlineData("@items/weapons:non-existent-item?")]
    public void Should_Handle_Optional_References(string reference)
    {
        // Act
        var result = _referenceResolver.Resolve(reference);

        // Assert - Optional references can return null without throwing
        // This is expected behavior for optional references
    }

    [Theory]
    [InlineData("invalid-reference")]
    [InlineData("@")]
    [InlineData("abilities/active/offensive:heal")]
    public void Should_Handle_Invalid_References_Gracefully(string reference)
    {
        // Act & Assert - Should not throw exceptions
        var result = _referenceResolver.Resolve(reference);
        
        // Invalid references should return null
        result.Should().BeNull($"Invalid reference {reference} should return null");
    }

    [Fact]
    public async Task Should_Support_Async_Resolution()
    {
        // Act
        var result = await _referenceResolver.ResolveAsync("@abilities/active:heal?");

        // Assert - Should complete without throwing
        // Result can be null for optional references
    }
}