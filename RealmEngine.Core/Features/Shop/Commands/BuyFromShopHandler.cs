using MediatR;
using Microsoft.Extensions.Logging;
using RealmEngine.Core.Features.SaveLoad;
using RealmEngine.Core.Services;

namespace RealmEngine.Core.Features.Shop.Commands;

/// <summary>
/// Handler for buying items from a merchant (player purchases).
/// </summary>
public class BuyFromShopHandler : IRequestHandler<BuyFromShopCommand, BuyFromShopResult>
{
    private readonly ShopEconomyService _shopService;
    private readonly ISaveGameService _saveGameService;
    private readonly ILogger<BuyFromShopHandler> _logger;

    public BuyFromShopHandler(
        ShopEconomyService shopService,
        ISaveGameService saveGameService,
        ILogger<BuyFromShopHandler> logger)
    {
        _shopService = shopService ?? throw new ArgumentNullException(nameof(shopService));
        _saveGameService = saveGameService ?? throw new ArgumentNullException(nameof(saveGameService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<BuyFromShopResult> Handle(BuyFromShopCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var saveGame = _saveGameService.GetCurrentSave();
            if (saveGame == null)
            {
                return Task.FromResult(new BuyFromShopResult
                {
                    Success = false,
                    ErrorMessage = "No active game session"
                });
            }

            // Find merchant NPC
            var merchant = saveGame.KnownNPCs?.FirstOrDefault(n => n.Id == request.MerchantId);
            if (merchant == null)
            {
                return Task.FromResult(new BuyFromShopResult
                {
                    Success = false,
                    ErrorMessage = "Merchant not found"
                });
            }

            // Get shop inventory
            var inventory = _shopService.GetOrCreateInventory(merchant);

            // Find the item in merchant inventory
            var item = inventory.CoreItems.FirstOrDefault(i => i.Id == request.ItemId)
                    ?? inventory.DynamicItems.FirstOrDefault(i => i.Id == request.ItemId)
                    ?? inventory.PlayerSoldItems.FirstOrDefault(pi => pi.Item.Id == request.ItemId)?.Item;

            if (item == null)
            {
                return Task.FromResult(new BuyFromShopResult
                {
                    Success = false,
                    ErrorMessage = "Item not found in shop"
                });
            }

            // Calculate price
            var price = _shopService.CalculateSellPrice(item, merchant);

            // Check if player can afford it
            if (saveGame.Character.Gold < price)
            {
                return Task.FromResult(new BuyFromShopResult
                {
                    Success = false,
                    ErrorMessage = $"Not enough gold. Need {price}g, have {saveGame.Character.Gold}g"
                });
            }

            // Process the transaction
            if (!_shopService.SellToPlayer(merchant, item, out var priceCharged))
            {
                return Task.FromResult(new BuyFromShopResult
                {
                    Success = false,
                    ErrorMessage = "Transaction failed"
                });
            }

            // Update player inventory and gold
            saveGame.Character.Gold -= priceCharged;
            merchant.Gold += priceCharged;
            saveGame.Character.Inventory.Add(item);

            // Save the game state
            _saveGameService.SaveGame(saveGame);

            _logger.LogInformation("Player bought {ItemName} from {MerchantName} for {Price}g",
                item.Name, merchant.Name, priceCharged);

            return Task.FromResult(new BuyFromShopResult
            {
                Success = true,
                ItemPurchased = item,
                PriceCharged = priceCharged,
                PlayerGoldRemaining = saveGame.Character.Gold
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error buying item {ItemId} from merchant {MerchantId}",
                request.ItemId, request.MerchantId);
            return Task.FromResult(new BuyFromShopResult
            {
                Success = false,
                ErrorMessage = $"Transaction error: {ex.Message}"
            });
        }
    }
}
