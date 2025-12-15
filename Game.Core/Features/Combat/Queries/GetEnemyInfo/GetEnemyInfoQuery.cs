using Game.Core.Models;
using MediatR;

namespace Game.Core.Features.Combat.Queries.GetEnemyInfo;

/// <summary>
/// Query to get detailed information about an enemy.
/// </summary>
public record GetEnemyInfoQuery : IRequest<EnemyInfoDto>
{
    public required Enemy Enemy { get; init; }
}

/// <summary>
/// Data transfer object for enemy information.
/// </summary>
public record EnemyInfoDto
{
    public string Name { get; init; } = string.Empty;
    public int Level { get; init; }
    public int Health { get; init; }
    public int MaxHealth { get; init; }
    public int Attack { get; init; }
    public int Defense { get; init; }
    public EnemyDifficulty Difficulty { get; init; }
    public string Description { get; init; } = string.Empty;
}
