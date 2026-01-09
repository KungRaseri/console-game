using MediatR;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Features.Shop.Queries;

/// <summary>
/// Query to get detailed information about a merchant's shop.
/// </summary>
public record GetMerchantInfoQuery(string MerchantId) : IRequest<GetMerchantInfoResult>;

/// <summary>
/// Detailed merchant information for UI display.
/// </summary>
public record GetMerchantInfoResult(
    bool Success,
    MerchantInfo? Merchant = null,
    string? ErrorMessage = null
);

/// <summary>
/// Complete merchant profile with shop details.
/// </summary>
public record MerchantInfo(
    string Id,
    string Name,
    string Occupation,
    int Gold,
    string ShopType,
    string ShopInventoryType,
    int TotalItemsForSale,
    int CoreItemsCount,
    int DynamicItemsCount,
    int PlayerSoldItemsCount,
    bool AcceptsPlayerItems,
    double BuyPriceModifier,
    double SellPriceModifier
);
