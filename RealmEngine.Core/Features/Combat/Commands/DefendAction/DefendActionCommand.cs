using Game.Shared.Models;
using MediatR;

namespace Game.Core.Features.Combat.Commands.DefendAction;

/// <summary>
/// Command to have the player defend (reduce incoming damage).
/// </summary>
public record DefendActionCommand : IRequest<DefendActionResult>
{
    public required Character Player { get; init; }
    public CombatLog? CombatLog { get; init; }
}

/// <summary>
/// Result of a defend action.
/// </summary>
public record DefendActionResult
{
    public int DefenseBonus { get; init; }
    public string Message { get; init; } = string.Empty;
}