using MediatR;
using Serilog;

using Game.Core.Services;
namespace Game.Core.Features.Exploration.Queries;

/// <summary>
/// Handler for GetCurrentLocationQuery.
/// </summary>
public class GetCurrentLocationQueryHandler : IRequestHandler<GetCurrentLocationQuery, GetCurrentLocationResult>
{
    private readonly GameStateService _gameState;

    public GetCurrentLocationQueryHandler(GameStateService gameState)
    {
        _gameState = gameState;
    }

    public Task<GetCurrentLocationResult> Handle(GetCurrentLocationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var currentLocation = _gameState.CurrentLocation;
            return Task.FromResult(new GetCurrentLocationResult(true, CurrentLocation: currentLocation));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting current location");
            return Task.FromResult(new GetCurrentLocationResult(false, ErrorMessage: ex.Message));
        }
    }
}