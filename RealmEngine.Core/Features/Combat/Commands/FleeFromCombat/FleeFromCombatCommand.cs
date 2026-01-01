using Game.Shared.Models;
using MediatR;

namespace Game.Core.Features.Combat.Commands.FleeFromCombat;

/// <summary>
/// Command to attempt to flee from combat.
/// </summary>
public record FleeFromCombatCommand : IRequest<FleeFromCombatResult>
{
    public required Character Player { get; init; }
    public required Enemy Enemy { get; init; }
    public CombatLog? CombatLog { get; init; }
}

/// <summary>
/// Result of a flee attempt.
/// </summary>
public record FleeFromCombatResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}