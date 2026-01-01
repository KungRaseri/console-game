using FluentAssertions;
using Game.Core.Features.CharacterCreation.Queries;
using Game.Data.Repositories;

namespace Game.Tests.Features.CharacterCreation.Queries;

[Trait("Category", "Feature")]
/// <summary>
/// Tests for GetCharacterClassesHandler.
/// </summary>
public class GetCharacterClassesHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_All_Character_Classes()
    {
        // Arrange
        var repository = new CharacterClassRepository();
        var handler = new GetCharacterClassesHandler(repository);
        var query = new GetCharacterClassesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Classes.Should().NotBeNull();
        result.Classes.Should().NotBeEmpty();
        result.Classes.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task Handle_Should_Return_Expected_Character_Classes()
    {
        // Arrange
        var repository = new CharacterClassRepository();
        var handler = new GetCharacterClassesHandler(repository);
        var query = new GetCharacterClassesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var classNames = result.Classes.Select(c => c.Name).ToList();
        classNames.Should().Contain("Warrior");
        classNames.Should().Contain("Mage");
        classNames.Should().Contain("Rogue");
    }

    [Fact]
    public async Task Handle_Should_Return_Classes_With_Valid_Data()
    {
        // Arrange
        var repository = new CharacterClassRepository();
        var handler = new GetCharacterClassesHandler(repository);
        var query = new GetCharacterClassesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Classes.Should().AllSatisfy(c =>
        {
            c.Name.Should().NotBeNullOrEmpty();
            c.Description.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task Handle_Should_Complete_Successfully()
    {
        // Arrange
        var repository = new CharacterClassRepository();
        var handler = new GetCharacterClassesHandler(repository);
        var query = new GetCharacterClassesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert - should complete successfully
        result.Should().NotBeNull();
        result.Classes.Should().NotBeNull();
    }
}
