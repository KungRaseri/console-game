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
        // Initialize JsonEditorService with path to Game.Shared Data directory
        var dataPath = GetDataDirectory();
        _jsonEditorService = new JsonEditorService(dataPath);
        _fileTreeService = new FileTreeService(dataPath);
        
        Log.Information("MainViewModel initialized - Data directory: {DataPath}", dataPath);
        
        InitializeCategories();
        StatusMessage = $"Content Builder initialized - Data: {dataPath}";
        
        Log.Information("Categories initialized - Total categories: {Count}", Categories.Count);
    }

    private string GetDataDirectory()
    {
        // Navigate from ContentBuilder bin folder to Game.Shared/Data/Json
        // Typical path: console-game/Game.ContentBuilder/bin/Debug/net9.0-windows/
        // Target path: console-game/Game.Shared/Data/Json/
        var baseDir = AppDomain.CurrentDomain.BaseDirectory; // bin/Debug/net9.0-windows/
        var solutionRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..")); // console-game/
        var dataPath = Path.Combine(solutionRoot, "Game.Shared", "Data", "Json");
        
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
                case EditorType.ItemPrefix:
                case EditorType.ItemSuffix:
                    LoadItemEditor(value.Tag?.ToString() ?? "");
                    break;
                
                case EditorType.FlatItem:
                    LoadFlatItemEditor(value.Tag?.ToString() ?? "");
                    break;
                
                case EditorType.NameList:
                    LoadNameListEditor(value.Tag?.ToString() ?? "");
                    break;
                
                case EditorType.HybridArray:
                    LoadHybridArrayEditor(value.Tag?.ToString() ?? "");
                    break;
                
                case EditorType.NamesEditor:
                    LoadNamesEditor(value.Tag?.ToString() ?? "");
                    break;
                
                case EditorType.ItemCatalogEditor:
                    LoadCatalogEditor(value.Tag?.ToString() ?? "");
                    break;
                
                case EditorType.AbilitiesEditor:
                    LoadAbilitiesEditor(value.Tag?.ToString() ?? "");
                    break;
                
                case EditorType.ComponentEditor:
                case EditorType.MaterialEditor:
                case EditorType.TraitEditor:
                case EditorType.CatalogEditor:
                case EditorType.NameCatalogEditor:
                case EditorType.QuestTemplateEditor:
                case EditorType.QuestDataEditor:
                case EditorType.ConfigEditor:
                    // TODO: Implement these specialized editors
                    StatusMessage = $"Editor for {value.EditorType} not yet implemented";
                    CurrentEditor = null;
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

    private void LoadItemEditor(string fileName)
    {
        try
        {
            Log.Debug("Loading ItemEditor for {FileName}", fileName);
            var viewModel = new ItemEditorViewModel(_jsonEditorService, fileName);
            var view = new ItemEditorView
            {
                DataContext = viewModel
            };
            CurrentEditor = view;
            StatusMessage = $"Loaded editor for {fileName}";
            Log.Information("ItemEditor loaded successfully for {FileName}", fileName);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load editor: {ex.Message}";
            Log.Error(ex, "Failed to load ItemEditor for {FileName}", fileName);
            CurrentEditor = null;
        }
    }

    private void LoadFlatItemEditor(string fileName)
    {
        try
        {
            Log.Debug("Loading FlatItemEditor for {FileName}", fileName);
            var viewModel = new FlatItemEditorViewModel(_jsonEditorService, fileName);
            var view = new FlatItemEditorView
            {
                DataContext = viewModel
            };
            CurrentEditor = view;
            StatusMessage = $"Loaded editor for {fileName}";
            Log.Information("FlatItemEditor loaded successfully for {FileName}", fileName);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load editor: {ex.Message}";
            Log.Error(ex, "Failed to load FlatItemEditor for {FileName}", fileName);
            CurrentEditor = null;
        }
    }

    private void LoadNameListEditor(string fileName)
    {
        try
        {
            Log.Debug("Loading NameListEditor for {FileName}", fileName);
            var viewModel = new NameListEditorViewModel(_jsonEditorService, fileName);
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

    private void LoadHybridArrayEditor(string fileName)
    {
        try
        {
            Log.Debug("Loading HybridArrayEditor for {FileName}", fileName);
            var viewModel = new HybridArrayEditorViewModel(_jsonEditorService, fileName);
            var view = new HybridArrayEditorView
            {
                DataContext = viewModel
            };
            CurrentEditor = view;
            StatusMessage = $"Loaded hybrid array editor for {fileName}";
            Log.Information("HybridArrayEditor loaded successfully for {FileName}", fileName);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load hybrid array editor: {ex.Message}";
            Log.Error(ex, "Failed to load HybridArrayEditor for {FileName}", fileName);
            CurrentEditor = null;
        }
    }

    private void LoadNamesEditor(string fileName)
    {
        try
        {
            Log.Debug("Loading NamesEditor for {FileName}", fileName);
            
            var fullPath = Path.Combine(GetDataDirectory(), fileName);
            var viewModel = new NamesEditorViewModel();
            viewModel.LoadFile(fullPath);
            
            var view = new NamesEditorView
            {
                DataContext = viewModel
            };
            
            CurrentEditor = view;
            StatusMessage = $"Loaded names editor for {fileName}";
            Log.Information("NamesEditor loaded successfully for {FileName}", fileName);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load names editor: {ex.Message}";
            Log.Error(ex, "Failed to load NamesEditor for {FileName}", fileName);
            CurrentEditor = null;
        }
    }

    private void LoadCatalogEditor(string fileName)
    {
        try
        {
            Log.Debug("Loading CatalogEditor for {FileName}", fileName);
            
            var fullPath = Path.Combine(GetDataDirectory(), fileName);
            var viewModel = new CatalogEditorViewModel();
            viewModel.LoadFile(fullPath);
            
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

    private void LoadAbilitiesEditor(string fileName)
    {
        try
        {
            Log.Debug("Loading AbilitiesEditor for {FileName}", fileName);
            
            var fullPath = Path.Combine(GetDataDirectory(), fileName);
            var viewModel = new AbilitiesEditorViewModel();
            viewModel.LoadFile(fullPath);
            
            var view = new AbilitiesEditorView
            {
                DataContext = viewModel
            };
            
            CurrentEditor = view;
            StatusMessage = $"Loaded abilities editor for {fileName}";
            Log.Information("AbilitiesEditor loaded successfully for {FileName}", fileName);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load abilities editor: {ex.Message}";
            Log.Error(ex, "Failed to load AbilitiesEditor for {FileName}", fileName);
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

