using MediatR;
using Serilog;

namespace RealmEngine.Core.Features.SaveLoad.Commands;

/// <summary>
/// Handles the LoadGame command.
/// </summary>
public class LoadGameHandler : IRequestHandler<LoadGameCommand, LoadGameResult>
{
    private readonly SaveGameService _saveGameService;

    public LoadGameHandler(SaveGameService saveGameService)
    {
        _saveGameService = saveGameService;
    }

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