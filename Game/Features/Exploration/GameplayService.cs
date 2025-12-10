using Game.Models;
using Game.Shared.UI;
using Game.Features.SaveLoad;
using Serilog;

namespace Game.Features.Exploration;

/// <summary>
/// Handles in-game operations like saving and resting.
/// </summary>
public class GameplayService
{
    private readonly SaveGameService _saveGameService;
    private readonly IConsoleUI _console;

    public GameplayService(SaveGameService saveGameService, IConsoleUI console)
    {
        _saveGameService = saveGameService;
        _console = console;
    }

    /// <summary>
    /// Rest and recover health and mana to maximum.
    /// </summary>
    public void Rest(Character player)
    {
        if (player == null) return;

        _console.ShowInfo("You rest and recover...");

        player.Health = player.MaxHealth;
        player.Mana = player.MaxMana;

        _console.ShowSuccess("Fully rested!");
        
        Log.Information("Player {PlayerName} rested", player.Name);
    }

    /// <summary>
    /// Save the current game state.
    /// </summary>
    public void SaveGame(Character player, List<Item> inventory, string? currentSaveId)
    {
        if (player == null)
        {
            _console.ShowError("No active game to save!");
            return;
        }

        _console.ShowInfo("Saving game...");

        try
        {
            _saveGameService.SaveGame(player, inventory, currentSaveId);
            _console.ShowSuccess("Game saved successfully!");
            Log.Information("Game saved for player {PlayerName}", player.Name);
        }
        catch (Exception ex)
        {
            _console.ShowError($"Failed to save game: {ex.Message}");
            Log.Error(ex, "Failed to save game");
        }
    }
}
