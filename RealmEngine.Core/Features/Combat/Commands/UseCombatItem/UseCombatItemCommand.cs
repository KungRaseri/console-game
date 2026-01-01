using Game.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.Combat.Commands.UseCombatItem;

/// <summary>
/// Command to use a consumable item during combat.
/// </summary>
public record UseCombatItemCommand : IRequest<UseCombatItemResult>
{
    public required Character Player { get; init; }
    public required Item Item { get; init; }
    public CombatLog? CombatLog { get; init; }
}

/// <summary>
/// Result of using a combat item.
/// </summary>
public record UseCombatItemResult
{
    public bool Success { get; init; }
    public int HealthRestored { get; init; }
    public int ManaRestored { get; init; }
    public string Message { get; init; } = string.Empty;
}