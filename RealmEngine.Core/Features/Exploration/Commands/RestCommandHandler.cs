using RealmEngine.Core.Abstractions;

using MediatR;
using Serilog;

using RealmEngine.Core.Services;
namespace RealmEngine.Core.Features.Exploration.Commands;

/// <summary>
/// Handler for RestCommand.
/// </summary>
public class RestCommandHandler : IRequestHandler<RestCommand, RestResult>
{
    private readonly GameStateService _gameState;
    private readonly IGameUI _console;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestCommandHandler"/> class.
    /// </summary>
    /// <param name="gameState">The game state service.</param>
    /// <param name="console">The game UI.</param>
    public RestCommandHandler(GameStateService gameState, IGameUI console)
    {
        _gameState = gameState;
        _console = console;
    }

    /// <summary>
    /// Handles the rest command to recover health and mana.
    /// </summary>
    /// <param name="request">The rest command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The rest result.</returns>
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