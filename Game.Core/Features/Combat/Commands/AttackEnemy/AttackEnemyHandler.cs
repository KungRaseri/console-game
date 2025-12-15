using Game.Core.Models;
using Game.Core.Services;
using Game.Core.Features.SaveLoad;
using MediatR;
using Serilog;

namespace Game.Core.Features.Combat.Commands.AttackEnemy;

/// <summary>
/// Handles the AttackEnemy command.
/// </summary>
public class AttackEnemyHandler : IRequestHandler<AttackEnemyCommand, AttackEnemyResult>
{
    private readonly CombatService _combatService;
    private readonly IMediator _mediator;
    private readonly SaveGameService _saveGameService;

    public AttackEnemyHandler(CombatService combatService, IMediator mediator, SaveGameService saveGameService)
    {
        _combatService = combatService;
        _mediator = mediator;
        _saveGameService = saveGameService;
    }

    public async Task<AttackEnemyResult> Handle(AttackEnemyCommand request, CancellationToken cancellationToken)
    {
        var player = request.Player;
        var enemy = request.Enemy;
        var combatLog = request.CombatLog;

        // Calculate damage using combat service
        var combatResult = _combatService.ExecutePlayerAttack(player, enemy);
        var damage = combatResult.Damage;
        var isCritical = combatResult.IsCritical;

        // Apply damage to enemy
        enemy.Health -= damage;
        
        // Log to combat log
        combatLog?.AddEntry($"{player.Name} attacks for {damage} damage" + 
            (isCritical ? " (CRITICAL!)" : ""));

        // Publish attack event
        await _mediator.Publish(new AttackPerformed(player.Name, enemy.Name, damage), cancellationToken);

        // Check if enemy defeated
        var isDefeated = enemy.Health <= 0;
        int xpGained = 0;
        int goldGained = 0;

        if (isDefeated)
        {
            // Award gold and experience with difficulty multiplier
            var difficulty = _saveGameService.GetDifficultySettings();
            xpGained = (int)(enemy.XPReward * difficulty.GoldXPMultiplier);
            goldGained = (int)(enemy.GoldReward * difficulty.GoldXPMultiplier);
            
            player.Experience += xpGained;
            player.Gold += goldGained;

            combatLog?.AddEntry($"{enemy.Name} defeated! Gained {xpGained} XP and {goldGained} gold!");

            await _mediator.Publish(new EnemyDefeated(player.Name, enemy.Name), cancellationToken);
            await _mediator.Publish(new GoldGained(player.Name, goldGained), cancellationToken);
        }

        Log.Information("Player {PlayerName} attacked {EnemyName} for {Damage} damage (critical: {IsCritical})",
            player.Name, enemy.Name, damage, isCritical);

        return new AttackEnemyResult
        {
            Damage = damage,
            IsCritical = isCritical,
            IsEnemyDefeated = isDefeated,
            ExperienceGained = xpGained,
            GoldGained = goldGained
        };
    }
}
