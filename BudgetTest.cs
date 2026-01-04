// Simple console app to test budget generation
using RealmEngine.Data.Services;
using RealmEngine.Core.Services.Budget;
using Microsoft.Extensions.Logging.Abstractions;

var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "RealmEngine.Data", "Data", "Json");
dataPath = Path.GetFullPath(dataPath);

Console.WriteLine($"Data path: {dataPath}");
Console.WriteLine($"Path exists: {Directory.Exists(dataPath)}");
Console.WriteLine();

var dataCache = new GameDataCache(dataPath);
dataCache.LoadAllData();
Console.WriteLine($"Loaded {dataCache.GetLoadedFileCount()} files");
Console.WriteLine();

var referenceResolver = new ReferenceResolverService(dataCache, NullLogger<ReferenceResolverService>.Instance);
var configFactory = new BudgetConfigFactory(dataCache, NullLogger<BudgetConfigFactory>.Instance);
var budgetConfig = configFactory.GetBudgetConfig();
var materialPools = configFactory.GetMaterialPools();
var enemyTypes = configFactory.GetEnemyTypes();

Console.WriteLine($"Material pools: {materialPools.Pools.Count}");
Console.WriteLine($"Enemy types: {enemyTypes.Types.Count}");
Console.WriteLine();

var budgetCalculator = new BudgetCalculator(budgetConfig, NullLogger<BudgetCalculator>.Instance);
var materialPoolService = new MaterialPoolService(
    dataCache,
    referenceResolver,
    budgetCalculator,
    materialPools,
    enemyTypes,
    NullLogger<MaterialPoolService>.Instance);

var generator = new BudgetItemGenerationService(
    dataCache,
    referenceResolver,
    budgetCalculator,
    materialPoolService,
    NullLogger<BudgetItemGenerationService>.Instance);

var request = new BudgetItemRequest
{
    EnemyType = "goblin",
    EnemyLevel = 1,
    ItemCategory = "weapons"
};

Console.WriteLine($"Generating weapon for level {request.EnemyLevel} {request.EnemyType}...");
Console.WriteLine();

try
{
    var result = await generator.GenerateItemAsync(request);
    
    if (result == null)
    {
        Console.WriteLine("FAILED: Result was null");
    }
    else
    {
        Console.WriteLine("SUCCESS!");
        Console.WriteLine($"Base budget: {result.BaseBudget}");
        Console.WriteLine($"Adjusted budget: {result.AdjustedBudget}");
        Console.WriteLine($"Spent budget: {result.SpentBudget}");
        Console.WriteLine($"Material: {result.Material?["name"]}");
        Console.WriteLine($"Base item: {result.BaseItem?["name"]}");
        Console.WriteLine($"Pattern: {result.PatternUsed}");
        Console.WriteLine($"Components: {result.Components.Count}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"EXCEPTION: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}
