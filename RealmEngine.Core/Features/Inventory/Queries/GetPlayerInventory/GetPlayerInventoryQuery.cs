using MediatR;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Features.Inventory.Queries.GetPlayerInventory;

/// <summary>
/// Query to get player's inventory with optional filtering and sorting.
/// </summary>
public record GetPlayerInventoryQuery : IRequest<InventoryQueryResult>
{
    /// <summary>Gets the item type filter.</summary>
    public string? ItemTypeFilter { get; init; }
    /// <summary>Gets the rarity filter.</summary>
    public string? RarityFilter { get; init; }
    /// <summary>Gets the minimum value filter.</summary>
    public int? MinValue { get; init; }
    /// <summary>Gets the maximum value filter.</summary>
    public int? MaxValue { get; init; }
    /// <summary>Gets the sort field (name, value, rarity, type).</summary>
    public string? SortBy { get; init; }
    /// <summary>Gets a value indicating whether to sort in descending order.</summary>
    public bool SortDescending { get; init; }
}

/// <summary>
/// Result containing filtered and sorted inventory items.
/// </summary>
public record InventoryQueryResult
{
    /// <summary>Gets a value indicating whether the query succeeded.</summary>
    public bool Success { get; init; }
    /// <summary>Gets the filtered inventory items.</summary>
    public List<InventoryItemInfo>? Items { get; init; }
    /// <summary>Gets the inventory summary statistics.</summary>
    public InventorySummary? Summary { get; init; }
    /// <summary>Gets the error message if query failed.</summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Information about a single inventory item.
/// </summary>
public record InventoryItemInfo
{
    /// <summary>Gets the item ID.</summary>
    public string Id { get; init; } = string.Empty;
    /// <summary>Gets the item name.</summary>
    public string Name { get; init; } = string.Empty;
    /// <summary>Gets the item type.</summary>
    public string ItemType { get; init; } = string.Empty;
    /// <summary>Gets the item rarity.</summary>
    public string Rarity { get; init; } = string.Empty;
    /// <summary>Gets the item value.</summary>
    public int Value { get; init; }
    /// <summary>Gets the item quantity.</summary>
    public int Quantity { get; init; }
    /// <summary>Gets the total value (value * quantity).</summary>
    public int TotalValue { get; init; }
    /// <summary>Gets a value indicating whether the item is equipped.</summary>
    public bool IsEquipped { get; init; }
    /// <summary>Gets a value indicating whether the item can have sockets.</summary>
    public bool IsSocketable { get; init; }
    /// <summary>Gets the number of sockets in the item.</summary>
    public int? SocketCount { get; init; }
    /// <summary>Gets the item prefixes.</summary>
    public List<string> Prefixes { get; init; } = new();
    /// <summary>Gets the item suffixes.</summary>
    public List<string> Suffixes { get; init; } = new();
}

/// <summary>
/// Summary statistics about the inventory.
/// </summary>
public record InventorySummary
{
    /// <summary>Gets the total number of items.</summary>
    public int TotalItems { get; init; }
    /// <summary>Gets the number of unique items.</summary>
    public int UniqueItems { get; init; }
    /// <summary>Gets the total value of all items.</summary>
    public int TotalValue { get; init; }
    /// <summary>Gets the number of equipped items.</summary>
    public int EquippedItems { get; init; }
    /// <summary>Gets the items grouped by type.</summary>
    public Dictionary<string, int> ItemsByType { get; init; } = new();
    /// <summary>Gets the items grouped by rarity.</summary>
    public Dictionary<string, int> ItemsByRarity { get; init; } = new();
}
