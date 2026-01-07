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
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Item? PreviousItem { get; set; }
    public List<string> GrantedAbilities { get; set; } = new();
    public List<string> RevokedAbilities { get; set; } = new();
}

/// <summary>
/// Equipment slot types.
/// </summary>
public enum EquipmentSlot
{
    MainHand,
    OffHand,
    Head,
    Shoulders,
    Chest,
    Bracers,
    Gloves,
    Belt,
    Legs,
    Boots,
    Necklace,
    Ring1,
    Ring2
}
