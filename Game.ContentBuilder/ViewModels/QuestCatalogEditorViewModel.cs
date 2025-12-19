using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Game.ContentBuilder.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Game.ContentBuilder.ViewModels;

/// <summary>
/// Editor for Quest v4.0 catalog.json - Templates and Locations
/// </summary>
public partial class QuestCatalogEditorViewModel : ObservableObject
{
    private readonly JsonEditorService _jsonEditorService;
    private readonly string _storedFileName;
    private JObject? _rootJson;
    
    private static readonly Regex PlaceholderRegex = new(@"\{(\w+)\}", RegexOptions.Compiled);

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _tabSelection = "templates"; // "templates" or "locations"

    // Templates Section
    [ObservableProperty]
    private ObservableCollection<QuestTypeNode> _questTypes = new();

    [ObservableProperty]
    private QuestTypeNode? _selectedQuestType;

    [ObservableProperty]
    private QuestDifficultyNode? _selectedDifficulty;

    [ObservableProperty]
    private ObservableCollection<QuestTemplateViewModel> _templates = new();

    [ObservableProperty]
    private QuestTemplateViewModel? _selectedTemplate;

    // Locations Section
    [ObservableProperty]
    private ObservableCollection<LocationCategoryNode> _locationCategories = new();

    [ObservableProperty]
    private LocationCategoryNode? _selectedLocationCategory;

    [ObservableProperty]
    private LocationDangerNode? _selectedDangerLevel;

    [ObservableProperty]
    private ObservableCollection<LocationViewModel> _locations = new();

    [ObservableProperty]
    private LocationViewModel? _selectedLocation;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private bool _isEditMode;

    // Edit properties - Template
    [ObservableProperty]
    private string _editName = string.Empty;

    [ObservableProperty]
    private string _editDisplayName = string.Empty;

    [ObservableProperty]
    private string _editDescription = string.Empty;

    [ObservableProperty]
    private int _editRarityWeight = 10;

    [ObservableProperty]
    private string _editQuestType = string.Empty;

    [ObservableProperty]
    private string _editDifficulty = string.Empty;

    [ObservableProperty]
    private int _editBaseGoldReward = 100;

    [ObservableProperty]
    private int _editBaseXpReward = 100;

    [ObservableProperty]
    private string _editLocation = string.Empty;

    [ObservableProperty]
    private int _editMinQuantity = 1;

    [ObservableProperty]
    private int _editMaxQuantity = 1;

    [ObservableProperty]
    private string _editItemType = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _detectedPlaceholders = new();

    [ObservableProperty]
    private string _previewDescription = string.Empty;

    // Confirmation dialog state
    [ObservableProperty]
    private bool _showDeleteConfirmation;

    [ObservableProperty]
    private string _confirmationMessage = string.Empty;

    [ObservableProperty]
    private object? _pendingDeleteItem;

    public QuestCatalogEditorViewModel(JsonEditorService jsonEditorService, string fileName)
    {
        _jsonEditorService = jsonEditorService;
        _storedFileName = fileName;
        FileName = Path.GetFileNameWithoutExtension(fileName);
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            var filePath = _jsonEditorService.GetFilePath(_storedFileName);
            var content = File.ReadAllText(filePath);
            _rootJson = JObject.Parse(content);

            LoadTemplates();
            LoadLocations();
            IsDirty = false;
            StatusMessage = "Quest catalog loaded successfully";
            Log.Information("Loaded quest catalog: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            var filePath = _jsonEditorService.GetFilePath(_storedFileName);
            Log.Error(ex, "Failed to load quest catalog: {FilePath}", filePath);
            StatusMessage = $"Error loading file: {ex.Message}";
        }
    }

