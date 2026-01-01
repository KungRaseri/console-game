using FluentValidation.TestHelper;
using Game.Core.Features.Combat.Commands.AttackEnemy;
using Game.Shared.Models;

namespace Game.Core.Tests.Features.Combat.Validators;

[Trait("Category", "Feature")]
/// <summary>
/// Tests for AttackEnemyValidator.
/// </summary>
public class AttackEnemyValidatorTests
{
    private readonly AttackEnemyValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Player_Is_Null()
    {
        // Arrange
        var command = new AttackEnemyCommand
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
        var command = new AttackEnemyCommand
        {
            Player = new Character { Health = 0, MaxHealth = 100 },
            Enemy = new Enemy { Health = 50 }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Player.Health)
            .WithErrorMessage("Player must be alive to attack");
    }

    [Fact]
    public void Should_Have_Error_When_Player_Health_Is_Negative()
    {
        // Arrange
        var command = new AttackEnemyCommand
        {
            Player = new Character { Health = -10, MaxHealth = 100 },
            Enemy = new Enemy { Health = 50 }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Player.Health);
    }

    [Fact]
    public void Should_Have_Error_When_Enemy_Is_Null()
    {
        // Arrange
        var command = new AttackEnemyCommand
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
    public void Should_Have_Error_When_Enemy_Health_Is_Zero()
    {
        // Arrange
        var command = new AttackEnemyCommand
        {
            Player = new Character { Health = 100, MaxHealth = 100 },
            Enemy = new Enemy { Health = 0, MaxHealth = 100 }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Enemy.Health)
            .WithErrorMessage("Enemy must be alive to be attacked");
    }

    [Fact]
    public void Should_Have_Error_When_Enemy_Health_Is_Negative()
    {
        // Arrange
        var command = new AttackEnemyCommand
        {
            Player = new Character { Health = 100, MaxHealth = 100 },
            Enemy = new Enemy { Health = -5, MaxHealth = 100 }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Enemy.Health);
    }

    [Fact]
    public void Should_Pass_When_Both_Player_And_Enemy_Are_Alive()
    {
        // Arrange
        var command = new AttackEnemyCommand
        {
            Player = new Character { Health = 50, MaxHealth = 100 },
            Enemy = new Enemy { Health = 30, MaxHealth = 50 }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void Should_Pass_When_Player_Health_Is_Positive(int health)
    {
        // Arrange
        var command = new AttackEnemyCommand
        {
            Player = new Character { Health = health, MaxHealth = 100 },
            Enemy = new Enemy { Health = 50, MaxHealth = 100 }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Player.Health);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(100)]
    public void Should_Pass_When_Enemy_Health_Is_Positive(int health)
    {
        // Arrange
        var command = new AttackEnemyCommand
        {
            Player = new Character { Health = 100, MaxHealth = 100 },
            Enemy = new Enemy { Health = health, MaxHealth = 100 }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Enemy.Health);
    }
}
