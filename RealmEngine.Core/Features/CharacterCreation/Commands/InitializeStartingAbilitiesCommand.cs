using RealmEngine.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.CharacterCreation.Commands;

/// <summary>
/// Command to initialize starting abilities for a new character based on their class.
/// </summary>
public record InitializeStartingAbilitiesCommand : IRequest<InitializeStartingAbilitiesResult>
{
    public required Character Character { get; init; }
    public required string ClassName { get; init; }
}

/// <summary>
/// Result of initializing starting abilities.
/// </summary>
public record InitializeStartingAbilitiesResult
{
    public int AbilitiesLearned { get; init; }
    public List<string> AbilityIds { get; init; } = new();
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
