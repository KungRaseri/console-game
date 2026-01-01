using MediatR;
using Serilog;

namespace RealmEngine.Core.Features.SaveLoad.Commands;

/// <summary>
/// Handles the SaveGame command.
/// </summary>
public class SaveGameHandler : IRequestHandler<SaveGameCommand, SaveGameResult>
{
    private readonly SaveGameService _saveGameService;

    public SaveGameHandler(SaveGameService saveGameService)
    {
        _saveGameService = saveGameService;
    }

    public Task<SaveGameResult> Handle(SaveGameCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _saveGameService.SaveGame(request.Player, request.Inventory, request.SaveId);

            Log.Information("Game saved for player {PlayerName}", request.Player.Name);

            return Task.FromResult(new SaveGameResult
            {
                Success = true,
                Message = "Game saved successfully!",
                SaveId = request.SaveId
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save game for player {PlayerName}", request.Player.Name);

            return Task.FromResult(new SaveGameResult
            {
                Success = false,
                Message = $"Failed to save game: {ex.Message}",
                SaveId = null
            });
        }
    }
}