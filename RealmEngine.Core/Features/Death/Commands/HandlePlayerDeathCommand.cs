using RealmEngine.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.Death.Commands;

/// <summary>
/// Command to handle player death with appropriate penalties.
/// </summary>
public record HandlePlayerDeathCommand : IRequest<HandlePlayerDeathResult>
{
    public required Character Player { get; init; }
    public required string DeathLocation { get; init; }
    public Enemy? Killer { get; init; }
}

/// <summary>
/// Result of player death handling.
/// </summary>
public record HandlePlayerDeathResult
{
    public required bool IsPermadeath { get; init; }
    public required bool SaveDeleted { get; init; }
    public List<Item> DroppedItems { get; init; } = new();
    public int GoldLost { get; init; }
    public int XPLost { get; init; }
    public string? HallOfFameId { get; init; }
}