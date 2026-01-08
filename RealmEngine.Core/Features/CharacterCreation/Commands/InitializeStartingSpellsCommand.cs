using RealmEngine.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.CharacterCreation.Commands;

/// <summary>
/// Command to initialize starting spells for a new character based on their class.
/// </summary>
public record InitializeStartingSpellsCommand : IRequest<InitializeStartingSpellsResult>
{
    public required Character Character { get; init; }
    public required string ClassName { get; init; }
}

/// <summary>
/// Result of initializing starting spells.
/// </summary>
public record InitializeStartingSpellsResult
{
    public int SpellsLearned { get; init; }
    public List<string> SpellIds { get; init; } = new();
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
