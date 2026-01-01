using Game.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.Combat.Commands.AttackEnemy;

/// <summary>
/// Command to execute a player attack against an enemy.
/// </summary>
public record AttackEnemyCommand : IRequest<AttackEnemyResult>
{
    public required Character Player { get; init; }
    public required Enemy Enemy { get; init; }
    public CombatLog? CombatLog { get; init; }
}

/// <summary>
/// Result of an attack command.
/// </summary>
public record AttackEnemyResult
{
    public int Damage { get; init; }
    public bool IsCritical { get; init; }
    public bool IsEnemyDefeated { get; init; }
    public int ExperienceGained { get; init; }
    public int GoldGained { get; init; }
}