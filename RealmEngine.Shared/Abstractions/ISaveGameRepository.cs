using RealmEngine.Shared.Models;

namespace RealmEngine.Shared.Abstractions;

/// <summary>
/// Repository interface for managing save game data.
/// </summary>
public interface ISaveGameRepository : IDisposable
{
    void Save(SaveGame saveGame); // Alias for SaveGame
    void SaveGame(SaveGame saveGame);
    SaveGame? LoadGame(int slot);
    SaveGame? GetById(string id);
    SaveGame? GetMostRecent();
    List<SaveGame> GetAll();
    List<SaveGame> GetAllSaves();
    List<SaveGame> GetByPlayerName(string playerName);
    bool Delete(string id);
    bool DeleteSave(int slot);
    bool SaveExists(int slot);
}
