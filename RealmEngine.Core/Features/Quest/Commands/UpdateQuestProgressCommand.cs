using MediatR;

namespace RealmEngine.Core.Features.Quests.Commands;

/// <summary>
/// Command to update quest progress.
/// </summary>
public record UpdateQuestProgressCommand(string QuestId, string ObjectiveId, int Amount) : IRequest<UpdateQuestProgressResult>;

/// <summary>
/// Result of updating quest progress.
/// </summary>
public record UpdateQuestProgressResult(bool Success, bool ObjectiveCompleted, bool QuestCompleted);

/// <summary>
/// Handles updating quest progress.
/// </summary>
public class UpdateQuestProgressHandler : IRequestHandler<UpdateQuestProgressCommand, UpdateQuestProgressResult>
{
    private readonly Services.QuestProgressService _progressService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateQuestProgressHandler"/> class.
    /// </summary>
    /// <param name="progressService">The quest progress service.</param>
    public UpdateQuestProgressHandler(Services.QuestProgressService progressService)
    {
        _progressService = progressService;
    }

    /// <summary>
    /// Handles updating quest progress.
    /// </summary>
    /// <param name="request">The update progress command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The update progress result.</returns>
    public async Task<UpdateQuestProgressResult> Handle(UpdateQuestProgressCommand request, CancellationToken cancellationToken)
    {
        var result = await _progressService.UpdateProgressAsync(request.QuestId, request.ObjectiveId, request.Amount);

        return new UpdateQuestProgressResult(
            result.Success,
            result.ObjectiveCompleted,
            result.QuestCompleted
        );
    }
}
