using MediatR;
using Microsoft.Extensions.Logging;
using RealmEngine.Core.Features.SaveLoad;
using RealmEngine.Core.Services;

namespace RealmEngine.Core.Features.Shop.Commands;

/// <summary>
/// Handler for selling items to a merchant (player sells).
/// </summary>
public class SellToShopHandler : IRequestHandler<SellToShopCommand, SellToShopResult>
{
    private readonly ShopEconomyService _shopService;
    private readonly ISaveGameService _saveGameService;
    private readonly ILogger<SellToShopHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SellToShopHandler"/> class.
    /// </summary>
    /// <param name="shopService">The shop economy service.</param>
    /// <param name="saveGameService">The save game service.</param>
    /// <param name="logger">The logger.</param>
    public SellToShopHandler(
        ShopEconomyService shopService,
        ISaveGameService saveGameService,
        ILogger<SellToShopHandler> logger)
    {
        _shopService = shopService ?? throw new ArgumentNullException(nameof(shopService));
        _saveGameService = saveGameService ?? throw new ArgumentNullException(nameof(saveGameService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the SellToShopCommand and returns the result.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<SellToShopResult> Handle(SellToShopCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var saveGame = _saveGameService.GetCurrentSave();
            if (saveGame == null)
            {
                return Task.FromResult(new SellToShopResult
                {
                    Success = false,
                    ErrorMessage = "No active game session"
                });
            }

            // Find merchant NPC
            var merchant = saveGame.KnownNPCs?.FirstOrDefault(n => n.Id == request.MerchantId);
            if (merchant == null)
            {
                return Task.FromResult(new SellToShopResult
                {
                    Success = false,
                    ErrorMessage = "Merchant not found"
                });
            }

            // Find the item in player inventory
            var item = saveGame.Character.Inventory.FirstOrDefault(i => i.Id == request.ItemId);
            if (item == null)
            {
                return Task.FromResult(new SellToShopResult
                {
                    Success = false,
                    ErrorMessage = "Item not found in your inventory"
                });
            }

            // Check if item is equipped
            if (saveGame.Character.EquippedMainHand?.Id == item.Id ||
                saveGame.Character.EquippedOffHand?.Id == item.Id)
            {
                return Task.FromResult(new SellToShopResult
                {
                    Success = false,
                    ErrorMessage = "Cannot sell equipped items. Unequip first."
                });
            }

            // Process the transaction
            if (!_shopService.BuyFromPlayer(merchant, item, out var pricePaid))
            {
                return Task.FromResult(new SellToShopResult
                {
                    Success = false,
                    ErrorMessage = "Merchant cannot buy this item (not enough gold or doesn't accept player items)"
                });
            }

            // Update player inventory and gold
            saveGame.Character.Inventory.Remove(item);
            saveGame.Character.Gold += pricePaid;
            merchant.Gold -= pricePaid;

            // Save the game state
            _saveGameService.SaveGame(saveGame);

            _logger.LogInformation("Player sold {ItemName} to {MerchantName} for {Price}g",
                item.Name, merchant.Name, pricePaid);

            return Task.FromResult(new SellToShopResult
            {
                Success = true,
                ItemSold = item,
                PriceReceived = pricePaid,
                PlayerGoldRemaining = saveGame.Character.Gold
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selling item {ItemId} to merchant {MerchantId}",
                request.ItemId, request.MerchantId);
            return Task.FromResult(new SellToShopResult
            {
                Success = false,
                ErrorMessage = $"Transaction error: {ex.Message}"
            });
        }
    }
}
