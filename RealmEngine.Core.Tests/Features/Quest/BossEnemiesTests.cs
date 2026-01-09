using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RealmEngine.Core.Generators.Modern;
using RealmEngine.Data.Services;
using Xunit;

namespace RealmEngine.Core.Tests.Features.Quest;

[Trait("Category", "Quest")]
public class BossEnemiesTests
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly EnemyGenerator _enemyGenerator;

    public BossEnemiesTests()
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "RealmEngine.Data", "Data", "Json");
        _dataCache = new GameDataCache(basePath);
        var mockLogger = new Mock<ILogger<ReferenceResolverService>>();
        _referenceResolver = new ReferenceResolverService(_dataCache, mockLogger.Object);
        var enemyLogger = new Mock<ILogger<EnemyGenerator>>();
        _enemyGenerator = new EnemyGenerator(_dataCache, _referenceResolver, enemyLogger.Object);
        _dataCache.LoadAllData();
    }

    [Fact]
    public async Task Should_Generate_Shrine_Guardian_Boss()
    {
        // Act
        var boss = await _enemyGenerator.GenerateEnemyByNameAsync("humanoids", "Shrine Guardian", hydrate: true);

        // Assert
        boss.Should().NotBeNull();
        boss!.Level.Should().Be(10);
        boss.MaxHealth.Should().BeGreaterThan(200); // ~207 HP
        boss.BasePhysicalDamage.Should().BeGreaterThan(30); // ~33 attack
        boss.Abilities.Should().HaveCountGreaterThanOrEqualTo(4); // At least 4 abilities
    }

    [Fact]
    public async Task Should_Generate_Abyssal_Lord_Boss()
    {
        // Act
        var boss = await _enemyGenerator.GenerateEnemyByNameAsync("demons", "Abyssal Lord", hydrate: true);

        // Assert
        boss.Should().NotBeNull();
        boss!.Level.Should().Be(18);
        boss.MaxHealth.Should().BeGreaterThanOrEqualTo(400); // ~400 HP
        boss.BasePhysicalDamage.Should().BeGreaterThan(50); // ~59 attack
        boss.Abilities.Should().HaveCountGreaterThanOrEqualTo(5); // At least 5 abilities including ultimate
    }

    [Fact]
    public async Task Should_Generate_Dark_Lord_Final_Boss()
    {
        // Act
        var boss = await _enemyGenerator.GenerateEnemyByNameAsync("demons", "Dark Lord", hydrate: true);

        // Assert
        boss.Should().NotBeNull();
        boss!.Level.Should().Be(20);
        boss.MaxHealth.Should().BeGreaterThan(600); // ~608 HP
        boss.BasePhysicalDamage.Should().BeGreaterThan(60); // ~70 attack
        boss.Abilities.Should().HaveCountGreaterThanOrEqualTo(6); // 6 abilities including ultimate
    }

    [Theory]
    [InlineData("humanoids", "Shrine Guardian", "shrine_guardian")]
    [InlineData("demons", "Abyssal Lord", "abyssal_lord")]
    [InlineData("demons", "Dark Lord", "dark_lord")]
    public async Task Boss_Names_Should_Match_Quest_Objectives(string category, string bossName, string expectedSlug)
    {
        // Act
        var boss = await _enemyGenerator.GenerateEnemyByNameAsync(category, bossName, hydrate: false);

        // Assert
        boss.Should().NotBeNull();
        var actualSlug = bossName.ToLowerInvariant().Replace(" ", "_");
        actualSlug.Should().Be(expectedSlug, 
            because: $"quest objective 'defeat_{expectedSlug}' should match boss name conversion");
    }
}
