using Xunit;
using FluentAssertions;
using System.Text.RegularExpressions;

namespace Game.Tests.Validators;

/// <summary>
/// Tests for JSON Reference System v4.1 syntax validation
/// Reference format: @domain/path/category:item-name[filters]?.property.nested
/// </summary>
public class JsonReferenceValidatorTests
{
    private static readonly Regex BasicReferencePattern = new(@"^@[\w-]+/([\w-]+/)+[\w-]+:[\w-*]+$", RegexOptions.Compiled);
    private static readonly Regex PropertyAccessPattern = new(@"^@[\w-]+/([\w-]+/)+[\w-]+:[\w-]+(\.\w+)+$", RegexOptions.Compiled);
    private static readonly Regex OptionalReferencePattern = new(@"^@[\w-]+/([\w-]+/)+[\w-]+:[\w-]+\?(\.\w+)*$", RegexOptions.Compiled);

    #region Syntax Validation Tests

    [Theory]
    [InlineData("@abilities/active/offensive:basic-attack")]
    [InlineData("@items/weapons/swords:iron-longsword")]
    [InlineData("@npcs/social_classes:merchants")]
    [InlineData("@world/locations/towns:Silverport")]
    [InlineData("@organizations/factions:nobility")]
    [InlineData("@enemies/humanoid:goblin-warrior")]
    public void Should_Match_Valid_Basic_References(string reference)
    {
        // Act & Assert
        BasicReferencePattern.IsMatch(reference).Should().BeTrue($"'{reference}' should be valid");
    }

    [Theory]
    [InlineData("abilities/active:basic-attack")] // Missing @
    [InlineData("@abilities:basic-attack")] // Missing path
    [InlineData("@abilities/active/offensive")] // Missing item
    [InlineData("@abilities/active/:basic-attack")] // Empty category
    [InlineData("@abilities/active/offensive:")] // Empty item name
    public void Should_Reject_Invalid_Basic_References(string reference)
    {
        // Act & Assert
        BasicReferencePattern.IsMatch(reference).Should().BeFalse($"'{reference}' should be invalid");
    }

    [Theory]
    [InlineData("@items/weapons/swords:iron-longsword.damage")]
    [InlineData("@npcs/merchants:blacksmith.name")]
    [InlineData("@abilities/active:fireball.manaCost.base")]
    public void Should_Match_Valid_Property_Access_References(string reference)
    {
        // Act & Assert
        PropertyAccessPattern.IsMatch(reference).Should().BeTrue($"'{reference}' should be valid");
    }

    [Theory]
    [InlineData("@items/weapons/swords:iron-longsword?")]
    [InlineData("@npcs/social_classes:nobles?.description")]
    [InlineData("@abilities/passive:non-existent?")]
    public void Should_Match_Valid_Optional_References(string reference)
    {
        // Act & Assert
        OptionalReferencePattern.IsMatch(reference).Should().BeTrue($"'{reference}' should be valid");
    }

    [Theory]
    [InlineData("@items/weapons/swords:*")]
    [InlineData("@enemies/humanoid:*")]
    [InlineData("@abilities/active/offensive:*")]
    public void Should_Match_Valid_Wildcard_References(string reference)
    {
        // Act & Assert
        BasicReferencePattern.IsMatch(reference).Should().BeTrue($"'{reference}' should be valid wildcard");
    }

    #endregion

    #region Reference Parsing Tests

    [Fact]
    public void Should_Parse_Domain_From_Reference()
    {
        // Arrange
        var reference = "@items/weapons/swords:iron-longsword";

        // Act
        var domain = reference.Split('/')[0].TrimStart('@');

        // Assert
        domain.Should().Be("items");
    }

    [Fact]
    public void Should_Parse_Category_From_Reference()
    {
        // Arrange
        var reference = "@items/weapons/swords:iron-longsword";

        // Act
        var parts = reference.Split(':')[0].Split('/');
        var category = parts[^1];

        // Assert
        category.Should().Be("swords");
    }

    [Fact]
    public void Should_Parse_Item_Name_From_Reference()
    {
        // Arrange
        var reference = "@items/weapons/swords:iron-longsword";

        // Act
        var itemName = reference.Split(':')[1].Split('.')[0].TrimEnd('?');

        // Assert
        itemName.Should().Be("iron-longsword");
    }

    [Fact]
    public void Should_Detect_Optional_Reference()
    {
        // Arrange
        var reference = "@items/weapons/swords:iron-longsword?";

        // Act
        var isOptional = reference.Contains('?');

        // Assert
        isOptional.Should().BeTrue();
    }

    [Fact]
    public void Should_Detect_Wildcard_Reference()
    {
        // Arrange
        var reference = "@enemies/humanoid:*";

        // Act
        var isWildcard = reference.EndsWith(":*");

        // Assert
        isWildcard.Should().BeTrue();
    }

    #endregion

    #region Domain Validation Tests

    [Theory]
    [InlineData("abilities")]
    [InlineData("classes")]
    [InlineData("enemies")]
    [InlineData("items")]
    [InlineData("npcs")]
    [InlineData("quests")]
    [InlineData("world")]
    [InlineData("social")]
    [InlineData("organizations")]
    [InlineData("general")]
    public void Should_Recognize_Valid_Domains(string domain)
    {
        // Arrange
        var validDomains = new[] { "abilities", "classes", "enemies", "items", "npcs", "quests", 
            "world", "social", "organizations", "general" };

        // Act & Assert
        validDomains.Should().Contain(domain);
    }

    [Theory]
    [InlineData("invalid-domain")]
    [InlineData("unknown")]
    [InlineData("test")]
    public void Should_Reject_Invalid_Domains(string domain)
    {
        // Arrange
        var validDomains = new[] { "abilities", "classes", "enemies", "items", "npcs", "quests", 
            "world", "social", "organizations", "general" };

        // Act & Assert
        validDomains.Should().NotContain(domain);
    }

    #endregion

    #region Reference Examples Tests

    [Fact]
    public void Example_Ability_Reference_Should_Be_Valid()
    {
        // Arrange
        var reference = "@abilities/active/offensive:basic-attack";

        // Assert
        BasicReferencePattern.IsMatch(reference).Should().BeTrue();
    }

    [Fact]
    public void Example_Item_Reference_Should_Be_Valid()
    {
        // Arrange
        var reference = "@items/weapons/swords:iron-longsword";

        // Assert
        BasicReferencePattern.IsMatch(reference).Should().BeTrue();
    }

    [Fact]
    public void Example_Enemy_Reference_Should_Be_Valid()
    {
        // Arrange
        var reference = "@enemies/humanoid:goblin-warrior";

        // Assert
        BasicReferencePattern.IsMatch(reference).Should().BeTrue();
    }

    [Fact]
    public void Example_Location_Reference_Should_Be_Valid()
    {
        // Arrange
        var reference = "@world/locations/towns:Silverport";

        // Assert
        BasicReferencePattern.IsMatch(reference).Should().BeTrue();
    }

    [Fact]
    public void Example_Faction_Reference_Should_Be_Valid()
    {
        // Arrange
        var reference = "@organizations/factions:nobility";

        // Assert
        BasicReferencePattern.IsMatch(reference).Should().BeTrue();
    }

    #endregion
}
