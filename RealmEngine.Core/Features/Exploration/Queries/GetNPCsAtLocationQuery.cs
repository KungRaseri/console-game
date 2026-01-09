using MediatR;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Features.Exploration.Queries;

/// <summary>
/// Query to get all NPCs at the player's current location.
/// </summary>
public record GetNPCsAtLocationQuery(string? LocationName = null) : IRequest<GetNPCsAtLocationResult>;

/// <summary>
/// Result containing NPCs at a location with their available actions.
/// </summary>
public record GetNPCsAtLocationResult(
    bool Success,
    string? LocationName = null,
    List<NPCInfo>? NPCs = null,
    string? ErrorMessage = null
);

/// <summary>
/// Detailed NPC information for UI display.
/// </summary>
public record NPCInfo(
    string Id,
    string Name,
    string Occupation,
    bool IsFriendly,
    bool IsMerchant,
    bool IsQuestGiver,
    bool IsTrainer,
    string? Dialogue = null,
    int? RelationshipValue = null
);
