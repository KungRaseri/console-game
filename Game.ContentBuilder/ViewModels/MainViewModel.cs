using System;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Game.ContentBuilder.Models;
using Game.ContentBuilder.Services;
using Game.ContentBuilder.Views;

namespace Game.ContentBuilder.ViewModels;

/// <summary>
/// Main ViewModel for the Content Builder application
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly JsonEditorService _jsonEditorService;

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
        
        InitializeCategories();
        StatusMessage = $"Content Builder initialized - Data: {dataPath}";
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
        // Create placeholder category tree structure
        Categories = new ObservableCollection<CategoryNode>
        {
            new CategoryNode
            {
                Name = "Items",
                Icon = "Sword",
                Children = new ObservableCollection<CategoryNode>
                {
                    new CategoryNode
                    {
                        Name = "Weapons",
                        Icon = "SwordCross",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode
                            {
                                Name = "Prefixes",
                                Icon = "FileEdit",
                                EditorType = EditorType.ItemPrefix,
                                Tag = "weapon_prefixes.json"
                            },
                            new CategoryNode
                            {
                                Name = "Suffixes",
                                Icon = "FileEdit",
                                EditorType = EditorType.ItemSuffix,
                                Tag = "weapon_suffixes.json"
                            }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Armor",
                        Icon = "ShieldOutline",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode
                            {
                                Name = "Prefixes",
                                Icon = "FileEdit",
                                EditorType = EditorType.ItemPrefix,
                                Tag = "armor_prefixes.json"
                            },
                            new CategoryNode
                            {
                                Name = "Suffixes",
                                Icon = "FileEdit",
                                EditorType = EditorType.ItemSuffix,
                                Tag = "armor_suffixes.json"
                            }
                        }
                    }
                }
            },
            new CategoryNode
            {
                Name = "Enemies",
                Icon = "Skull",
                Children = new ObservableCollection<CategoryNode>
                {
                    new CategoryNode
                    {
                        Name = "Names",
                        Icon = "FileEdit",
                        EditorType = EditorType.EnemyNames,
                        Tag = "enemy_names.json"
                    }
                }
            },
            new CategoryNode
            {
                Name = "NPCs",
                Icon = "AccountGroup",
                Children = new ObservableCollection<CategoryNode>
                {
                    new CategoryNode
                    {
                        Name = "Names",
                        Icon = "FileEdit",
                        EditorType = EditorType.NpcNames,
                        Tag = "npc_names.json"
                    }
                }
            },
            new CategoryNode
            {
                Name = "Quests",
                Icon = "BookOpenVariant",
                Children = new ObservableCollection<CategoryNode>
                {
                    new CategoryNode
                    {
                        Name = "Quest Templates",
                        Icon = "FileEdit",
                        EditorType = EditorType.Quest,
                        Tag = "quest_templates.json"
                    }
                }
            }
        };
    }

    partial void OnSelectedCategoryChanged(CategoryNode? value)
    {
        if (value?.EditorType != EditorType.None && value is not null)
        {
            StatusMessage = $"Selected: {value.Name} ({value.Tag ?? "unknown"})";
            
            // Load appropriate editor based on EditorType
            switch (value.EditorType)
            {
                case EditorType.ItemPrefix:
                case EditorType.ItemSuffix:
                    LoadItemEditor(value.Tag?.ToString() ?? "");
                    break;
                
                case EditorType.EnemyNames:
                case EditorType.NpcNames:
                case EditorType.Quest:
                    // TODO: Implement other editors in future tasks
                    StatusMessage = $"Editor for {value.Name} not yet implemented";
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
            var viewModel = new ItemEditorViewModel(_jsonEditorService, fileName);
            var view = new ItemEditorView
            {
                DataContext = viewModel
            };
            CurrentEditor = view;
            StatusMessage = $"Loaded editor for {fileName}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load editor: {ex.Message}";
            CurrentEditor = null;
        }
    }

    [RelayCommand]
    private void Exit()
    {
        System.Windows.Application.Current.Shutdown();
    }
}
