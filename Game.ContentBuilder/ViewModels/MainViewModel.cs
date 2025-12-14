using System;
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
                                Name = "Names",
                                Icon = "FormatListBulleted",
                                EditorType = EditorType.NameList,
                                Tag = "weapon_names.json"
                            }
                            // Note: weapon_suffixes.json doesn't exist yet
                            // Note: metals.json, woods.json have flat structure - using FlatItemEditor
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
                                Name = "Materials",
                                Icon = "FileEdit",
                                EditorType = EditorType.ItemPrefix,
                                Tag = "armor_materials.json"
                            }
                            // Note: armor_prefixes.json and armor_suffixes.json don't exist yet
                            // Note: leathers.json has flat structure - using FlatItemEditor
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Enchantments",
                        Icon = "AutoFix",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode
                            {
                                Name = "Suffixes",
                                Icon = "FileEdit",
                                EditorType = EditorType.ItemSuffix,
                                Tag = "enchantment_suffixes.json"
                            }
                            // Note: gemstones.json has flat structure - using FlatItemEditor
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Materials",
                        Icon = "HammerWrench",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode
                            {
                                Name = "Metals",
                                Icon = "FileEdit",
                                EditorType = EditorType.FlatItem,
                                Tag = "metals.json"
                            },
                            new CategoryNode
                            {
                                Name = "Woods",
                                Icon = "FileEdit",
                                EditorType = EditorType.FlatItem,
                                Tag = "woods.json"
                            },
                            new CategoryNode
                            {
                                Name = "Leathers",
                                Icon = "FileEdit",
                                EditorType = EditorType.FlatItem,
                                Tag = "leathers.json"
                            },
                            new CategoryNode
                            {
                                Name = "Gemstones",
                                Icon = "FileEdit",
                                EditorType = EditorType.FlatItem,
                                Tag = "gemstones.json"
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
                        Name = "Beasts",
                        Icon = "Paw",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode
                            {
                                Name = "Names",
                                Icon = "FormatListBulleted",
                                EditorType = EditorType.NameList,
                                Tag = "enemies/beast_names.json"
                            },
                            new CategoryNode
                            {
                                Name = "Prefixes",
                                Icon = "FileEdit",
                                EditorType = EditorType.ItemPrefix,
                                Tag = "enemies/beast_prefixes.json"
                            }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Demons",
                        Icon = "Fire",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode
                            {
                                Name = "Names",
                                Icon = "FormatListBulleted",
                                EditorType = EditorType.NameList,
                                Tag = "enemies/demon_names.json"
                            },
                            new CategoryNode
                            {
                                Name = "Prefixes",
                                Icon = "FileEdit",
                                EditorType = EditorType.ItemPrefix,
                                Tag = "enemies/demon_prefixes.json"
                            }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Dragons",
                        Icon = "Creation",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode
                            {
                                Name = "Names",
                                Icon = "FormatListBulleted",
                                EditorType = EditorType.NameList,
                                Tag = "enemies/dragon_names.json"
                            },
                            new CategoryNode
                            {
                                Name = "Prefixes",
                                Icon = "FileEdit",
                                EditorType = EditorType.ItemPrefix,
                                Tag = "enemies/dragon_prefixes.json"
                            },
                            new CategoryNode
                            {
                                Name = "Colors",
                                Icon = "Palette",
                                EditorType = EditorType.FlatItem,
                                Tag = "enemies/dragon_colors.json"
                            }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Elementals",
                        Icon = "Water",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode
                            {
                                Name = "Names",
                                Icon = "FormatListBulleted",
                                EditorType = EditorType.NameList,
                                Tag = "enemies/elemental_names.json"
                            },
                            new CategoryNode
                            {
                                Name = "Prefixes",
                                Icon = "FileEdit",
                                EditorType = EditorType.ItemPrefix,
                                Tag = "enemies/elemental_prefixes.json"
                            }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Humanoids",
                        Icon = "HumanGreeting",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode
                            {
                                Name = "Names",
                                Icon = "FormatListBulleted",
                                EditorType = EditorType.NameList,
                                Tag = "enemies/humanoid_names.json"
                            },
                            new CategoryNode
                            {
                                Name = "Prefixes",
                                Icon = "FileEdit",
                                EditorType = EditorType.ItemPrefix,
                                Tag = "enemies/humanoid_prefixes.json"
                            }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Undead",
                        Icon = "Ghost",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode
                            {
                                Name = "Names",
                                Icon = "FormatListBulleted",
                                EditorType = EditorType.NameList,
                                Tag = "enemies/undead_names.json"
                            },
                            new CategoryNode
                            {
                                Name = "Prefixes",
                                Icon = "FileEdit",
                                EditorType = EditorType.ItemPrefix,
                                Tag = "enemies/undead_prefixes.json"
                            }
                        }
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
                        Name = "Fantasy Names",
                        Icon = "FormatListBulleted",
                        EditorType = EditorType.NameList,
                        Tag = "npcs/fantasy_names.json"
                    },
                    new CategoryNode
                    {
                        Name = "Occupations",
                        Icon = "FileEdit",
                        EditorType = EditorType.ItemPrefix,
                        Tag = "npcs/occupations.json"
                    },
                    new CategoryNode
                    {
                        Name = "Dialogue Templates",
                        Icon = "FormatListBulleted",
                        EditorType = EditorType.NameList,
                        Tag = "npcs/dialogue_templates.json"
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
                        EditorType = EditorType.ItemPrefix,
                        Tag = "quests/quest_templates.json"
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
                
                case EditorType.FlatItem:
                    LoadFlatItemEditor(value.Tag?.ToString() ?? "");
                    break;
                
                case EditorType.NameList:
                    LoadNameListEditor(value.Tag?.ToString() ?? "");
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

    private void LoadFlatItemEditor(string fileName)
    {
        try
        {
            var viewModel = new FlatItemEditorViewModel(_jsonEditorService, fileName);
            var view = new FlatItemEditorView
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

    private void LoadNameListEditor(string fileName)
    {
        try
        {
            var viewModel = new NameListEditorViewModel(_jsonEditorService, fileName);
            var view = new NameListEditorView
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
    private void ShowPreview()
    {
        try
        {
            var previewWindow = new Views.PreviewWindow
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            previewWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to open preview: {ex.Message}";
            Log.Error(ex, "Failed to open preview window");
        }
    }

    [RelayCommand]
    private void Exit()
    {
        System.Windows.Application.Current.Shutdown();
    }
}
