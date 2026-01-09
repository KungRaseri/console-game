using MediatR;

namespace RealmEngine.Core.Features.Inventory.Queries.GetInventoryValue;

/// <summary>
/// Query to calculate the total value of the player's inventory.
/// </summary>
public record GetInventoryValueQuery : IRequest<InventoryValueResult>
{
    /// <summary>Gets a value indicating whether to include equipped items in value calculation.</summary>
    public bool IncludeEquipped { get; init; } = true;
}

/// <summary>
/// Result containing inventory value calculations.
/// </summary>
public record InventoryValueResult
{
    /// <summary>Gets a value indicating whether the query succeeded.</summary>
    public bool Success { get; init; }
    /// <summary>Gets the total value of all items.</summary>
    public int TotalValue { get; init; }
    /// <summary>Gets the value of equipped items.</summary>
    public int EquippedValue { get; init; }
    /// <summary>Gets the value of unequipped items.</summary>
    public int UnequippedValue { get; init; }
    /// <summary>Gets the price of the most valuable item.</summary>
    public int MostValuableItemPrice { get; init; }
    /// <summary>Gets the name of the most valuable item.</summary>
    public string? MostValuableItemName { get; init; }
    /// <summary>Gets the wealth category based on total value.</summary>
    public WealthCategory WealthCategory { get; init; }
    /// <summary>Gets the error message if query failed.</summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Player wealth categories based on total inventory value.
/// </summary>
public enum WealthCategory
{
    /// <summary>Pauper wealth level (0-99g).</summary>
    Pauper,
    /// <summary>Common wealth level (100-499g).</summary>
    Common,
    /// <summary>Comfortable wealth level (500-1999g).</summary>
    Comfortable,
    /// <summary>Wealthy level (2000-9999g).</summary>
    Wealthy,
    /// <summary>Rich wealth level (10000-49999g).</summary>
    Rich,
    /// <summary>Noble wealth level (50000+g).</summary>
    Noble,
}
