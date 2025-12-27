using Game.Core.Models;
using Game.Core.Features.SaveLoad;
using Serilog;

namespace Game.Core.Features.Quest.Services;

public class QuestService
{
    private readonly ISaveGameService _saveGameService;
    private readonly MainQuestService _mainQuestService;

    public QuestService(ISaveGameService saveGameService, MainQuestService mainQuestService)
    {
        _saveGameService = saveGameService;
        _mainQuestService = mainQuestService;
    }

    public async Task<(bool Success, Models.Quest? Quest, string ErrorMessage)> StartQuestAsync(string questId)
    {
        var saveGame = _saveGameService.GetCurrentSave();
        if (saveGame == null)
            return (false, null, "No active save game");

        // Check if quest is already active or completed
        if (saveGame.ActiveQuests.Any(q => q.Id == questId))
            return (false, null, "Quest is already active");

        if (saveGame.CompletedQuests.Any(q => q.Id == questId))
            return (false, null, "Quest is already completed");

        // Get quest definition
        var quest = await GetQuestDefinitionAsync(questId);
        if (quest == null)
            return (false, null, "Quest not found");

        // Check prerequisites
        if (!CheckPrerequisites(quest, saveGame))
            return (false, null, "Prerequisites not met");

        // Check level requirement (use existing logic or default to level 1)
        // For now, we'll skip level requirements since the existing Quest model doesn't have RequiredLevel

        // Start quest
        quest.IsActive = true;
        quest.StartTime = DateTime.Now;
        saveGame.ActiveQuests.Add(quest);

        _saveGameService.SaveGame(saveGame);

        Log.Information("Quest started: {QuestId} - {QuestTitle}", questId, quest.Title);

        return await Task.FromResult((true, quest, string.Empty));
    }

    public async Task<(bool Success, Models.Quest? Quest, string ErrorMessage)> CompleteQuestAsync(string questId)
    {
        var saveGame = _saveGameService.GetCurrentSave();
        if (saveGame == null)
            return (false, null, "No active save game");

        var quest = saveGame.ActiveQuests.FirstOrDefault(q => q.Id == questId);
        if (quest == null)
            return (false, null, "Quest is not active");

        // Check if all objectives are complete
        var allObjectivesComplete = quest.Objectives.All(kvp =>
            quest.ObjectiveProgress.ContainsKey(kvp.Key) && quest.ObjectiveProgress[kvp.Key] >= kvp.Value);

        if (!allObjectivesComplete)
            return (false, null, "Not all objectives complete");

        // Complete quest
        quest.IsCompleted = true;
        quest.IsActive = false;

        saveGame.ActiveQuests.Remove(quest);
        saveGame.CompletedQuests.Add(quest);
        saveGame.QuestsCompleted++;

        _saveGameService.SaveGame(saveGame);

        Log.Information("Quest completed: {QuestId} - {QuestTitle}", questId, quest.Title);

        return await Task.FromResult((true, quest, string.Empty));
    }

    public async Task<List<Models.Quest>> GetActiveQuestsAsync()
    {
        var saveGame = _saveGameService.GetCurrentSave();
        return await Task.FromResult(saveGame?.ActiveQuests ?? new List<Models.Quest>());
    }

    private async Task<Models.Quest?> GetQuestDefinitionAsync(string questId)
    {
        // Get from main quest service or quest database
        return await _mainQuestService.GetQuestByIdAsync(questId);
    }

    private bool CheckPrerequisites(Models.Quest quest, SaveGame saveGame)
    {
        return quest.Prerequisites.All(prereqId =>
            saveGame.CompletedQuests.Any(q => q.Id == prereqId));
    }
}
