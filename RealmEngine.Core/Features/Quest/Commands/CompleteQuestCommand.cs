using MediatR;
using RealmEngine.Core.Features.SaveLoad;

namespace RealmEngine.Core.Features.Quests.Commands;

public record CompleteQuestCommand(string QuestId) : IRequest<CompleteQuestResult>;

public record CompleteQuestResult(bool Success, string Message, QuestRewards? Rewards = null);

public record QuestRewards(int Xp, int Gold, int ApocalypseBonus, List<string> Items);

public class CompleteQuestHandler : IRequestHandler<CompleteQuestCommand, CompleteQuestResult>
{
    private readonly Services.QuestService _questService;
    private readonly Services.QuestRewardService _rewardService;
    private readonly SaveGameService _saveGameService;

    public CompleteQuestHandler(
        Services.QuestService questService, 
        Services.QuestRewardService rewardService,
        SaveGameService saveGameService)
    {
        _questService = questService;
        _rewardService = rewardService;
        _saveGameService = saveGameService;
    }

    public async Task<CompleteQuestResult> Handle(CompleteQuestCommand request, CancellationToken cancellationToken)
    {
        var result = await _questService.CompleteQuestAsync(request.QuestId);

        if (result.Success)
        {
            var saveGame = _saveGameService.GetCurrentSave();
            if (saveGame != null)
            {
                // Distribute rewards to the player
                _rewardService.DistributeRewards(result.Quest!, saveGame.Character, saveGame);
            }

            var rewards = new QuestRewards(
                result.Quest!.XpReward,
                result.Quest.GoldReward,
                result.Quest.ApocalypseBonusMinutes,
                result.Quest.ItemRewardIds
            );

            return new CompleteQuestResult(true, $"Quest completed: {result.Quest.Title}", rewards);
        }

        return new CompleteQuestResult(false, result.ErrorMessage);
    }
}
