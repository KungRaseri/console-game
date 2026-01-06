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

    public async Task<GetKnownLocationsResult> Handle(GetKnownLocationsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var locations = await _explorationService.GetKnownLocationsAsync();
            var locationNames = locations.Select(l => l.Name).ToList();
            return new GetKnownLocationsResult(true, Locations: locationNames);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting known locations");
            return new GetKnownLocationsResult(false, ErrorMessage: ex.Message);
        }
    }
}