using FluentAssertions;
using Microsoft.Extensions.Logging;
using RealmEngine.Core.Services;
using RealmEngine.Core.Services.Budget;
using RealmEngine.Shared.Services;
using Xunit;
using Xunit.Abstractions;

namespace RealmEngine.Core.Tests.Services;

public class MaterialPoolDebugTests
{
    private readonly ITestOutputHelper _output;
    private readonly DataReferenceResolver _referenceResolver;
    private readonly BudgetCalculator _budgetCalculator;
    private readonly ILoggerFactory _loggerFactory;

    public MaterialPoolDebugTests(ITestOutputHelper output)
    {
        _output = output;
        
        // Setup logging to capture output
        _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        
        var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "RealmEngine.Data", "Data", "Json");
        var fullPath = Path.GetFullPath(dataPath);
        
        _output.WriteLine($"Working Directory: {Directory.GetCurrentDirectory()}");
        _output.WriteLine($"Data Path: {fullPath}");
        _output.WriteLine($"Path Exists: {Directory.Exists(fullPath)}");
        
        _referenceResolver = new DataReferenceResolver();
        _referenceResolver.LoadAllData(fullPath);
        
        _output.WriteLine($"Total Files Loaded: {_referenceResolver.GetLoadedFileCount()}");
        
        _budgetCalculator = new BudgetCalculator(_loggerFactory.CreateLogger<BudgetCalculator>());
    }

    [Fact]
    public async Task Debug_MaterialPoolLoading()
    {
        var configFactory = new BudgetConfigFactory();
        var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "RealmEngine.Data", "Data", "Json");
        var fullPath = Path.GetFullPath(dataPath);
        
        var pools = await configFactory.GetMaterialPoolsAsync(fullPath);
        var enemyTypes = await configFactory.GetEnemyTypesAsync(fullPath);
        
        _output.WriteLine($"Material Pools Loaded: {pools.Pools.Count}");
        _output.WriteLine($"Enemy Types Loaded: {enemyTypes.Types.Count}");
        
        pools.Pools.Should().ContainKey("humanoid_low");
        enemyTypes.Types.Should().ContainKey("goblin");
        
        var goblinConfig = enemyTypes.Types["goblin"];
        _output.WriteLine($"Goblin material pool: {goblinConfig.MaterialPool}");
        _output.WriteLine($"Goblin budget multiplier: {goblinConfig.BudgetMultiplier}");
        
        var humanoidLowPool = pools.Pools["humanoid_low"];
        _output.WriteLine($"humanoid_low pool metals count: {humanoidLowPool.Metals.Count}");
        
        foreach (var metal in humanoidLowPool.Metals)
        {
            _output.WriteLine($"  - {metal.MaterialRef} (weight: {metal.SelectionWeight})");
        }
    }

    [Fact]
    public async Task Debug_MaterialResolution()
    {
        var ironRef = "@items/materials/metals:iron";
        var resolved = await _referenceResolver.ResolveAsync(ironRef);
        
        _output.WriteLine($"Resolved iron: {resolved != null}");
        if (resolved != null)
        {
            _output.WriteLine($"Iron type: {resolved.GetType().Name}");
            _output.WriteLine($"Iron value: {resolved}");
            
            var cost = _budgetCalculator.CalculateMaterialCost(resolved as Newtonsoft.Json.Linq.JToken);
            _output.WriteLine($"Iron cost: {cost}");
        }
    }

    [Fact]
    public async Task Debug_MaterialPoolSelection()
    {
        var configFactory = new BudgetConfigFactory();
        var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "RealmEngine.Data", "Data", "Json");
        var fullPath = Path.GetFullPath(dataPath);
        
        var pools = await configFactory.GetMaterialPoolsAsync(fullPath);
        var enemyTypes = await configFactory.GetEnemyTypesAsync(fullPath);
        
        var materialPoolService = new MaterialPoolService(
            pools,
            enemyTypes,
            _referenceResolver,
            _budgetCalculator,
            _loggerFactory.CreateLogger<MaterialPoolService>()
        );
        
        // Level 1 goblin budget calculation
        int level = 1;
        decimal baseBudget = level * 5.0m;
        var goblinConfig = enemyTypes.Types["goblin"];
        decimal totalBudget = baseBudget * goblinConfig.BudgetMultiplier;
        
        _output.WriteLine($"Level {level} goblin total budget: {totalBudget}");
        
        var budgetConfig = await configFactory.GetBudgetConfigAsync(fullPath);
        decimal materialBudget = totalBudget * budgetConfig.BudgetAllocation.MaterialPercentage;
        
        _output.WriteLine($"Material budget (30%): {materialBudget}");
        
        var material = await materialPoolService.SelectMaterialAsync("goblin", (int)materialBudget);
        
        _output.WriteLine($"Selected material: {material != null}");
        if (material != null)
        {
            _output.WriteLine($"Material name: {material["name"]}");
            _output.WriteLine($"Material budgetCost: {material["budgetCost"]}");
        }
        
        material.Should().NotBeNull("A material should be selected for level 1 goblin");
    }
}
