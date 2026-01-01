using Game.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.Inventory.Queries;

/// <summary>
/// Query to get all items in player's inventory.
/// </summary>
public record GetInventoryItemsQuery : IRequest<GetInventoryItemsResult>
{
    public required Character Player { get; init; }
}

/// <summary>
/// Result containing inventory items.
/// </summary>
public record GetInventoryItemsResult
{
    public required List<Item> Items { get; init; }
    public required int TotalItems { get; init; }
}