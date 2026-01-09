using MediatR;

namespace RealmEngine.Core.Features.Shop.Queries;

/// <summary>
/// Query to check if player can afford an item from a merchant.
/// </summary>
public record CheckAffordabilityQuery(
    string MerchantId,
    string ItemId
) : IRequest<CheckAffordabilityResult>;

/// <summary>
/// Result indicating if player can afford an item.
/// </summary>
public record CheckAffordabilityResult(
    bool Success,
    bool CanAfford,
    int ItemPrice,
    int PlayerGold,
    int GoldShortfall,
    string? ItemName = null,
    string? ErrorMessage = null
);
