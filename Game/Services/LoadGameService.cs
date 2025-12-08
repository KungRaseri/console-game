using Game.Models;
using Game.UI;
using Serilog;

namespace Game.Services;

/// <summary>
/// Handles loading saved games and deleting saves.
/// </summary>
public class LoadGameService
{
    private readonly SaveGameService _saveGameService;

    public LoadGameService(SaveGameService saveGameService)
    {
        _saveGameService = saveGameService;
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
                ConsoleUI.ShowWarning("No saved games found!");
                await Task.Delay(500);
                return (null, false);
            }

            ConsoleUI.Clear();
            ConsoleUI.ShowBanner("Load Game", "Select a save to continue your adventure");

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

            ConsoleUI.ShowTable("Available Saves", headers, rows);

            // Build menu options
            var menuOptions = saves.Select(s =>
                $"{s.Character.Name} - Level {s.Character.Level} {s.Character.ClassName}"
            ).ToList();
            menuOptions.Add("Delete a Save");
            menuOptions.Add("Back to Menu");

            var choice = ConsoleUI.ShowMenu("Select save:", menuOptions.ToArray());

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
            ConsoleUI.ShowProgress("Loading game...", task =>
            {
                task.MaxValue = 100;
                for (int i = 0; i <= 100; i += 20)
                {
                    task.Value = i;
                    Thread.Sleep(150);
                }
            });

            ConsoleUI.ShowSuccess($"Welcome back, {selectedSave.Character.Name}!");
            Log.Information("Game loaded for player {PlayerName}", selectedSave.Character.Name);
            await Task.Delay(500);

            return (selectedSave, true);
        }
        catch (Exception ex)
        {
            ConsoleUI.ShowError($"Failed to load game: {ex.Message}");
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
        ConsoleUI.Clear();
        ConsoleUI.ShowBanner("Delete Save", "⚠️ This action cannot be undone!");

        var menuOptions = saves.Select(s =>
            $"{s.Character.Name} - Level {s.Character.Level} {s.Character.ClassName}"
        ).ToList();
        menuOptions.Add("Cancel");

        var choice = ConsoleUI.ShowMenu("Select save to delete:", menuOptions.ToArray());

        if (choice == "Cancel")
        {
            await LoadGameAsync(); // Return to load menu
            return;
        }

        var selectedIndex = menuOptions.IndexOf(choice);
        var selectedSave = saves[selectedIndex];

        if (ConsoleUI.Confirm($"Delete save for {selectedSave.Character.Name}?"))
        {
            try
            {
                _saveGameService.DeleteSave(selectedSave.Id);
                ConsoleUI.ShowSuccess("Save deleted successfully!");
                Log.Information("Save deleted for player {PlayerName}", selectedSave.Character.Name);
                await Task.Delay(300);
                
                // Return to load menu
                await LoadGameAsync();
            }
            catch (Exception ex)
            {
                ConsoleUI.ShowError($"Failed to delete save: {ex.Message}");
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
