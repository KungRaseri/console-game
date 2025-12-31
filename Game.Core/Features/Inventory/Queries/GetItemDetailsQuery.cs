using Game.Shared.Models;
using MediatR;

namespace Game.Core.Features.Inventory.Queries;

/// <summary>
/// Query to get detailed information about a specific item.
/// </summary>
public record GetItemDetailsQuery : IRequest<GetItemDetailsResult>
{
    public required Item Item { get; init; }
}

/// <summary>
/// Result containing detailed item information.
/// </summary>
public record GetItemDetailsResult
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required ItemType Type { get; init; }
    public required ItemRarity Rarity { get; init; }
    public required int Price { get; init; }
    public required Dictionary<string, TraitValue> Traits { get; init; }
    public required int UpgradeLevel { get; init; }
    public required List<Enchantment> Enchantments { get; init; }
    public required string? SetName { get; init; }
}