    private void LoadTemplates()
    {
        QuestTypes.Clear();

        var templatesNode = _rootJson?["components"]?["templates"];
        if (templatesNode is not JObject templates)
        {
            Log.Warning("No templates found in quest catalog");
            return;
        }

        foreach (var questType in templates.Properties())
        {
            var typeNode = new QuestTypeNode
            {
                Name = questType.Name,
                DisplayName = FormatQuestTypeName(questType.Name),
                Difficulties = new ObservableCollection<QuestDifficultyNode>()
            };

            // Load difficulty levels
            if (questType.Value is JObject difficulties)
            {
                foreach (var difficulty in difficulties.Properties())
                {
                    var templateCount = difficulty.Value is JArray arr ? arr.Count : 0;
                    var diffNode = new QuestDifficultyNode
                    {
                        Name = difficulty.Name,
                        DisplayName = FormatDifficultyName(difficulty.Name),
                        TemplateCount = templateCount,
                        ParentQuestType = typeNode
                    };
                    typeNode.Difficulties.Add(diffNode);
                }
            }

            QuestTypes.Add(typeNode);
        }

        // Select first type/difficulty if available
        if (QuestTypes.Any())
        {
            SelectedQuestType = QuestTypes[0];
            if (SelectedQuestType.Difficulties.Any())
            {
                SelectedDifficulty = SelectedQuestType.Difficulties[0];
            }
        }
    }

    private void LoadLocations()
    {
        LocationCategories.Clear();

        var locationsNode = _rootJson?["components"]?["locations"];
        if (locationsNode is not JObject locations)
        {
            Log.Warning("No locations found in quest catalog");
            return;
        }

        foreach (var category in locations.Properties())
        {
            var categoryNode = new LocationCategoryNode
            {
                Name = category.Name,
                DisplayName = FormatCategoryName(category.Name),
                DangerLevels = new ObservableCollection<LocationDangerNode>()
            };

            // Load danger levels
            if (category.Value is JObject dangerLevels)
            {
                foreach (var dangerLevel in dangerLevels.Properties())
                {
                    var locationCount = dangerLevel.Value is JArray arr ? arr.Count : 0;
                    var dangerNode = new LocationDangerNode
                    {
                        Name = dangerLevel.Name,
                        DisplayName = FormatDangerName(dangerLevel.Name),
                        LocationCount = locationCount,
                        ParentCategory = categoryNode
                    };
                    categoryNode.DangerLevels.Add(dangerNode);
                }
            }

            LocationCategories.Add(categoryNode);
        }

        // Select first category/danger level if available
        if (LocationCategories.Any())
        {
            SelectedLocationCategory = LocationCategories[0];
            if (SelectedLocationCategory.DangerLevels.Any())
            {
                SelectedDangerLevel = SelectedLocationCategory.DangerLevels[0];
            }
        }
    }

    partial void OnSelectedDifficultyChanged(QuestDifficultyNode? value)
    {
        if (value == null) return;
        LoadTemplatesForDifficulty();
    }

    partial void OnSelectedDangerLevelChanged(LocationDangerNode? value)
    {
        if (value == null) return;
        LoadLocationsForDangerLevel();
    }

    private void LoadTemplatesForDifficulty()
    {
        Templates.Clear();

        if (SelectedQuestType == null || SelectedDifficulty == null) return;

        var path = _rootJson?["components"]?["templates"]?[SelectedQuestType.Name]?[SelectedDifficulty.Name];
        if (path is not JArray templates)
        {
            StatusMessage = $"No templates found for {SelectedDifficulty.DisplayName}";
            return;
        }

        foreach (var template in templates.OfType<JObject>())
        {
            var vm = CreateTemplateViewModel(template);
            Templates.Add(vm);
        }

        StatusMessage = $"Loaded {Templates.Count} template(s) for {SelectedQuestType.DisplayName} - {SelectedDifficulty.DisplayName}";
    }

