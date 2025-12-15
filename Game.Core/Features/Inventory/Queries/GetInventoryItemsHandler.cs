using MediatR;

namespace Game.Core.Features.Inventory.Queries;

/// <summary>
/// Handles the GetInventoryItems query.
/// </summary>
public class GetInventoryItemsHandler : IRequestHandler<GetInventoryItemsQuery, GetInventoryItemsResult>
{
    public Task<GetInventoryItemsResult> Handle(GetInventoryItemsQuery request, CancellationToken cancellationToken)
    {
        var player = request.Player;

        var result = new GetInventoryItemsResult
        {
            Items = player.Inventory.ToList(),
            TotalItems = player.Inventory.Count
        };

        return Task.FromResult(result);
    }
}
