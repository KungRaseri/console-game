using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Game.ContentBuilder.Models;
using Game.ContentBuilder.Services;
using Game.ContentBuilder.Views;
using Serilog;

namespace Game.ContentBuilder.ViewModels;

/// <summary>
/// Main ViewModel for the Content Builder application
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly JsonEditorService _jsonEditorService;
    private readonly CatalogTokenService _catalogTokenService;
    private readonly FileTreeService _fileTreeService;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _title = "Game Content Builder";

    [ObservableProperty]
    private ObservableCollection<CategoryNode> _categories = new();

    [ObservableProperty]
    private CategoryNode? _selectedCategory;

    [ObservableProperty]
    private object? _currentEditor;

    public MainViewModel()
    {
        // Initialize services with path to Game.Shared Data directory
        var dataPath = GetDataDirectory();
        _jsonEditorService = new JsonEditorService(dataPath);
        _catalogTokenService = new CatalogTokenService(_jsonEditorService);
        _fileTreeService = new FileTreeService(dataPath);

        Log.Information("MainViewModel initialized - Data directory: {DataPath}", dataPath);

        InitializeCategories();
        StatusMessage = $"Content Builder initialized - Data: {dataPath}";

        Log.Information("Categories initialized - Total categories: {Count}", Categories.Count);
    }

    private string GetDataDirectory()
    {
        // Navigate from ContentBuilder bin folder to Game.Data/Data/Json
        // Typical path: console-game/Game.ContentBuilder/bin/Debug/net9.0-windows/
        // Target path: console-game/Game.Data/Data/Json/
        var baseDir = AppDomain.CurrentDomain.BaseDirectory; // bin/Debug/net9.0-windows/
        var solutionRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..")); // console-game/
        var dataPath = Path.Combine(solutionRoot, "Game.Data", "Data", "Json");

        if (!Directory.Exists(dataPath))
        {
            throw new DirectoryNotFoundException($"Data directory not found: {dataPath}");
        }

        return dataPath;
    }

    private void InitializeCategories()
    {
        // Build category tree dynamically from file system
        Categories = _fileTreeService.BuildCategoryTree();
    }

    partial void OnSelectedCategoryChanged(CategoryNode? value)
    {
        if (value?.EditorType != EditorType.None && value is not null)
        {
            StatusMessage = $"Selected: {value.Name} ({value.Tag ?? "unknown"})";
            Log.Information("Category selected: {CategoryName} - Type: {EditorType} - File: {FileName}",
                value.Name, value.EditorType, value.Tag ?? "unknown");

            // Load appropriate editor based on EditorType
            switch (value.EditorType)
            {
                case EditorType.NameListEditor:
                    LoadNameListEditor(value.Tag?.ToString() ?? string.Empty);
                    break;
                case EditorType.CatalogEditor:
                    LoadCatalogEditor(value.Tag?.ToString() ?? string.Empty);
                    break;
                case EditorType.ComponentDataEditor:
                    LoadComponentDataEditor(value.Tag?.ToString() ?? string.Empty);
                    break;
                case EditorType.ConfigEditor:
                    LoadConfigEditor(value.Tag?.ToString() ?? string.Empty);
                    break;
                default:
                    CurrentEditor = null;
                    break;
            }
        }
        else
        {
            StatusMessage = "Select an item to edit";
            CurrentEditor = null;
        }
    }

    private void LoadNameListEditor(string fileName)
    {
        try
        {
            Log.Debug("Loading NameListEditor for {FileName}", fileName);
            var viewModel = new NameListEditorViewModel(_jsonEditorService, _catalogTokenService, fileName);
            var view = new NameListEditorView
            {
                DataContext = viewModel
            };
            CurrentEditor = view;
            StatusMessage = $"Loaded editor for {fileName}";
            Log.Information("NameListEditor loaded successfully for {FileName}", fileName);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load editor: {ex.Message}";
            Log.Error(ex, "Failed to load NameListEditor for {FileName}", fileName);
            CurrentEditor = null;
        }
    }

    private void LoadCatalogEditor(string fileName)
    {
        try
        {
            Log.Debug("Loading CatalogEditor for {FileName}", fileName);

            var viewModel = new CatalogEditorViewModel(_jsonEditorService, fileName);

            var view = new CatalogEditorView
            {
                DataContext = viewModel
            };

            CurrentEditor = view;
            StatusMessage = $"Loaded types editor for {fileName}";
            Log.Information("CatalogEditor loaded successfully for {FileName}", fileName);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load types editor: {ex.Message}";
            Log.Error(ex, "Failed to load CatalogEditor for {FileName}", fileName);
            CurrentEditor = null;
        }
    }

    private void LoadComponentDataEditor(string fileName)
    {
        try
        {
            Log.Debug("Loading ComponentDataEditor for {FileName}", fileName);

            var viewModel = new ComponentDataEditorViewModel(_jsonEditorService, fileName);

            var view = new ComponentDataEditorView
            {
                DataContext = viewModel
            };

            CurrentEditor = view;
            StatusMessage = $"Loaded component editor for {fileName}";
            Log.Information("ComponentDataEditor loaded successfully for {FileName}", fileName);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load component editor: {ex.Message}";
            Log.Error(ex, "Failed to load ComponentDataEditor for {FileName}", fileName);
            CurrentEditor = null;
        }
    }

    private void LoadConfigEditor(string fileName)
    {
        try
        {
            Log.Debug("Loading ConfigEditor for {FileName}", fileName);

            var viewModel = new ConfigEditorViewModel(_jsonEditorService, fileName);

            var view = new ConfigEditorView
            {
                DataContext = viewModel
            };

            CurrentEditor = view;
            StatusMessage = $"Loaded config editor for {fileName}";
            Log.Information("ConfigEditor loaded successfully for {FileName}", fileName);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load config editor: {ex.Message}";
            Log.Error(ex, "Failed to load ConfigEditor for {FileName}", fileName);
            CurrentEditor = null;
        }
    }

    [RelayCommand]
    private void ShowPreview()
    {
        try
        {
            Log.Information("Opening preview window");
            var previewWindow = new Views.PreviewWindow
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            previewWindow.ShowDialog();
            Log.Information("Preview window closed");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to open preview: {ex.Message}";
            Log.Error(ex, "Failed to open preview window");
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        try
        {
            Log.Information("Refreshing file tree");

            // Clear current editor
            CurrentEditor = null;
            SelectedCategory = null;

            // Rebuild category tree
            Categories = _fileTreeService.BuildCategoryTree();

            StatusMessage = $"File tree refreshed - {Categories.Sum(c => c.TotalFileCount)} files found";
            Log.Information("File tree refreshed successfully - {FileCount} files",
                Categories.Sum(c => c.TotalFileCount));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to refresh: {ex.Message}";
            Log.Error(ex, "Failed to refresh file tree");
        }
    }

    [RelayCommand]
    private void Exit()
    {
        Log.Information("Exit command invoked");
        System.Windows.Application.Current.Shutdown();
    }
}

