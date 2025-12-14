using FluentAssertions;
using Game.Models;
using Game.Shared.Models;
using Game.Utilities;
using Xunit;

namespace Game.Tests.Utilities;

/// <summary>
/// Tests for TraitApplicator utility class.
/// </summary>
public class TraitApplicatorTests
{
    // Simple test class implementing ITraitable
    private class TestEntity : ITraitable
    {
        public Dictionary<string, TraitValue> Traits { get; set; } = new();
    }

    #region Apply Traits Tests

    [Fact]
    public void ApplyTraits_Should_Add_All_Traits_To_Entity()
    {
        // Arrange
        var entity = new TestEntity();
        var traits = new Dictionary<string, TraitValue>
        {
            { "strength", new TraitValue(15, TraitType.Number) },
            { "name", new TraitValue("TestEntity", TraitType.String) },
            { "isActive", new TraitValue(true, TraitType.Boolean) }
        };

        // Act
        TraitApplicator.ApplyTraits(entity, traits);

        // Assert
        entity.Traits.Should().HaveCount(3);
        entity.Traits["strength"].AsInt().Should().Be(15);
        entity.Traits["name"].AsString().Should().Be("TestEntity");
        entity.Traits["isActive"].AsBool().Should().BeTrue();
    }

    [Fact]
    public void ApplyTraits_Should_Overwrite_Existing_Traits()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Traits["power"] = new TraitValue(10, TraitType.Number);

        var traits = new Dictionary<string, TraitValue>
        {
            { "power", new TraitValue(20, TraitType.Number) }
        };

        // Act
        TraitApplicator.ApplyTraits(entity, traits);

