using RealmEngine.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.Inventory.Commands;

/// <summary>
/// Command to use a consumable item.
/// </summary>
public record UseItemCommand : IRequest<UseItemResult>
{
    public required Character Player { get; init; }
    public required Item Item { get; init; }
}

/// <summary>
/// Result of using an item.
/// </summary>
public record UseItemResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int HealthRestored { get; init; }
    public int ManaRestored { get; init; }
}