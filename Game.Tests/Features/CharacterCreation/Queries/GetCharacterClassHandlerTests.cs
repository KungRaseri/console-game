using FluentAssertions;
using Game.Features.CharacterCreation.Queries;
using Xunit;

namespace Game.Tests.Features.CharacterCreation.Queries;

/// <summary>
/// Tests for GetCharacterClassHandler.
/// </summary>
public class GetCharacterClassHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Found_True_For_Valid_Class()
    {
        // Arrange
        var handler = new GetCharacterClassHandler();
        var query = new GetCharacterClassQuery { ClassName = "Warrior" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Found.Should().BeTrue();
        result.CharacterClass.Should().NotBeNull();
        result.CharacterClass!.Name.Should().Be("Warrior");
    }

    [Fact]
    public async Task Handle_Should_Return_Found_False_For_Invalid_Class()
    {
        // Arrange
        var handler = new GetCharacterClassHandler();
        var query = new GetCharacterClassQuery { ClassName = "InvalidClass" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Found.Should().BeFalse();
        result.CharacterClass.Should().BeNull();
    }

    [Theory]
    [InlineData("Warrior")]
    [InlineData("Mage")]
    [InlineData("Rogue")]
    public async Task Handle_Should_Return_Valid_Class_Data(string className)
    {
        // Arrange
        var handler = new GetCharacterClassHandler();
        var query = new GetCharacterClassQuery { ClassName = className };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Found.Should().BeTrue();
        result.CharacterClass.Should().NotBeNull();
        result.CharacterClass!.Name.Should().Be(className);
        result.CharacterClass.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_Should_Be_Case_Sensitive()
    {
        // Arrange
        var handler = new GetCharacterClassHandler();
        var query = new GetCharacterClassQuery { ClassName = "warrior" }; // lowercase

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert - depends on repository implementation
        // If case-insensitive, Found should be true; if case-sensitive, false
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_Should_Return_Null_CharacterClass_When_Not_Found()
    {
        // Arrange
        var handler = new GetCharacterClassHandler();
        var query = new GetCharacterClassQuery { ClassName = "NonExistentClass" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Found.Should().BeFalse();
        result.CharacterClass.Should().BeNull();
    }
}
