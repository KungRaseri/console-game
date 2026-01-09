using MediatR;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Features.Shop.Commands;

/// <summary>
/// Command to buy an item from a merchant (player purchases).
/// </summary>
public record BuyFromShopCommand(string MerchantId, string ItemId) : IRequest<BuyFromShopResult>;

/// <summary>
/// Result of buying from a shop.
/// </summary>
public record BuyFromShopResult
{
    public required bool Success { get; init; }
    public Item? ItemPurchased { get; init; }
    public int PriceCharged { get; init; }
    public int PlayerGoldRemaining { get; init; }
    public string? ErrorMessage { get; init; }
}
