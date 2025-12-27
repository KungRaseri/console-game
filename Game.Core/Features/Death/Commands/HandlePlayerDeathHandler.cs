using Game.Core.Features.SaveLoad;
using Game.Core.Models;
using Game.Core.Abstractions;
using MediatR;
using Serilog;

namespace Game.Core.Features.Death.Commands;

/// <summary>
/// Handles player death with difficulty-appropriate penalties.
/// </summary>
public class HandlePlayerDeathHandler : IRequestHandler<HandlePlayerDeathCommand, HandlePlayerDeathResult>
{
    private readonly DeathService _deathService;
    private readonly SaveGameService _saveGameService;
    private readonly IHallOfFameRepository _hallOfFameService;
    private readonly IGameUI _console;

    public HandlePlayerDeathHandler(
        DeathService deathService,
        SaveGameService saveGameService,
        IHallOfFameRepository IHallOfFameRepository,
        IGameUI console)
    {
        _deathService = deathService;
        _saveGameService = saveGameService;
        _hallOfFameService = IHallOfFameRepository;
        _console = console;
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
        _console.Clear();
        _console.ShowError("═══════════════════════════════════════");
        _console.ShowError("           YOU HAVE DIED               ");
        _console.ShowError("═══════════════════════════════════════");
        await Task.Delay(2000);

        Console.WriteLine();
        _console.WriteText($"You died at: {location}");
        _console.WriteText($"Death count: {saveGame.DeathCount}");
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
        _console.ShowError($"Penalties:");
        if (goldLost > 0)
            _console.WriteText($"  • Lost {goldLost} gold");
        if (xpLost > 0)
            _console.WriteText($"  • Lost {xpLost} XP");
        if (droppedItems.Count > 0)
        {
            if (difficulty.DropAllInventoryOnDeath)
                _console.WriteText($"  • Dropped ALL {droppedItems.Count} items at {location}");
            else
                _console.WriteText($"  • Dropped {droppedItems.Count} item(s) at {location}");
        }

        Console.WriteLine();

        // Respawn
        player.Health = player.MaxHealth;
        player.Mana = player.MaxMana;

        _console.ShowSuccess("You have respawned at Hub Town with full health!");
        _console.ShowInfo("Return to your death location to recover dropped items.");

        await Task.Delay(1500);

        // Auto-save in Ironman mode
        if (difficulty.AutoSaveOnly)
        {
            _saveGameService.SaveGame(saveGame);
            _console.ShowInfo("Game auto-saved (Ironman mode)");
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
        _console.Clear();
        _console.ShowError("═══════════════════════════════════════");
        _console.ShowError("          PERMADEATH                   ");
        _console.ShowError("═══════════════════════════════════════");
        await Task.Delay(2000);

        Console.WriteLine();
        _console.ShowError($"{player.Name} has fallen.");
        _console.WriteText($"Location: {location}");
        if (killer != null)
            _console.WriteText($"Slain by: {killer.Name}");
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

        await Task.Delay(2500);

        // Delete save
        _saveGameService.DeleteSave(saveGame.Id);

        _console.ShowError("Your save file has been deleted.");
        _console.ShowInfo($"Your legacy lives on in the Hall of Fame (Score: {entry.GetFameScore()})");

        await Task.Delay(1500);

        return new HandlePlayerDeathResult
        {
            IsPermadeath = true,
            SaveDeleted = true,
            HallOfFameId = entry.Id
        };
    }

    private void ShowPermadeathStatistics(Character player, SaveGame saveGame, HallOfFameEntry entry)
    {
        _console.Clear();
        _console.ShowBanner($"{player.Name}'s Legacy", "Final Statistics");
        Console.WriteLine();

        _console.WriteText($"Class: {player.ClassName}");
        _console.WriteText($"Level: {player.Level}");
        _console.WriteText($"Playtime: {entry.GetPlaytimeFormatted()}");
        _console.WriteText($"Enemies Defeated: {saveGame.TotalEnemiesDefeated}");
        _console.WriteText($"Quests Completed: {saveGame.QuestsCompleted}");
        _console.WriteText($"Locations Discovered: {saveGame.DiscoveredLocations.Count}");
        _console.WriteText($"Achievements Unlocked: {saveGame.UnlockedAchievements.Count}");
        Console.WriteLine();
        _console.WriteText($"Fame Score: {entry.GetFameScore()}");
    }
}
