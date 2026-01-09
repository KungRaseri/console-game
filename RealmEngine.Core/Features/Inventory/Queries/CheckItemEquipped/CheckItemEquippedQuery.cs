using MediatR;

namespace RealmEngine.Core.Features.Inventory.Queries.CheckItemEquipped;

/// <summary>
/// Query to check if a specific item is currently equipped.
/// </summary>
public record CheckItemEquippedQuery : IRequest<ItemEquippedResult>
{
    public required string ItemId { get; init; }
}

/// <summary>
/// Result indicating if an item is equipped and where.
/// </summary>
public record ItemEquippedResult
{
    public bool Success { get; init; }
    public bool IsEquipped { get; init; }
    public string? EquipSlot { get; init; }
    public string? ErrorMessage { get; init; }
}
