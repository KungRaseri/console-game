using LiteDB;
using Game.Models;

namespace Game.Data;

/// <summary>
/// Repository for managing save game data using LiteDB.
/// </summary>
public class SaveGameRepository : IDisposable
{
    private readonly LiteDatabase _database;
    private readonly ILiteCollection<SaveGame> _collection;

    public SaveGameRepository(string databasePath = "game.db")
    {
        _database = new LiteDatabase(databasePath);
        _collection = _database.GetCollection<SaveGame>("saves");
        
        // Create indexes for better performance
        _collection.EnsureIndex(x => x.PlayerName);
        _collection.EnsureIndex(x => x.SaveDate);
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

    public void Dispose()
    {
        _database?.Dispose();
    }
}
