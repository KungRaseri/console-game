using MediatR;

namespace RealmEngine.Core.Features.SaveLoad.Queries;

/// <summary>
/// Handles the GetAllSaves query.
/// </summary>
public class GetAllSavesHandler : IRequestHandler<GetAllSavesQuery, GetAllSavesResult>
{
    private readonly SaveGameService _saveGameService;

    public GetAllSavesHandler(SaveGameService saveGameService)
    {
        _saveGameService = saveGameService;
    }

    public Task<GetAllSavesResult> Handle(GetAllSavesQuery request, CancellationToken cancellationToken)
    {
        var saveGames = _saveGameService.GetAllSaves();

        return Task.FromResult(new GetAllSavesResult
        {
            SaveGames = saveGames
        });
    }
}