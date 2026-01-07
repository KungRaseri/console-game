using RealmEngine.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.Progression.Queries;

/// <summary>
/// Query to get abilities available to a character.
/// </summary>
public record GetAvailableAbilitiesQuery : IRequest<GetAvailableAbilitiesResult>
{
    public required string ClassName { get; init; }
    public int Level { get; init; }
    public int? Tier { get; init; }
}

/// <summary>
/// Result containing available abilities.
/// </summary>
public record GetAvailableAbilitiesResult
{
    public required List<Ability> Abilities { get; init; }
    public int TotalCount { get; init; }
}
