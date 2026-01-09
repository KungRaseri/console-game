using RealmEngine.Shared.Models;

namespace RealmEngine.Shared.Abstractions;

/// <summary>
/// Repository interface for managing save game data.
/// </summary>
public interface ISaveGameRepository : IDisposable
{
    /// <summary>Saves a game (alias for SaveGame).</summary>
    /// <param name="saveGame">The save game data.</param>
    void Save(SaveGame saveGame);
    
    /// <summary>Saves a game to the repository.</summary>
    /// <param name="saveGame">The save game data.</param>
    void SaveGame(SaveGame saveGame);
    
    /// <summary>Loads a game from the specified slot.</summary>
    /// <param name="slot">The save slot number.</param>
    /// <returns>The save game data if found; otherwise null.</returns>
    SaveGame? LoadGame(int slot);
    
    /// <summary>Gets a save game by its unique identifier.</summary>
    /// <param name="id">The save game identifier.</param>
    /// <returns>The save game if found; otherwise null.</returns>
    SaveGame? GetById(string id);
    
    /// <summary>Gets the most recently saved game.</summary>
    /// <returns>The most recent save game if any exist; otherwise null.</returns>
    SaveGame? GetMostRecent();
    
    /// <summary>Gets all save games.</summary>
    /// <returns>A list of all save games.</returns>
    List<SaveGame> GetAll();
    
    /// <summary>Gets all save games (alias for GetAll).</summary>
    /// <returns>A list of all save games.</returns>
    List<SaveGame> GetAllSaves();
    
    /// <summary>Gets all save games for a specific player.</summary>
    /// <param name="playerName">The player name to filter by.</param>
    /// <returns>A list of save games for the specified player.</returns>
    List<SaveGame> GetByPlayerName(string playerName);
    
    /// <summary>Deletes a save game by its identifier.</summary>
    /// <param name="id">The save game identifier.</param>
    /// <returns>True if the save was deleted; otherwise false.</returns>
    bool Delete(string id);
    
    /// <summary>Deletes a save game from the specified slot.</summary>
    /// <param name="slot">The save slot number.</param>
    /// <returns>True if the save was deleted; otherwise false.</returns>
    bool DeleteSave(int slot);
    
    /// <summary>Checks if a save exists in the specified slot.</summary>
    /// <param name="slot">The save slot number.</param>
    /// <returns>True if a save exists in the slot; otherwise false.</returns>
    bool SaveExists(int slot);
}
