using FluentAssertions;
using Game.Core.Generators.Modern;
using Game.Data.Services;
using Game.Shared.Models;
using Xunit;

namespace Game.Core.Tests.Generators;

[Trait("Category", "Generator")]
public class AbilityGeneratorTests
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly AbilityGenerator _generator;

    public AbilityGeneratorTests()
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "Game.Data", "Data", "Json");
        _dataCache = new GameDataCache(basePath);
        _referenceResolver = new ReferenceResolverService(_dataCache);
        _generator = new AbilityGenerator(_dataCache, _referenceResolver);
    }

    [Fact]
    public async Task Should_Generate_Abilities_From_Category()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var abilities = await _generator.GenerateAbilitiesAsync("active", "offensive", 5);

        // Assert
        abilities.Should().NotBeNull();
        abilities.Should().HaveCount(5);
        abilities.Should().AllSatisfy(ability =>
        {
            ability.Name.Should().NotBeNullOrEmpty();
            ability.Id.Should().Contain("active/offensive:");
        });
    }

    [Theory]
    [InlineData("active", "offensive")]
    [InlineData("active", "defensive")]
    [InlineData("active", "support")]
    [InlineData("passive", "offensive")]
    [InlineData("passive", "defensive")]
    public async Task Should_Generate_Abilities_From_Different_Categories(string category, string subcategory)
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var abilities = await _generator.GenerateAbilitiesAsync(category, subcategory, 3);

        // Assert
        abilities.Should().NotBeNull();
        abilities.Should().HaveCountGreaterThan(0);
        abilities.Should().AllSatisfy(ability =>
        {
            ability.Id.Should().Contain($"{category}/{subcategory}:");
        });
    }

    [Fact]
    public async Task Should_Generate_Ability_By_Name()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var ability = await _generator.GenerateAbilityByNameAsync("active", "offensive", "basic-attack");

        // Assert
        ability.Should().NotBeNull();
        ability!.Name.Should().Be("basic-attack");
        ability.DisplayName.Should().Be("Basic Attack");
    }

    [Fact]
    public async Task Should_Return_Null_For_Non_Existent_Ability()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var ability = await _generator.GenerateAbilityByNameAsync("active", "offensive", "NonExistentAbility");

        // Assert
        ability.Should().BeNull();
    }

    [Fact]
    public async Task Should_Generate_Abilities_With_Valid_Properties()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var abilities = await _generator.GenerateAbilitiesAsync("active", "offensive", 10);

        // Assert
        abilities.Should().AllSatisfy(ability =>
        {
            ability.Name.Should().NotBeNullOrEmpty();
            ability.DisplayName.Should().NotBeNullOrEmpty();
            ability.Description.Should().NotBeNullOrEmpty();
            ability.RarityWeight.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public async Task Should_Set_Passive_Flag_For_Passive_Abilities()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var passiveAbilities = await _generator.GenerateAbilitiesAsync("passive", "offensive", 5);
        var activeAbilities = await _generator.GenerateAbilitiesAsync("active", "offensive", 5);

        // Assert
        passiveAbilities.Should().AllSatisfy(a => a.IsPassive.Should().BeTrue());
        activeAbilities.Should().AllSatisfy(a => a.IsPassive.Should().BeFalse());
    }

    [Fact]
    public async Task Should_Parse_Ability_Traits_Correctly()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var ability = await _generator.GenerateAbilityByNameAsync("active", "offensive", "basic-attack");

        // Assert
        ability.Should().NotBeNull();
        ability!.Traits.Should().NotBeNull();
        ability.Traits.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Should_Map_Ability_Types_Correctly()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var offensiveAbilities = await _generator.GenerateAbilitiesAsync("active", "offensive", 5);
        var defensiveAbilities = await _generator.GenerateAbilitiesAsync("active", "defensive", 5);
        var supportAbilities = await _generator.GenerateAbilitiesAsync("active", "support", 5);

        // Assert
        offensiveAbilities.Should().AllSatisfy(a => a.Type.Should().Be(AbilityTypeEnum.Offensive));
        defensiveAbilities.Should().AllSatisfy(a => a.Type.Should().Be(AbilityTypeEnum.Defensive));
        supportAbilities.Should().AllSatisfy(a => a.Type.Should().Be(AbilityTypeEnum.Support));
    }

    [Fact]
    public async Task Should_Handle_Empty_Category_Gracefully()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var abilities = await _generator.GenerateAbilitiesAsync("nonexistent", "category", 5);

        // Assert
        abilities.Should().NotBeNull();
        abilities.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_Generate_Different_Abilities_From_Same_Category()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var abilities = await _generator.GenerateAbilitiesAsync("active", "offensive", 30);

        // Assert
        var uniqueNames = abilities.Select(a => a.Name).Distinct().ToList();
        uniqueNames.Should().HaveCountGreaterThan(1, "should generate variety of abilities");
    }

    [Fact]
    public async Task Should_Set_Proper_Ability_Id_Format()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var abilities = await _generator.GenerateAbilitiesAsync("active", "support", 5);

        // Assert
        abilities.Should().AllSatisfy(ability =>
        {
            ability.Id.Should().MatchRegex(@"^active/support:.+$");
        });
    }

    [Fact]
    public async Task Should_Parse_BaseDamage_Property()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var ability = await _generator.GenerateAbilityByNameAsync("active", "offensive", "basic-attack");

        // Assert
        ability.Should().NotBeNull();
        ability!.BaseDamage.Should().NotBeNullOrEmpty();
        ability.BaseDamage.Should().Contain("d"); // dice notation like "1d6"
    }

    [Fact]
    public async Task Should_Handle_Cooldown_Property()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var abilities = await _generator.GenerateAbilitiesAsync("active", "offensive", 10);

        // Assert
        abilities.Should().AllSatisfy(ability =>
        {
            ability.Cooldown.Should().BeGreaterThanOrEqualTo(0);
        });
    }

    [Fact]
    public async Task Should_Generate_Ultimate_Abilities()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var abilities = await _generator.GenerateAbilitiesAsync("ultimate", "offensive", 3);

        // Assert
        abilities.Should().NotBeNull();
        if (abilities.Any())
        {
            abilities.Should().AllSatisfy(ability =>
            {
                ability.Id.Should().Contain("ultimate/");
            });
        }
    }
}
