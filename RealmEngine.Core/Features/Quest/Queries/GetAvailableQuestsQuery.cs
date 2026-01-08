using MediatR;
using RealmEngine.Shared.Models;
using RealmEngine.Core.Features.SaveLoad;

namespace RealmEngine.Core.Features.Quests.Queries;

/// <summary>
/// Query to get all available quests (not yet accepted).
/// </summary>
public record GetAvailableQuestsQuery : IRequest<List<Quest>>;

public class GetAvailableQuestsHandler : IRequestHandler<GetAvailableQuestsQuery, List<Quest>>
{
    private readonly ISaveGameService _saveGameService;

    public GetAvailableQuestsHandler(ISaveGameService saveGameService)
    {
        _saveGameService = saveGameService;
    }

    public async Task<List<Quest>> Handle(GetAvailableQuestsQuery request, CancellationToken cancellationToken)
    {
        var saveGame = _saveGameService.GetCurrentSave();
        return await Task.FromResult(saveGame?.AvailableQuests ?? new List<Quest>());
    }
}
