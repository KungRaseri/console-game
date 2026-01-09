using MediatR;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Features.Shop.Commands;

/// <summary>
/// Command to sell an item to a merchant (player sells).
/// </summary>
/// <param name="MerchantId">The merchant identifier.</param>
/// <param name="ItemId">The item identifier.</param>
public record SellToShopCommand(string MerchantId, string ItemId) : IRequest<SellToShopResult>;

/// <summary>
/// Result of selling to a shop.
/// </summary>
public record SellToShopResult
{
    /// <summary>Gets a value indicating whether the operation was successful.</summary>
    public required bool Success { get; init; }
    /// <summary>Gets the item sold.</summary>
    public Item? ItemSold { get; init; }
    /// <summary>Gets the price received.</summary>
    public int PriceReceived { get; init; }
    /// <summary>Gets the player's remaining gold.</summary>
    public int PlayerGoldRemaining { get; init; }
    /// <summary>Gets the error message if any.</summary>
    public string? ErrorMessage { get; init; }
}
