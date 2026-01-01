using MediatR;
using Serilog;

namespace Game.Core.Features.Inventory.Commands;

/// <summary>
/// Handles the DropItem command.
/// </summary>
public class DropItemHandler : IRequestHandler<DropItemCommand, DropItemResult>
{
    public Task<DropItemResult> Handle(DropItemCommand request, CancellationToken cancellationToken)
    {
        var player = request.Player;
        var item = request.Item;

        // Remove item from inventory
        var removed = player.Inventory.Remove(item);

        if (removed)
        {
            Log.Information("Player {PlayerName} dropped {ItemName}",
                player.Name, item.Name);

            return Task.FromResult(new DropItemResult
            {
                Success = true,
                Message = $"Dropped {item.Name}"
            });
        }

        return Task.FromResult(new DropItemResult
        {
            Success = false,
            Message = "Item not found in inventory"
        });
    }
}