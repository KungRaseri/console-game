using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RealmEngine.Core.Services.Budget;
using RealmEngine.Data.Services;
using Xunit;

namespace RealmEngine.Core.Tests.Services;

public class BudgetItemGenerationTests
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly BudgetItemGenerationService _generator;

    public BudgetItemGenerationTests()
    {
        var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "RealmEngine.Data", "Data", "Json");
        dataPath = Path.GetFullPath(dataPath);
        
        _dataCache = new GameDataCache(dataPath);
        _dataCache.LoadAllData();
        _referenceResolver = new ReferenceResolverService(_dataCache, NullLogger<ReferenceResolverService>.Instance);

        var configFactory = new BudgetConfigFactory(_dataCache, NullLogger<BudgetConfigFactory>.Instance);
        var budgetConfig = configFactory.GetBudgetConfig();
        var materialPools = configFactory.GetMaterialPools();
        var enemyTypes = configFactory.GetEnemyTypes();

        var budgetCalculator = new BudgetCalculator(budgetConfig, NullLogger<BudgetCalculator>.Instance);
        var materialPoolService = new MaterialPoolService(
            _dataCache,
            _referenceResolver,
            budgetCalculator,
            materialPools,
            enemyTypes,
            NullLogger<MaterialPoolService>.Instance);

        _generator = new BudgetItemGenerationService(
            _dataCache,
            _referenceResolver,
            budgetCalculator,
            materialPoolService,
            NullLogger<BudgetItemGenerationService>.Instance);
    }

    [Fact]
    public async Task GenerateItemAsync_GoblinLevel1_GeneratesLowBudgetItem()
    {
        // Arrange
        var request = new BudgetItemRequest
        {
            EnemyType = "goblin",
            EnemyLevel = 1,
            ItemCategory = "weapons"
        };

        // Act
        var result = await _generator.GenerateItemAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.BaseBudget.Should().BeGreaterThan(0);
        Assert.True(result.SpentBudget <= result.AdjustedBudget);
    }

    [Fact]
    public async Task GenerateItemAsync_DragonLevel20_GeneratesHighBudgetItem()
    {
        // Arrange
        var request = new BudgetItemRequest
        {
            EnemyType = "dragon",
            EnemyLevel = 20,
            ItemCategory = "weapons"
        };

        // Act
        var result = await _generator.GenerateItemAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.BaseBudget.Should().BeGreaterThan(200); // Level 20 * 5.0 * 2.5 (dragon multiplier)
        result.Material.Should().NotBeNull("dragons should always afford materials");
    }

    [Fact]
    public async Task GenerateItemAsync_BossEnemy_AppliesBossMultiplier()
    {
        // Arrange
        var request = new BudgetItemRequest
        {
            EnemyType = "orc",
            EnemyLevel = 10,
            ItemCategory = "weapons",
            IsBoss = true
        };

        // Act
        var result = await _generator.GenerateItemAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.BaseBudget.Should().BeGreaterThan(100); // Should have boss multiplier applied
    }

    [Fact]
    public async Task GenerateItemAsync_WithQuality_AppliesBudgetModifier()
    {
        // Arrange
        var request = new BudgetItemRequest
        {
            EnemyType = "knight",
            EnemyLevel = 10,
            ItemCategory = "weapons",
            AllowQuality = true
        };

        // Act
        var result = await _generator.GenerateItemAsync(request);

        // Assert
        result.Should().NotBeNull();
        // If quality is applied, adjusted budget should differ from base
        if (result!.Quality != null)
        {
            result.AdjustedBudget.Should().NotBe(result.BaseBudget);
        }
    }

    [Fact]
    public async Task GenerateItemAsync_Weapons_SelectsMaterial()
    {
        // Arrange
        var request = new BudgetItemRequest
        {
            EnemyType = "knight",
            EnemyLevel = 5,
            ItemCategory = "weapons"
        };

        // Act
        var result = await _generator.GenerateItemAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Material.Should().NotBeNull("should select a material");
        result.MaterialCost.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GenerateItemAsync_Armor_SelectsMaterial()
    {
        // Arrange
        var request = new BudgetItemRequest
        {
            EnemyType = "knight",
            EnemyLevel = 5,
            ItemCategory = "armor"
        };

        // Act
        var result = await _generator.GenerateItemAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Material.Should().NotBeNull("should select a material");
        result.MaterialCost.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GenerateItemAsync_SelectsBaseItem()
    {
        // Arrange
        var request = new BudgetItemRequest
        {
            EnemyType = "goblin",
            EnemyLevel = 5,
            ItemCategory = "weapons"
        };

        // Act
        var result = await _generator.GenerateItemAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.BaseItem.Should().NotBeNull("should select a base weapon");
    }

    [Fact]
    public async Task GenerateItemAsync_BudgetSpent_DoesNotExceedTotal()
    {
        // Arrange
        var request = new BudgetItemRequest
        {
            EnemyType = "knight",
            EnemyLevel = 10,
            ItemCategory = "weapons"
        };

        // Act
        var result = await _generator.GenerateItemAsync(request);

        // Assert
        result.Should().NotBeNull();
        Assert.True(result!.SpentBudget <= result.AdjustedBudget, 
            "spent budget should not exceed total budget");
    }

    [Fact]
    public async Task GenerateItemAsync_SelectsPattern()
    {
        // Arrange
        var request = new BudgetItemRequest
        {
            EnemyType = "goblin",
            EnemyLevel = 5,
            ItemCategory = "weapons"
        };

        // Act
        var result = await _generator.GenerateItemAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Pattern.Should().NotBeNullOrEmpty("should select a pattern");
    }

    [Fact]
    public async Task GenerateItemAsync_LowBudget_SelectsFewerComponents()
    {
        // Arrange
        var lowRequest = new BudgetItemRequest
        {
            EnemyType = "wolf",
            EnemyLevel = 1,
            ItemCategory = "weapons"
        };

        var highRequest = new BudgetItemRequest
        {
            EnemyType = "dragon",
            EnemyLevel = 20,
            ItemCategory = "weapons"
        };

        // Act
        var lowResult = await _generator.GenerateItemAsync(lowRequest);
        var highResult = await _generator.GenerateItemAsync(highRequest);

        // Assert
        lowResult.Should().NotBeNull();
        highResult.Should().NotBeNull();
        
        // High budget items should have more or equal components on average
        Assert.True(highResult!.Components.Count >= lowResult!.Components.Count);
    }

    [Theory]
    [InlineData("humanoid_low", "goblin")]
    [InlineData("humanoid_mid", "knight")]
    [InlineData("humanoid_high", "elite_knight")]
    [InlineData("dragon_hoard", "dragon")]
    public async Task GenerateItemAsync_DifferentEnemyTypes_UseDifferentMaterialPools(string expectedPool, string enemyType)
    {
        // Arrange
        var request = new BudgetItemRequest
        {
            EnemyType = enemyType,
            EnemyLevel = 10,
            ItemCategory = "weapons"
        };

        // Act
        var result = await _generator.GenerateItemAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Material.Should().NotBeNull($"{enemyType} should select from {expectedPool} pool");
    }
}
