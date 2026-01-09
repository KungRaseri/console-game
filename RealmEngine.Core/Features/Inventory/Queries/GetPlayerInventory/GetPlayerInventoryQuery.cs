using MediatR;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Features.Inventory.Queries.GetPlayerInventory;

/// <summary>
/// Query to get player's inventory with optional filtering and sorting.
/// </summary>
public record GetPlayerInventoryQuery : IRequest<InventoryQueryResult>
{
    public string? ItemTypeFilter { get; init; }
    public string? RarityFilter { get; init; }
    public int? MinValue { get; init; }
    public int? MaxValue { get; init; }
    public string? SortBy { get; init; } // "name", "value", "rarity", "type"
    public bool SortDescending { get; init; }
}

/// <summary>
/// Result containing filtered and sorted inventory items.
/// </summary>
public record InventoryQueryResult
{
    public bool Success { get; init; }
    public List<InventoryItemInfo>? Items { get; init; }
    public InventorySummary? Summary { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Information about a single inventory item.
/// </summary>
public record InventoryItemInfo
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string ItemType { get; init; } = string.Empty;
    public string Rarity { get; init; } = string.Empty;
    public int Value { get; init; }
    public int Quantity { get; init; }
    public int TotalValue { get; init; }
    public bool IsEquipped { get; init; }
    public bool IsSocketable { get; init; }
    public int? SocketCount { get; init; }
    public List<string> Prefixes { get; init; } = new();
    public List<string> Suffixes { get; init; } = new();
}

/// <summary>
/// Summary statistics about the inventory.
/// </summary>
public record InventorySummary
{
    public int TotalItems { get; init; }
    public int UniqueItems { get; init; }
    public int TotalValue { get; init; }
    public int EquippedItems { get; init; }
    public Dictionary<string, int> ItemsByType { get; init; } = new();
    public Dictionary<string, int> ItemsByRarity { get; init; } = new();
}
