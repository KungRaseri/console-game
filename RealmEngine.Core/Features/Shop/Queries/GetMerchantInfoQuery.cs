using MediatR;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Features.Shop.Queries;

/// <summary>
/// Query to get detailed information about a merchant's shop.
/// </summary>
/// <param name="MerchantId">The merchant identifier.</param>
public record GetMerchantInfoQuery(string MerchantId) : IRequest<GetMerchantInfoResult>;

/// <summary>
/// Detailed merchant information for UI display.
/// </summary>
/// <param name="Success">Whether the query was successful.</param>
/// <param name="Merchant">The merchant information.</param>
/// <param name="ErrorMessage">Error message if any.</param>
public record GetMerchantInfoResult(
    bool Success,
    MerchantInfo? Merchant = null,
    string? ErrorMessage = null
);

/// <summary>
/// Complete merchant profile with shop details.
/// </summary>
/// <param name="Id">The merchant ID.</param>
/// <param name="Name">The merchant name.</param>
/// <param name="Occupation">The merchant occupation.</param>
/// <param name="Gold">The merchant's gold.</param>
/// <param name="ShopType">The shop type.</param>
/// <param name="ShopInventoryType">The shop inventory type.</param>
/// <param name="TotalItemsForSale">Total items for sale.</param>
/// <param name="CoreItemsCount">Number of core items.</param>
/// <param name="DynamicItemsCount">Number of dynamic items.</param>
/// <param name="PlayerSoldItemsCount">Number of player-sold items.</param>
/// <param name="AcceptsPlayerItems">Whether merchant accepts player items.</param>
/// <param name="BuyPriceModifier">Buy price modifier.</param>
/// <param name="SellPriceModifier">Sell price modifier.</param>
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