    private void LoadLocationsForDangerLevel()
    {
        Locations.Clear();

        if (SelectedLocationCategory == null || SelectedDangerLevel == null) return;

        var path = _rootJson?["components"]?["locations"]?[SelectedLocationCategory.Name]?[SelectedDangerLevel.Name];
        if (path is not JArray locationArray)
        {
            StatusMessage = $"No locations found for {SelectedDangerLevel.DisplayName}";
            return;
        }

        foreach (var location in locationArray.OfType<JObject>())
        {
            var vm = CreateLocationViewModel(location);
            Locations.Add(vm);
        }

        StatusMessage = $"Loaded {Locations.Count} location(s) for {SelectedLocationCategory.DisplayName} - {SelectedDangerLevel.DisplayName}";
    }

    private QuestTemplateViewModel CreateTemplateViewModel(JObject template)
    {
        return new QuestTemplateViewModel
        {
            Name = template["name"]?.ToString() ?? "",
            DisplayName = template["displayName"]?.ToString() ?? "",
            Description = template["description"]?.ToString() ?? "",
            RarityWeight = template["rarityWeight"]?.Value<int>() ?? 10,
            QuestType = template["questType"]?.ToString() ?? "",
            Difficulty = template["difficulty"]?.ToString() ?? "",
            BaseGoldReward = template["baseGoldReward"]?.Value<int>() ?? 0,
            BaseXpReward = template["baseXpReward"]?.Value<int>() ?? 0,
            Location = template["location"]?.ToString() ?? "",
            MinQuantity = template["minQuantity"]?.Value<int>() ?? 1,
            MaxQuantity = template["maxQuantity"]?.Value<int>() ?? 1,
            ItemType = template["itemType"]?.ToString() ?? "",
            JsonData = template
        };
    }

    private LocationViewModel CreateLocationViewModel(JObject location)
    {
        return new LocationViewModel
        {
            Name = location["name"]?.ToString() ?? "",
            DisplayName = location["displayName"]?.ToString() ?? "",
            Description = location["description"]?.ToString() ?? "",
            RarityWeight = location["rarityWeight"]?.Value<int>() ?? 10,
            LocationType = location["locationType"]?.ToString() ?? "",
            Danger = location["danger"]?.ToString() ?? "",
            Difficulty = location["difficulty"]?.ToString() ?? "",
            Features = location["features"]?.Values<string>().ToList() ?? new List<string>(),
            JsonData = location
        };
    }

    [RelayCommand]
    private void NewTemplate()
    {
        if (SelectedQuestType == null || SelectedDifficulty == null)
        {
            StatusMessage = "Please select a quest type and difficulty first";
            return;
        }

        IsEditMode = true;
        SelectedTemplate = null;
        EditName = $"New{SelectedQuestType.Name.Capitalize()}";
        EditDisplayName = $"New {SelectedQuestType.DisplayName}";
        EditDescription = "Describe the quest objective here. Use placeholders like {quantity}, {itemType}, {location}";
        EditRarityWeight = 10;
        EditQuestType = SelectedQuestType.Name;
        EditDifficulty = SelectedDifficulty.Name;
        EditBaseGoldReward = SelectedDifficulty.Name switch
        {
            "easy" or "easy_fetch" or "easy_combat" or "easy_escort" or "easy_delivery" or "easy_investigation" => 50,
            "medium" or "medium_fetch" or "medium_combat" or "medium_escort" or "medium_delivery" or "medium_investigation" => 150,
            "hard" or "hard_fetch" or "hard_combat" or "hard_escort" or "hard_delivery" or "hard_investigation" => 300,
            _ => 100
        };
        EditBaseXpReward = EditBaseGoldReward * 2;
        EditLocation = "Wilderness";
        EditMinQuantity = 1;
        EditMaxQuantity = 5;
        EditItemType = "item";
        DetectedPlaceholders.Clear();
        UpdatePreviewDescription();
        StatusMessage = "Create new template";
    }

