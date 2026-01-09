using MediatR;

using RealmEngine.Shared.Models;
namespace RealmEngine.Core.Features.Achievements.Commands;

/// <summary>
/// Represents a command to unlock an achievement by ID.
/// </summary>
/// <param name="AchievementId">The unique identifier of the achievement to unlock.</param>
public record UnlockAchievementCommand(string AchievementId) : IRequest<UnlockAchievementResult>;

/// <summary>
/// Represents the result of an achievement unlock operation.
/// </summary>
/// <param name="Success">Gets a value indicating whether the unlock was successful.</param>
/// <param name="Achievement">Gets the unlocked achievement, or null if the unlock failed.</param>
public record UnlockAchievementResult(bool Success, Achievement? Achievement = null);

/// <summary>
/// Handles the <see cref="UnlockAchievementCommand"/> by unlocking the specified achievement.
/// </summary>
public class UnlockAchievementHandler : IRequestHandler<UnlockAchievementCommand, UnlockAchievementResult>
{
    private readonly Services.AchievementService _achievementService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnlockAchievementHandler"/> class.
    /// </summary>
    /// <param name="achievementService">The achievement service.</param>
    public UnlockAchievementHandler(Services.AchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    /// <summary>
    /// Handles the unlock achievement command and returns the result.
    /// </summary>
    /// <param name="request">The unlock achievement command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the unlock result.</returns>
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
