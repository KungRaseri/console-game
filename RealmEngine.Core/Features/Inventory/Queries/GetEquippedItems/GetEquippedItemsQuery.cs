using MediatR;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Features.Inventory.Queries.GetEquippedItems;

/// <summary>
/// Query to get all currently equipped items on the character.
/// </summary>
public record GetEquippedItemsQuery : IRequest<EquippedItemsResult>;

/// <summary>
/// Result containing all equipped items.
/// </summary>
public record EquippedItemsResult
{
    public bool Success { get; init; }
    public EquipmentLoadout? Equipment { get; init; }
    public EquipmentStats? Stats { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Complete equipment loadout.
/// </summary>
public record EquipmentLoadout
{
    public Item? MainHand { get; init; }
    public Item? OffHand { get; init; }
    public Item? Helm { get; init; }
    public Item? Shoulders { get; init; }
    public Item? Chest { get; init; }
    public Item? Bracers { get; init; }
    public Item? Gloves { get; init; }
    public Item? Belt { get; init; }
    public Item? Legs { get; init; }
    public Item? Boots { get; init; }
    public Item? Necklace { get; init; }
    public Item? Ring1 { get; init; }
    public Item? Ring2 { get; init; }
}

/// <summary>
/// Aggregated stats from all equipped items.
/// </summary>
public record EquipmentStats
{
    public int TotalEquippedItems { get; init; }
    public int TotalValue { get; init; }
    public int TotalAttackBonus { get; init; }
    public int TotalDefenseBonus { get; init; }
    public int TotalSockets { get; init; }
    public int FilledSockets { get; init; }
    public Dictionary<string, int> SetBonuses { get; init; } = new();
}
