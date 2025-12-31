using FluentValidation.TestHelper;
using Game.Core.Features.Combat.Commands.FleeFromCombat;
using Game.Shared.Models;

namespace Game.Tests.Features.Combat.Validators;

/// <summary>
/// Tests for FleeFromCombatValidator.
/// </summary>
public class FleeFromCombatValidatorTests
{
    private readonly FleeFromCombatValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Player_Is_Null()
    {
        // Arrange
        var command = new FleeFromCombatCommand
        {
            Player = null!,
            Enemy = new Enemy { Health = 50 }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Player)
            .WithErrorMessage("Player cannot be null");
    }

    [Fact]
    public void Should_Have_Error_When_Player_Health_Is_Zero()
    {
        // Arrange
        var command = new FleeFromCombatCommand
        {
            Player = new Character { Health = 0, MaxHealth = 100 },
            Enemy = new Enemy { Health = 50 }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Player.Health)
            .WithErrorMessage("Player must be alive to flee");
    }

    [Fact]
    public void Should_Have_Error_When_Enemy_Is_Null()
    {
        // Arrange
        var command = new FleeFromCombatCommand
        {
            Player = new Character { Health = 100, MaxHealth = 100 },
            Enemy = null!
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Enemy)
            .WithErrorMessage("Enemy cannot be null");
    }

    [Fact]
    public void Should_Pass_When_Player_Is_Alive_And_Enemy_Exists()
    {
        // Arrange
        var command = new FleeFromCombatCommand
        {
            Player = new Character { Health = 30, MaxHealth = 100 },
            Enemy = new Enemy { Health = 80, MaxHealth = 100 }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_Even_When_Enemy_Is_Dead()
    {
        // Arrange - Player can flee even if enemy is dead (edge case)
        var command = new FleeFromCombatCommand
        {
            Player = new Character { Health = 50, MaxHealth = 100 },
            Enemy = new Enemy { Health = 0, MaxHealth = 100 }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Enemy.Health);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void Should_Pass_When_Player_Health_Is_Positive(int health)
    {
        // Arrange
        var command = new FleeFromCombatCommand
        {
            Player = new Character { Health = health, MaxHealth = 100 },
            Enemy = new Enemy { Health = 50, MaxHealth = 100 }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Player.Health);
    }
}
