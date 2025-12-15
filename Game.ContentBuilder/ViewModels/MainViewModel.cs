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
        // Complete category tree with all 93 JSON files
        Categories = new ObservableCollection<CategoryNode>
        {
            new CategoryNode
            {
                Name = "General",
                Icon = "Earth",
                Children = new ObservableCollection<CategoryNode>
                {
                    new CategoryNode { Name = "Colors", Icon = "Palette", EditorType = EditorType.HybridArray, Tag = "general/colors.json" },
                    new CategoryNode { Name = "Smells", Icon = "Flower", EditorType = EditorType.HybridArray, Tag = "general/smells.json" },
                    new CategoryNode { Name = "Sounds", Icon = "VolumeHigh", EditorType = EditorType.HybridArray, Tag = "general/sounds.json" },
                    new CategoryNode { Name = "Textures", Icon = "Texture", EditorType = EditorType.HybridArray, Tag = "general/textures.json" },
                    new CategoryNode { Name = "Time of Day", Icon = "ClockOutline", EditorType = EditorType.HybridArray, Tag = "general/time_of_day.json" },
                    new CategoryNode { Name = "Weather", Icon = "WeatherPartlyCloudy", EditorType = EditorType.HybridArray, Tag = "general/weather.json" },
                    new CategoryNode { Name = "Verbs", Icon = "Run", EditorType = EditorType.HybridArray, Tag = "general/verbs.json" },
                    new CategoryNode { Name = "Adjectives", Icon = "FormatListBulleted", EditorType = EditorType.NameList, Tag = "general/adjectives.json" },
                    new CategoryNode { Name = "Materials", Icon = "HammerWrench", EditorType = EditorType.NameList, Tag = "general/materials.json" }
                }
            },
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
                            new CategoryNode { Name = "Names", Icon = "FormatListBulleted", EditorType = EditorType.HybridArray, Tag = "items/weapons/names.json" },
                            new CategoryNode { Name = "Prefixes", Icon = "FileEdit", EditorType = EditorType.FlatItem, Tag = "items/weapons/prefixes.json" },
                            new CategoryNode { Name = "Suffixes", Icon = "FileEdit", EditorType = EditorType.HybridArray, Tag = "items/weapons/suffixes.json" }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Armor",
                        Icon = "ShieldOutline",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "Names", Icon = "FormatListBulleted", EditorType = EditorType.HybridArray, Tag = "items/armor/names.json" },
                            new CategoryNode { Name = "Prefixes", Icon = "FileEdit", EditorType = EditorType.HybridArray, Tag = "items/armor/prefixes.json" },
                            new CategoryNode { Name = "Suffixes", Icon = "FileEdit", EditorType = EditorType.HybridArray, Tag = "items/armor/suffixes.json" },
                            new CategoryNode { Name = "Materials", Icon = "HammerWrench", EditorType = EditorType.FlatItem, Tag = "items/armor/materials.json" }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Consumables",
                        Icon = "BottleTonicPlus",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "Names", Icon = "FormatListBulleted", EditorType = EditorType.HybridArray, Tag = "items/consumables/names.json" },
                            new CategoryNode { Name = "Effects", Icon = "AutoFix", EditorType = EditorType.HybridArray, Tag = "items/consumables/effects.json" },
                            new CategoryNode { Name = "Rarities", Icon = "Star", EditorType = EditorType.HybridArray, Tag = "items/consumables/rarities.json" }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Enchantments",
                        Icon = "AutoFix",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "Prefixes", Icon = "FileEdit", EditorType = EditorType.HybridArray, Tag = "items/enchantments/prefixes.json" },
                            new CategoryNode { Name = "Effects", Icon = "Sparkles", EditorType = EditorType.HybridArray, Tag = "items/enchantments/effects.json" },
                            new CategoryNode { Name = "Suffixes", Icon = "FileEdit", EditorType = EditorType.FlatItem, Tag = "items/enchantments/suffixes.json" }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Materials",
                        Icon = "HammerWrench",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "Metals", Icon = "Anvil", EditorType = EditorType.FlatItem, Tag = "items/materials/metals.json" },
                            new CategoryNode { Name = "Woods", Icon = "Tree", EditorType = EditorType.FlatItem, Tag = "items/materials/woods.json" },
                            new CategoryNode { Name = "Leathers", Icon = "Leather", EditorType = EditorType.FlatItem, Tag = "items/materials/leathers.json" },
                            new CategoryNode { Name = "Gemstones", Icon = "Diamond", EditorType = EditorType.FlatItem, Tag = "items/materials/gemstones.json" }
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
                            new CategoryNode { Name = "Names", Icon = "FormatListBulleted", EditorType = EditorType.NameList, Tag = "enemies/beasts/names.json" },
                            new CategoryNode { Name = "Prefixes", Icon = "FileEdit", EditorType = EditorType.FlatItem, Tag = "enemies/beasts/prefixes.json" },
                            new CategoryNode { Name = "Traits", Icon = "Star", EditorType = EditorType.HybridArray, Tag = "enemies/beasts/traits.json" },
                            new CategoryNode { Name = "Suffixes", Icon = "FileEdit", EditorType = EditorType.HybridArray, Tag = "enemies/beasts/suffixes.json" }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Demons",
                        Icon = "Fire",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "Names", Icon = "FormatListBulleted", EditorType = EditorType.NameList, Tag = "enemies/demons/names.json" },
                            new CategoryNode { Name = "Prefixes", Icon = "FileEdit", EditorType = EditorType.FlatItem, Tag = "enemies/demons/prefixes.json" },
                            new CategoryNode { Name = "Traits", Icon = "Star", EditorType = EditorType.HybridArray, Tag = "enemies/demons/traits.json" },
                            new CategoryNode { Name = "Suffixes", Icon = "FileEdit", EditorType = EditorType.HybridArray, Tag = "enemies/demons/suffixes.json" }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Dragons",
                        Icon = "Creation",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "Names", Icon = "FormatListBulleted", EditorType = EditorType.NameList, Tag = "enemies/dragons/names.json" },
                            new CategoryNode { Name = "Prefixes", Icon = "FileEdit", EditorType = EditorType.FlatItem, Tag = "enemies/dragons/prefixes.json" },
                            new CategoryNode { Name = "Colors", Icon = "Palette", EditorType = EditorType.FlatItem, Tag = "enemies/dragons/colors.json" },
                            new CategoryNode { Name = "Traits", Icon = "Star", EditorType = EditorType.HybridArray, Tag = "enemies/dragons/traits.json" },
                            new CategoryNode { Name = "Suffixes", Icon = "FileEdit", EditorType = EditorType.HybridArray, Tag = "enemies/dragons/suffixes.json" }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Elementals",
                        Icon = "Water",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "Names", Icon = "FormatListBulleted", EditorType = EditorType.NameList, Tag = "enemies/elementals/names.json" },
                            new CategoryNode { Name = "Prefixes", Icon = "FileEdit", EditorType = EditorType.FlatItem, Tag = "enemies/elementals/prefixes.json" },
                            new CategoryNode { Name = "Traits", Icon = "Star", EditorType = EditorType.HybridArray, Tag = "enemies/elementals/traits.json" },
                            new CategoryNode { Name = "Suffixes", Icon = "FileEdit", EditorType = EditorType.HybridArray, Tag = "enemies/elementals/suffixes.json" }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Humanoids",
                        Icon = "HumanGreeting",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "Names", Icon = "FormatListBulleted", EditorType = EditorType.NameList, Tag = "enemies/humanoids/names.json" },
                            new CategoryNode { Name = "Prefixes", Icon = "FileEdit", EditorType = EditorType.FlatItem, Tag = "enemies/humanoids/prefixes.json" },
                            new CategoryNode { Name = "Traits", Icon = "Star", EditorType = EditorType.HybridArray, Tag = "enemies/humanoids/traits.json" },
                            new CategoryNode { Name = "Suffixes", Icon = "FileEdit", EditorType = EditorType.HybridArray, Tag = "enemies/humanoids/suffixes.json" }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Undead",
                        Icon = "Ghost",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "Names", Icon = "FormatListBulleted", EditorType = EditorType.NameList, Tag = "enemies/undead/names.json" },
                            new CategoryNode { Name = "Prefixes", Icon = "FileEdit", EditorType = EditorType.FlatItem, Tag = "enemies/undead/prefixes.json" },
                            new CategoryNode { Name = "Traits", Icon = "Star", EditorType = EditorType.HybridArray, Tag = "enemies/undead/traits.json" },
                            new CategoryNode { Name = "Suffixes", Icon = "FileEdit", EditorType = EditorType.HybridArray, Tag = "enemies/undead/suffixes.json" }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Vampires",
                        Icon = "VampireFangs",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "Traits", Icon = "Star", EditorType = EditorType.HybridArray, Tag = "enemies/vampires/traits.json" },
                            new CategoryNode { Name = "Suffixes", Icon = "FileEdit", EditorType = EditorType.HybridArray, Tag = "enemies/vampires/suffixes.json" }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Goblinoids",
                        Icon = "MonsterGoblin",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "Traits", Icon = "Star", EditorType = EditorType.HybridArray, Tag = "enemies/goblinoids/traits.json" },
                            new CategoryNode { Name = "Suffixes", Icon = "FileEdit", EditorType = EditorType.HybridArray, Tag = "enemies/goblinoids/suffixes.json" }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Orcs",
                        Icon = "MonsterOrc",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "Traits", Icon = "Star", EditorType = EditorType.HybridArray, Tag = "enemies/orcs/traits.json" },
                            new CategoryNode { Name = "Suffixes", Icon = "FileEdit", EditorType = EditorType.HybridArray, Tag = "enemies/orcs/suffixes.json" }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Insects",
                        Icon = "Bug",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "Traits", Icon = "Star", EditorType = EditorType.HybridArray, Tag = "enemies/insects/traits.json" },
                            new CategoryNode { Name = "Suffixes", Icon = "FileEdit", EditorType = EditorType.HybridArray, Tag = "enemies/insects/suffixes.json" }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Plants",
                        Icon = "Sprout",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "Traits", Icon = "Star", EditorType = EditorType.HybridArray, Tag = "enemies/plants/traits.json" },
                            new CategoryNode { Name = "Suffixes", Icon = "FileEdit", EditorType = EditorType.HybridArray, Tag = "enemies/plants/suffixes.json" }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Reptilians",
                        Icon = "Snake",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "Traits", Icon = "Star", EditorType = EditorType.HybridArray, Tag = "enemies/reptilians/traits.json" },
                            new CategoryNode { Name = "Suffixes", Icon = "FileEdit", EditorType = EditorType.HybridArray, Tag = "enemies/reptilians/suffixes.json" }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Trolls",
                        Icon = "MonsterTroll",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "Traits", Icon = "Star", EditorType = EditorType.HybridArray, Tag = "enemies/trolls/traits.json" },
                            new CategoryNode { Name = "Suffixes", Icon = "FileEdit", EditorType = EditorType.HybridArray, Tag = "enemies/trolls/suffixes.json" }
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
                        Name = "Names",
                        Icon = "CardAccountDetails",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "First Names", Icon = "FormatListBulleted", EditorType = EditorType.HybridArray, Tag = "npcs/names/first_names.json" },
                            new CategoryNode { Name = "Last Names", Icon = "FormatListBulleted", EditorType = EditorType.HybridArray, Tag = "npcs/names/last_names.json" }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Personalities",
                        Icon = "EmoticonHappy",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "Traits", Icon = "Star", EditorType = EditorType.HybridArray, Tag = "npcs/personalities/traits.json" },
                            new CategoryNode { Name = "Quirks", Icon = "Puzzle", EditorType = EditorType.HybridArray, Tag = "npcs/personalities/quirks.json" },
                            new CategoryNode { Name = "Backgrounds", Icon = "Book", EditorType = EditorType.HybridArray, Tag = "npcs/personalities/backgrounds.json" }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Dialogue",
                        Icon = "CommentText",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "Greetings", Icon = "HandWave", EditorType = EditorType.HybridArray, Tag = "npcs/dialogue/greetings.json" },
                            new CategoryNode { Name = "Farewells", Icon = "HandPeace", EditorType = EditorType.HybridArray, Tag = "npcs/dialogue/farewells.json" },
                            new CategoryNode { Name = "Rumors", Icon = "Microphone", EditorType = EditorType.HybridArray, Tag = "npcs/dialogue/rumors.json" },
                            new CategoryNode { Name = "Templates", Icon = "FormatListBulleted", EditorType = EditorType.HybridArray, Tag = "npcs/dialogue/templates.json" },
                            new CategoryNode { Name = "Traits", Icon = "Star", EditorType = EditorType.FlatItem, Tag = "npcs/dialogue/traits.json" }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Occupations",
                        Icon = "Briefcase",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "Common", Icon = "AccountTie", EditorType = EditorType.FlatItem, Tag = "npcs/occupations/common.json" },
                            new CategoryNode { Name = "Criminal", Icon = "Incognito", EditorType = EditorType.FlatItem, Tag = "npcs/occupations/criminal.json" },
                            new CategoryNode { Name = "Magical", Icon = "AutoFix", EditorType = EditorType.FlatItem, Tag = "npcs/occupations/magical.json" },
                            new CategoryNode { Name = "Noble", Icon = "Crown", EditorType = EditorType.FlatItem, Tag = "npcs/occupations/noble.json" }
                        }
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
                        Name = "Objectives",
                        Icon = "ChecklistCheck",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "Primary", Icon = "Numeric1Circle", EditorType = EditorType.HybridArray, Tag = "quests/objectives/primary.json" },
                            new CategoryNode { Name = "Secondary", Icon = "Numeric2Circle", EditorType = EditorType.HybridArray, Tag = "quests/objectives/secondary.json" },
                            new CategoryNode { Name = "Hidden", Icon = "EyeOff", EditorType = EditorType.HybridArray, Tag = "quests/objectives/hidden.json" }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Rewards",
                        Icon = "TreasureChest",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "Gold", Icon = "CurrencyUsd", EditorType = EditorType.HybridArray, Tag = "quests/rewards/gold.json" },
                            new CategoryNode { Name = "Experience", Icon = "ChartLine", EditorType = EditorType.HybridArray, Tag = "quests/rewards/experience.json" },
                            new CategoryNode { Name = "Items", Icon = "Gift", EditorType = EditorType.HybridArray, Tag = "quests/rewards/items.json" }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Locations",
                        Icon = "MapMarker",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "Towns", Icon = "TownHall", EditorType = EditorType.HybridArray, Tag = "quests/locations/towns.json" },
                            new CategoryNode { Name = "Dungeons", Icon = "CastleOutline", EditorType = EditorType.HybridArray, Tag = "quests/locations/dungeons.json" },
                            new CategoryNode { Name = "Wilderness", Icon = "Forest", EditorType = EditorType.HybridArray, Tag = "quests/locations/wilderness.json" }
                        }
                    },
                    new CategoryNode
                    {
                        Name = "Templates",
                        Icon = "FileDocument",
                        Children = new ObservableCollection<CategoryNode>
                        {
                            new CategoryNode { Name = "Kill", Icon = "Sword", EditorType = EditorType.FlatItem, Tag = "quests/templates/kill.json" },
                            new CategoryNode { Name = "Delivery", Icon = "PackageVariant", EditorType = EditorType.FlatItem, Tag = "quests/templates/delivery.json" },
                            new CategoryNode { Name = "Escort", Icon = "AccountArrowRight", EditorType = EditorType.FlatItem, Tag = "quests/templates/escort.json" },
                            new CategoryNode { Name = "Fetch", Icon = "MagnifyFindReplace", EditorType = EditorType.FlatItem, Tag = "quests/templates/fetch.json" },
                            new CategoryNode { Name = "Investigate", Icon = "Magnify", EditorType = EditorType.FlatItem, Tag = "quests/templates/investigate.json" }
                        }
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
                
                case EditorType.HybridArray:
                    LoadHybridArrayEditor(value.Tag?.ToString() ?? "");
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

    private void LoadHybridArrayEditor(string fileName)
    {
        try
        {
            var viewModel = new HybridArrayEditorViewModel(_jsonEditorService, fileName);
            var view = new HybridArrayEditorView
            {
                DataContext = viewModel
            };
            CurrentEditor = view;
            StatusMessage = $"Loaded hybrid array editor for {fileName}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load hybrid array editor: {ex.Message}";
            Log.Error(ex, "Failed to load hybrid array editor for {FileName}", fileName);
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
