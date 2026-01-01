using RealmEngine.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.Inventory.Commands;

/// <summary>
/// Command to sort inventory items by specified criteria.
/// </summary>
public record SortInventoryCommand : IRequest<SortInventoryResult>
{
    public required Character Player { get; init; }
    public required SortCriteria SortBy { get; init; }
}

/// <summary>
/// Result of sorting inventory.
/// </summary>
public record SortInventoryResult
{
    public required bool Success { get; init; }
    public required string Message { get; init; }
}

/// <summary>
/// Criteria for sorting inventory.
/// </summary>
public enum SortCriteria
{
    Name,
    Type,
    Rarity,
    Value
}