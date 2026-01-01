using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RealmForge.Services;
using RealmEngine.Shared.Models;
using Serilog;

namespace RealmForge.ViewModels;

/// <summary>
/// ViewModel for the Preview Window
/// </summary>
public partial class PreviewWindowViewModel : ObservableObject
{
    private readonly PreviewService _previewService;

    [ObservableProperty]
    private ObservableCollection<string> _contentTypes = new()
    {
        "Items (Random)",
        "Weapons",
        "Consumables",
        "Enemies (Random)",
        "Enemies - Beasts",
        "Enemies - Demons",
        "Enemies - Dragons",
        "Enemies - Elementals",
        "Enemies - Humanoids",
        "Enemies - Undead",
        "NPCs",
        "Quests (Random)",
        "Quests - Kill",
        "Quests - Collect",
        "Quests - Escort",
        "Quests - Explore"
    };

    [ObservableProperty]
    private string _selectedContentType = "Items (Random)";

    [ObservableProperty]
    private int _count = 10;

    [ObservableProperty]
    private ObservableCollection<PreviewItem> _previewItems = new();

    [ObservableProperty]
    private string _statusMessage = "Select a content type and click Generate";

    public PreviewWindowViewModel()
    {
        _previewService = new PreviewService();

        // Auto-generate on load
        Generate();
    }

    [RelayCommand]
    private void Generate()
    {
        try
        {
            StatusMessage = $"Generating {Count} {SelectedContentType}...";
            PreviewItems.Clear();

            List<PreviewItem> items = SelectedContentType switch
            {
                "Items (Random)" => _previewService.GenerateItemPreviews(Count),
                "Weapons" => _previewService.GenerateWeaponPreviews(Count),
                "Consumables" => _previewService.GenerateConsumablePreviews(Count),
                "Enemies (Random)" => _previewService.GenerateEnemyPreviews(5, Count),
                "Enemies - Beasts" => _previewService.GenerateEnemyPreviewsByType("Beast", 5, Count),
                "Enemies - Demons" => _previewService.GenerateEnemyPreviewsByType("Demon", 5, Count),
                "Enemies - Dragons" => _previewService.GenerateEnemyPreviewsByType("Dragon", 5, Count),
                "Enemies - Elementals" => _previewService.GenerateEnemyPreviewsByType("Elemental", 5, Count),
                "Enemies - Humanoids" => _previewService.GenerateEnemyPreviewsByType("Humanoid", 5, Count),
                "Enemies - Undead" => _previewService.GenerateEnemyPreviewsByType("Undead", 5, Count),
                "NPCs" => _previewService.GenerateNpcPreviews(Count),
                "Quests (Random)" => _previewService.GenerateQuestPreviews(Count),
                "Quests - Kill" => _previewService.GenerateQuestPreviewsByType("Kill", Count),
                "Quests - Collect" => _previewService.GenerateQuestPreviewsByType("Collect", Count),
                "Quests - Escort" => _previewService.GenerateQuestPreviewsByType("Escort", Count),
                "Quests - Explore" => _previewService.GenerateQuestPreviewsByType("Explore", Count),
                _ => new List<PreviewItem>()
            };

            foreach (var item in items)
            {
                PreviewItems.Add(item);
            }

            StatusMessage = $"Generated {PreviewItems.Count} items successfully";
            Log.Information("Generated {Count} {Type} previews", PreviewItems.Count, SelectedContentType);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            Log.Error(ex, "Failed to generate previews for {Type}", SelectedContentType);
        }
    }

    [RelayCommand]
    private void CopyAll()
    {
        try
        {
            var text = string.Join("\n\n", PreviewItems.Select(item =>
                $"[{item.Category}] {item.Name}\n{item.Details}\n{item.FullDescription}"));

            Clipboard.SetText(text);
            StatusMessage = $"Copied {PreviewItems.Count} items to clipboard";
            Log.Information("Copied {Count} preview items to clipboard", PreviewItems.Count);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to copy: {ex.Message}";
            Log.Error(ex, "Failed to copy preview items to clipboard");
        }
    }

    [RelayCommand]
    private void Close()
    {
        // Window will handle closing via CloseRequested event
        Application.Current.Windows.OfType<Window>()
            .FirstOrDefault(w => w.DataContext == this)?.Close();
    }
}
