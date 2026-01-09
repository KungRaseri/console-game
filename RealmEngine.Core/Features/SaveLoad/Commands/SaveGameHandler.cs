using MediatR;
using Serilog;

namespace RealmEngine.Core.Features.SaveLoad.Commands;

/// <summary>
/// Handles the SaveGame command.
/// </summary>
public class SaveGameHandler : IRequestHandler<SaveGameCommand, SaveGameResult>
{
    private readonly SaveGameService _saveGameService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveGameHandler"/> class.
    /// </summary>
    /// <param name="saveGameService">The save game service.</param>
    public SaveGameHandler(SaveGameService saveGameService)
    {
        _saveGameService = saveGameService;
    }

    /// <summary>
    /// Handles the SaveGameCommand and returns the result.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result.</returns>
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