using MediatR;

using RealmEngine.Shared.Models;
namespace RealmEngine.Core.Features.Achievements.Commands;

public record CheckAchievementProgressCommand : IRequest<List<Achievement>>;

public class CheckAchievementProgressHandler : IRequestHandler<CheckAchievementProgressCommand, List<Achievement>>
{
    private readonly Services.AchievementService _achievementService;

    public CheckAchievementProgressHandler(Services.AchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    public async Task<List<Achievement>> Handle(CheckAchievementProgressCommand request, CancellationToken cancellationToken)
    {
        return await _achievementService.CheckAllAchievementsAsync();
    }
}
