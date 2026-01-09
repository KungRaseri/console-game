using RealmEngine.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.Progression.Queries;

/// <summary>
/// Query to get abilities available to a character.
/// </summary>
public record GetAvailableAbilitiesQuery : IRequest<GetAvailableAbilitiesResult>
{
    /// <summary>Gets the class name to get abilities for.</summary>
    public required string ClassName { get; init; }
    /// <summary>Gets the character level.</summary>
    public int Level { get; init; }
    /// <summary>Gets the optional tier filter.</summary>
    public int? Tier { get; init; }
}

/// <summary>
/// Result containing available abilities.
/// </summary>
public record GetAvailableAbilitiesResult
{
    /// <summary>Gets the list of available abilities.</summary>
    public required List<Ability> Abilities { get; init; }
    /// <summary>Gets the total count of abilities.</summary>
    public int TotalCount { get; init; }
}
