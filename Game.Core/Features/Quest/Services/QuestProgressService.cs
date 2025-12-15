using Game.Core.Features.SaveLoad;
using Serilog;

namespace Game.Core.Features.Quest.Services;

public class QuestProgressService
{
    private readonly SaveGameService _saveGameService;
    
    public QuestProgressService(SaveGameService saveGameService)
    {
        _saveGameService = saveGameService;
    }
    
    public async Task<(bool Success, bool ObjectiveCompleted, bool QuestCompleted)> UpdateProgressAsync(
        string questId, string objectiveId, int amount)
    {
        var saveGame = _saveGameService.GetCurrentSave();
        if (saveGame == null)
            return (false, false, false);
        
        var quest = saveGame.ActiveQuests.FirstOrDefault(q => q.Id == questId);
        if (quest == null)
            return (false, false, false);
        
        if (!quest.Objectives.ContainsKey(objectiveId))
            return (false, false, false);
        
        // Update progress
        var current = quest.ObjectiveProgress.ContainsKey(objectiveId) ? quest.ObjectiveProgress[objectiveId] : 0;
        var required = quest.Objectives[objectiveId];
        
        quest.ObjectiveProgress[objectiveId] = Math.Min(current + amount, required);
        
        var objectiveCompleted = quest.ObjectiveProgress[objectiveId] >= required;
        var questCompleted = quest.Objectives.All(kvp => 
            quest.ObjectiveProgress.ContainsKey(kvp.Key) && quest.ObjectiveProgress[kvp.Key] >= kvp.Value);
        
        _saveGameService.SaveGame(saveGame);
        
        Log.Debug("Quest progress updated: {QuestId}/{ObjectiveId} = {Current}/{Required}",
            questId, objectiveId, quest.ObjectiveProgress[objectiveId], required);
        
        return await Task.FromResult((true, objectiveCompleted, questCompleted));
    }
}
