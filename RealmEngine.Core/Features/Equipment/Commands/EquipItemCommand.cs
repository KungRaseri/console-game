using MediatR;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Features.Equipment.Commands;

/// <summary>
/// Command to equip an item from inventory.
/// </summary>
public record EquipItemCommand(string CharacterId, string ItemId, EquipmentSlot Slot) : IRequest<EquipItemResult>;

/// <summary>
/// Result of equipping an item.
/// </summary>
public class EquipItemResult
{
    /// <summary>Gets or sets a value indicating whether the equip operation succeeded.</summary>
    public bool Success { get; set; }
    /// <summary>Gets or sets the result message.</summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>Gets or sets the item that was previously equipped in the slot, if any.</summary>
    public Item? PreviousItem { get; set; }
    /// <summary>Gets or sets the list of abilities granted by the equipped item.</summary>
    public List<string> GrantedAbilities { get; set; } = new();
    /// <summary>Gets or sets the list of abilities revoked from the unequipped item.</summary>
    public List<string> RevokedAbilities { get; set; } = new();
}

/// <summary>
/// Equipment slot types.
/// </summary>
public enum EquipmentSlot
{
    /// <summary>Main hand weapon slot.</summary>
    MainHand,
    /// <summary>Off hand weapon or shield slot.</summary>
    OffHand,
    /// <summary>Head armor slot.</summary>
    Head,
    /// <summary>Shoulder armor slot.</summary>
    Shoulders,
    /// <summary>Chest armor slot.</summary>
    Chest,
    /// <summary>Bracer armor slot.</summary>
    Bracers,
    /// <summary>Gloves armor slot.</summary>
    Gloves,
    /// <summary>Belt accessory slot.</summary>
    Belt,
    /// <summary>Leg armor slot.</summary>
    Legs,
    /// <summary>Boot armor slot.</summary>
    Boots,
    /// <summary>Necklace accessory slot.</summary>
    Necklace,
    /// <summary>First ring accessory slot.</summary>
    Ring1,
    /// <summary>Second ring accessory slot.</summary>
    Ring2
}
