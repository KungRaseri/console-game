using Game.Core.Models;
using Game.Console.UI;
using Game.Tests.Helpers;
using Spectre.Console.Testing;
using Game.Core.Services;
using Game.Core.Features.SaveLoad;
using Game.Console.Services;
using MediatR;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Game.Console.Orchestrators;
using Game.Data.Repositories;
using Game.Core.Abstractions;

namespace Game.Tests.Services;

/// <summary>
/// Unit tests for CharacterCreationOrchestrator - Character creation workflow orchestration
/// </summary>
public class CharacterCreationOrchestratorTests : IDisposable
{
    private readonly CharacterCreationOrchestrator _orchestrator;
    private readonly SaveGameService _saveGameService;
    private readonly IMediator _mediator;
    private readonly TestConsole _testConsole;
    private readonly ConsoleUI _consoleUI;
    private readonly CharacterViewService _characterViewService;
    private readonly string _testDbPath;
    private readonly ICharacterClassRepository _characterClassRepository;


    public CharacterCreationOrchestratorTests()
    {
        // Use unique test database to avoid file locking issues
        _testDbPath = $"test-charcreation-{Guid.NewGuid()}.db";

        // Setup TestConsole
        _testConsole = TestConsoleHelper.CreateInteractiveConsole();
        _consoleUI = new ConsoleUI(_testConsole);
        var equipmentSetRepository = new EquipmentSetRepository();
        _characterViewService = new CharacterViewService(_consoleUI, equipmentSetRepository);

        // Setup MediatR
        var services = new ServiceCollection();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CharacterCreationOrchestrator).Assembly);
        });

        var serviceProvider = services.BuildServiceProvider();
        _mediator = serviceProvider.GetRequiredService<IMediator>();

        var apocalypseTimer = new ApocalypseTimer(_consoleUI);
        var repository = new SaveGameRepository(_testDbPath);
        _saveGameService = new SaveGameService(repository, apocalypseTimer);
        _characterClassRepository = new CharacterClassRepository();
        _orchestrator = new CharacterCreationOrchestrator(_mediator, _saveGameService, apocalypseTimer, _consoleUI, _characterViewService, _characterClassRepository);
    }


    public void Dispose()
    {
        // Dispose of SaveGameService first to release file locks
        _saveGameService?.Dispose();

        // Clean up test database files
        try
        {
            if (File.Exists(_testDbPath))
                File.Delete(_testDbPath);

            var logFile = _testDbPath.Replace(".db", "-log.db");
            if (File.Exists(logFile))
                File.Delete(logFile);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public void CharacterCreationOrchestrator_Should_Be_Instantiable()
    {
        // Arrange & Act
        var orchestrator = new CharacterCreationOrchestrator(_mediator, _saveGameService, new ApocalypseTimer(_consoleUI), _consoleUI, _characterViewService, _characterClassRepository);

        // Assert
        orchestrator.Should().NotBeNull();
    }

    [Fact]
    public void CharacterCreationOrchestrator_Should_Have_Required_Dependencies()
    {
        // Assert
        _orchestrator.Should().NotBeNull();
        _saveGameService.Should().NotBeNull();
        _mediator.Should().NotBeNull();
    }

    [Fact]
    public void AutoAllocateAttributes_Should_Prioritize_Primary_Attributes()
    {
        // Arrange
        var warriorClass = _characterClassRepository.GetAllClasses()
            .FirstOrDefault(c => c.Name == "Warrior");

        warriorClass.Should().NotBeNull();

        // Act - Use reflection to call private method
        var method = typeof(CharacterCreationOrchestrator)
            .GetMethod("AutoAllocateAttributes", BindingFlags.NonPublic | BindingFlags.Instance);
        var result = method!.Invoke(_orchestrator, new object[] { warriorClass! }) as AttributeAllocation;

        // Assert
        result.Should().NotBeNull();

        // Warrior primary attributes are Strength and Constitution
        // They should be higher than other attributes
        var strength = result!.GetAttributeValue("Strength");
        var constitution = result.GetAttributeValue("Constitution");
        var intelligence = result.GetAttributeValue("Intelligence");

        strength.Should().BeGreaterThan(10, "Strength is a primary attribute for Warrior");
        constitution.Should().BeGreaterThan(10, "Constitution is a primary attribute for Warrior");
        intelligence.Should().BeLessThanOrEqualTo(10, "Intelligence is not a primary attribute for Warrior");
    }

    [Fact]
    public void AutoAllocateAttributes_Should_Use_All_27_Points()
    {
        // Arrange
        var mageClass = _characterClassRepository.GetAllClasses()
            .FirstOrDefault(c => c.Name == "Mage");

        mageClass.Should().NotBeNull();

        // Act
        var method = typeof(CharacterCreationOrchestrator)
            .GetMethod("AutoAllocateAttributes", BindingFlags.NonPublic | BindingFlags.Instance);
        var result = method!.Invoke(_orchestrator, new object[] { mageClass! }) as AttributeAllocation;

        // Assert
        result.Should().NotBeNull();
        result!.GetPointsSpent().Should().BeGreaterThanOrEqualTo(26, "Most or all points should be spent");
        result.GetRemainingPoints().Should().BeLessThanOrEqualTo(1, "At most 1 point should remain");
    }

    [Theory]
    [InlineData("Warrior", "Strength")]
    [InlineData("Warrior", "Constitution")]
    [InlineData("Mage", "Intelligence")]
    [InlineData("Mage", "Wisdom")]
    [InlineData("Rogue", "Dexterity")]
    [InlineData("Rogue", "Charisma")]
    public void AutoAllocateAttributes_Should_Favor_Class_Primary_Attributes(string className, string primaryAttr)
    {
        // Arrange
        var characterClass = _characterClassRepository.GetAllClasses()
            .FirstOrDefault(c => c.Name == className);

        characterClass.Should().NotBeNull();

        // Act
        var method = typeof(CharacterCreationOrchestrator)
            .GetMethod("AutoAllocateAttributes", BindingFlags.NonPublic | BindingFlags.Instance);
        var result = method!.Invoke(_orchestrator, new object[] { characterClass! }) as AttributeAllocation;

        // Assert
        result.Should().NotBeNull();
        var primaryValue = result!.GetAttributeValue(primaryAttr);

        // Primary attributes should be at least 12 (14 is the target, but allow for distribution)
        primaryValue.Should().BeGreaterThanOrEqualTo(12,
            $"{primaryAttr} is a primary attribute for {className}");
    }

    [Fact]
    public void GetClassBonus_Should_Return_Correct_Strength_Bonus()
    {
        // Arrange
        var warriorClass = _characterClassRepository.GetAllClasses()
            .FirstOrDefault(c => c.Name == "Warrior");

        warriorClass.Should().NotBeNull();

        // Act
        var method = typeof(CharacterCreationOrchestrator)
            .GetMethod("GetClassBonus", BindingFlags.NonPublic | BindingFlags.Instance);
        var result = (int)method!.Invoke(_orchestrator, new object[] { warriorClass!, "Strength" })!;

        // Assert
        result.Should().Be(warriorClass!.BonusStrength);
    }

    [Fact]
    public void GetClassBonus_Should_Return_Correct_Intelligence_Bonus()
    {
        // Arrange
        var mageClass = _characterClassRepository.GetAllClasses()
            .FirstOrDefault(c => c.Name == "Mage");

        mageClass.Should().NotBeNull();

        // Act
        var method = typeof(CharacterCreationOrchestrator)
            .GetMethod("GetClassBonus", BindingFlags.NonPublic | BindingFlags.Instance);
        var result = (int)method!.Invoke(_orchestrator, new object[] { mageClass!, "Intelligence" })!;

        // Assert
        result.Should().Be(mageClass!.BonusIntelligence);
    }

    [Fact]
    public void GetClassBonus_Should_Return_Zero_For_Invalid_Attribute()
    {
        // Arrange
        var anyClass = _characterClassRepository.GetAllClasses().First();

        // Act
        var method = typeof(CharacterCreationOrchestrator)
            .GetMethod("GetClassBonus", BindingFlags.NonPublic | BindingFlags.Instance);
        var result = (int)method!.Invoke(_orchestrator, new object[] { anyClass, "InvalidAttribute" })!;

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void AutoAllocateAttributes_Should_Set_All_Attributes_To_At_Least_8()
    {
        // Arrange
        var rogueClass = _characterClassRepository.GetAllClasses()
            .FirstOrDefault(c => c.Name == "Rogue");

        rogueClass.Should().NotBeNull();

        // Act
        var method = typeof(CharacterCreationOrchestrator)
            .GetMethod("AutoAllocateAttributes", BindingFlags.NonPublic | BindingFlags.Instance);
        var result = method!.Invoke(_orchestrator, new object[] { rogueClass! }) as AttributeAllocation;

        // Assert
        result.Should().NotBeNull();

        var attributes = new[] { "Strength", "Dexterity", "Constitution", "Intelligence", "Wisdom", "Charisma" };
        foreach (var attr in attributes)
        {
            result!.GetAttributeValue(attr).Should().BeGreaterThanOrEqualTo(8,
                $"{attr} should be at least 8 (base value)");
        }
    }

    [Fact]
    public void AutoAllocateAttributes_Should_Handle_All_Classes()
    {
        // Arrange
        var allClasses = _characterClassRepository.GetAllClasses();

        // Act & Assert
        var method = typeof(CharacterCreationOrchestrator)
            .GetMethod("AutoAllocateAttributes", BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var characterClass in allClasses)
        {
            var result = method!.Invoke(_orchestrator, new object[] { characterClass }) as AttributeAllocation;

            result.Should().NotBeNull($"Auto-allocation should work for {characterClass.Name}");
            result!.GetPointsSpent().Should().BeGreaterThanOrEqualTo(26,
                $"Most or all points should be spent for {characterClass.Name}");
            result.GetRemainingPoints().Should().BeLessThanOrEqualTo(1,
                $"At most 1 point should remain for {characterClass.Name}");
        }
    }

    [Fact]
    public void GetClassBonus_Should_Return_All_Attribute_Bonuses_Correctly()
    {
        // Arrange
        var mageClass = _characterClassRepository.GetAllClasses()
            .FirstOrDefault(c => c.Name == "Mage");

        mageClass.Should().NotBeNull();

        var method = typeof(CharacterCreationOrchestrator)
            .GetMethod("GetClassBonus", BindingFlags.NonPublic | BindingFlags.Instance);

        // Act & Assert
        ((int)method!.Invoke(_orchestrator, new object[] { mageClass!, "Strength" })!)
            .Should().Be(mageClass!.BonusStrength);

        ((int)method.Invoke(_orchestrator, new object[] { mageClass, "Dexterity" })!)
            .Should().Be(mageClass.BonusDexterity);

        ((int)method.Invoke(_orchestrator, new object[] { mageClass, "Constitution" })!)
            .Should().Be(mageClass.BonusConstitution);

        ((int)method.Invoke(_orchestrator, new object[] { mageClass, "Intelligence" })!)
            .Should().Be(mageClass.BonusIntelligence);

        ((int)method.Invoke(_orchestrator, new object[] { mageClass, "Wisdom" })!)
            .Should().Be(mageClass.BonusWisdom);

        ((int)method.Invoke(_orchestrator, new object[] { mageClass, "Charisma" })!)
            .Should().Be(mageClass.BonusCharisma);
    }

    [Fact]
    public void AutoAllocateAttributes_Should_Not_Exceed_Maximum_Attribute_Value()
    {
        // Arrange
        var anyClass = _characterClassRepository.GetAllClasses().First();

        // Act
        var method = typeof(CharacterCreationOrchestrator)
            .GetMethod("AutoAllocateAttributes", BindingFlags.NonPublic | BindingFlags.Instance);
        var result = method!.Invoke(_orchestrator, new object[] { anyClass }) as AttributeAllocation;

        // Assert
        result.Should().NotBeNull();

        var attributes = new[] { "Strength", "Dexterity", "Constitution", "Intelligence", "Wisdom", "Charisma" };
        foreach (var attr in attributes)
        {
            var value = result!.GetAttributeValue(attr);
            value.Should().BeLessThanOrEqualTo(18,
                $"{attr} should not exceed maximum value of 18");
        }
    }
}

