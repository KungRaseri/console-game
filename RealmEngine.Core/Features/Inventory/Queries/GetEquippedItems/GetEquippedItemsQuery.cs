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
    /// <summary>Gets a value indicating whether the query succeeded.</summary>
    public bool Success { get; init; }
    /// <summary>Gets the equipment loadout.</summary>
    public EquipmentLoadout? Equipment { get; init; }
    /// <summary>Gets the equipment statistics.</summary>
    public EquipmentStats? Stats { get; init; }
    /// <summary>Gets the error message if query failed.</summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Complete equipment loadout.
/// </summary>
public record EquipmentLoadout
{
    /// <summary>Gets the main hand item.</summary>
    public Item? MainHand { get; init; }
    /// <summary>Gets the off hand item.</summary>
    public Item? OffHand { get; init; }
    /// <summary>Gets the helm item.</summary>
    public Item? Helm { get; init; }
    /// <summary>Gets the shoulders item.</summary>
    public Item? Shoulders { get; init; }
    /// <summary>Gets the chest item.</summary>
    public Item? Chest { get; init; }
    /// <summary>Gets the bracers item.</summary>
    public Item? Bracers { get; init; }
    /// <summary>Gets the gloves item.</summary>
    public Item? Gloves { get; init; }
    /// <summary>Gets the belt item.</summary>
    public Item? Belt { get; init; }
    /// <summary>Gets the legs item.</summary>
    public Item? Legs { get; init; }
    /// <summary>Gets the boots item.</summary>
    public Item? Boots { get; init; }
    /// <summary>Gets the necklace item.</summary>
    public Item? Necklace { get; init; }
    /// <summary>Gets the first ring item.</summary>
    public Item? Ring1 { get; init; }
    /// <summary>Gets the second ring item.</summary>
    public Item? Ring2 { get; init; }
}

/// <summary>
/// Aggregated stats from all equipped items.
/// </summary>
public record EquipmentStats
{
    /// <summary>Gets the total number of equipped items.</summary>
    public int TotalEquippedItems { get; init; }
    /// <summary>Gets the total value of equipped items.</summary>
    public int TotalValue { get; init; }
    /// <summary>Gets the total attack bonus from equipment.</summary>
    public int TotalAttackBonus { get; init; }
    /// <summary>Gets the total defense bonus from equipment.</summary>
    public int TotalDefenseBonus { get; init; }
    /// <summary>Gets the total number of sockets.</summary>
    public int TotalSockets { get; init; }
    /// <summary>Gets the number of filled sockets.</summary>
    public int FilledSockets { get; init; }
    /// <summary>Gets the set bonuses from equipped items.</summary>
    public Dictionary<string, int> SetBonuses { get; init; } = new();
}
