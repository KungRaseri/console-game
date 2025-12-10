using Game.Shared.Services;
using Game.Shared.UI;
using MediatR;
using Serilog;

namespace Game.Features.Exploration.Commands;

/// <summary>
/// Handler for RestCommand.
/// </summary>
public class RestCommandHandler : IRequestHandler<RestCommand, RestResult>
{
    private readonly GameStateService _gameState;
    private readonly IConsoleUI _console;

    public RestCommandHandler(GameStateService gameState, IConsoleUI console)
    {
        _gameState = gameState;
        _console = console;
    }

    public Task<RestResult> Handle(RestCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var player = _gameState.Player;
            
            if (player == null)
            {
                return Task.FromResult(new RestResult(false, ErrorMessage: "No active player"));
            }

            _console.ShowInfo("You rest and recover...");

            var healthRecovered = player.MaxHealth - player.Health;
            var manaRecovered = player.MaxMana - player.Mana;

            player.Health = player.MaxHealth;
            player.Mana = player.MaxMana;

            _console.ShowSuccess("Fully rested!");
            
            Log.Information("Player {PlayerName} rested", player.Name);

            return Task.FromResult(new RestResult(
                Success: true,
                HealthRecovered: healthRecovered,
                ManaRecovered: manaRecovered
            ));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during rest");
            return Task.FromResult(new RestResult(false, ErrorMessage: ex.Message));
        }
    }
}
