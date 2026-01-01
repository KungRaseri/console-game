using Game.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.Combat.Queries.GetCombatState;

/// <summary>
/// Query to get the current state of combat.
/// </summary>
public record GetCombatStateQuery : IRequest<CombatStateDto>
{
    public required Character Player { get; init; }
    public required Enemy Enemy { get; init; }
}

/// <summary>
/// Data transfer object for combat state.
/// </summary>
public record CombatStateDto
{
    public int PlayerHealthPercentage { get; init; }
    public int EnemyHealthPercentage { get; init; }
    public bool PlayerCanFlee { get; init; }
    public bool PlayerHasItems { get; init; }
    public List<string> AvailableActions { get; init; } = new();
}