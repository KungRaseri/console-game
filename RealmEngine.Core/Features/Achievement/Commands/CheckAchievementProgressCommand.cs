using MediatR;

using RealmEngine.Shared.Models;
namespace RealmEngine.Core.Features.Achievements.Commands;

/// <summary>
/// Represents a command to check progress for all achievements.
/// </summary>
public record CheckAchievementProgressCommand : IRequest<List<Achievement>>;

/// <summary>
/// Handles the <see cref="CheckAchievementProgressCommand"/> by checking all achievement criteria.
/// </summary>
public class CheckAchievementProgressHandler : IRequestHandler<CheckAchievementProgressCommand, List<Achievement>>
{
    private readonly Services.AchievementService _achievementService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckAchievementProgressHandler"/> class.
    /// </summary>
    /// <param name="achievementService">The achievement service.</param>
    public CheckAchievementProgressHandler(Services.AchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    /// <summary>
    /// Handles the check achievement progress command and returns newly unlocked achievements.
    /// </summary>
    /// <param name="request">The check achievement progress command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing a list of newly unlocked achievements.</returns>
    public async Task<List<Achievement>> Handle(CheckAchievementProgressCommand request, CancellationToken cancellationToken)
    {
        return await _achievementService.CheckAllAchievementsAsync();
    }
}
