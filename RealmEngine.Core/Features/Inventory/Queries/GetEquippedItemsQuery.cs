using Game.Shared.Models;
using MediatR;

namespace Game.Core.Features.Inventory.Queries;

/// <summary>
/// Query to get all equipped items on player.
/// </summary>
public record GetEquippedItemsQuery : IRequest<GetEquippedItemsResult>
{
    public required Character Player { get; init; }
}

/// <summary>
/// Result containing equipped items.
/// </summary>
public record GetEquippedItemsResult
{
    public required Dictionary<string, Item?> EquippedItems { get; init; }
}