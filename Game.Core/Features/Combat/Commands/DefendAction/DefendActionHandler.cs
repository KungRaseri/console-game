using MediatR;
using Serilog;

namespace Game.Core.Features.Combat.Commands.DefendAction;

/// <summary>
/// Handles the DefendAction command.
/// </summary>
public class DefendActionHandler : IRequestHandler<DefendActionCommand, DefendActionResult>
{
    public Task<DefendActionResult> Handle(DefendActionCommand request, CancellationToken cancellationToken)
    {
        var player = request.Player;
        var combatLog = request.CombatLog;

        // Apply temporary defense bonus (based on Constitution)
        var defenseBonus = player.Constitution / 2;

        combatLog?.AddEntry($"{player.Name} takes a defensive stance! (Defense +{defenseBonus})");
        
        Log.Information("Player {PlayerName} defended (bonus: {DefenseBonus})", 
            player.Name, defenseBonus);

        return Task.FromResult(new DefendActionResult
        {
            DefenseBonus = defenseBonus,
            Message = $"You brace yourself for impact! Defense +{defenseBonus}"
        });
    }
}