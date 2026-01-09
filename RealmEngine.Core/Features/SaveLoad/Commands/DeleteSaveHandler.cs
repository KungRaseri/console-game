using MediatR;
using Serilog;

namespace RealmEngine.Core.Features.SaveLoad.Commands;

/// <summary>
/// Handles the DeleteSave command.
/// </summary>
public class DeleteSaveHandler : IRequestHandler<DeleteSaveCommand, DeleteSaveResult>
{
    private readonly SaveGameService _saveGameService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteSaveHandler"/> class.
    /// </summary>
    /// <param name="saveGameService">The save game service.</param>
    public DeleteSaveHandler(SaveGameService saveGameService)
    {
        _saveGameService = saveGameService;
    }

    /// <summary>
    /// Handles the DeleteSaveCommand and returns the result.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<DeleteSaveResult> Handle(DeleteSaveCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var success = _saveGameService.DeleteSave(request.SaveId);

            if (!success)
            {
                return Task.FromResult(new DeleteSaveResult
                {
                    Success = false,
                    Message = "Save game not found or could not be deleted"
                });
            }

            Log.Information("Save game deleted: {SaveId}", request.SaveId);

            return Task.FromResult(new DeleteSaveResult
            {
                Success = true,
                Message = "Save game deleted successfully"
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to delete save game {SaveId}", request.SaveId);

            return Task.FromResult(new DeleteSaveResult
            {
                Success = false,
                Message = $"Failed to delete save: {ex.Message}"
            });
        }
    }
}