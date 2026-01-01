using MediatR;

namespace RealmEngine.Core.Features.Combat.Queries.GetEnemyInfo;

/// <summary>
/// Handles the GetEnemyInfo query.
/// </summary>
public class GetEnemyInfoHandler : IRequestHandler<GetEnemyInfoQuery, EnemyInfoDto>
{
    public Task<EnemyInfoDto> Handle(GetEnemyInfoQuery request, CancellationToken cancellationToken)
    {
        var enemy = request.Enemy;

        var info = new EnemyInfoDto
        {
            Name = enemy.Name,
            Level = enemy.Level,
            Health = enemy.Health,
            MaxHealth = enemy.MaxHealth,
            Attack = enemy.Strength,
            Defense = enemy.GetPhysicalDefense(),
            Difficulty = enemy.Difficulty,
            Description = $"A level {enemy.Level} {enemy.Difficulty.ToString().ToLower()} enemy"
        };

        return Task.FromResult(info);
    }
}