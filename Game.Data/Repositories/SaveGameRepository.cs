using LiteDB;
using Game.Core.Models;
using Game.Core.Abstractions;

namespace Game.Data.Repositories;

/// <summary>
/// Repository for managing save game data using LiteDB.
/// </summary>
public class SaveGameRepository : ISaveGameRepository
{
    private readonly LiteDatabase _database;
    private readonly ILiteCollection<SaveGame> _collection;

    public SaveGameRepository(string databasePath = "savegames.db")
    {
        _database = new LiteDatabase(databasePath);
        _collection = _database.GetCollection<SaveGame>("saves");
        
        // Create indexes for better performance
        // Use string-based indexing to avoid BsonMapper issues with complex types
        try
        {
            _collection.EnsureIndex("PlayerName");
            _collection.EnsureIndex("SaveDate");
        }
        catch (NotSupportedException)
        {
            // If indexing fails due to complex types, continue without indexes
            // Performance will be slightly worse but functionality remains intact
        }
    }

    /// <summary>
    /// Save or update a game save.
    /// </summary>
    public void Save(SaveGame saveGame)
    {
        _collection.Upsert(saveGame);
    }

    /// <summary>
    /// Get all saved games.
    /// </summary>
    public List<SaveGame> GetAll()
    {
        return _collection.FindAll().ToList();
    }

    /// <summary>
    /// Get a save game by ID.
    /// </summary>
    public SaveGame? GetById(string id)
    {
        return _collection.FindById(id);
    }

    /// <summary>
    /// Get save games by player name.
    /// </summary>
    public List<SaveGame> GetByPlayerName(string playerName)
    {
        return _collection.Find(x => x.PlayerName == playerName).ToList();
    }

    /// <summary>
    /// Delete a save game.
    /// </summary>
    public bool Delete(string id)
    {
        return _collection.Delete(id);
    }

    /// <summary>
    /// Get the most recent save game.
    /// </summary>
    public SaveGame? GetMostRecent()
    {
        return _collection
            .Find(Query.All(Query.Descending))
            .OrderByDescending(x => x.SaveDate)
            .FirstOrDefault();
    }

    // Additional interface methods (aliases for consistency)
    public void SaveGame(SaveGame saveGame) => Save(saveGame);
    public SaveGame? LoadGame(int id) => GetById(id.ToString());
    public List<SaveGame> GetAllSaves() => GetAll();
    public bool DeleteSave(int id) => Delete(id.ToString());
    public bool SaveExists(int id) => GetById(id.ToString()) != null;

    public void Dispose()
    {
        _database?.Dispose();
    }
}