    [RelayCommand]
    private void EditTemplate()
    {
        if (SelectedTemplate == null)
        {
            StatusMessage = "Please select a template to edit";
            return;
        }

        IsEditMode = true;
        EditName = SelectedTemplate.Name;
        EditDisplayName = SelectedTemplate.DisplayName;
        EditDescription = SelectedTemplate.Description;
        EditRarityWeight = SelectedTemplate.RarityWeight;
        EditQuestType = SelectedTemplate.QuestType;
        EditDifficulty = SelectedTemplate.Difficulty;
        EditBaseGoldReward = SelectedTemplate.BaseGoldReward;
        EditBaseXpReward = SelectedTemplate.BaseXpReward;
        EditLocation = SelectedTemplate.Location;
        EditMinQuantity = SelectedTemplate.MinQuantity;
        EditMaxQuantity = SelectedTemplate.MaxQuantity;
        EditItemType = SelectedTemplate.ItemType;
        DetectPlaceholders();
        UpdatePreviewDescription();
        StatusMessage = $"Editing: {SelectedTemplate.DisplayName}";
    }

    [RelayCommand]
    private void SaveTemplate()
    {
        if (SelectedQuestType == null || SelectedDifficulty == null)
        {
            StatusMessage = "Please select a quest type and difficulty";
            return;
        }

        var newTemplate = new JObject
        {
            ["name"] = EditName,
            ["displayName"] = EditDisplayName,
            ["rarityWeight"] = EditRarityWeight,
            ["questType"] = EditQuestType,
            ["difficulty"] = EditDifficulty,
            ["baseGoldReward"] = EditBaseGoldReward,
            ["baseXpReward"] = EditBaseXpReward,
            ["location"] = EditLocation,
            ["description"] = EditDescription
        };

        // Add optional fields
        if (EditMinQuantity > 0)
            newTemplate["minQuantity"] = EditMinQuantity;
        if (EditMaxQuantity > 0)
            newTemplate["maxQuantity"] = EditMaxQuantity;
        if (!string.IsNullOrWhiteSpace(EditItemType))
            newTemplate["itemType"] = EditItemType;

        var templatesArray = _rootJson?["components"]?["templates"]?[SelectedQuestType.Name]?[SelectedDifficulty.Name] as JArray;
        
        if (SelectedTemplate != null)
        {
            // Update existing
            var index = Templates.IndexOf(SelectedTemplate);
            if (index >= 0 && templatesArray != null)
            {
                templatesArray[index] = newTemplate;
            }
        }
        else
        {
            // Add new
            templatesArray?.Add(newTemplate);
        }

        IsDirty = true;
        IsEditMode = false;
        LoadTemplatesForDifficulty();
        StatusMessage = $"Template '{EditDisplayName}' saved";
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditMode = false;
        StatusMessage = "Edit cancelled";
    }

    [RelayCommand]
    private void DeleteTemplate()
    {
        if (SelectedTemplate == null) return;

        PendingDeleteItem = SelectedTemplate;
        ConfirmationMessage = $"Are you sure you want to delete the template '{SelectedTemplate.DisplayName}'?";
        ShowDeleteConfirmation = true;
    }

    [RelayCommand]
    private void ConfirmDelete()
    {
        ShowDeleteConfirmation = false;

        if (PendingDeleteItem is QuestTemplateViewModel template && SelectedQuestType != null && SelectedDifficulty != null)
        {
            var templatesArray = _rootJson?["components"]?["templates"]?[SelectedQuestType.Name]?[SelectedDifficulty.Name] as JArray;
            var index = Templates.IndexOf(template);
            
            if (index >= 0 && templatesArray != null)
            {
                templatesArray.RemoveAt(index);
                Templates.RemoveAt(index);
                IsDirty = true;
                StatusMessage = $"Deleted template: {template.DisplayName}";
                
                // Update template count
                if (SelectedDifficulty != null)
                {
                    SelectedDifficulty.TemplateCount = templatesArray.Count;
                }
            }
        }

        PendingDeleteItem = null;
    }

