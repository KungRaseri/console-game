using FluentAssertions;
using FluentValidation.TestHelper;
using Game.Features.CharacterCreation.Commands;
using Game.Models;
using Xunit;

namespace Game.Tests.Features.CharacterCreation.Commands;

/// <summary>
/// Tests for CreateCharacterValidator.
/// </summary>
public class CreateCharacterValidatorTests
{
    private readonly CreateCharacterValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_PlayerName_Is_Empty()
    {
        // Arrange
        var command = new CreateCharacterCommand
        {
            PlayerName = "",
            ClassName = "Warrior",
            AttributeAllocation = CreateValidAllocation()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PlayerName)
            .WithErrorMessage("Player name is required");
    }

    [Fact]
    public void Should_Have_Error_When_PlayerName_Is_Too_Long()
    {
        // Arrange
        var command = new CreateCharacterCommand
        {
            PlayerName = new string('a', 31), // 31 characters
            ClassName = "Warrior",
            AttributeAllocation = CreateValidAllocation()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PlayerName)
            .WithErrorMessage("Player name must be between 1 and 30 characters");
    }

    [Theory]
    [InlineData("123Invalid")] // Starts with number
    [InlineData("!Invalid")] // Starts with special char
    [InlineData("Invalid@Name")] // Contains special char
    [InlineData("Invalid#123")] // Contains special char
    public void Should_Have_Error_When_PlayerName_Has_Invalid_Characters(string playerName)
    {
        // Arrange
        var command = new CreateCharacterCommand
        {
            PlayerName = playerName,
            ClassName = "Warrior",
            AttributeAllocation = CreateValidAllocation()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PlayerName)
            .WithErrorMessage("Player name must start with a letter and contain only letters, numbers, and spaces");
    }

    [Theory]
    [InlineData("Alice")]
    [InlineData("Bob the Great")]
    [InlineData("Player123")]
    [InlineData("A")]
    [InlineData("ValidName")]
    public void Should_Not_Have_Error_When_PlayerName_Is_Valid(string playerName)
    {
        // Arrange
        var command = new CreateCharacterCommand
        {
            PlayerName = playerName,
            ClassName = "Warrior",
            AttributeAllocation = CreateValidAllocation()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PlayerName);
    }

    [Fact]
    public void Should_Have_Error_When_ClassName_Is_Empty()
    {
        // Arrange
        var command = new CreateCharacterCommand
        {
            PlayerName = "Alice",
            ClassName = "",
            AttributeAllocation = CreateValidAllocation()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClassName)
            .WithErrorMessage("Class name is required");
    }

    [Fact]
    public void Should_Have_Error_When_Points_Not_Fully_Allocated()
    {
        // Arrange - Only allocate 10 points instead of 27
        var command = new CreateCharacterCommand
        {
            PlayerName = "Alice",
            ClassName = "Warrior",
            AttributeAllocation = new AttributeAllocation
            {
                Strength = 13,  // 8→13 = 5 points
                Dexterity = 13, // 8→13 = 5 points
                Constitution = 8,
                Intelligence = 8,
                Wisdom = 8,
                Charisma = 8
                // Total: 10 points (need 27)
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AttributeAllocation)
            .WithErrorMessage("All 27 attribute points must be allocated");
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Points_Allocated()
    {
        // Arrange
        var command = new CreateCharacterCommand
        {
            PlayerName = "Alice",
            ClassName = "Warrior",
            AttributeAllocation = CreateValidAllocation() // 27 points
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AttributeAllocation);
    }

    [Fact]
    public void Should_Pass_When_All_Fields_Are_Valid()
    {
        // Arrange
        var command = new CreateCharacterCommand
        {
            PlayerName = "Alice the Great",
            ClassName = "Warrior",
            AttributeAllocation = CreateValidAllocation()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    private static AttributeAllocation CreateValidAllocation()
    {
        // Point-buy system: Start at 8, costs 1 point per level up to 13, then 2 points per level
        // 8→13 costs 5 points (1 each)
        // 13→15 costs 4 points (2 each)
        // Total budget: 27 points
        return new AttributeAllocation
        {
            Strength = 15,      // 8→15 = 5 + 4 = 9 points
            Dexterity = 14,     // 8→14 = 5 + 2 = 7 points
            Constitution = 13,  // 8→13 = 5 points
            Intelligence = 12,  // 8→12 = 4 points
            Wisdom = 10,        // 8→10 = 2 points
            Charisma = 8        // 8→8 = 0 points
            // Total: 9 + 7 + 5 + 4 + 2 + 0 = 27 points
        };
    }
}
