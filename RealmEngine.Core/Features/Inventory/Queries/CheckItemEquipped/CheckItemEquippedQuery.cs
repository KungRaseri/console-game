using MediatR;

namespace RealmEngine.Core.Features.Inventory.Queries.CheckItemEquipped;

/// <summary>
/// Query to check if a specific item is currently equipped.
/// </summary>
public record CheckItemEquippedQuery : IRequest<ItemEquippedResult>
{
    /// <summary>Gets the ID of the item to check.</summary>
    public required string ItemId { get; init; }
}

/// <summary>
/// Result indicating if an item is equipped and where.
/// </summary>
public record ItemEquippedResult
{
    /// <summary>Gets a value indicating whether the query succeeded.</summary>
    public bool Success { get; init; }
    /// <summary>Gets a value indicating whether the item is equipped.</summary>
    public bool IsEquipped { get; init; }
    /// <summary>Gets the equipment slot name if equipped.</summary>
    public string? EquipSlot { get; init; }
    /// <summary>Gets the error message if query failed.</summary>
    public string? ErrorMessage { get; init; }
}
