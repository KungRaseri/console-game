using RealmEngine.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.CharacterCreation.Commands;

/// <summary>
/// Command to initialize starting spells for a new character based on their class.
/// </summary>
public record InitializeStartingSpellsCommand : IRequest<InitializeStartingSpellsResult>
{
    /// <summary>
    /// Gets the character to initialize spells for.
    /// </summary>
    public required Character Character { get; init; }
    
    /// <summary>
    /// Gets the name of the character class.
    /// </summary>
    public required string ClassName { get; init; }
}

/// <summary>
/// Result of initializing starting spells.
/// </summary>
public record InitializeStartingSpellsResult
{
    /// <summary>
    /// Gets the number of spells learned.
    /// </summary>
    public int SpellsLearned { get; init; }
    
    /// <summary>
    /// Gets the list of spell IDs that were learned.
    /// </summary>
    public List<string> SpellIds { get; init; } = new();
    
    /// <summary>
    /// Gets a value indicating whether the initialization was successful.
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// Gets a message describing the result.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
