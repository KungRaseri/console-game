using MediatR;

namespace Game.Core.Features.Quest.Commands;

public record CompleteQuestCommand(string QuestId) : IRequest<CompleteQuestResult>;

public record CompleteQuestResult(bool Success, string Message, QuestRewards? Rewards = null);

public record QuestRewards(int Xp, int Gold, int ApocalypseBonus, List<string> Items);

public class CompleteQuestHandler : IRequestHandler<CompleteQuestCommand, CompleteQuestResult>
{
    private readonly Services.QuestService _questService;

    public CompleteQuestHandler(Services.QuestService questService)
    {
        _questService = questService;
    }

    public async Task<CompleteQuestResult> Handle(CompleteQuestCommand request, CancellationToken cancellationToken)
    {
        var result = await _questService.CompleteQuestAsync(request.QuestId);

        if (result.Success)
        {
            var rewards = new QuestRewards(
                result.Quest!.XpReward,
                result.Quest.GoldReward,
                result.Quest.ApocalypseBonusMinutes,
                result.Quest.ItemRewards
            );

            return new CompleteQuestResult(true, $"Quest completed: {result.Quest.Title}", rewards);
        }

        return new CompleteQuestResult(false, result.ErrorMessage);
    }
}
