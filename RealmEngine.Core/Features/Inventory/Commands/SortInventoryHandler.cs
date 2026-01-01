using MediatR;
using Serilog;

namespace RealmEngine.Core.Features.Inventory.Commands;

/// <summary>
/// Handles the SortInventory command.
/// </summary>
public class SortInventoryHandler : IRequestHandler<SortInventoryCommand, SortInventoryResult>
{
    public Task<SortInventoryResult> Handle(SortInventoryCommand request, CancellationToken cancellationToken)
    {
        var player = request.Player;
        var sortBy = request.SortBy;

        var sortedInventory = sortBy switch
        {
            SortCriteria.Name => player.Inventory.OrderBy(i => i.Name).ToList(),
            SortCriteria.Type => player.Inventory.OrderBy(i => i.Type).ThenBy(i => i.Name).ToList(),
            SortCriteria.Rarity => player.Inventory.OrderByDescending(i => i.Rarity).ThenBy(i => i.Name).ToList(),
            SortCriteria.Value => player.Inventory.OrderByDescending(i => i.Price).ThenBy(i => i.Name).ToList(),
            _ => player.Inventory.OrderBy(i => i.Name).ToList()
        };

        player.Inventory.Clear();
        foreach (var item in sortedInventory)
        {
            player.Inventory.Add(item);
        }

        Log.Information("Player {PlayerName} sorted inventory by {SortCriteria}",
            player.Name, sortBy);

        return Task.FromResult(new SortInventoryResult
        {
            Success = true,
            Message = $"Inventory sorted by {sortBy}"
        });
    }
}