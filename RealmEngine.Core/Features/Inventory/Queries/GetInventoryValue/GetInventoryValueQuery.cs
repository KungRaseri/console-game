using MediatR;

namespace RealmEngine.Core.Features.Inventory.Queries.GetInventoryValue;

/// <summary>
/// Query to calculate the total value of the player's inventory.
/// </summary>
public record GetInventoryValueQuery : IRequest<InventoryValueResult>
{
    public bool IncludeEquipped { get; init; } = true;
}

/// <summary>
/// Result containing inventory value calculations.
/// </summary>
public record InventoryValueResult
{
    public bool Success { get; init; }
    public int TotalValue { get; init; }
    public int EquippedValue { get; init; }
    public int UnequippedValue { get; init; }
    public int MostValuableItemPrice { get; init; }
    public string? MostValuableItemName { get; init; }
    public WealthCategory WealthCategory { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Player wealth categories based on total inventory value.
/// </summary>
public enum WealthCategory
{
    Pauper,      // 0-99g
    Common,      // 100-499g
    Comfortable, // 500-1999g
    Wealthy,     // 2000-9999g
    Rich,        // 10000-49999g
    Noble,       // 50000+g
}
