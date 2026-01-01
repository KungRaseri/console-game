using MediatR;

using RealmEngine.Shared.Models;
namespace RealmEngine.Core.Features.Achievements.Commands;

public record UnlockAchievementCommand(string AchievementId) : IRequest<UnlockAchievementResult>;

public record UnlockAchievementResult(bool Success, Achievement? Achievement = null);

public class UnlockAchievementHandler : IRequestHandler<UnlockAchievementCommand, UnlockAchievementResult>
{
    private readonly Services.AchievementService _achievementService;

    public UnlockAchievementHandler(Services.AchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    public async Task<UnlockAchievementResult> Handle(UnlockAchievementCommand request, CancellationToken cancellationToken)
    {
        var achievement = await _achievementService.UnlockAchievementAsync(request.AchievementId);

        if (achievement != null)
        {
            return new UnlockAchievementResult(true, achievement);
        }

        return new UnlockAchievementResult(false);
    }
}
