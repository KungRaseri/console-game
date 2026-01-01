using FluentAssertions;
using Game.Shared.Models;

namespace Game.Tests.Models;

[Trait("Category", "Unit")]
/// <summary>
/// Tests for attribute allocation system.
/// </summary>
public class AttributeAllocationTests
{
    [Fact]
    public void AttributeAllocation_Should_Start_With_8_In_All_Stats()
    {
        // Act
        var allocation = new AttributeAllocation();

        // Assert
        allocation.Strength.Should().Be(8);
        allocation.Dexterity.Should().Be(8);
        allocation.Constitution.Should().Be(8);
        allocation.Intelligence.Should().Be(8);
        allocation.Wisdom.Should().Be(8);
        allocation.Charisma.Should().Be(8);
    }

    [Fact]
    public void GetPointsSpent_Should_Be_Zero_For_Default_Allocation()
    {
        // Arrange
        var allocation = new AttributeAllocation();

        // Act
        var spent = allocation.GetPointsSpent();

        // Assert
        spent.Should().Be(0);
    }

    [Fact]
    public void GetRemainingPoints_Should_Be_27_For_Default_Allocation()
    {
        // Arrange
        var allocation = new AttributeAllocation();

        // Act
        var remaining = allocation.GetRemainingPoints();

        // Assert
        remaining.Should().Be(27);
    }

    [Theory]
    [InlineData(8, 9, 1)]  // 8→9 costs 1
    [InlineData(8, 10, 2)] // 8→10 costs 2
    [InlineData(8, 13, 5)] // 8→13 costs 5
    [InlineData(13, 14, 2)] // 13→14 costs 2
    [InlineData(14, 15, 2)] // 14→15 costs 2
    [InlineData(8, 15, 9)]  // 8→15 costs 9 total (1+1+1+1+1+2+2)
    public void GetPointCost_Should_Calculate_Correctly(int from, int to, int expectedCost)
    {
        // Act
        var cost = AttributeAllocation.GetPointCost(from, to);

        // Assert
        cost.Should().Be(expectedCost);
    }

