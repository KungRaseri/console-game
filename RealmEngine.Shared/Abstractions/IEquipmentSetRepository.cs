using RealmEngine.Shared.Models;

namespace RealmEngine.Shared.Abstractions;

/// <summary>
/// Repository interface for managing equipment sets from JSON data.
/// </summary>
public interface IEquipmentSetRepository : IDisposable
{
    /// <summary>Gets an equipment set by its unique identifier.</summary>
    /// <param name="id">The equipment set identifier.</param>
    /// <returns>The equipment set if found; otherwise null.</returns>
    EquipmentSet? GetById(string id);
    
    /// <summary>Gets an equipment set by its name.</summary>
    /// <param name="name">The equipment set name.</param>
    /// <returns>The equipment set if found; otherwise null.</returns>
    EquipmentSet? GetByName(string name);
    
    /// <summary>Gets all equipment sets.</summary>
    /// <returns>A list of all equipment sets.</returns>
    List<EquipmentSet> GetAll();
    
    /// <summary>Adds a new equipment set to the repository.</summary>
    /// <param name="equipmentSet">The equipment set to add.</param>
    void Add(EquipmentSet equipmentSet);
    
    /// <summary>Updates an existing equipment set in the repository.</summary>
    /// <param name="equipmentSet">The equipment set to update.</param>
    void Update(EquipmentSet equipmentSet);
    
    /// <summary>Deletes an equipment set from the repository.</summary>
    /// <param name="id">The identifier of the equipment set to delete.</param>
    void Delete(string id);
}
