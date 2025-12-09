using MediatR;

namespace Game.Features.Quest.Queries;

public record GetMainQuestChainQuery : IRequest<List<Models.Quest>>;

public class GetMainQuestChainHandler : IRequestHandler<GetMainQuestChainQuery, List<Models.Quest>>
{
    private readonly Services.MainQuestService _mainQuestService;
    
    public GetMainQuestChainHandler(Services.MainQuestService mainQuestService)
    {
        _mainQuestService = mainQuestService;
    }
    
    public async Task<List<Models.Quest>> Handle(GetMainQuestChainQuery request, CancellationToken cancellationToken)
    {
        return await _mainQuestService.GetMainQuestChainAsync();
    }
}
