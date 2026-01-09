using RealmEngine.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.CharacterCreation.Commands;

/// <summary>
/// Command to create a new character with full initialization (abilities, spells, equipment).
/// </summary>
public record CreateCharacterCommand : IRequest<CreateCharacterResult>
{
    /// <summary>
    /// Gets the name of the new character.
    /// </summary>
    public required string CharacterName { get; init; }
    
    /// <summary>
    /// Gets the character class for the new character.
    /// </summary>
    public required CharacterClass CharacterClass { get; init; }
}

/// <summary>
/// Result of creating a new character.
/// </summary>
public record CreateCharacterResult
{
    /// <summary>
    /// Gets the newly created and initialized character.
    /// </summary>
    public Character? Character { get; init; }
    
    /// <summary>
    /// Gets a value indicating whether character creation was successful.
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// Gets a message describing the result.
    /// </summary>
    public string Message { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets the number of starting abilities that were learned.
    /// </summary>
    public int AbilitiesLearned { get; init; }
    
    /// <summary>
    /// Gets the number of starting spells that were learned.
    /// </summary>
    public int SpellsLearned { get; init; }
}
