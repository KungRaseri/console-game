using MediatR;

using Game.Shared.Models;
namespace Game.Core.Features.Quests.Queries;

public record GetActiveQuestsQuery : IRequest<List<Quest>>;

public class GetActiveQuestsHandler : IRequestHandler<GetActiveQuestsQuery, List<Quest>>
{
    private readonly Services.QuestService _questService;

    public GetActiveQuestsHandler(Services.QuestService questService)
    {
        _questService = questService;
    }

    public async Task<List<Quest>> Handle(GetActiveQuestsQuery request, CancellationToken cancellationToken)
    {
        return await _questService.GetActiveQuestsAsync();
    }
}
