using MediatR;

namespace Game.Core.Features.Exploration.Commands;

/// <summary>
/// Command to travel to a new location.
/// </summary>
public record TravelToLocationCommand(string Destination) : IRequest<TravelToLocationResult>;

/// <summary>
/// Result of travel.
/// </summary>
public record TravelToLocationResult(
    bool Success,
    string? NewLocation = null,
    string? ErrorMessage = null
);
