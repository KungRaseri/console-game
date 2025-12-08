using MediatR;

namespace Game.Features.SaveLoad.Commands;

/// <summary>
/// Command to delete a saved game.
/// </summary>
public record DeleteSaveCommand : IRequest<DeleteSaveResult>
{
    public required string SaveId { get; init; }
}

/// <summary>
/// Result of delete save operation.
/// </summary>
public record DeleteSaveResult
{
    public required bool Success { get; init; }
    public required string Message { get; init; }
}
