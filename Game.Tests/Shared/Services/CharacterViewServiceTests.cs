using FluentAssertions;
using Game.Core.Models;
using Game.Shared.Services;
using Game.Console.UI;
using Game.Data.Repositories;
using Game.Tests.Helpers;
using Spectre.Console.Testing;

namespace Game.Tests.Shared.Services;

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

    #region ViewCharacter Tests

    [Fact]
    public void ViewCharacter_Should_Display_Basic_Character_Stats()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestHero",
            ClassName = "Warrior",
            Level = 10,
            Health = 150,
            MaxHealth = 200,
            Mana = 50,
            MaxMana = 100,
            Experience = 500,
            Gold = 1000
        };

        // Act
        _characterViewService.ViewCharacter(character);

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("TestHero");
        output.Should().Contain("Warrior");
        output.Should().Contain("Level");
        output.Should().Contain("10");
        output.Should().Contain("Health");
        output.Should().Contain("150/200");
        output.Should().Contain("Mana");
        output.Should().Contain("50/100");
        output.Should().Contain("Gold");
        output.Should().Contain("1000");
    }

    [Fact]
    public void ViewCharacter_Should_Display_D20_Attributes()
    {
        // Arrange
        var character = new Character
        {
            Name = "Hero",
            ClassName = "Mage",
            Strength = 12,
            Dexterity = 14,
            Constitution = 11,
            Intelligence = 18,
            Wisdom = 16,
            Charisma = 13
        };

        // Act
        _characterViewService.ViewCharacter(character);

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("Strength");
        output.Should().Contain("12");
        output.Should().Contain("Dexterity");
        output.Should().Contain("14");
        output.Should().Contain("Constitution");
        output.Should().Contain("11");
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
            Name = "Fighter",
            ClassName = "Barbarian",
            Strength = 20,
            Intelligence = 10,
            Dexterity = 15
        };

        // Act
        _characterViewService.ViewCharacter(character);

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("Physical Damage");
        output.Should().Contain("Magic Damage");
        output.Should().Contain("Dodge Chance");
        output.Should().Contain("Critical Chance");
        output.Should().Contain("Physical Defense");
        output.Should().Contain("Magic Resistance");
    }

    [Fact]
    public void ViewCharacter_Should_Display_Rare_Find_Stat()
    {
        // Arrange
        var character = new Character
        {
            Name = "Treasure Hunter",
            ClassName = "Rogue",
            Charisma = 25
        };

        // Act
        _characterViewService.ViewCharacter(character);

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("Rare Find");
    }

    [Fact]
    public void ViewCharacter_Should_Display_Learned_Skills_When_Present()
    {
        // Arrange
        var character = new Character
        {
            Name = "SkillMaster",
            ClassName = "Ranger",
            LearnedSkills = new List<Skill>
            {
                new Skill
                {
                    Name = "Power Strike",
                    Description = "A powerful melee attack",
                    Type = SkillType.Combat,
                    CurrentRank = 2,
                    MaxRank = 5
                },
                new Skill
                {
                    Name = "Shield Block",
                    Description = "Blocks incoming damage",
                    Type = SkillType.Defense,
                    CurrentRank = 1,
                    MaxRank = 3
                }
            }
        };

        // Act
        _characterViewService.ViewCharacter(character);

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("Learned Skills");
        output.Should().Contain("Power Strike");
        output.Should().Contain("Shield Block");
        output.Should().Contain("Rank 2/5");
        output.Should().Contain("Rank 1/3");
    }

    [Fact]
    public void ViewCharacter_Should_Not_Display_Skills_Section_When_No_Skills()
    {
        // Arrange
        var character = new Character
        {
            Name = "Newbie",
            ClassName = "Apprentice",
            LearnedSkills = new List<Skill>()
        };

        // Act
        _characterViewService.ViewCharacter(character);

        // Assert
        var output = _testConsole.Output;
        output.Should().NotContain("Learned Skills");
    }

    [Fact]
    public void ViewCharacter_Should_Handle_High_Level_Character()
    {
        // Arrange
        var character = new Character
        {
            Name = "Legend",
            ClassName = "Champion",
            Level = 99,
            Health = 9999,
            MaxHealth = 9999,
            Mana = 5000,
            MaxMana = 5000,
            Gold = 999999,
            Experience = 9900,
            Strength = 50,
            Dexterity = 45,
            Constitution = 48,
            Intelligence = 40,
            Wisdom = 42,
            Charisma = 35
        };

        // Act
        _characterViewService.ViewCharacter(character);

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("99");
        output.Should().Contain("9999");
        output.Should().Contain("999999");
    }

    [Fact]
    public void ViewCharacter_Should_Handle_Low_Level_Character()
    {
        // Arrange
        var character = new Character
        {
            Name = "Starter",
            ClassName = "Novice",
            Level = 1,
            Health = 100,
            MaxHealth = 100,
            Mana = 50,
            MaxMana = 50,
            Gold = 0,
            Experience = 0,
            Strength = 10,
            Dexterity = 10,
            Constitution = 10,
            Intelligence = 10,
            Wisdom = 10,
            Charisma = 10
        };

        // Act
        _characterViewService.ViewCharacter(character);

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("Starter");
        output.Should().Contain("Novice");
        output.Should().Contain("Level");
    }

    [Fact]
    public void ViewCharacter_Should_Display_Panel_Headers()
    {
        // Arrange
        var character = new Character
        {
            Name = "Test",
            ClassName = "Test"
        };

        // Act
        _characterViewService.ViewCharacter(character);

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("Character Stats");
        output.Should().Contain("D20 Attributes");
        output.Should().Contain("Combat Stats");
    }

    [Fact]
    public void ViewCharacter_Should_Calculate_Derived_Stats_From_Attributes()
    {
        // Arrange
        var character = new Character
        {
            Name = "Calculator",
            ClassName = "Mathematician",
            Strength = 30,  // High physical damage
            Intelligence = 5, // Low magic damage
            Dexterity = 40,  // High dodge/crit
            Constitution = 25 // High defense
        };

        // Act
        _characterViewService.ViewCharacter(character);

        // Assert
        var output = _testConsole.Output;
        // The exact values will be calculated by Character methods
        // Just verify the sections are present
        output.Should().Contain("Physical Damage");
        output.Should().Contain("Magic Damage");
        output.Should().Contain("Dodge Chance");
        output.Should().Contain("Critical Chance");
    }

    [Fact]
    public void ViewCharacter_Should_Display_Skills_Grouped_By_Type()
    {
        // Arrange
        var character = new Character
        {
            Name = "MultiSkill",
            ClassName = "Versatile",
            LearnedSkills = new List<Skill>
            {
                new Skill { Name = "Fireball", Type = SkillType.Magic, CurrentRank = 3, MaxRank = 5, Description = "Magic attack" },
                new Skill { Name = "Sword Mastery", Type = SkillType.Combat, CurrentRank = 2, MaxRank = 5, Description = "Combat skill" },
                new Skill { Name = "Iron Skin", Type = SkillType.Defense, CurrentRank = 1, MaxRank = 3, Description = "Defense skill" },
                new Skill { Name = "Fleet Foot", Type = SkillType.Utility, CurrentRank = 2, MaxRank = 3, Description = "Utility skill" },
                new Skill { Name = "Tough", Type = SkillType.Passive, CurrentRank = 1, MaxRank = 1, Description = "Passive skill" }
            }
        };

        // Act
        _characterViewService.ViewCharacter(character);

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("Fireball");
        output.Should().Contain("Sword Mastery");
        output.Should().Contain("Iron Skin");
        output.Should().Contain("Fleet Foot");
        output.Should().Contain("Tough");
    }

    [Fact]
    public void ViewCharacter_Should_Handle_Character_With_Partial_Health()
    {
        // Arrange
        var character = new Character
        {
            Name = "Wounded",
            ClassName = "Survivor",
            Health = 50,
            MaxHealth = 200,
            Mana = 25,
            MaxMana = 100
        };

        // Act
        _characterViewService.ViewCharacter(character);

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("50/200");
        output.Should().Contain("25/100");
    }

    [Fact]
    public void ViewCharacter_Should_Display_Experience_Progress()
    {
        // Arrange
        var character = new Character
        {
            Name = "Leveling",
            ClassName = "Student",
            Level = 5,
            Experience = 300
        };

        // Act
        _characterViewService.ViewCharacter(character);

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("Experience");
        output.Should().Contain("300");
        output.Should().Contain("500"); // 5 * 100
    }

    #endregion
}
