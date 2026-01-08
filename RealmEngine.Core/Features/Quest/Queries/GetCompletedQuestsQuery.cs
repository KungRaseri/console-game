using MediatR;
using RealmEngine.Shared.Models;
using RealmEngine.Core.Features.SaveLoad;

namespace RealmEngine.Core.Features.Quests.Queries;

/// <summary>
/// Query to get all completed quests.
/// </summary>
public record GetCompletedQuestsQuery : IRequest<List<Quest>>;

public class GetCompletedQuestsHandler : IRequestHandler<GetCompletedQuestsQuery, List<Quest>>
{
    private readonly ISaveGameService _saveGameService;

    public GetCompletedQuestsHandler(ISaveGameService saveGameService)
    {
        _saveGameService = saveGameService;
    }

    public async Task<List<Quest>> Handle(GetCompletedQuestsQuery request, CancellationToken cancellationToken)
    {
        var saveGame = _saveGameService.GetCurrentSave();
        return await Task.FromResult(saveGame?.CompletedQuests ?? new List<Quest>());
    }
}
