using Game.Shared.Models;

namespace Game.Shared.Abstractions;

/// <summary>
/// Repository interface for managing equipment sets from JSON data.
/// </summary>
public interface IEquipmentSetRepository : IDisposable
{
    EquipmentSet? GetById(string id);
    EquipmentSet? GetByName(string name);
    List<EquipmentSet> GetAll();
    void Add(EquipmentSet equipmentSet);
    void Update(EquipmentSet equipmentSet);
    void Delete(string id);
}
