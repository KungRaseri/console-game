using MediatR;

namespace RealmEngine.Core.Features.Shop.Commands;

/// <summary>
/// Command to refresh/restock a merchant's dynamic inventory.
/// Called when time passes or player revisits after a period.
/// </summary>
public record RefreshMerchantInventoryCommand(string MerchantId) : IRequest<RefreshMerchantInventoryResult>;

/// <summary>
/// Result of inventory refresh operation.
/// </summary>
public record RefreshMerchantInventoryResult(
    bool Success,
    int ItemsAdded,
    int ItemsRemoved,
    int ItemsExpired,
    string? ErrorMessage = null
);
