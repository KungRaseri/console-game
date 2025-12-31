using Game.Shared.Models;
using MediatR;

namespace Game.Core.Features.SaveLoad.Commands;

/// <summary>
/// Command to load a saved game.
/// </summary>
public record LoadGameCommand : IRequest<LoadGameResult>
{
    public required string SaveId { get; init; }
}

/// <summary>
/// Result of load game operation.
/// </summary>
public record LoadGameResult
{
    public required bool Success { get; init; }
    public required string Message { get; init; }
    public SaveGame? SaveGame { get; init; }
}