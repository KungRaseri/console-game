using Game.Core.Models;
using MediatR;

namespace Game.Core.Features.Inventory.Commands;

/// <summary>
/// Command to drop an item from inventory.
/// </summary>
public record DropItemCommand : IRequest<DropItemResult>
{
    public required Character Player { get; init; }
    public required Item Item { get; init; }
}

/// <summary>
/// Result of dropping an item.
/// </summary>
public record DropItemResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
