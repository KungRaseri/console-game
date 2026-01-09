using MediatR;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Features.Shop.Commands;

/// <summary>
/// Command to sell an item to a merchant (player sells).
/// </summary>
public record SellToShopCommand(string MerchantId, string ItemId) : IRequest<SellToShopResult>;

/// <summary>
/// Result of selling to a shop.
/// </summary>
public record SellToShopResult
{
    public required bool Success { get; init; }
    public Item? ItemSold { get; init; }
    public int PriceReceived { get; init; }
    public int PlayerGoldRemaining { get; init; }
    public string? ErrorMessage { get; init; }
}
