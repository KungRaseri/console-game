using Game.Services;
using MediatR;
using Serilog;

namespace Game.Features.Combat.Commands.FleeFromCombat;

/// <summary>
/// Handles the FleeFromCombat command.
/// </summary>
public class FleeFromCombatHandler : IRequestHandler<FleeFromCombatCommand, FleeFromCombatResult>
{
    private readonly CombatService _combatService;

    public FleeFromCombatHandler(CombatService combatService)
    {
        _combatService = combatService;
    }

    public Task<FleeFromCombatResult> Handle(FleeFromCombatCommand request, CancellationToken cancellationToken)
    {
        var player = request.Player;
        var enemy = request.Enemy;
        var combatLog = request.CombatLog;

        // Attempt to flee using combat service
        var combatResult = _combatService.AttemptFlee(player, enemy);
        var success = combatResult.Success;

        if (success)
        {
            combatLog?.AddEntry($"{player.Name} successfully fled from {enemy.Name}!");
            Log.Information("Player {PlayerName} fled from {EnemyName}", player.Name, enemy.Name);
            
            return Task.FromResult(new FleeFromCombatResult
            {
                Success = true,
                Message = "You successfully escaped!"
            });
        }
        else
        {
            combatLog?.AddEntry($"{player.Name} failed to escape!");
            Log.Information("Player {PlayerName} failed to flee from {EnemyName}", player.Name, enemy.Name);
            
            return Task.FromResult(new FleeFromCombatResult
            {
                Success = false,
                Message = "You couldn't escape!"
            });
        }
    }
}
