using Game.Models;
using MediatR;

namespace Game.Features.SaveLoad.Commands;

/// <summary>
/// Command to save the current game state.
/// </summary>
public record SaveGameCommand : IRequest<SaveGameResult>
{
    public required Character Player { get; init; }
    public required List<Item> Inventory { get; init; }
    public string? SaveId { get; init; }
}

/// <summary>
/// Result of save game operation.
/// </summary>
public record SaveGameResult
{
    public required bool Success { get; init; }
    public required string Message { get; init; }
    public string? SaveId { get; init; }
}
