using MediatR;
using Serilog;

namespace RealmEngine.Core.Features.Exploration.Queries;

/// <summary>
/// Handler for GetKnownLocationsQuery.
/// </summary>
public class GetKnownLocationsQueryHandler : IRequestHandler<GetKnownLocationsQuery, GetKnownLocationsResult>
{
    private readonly ExplorationService _explorationService;

    public GetKnownLocationsQueryHandler(ExplorationService explorationService)
    {
        _explorationService = explorationService;
    }

    public Task<GetKnownLocationsResult> Handle(GetKnownLocationsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var locations = _explorationService.GetKnownLocations();
            return Task.FromResult(new GetKnownLocationsResult(true, Locations: locations));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting known locations");
            return Task.FromResult(new GetKnownLocationsResult(false, ErrorMessage: ex.Message));
        }
    }
}