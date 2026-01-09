using MediatR;

using RealmEngine.Shared.Models;
namespace RealmEngine.Core.Features.Quests.Queries;

/// <summary>
/// Query to get the main quest chain.
/// </summary>
public record GetMainQuestChainQuery : IRequest<List<Quest>>;

/// <summary>
/// Handles getting the main quest chain.
/// </summary>
public class GetMainQuestChainHandler : IRequestHandler<GetMainQuestChainQuery, List<Quest>>
{
    private readonly Services.MainQuestService _mainQuestService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMainQuestChainHandler"/> class.
    /// </summary>
    /// <param name="mainQuestService">The main quest service.</param>
    public GetMainQuestChainHandler(Services.MainQuestService mainQuestService)
    {
        _mainQuestService = mainQuestService;
    }

    /// <summary>
    /// Handles getting the main quest chain.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The list of main quest chain quests.</returns>
    public async Task<List<Quest>> Handle(GetMainQuestChainQuery request, CancellationToken cancellationToken)
    {
        return await _mainQuestService.GetMainQuestChainAsync();
    }
}
