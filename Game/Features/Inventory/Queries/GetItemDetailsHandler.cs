using MediatR;

namespace Game.Features.Inventory.Queries;

/// <summary>
/// Handles the GetItemDetails query.
/// </summary>
public class GetItemDetailsHandler : IRequestHandler<GetItemDetailsQuery, GetItemDetailsResult>
{
    public Task<GetItemDetailsResult> Handle(GetItemDetailsQuery request, CancellationToken cancellationToken)
    {
        var item = request.Item;

        var result = new GetItemDetailsResult
        {
            Name = item.Name,
            Description = item.Description,
            Type = item.Type,
            Rarity = item.Rarity,
            Price = item.Price,
            Traits = item.Traits,
            UpgradeLevel = item.UpgradeLevel,
            Enchantments = item.Enchantments,
            SetName = item.SetName
        };

        return Task.FromResult(result);
    }
}
