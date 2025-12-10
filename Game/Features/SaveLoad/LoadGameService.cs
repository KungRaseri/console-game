using Game.Models;
using Game.Shared.UI;
using Game.Shared.Services;
using Serilog;

namespace Game.Features.SaveLoad;

/// <summary>
/// Handles loading saved games and deleting saves.
/// </summary>
public class LoadGameService
{
    private readonly SaveGameService _saveGameService;
    private readonly ApocalypseTimer _apocalypseTimer;
    private readonly IConsoleUI _console;

    public LoadGameService(SaveGameService saveGameService, ApocalypseTimer apocalypseTimer, IConsoleUI console)
    {
        _saveGameService = saveGameService;
        _apocalypseTimer = apocalypseTimer;
        _console = console;
    }

    /// <summary>
    /// Shows the load game menu and returns the selected save.
    /// Returns null if user cancelled or no saves exist.
    /// </summary>
    public async Task<(SaveGame? SelectedSave, bool LoadSuccessful)> LoadGameAsync()
    {
        try
        {
            var saves = _saveGameService.GetAllSaves();

            if (!saves.Any())
            {
                _console.ShowWarning("No saved games found!");
                await Task.Delay(500);
                return (null, false);
            }

            _console.Clear();
            _console.ShowBanner("Load Game", "Select a save to continue your adventure");

            // Display saves in a table
            var headers = new[] { "Player", "Class", "Level", "Last Played", "Play Time" };
            var rows = saves.Select(s =>
            {
                var timeSinceSave = DateTime.Now - s.SaveDate;
                var timeAgo = timeSinceSave.TotalHours < 24
                    ? $"{(int)timeSinceSave.TotalHours}h ago"
                    : $"{(int)timeSinceSave.TotalDays}d ago";
                
                var playTime = s.PlayTimeMinutes < 60
                    ? $"{s.PlayTimeMinutes}m"
                    : $"{s.PlayTimeMinutes / 60}h {s.PlayTimeMinutes % 60}m";

                return new[]
                {
                    s.Character.Name,
                    s.Character.ClassName,
                    s.Character.Level.ToString(),
                    timeAgo,
                    playTime
                };
            }).ToList();

            _console.ShowTable("Available Saves", headers, rows);

            // Build menu options
            var menuOptions = saves.Select(s =>
                $"{s.Character.Name} - Level {s.Character.Level} {s.Character.ClassName}"
            ).ToList();
            menuOptions.Add("Delete a Save");
            menuOptions.Add("Back to Menu");

            var choice = _console.ShowMenu("Select save:", menuOptions.ToArray());

            if (choice == "Back to Menu")
            {
                return (null, false);
            }

            if (choice == "Delete a Save")
            {
                await DeleteSaveAsync(saves);
                return (null, false);
            }

            // Find selected save
            var selectedIndex = menuOptions.IndexOf(choice);
            var selectedSave = saves[selectedIndex];

            // Load the save
            _console.ShowProgress("Loading game...", task =>
            {
                task.MaxValue = 100;
                for (int i = 0; i <= 100; i += 20)
                {
                    task.Value = i;
                    Thread.Sleep(150);
                }
            });

            _console.ShowSuccess($"Welcome back, {selectedSave.Character.Name}!");
            Log.Information("Game loaded for player {PlayerName}", selectedSave.Character.Name);
            
            // Restore apocalypse timer if applicable
            if (selectedSave.ApocalypseMode && selectedSave.ApocalypseStartTime.HasValue)
            {
                _apocalypseTimer.StartFromSave(selectedSave.ApocalypseStartTime.Value, selectedSave.ApocalypseBonusMinutes);
                
                // Check if time expired while they were away
                if (_apocalypseTimer.IsExpired())
                {
                    _console.ShowError("Time has run out! The apocalypse occurred while you were gone.");
                    await Task.Delay(3000);
                    // Return to GameEngine which will handle the apocalypse game over
                    return (selectedSave, true);
                }
                
                // Show time remaining
                var remaining = _apocalypseTimer.GetRemainingMinutes();
                _console.ShowWarning($"Apocalypse Mode: {remaining} minutes remaining!");
                
                if (remaining < 60)
                {
                    _console.ShowError("WARNING: Less than 1 hour remaining!");
                }
                
                await Task.Delay(2000);
            }
            
            await Task.Delay(500);

            return (selectedSave, true);
        }
        catch (Exception ex)
        {
            _console.ShowError($"Failed to load game: {ex.Message}");
            Log.Error(ex, "Failed to load game");
            await Task.Delay(500);
            return (null, false);
        }
    }

    /// <summary>
    /// Handles save deletion with confirmation.
    /// </summary>
    private async Task DeleteSaveAsync(List<SaveGame> saves)
    {
        _console.Clear();
        _console.ShowBanner("Delete Save", "⚠️ This action cannot be undone!");

        var menuOptions = saves.Select(s =>
            $"{s.Character.Name} - Level {s.Character.Level} {s.Character.ClassName}"
        ).ToList();
        menuOptions.Add("Cancel");

        var choice = _console.ShowMenu("Select save to delete:", menuOptions.ToArray());

        if (choice == "Cancel")
        {
            await LoadGameAsync(); // Return to load menu
            return;
        }

        var selectedIndex = menuOptions.IndexOf(choice);
        var selectedSave = saves[selectedIndex];

        if (_console.Confirm($"Delete save for {selectedSave.Character.Name}?"))
        {
            try
            {
                _saveGameService.DeleteSave(selectedSave.Id);
                _console.ShowSuccess("Save deleted successfully!");
                Log.Information("Save deleted for player {PlayerName}", selectedSave.Character.Name);
                await Task.Delay(300);
                
                // Return to load menu
                await LoadGameAsync();
            }
            catch (Exception ex)
            {
                _console.ShowError($"Failed to delete save: {ex.Message}");
                Log.Error(ex, "Failed to delete save");
                await Task.Delay(500);
            }
        }
        else
        {
            await LoadGameAsync(); // Return to load menu
        }
    }
}
