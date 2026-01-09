using MediatR;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Features.Shop.Commands;

/// <summary>
/// Command to browse a merchant's shop inventory.
/// </summary>
public record BrowseShopCommand(string MerchantId) : IRequest<BrowseShopResult>;

/// <summary>
/// Result of browsing a shop.
/// </summary>
public record BrowseShopResult
{
    public required bool Success { get; init; }
    public string? MerchantName { get; init; }
    public List<ShopItem> CoreItems { get; init; } = new();
    public List<ShopItem> DynamicItems { get; init; } = new();
    public List<ShopItem> PlayerSoldItems { get; init; } = new();
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Represents an item in the shop with pricing information.
/// </summary>
public record ShopItem
{
    public required Item Item { get; init; }
    public required int BuyPrice { get; init; }  // What player pays to buy
    public required int SellPrice { get; init; } // What player gets when selling
    public bool IsUnlimited { get; init; }       // Core items are unlimited
    public int? DaysRemaining { get; init; }     // For player-sold items
}
