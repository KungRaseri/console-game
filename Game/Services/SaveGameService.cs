using Game.Data;
using Game.Models;
using Serilog;

namespace Game.Services;

/// <summary>
/// Service for managing game saves and loads
/// </summary>
public class SaveGameService : IDisposable
{
    private readonly SaveGameRepository _repository;
    private DateTime _gameStartTime;

    public SaveGameService(string databasePath = "savegames.db")
    {
        _repository = new SaveGameRepository(databasePath);
        _gameStartTime = DateTime.Now;
    }

    /// <summary>
    /// Save the current game state
    /// </summary>
    public void SaveGame(Character player, List<Item> inventory, string? saveId = null)
    {
        try
        {
            var playTime = (int)(DateTime.Now - _gameStartTime).TotalMinutes;

            var saveGame = new SaveGame
            {
                Id = saveId ?? Guid.NewGuid().ToString(),
                PlayerName = player.Name,
                SaveDate = DateTime.Now,
                Character = player,
                Inventory = inventory,
                PlayTimeMinutes = playTime
            };

            _repository.Save(saveGame);
            Log.Information("Game saved for player {PlayerName} (ID: {SaveId})", player.Name, saveGame.Id);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save game for player {PlayerName}", player.Name);
            throw;
        }
    }

    /// <summary>
    /// Load a game by save ID
    /// </summary>
    public SaveGame? LoadGame(string saveId)
    {
        try
        {
            var save = _repository.GetById(saveId);
            if (save != null)
            {
                Log.Information("Game loaded for player {PlayerName} (ID: {SaveId})", save.PlayerName, saveId);
                _gameStartTime = DateTime.Now.AddMinutes(-save.PlayTimeMinutes);
            }
            return save;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load game with ID {SaveId}", saveId);
            throw;
        }
    }

    /// <summary>
    /// Get all available save games
    /// </summary>
    public List<SaveGame> GetAllSaves()
    {
        try
        {
            return _repository.GetAll();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve save games");
            throw;
        }
    }

    /// <summary>
    /// Delete a save game
    /// </summary>
    public bool DeleteSave(string saveId)
    {
        try
        {
            var result = _repository.Delete(saveId);
            if (result)
            {
                Log.Information("Save game deleted (ID: {SaveId})", saveId);
            }
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to delete save with ID {SaveId}", saveId);
            throw;
        }
    }

    /// <summary>
    /// Get the most recent save
    /// </summary>
    public SaveGame? GetMostRecentSave()
    {
        try
        {
            return _repository.GetMostRecent();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve most recent save");
            throw;
        }
    }

    /// <summary>
    /// Check if any saves exist
    /// </summary>
    public bool HasAnySaves()
    {
        try
        {
            return _repository.GetAll().Any();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to check for existing saves");
            return false;
        }
    }

    /// <summary>
    /// Auto-save the current game (overwrites existing save for this character)
    /// </summary>
    public void AutoSave(Character player, List<Item> inventory)
    {
        try
        {
            // Find existing save for this character
            var existingSaves = _repository.GetByPlayerName(player.Name);
            var saveId = existingSaves.FirstOrDefault()?.Id ?? Guid.NewGuid().ToString();

            SaveGame(player, inventory, saveId);
            Log.Information("Auto-save completed for player {PlayerName}", player.Name);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Auto-save failed for player {PlayerName}", player.Name);
            // Don't throw on auto-save failure, just log it
        }
    }

    public void Dispose()
    {
        _repository?.Dispose();
    }
}
