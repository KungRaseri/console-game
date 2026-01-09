using MediatR;
using Serilog;

namespace RealmEngine.Core.Features.SaveLoad.Commands;

/// <summary>
/// Handles the LoadGame command.
/// </summary>
public class LoadGameHandler : IRequestHandler<LoadGameCommand, LoadGameResult>
{
    private readonly SaveGameService _saveGameService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadGameHandler"/> class.
    /// </summary>
    /// <param name="saveGameService">The save game service.</param>
    public LoadGameHandler(SaveGameService saveGameService)
    {
        _saveGameService = saveGameService;
    }

    /// <summary>
    /// Handles the LoadGameCommand and returns the result.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<LoadGameResult> Handle(LoadGameCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var saveGame = _saveGameService.LoadGame(request.SaveId);

            if (saveGame == null)
            {
                return Task.FromResult(new LoadGameResult
                {
                    Success = false,
                    Message = "Save game not found",
                    SaveGame = null
                });
            }

            Log.Information("Game loaded for player {PlayerName}", saveGame.Character.Name);

            return Task.FromResult(new LoadGameResult
            {
                Success = true,
                Message = $"Loaded game: {saveGame.Character.Name}",
                SaveGame = saveGame
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load game {SaveId}", request.SaveId);

            return Task.FromResult(new LoadGameResult
            {
                Success = false,
                Message = $"Failed to load game: {ex.Message}",
                SaveGame = null
            });
        }
    }
}