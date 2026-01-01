using Game.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.Death.Queries;

/// <summary>
/// Query to get dropped items at a specific location.
/// </summary>
public record GetDroppedItemsQuery : IRequest<GetDroppedItemsResult>
{
    public required string Location { get; init; }
}

/// <summary>
/// Result containing dropped items at the location.
/// </summary>
public record GetDroppedItemsResult
{
    public List<Item> Items { get; init; } = new();
    public bool HasItems => Items.Count > 0;
}