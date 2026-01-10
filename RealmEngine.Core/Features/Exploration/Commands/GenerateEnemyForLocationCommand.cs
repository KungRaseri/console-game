using MediatR;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Features.Exploration.Commands;

/// <summary>
/// Command to generate an enemy appropriate for the current location.
/// Called when exploration triggers combat.
/// </summary>
public record GenerateEnemyForLocationCommand : IRequest<GenerateEnemyForLocationResult>
{
    /// <summary>
    /// Optional location ID. If not provided, uses GameStateService.CurrentLocation.
    /// </summary>
    public string? LocationId { get; init; }
}

/// <summary>
/// Result of enemy generation for a location.
/// </summary>
public record GenerateEnemyForLocationResult(
    bool Success,
    Enemy? Enemy = null,
    string? ErrorMessage = null
);
