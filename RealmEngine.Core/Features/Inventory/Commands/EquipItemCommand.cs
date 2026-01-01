using Game.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.Inventory.Commands;

/// <summary>
/// Command to equip an item on the character.
/// </summary>
public record EquipItemCommand : IRequest<EquipItemResult>
{
    public required Character Player { get; init; }
    public required Item Item { get; init; }
}

/// <summary>
/// Result of equipping an item.
/// </summary>
public record EquipItemResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public Item? UnequippedItem { get; init; }
}