using MediatR;

using Game.Shared.Models;
namespace Game.Core.Features.Achievements.Queries;

public record GetUnlockedAchievementsQuery : IRequest<List<Achievement>>;

public class GetUnlockedAchievementsHandler : IRequestHandler<GetUnlockedAchievementsQuery, List<Achievement>>
{
    private readonly Services.AchievementService _achievementService;

    public GetUnlockedAchievementsHandler(Services.AchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    public async Task<List<Achievement>> Handle(GetUnlockedAchievementsQuery request, CancellationToken cancellationToken)
    {
        return await _achievementService.GetUnlockedAchievementsAsync();
    }
}
