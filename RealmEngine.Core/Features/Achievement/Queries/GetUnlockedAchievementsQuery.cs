using MediatR;

using RealmEngine.Shared.Models;
namespace RealmEngine.Core.Features.Achievements.Queries;

/// <summary>
/// Represents a query to retrieve all unlocked achievements.
/// </summary>
public record GetUnlockedAchievementsQuery : IRequest<List<Achievement>>;

/// <summary>
/// Handles the <see cref="GetUnlockedAchievementsQuery"/> by retrieving all unlocked achievements.
/// </summary>
public class GetUnlockedAchievementsHandler : IRequestHandler<GetUnlockedAchievementsQuery, List<Achievement>>
{
    private readonly Services.AchievementService _achievementService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUnlockedAchievementsHandler"/> class.
    /// </summary>
    /// <param name="achievementService">The achievement service.</param>
    public GetUnlockedAchievementsHandler(Services.AchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    /// <summary>
    /// Handles the get unlocked achievements query and returns the list of unlocked achievements.
    /// </summary>
    /// <param name="request">The get unlocked achievements query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing a list of unlocked achievements.</returns>
    public async Task<List<Achievement>> Handle(GetUnlockedAchievementsQuery request, CancellationToken cancellationToken)
    {
        return await _achievementService.GetUnlockedAchievementsAsync();
    }
}
