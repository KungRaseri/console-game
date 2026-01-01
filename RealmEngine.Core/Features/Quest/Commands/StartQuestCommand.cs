using MediatR;

using Game.Shared.Models;
namespace Game.Core.Features.Quests.Commands;

public record StartQuestCommand(string QuestId) : IRequest<StartQuestResult>;

public record StartQuestResult(bool Success, string Message, Quest? Quest = null);

public class StartQuestHandler : IRequestHandler<StartQuestCommand, StartQuestResult>
{
    private readonly Services.QuestService _questService;

    public StartQuestHandler(Services.QuestService questService)
    {
        _questService = questService;
    }

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
