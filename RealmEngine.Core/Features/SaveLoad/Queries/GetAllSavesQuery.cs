using RealmEngine.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.SaveLoad.Queries;

/// <summary>
/// Query to get all saved games.
/// </summary>
public record GetAllSavesQuery : IRequest<GetAllSavesResult>
{
}

/// <summary>
/// Result containing all saved games.
/// </summary>
public record GetAllSavesResult
{
    public required List<SaveGame> SaveGames { get; init; }
}