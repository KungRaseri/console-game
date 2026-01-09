using MediatR;
using Microsoft.Extensions.Logging;
using RealmEngine.Core.Features.SaveLoad;
using RealmEngine.Core.Services;

namespace RealmEngine.Core.Features.Shop.Queries;

/// <summary>
/// Handler for CheckAffordabilityQuery.
/// Checks if player has enough gold to buy an item.
/// </summary>
public class CheckAffordabilityQueryHandler : IRequestHandler<CheckAffordabilityQuery, CheckAffordabilityResult>
{
    private readonly ISaveGameService _saveGameService;
    private readonly ShopEconomyService _shopService;
    private readonly ILogger<CheckAffordabilityQueryHandler> _logger;

    public CheckAffordabilityQueryHandler(
        ISaveGameService saveGameService,
        ShopEconomyService shopService,
        ILogger<CheckAffordabilityQueryHandler> logger)
    {
        _saveGameService = saveGameService;
        _shopService = shopService;
        _logger = logger;
    }

    public Task<CheckAffordabilityResult> Handle(CheckAffordabilityQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var saveGame = _saveGameService.GetCurrentSave();
            if (saveGame == null)
            {
                return Task.FromResult(new CheckAffordabilityResult(
                    false, false, 0, 0, 0,
                    ErrorMessage: "No active game session"
                ));
            }

            var merchant = saveGame.KnownNPCs.FirstOrDefault(n => n.Id == request.MerchantId);
            if (merchant == null)
            {
                return Task.FromResult(new CheckAffordabilityResult(
                    false, false, 0, 0, 0,
                    ErrorMessage: "Merchant not found"
                ));
            }

            var inventory = _shopService.GetOrCreateInventory(merchant);
            var item = inventory.CoreItems.FirstOrDefault(i => i.Id == request.ItemId)
                ?? inventory.DynamicItems.FirstOrDefault(i => i.Id == request.ItemId)
                ?? inventory.PlayerSoldItems.FirstOrDefault(i => i.Item.Id == request.ItemId)?.Item;

            if (item == null)
            {
                return Task.FromResult(new CheckAffordabilityResult(
                    false, false, 0, 0, 0,
                    ErrorMessage: "Item not found in merchant inventory"
                ));
            }

            var itemPrice = _shopService.CalculateSellPrice(item, merchant);
            var playerGold = saveGame.Character.Gold;
            var canAfford = playerGold >= itemPrice;
            var shortfall = canAfford ? 0 : itemPrice - playerGold;

            _logger.LogInformation("Affordability check: {ItemName} costs {Price}g, player has {Gold}g (can afford: {CanAfford})",
                item.Name, itemPrice, playerGold, canAfford);

            return Task.FromResult(new CheckAffordabilityResult(
                Success: true,
                CanAfford: canAfford,
                ItemPrice: itemPrice,
                PlayerGold: playerGold,
                GoldShortfall: shortfall,
                ItemName: item.Name
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking affordability for item {ItemId} at merchant {MerchantId}",
                request.ItemId, request.MerchantId);
            return Task.FromResult(new CheckAffordabilityResult(
                false, false, 0, 0, 0,
                ErrorMessage: $"Failed to check affordability: {ex.Message}"
            ));
        }
    }
}
