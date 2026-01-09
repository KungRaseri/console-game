using RealmEngine.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.Inventory.Commands;

/// <summary>
/// Command to use a consumable item.
/// </summary>
public record UseItemCommand : IRequest<UseItemResult>
{
    /// <summary>
    /// Gets the player character using the item.
    /// </summary>
    public required Character Player { get; init; }
    
    /// <summary>
    /// Gets the item to use.
    /// </summary>
    public required Item Item { get; init; }
}

/// <summary>
/// Result of using an item.
/// </summary>
public record UseItemResult
{
    /// <summary>
    /// Gets a value indicating whether the use was successful.
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// Gets a message describing the use result.
    /// </summary>
    public string Message { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets the amount of health restored.
    /// </summary>
    public int HealthRestored { get; init; }
    
    /// <summary>
    /// Gets the amount of mana restored.
    /// </summary>
    public int ManaRestored { get; init; }
}