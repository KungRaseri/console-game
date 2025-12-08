using Game.Models;
using Game.Shared.UI;
using Game.Features.SaveLoad;
using Serilog;

namespace Game.Services;

/// <summary>
/// Handles in-game operations like saving and resting.
/// </summary>
public class GameplayService
{
    private readonly SaveGameService _saveGameService;

    public GameplayService(SaveGameService saveGameService)
    {
        _saveGameService = saveGameService;
    }

    /// <summary>
    /// Rest and recover health and mana to maximum.
    /// </summary>
    public void Rest(Character player)
    {
        if (player == null) return;

        ConsoleUI.ShowInfo("You rest and recover...");

        player.Health = player.MaxHealth;
        player.Mana = player.MaxMana;

        ConsoleUI.ShowSuccess("Fully rested!");
        
        Log.Information("Player {PlayerName} rested", player.Name);
    }

    /// <summary>
    /// Save the current game state.
    /// </summary>
    public void SaveGame(Character player, List<Item> inventory, string? currentSaveId)
    {
        if (player == null)
        {
            ConsoleUI.ShowError("No active game to save!");
            return;
        }

        ConsoleUI.ShowInfo("Saving game...");

        try
        {
            _saveGameService.SaveGame(player, inventory, currentSaveId);
            ConsoleUI.ShowSuccess("Game saved successfully!");
            Log.Information("Game saved for player {PlayerName}", player.Name);
        }
        catch (Exception ex)
        {
            ConsoleUI.ShowError($"Failed to save game: {ex.Message}");
            Log.Error(ex, "Failed to save game");
        }
    }
}
