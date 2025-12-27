using MediatR;

namespace Game.Core.Features.Quest.Queries;

public record GetActiveQuestsQuery : IRequest<List<Models.Quest>>;

public class GetActiveQuestsHandler : IRequestHandler<GetActiveQuestsQuery, List<Models.Quest>>
{
    private readonly Services.QuestService _questService;

    public GetActiveQuestsHandler(Services.QuestService questService)
    {
        _questService = questService;
    }

    public async Task<List<Models.Quest>> Handle(GetActiveQuestsQuery request, CancellationToken cancellationToken)
    {
        return await _questService.GetActiveQuestsAsync();
    }
}
