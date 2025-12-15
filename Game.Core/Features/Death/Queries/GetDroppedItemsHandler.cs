using Game.Core.Features.SaveLoad;
using MediatR;

namespace Game.Core.Features.Death.Queries;

/// <summary>
/// Handles retrieval of dropped items at a location.
/// </summary>
public class GetDroppedItemsHandler : IRequestHandler<GetDroppedItemsQuery, GetDroppedItemsResult>
{
    private readonly SaveGameService _saveGameService;
    
    public GetDroppedItemsHandler(SaveGameService saveGameService)
    {
        _saveGameService = saveGameService;
    }
    
    public Task<GetDroppedItemsResult> Handle(GetDroppedItemsQuery request, CancellationToken cancellationToken)
    {
        var saveGame = _saveGameService.GetCurrentSave();
        if (saveGame == null)
        {
            return Task.FromResult(new GetDroppedItemsResult());
        }
        
        if (saveGame.DroppedItemsAtLocations.TryGetValue(request.Location, out var items))
        {
            return Task.FromResult(new GetDroppedItemsResult { Items = items });
        }
        
        return Task.FromResult(new GetDroppedItemsResult());
    }
}
