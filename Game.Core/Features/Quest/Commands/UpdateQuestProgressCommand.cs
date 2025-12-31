using MediatR;

namespace Game.Core.Features.Quests.Commands;

public record UpdateQuestProgressCommand(string QuestId, string ObjectiveId, int Amount) : IRequest<UpdateQuestProgressResult>;

public record UpdateQuestProgressResult(bool Success, bool ObjectiveCompleted, bool QuestCompleted);

public class UpdateQuestProgressHandler : IRequestHandler<UpdateQuestProgressCommand, UpdateQuestProgressResult>
{
    private readonly Services.QuestProgressService _progressService;

    public UpdateQuestProgressHandler(Services.QuestProgressService progressService)
    {
        _progressService = progressService;
    }

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
