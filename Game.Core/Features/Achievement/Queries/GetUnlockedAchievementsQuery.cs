using MediatR;

namespace Game.Core.Features.Achievement.Queries;

public record GetUnlockedAchievementsQuery : IRequest<List<Models.Achievement>>;

public class GetUnlockedAchievementsHandler : IRequestHandler<GetUnlockedAchievementsQuery, List<Models.Achievement>>
{
    private readonly Services.AchievementService _achievementService;
    
    public GetUnlockedAchievementsHandler(Services.AchievementService achievementService)
    {
        _achievementService = achievementService;
    }
    
    public async Task<List<Models.Achievement>> Handle(GetUnlockedAchievementsQuery request, CancellationToken cancellationToken)
    {
        return await _achievementService.GetUnlockedAchievementsAsync();
    }
}
