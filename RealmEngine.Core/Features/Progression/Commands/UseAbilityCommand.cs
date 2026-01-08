using RealmEngine.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.Progression.Commands;

/// <summary>
/// Command to use an ability in combat or exploration.
/// </summary>
public record UseAbilityCommand : IRequest<UseAbilityResult>
{
    public required Character User { get; init; }
    public required string AbilityId { get; init; }
    public Character? TargetCharacter { get; init; }
    public Enemy? TargetEnemy { get; init; }
}

/// <summary>
/// Result of using an ability.
/// </summary>
public record UseAbilityResult
{
    public bool Success { get; init; }
    public required string Message { get; init; }
    public int DamageDealt { get; init; }
    public int HealingDone { get; init; }
    public int ManaCost { get; init; }
    public Ability? AbilityUsed { get; init; }
}
