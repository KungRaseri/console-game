using Game.Models;
using MediatR;

namespace Game.Features.CharacterCreation.Commands;

/// <summary>
/// Command to create a new character with the specified parameters.
/// </summary>
public record CreateCharacterCommand : IRequest<CreateCharacterResult>
{
    public required string PlayerName { get; init; }
    public required string ClassName { get; init; }
    public required AttributeAllocation AttributeAllocation { get; init; }
}

/// <summary>
/// Result of character creation.
/// </summary>
public record CreateCharacterResult
{
    public required bool Success { get; init; }
    public required string Message { get; init; }
    public Character? Character { get; init; }
    public string? SaveGameId { get; init; }
}
