using Game.Core.Features.SaveLoad;
using Game.Core.Features.Victory.Commands;
using Serilog;

namespace Game.Core.Features.Victory.Services;

public class VictoryService
{
    private readonly ISaveGameService _saveGameService;
    
    public VictoryService(ISaveGameService saveGameService)
    {
        _saveGameService = saveGameService;
    }
    
    public async Task<VictoryStatistics?> CalculateVictoryStatisticsAsync()
    {
        var saveGame = _saveGameService.GetCurrentSave();
        if (saveGame == null)
            return null;
        
        var statistics = new VictoryStatistics(
            saveGame.Character.Name,
            saveGame.Character.ClassName,
            saveGame.Character.Level,
            saveGame.DifficultyLevel,
            saveGame.PlayTimeMinutes,
            saveGame.QuestsCompleted,
            saveGame.TotalEnemiesDefeated,
            saveGame.DeathCount,
            saveGame.UnlockedAchievements.Count,
            saveGame.TotalGoldEarned
        );
        
        Log.Information("Victory statistics calculated for {PlayerName}", saveGame.Character.Name);
        
        return await Task.FromResult(statistics);
    }
    
    public async Task MarkGameCompleteAsync()
    {
        var saveGame = _saveGameService.GetCurrentSave();
        if (saveGame == null)
            return;
        
        // Add a game flag for completion
        saveGame.GameFlags["GameCompleted"] = true;
        saveGame.GameFlags["CompletionDate"] = true;
        
        _saveGameService.SaveGame(saveGame);
        
        Log.Information("Game marked as completed for {PlayerName}", saveGame.Character.Name);
        
        await Task.CompletedTask;
    }
}
