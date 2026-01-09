using MediatR;

using RealmEngine.Shared.Models;
namespace RealmEngine.Core.Features.Quests.Queries;

/// <summary>
/// Query to get all active quests.
/// </summary>
public record GetActiveQuestsQuery : IRequest<List<Quest>>;

/// <summary>
/// Handles getting active quests.
/// </summary>
public class GetActiveQuestsHandler : IRequestHandler<GetActiveQuestsQuery, List<Quest>>
{
    private readonly Services.QuestService _questService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetActiveQuestsHandler"/> class.
    /// </summary>
    /// <param name="questService">The quest service.</param>
    public GetActiveQuestsHandler(Services.QuestService questService)
    {
        _questService = questService;
    }

    /// <summary>
    /// Handles getting active quests.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The list of active quests.</returns>
    public async Task<List<Quest>> Handle(GetActiveQuestsQuery request, CancellationToken cancellationToken)
    {
        return await _questService.GetActiveQuestsAsync();
    }
}
