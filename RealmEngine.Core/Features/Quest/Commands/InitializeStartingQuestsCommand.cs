using MediatR;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Features.Quests.Commands;

/// <summary>
/// Command to initialize the starting quests when creating a new game.
/// </summary>
public record InitializeStartingQuestsCommand(SaveGame SaveGame) : IRequest<InitializeStartingQuestsResult>;

public record InitializeStartingQuestsResult(bool Success, int QuestsInitialized);

public class InitializeStartingQuestsHandler : IRequestHandler<InitializeStartingQuestsCommand, InitializeStartingQuestsResult>
{
    private readonly Services.QuestInitializationService _initService;

    public InitializeStartingQuestsHandler(Services.QuestInitializationService initService)
    {
        _initService = initService;
    }

    public async Task<InitializeStartingQuestsResult> Handle(InitializeStartingQuestsCommand request, CancellationToken cancellationToken)
    {
        await _initService.InitializeStartingQuests(request.SaveGame);

        return new InitializeStartingQuestsResult(
            true, 
            request.SaveGame.AvailableQuests.Count);
    }
}
