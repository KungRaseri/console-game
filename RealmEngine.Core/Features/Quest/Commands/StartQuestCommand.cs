using MediatR;

using RealmEngine.Shared.Models;
namespace RealmEngine.Core.Features.Quests.Commands;

/// <summary>
/// Command to start a quest.
/// </summary>
public record StartQuestCommand(string QuestId) : IRequest<StartQuestResult>;

/// <summary>
/// Result of starting a quest.
/// </summary>
public record StartQuestResult(bool Success, string Message, Quest? Quest = null);

/// <summary>
/// Handles starting a quest.
/// </summary>
public class StartQuestHandler : IRequestHandler<StartQuestCommand, StartQuestResult>
{
    private readonly Services.QuestService _questService;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartQuestHandler"/> class.
    /// </summary>
    /// <param name="questService">The quest service.</param>
    public StartQuestHandler(Services.QuestService questService)
    {
        _questService = questService;
    }

    /// <summary>
    /// Handles starting a quest.
    /// </summary>
    /// <param name="request">The start quest command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The start quest result.</returns>
    public async Task<StartQuestResult> Handle(StartQuestCommand request, CancellationToken cancellationToken)
    {
        var result = await _questService.StartQuestAsync(request.QuestId);

        if (result.Success)
        {
            return new StartQuestResult(true, $"Quest started: {result.Quest!.Title}", result.Quest);
        }

        return new StartQuestResult(false, result.ErrorMessage);
    }
}
