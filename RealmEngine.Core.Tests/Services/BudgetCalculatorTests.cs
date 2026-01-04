using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using RealmEngine.Core.Services.Budget;
using Xunit;

namespace RealmEngine.Core.Tests.Services;

public class BudgetCalculatorTests
{
    private readonly BudgetCalculator _calculator;
    private readonly BudgetConfig _config;

    public BudgetCalculatorTests()
    {
        _config = new BudgetConfig
        {
            Allocation = new BudgetAllocation
            {
                MaterialPercentage = 0.30,
                ComponentPercentage = 0.70
            },
            Formulas = new CostFormulas
            {
                Material = new CostFormula { Formula = "direct", Field = "budgetCost" },
                Component = new CostFormula { Formula = "inverse", Numerator = 100, Field = "selectionWeight" },
                Enchantment = new CostFormula { Formula = "inverse", Numerator = 130, Field = "selectionWeight" },
                MaterialQuality = new CostFormula { Formula = "inverse", Numerator = 150, Field = "selectionWeight" }
            },
            PatternCosts = new Dictionary<string, int>
            {
                ["{base}"] = 0,
                ["{prefix} {base} {suffix}"] = 5
            },
            SourceMultipliers = new SourceMultipliers
            {
                EnemyLevelMultiplier = 5.0,
                BossMultiplier = 2.5,
                EliteMultiplier = 1.5
            }
        };

        _calculator = new BudgetCalculator(_config, NullLogger<BudgetCalculator>.Instance);
    }

    [Fact]
    public void CalculateBaseBudget_Level10_Returns50()
    {
        // Arrange & Act
        var result = _calculator.CalculateBaseBudget(10);

        // Assert
        result.Should().Be(50); // 10 * 5.0
    }

    [Fact]
    public void CalculateBaseBudget_Level10Boss_Returns125()
    {
        // Arrange & Act
        var result = _calculator.CalculateBaseBudget(10, isBoss: true);

        // Assert
        result.Should().Be(125); // 10 * 5.0 * 2.5
    }

    [Fact]
    public void CalculateBaseBudget_Level10Elite_Returns75()
    {
        // Arrange & Act
        var result = _calculator.CalculateBaseBudget(10, isElite: true);

        // Assert
        result.Should().Be(75); // 10 * 5.0 * 1.5
    }

    [Fact]
    public void ApplyQualityModifier_PositiveModifier_IncreaseBudget()
    {
        // Arrange
        var baseBudget = 100;
        var qualityModifier = 0.50; // +50%

        // Act
        var result = _calculator.ApplyQualityModifier(baseBudget, qualityModifier);

        // Assert
        result.Should().Be(150); // 100 * (1.0 + 0.5)
    }

    [Fact]
    public void ApplyQualityModifier_NegativeModifier_DecreaseBudget()
    {
        // Arrange
        var baseBudget = 100;
        var qualityModifier = -0.40; // -40%

        // Act
        var result = _calculator.ApplyQualityModifier(baseBudget, qualityModifier);

        // Assert
        result.Should().Be(60); // 100 * (1.0 - 0.4)
    }

    [Fact]
    public void CalculateMaterialBudget_DefaultPercentage_Returns30Percent()
    {
        // Arrange
        var totalBudget = 100;

        // Act
        var result = _calculator.CalculateMaterialBudget(totalBudget);

        // Assert
        result.Should().Be(30); // 100 * 0.30
    }

    [Fact]
    public void CalculateMaterialBudget_CustomPercentage_ReturnsCustomAmount()
    {
        // Arrange
        var totalBudget = 100;

        // Act
        var result = _calculator.CalculateMaterialBudget(totalBudget, 0.40);

        // Assert
        result.Should().Be(40); // 100 * 0.40
    }

    [Fact]
    public void CalculateComponentBudget_ReturnsRemainder()
    {
        // Arrange
        var totalBudget = 100;
        var materialBudget = 30;

        // Act
        var result = _calculator.CalculateComponentBudget(totalBudget, materialBudget);

        // Assert
        result.Should().Be(70); // 100 - 30
    }

    [Fact]
    public void CalculateMaterialCost_DirectFormula_ReturnsBudgetCost()
    {
        // Arrange
        var material = JToken.Parse(@"{
            ""name"": ""Iron"",
            ""budgetCost"": 25
        }");

        // Act
        var result = _calculator.CalculateMaterialCost(material);

        // Assert
        result.Should().Be(25);
    }

    [Fact]
    public void CalculateComponentCost_InverseFormula_ReturnsInverseCost()
    {
        // Arrange
        var component = JToken.Parse(@"{
            ""value"": ""Flaming"",
            ""selectionWeight"": 20
        }");

        // Act
        var result = _calculator.CalculateComponentCost(component);

        // Assert
        result.Should().Be(5); // 100 / 20
    }

    [Fact]
    public void CalculateEnchantmentCost_PremiumFormula_ReturnsHigherCost()
    {
        // Arrange
        var enchantment = JToken.Parse(@"{
            ""value"": ""of Power"",
            ""selectionWeight"": 10
        }");

        // Act
        var result = _calculator.CalculateEnchantmentCost(enchantment);

        // Assert
        result.Should().Be(13); // 130 / 10
    }

    [Fact]
    public void CalculateQualityCost_InverseFormula_ReturnsQualityCost()
    {
        // Arrange
        var quality = JToken.Parse(@"{
            ""value"": ""Fine"",
            ""selectionWeight"": 20
        }");

        // Act
        var result = _calculator.CalculateQualityCost(quality);

        // Assert
        result.Should().Be(8); // 150 / 20 = 7.5, rounded to 8
    }

    [Fact]
    public void GetPatternCost_KnownPattern_ReturnsCost()
    {
        // Arrange & Act
        var result = _calculator.GetPatternCost("{prefix} {base} {suffix}");

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public void GetPatternCost_UnknownPattern_ReturnsZero()
    {
        // Arrange & Act
        var result = _calculator.GetPatternCost("{unknown} {pattern}");

        // Assert
        result.Should().Be(0);
    }

    [Theory]
    [InlineData(100, 50, true)]
    [InlineData(100, 100, true)]
    [InlineData(100, 101, false)]
    [InlineData(50, 51, false)]
    public void CanAfford_VariousBudgets_ReturnsCorrectly(int budget, int cost, bool expected)
    {
        // Act
        var result = _calculator.CanAfford(budget, cost);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void CalculateComponentCost_HighSelectionWeight_ReturnsLowCost()
    {
        // Arrange - Common component
        var component = JToken.Parse(@"{
            ""value"": ""Common Prefix"",
            ""selectionWeight"": 50
        }");

        // Act
        var result = _calculator.CalculateComponentCost(component);

        // Assert
        result.Should().Be(2); // 100 / 50 = 2 (low cost for common)
    }

    [Fact]
    public void CalculateComponentCost_LowSelectionWeight_ReturnsHighCost()
    {
        // Arrange - Rare component
        var component = JToken.Parse(@"{
            ""value"": ""Rare Prefix"",
            ""selectionWeight"": 5
        }");

        // Act
        var result = _calculator.CalculateComponentCost(component);

        // Assert
        result.Should().Be(20); // 100 / 5 = 20 (high cost for rare)
    }
}
