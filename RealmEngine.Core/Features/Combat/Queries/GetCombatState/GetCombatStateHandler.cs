using RealmEngine.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.Combat.Queries.GetCombatState;

/// <summary>
/// Handles the GetCombatState query.
/// </summary>
public class GetCombatStateHandler : IRequestHandler<GetCombatStateQuery, CombatStateDto>
{
    public Task<CombatStateDto> Handle(GetCombatStateQuery request, CancellationToken cancellationToken)
    {
        var player = request.Player;
        var enemy = request.Enemy;

        var state = new CombatStateDto
        {
            PlayerHealthPercentage = (int)((double)player.Health / player.MaxHealth * 100),
            EnemyHealthPercentage = (int)((double)enemy.Health / enemy.MaxHealth * 100),
            PlayerCanFlee = true, // Always can attempt to flee
            PlayerHasItems = player.Inventory.Any(i => i.Type == ItemType.Consumable),
            AvailableActions = new List<string> { "Attack", "Defend", "Use Item", "Flee" }
        };

        return Task.FromResult(state);
    }
}