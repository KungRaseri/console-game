using MediatR;

namespace RealmEngine.Core.Features.Victory.Commands;

public record StartNewGamePlusCommand : IRequest<StartNewGamePlusResult>;

public record StartNewGamePlusResult(bool Success, string Message);

public class StartNewGamePlusHandler : IRequestHandler<StartNewGamePlusCommand, StartNewGamePlusResult>
{
    private readonly Services.NewGamePlusService _ngPlusService;

    public StartNewGamePlusHandler(Services.NewGamePlusService ngPlusService)
    {
        _ngPlusService = ngPlusService;
    }

    public async Task<StartNewGamePlusResult> Handle(StartNewGamePlusCommand request, CancellationToken cancellationToken)
    {
        var result = await _ngPlusService.StartNewGamePlusAsync();

        if (result.Success)
        {
            return new StartNewGamePlusResult(true, "New Game+ started!");
        }

        return new StartNewGamePlusResult(false, "Failed to start New Game+");
    }
}