        // Assert
        entity.Traits["power"].AsInt().Should().Be(20);
    }

    [Fact]
    public void ApplyTrait_Should_Add_Single_Trait()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        TraitApplicator.ApplyTrait(entity, "defense", 25, TraitType.Number);

        // Assert
        entity.Traits.Should().ContainKey("defense");
        entity.Traits["defense"].AsInt().Should().Be(25);
    }

    [Fact]
    public void ApplyTrait_Should_Overwrite_Existing_Trait()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Traits["speed"] = new TraitValue(5, TraitType.Number);

        // Act
        TraitApplicator.ApplyTrait(entity, "speed", 10, TraitType.Number);

        // Assert
        entity.Traits["speed"].AsInt().Should().Be(10);
    }

    #endregion

    #region Get Trait Tests

    [Fact]
    public void GetTrait_Should_Return_Int_Value()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Traits["level"] = new TraitValue(42, TraitType.Number);

        // Act
        var value = TraitApplicator.GetTrait(entity, "level", 0);

        // Assert
        value.Should().Be(42);
    }

    [Fact]
    public void GetTrait_Should_Return_Double_Value()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Traits["multiplier"] = new TraitValue(1.5, TraitType.Number);

        // Act
        var value = TraitApplicator.GetTrait(entity, "multiplier", 0.0);

        // Assert
        value.Should().BeApproximately(1.5, 0.001);
    }

    [Fact]
    public void GetTrait_Should_Return_String_Value()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Traits["title"] = new TraitValue("Champion", TraitType.String);

        // Act
        var value = TraitApplicator.GetTrait(entity, "title", "Unknown");

        // Assert
        value.Should().Be("Champion");
    }

    [Fact]
    public void GetTrait_Should_Return_Bool_Value()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Traits["canFly"] = new TraitValue(true, TraitType.Boolean);

        // Act
        var value = TraitApplicator.GetTrait(entity, "canFly", false);

        // Assert
        value.Should().BeTrue();
    }

    [Fact]
    public void GetTrait_Should_Return_StringList_Value()
    {
        // Arrange
        var entity = new TestEntity();
        var tags = new List<string> { "Fire", "Ice", "Lightning" };
        entity.Traits["elements"] = new TraitValue(tags, TraitType.StringArray);

        // Act
        var value = TraitApplicator.GetTrait(entity, "elements", new List<string>());

        // Assert
        value.Should().BeEquivalentTo(tags);
    }

    [Fact]
    public void GetTrait_Should_Return_IntList_Value()
    {
        // Arrange
        var entity = new TestEntity();
        var numbers = new List<int> { 1, 2, 3, 4, 5 };
        entity.Traits["scores"] = new TraitValue(numbers, TraitType.NumberArray);

        // Act
        var value = TraitApplicator.GetTrait(entity, "scores", new List<int>());

        // Assert
        value.Should().BeEquivalentTo(numbers);
    }

    [Fact]
    public void GetTrait_Should_Return_Default_When_Trait_Missing()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        var intValue = TraitApplicator.GetTrait(entity, "missing", 99);
        var stringValue = TraitApplicator.GetTrait(entity, "missing", "default");
        var boolValue = TraitApplicator.GetTrait(entity, "missing", true);

        // Assert
        intValue.Should().Be(99);
        stringValue.Should().Be("default");
        boolValue.Should().BeTrue();
    }

    #endregion

    #region Has Trait Tests

    [Fact]
    public void HasTrait_Should_Return_True_When_Trait_Exists()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Traits["immunity"] = new TraitValue("poison", TraitType.String);

        // Act
        var hasTrait = TraitApplicator.HasTrait(entity, "immunity");

        // Assert
        hasTrait.Should().BeTrue();
    }

    [Fact]
    public void HasTrait_Should_Return_False_When_Trait_Missing()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        var hasTrait = TraitApplicator.HasTrait(entity, "nonexistent");

        // Assert
        hasTrait.Should().BeFalse();
    }

    #endregion

    #region Remove Trait Tests

    [Fact]
    public void RemoveTrait_Should_Remove_Existing_Trait()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Traits["temporary"] = new TraitValue(100, TraitType.Number);

        // Act
        TraitApplicator.RemoveTrait(entity, "temporary");

        // Assert
        entity.Traits.Should().NotContainKey("temporary");
    }

    [Fact]
    public void RemoveTrait_Should_Not_Throw_When_Trait_Missing()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        var act = () => TraitApplicator.RemoveTrait(entity, "nonexistent");

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region Get Trait Names Tests

    [Fact]
    public void GetTraitNames_Should_Return_All_Trait_Keys()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Traits["str"] = new TraitValue(10, TraitType.Number);
        entity.Traits["dex"] = new TraitValue(12, TraitType.Number);
        entity.Traits["con"] = new TraitValue(14, TraitType.Number);

        // Act
        var names = TraitApplicator.GetTraitNames(entity);

        // Assert
        names.Should().HaveCount(3);
        names.Should().Contain(new[] { "str", "dex", "con" });
    }

    [Fact]
    public void GetTraitNames_Should_Return_Empty_List_When_No_Traits()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        var names = TraitApplicator.GetTraitNames(entity);

        // Assert
        names.Should().BeEmpty();
    }

    #endregion

    #region Merge Traits Tests

    [Fact]
    public void MergeTraits_Should_Add_Missing_Traits_Only()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Traits["existing"] = new TraitValue(100, TraitType.Number);

        var sourceTraits = new Dictionary<string, TraitValue>
        {
            { "existing", new TraitValue(200, TraitType.Number) }, // Should NOT overwrite
            { "new", new TraitValue(50, TraitType.Number) }         // Should add
        };

        // Act
        TraitApplicator.MergeTraits(entity, sourceTraits);

        // Assert
        entity.Traits["existing"].AsInt().Should().Be(100); // Original preserved
        entity.Traits["new"].AsInt().Should().Be(50);       // New added
    }

    [Fact]
    public void MergeTraits_Should_Add_All_When_Target_Empty()
    {
        // Arrange
        var entity = new TestEntity();
        var sourceTraits = new Dictionary<string, TraitValue>
        {
            { "a", new TraitValue(1, TraitType.Number) },
            { "b", new TraitValue(2, TraitType.Number) }
        };

        // Act
        TraitApplicator.MergeTraits(entity, sourceTraits);

        // Assert
        entity.Traits.Should().HaveCount(2);
        entity.Traits["a"].AsInt().Should().Be(1);
        entity.Traits["b"].AsInt().Should().Be(2);
    }

    #endregion

    #region Numeric Bonus Tests

    [Fact]
    public void AddNumericBonus_Should_Add_To_Existing_Trait()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Traits["damage"] = new TraitValue(10, TraitType.Number);

        // Act
        TraitApplicator.AddNumericBonus(entity, "damage", 5);

        // Assert
        entity.Traits["damage"].AsDouble().Should().BeApproximately(15.0, 0.001);
    }

    [Fact]
    public void AddNumericBonus_Should_Create_Trait_If_Missing()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        TraitApplicator.AddNumericBonus(entity, "newBonus", 25);

        // Assert
        entity.Traits["newBonus"].AsDouble().Should().BeApproximately(25.0, 0.001);
    }

    [Fact]
    public void AddNumericBonus_Should_Handle_Negative_Values()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Traits["health"] = new TraitValue(100, TraitType.Number);

        // Act
        TraitApplicator.AddNumericBonus(entity, "health", -30);

        // Assert
        entity.Traits["health"].AsDouble().Should().BeApproximately(70.0, 0.001);
    }

    #endregion

    #region Total Stat Bonus Tests

    [Fact]
    public void GetTotalStatBonus_Should_Sum_All_Traits()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Traits["baseStr"] = new TraitValue(10, TraitType.Number);
        entity.Traits["bonusStr"] = new TraitValue(5, TraitType.Number);
        entity.Traits["titanStr"] = new TraitValue(3, TraitType.Number);

        // Act
        var total = TraitApplicator.GetTotalStatBonus(entity, "baseStr", "bonusStr", "titanStr");

        // Assert
        total.Should().Be(18);
    }

    [Fact]
    public void GetTotalStatBonus_Should_Ignore_Missing_Traits()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Traits["str"] = new TraitValue(10, TraitType.Number);

        // Act
        var total = TraitApplicator.GetTotalStatBonus(entity, "str", "missing1", "missing2");

        // Assert
        total.Should().Be(10);
    }

    [Fact]
    public void GetTotalStatBonus_Should_Return_0_When_No_Traits()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        var total = TraitApplicator.GetTotalStatBonus(entity, "a", "b", "c");

        // Assert
        total.Should().Be(0);
    }

    #endregion

    #region Resistance Tests

    [Fact]
    public void GetResistance_Should_Return_Resistance_Value()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Traits[StandardTraits.ResistFire] = new TraitValue(50, TraitType.Number);

        // Act
        var resistance = TraitApplicator.GetResistance(entity, StandardTraits.ResistFire);

        // Assert
        resistance.Should().Be(50);
    }

    [Fact]
    public void GetResistance_Should_Return_0_When_Missing()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        var resistance = TraitApplicator.GetResistance(entity, StandardTraits.ResistIce);

        // Assert
        resistance.Should().Be(0);
    }

    [Fact]
    public void HasResistance_Should_Return_True_When_Resistance_Greater_Than_0()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Traits[StandardTraits.ResistPoison] = new TraitValue(25, TraitType.Number);

        // Act
        var hasResistance = TraitApplicator.HasResistance(entity, StandardTraits.ResistPoison);

        // Assert
        hasResistance.Should().BeTrue();
    }

    [Fact]
    public void HasResistance_Should_Return_False_When_Resistance_Is_0()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Traits[StandardTraits.ResistLightning] = new TraitValue(0, TraitType.Number);

        // Act
        var hasResistance = TraitApplicator.HasResistance(entity, StandardTraits.ResistLightning);

        // Assert
        hasResistance.Should().BeFalse();
    }

    [Fact]
    public void GetAllResistances_Should_Return_All_Positive_Resistances()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Traits[StandardTraits.ResistFire] = new TraitValue(30, TraitType.Number);
        entity.Traits[StandardTraits.ResistIce] = new TraitValue(0, TraitType.Number);
        entity.Traits[StandardTraits.ResistLightning] = new TraitValue(50, TraitType.Number);
        entity.Traits[StandardTraits.ResistPoison] = new TraitValue(10, TraitType.Number);

        // Act
        var resistances = TraitApplicator.GetAllResistances(entity);

        // Assert
        resistances.Should().HaveCount(3);
        resistances[StandardTraits.ResistFire].Should().Be(30);
        resistances[StandardTraits.ResistLightning].Should().Be(50);
        resistances[StandardTraits.ResistPoison].Should().Be(10);
        resistances.Should().NotContainKey(StandardTraits.ResistIce);
    }

    [Fact]
    public void GetAllResistances_Should_Return_Empty_When_No_Resistances()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        var resistances = TraitApplicator.GetAllResistances(entity);

        // Assert
        resistances.Should().BeEmpty();
    }

    #endregion

    #region Debug Traits Tests

    [Fact]
    public void DebugTraits_Should_Return_Message_When_No_Traits()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        var debug = TraitApplicator.DebugTraits(entity);

        // Assert
        debug.Should().Be("No traits");
    }

    [Fact]
    public void DebugTraits_Should_Format_Number_Traits()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Traits["power"] = new TraitValue(42.5, TraitType.Number);

        // Act
        var debug = TraitApplicator.DebugTraits(entity);

        // Assert
        debug.Should().Contain("power: 42.50");
    }

    [Fact]
    public void DebugTraits_Should_Format_String_Traits()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Traits["name"] = new TraitValue("Hero", TraitType.String);

        // Act
        var debug = TraitApplicator.DebugTraits(entity);

        // Assert
        debug.Should().Contain("name: \"Hero\"");
    }

    [Fact]
    public void DebugTraits_Should_Format_Boolean_Traits()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Traits["active"] = new TraitValue(true, TraitType.Boolean);

        // Act
        var debug = TraitApplicator.DebugTraits(entity);

        // Assert
        debug.Should().Contain("active: True");
    }

    [Fact]
    public void DebugTraits_Should_Format_Multiple_Traits()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Traits["level"] = new TraitValue(10, TraitType.Number);
        entity.Traits["title"] = new TraitValue("Champion", TraitType.String);
        entity.Traits["isElite"] = new TraitValue(true, TraitType.Boolean);

        // Act
        var debug = TraitApplicator.DebugTraits(entity);

        // Assert
        debug.Should().Contain("level:");
        debug.Should().Contain("title:");
        debug.Should().Contain("isElite:");
    }

    #endregion
}
