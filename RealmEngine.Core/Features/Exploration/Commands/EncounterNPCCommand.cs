using MediatR;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Features.Exploration.Commands;

/// <summary>
/// Command to encounter an NPC during exploration.
/// </summary>
public record EncounterNPCCommand(string NpcId) : IRequest<EncounterNPCResult>;

/// <summary>
/// Result of NPC encounter with available interaction options.
/// </summary>
public record EncounterNPCResult(
    bool Success,
    NPC? Npc = null,
    List<string>? AvailableActions = null,
    string? ErrorMessage = null
);
