using MediatR;

namespace RealmEngine.Core.Features.Shop.Commands;

/// <summary>
/// Command to refresh/restock a merchant's dynamic inventory.
/// Called when time passes or player revisits after a period.
/// </summary>
/// <param name="MerchantId">The merchant identifier.</param>
public record RefreshMerchantInventoryCommand(string MerchantId) : IRequest<RefreshMerchantInventoryResult>;

/// <summary>
/// Result of inventory refresh operation.
/// </summary>
/// <param name="Success">Whether the operation was successful.</param>
/// <param name="ItemsAdded">Number of items added.</param>
/// <param name="ItemsRemoved">Number of items removed.</param>
/// <param name="ItemsExpired">Number of items expired.</param>
/// <param name="ErrorMessage">Error message if any.</param>
public record RefreshMerchantInventoryResult(
    bool Success,
    int ItemsAdded,
    int ItemsRemoved,
    int ItemsExpired,
    string? ErrorMessage = null
);
