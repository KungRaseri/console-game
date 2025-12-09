using Game.Features.Death;
using Game.Features.SaveLoad;
using Game.Models;
using Game.Shared.UI;
using MediatR;
using Serilog;

namespace Game.Features.Death.Commands;

/// <summary>
/// Handles player death with difficulty-appropriate penalties.
/// </summary>
public class HandlePlayerDeathHandler : IRequestHandler<HandlePlayerDeathCommand, HandlePlayerDeathResult>
{
    private readonly DeathService _deathService;
    private readonly SaveGameService _saveGameService;
    private readonly HallOfFameService _hallOfFameService;
    
    public HandlePlayerDeathHandler(
        DeathService deathService,
        SaveGameService saveGameService,
        HallOfFameService hallOfFameService)
    {
        _deathService = deathService;
        _saveGameService = saveGameService;
        _hallOfFameService = hallOfFameService;
    }
    
    public async Task<HandlePlayerDeathResult> Handle(HandlePlayerDeathCommand request, CancellationToken cancellationToken)
    {
        var player = request.Player;
        var location = request.DeathLocation;
        var killer = request.Killer;
        var saveGame = _saveGameService.GetCurrentSave();
        
        if (saveGame == null)
        {
            Log.Error("No active save game found during death handling");
            return new HandlePlayerDeathResult
            {
                IsPermadeath = false,
                SaveDeleted = false
            };
        }
        
        var difficulty = _saveGameService.GetDifficultySettings();
        
        Log.Warning("Player death at {Location}. Difficulty: {Difficulty}, Death count: {DeathCount}",
            location, difficulty.Name, saveGame.DeathCount + 1);
        
        // Record death in save
        saveGame.DeathCount++;
        saveGame.LastDeathLocation = location;
        saveGame.LastDeathDate = DateTime.Now;
        
        // Handle based on difficulty
        if (difficulty.IsPermadeath)
        {
            return await HandlePermadeathAsync(player, saveGame, location, killer);
        }
        else
        {
            return await HandleStandardDeathAsync(player, saveGame, location, difficulty);
        }
    }
    
    private async Task<HandlePlayerDeathResult> HandleStandardDeathAsync(
        Character player, SaveGame saveGame, string location, DifficultySettings difficulty)
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowError("═══════════════════════════════════════");
        ConsoleUI.ShowError("           YOU HAVE DIED               ");
        ConsoleUI.ShowError("═══════════════════════════════════════");
        await Task.Delay(2000);
        
        Console.WriteLine();
        ConsoleUI.WriteText($"You died at: {location}");
        ConsoleUI.WriteText($"Death count: {saveGame.DeathCount}");
        Console.WriteLine();
        
        // Calculate penalties
        var goldLost = (int)(player.Gold * difficulty.GoldLossPercentage);
        var xpLost = (int)(player.Experience * difficulty.XPLossPercentage);
        
        // Apply penalties
        player.Gold = Math.Max(0, player.Gold - goldLost);
        player.Experience = Math.Max(0, player.Experience - xpLost);
        
        // Handle item dropping
        var droppedItems = _deathService.HandleItemDropping(
            player, saveGame, location, difficulty);
        
        // Show penalties
        ConsoleUI.ShowError($"Penalties:");
        if (goldLost > 0)
            ConsoleUI.WriteText($"  • Lost {goldLost} gold");
        if (xpLost > 0)
            ConsoleUI.WriteText($"  • Lost {xpLost} XP");
        if (droppedItems.Count > 0)
        {
            if (difficulty.DropAllInventoryOnDeath)
                ConsoleUI.WriteText($"  • Dropped ALL {droppedItems.Count} items at {location}");
            else
                ConsoleUI.WriteText($"  • Dropped {droppedItems.Count} item(s) at {location}");
        }
        
        Console.WriteLine();
        
        // Respawn
        player.Health = player.MaxHealth;
        player.Mana = player.MaxMana;
        