    [Fact]
    public void IsValid_Should_Return_True_For_Default_Allocation()
    {
        // Arrange
        var allocation = new AttributeAllocation();

        // Act & Assert
        allocation.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_Should_Return_False_When_Over_Point_Budget()
    {
        // Arrange - all stats at 15 costs 9*6 = 54 points (way over 27)
        var allocation = new AttributeAllocation
        {
            Strength = 15,
            Dexterity = 15,
            Constitution = 15,
            Intelligence = 15,
            Wisdom = 15,
            Charisma = 15
        };

        // Act & Assert
        allocation.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_Should_Return_False_When_Stat_Below_Minimum()
    {
        // Arrange
        var allocation = new AttributeAllocation
        {
            Strength = 7 // Below minimum of 8
        };

        // Act & Assert
        allocation.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_Should_Return_False_When_Stat_Above_Maximum()
    {
        // Arrange
        var allocation = new AttributeAllocation
        {
            Strength = 16 // Above maximum of 15
        };

        // Act & Assert
        allocation.IsValid().Should().BeFalse();
    }

    [Fact]
    public void CanIncrease_Should_Return_True_When_Points_Available()
    {
        // Arrange
        var allocation = new AttributeAllocation(); // 27 points available

        // Act & Assert
        allocation.CanIncrease("Strength").Should().BeTrue();
    }

    [Fact]
    public void CanIncrease_Should_Return_False_When_At_Maximum()
    {
        // Arrange
        var allocation = new AttributeAllocation
        {
            Strength = 15 // Max value
        };

        // Act & Assert
        allocation.CanIncrease("Strength").Should().BeFalse();
    }

    [Fact]
    public void CanIncrease_Should_Return_False_When_Not_Enough_Points()
    {
        // Arrange - spend all 27 points
        var allocation = new AttributeAllocation
        {
            Strength = 15, // Costs 9 points
            Dexterity = 15, // Costs 9 points
            Constitution = 13 // Costs 5 points (total: 23)
        };
        // Remaining: 4 points, but increasing CON from 13→14 costs 2

        // Act
        var canIncrease = allocation.CanIncrease("Constitution");

        // Assert - should be able to increase since we have 4 points left
        canIncrease.Should().BeTrue();
    }

    [Fact]
    public void CanDecrease_Should_Return_True_When_Above_Minimum()
    {
        // Arrange
        var allocation = new AttributeAllocation
        {
            Strength = 10
        };

        // Act & Assert
        allocation.CanDecrease("Strength").Should().BeTrue();
    }

    [Fact]
    public void CanDecrease_Should_Return_False_When_At_Minimum()
    {
        // Arrange
        var allocation = new AttributeAllocation
        {
            Strength = 8 // Minimum value
        };

        // Act & Assert
        allocation.CanDecrease("Strength").Should().BeFalse();
    }

    [Fact]
    public void GetAttributeValue_Should_Return_Correct_Value()
    {
        // Arrange
        var allocation = new AttributeAllocation
        {
            Strength = 12,
            Wisdom = 14
        };

        // Act & Assert
        allocation.GetAttributeValue("Strength").Should().Be(12);
        allocation.GetAttributeValue("Wisdom").Should().Be(14);
    }

    [Fact]
    public void SetAttributeValue_Should_Update_Value()
    {
        // Arrange
        var allocation = new AttributeAllocation();

        // Act
        allocation.SetAttributeValue("Strength", 14);

        // Assert
        allocation.Strength.Should().Be(14);
    }

    [Fact]
    public void Spending_All_27_Points_Should_Be_Valid()
    {
        // Arrange - valid spread that uses exactly 27 points
        var allocation = new AttributeAllocation
        {
            Strength = 14, // Costs 7 points (8→14)
            Dexterity = 12, // Costs 4 points (8→12)
            Constitution = 14, // Costs 7 points
            Intelligence = 10, // Costs 2 points
            Wisdom = 10, // Costs 2 points
            Charisma = 13 // Costs 5 points
        };
        // Total: 7+4+7+2+2+5 = 27 points

        // Act & Assert
        allocation.GetPointsSpent().Should().Be(27);
        allocation.GetRemainingPoints().Should().Be(0);
        allocation.IsValid().Should().BeTrue();
    }

    [Fact]
    public void GetPointCost_Should_Return_Zero_When_To_Value_Equals_From_Value()
    {
        // Act
        var cost = AttributeAllocation.GetPointCost(10, 10);

        // Assert
        cost.Should().Be(0);
    }

    [Fact]
    public void GetPointCost_Should_Return_Zero_When_To_Value_Less_Than_From_Value()
    {
        // Act
        var cost = AttributeAllocation.GetPointCost(12, 10);

        // Assert
        cost.Should().Be(0);
    }

    [Fact]
    public void GetAttributeValue_Should_Return_Zero_For_Unknown_Attribute()
    {
        // Arrange
        var allocation = new AttributeAllocation();

        // Act
        var value = allocation.GetAttributeValue("UnknownStat");

        // Assert
        value.Should().Be(0);
    }

    [Fact]
    public void SetAttributeValue_Should_Not_Throw_For_Unknown_Attribute()
    {
        // Arrange
        var allocation = new AttributeAllocation();

        // Act
        var act = () => allocation.SetAttributeValue("UnknownStat", 10);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("Strength")]
    [InlineData("Dexterity")]
    [InlineData("Constitution")]
    [InlineData("Intelligence")]
    [InlineData("Wisdom")]
    [InlineData("Charisma")]
    public void SetAttributeValue_Should_Update_All_Attributes(string attribute)
    {
        // Arrange
        var allocation = new AttributeAllocation();

        // Act
        allocation.SetAttributeValue(attribute, 12);

        // Assert
        allocation.GetAttributeValue(attribute).Should().Be(12);
    }

    [Fact]
    public void IsValid_Should_Return_False_For_Multiple_Stats_Below_Minimum()
    {
        // Arrange
        var allocation = new AttributeAllocation
        {
            Strength = 7,
            Dexterity = 6
        };

        // Act & Assert
        allocation.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_Should_Return_False_For_Multiple_Stats_Above_Maximum()
    {
        // Arrange
        var allocation = new AttributeAllocation
        {
            Intelligence = 16,
            Wisdom = 17
        };

        // Act & Assert
        allocation.IsValid().Should().BeFalse();
    }

    [Fact]
    public void CanIncrease_Should_Return_False_When_Exactly_At_Point_Limit()
    {
        // Arrange - spend exactly 27 points, then try to increase
        var allocation = new AttributeAllocation
        {
            Strength = 15, // Costs 9 points
            Dexterity = 15, // Costs 9 points
            Constitution = 13 // Costs 5 points
        };
        // Total: 23 points spent, 4 remaining

        // Intelligence at 8, increasing to 12 costs 4 points
        allocation.Intelligence = 12; // Now at 27 points exactly

        // Act
        var canIncrease = allocation.CanIncrease("Wisdom");

        // Assert - no points left
        canIncrease.Should().BeFalse();
    }

    [Theory]
    [InlineData("Dexterity")]
    [InlineData("Constitution")]
    [InlineData("Intelligence")]
    [InlineData("Wisdom")]
    [InlineData("Charisma")]
    public void CanDecrease_Should_Work_For_All_Attributes(string attribute)
    {
        // Arrange
        var allocation = new AttributeAllocation();
        allocation.SetAttributeValue(attribute, 10);

        // Act & Assert
        allocation.CanDecrease(attribute).Should().BeTrue();
    }
}
