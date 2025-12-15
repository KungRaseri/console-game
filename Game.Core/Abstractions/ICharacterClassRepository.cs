using Game.Core.Models;

namespace Game.Core.Abstractions;

/// <summary>
/// Repository interface for managing character classes from JSON data.
/// </summary>
public interface ICharacterClassRepository : IDisposable
{
    CharacterClass? GetById(string id);
    CharacterClass? GetByName(string name);
    CharacterClass? GetClassByName(string name); // Alias for GetByName
    List<CharacterClass> GetAll();
    List<CharacterClass> GetAllClasses(); // Alias for GetAll
    void Add(CharacterClass characterClass);
    void Update(CharacterClass characterClass);
    void Delete(string id);
}
