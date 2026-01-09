using MediatR;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Features.Shop.Commands;

/// <summary>
/// Command to browse a merchant's shop inventory.
/// </summary>
/// <param name="MerchantId">The merchant identifier.</param>
public record BrowseShopCommand(string MerchantId) : IRequest<BrowseShopResult>;

/// <summary>
/// Result of browsing a shop.
/// </summary>
public record BrowseShopResult
{
    /// <summary>Gets a value indicating whether the operation was successful.</summary>
    public required bool Success { get; init; }
    /// <summary>Gets the merchant name.</summary>
    public string? MerchantName { get; init; }
    /// <summary>Gets the core items always in stock.</summary>
    public List<ShopItem> CoreItems { get; init; } = new();
    /// <summary>Gets the dynamic rotating items.</summary>
    public List<ShopItem> DynamicItems { get; init; } = new();
    /// <summary>Gets the items sold by the player.</summary>
    public List<ShopItem> PlayerSoldItems { get; init; } = new();
    /// <summary>Gets the error message if any.</summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Represents an item in the shop with pricing information.
/// </summary>
public record ShopItem
{
    /// <summary>Gets the item.</summary>
    public required Item Item { get; init; }
    /// <summary>Gets the buy price (what player pays).</summary>
    public required int BuyPrice { get; init; }
    /// <summary>Gets the sell price (what player gets when selling).</summary>
    public required int SellPrice { get; init; }
    /// <summary>Gets a value indicating whether the item has unlimited stock.</summary>
    public bool IsUnlimited { get; init; }
    /// <summary>Gets the days remaining for player-sold items.</summary>
    public int? DaysRemaining { get; init; }
}
