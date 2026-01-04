using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RealmEngine.Core.Services.Budget;
using RealmEngine.Data.Services;
using Xunit;

namespace RealmEngine.Core.Tests.Services;

public class BudgetConfigFactoryTests
{
    private readonly GameDataCache _dataCache;
    private readonly BudgetConfigFactory _configFactory;

    public BudgetConfigFactoryTests()
    {
        // Use absolute path or path relative to test project
        var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "RealmEngine.Data", "Data", "Json");
        dataPath = Path.GetFullPath(dataPath); // Normalize path
        
        _dataCache = new GameDataCache(dataPath);
        _dataCache.LoadAllData();
        _configFactory = new BudgetConfigFactory(_dataCache, NullLogger<BudgetConfigFactory>.Instance);
    }

    [Fact]
    public void GetBudgetConfig_ShouldLoadConfiguration()
    {
        // Act
        var config = _configFactory.GetBudgetConfig();

        // Assert
        config.Should().NotBeNull();
        config.Allocation.Should().NotBeNull();
        config.Formulas.Should().NotBeNull();
    }

    [Fact]
    public void GetMaterialPools_ShouldLoadConfiguration()
    {
        // Act
        var pools = _configFactory.GetMaterialPools();

        // Assert
        pools.Should().NotBeNull();
        pools.Pools.Should().NotBeEmpty();
    }

    [Fact]
    public void GetEnemyTypes_ShouldLoadConfiguration()
    {
        // Act
        var enemyTypes = _configFactory.GetEnemyTypes();

        // Assert
        enemyTypes.Should().NotBeNull();
        enemyTypes.Types.Should().NotBeEmpty();
        enemyTypes.Types.Should().ContainKey("goblin");
    }

    [Fact]
    public void DataCache_ShouldHaveConfigFiles()
    {
        // Act - test different path formats
        var budgetConfig1 = _dataCache.FileExists("general/budget-config.json");
        var budgetConfig2 = _dataCache.FileExists("general\\budget-config.json");
        
        var materialPools1 = _dataCache.FileExists("general/material-pools.json");
        var materialPools2 = _dataCache.FileExists("general\\material-pools.json");
        
        var enemyTypes1 = _dataCache.FileExists("enemies/enemy-types.json");
        var enemyTypes2 = _dataCache.FileExists("enemies\\enemy-types.json");

        // Print what files are actually loaded
        var totalFiles = _dataCache.TotalFilesLoaded;

        // Assert - at least one path format should work
        (budgetConfig1 || budgetConfig2).Should().BeTrue($"budget-config.json should exist (tried forward and backslash). Total files loaded: {totalFiles}");
        (materialPools1 || materialPools2).Should().BeTrue($"material-pools.json should exist. Total files loaded: {totalFiles}");
        (enemyTypes1 || enemyTypes2).Should().BeTrue($"enemy-types.json should exist. Total files loaded: {totalFiles}");
    }
}
