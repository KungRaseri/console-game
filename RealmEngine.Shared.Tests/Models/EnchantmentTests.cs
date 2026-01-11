using FluentAssertions;
using RealmEngine.Shared.Models;

namespace RealmEngine.Shared.Tests.Models;

[Trait("Category", "Unit")]
/// <summary>
/// Comprehensive tests for Enchantment model.
/// Target: 91.6% -> 100% coverage.
/// </summary>
public class EnchantmentTests
{
    #region Initialization Tests

    [Fact]
    public void Enchantment_Should_Initialize_With_Default_Values()
    {
        // Act
        var enchantment = new Enchantment();

        // Assert
        enchantment.Id.Should().NotBeEmpty();
        enchantment.Name.Should().BeEmpty();
        enchantment.Description.Should().BeEmpty();
        enchantment.Rarity.Should().Be(EnchantmentRarity.Minor);
        enchantment.Level.Should().Be(1);
        enchantment.Traits.Should().BeEmpty();
        enchantment.SpecialEffect.Should().BeNull();
    }

    [Fact]
    public void Enchantment_Should_Generate_Unique_Ids()
    {
        // Act
        var enchantment1 = new Enchantment();
        var enchantment2 = new Enchantment();

        // Assert
        enchantment1.Id.Should().NotBe(enchantment2.Id);
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void Enchantment_Should_Allow_Name_Assignment()
    {
        // Arrange
        var enchantment = new Enchantment();

        // Act
        enchantment.Name = "Flaming";

        // Assert
        enchantment.Name.Should().Be("Flaming");
    }

    [Fact]
    public void Enchantment_Should_Allow_Description_Assignment()
    {
        // Arrange
        var enchantment = new Enchantment();

        // Act
        enchantment.Description = "Adds fire damage";

        // Assert
        enchantment.Description.Should().Be("Adds fire damage");
    }

    [Fact]
    public void Enchantment_Should_Allow_Level_Assignment()
    {
        // Arrange
        var enchantment = new Enchantment();

        // Act
        enchantment.Level = 5;

        // Assert
        enchantment.Level.Should().Be(5);
    }

    [Fact]
    public void Enchantment_Should_Allow_SpecialEffect_Assignment()
    {
        // Arrange
        var enchantment = new Enchantment();

        // Act
        enchantment.SpecialEffect = "FireAura";

        // Assert
        enchantment.SpecialEffect.Should().Be("FireAura");
    }

    #endregion

    #region Rarity Tests

    [Theory]
    [InlineData(EnchantmentRarity.Minor)]
    [InlineData(EnchantmentRarity.Lesser)]
    [InlineData(EnchantmentRarity.Greater)]
    [InlineData(EnchantmentRarity.Superior)]
    [InlineData(EnchantmentRarity.Legendary)]
    public void Enchantment_Should_Support_All_Rarities(EnchantmentRarity rarity)
    {
        // Arrange
        var enchantment = new Enchantment();

        // Act
        enchantment.Rarity = rarity;

        // Assert
        enchantment.Rarity.Should().Be(rarity);
    }

    [Fact]
    public void EnchantmentRarity_Should_Have_Five_Tiers()
    {
        // Assert
        var rarities = Enum.GetValues<EnchantmentRarity>();
        rarities.Should().HaveCount(5);
        rarities.Should().Contain(new[]
        {
            EnchantmentRarity.Minor,
            EnchantmentRarity.Lesser,
            EnchantmentRarity.Greater,
            EnchantmentRarity.Superior,
            EnchantmentRarity.Legendary
        });
    }

    #endregion

    #region Attribute Bonus Tests

    [Fact]
    public void Enchantment_Should_Allow_Trait_Assignment()
    {
        // Arrange
        var enchantment = new Enchantment();

        // Act
        enchantment.Traits = new Dictionary<string, TraitValue>
        {
            { "Strength", new TraitValue(5, TraitType.Number) },
            { "FireDamage", new TraitValue(10, TraitType.Number) }
        };

        // Assert
        enchantment.Traits.Should().HaveCount(2);
        enchantment.Traits["Strength"].AsDouble().Should().Be(5);
        enchantment.Traits["FireDamage"].AsDouble().Should().Be(10);
    }

    [Fact]
    public void Enchantment_Should_Support_Multiple_Trait_Types()
    {
        // Arrange
        var enchantment = new Enchantment();

        // Act
        enchantment.Traits = new Dictionary<string, TraitValue>
        {
            { "Strength", new TraitValue(3, TraitType.Number) },
            { "Dexterity", new TraitValue(2, TraitType.Number) },
            { "FireResistance", new TraitValue(15, TraitType.Number) }
        };

        // Assert
        enchantment.Traits.Should().HaveCount(3);
        enchantment.Traits["Strength"].AsDouble().Should().Be(3);
        enchantment.Traits["Dexterity"].AsDouble().Should().Be(2);
        enchantment.Traits["FireResistance"].AsDouble().Should().Be(15);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Enchantment_Should_Create_Complete_Flaming_Enchantment()
    {
        // Arrange & Act
        var enchantment = new Enchantment
        {
            Name = "Flaming",
            Description = "Deals additional fire damage",
            Rarity = EnchantmentRarity.Greater,
            Level = 3,
            Traits = new Dictionary<string, TraitValue>
            {
                { "Strength", new TraitValue(2, TraitType.Number) },
                { "Intelligence", new TraitValue(3, TraitType.Number) }
            },
            SpecialEffect = "FireDamage"
        };

        // Assert
        enchantment.Name.Should().Be("Flaming");
        enchantment.Description.Should().Be("Deals additional fire damage");
        enchantment.Rarity.Should().Be(EnchantmentRarity.Greater);
        enchantment.Level.Should().Be(3);
        enchantment.Traits["Strength"].AsDouble().Should().Be(2);
        enchantment.Traits["Intelligence"].AsDouble().Should().Be(3);
        enchantment.SpecialEffect.Should().Be("FireDamage");
    }

    [Fact]
    public void Enchantment_Should_Create_Complete_Protection_Enchantment()
    {
        // Arrange & Act
        var enchantment = new Enchantment
        {
            Name = "Protection",
            Description = "Increases defensive capabilities",
            Rarity = EnchantmentRarity.Superior,
            Level = 4,
            Traits = new Dictionary<string, TraitValue>
            {
                { "Constitution", new TraitValue(5, TraitType.Number) },
                { "Wisdom", new TraitValue(2, TraitType.Number) }
            },
            SpecialEffect = "Shield"
        };

        // Assert
        enchantment.Name.Should().Be("Protection");
        enchantment.Rarity.Should().Be(EnchantmentRarity.Superior);
        enchantment.Level.Should().Be(4);
        enchantment.Traits["Constitution"].AsDouble().Should().Be(5);
        enchantment.Traits["Wisdom"].AsDouble().Should().Be(2);
    }

    [Fact]
    public void Enchantment_Should_Create_Legendary_Enchantment()
    {
        // Arrange & Act
        var enchantment = new Enchantment
        {
            Name = "Divine Power",
            Description = "Grants immense power",
            Rarity = EnchantmentRarity.Legendary,
            Level = 10,
            Traits = new Dictionary<string, TraitValue>
            {
                { "Strength", new TraitValue(10, TraitType.Number) },
                { "Dexterity", new TraitValue(10, TraitType.Number) },
                { "Constitution", new TraitValue(10, TraitType.Number) },
                { "Intelligence", new TraitValue(10, TraitType.Number) },
                { "Wisdom", new TraitValue(10, TraitType.Number) },
                { "Charisma", new TraitValue(10, TraitType.Number) }
            },
            SpecialEffect = "GlowingAura"
        };

        // Assert
        enchantment.Rarity.Should().Be(EnchantmentRarity.Legendary);
        enchantment.Level.Should().Be(10);
        enchantment.Traits["Strength"].AsDouble().Should().Be(10);
        enchantment.Traits["Dexterity"].AsDouble().Should().Be(10);
        enchantment.Traits["Constitution"].AsDouble().Should().Be(10);
        enchantment.Traits["Intelligence"].AsDouble().Should().Be(10);
        enchantment.Traits["Wisdom"].AsDouble().Should().Be(10);
        enchantment.Traits["Charisma"].AsDouble().Should().Be(10);
    }

    [Fact]
    public void Enchantment_Should_Support_Zero_Bonuses()
    {
        // Arrange & Act
        var enchantment = new Enchantment
        {
            Name = "Placeholder",
            Description = "No bonuses",
            Rarity = EnchantmentRarity.Minor
        };

        // Assert
        enchantment.Traits.Should().BeEmpty();
    }

    [Fact]
    public void Enchantment_Should_Support_High_Level_Values()
    {
        // Arrange & Act
        var enchantment = new Enchantment { Level = 100 };

        // Assert
        enchantment.Level.Should().Be(100);
    }

    [Fact]
    public void Enchantment_Should_Allow_Empty_Special_Effect()
    {
        // Arrange & Act
        var enchantment = new Enchantment
        {
            Name = "Simple Bonus",
            SpecialEffect = null
        };

        // Assert
        enchantment.SpecialEffect.Should().BeNull();
    }

    #endregion
}
