using MediatR;

namespace Game.Core.Features.Victory.Commands;

public record TriggerVictoryCommand : IRequest<TriggerVictoryResult>;

public record TriggerVictoryResult(bool Success, VictoryStatistics? Statistics = null);

public record VictoryStatistics(
    string PlayerName,
    string ClassName,
    int FinalLevel,
    string Difficulty,
    int PlayTimeMinutes,
    int QuestsCompleted,
    int EnemiesDefeated,
    int DeathCount,
    int AchievementsUnlocked,
    int TotalGoldEarned
);

public class TriggerVictoryHandler : IRequestHandler<TriggerVictoryCommand, TriggerVictoryResult>
{
    private readonly Services.VictoryService _victoryService;

    public TriggerVictoryHandler(Services.VictoryService victoryService)
    {
        _victoryService = victoryService;
    }

    public async Task<TriggerVictoryResult> Handle(TriggerVictoryCommand request, CancellationToken cancellationToken)
    {
        var statistics = await _victoryService.CalculateVictoryStatisticsAsync();

        if (statistics != null)
        {
            await _victoryService.MarkGameCompleteAsync();
            return new TriggerVictoryResult(true, statistics);
        }

        return new TriggerVictoryResult(false);
    }
}