    [RelayCommand]
    private void CancelDelete()
    {
        ShowDeleteConfirmation = false;
        PendingDeleteItem = null;
        StatusMessage = "Delete cancelled";
    }

    [RelayCommand]
    private async Task Save()
    {
        if (!IsDirty)
        {
            StatusMessage = "No changes to save";
            return;
        }

        try
        {
            var filePath = _jsonEditorService.GetFilePath(_storedFileName);
            var json = _rootJson?.ToString(Formatting.Indented);
            
            if (json != null)
            {
                await File.WriteAllTextAsync(filePath, json);
                IsDirty = false;
                StatusMessage = "Quest catalog saved successfully";
                Log.Information("Saved quest catalog: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save quest catalog");
            StatusMessage = $"Error saving: {ex.Message}";
        }
    }

    partial void OnEditDescriptionChanged(string value)
    {
        DetectPlaceholders();
        UpdatePreviewDescription();
    }

    private void DetectPlaceholders()
    {
        DetectedPlaceholders.Clear();
        
        if (string.IsNullOrWhiteSpace(EditDescription)) return;

        var matches = PlaceholderRegex.Matches(EditDescription);
        foreach (Match match in matches)
        {
            var placeholder = match.Groups[1].Value;
            if (!DetectedPlaceholders.Contains(placeholder))
            {
                DetectedPlaceholders.Add(placeholder);
            }
        }
    }

    private void UpdatePreviewDescription()
    {
        if (string.IsNullOrWhiteSpace(EditDescription))
        {
            PreviewDescription = "";
            return;
        }

        var preview = EditDescription;
        
        // Replace common placeholders with example values
        preview = preview.Replace("{quantity}", EditMaxQuantity.ToString());
        preview = preview.Replace("{itemType}", EditItemType);
        preview = preview.Replace("{location}", EditLocation);
        preview = preview.Replace("{target}", "Example Enemy");
        preview = preview.Replace("{npc}", "Merchant");
        
        PreviewDescription = preview;
    }

    private static string FormatQuestTypeName(string name)
    {
        return name.Replace("_", " ").Capitalize();
    }

    private static string FormatDifficultyName(string name)
    {
        return name.Replace("_", " ").Capitalize();
    }

    private static string FormatCategoryName(string name)
    {
        return name.Replace("_", " ").Capitalize();
    }

    private static string FormatDangerName(string name)
    {
        return name.Replace("_", " ").Capitalize();
    }
}

// Helper extension
public static class StringExtensions
{
    public static string Capitalize(this string str)
    {
        if (string.IsNullOrWhiteSpace(str)) return str;
        var words = str.Split(' ');
        return string.Join(" ", words.Select(w => 
            char.ToUpper(w[0]) + (w.Length > 1 ? w.Substring(1).ToLower() : "")));
    }
}

// Quest Template ViewModel
public class QuestTemplateViewModel
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RarityWeight { get; set; } = 100;
    public int BaseGoldReward { get; set; }
    public int BaseXpReward { get; set; }
    public string QuestType { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int MinQuantity { get; set; }
    public int MaxQuantity { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public JObject? JsonData { get; set; }
}

// Location View Models (unique to catalog editor)
public class LocationViewModel
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RarityWeight { get; set; }
    public string LocationType { get; set; } = string.Empty;
    public string Danger { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
    public JObject? JsonData { get; set; }
}

public class LocationCategoryNode
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public ObservableCollection<LocationDangerNode> DangerLevels { get; set; } = new();
}

public class LocationDangerNode
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int LocationCount { get; set; }
    public LocationCategoryNode? ParentCategory { get; set; }
}

// Quest Type/Difficulty navigation nodes
public class QuestTypeNode
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public ObservableCollection<QuestDifficultyNode> Difficulties { get; set; } = new();
}

public class QuestDifficultyNode
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int TemplateCount { get; set; }
    public QuestTypeNode? ParentQuestType { get; set; }
}
