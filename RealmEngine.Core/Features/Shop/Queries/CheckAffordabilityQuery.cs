using MediatR;

namespace RealmEngine.Core.Features.Shop.Queries;

/// <summary>
/// Query to check if player can afford an item from a merchant.
/// </summary>
/// <param name="MerchantId">The merchant identifier.</param>
/// <param name="ItemId">The item identifier.</param>
public record CheckAffordabilityQuery(
    string MerchantId,
    string ItemId
) : IRequest<CheckAffordabilityResult>;

/// <summary>
/// Result indicating if player can afford an item.
/// </summary>
/// <param name="Success">Whether the query was successful.</param>
/// <param name="CanAfford">Whether player can afford the item.</param>
/// <param name="ItemPrice">The item price.</param>
/// <param name="PlayerGold">The player's current gold.</param>
/// <param name="GoldShortfall">Amount of gold needed if cannot afford.</param>
/// <param name="ItemName">The item name.</param>
/// <param name="ErrorMessage">Error message if any.</param>
public record CheckAffordabilityResult(
    bool Success,
    bool CanAfford,
    int ItemPrice,
    int PlayerGold,
    int GoldShortfall,
    string? ItemName = null,
    string? ErrorMessage = null
);
