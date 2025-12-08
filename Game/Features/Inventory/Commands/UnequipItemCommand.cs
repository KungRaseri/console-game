using Game.Models;
using MediatR;

namespace Game.Features.Inventory.Commands;

/// <summary>
/// Command to unequip an item from a specific slot.
/// </summary>
public record UnequipItemCommand : IRequest<UnequipItemResult>
{
    public required Character Player { get; init; }
    public required ItemType SlotType { get; init; }
}

/// <summary>
/// Result of unequipping an item.
/// </summary>
public record UnequipItemResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public Item? UnequippedItem { get; init; }
}
