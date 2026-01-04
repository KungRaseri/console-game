using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RealmEngine.Data.Services;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Services.Budget;

/// <summary>
/// Budget-based item generation service implementing forward-building approach.
/// Generates items by allocating budget across materials and components.
/// </summary>
public class BudgetItemGenerationService
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly BudgetCalculator _budgetCalculator;
    private readonly MaterialPoolService _materialPoolService;
    private readonly ILogger<BudgetItemGenerationService> _logger;
    private readonly Random _random;

    public BudgetItemGenerationService(
        GameDataCache dataCache,
        ReferenceResolverService referenceResolver,
        BudgetCalculator budgetCalculator,
        MaterialPoolService materialPoolService,
        ILogger<BudgetItemGenerationService> logger)
    {
        _dataCache = dataCache ?? throw new ArgumentNullException(nameof(dataCache));
        _referenceResolver = referenceResolver ?? throw new ArgumentNullException(nameof(referenceResolver));
        _budgetCalculator = budgetCalculator ?? throw new ArgumentNullException(nameof(budgetCalculator));
        _materialPoolService = materialPoolService ?? throw new ArgumentNullException(nameof(materialPoolService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = new Random();
    }

    /// <summary>
    /// Generate an item with budget-based forward building.
    /// </summary>
    public async Task<BudgetItemResult?> GenerateItemAsync(BudgetItemRequest request)
    {
        try
        {
            // Step 1: Calculate base budget
            var baseBudget = _budgetCalculator.CalculateBaseBudget(
                request.EnemyLevel, 
                request.IsBoss, 
                request.IsElite);

            // Apply enemy type budget multiplier
            var typeMultiplier = _materialPoolService.GetBudgetMultiplier(request.EnemyType);
            baseBudget = (int)Math.Round(baseBudget * typeMultiplier);

            _logger.LogDebug("Base budget calculated: {Budget} (level={Level}, type={Type}, multiplier={Multiplier})", 
                baseBudget, request.EnemyLevel, request.EnemyType, typeMultiplier);

            // Step 2: Select material quality (optional, affects total budget)
            JToken? qualityComponent = null;
            var qualityModifier = 0.0;

            if (request.AllowQuality)
            {
                qualityComponent = await SelectQualityAsync();
                if (qualityComponent != null)
                {
                    qualityModifier = GetDoubleProperty(qualityComponent, "budgetModifier", 0.0);
                }
            }

            // Step 3: Apply quality modifier to budget (BEFORE split)
            var adjustedBudget = _budgetCalculator.ApplyQualityModifier(baseBudget, qualityModifier);

            _logger.LogDebug("Quality modifier applied: {Modifier} -> Adjusted budget: {Budget}", 
                qualityModifier, adjustedBudget);

            // Step 4: Split budget into material and component budgets
            var materialPercentageOverride = _materialPoolService.GetMaterialPercentage(request.EnemyType);
            var materialBudget = _budgetCalculator.CalculateMaterialBudget(adjustedBudget, materialPercentageOverride);
            var componentBudget = _budgetCalculator.CalculateComponentBudget(adjustedBudget, materialBudget);

            _logger.LogDebug("Budget split: Material={MaterialBudget}, Components={ComponentBudget}", 
                materialBudget, componentBudget);

            // Step 5: Select material from enemy-specific pool
            var material = await _materialPoolService.SelectMaterialAsync(request.EnemyType, materialBudget);
            if (material == null)
            {
                _logger.LogWarning("Failed to select material for enemy type {EnemyType}", request.EnemyType);
                return null;
            }

            var materialCost = _budgetCalculator.CalculateMaterialCost(material);
            var remainingBudget = componentBudget; // Components use their allocated budget

            _logger.LogDebug("Material selected: {MaterialName} (cost={Cost})", 
                GetStringProperty(material, "name"), materialCost);

            // Step 6: Select base item (weapon/armor type) - uses component budget
            var baseItem = await SelectBaseItemAsync(request.ItemCategory);
            if (baseItem == null)
            {
                _logger.LogWarning("Failed to select base item from category {Category}", request.ItemCategory);
                return null;
            }

            var baseItemCost = GetIntProperty(baseItem, "budgetCost", 0);
            if (!_budgetCalculator.CanAfford(remainingBudget, baseItemCost))
            {
                _logger.LogWarning("Cannot afford base item {ItemName} (cost={Cost}, budget={Budget})", 
                    GetStringProperty(baseItem, "name"), baseItemCost, remainingBudget);
                return null;
            }

            remainingBudget -= baseItemCost;

            // Step 7: Select pattern from names.json
            var pattern = await SelectPatternAsync(request.ItemCategory);
            if (pattern == null)
            {
                _logger.LogWarning("Failed to select pattern for category {Category}", request.ItemCategory);
                return null;
            }

            var patternString = GetStringProperty(pattern, "pattern");
            var patternCost = _budgetCalculator.GetPatternCost(patternString);
            remainingBudget -= patternCost;

            // Step 8: Forward-build components that fit budget
            var components = await SelectComponentsAsync(
                request.ItemCategory, 
                patternString, 
                remainingBudget);

            // Step 9: Build result
            var result = new BudgetItemResult
            {
                BaseBudget = baseBudget,
                AdjustedBudget = adjustedBudget,
                MaterialBudget = materialBudget,
                ComponentBudget = componentBudget,
                SpentBudget = materialCost + baseItemCost + patternCost + components.Sum(c => c.Cost),
                Material = material,
                MaterialCost = materialCost,
                Quality = qualityComponent,
                QualityModifier = qualityModifier,
                BaseItem = baseItem,
                BaseItemCost = baseItemCost,
                Pattern = patternString,
                PatternCost = patternCost,
                Components = components.Select(c => c.Component).ToList(),
                ComponentCosts = components.ToDictionary(c => GetStringProperty(c.Component, "value"), c => c.Cost)
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating budget-based item");
            return null;
        }
    }

    private Task<JToken?> SelectQualityAsync()
    {
        var namesPath = "items/materials/names.json";
        if (!_dataCache.FileExists(namesPath))
            return null;

        var namesFile = _dataCache.GetFile(namesPath);
        if (namesFile?.JsonData == null)
            return null;

        var qualityComponents = namesFile.JsonData["components"]?["quality"];
        if (qualityComponents == null)
            return null;

        // 50% chance to not have quality modifier
        if (_random.Next(100) < 50)
            return Task.FromResult<JToken?>(null);

        return Task.FromResult(SelectWeightedRandomComponent(qualityComponents));
    }

    private Task<JToken?> SelectBaseItemAsync(string category)
    {
        var catalogPath = $"items/{category}/catalog.json";
        if (!_dataCache.FileExists(catalogPath))
            return null;

        var catalogFile = _dataCache.GetFile(catalogPath);
        if (catalogFile?.JsonData == null)
            return null;

        var items = GetItemsFromCatalog(catalogFile.JsonData);
        if (items == null || !items.Any())
            return Task.FromResult<JToken?>(null);

        return Task.FromResult(SelectWeightedRandomItem(items));
    }

    private Task<JToken?> SelectPatternAsync(string category)
    {
        var namesPath = $"items/{category}/names.json";
        if (!_dataCache.FileExists(namesPath))
            return null;

        var namesFile = _dataCache.GetFile(namesPath);
        if (namesFile?.JsonData == null)
            return null;

        var patterns = namesFile.JsonData["patterns"];
        if (patterns == null)
            return Task.FromResult<JToken?>(null);

        return Task.FromResult(SelectWeightedRandomPattern(patterns));
    }

    private Task<List<(JToken Component, int Cost)>> SelectComponentsAsync(
        string category,
        string patternString,
        int availableBudget)
    {
        var result = new List<(JToken, int)>();

        var namesPath = $"items/{category}/names.json";
        if (!_dataCache.FileExists(namesPath))
            return result;

        var namesFile = _dataCache.GetFile(namesPath);
        if (namesFile?.JsonData == null)
            return result;

        var components = namesFile.JsonData["components"];
        if (components == null)
            return result;

        // Parse pattern tokens (e.g., {prefix}, {suffix}, {descriptive})
        var tokens = ExtractTokens(patternString);

        foreach (var token in tokens)
        {
            // Skip base and quality tokens
            if (token == "base" || token == "quality")
                continue;

            var componentArray = components[token];
            if (componentArray == null)
                continue;

            // Try to select affordable component
            var affordableComponents = componentArray
                .Where(c => _budgetCalculator.CanAfford(availableBudget, _budgetCalculator.CalculateComponentCost(c)))
                .ToList();

            if (!affordableComponents.Any())
                continue;

            var selected = SelectWeightedRandomComponent(JToken.FromObject(affordableComponents));
            if (selected != null)
            {
                var cost = _budgetCalculator.CalculateComponentCost(selected);
                result.Add((selected, cost));
                availableBudget -= cost;
            }
        }

        return Task.FromResult(result);
    }

    private List<string> ExtractTokens(string patternString)
    {
        var tokens = new List<string>();
        var currentToken = "";
        var inToken = false;

        foreach (var ch in patternString)
        {
            if (ch == '{')
            {
                inToken = true;
                currentToken = "";
            }
            else if (ch == '}')
            {
                if (inToken && !string.IsNullOrEmpty(currentToken))
                {
                    tokens.Add(currentToken);
                }
                inToken = false;
            }
            else if (inToken)
            {
                currentToken += ch;
            }
        }

        return tokens;
    }

    private JToken? SelectWeightedRandomItem(IEnumerable<JToken> items)
    {
        var itemList = items.ToList();
        if (!itemList.Any()) return null;

        var totalWeight = itemList.Sum(i => GetIntProperty(i, "selectionWeight", 1));
        var randomValue = _random.Next(totalWeight);
        var cumulative = 0;

        foreach (var item in itemList)
        {
            cumulative += GetIntProperty(item, "selectionWeight", 1);
            if (randomValue < cumulative)
                return item;
        }

        return itemList.Last();
    }

    private JToken? SelectWeightedRandomPattern(JToken patterns)
    {
        var patternList = patterns.Children().ToList();
        if (!patternList.Any()) return null;

        var totalWeight = patternList.Sum(p => GetIntProperty(p, "rarityWeight", 1));
        var randomValue = _random.Next(totalWeight);
        var cumulative = 0;

        foreach (var pattern in patternList)
        {
            cumulative += GetIntProperty(pattern, "rarityWeight", 1);
            if (randomValue < cumulative)
                return pattern;
        }

        return patternList.Last();
    }

    private JToken? SelectWeightedRandomComponent(JToken components)
    {
        var componentList = components.Children().ToList();
        if (!componentList.Any()) return null;

        var totalWeight = componentList.Sum(c => GetIntProperty(c, "rarityWeight", 1));
        var randomValue = _random.Next(totalWeight);
        var cumulative = 0;

        foreach (var component in componentList)
        {
            cumulative += GetIntProperty(component, "rarityWeight", 1);
            if (randomValue < cumulative)
                return component;
        }

        return componentList.Last();
    }

    private static IEnumerable<JToken>? GetItemsFromCatalog(JToken catalog)
    {
        var allItems = new List<JToken>();
        
        if (catalog["items"] != null)
        {
            return catalog["items"]?.Children();
        }
        
        foreach (var property in catalog.Children<JProperty>())
        {
            if (property.Name == "metadata") continue;
            
            var typeContainer = property.Value;
            if (typeContainer is JObject typeObj)
            {
                foreach (var typeProperty in typeObj.Children<JProperty>())
                {
                    var items = typeProperty.Value["items"];
                    if (items != null)
                    {
                        foreach (var item in items.Children())
                        {
                            allItems.Add(item);
                        }
                    }
                }
            }
        }

        return allItems.Any() ? allItems : null;
    }

    private static string GetStringProperty(JToken token, string propertyName)
    {
        return token[propertyName]?.Value<string>() ?? string.Empty;
    }

    private static int GetIntProperty(JToken token, string propertyName, int defaultValue)
    {
        return token[propertyName]?.Value<int>() ?? defaultValue;
    }

    private static double GetDoubleProperty(JToken token, string propertyName, double defaultValue)
    {
        return token[propertyName]?.Value<double>() ?? defaultValue;
    }
}

/// <summary>
/// Request parameters for budget-based item generation.
/// </summary>
public class BudgetItemRequest
{
    public string EnemyType { get; set; } = "default";
    public int EnemyLevel { get; set; } = 1;
    public bool IsBoss { get; set; } = false;
    public bool IsElite { get; set; } = false;
    public string ItemCategory { get; set; } = "weapons";
    public bool AllowQuality { get; set; } = true;
}

/// <summary>
/// Result of budget-based item generation with detailed breakdown.
/// </summary>
public class BudgetItemResult
{
    public int BaseBudget { get; set; }
    public int AdjustedBudget { get; set; }
    public int MaterialBudget { get; set; }
    public int ComponentBudget { get; set; }
    public int SpentBudget { get; set; }
    
    public JToken? Material { get; set; }
    public int MaterialCost { get; set; }
    
    public JToken? Quality { get; set; }
    public double QualityModifier { get; set; }
    
    public JToken? BaseItem { get; set; }
    public int BaseItemCost { get; set; }
    
    public string Pattern { get; set; } = string.Empty;
    public int PatternCost { get; set; }
    
    public List<JToken> Components { get; set; } = new();
    public Dictionary<string, int> ComponentCosts { get; set; } = new();
}
