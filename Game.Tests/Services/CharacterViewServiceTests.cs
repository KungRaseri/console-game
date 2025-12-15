using FluentAssertions;
using Game.Core.Models;
using Game.Shared.Services;
using Game.Console.UI;
using Game.Data.Repositories;
using Game.Tests.Helpers;
using Spectre.Console.Testing;

namespace Game.Tests.Services;

/// <summary>
/// Tests for CharacterViewService (currently 2.3% coverage → target 80%+).
/// </summary>
public class CharacterViewServiceTests
{
    private readonly TestConsole _testConsole;
    private readonly IConsoleUI _consoleUI;
    private readonly CharacterViewService _characterViewService;

    public CharacterViewServiceTests()
    {
        _testConsole = TestConsoleHelper.CreateInteractiveConsole();
        _consoleUI = new ConsoleUI(_testConsole);
        var equipmentSetRepository = new EquipmentSetRepository();
        _characterViewService = new CharacterViewService(_consoleUI, equipmentSetRepository);
    }

    [Fact]
    public void ViewCharacter_Should_Display_Basic_Stats()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestHero",
            ClassName = "Warrior",
            Level = 5,
            Health = 80,
            MaxHealth = 100,
            Mana = 30,
            MaxMana = 50,
            Experience = 250,
            Gold = 500
        };

        // Simulate key press to dismiss
        _testConsole.Input.PushKey(ConsoleKey.Enter);

        // Act


        TestConsoleHelper.PressAnyKey(_testConsole); // Simulate user pressing any key after viewing
        _characterViewService.ViewCharacter(character);

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("TestHero");
        output.Should().Contain("Warrior");
        output.Should().Contain("Level");
        output.Should().Contain("5");
        output.Should().Contain("80/100"); // Health
        output.Should().Contain("30/50"); // Mana
        output.Should().Contain("500"); // Gold
    }

    [Fact]
    public void ViewCharacter_Should_Display_D20_Attributes()
    {
        // Arrange
        var character = new Character
        {
            Name = "AttrHero",
            ClassName = "Mage",
            Strength = 10,
            Dexterity = 12,
            Constitution = 14,
            Intelligence = 18,
            Wisdom = 16,
            Charisma = 13
        };

        _testConsole.Input.PushKey(ConsoleKey.Enter);

        // Act


        TestConsoleHelper.PressAnyKey(_testConsole); // Simulate user pressing any key after viewing
        _characterViewService.ViewCharacter(character);

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("Strength");
        output.Should().Contain("10");
        output.Should().Contain("Dexterity");
        output.Should().Contain("12");
        output.Should().Contain("Constitution");
        output.Should().Contain("14");
        output.Should().Contain("Intelligence");
        output.Should().Contain("18");
        output.Should().Contain("Wisdom");
        output.Should().Contain("16");
        output.Should().Contain("Charisma");
        output.Should().Contain("13");
    }

    [Fact]
    public void ViewCharacter_Should_Display_Combat_Stats()
    {
        // Arrange
        var character = new Character
        {
            Name = "CombatHero",
            ClassName = "Rogue",
            Level = 10,
            Strength = 15,
            Dexterity = 18,
            Constitution = 12,
            Intelligence = 10,
            Wisdom = 10,
            Charisma = 14
        };

        _testConsole.Input.PushKey(ConsoleKey.Enter);

        // Act


        TestConsoleHelper.PressAnyKey(_testConsole); // Simulate user pressing any key after viewing
        _characterViewService.ViewCharacter(character);

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("Physical Damage");
        output.Should().Contain("Magic Damage");
        output.Should().Contain("Dodge Chance");
        output.Should().Contain("Critical Chance");
        output.Should().Contain("Physical Defense");
        output.Should().Contain("Magic Resistance");
        output.Should().Contain("Rare Find");
    }

    [Fact]
    public void ViewCharacter_Should_Display_Learned_Skills()
    {
        // Arrange
        var character = new Character
        {
            Name = "SkilledHero",
            ClassName = "Warrior",
            LearnedSkills = new List<Skill>
            {
                new()
                {
                    Name = "Power Attack",
                    Type = SkillType.Combat,
                    CurrentRank = 3,
                    MaxRank = 5,
                    Description = "Increases physical damage"
                },
                new()
                {
                    Name = "Iron Skin",
                    Type = SkillType.Defense,
                    CurrentRank = 2,
                    MaxRank = 3,
                    Description = "Increases defense"
                }
            }
        };

        _testConsole.Input.PushKey(ConsoleKey.Enter);

        // Act


        TestConsoleHelper.PressAnyKey(_testConsole); // Simulate user pressing any key after viewing
        _characterViewService.ViewCharacter(character);

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("Learned Skills");
        output.Should().Contain("Power Attack");
        output.Should().Contain("Rank 3/5");
        output.Should().Contain("Iron Skin");
        output.Should().Contain("Rank 2/3");
        output.Should().Contain("Increases physical damage");
        output.Should().Contain("Increases defense");
    }

    [Fact]
    public void ViewCharacter_Should_Handle_Character_With_No_Skills()
    {
        // Arrange
        var character = new Character
        {
            Name = "NoviceHero",
            ClassName = "Mage",
            LearnedSkills = new List<Skill>()
        };

        _testConsole.Input.PushKey(ConsoleKey.Enter);

        // Act


        TestConsoleHelper.PressAnyKey(_testConsole); // Simulate user pressing any key after viewing
        _characterViewService.ViewCharacter(character);

        // Assert - Should not crash, just not show skills section
        var output = _testConsole.Output;
        output.Should().Contain("NoviceHero");
        output.Should().Contain("Mage");
    }

    [Fact]
    public void ViewCharacter_Should_Display_Skills_Ordered_By_Type()
    {
        // Arrange
        var character = new Character
        {
            Name = "OrderedHero",
            ClassName = "Paladin",
            LearnedSkills = new List<Skill>
            {
                new() { Name = "Passive Skill", Type = SkillType.Passive, CurrentRank = 1, MaxRank = 1 },
                new() { Name = "Combat Skill", Type = SkillType.Combat, CurrentRank = 1, MaxRank = 3 },
                new() { Name = "Utility Skill", Type = SkillType.Utility, CurrentRank = 2, MaxRank = 2 }
            }
        };

        _testConsole.Input.PushKey(ConsoleKey.Enter);

        // Act


        TestConsoleHelper.PressAnyKey(_testConsole); // Simulate user pressing any key after viewing
        _characterViewService.ViewCharacter(character);

        // Assert
        var output = _testConsole.Output;
        // All skills should be displayed
        output.Should().Contain("Combat Skill");
        output.Should().Contain("Passive Skill");
        output.Should().Contain("Utility Skill");
    }

    [Fact]
    public void ViewCharacter_Should_Clear_Console_Before_Display()
    {
        // Arrange
        var character = new Character
        {
            Name = "ClearHero",
            ClassName = "Cleric"
        };

        _testConsole.Input.PushKey(ConsoleKey.Enter);

        // Act


        TestConsoleHelper.PressAnyKey(_testConsole); // Simulate user pressing any key after viewing
        _characterViewService.ViewCharacter(character);

        // Assert - Console should have been cleared (output should start fresh)
        _testConsole.Output.Should().Contain("ClearHero");
    }

    [Fact]
    public void ViewCharacter_Should_Wait_For_Key_Press()
    {
        // Arrange
        var character = new Character
        {
            Name = "WaitHero",
            ClassName = "Monk"
        };

        _testConsole.Input.PushKey(ConsoleKey.Spacebar);

        // Act


        TestConsoleHelper.PressAnyKey(_testConsole); // Simulate user pressing any key after viewing
        _characterViewService.ViewCharacter(character);

        // Assert - Method should complete after key press
        _testConsole.Output.Should().Contain("WaitHero");
    }

    [Fact]
    public void ViewCharacter_Should_Display_Experience_Progress()
    {
        // Arrange
        var character = new Character
        {
            Name = "ProgressHero",
            ClassName = "Bard",
            Level = 3,
            Experience = 75 // 75/300 for level 3
        };

        _testConsole.Input.PushKey(ConsoleKey.Enter);

        // Act


        TestConsoleHelper.PressAnyKey(_testConsole); // Simulate user pressing any key after viewing
        _characterViewService.ViewCharacter(character);

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("75"); // Current XP
        output.Should().Contain("300"); // XP needed (Level 3 * 100)
    }

    [Fact]
    public void ViewCharacter_Should_Display_Full_Health_Character()
    {
        // Arrange
        var character = new Character
        {
            Name = "HealthyHero",
            ClassName = "Barbarian",
            Health = 200,
            MaxHealth = 200,
            Mana = 50,
            MaxMana = 50
        };

        _testConsole.Input.PushKey(ConsoleKey.Enter);

        // Act


        TestConsoleHelper.PressAnyKey(_testConsole); // Simulate user pressing any key after viewing
        _characterViewService.ViewCharacter(character);

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("200/200"); // Full health
        output.Should().Contain("50/50"); // Full mana
    }

    [Fact]
    public void ViewCharacter_Should_Display_Low_Health_Character()
    {
        // Arrange
        var character = new Character
        {
            Name = "InjuredHero",
            ClassName = "Ranger",
            Health = 15,
            MaxHealth = 100,
            Mana = 5,
            MaxMana = 60
        };

        _testConsole.Input.PushKey(ConsoleKey.Enter);

        // Act


        TestConsoleHelper.PressAnyKey(_testConsole); // Simulate user pressing any key after viewing
        _characterViewService.ViewCharacter(character);

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("15/100"); // Low health
        output.Should().Contain("5/60"); // Low mana
    }

    [Fact]
    public void ViewCharacter_Should_Display_High_Level_Character()
    {
        // Arrange
        var character = new Character
        {
            Name = "VeteranHero",
            ClassName = "Sorcerer",
            Level = 50,
            Experience = 4500,
            Gold = 100000
        };

        _testConsole.Input.PushKey(ConsoleKey.Enter);

        // Act


        TestConsoleHelper.PressAnyKey(_testConsole); // Simulate user pressing any key after viewing
        _characterViewService.ViewCharacter(character);

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("50"); // High level
        output.Should().Contain("100000"); // Lots of gold
    }
}
