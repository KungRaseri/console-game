using MediatR;

using Game.Shared.Models;
namespace Game.Core.Features.Quests.Queries;

public record GetMainQuestChainQuery : IRequest<List<Quest>>;

public class GetMainQuestChainHandler : IRequestHandler<GetMainQuestChainQuery, List<Quest>>
{
    private readonly Services.MainQuestService _mainQuestService;

    public GetMainQuestChainHandler(Services.MainQuestService mainQuestService)
    {
        _mainQuestService = mainQuestService;
    }

    public async Task<List<Quest>> Handle(GetMainQuestChainQuery request, CancellationToken cancellationToken)
    {
        return await _mainQuestService.GetMainQuestChainAsync();
    }
}
