using MediatR;

namespace Game.Core.Features.SaveLoad.Queries;

/// <summary>
/// Handles the GetMostRecentSave query.
/// </summary>
public class GetMostRecentSaveHandler : IRequestHandler<GetMostRecentSaveQuery, GetMostRecentSaveResult>
{
    private readonly SaveGameService _saveGameService;

    public GetMostRecentSaveHandler(SaveGameService saveGameService)
    {
        _saveGameService = saveGameService;
    }

    public Task<GetMostRecentSaveResult> Handle(GetMostRecentSaveQuery request, CancellationToken cancellationToken)
    {
        var saveGame = _saveGameService.GetMostRecentSave();

        return Task.FromResult(new GetMostRecentSaveResult
        {
            SaveGame = saveGame
        });
    }
}
