using Game.Shared.Services;
using Game.Shared.UI;
using MediatR;
using Serilog;

namespace Game.Features.Exploration.Commands;

/// <summary>
/// Handler for TravelToLocationCommand.
/// </summary>
public class TravelToLocationCommandHandler : IRequestHandler<TravelToLocationCommand, TravelToLocationResult>
{
    private readonly GameStateService _gameState;

    public TravelToLocationCommandHandler(GameStateService gameState)
    {
        _gameState = gameState;
    }

    public Task<TravelToLocationResult> Handle(TravelToLocationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Destination))
            {
                return Task.FromResult(new TravelToLocationResult(false, ErrorMessage: "Destination cannot be empty"));
            }

            if (request.Destination == _gameState.CurrentLocation)
            {
                return Task.FromResult(new TravelToLocationResult(false, ErrorMessage: "Already at this location"));
            }

            _gameState.UpdateLocation(request.Destination);
            
            ConsoleUI.ShowSuccess($"Traveled to {_gameState.CurrentLocation}");
            Log.Information("Player traveled to {Location}", _gameState.CurrentLocation);

            return Task.FromResult(new TravelToLocationResult(true, NewLocation: _gameState.CurrentLocation));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error traveling to location {Destination}", request.Destination);
            return Task.FromResult(new TravelToLocationResult(false, ErrorMessage: ex.Message));
        }
    }
}
