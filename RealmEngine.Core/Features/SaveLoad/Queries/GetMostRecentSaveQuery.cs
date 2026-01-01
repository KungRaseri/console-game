using RealmEngine.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.SaveLoad.Queries;

/// <summary>
/// Query to get the most recent saved game.
/// </summary>
public record GetMostRecentSaveQuery : IRequest<GetMostRecentSaveResult>
{
}

/// <summary>
/// Result containing the most recent save game.
/// </summary>
public record GetMostRecentSaveResult
{
    public SaveGame? SaveGame { get; init; }
}