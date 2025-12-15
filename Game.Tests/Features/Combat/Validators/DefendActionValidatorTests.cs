using FluentValidation.TestHelper;
using Game.Core.Features.Combat.Commands.DefendAction;
using Game.Core.Models;

namespace Game.Tests.Features.Combat.Validators;

/// <summary>
/// Tests for DefendActionValidator.
/// </summary>
public class DefendActionValidatorTests
{
    private readonly DefendActionValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Player_Is_Null()
    {
        // Arrange
        var command = new DefendActionCommand
        {
            Player = null!
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
        var command = new DefendActionCommand
        {
            Player = new Character { Health = 0, MaxHealth = 100 }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Player.Health)
            .WithErrorMessage("Player must be alive to defend");
    }

    [Fact]
    public void Should_Have_Error_When_Player_Health_Is_Negative()
    {
        // Arrange
        var command = new DefendActionCommand
        {
            Player = new Character { Health = -10, MaxHealth = 100 }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Player.Health);
    }

    [Fact]
    public void Should_Pass_When_Player_Is_Alive()
    {
        // Arrange
        var command = new DefendActionCommand
        {
            Player = new Character { Health = 50, MaxHealth = 100 }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(100)]
    public void Should_Pass_When_Player_Health_Is_Positive(int health)
    {
        // Arrange
        var command = new DefendActionCommand
        {
            Player = new Character { Health = health, MaxHealth = 100 }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Player.Health);
    }
}
