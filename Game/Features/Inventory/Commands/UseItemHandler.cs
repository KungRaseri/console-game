using Game.Models;
using Game.Features.Inventory;
using MediatR;
using Serilog;

namespace Game.Features.Inventory.Commands;

/// <summary>
/// Handles the UseItem command.
/// </summary>
public class UseItemHandler : IRequestHandler<UseItemCommand, UseItemResult>
{
    private readonly InventoryService _inventoryService;

    public UseItemHandler(InventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public async Task<UseItemResult> Handle(UseItemCommand request, CancellationToken cancellationToken)
    {
        var player = request.Player;
        var item = request.Item;

        // Use inventory service to use item
        var success = await _inventoryService.UseItemAsync(item, player, player.Name);

        if (!success)
        {
            return new UseItemResult
            {
                Success = false,
                Message = $"Could not use {item.Name}",
                HealthRestored = 0,
                ManaRestored = 0
            };
        }

        Log.Information("Player {PlayerName} used {ItemName}",
            player.Name, item.Name);

        return new UseItemResult
        {
            Success = true,
            Message = $"Used {item.Name}",
            HealthRestored = 0,  // TODO: Return actual values from service
            ManaRestored = 0
        };
    }
}