        ConsoleUI.ShowSuccess("You have respawned at Hub Town with full health!");
        ConsoleUI.ShowInfo("Return to your death location to recover dropped items.");
        
        await Task.Delay(3000);
        
        // Auto-save in Ironman mode
        if (difficulty.AutoSaveOnly)
        {
            _saveGameService.SaveGame(saveGame);
            ConsoleUI.ShowInfo("Game auto-saved (Ironman mode)");
            await Task.Delay(1000);
        }
        
        return new HandlePlayerDeathResult
        {
            IsPermadeath = false,
            SaveDeleted = false,
            DroppedItems = droppedItems,
            GoldLost = goldLost,
            XPLost = xpLost
        };
    }
    
    private async Task<HandlePlayerDeathResult> HandlePermadeathAsync(
        Character player, SaveGame saveGame, string location, Enemy? killer)
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowError("═══════════════════════════════════════");
        ConsoleUI.ShowError("          PERMADEATH                   ");
        ConsoleUI.ShowError("═══════════════════════════════════════");
        await Task.Delay(2000);
        
        Console.WriteLine();
        ConsoleUI.ShowError($"{player.Name} has fallen.");
        ConsoleUI.WriteText($"Location: {location}");
        if (killer != null)
            ConsoleUI.WriteText($"Slain by: {killer.Name}");
        Console.WriteLine();
        
        await Task.Delay(2000);
        
        // Create Hall of Fame entry
        var entry = new HallOfFameEntry
        {
            CharacterName = player.Name,
            ClassName = player.ClassName,
            Level = player.Level,
            PlayTimeMinutes = saveGame.PlayTimeMinutes,
            TotalEnemiesDefeated = saveGame.TotalEnemiesDefeated,
            QuestsCompleted = saveGame.QuestsCompleted,
            DeathCount = saveGame.DeathCount,
            DeathReason = killer != null ? $"Slain by {killer.Name}" : "Unknown cause",
            DeathLocation = location,
            DeathDate = DateTime.Now,
            AchievementsUnlocked = saveGame.UnlockedAchievements.Count,
            IsPermadeath = true,
            DifficultyLevel = saveGame.DifficultyLevel
        };
        
        _hallOfFameService.AddEntry(entry);
        
        // Show final statistics
        ShowPermadeathStatistics(player, saveGame, entry);
        
        await Task.Delay(5000);
        
        // Delete save
        _saveGameService.DeleteSave(saveGame.Id);
        
        ConsoleUI.ShowError("Your save file has been deleted.");
        ConsoleUI.ShowInfo($"Your legacy lives on in the Hall of Fame (Score: {entry.GetFameScore()})");
        
        await Task.Delay(3000);
        
        return new HandlePlayerDeathResult
        {
            IsPermadeath = true,
            SaveDeleted = true,
            HallOfFameId = entry.Id
        };
    }
    
    private void ShowPermadeathStatistics(Character player, SaveGame saveGame, HallOfFameEntry entry)
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowBanner($"{player.Name}'s Legacy", "Final Statistics");
        Console.WriteLine();
        
        ConsoleUI.WriteText($"Class: {player.ClassName}");
        ConsoleUI.WriteText($"Level: {player.Level}");
        ConsoleUI.WriteText($"Playtime: {entry.GetPlaytimeFormatted()}");
        ConsoleUI.WriteText($"Enemies Defeated: {saveGame.TotalEnemiesDefeated}");
        ConsoleUI.WriteText($"Quests Completed: {saveGame.QuestsCompleted}");
        ConsoleUI.WriteText($"Locations Discovered: {saveGame.DiscoveredLocations.Count}");
        ConsoleUI.WriteText($"Achievements Unlocked: {saveGame.UnlockedAchievements.Count}");
        Console.WriteLine();
        ConsoleUI.WriteText($"Fame Score: {entry.GetFameScore()}");
    }
}
