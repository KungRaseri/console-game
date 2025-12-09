using MediatR;

namespace Game.Features.Achievement.Commands;

public record CheckAchievementProgressCommand : IRequest<List<Models.Achievement>>;

public class CheckAchievementProgressHandler : IRequestHandler<CheckAchievementProgressCommand, List<Models.Achievement>>
{
    private readonly Services.AchievementService _achievementService;
    
    public CheckAchievementProgressHandler(Services.AchievementService achievementService)
    {
        _achievementService = achievementService;
    }
    
    public async Task<List<Models.Achievement>> Handle(CheckAchievementProgressCommand request, CancellationToken cancellationToken)
    {
        return await _achievementService.CheckAllAchievementsAsync();
    }
}
