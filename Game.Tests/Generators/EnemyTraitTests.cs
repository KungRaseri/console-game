using FluentAssertions;
using Game.Core.Generators;
using Game.Core.Models;
using Game.Core.Services;

namespace Game.Tests.Generators;

public class EnemyTraitTests
{
    public EnemyTraitTests()
    {
        // Initialize GameDataService for tests
        var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Game.Shared", "Data", "Json");
        if (Directory.Exists(dataPath))
        {
            GameDataService.Initialize(dataPath);
        }
    }
    
    [Theory]
    [InlineData(EnemyType.Beast)]
    [InlineData(EnemyType.Undead)]
    [InlineData(EnemyType.Demon)]
    [InlineData(EnemyType.Elemental)]
    [InlineData(EnemyType.Dragon)]
    [InlineData(EnemyType.Humanoid)]
    public void Enemy_Should_Have_Traits_Applied_From_Prefix(EnemyType type)
    {
        // Arrange & Act
        var enemy = EnemyGenerator.GenerateByType(type, playerLevel: 10, EnemyDifficulty.Normal);
        
        // Assert
        enemy.Should().NotBeNull();
        enemy.Traits.Should().NotBeEmpty("enemy should have traits from prefix");
        enemy.MaxHealth.Should().BeGreaterThan(0);
        enemy.BasePhysicalDamage.Should().BeGreaterThan(0);
    }
    
    [Theory]
    [InlineData(EnemyDifficulty.Easy)]
    [InlineData(EnemyDifficulty.Normal)]
    [InlineData(EnemyDifficulty.Hard)]
    [InlineData(EnemyDifficulty.Elite)]
    [InlineData(EnemyDifficulty.Boss)]
    public void Beast_Enemy_Should_Scale_With_Difficulty(EnemyDifficulty difficulty)
    {
        // Arrange
        var playerLevel = 10;
        
        // Act
        var enemy = EnemyGenerator.GenerateByType(EnemyType.Beast, playerLevel, difficulty);
        
        // Assert
        enemy.Should().NotBeNull();
        enemy.Type.Should().Be(EnemyType.Beast);
        enemy.Difficulty.Should().Be(difficulty);
        enemy.Traits.Should().NotBeEmpty();
        
        // Boss enemies should have significantly more health
        if (difficulty == EnemyDifficulty.Boss)
        {
            enemy.MaxHealth.Should().BeGreaterThan(200, "boss should have high health");
        }
    }
    
    [Fact]
    public void Undead_Enemy_Should_Have_Undead_Specific_Traits()
    {
        // Arrange & Act
        var enemy = EnemyGenerator.GenerateByType(EnemyType.Undead, playerLevel: 10, EnemyDifficulty.Hard);
        
        // Assert
        enemy.Should().NotBeNull();
        enemy.Type.Should().Be(EnemyType.Undead);
        
        // Undead should have some undead-specific traits (check for common ones)
        var traitKeys = string.Join(",", enemy.Traits.Keys);
        var hasUndeadTraits = enemy.Traits.Keys.Any(k => 
            k.Contains("poison") || 
            k.Contains("necrotic") || 
            k.Contains("lifeSteal") ||
            k.Contains("Immune"));
            
        hasUndeadTraits.Should().BeTrue($"undead should have undead-specific traits, has: {traitKeys}");
    }
    
    [Fact]
    public void Dragon_Enemy_Should_Have_Dragon_Specific_Traits()
    {
        // Arrange & Act
        var enemy = EnemyGenerator.GenerateByType(EnemyType.Dragon, playerLevel: 15, EnemyDifficulty.Elite);
        
        // Assert
        enemy.Should().NotBeNull();
        enemy.Type.Should().Be(EnemyType.Dragon);
        
        // Dragons should have dragon-specific traits
        var traitKeys = string.Join(",", enemy.Traits.Keys);
        var hasDragonTraits = enemy.Traits.Keys.Any(k => 
            k.Contains("breath") || 
            k.Contains("flying") || 
            k.Contains("treasure") ||
            k.Contains("scale") ||
            k.Contains("legendary"));
            
        hasDragonTraits.Should().BeTrue($"dragon should have dragon-specific traits, has: {traitKeys}");
    }
    
    [Fact]
    public void Enemy_Health_And_Damage_Should_Be_Modified_By_Multipliers()
    {
        // Arrange - Generate multiple bosses and check that at least some have higher stats
        var normalEnemies = Enumerable.Range(0, 10)
            .Select(_ => EnemyGenerator.GenerateByType(EnemyType.Demon, 10, EnemyDifficulty.Normal))
            .ToList();
        var bossEnemies = Enumerable.Range(0, 10)
            .Select(_ => EnemyGenerator.GenerateByType(EnemyType.Demon, 10, EnemyDifficulty.Boss))
            .ToList();
        
        var avgNormalHealth = normalEnemies.Average(e => e.MaxHealth);
        var avgBossHealth = bossEnemies.Average(e => e.MaxHealth);
        
        // Assert - On average, bosses should have more health due to higher-tier prefixes
        avgBossHealth.Should().BeGreaterThan(avgNormalHealth, "bosses on average should have more health than normal enemies");
        
        // All enemies should have traits from prefixes
        normalEnemies.Should().AllSatisfy(e => e.Traits.Should().NotBeEmpty("every enemy should have traits from prefix"));
        bossEnemies.Should().AllSatisfy(e => e.Traits.Should().NotBeEmpty("every enemy should have traits from prefix"));
    }
}
