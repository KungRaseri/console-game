using MediatR;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Features.Shop.Commands;

/// <summary>
/// Command to buy an item from a merchant (player purchases).
/// </summary>
/// <param name="MerchantId">The merchant identifier.</param>
/// <param name="ItemId">The item identifier.</param>
public record BuyFromShopCommand(string MerchantId, string ItemId) : IRequest<BuyFromShopResult>;

/// <summary>
/// Result of buying from a shop.
/// </summary>
public record BuyFromShopResult
{
    /// <summary>Gets a value indicating whether the operation was successful.</summary>
    public required bool Success { get; init; }
    /// <summary>Gets the item purchased.</summary>
    public Item? ItemPurchased { get; init; }
    /// <summary>Gets the price charged.</summary>
    public int PriceCharged { get; init; }
    /// <summary>Gets the player's remaining gold.</summary>
    public int PlayerGoldRemaining { get; init; }
    /// <summary>Gets the error message if any.</summary>
    public string? ErrorMessage { get; init; }
}
