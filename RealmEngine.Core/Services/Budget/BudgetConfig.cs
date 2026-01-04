using Newtonsoft.Json;

namespace RealmEngine.Core.Services.Budget;

/// <summary>
/// Budget configuration loaded from general/budget-config.json.
/// Controls how item budget is calculated and allocated across components.
/// </summary>
public class BudgetConfig
{
  [JsonProperty("metadata")]
  public BudgetMetadata? Metadata { get; set; }

  [JsonProperty("budgetAllocation")]
  public BudgetAllocation Allocation { get; set; } = new();

  [JsonProperty("costFormulas")]
  public CostFormulas Formulas { get; set; } = new();

  [JsonProperty("patternCosts")]
  public Dictionary<string, int> PatternCosts { get; set; } = new();

  [JsonProperty("minimumCosts")]
  public MinimumCosts MinimumCosts { get; set; } = new();

  [JsonProperty("budgetRanges")]
  public Dictionary<string, BudgetRange> BudgetRanges { get; set; } = new();

  [JsonProperty("sourceMultipliers")]
  public SourceMultipliers SourceMultipliers { get; set; } = new();
}

public class BudgetMetadata
{
  [JsonProperty("description")]
  public string Description { get; set; } = string.Empty;

  [JsonProperty("version")]
  public string Version { get; set; } = string.Empty;

  [JsonProperty("lastUpdated")]
  public string LastUpdated { get; set; } = string.Empty;

  [JsonProperty("type")]
  public string Type { get; set; } = string.Empty;
}

public class BudgetAllocation
{
  [JsonProperty("materialPercentage")]
  public double MaterialPercentage { get; set; } = 0.30;

  [JsonProperty("componentPercentage")]
  public double ComponentPercentage { get; set; } = 0.70;

  [JsonProperty("description")]
  public string Description { get; set; } = string.Empty;
}

public class CostFormulas
{
  [JsonProperty("material")]
  public CostFormula Material { get; set; } = new();

  [JsonProperty("component")]
  public CostFormula Component { get; set; } = new();

  [JsonProperty("enchantment")]
  public CostFormula Enchantment { get; set; } = new();

  [JsonProperty("materialQuality")]
  public CostFormula MaterialQuality { get; set; } = new();
}

public class CostFormula
{
  [JsonProperty("formula")]
  public string Formula { get; set; } = string.Empty;

  [JsonProperty("numerator")]
  public int? Numerator { get; set; }

  [JsonProperty("field")]
  public string Field { get; set; } = string.Empty;

  [JsonProperty("description")]
  public string Description { get; set; } = string.Empty;
}

public class MinimumCosts
{
  [JsonProperty("materialQuality")]
  public int MaterialQuality { get; set; } = 5;

  [JsonProperty("prefix")]
  public int Prefix { get; set; } = 3;

  [JsonProperty("suffix")]
  public int Suffix { get; set; } = 3;

  [JsonProperty("descriptive")]
  public int Descriptive { get; set; } = 3;

  [JsonProperty("enchantment")]
  public int Enchantment { get; set; } = 15;

  [JsonProperty("socket")]
  public int Socket { get; set; } = 10;
}

public class BudgetRange
{
  [JsonProperty("min")]
  public int Min { get; set; }

  [JsonProperty("max")]
  public int Max { get; set; }
}

public class SourceMultipliers
{
  [JsonProperty("enemyLevelMultiplier")]
  public double EnemyLevelMultiplier { get; set; } = 5.0;

  [JsonProperty("shopTierBase")]
  public int ShopTierBase { get; set; } = 30;

  [JsonProperty("bossMultiplier")]
  public double BossMultiplier { get; set; } = 2.5;

  [JsonProperty("eliteMultiplier")]
  public double EliteMultiplier { get; set; } = 1.5;
}
