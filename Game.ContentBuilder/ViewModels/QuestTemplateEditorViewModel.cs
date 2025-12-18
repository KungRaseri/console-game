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
/// Editor for quest template files (quest_templates.json)
/// Supports two-level tree structure (quest type → difficulty) with placeholder system
/// </summary>
public partial class QuestTemplateEditorViewModel : ObservableObject
{
    private readonly JsonEditorService _jsonEditorService;
    private readonly string _storedFileName;
    private JObject? _rootJson;
    
    private static readonly Regex PlaceholderRegex = new(@"\{(\w+)\}", RegexOptions.Compiled);

    [ObservableProperty]
    private string _fileName = string.Empty;

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

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private bool _isEditMode;

    // Edit properties
    [ObservableProperty]
    private string _editName = string.Empty;

    [ObservableProperty]
    private string _editDisplayName = string.Empty;

    [ObservableProperty]
    private string _editDescription = string.Empty;

    [ObservableProperty]
    private int _editRarityWeight = 10;

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
    private ObservableCollection<string> _detectedPlaceholders = new();

    [ObservableProperty]
    private string _previewDescription = string.Empty;

    // Confirmation dialog state
    [ObservableProperty]
    private bool _showDeleteConfirmation;

    [ObservableProperty]
    private string _confirmationMessage = string.Empty;

    [ObservableProperty]
    private QuestTemplateViewModel? _pendingDeleteTemplate;

    public QuestTemplateEditorViewModel(JsonEditorService jsonEditorService, string fileName)
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

            LoadQuestTypes();
            IsDirty = false;
            StatusMessage = "Quest templates loaded successfully";
            Log.Information("Loaded quest templates: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            var filePath = _jsonEditorService.GetFilePath(_storedFileName);
            Log.Error(ex, "Failed to load quest templates: {FilePath}", filePath);
            StatusMessage = $"Error loading file: {ex.Message}";
        }
    }

    private void LoadQuestTypes()
    {
        QuestTypes.Clear();

        if (_rootJson?["components"] is not JObject components)
        {
            Log.Warning("No components found in quest templates");
            return;
        }

        foreach (var questType in components.Properties())
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

    partial void OnSelectedDifficultyChanged(QuestDifficultyNode? value)
    {
        if (value == null) return;
        LoadTemplates();
    }

    private void LoadTemplates()
    {
        Templates.Clear();

        if (SelectedQuestType == null || SelectedDifficulty == null) return;

        var path = _rootJson?["components"]?[SelectedQuestType.Name]?[SelectedDifficulty.Name];
        if (path is not JArray templates)
        {
            StatusMessage = $"No templates found for {SelectedDifficulty.DisplayName}";
            return;
        }

        foreach (var template in templates)
        {
            var vm = new QuestTemplateViewModel
            {
                Name = template["name"]?.ToString() ?? "",
                DisplayName = template["displayName"]?.ToString() ?? "",
                Description = template["description"]?.ToString() ?? "",
                RarityWeight = template["rarityWeight"]?.Value<int>() ?? 10,
                BaseGoldReward = template["baseGoldReward"]?.Value<int>() ?? 100,
                BaseXpReward = template["baseXpReward"]?.Value<int>() ?? 100,
                SourceJson = template as JObject,
                IsVisible = true
            };

            Templates.Add(vm);
        }

        ApplySearch();
        StatusMessage = $"Loaded {Templates.Count} templates for {SelectedDifficulty.DisplayName}";
    }

    [RelayCommand]
    private void Search()
    {
        ApplySearch();
    }

    private void ApplySearch()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            foreach (var template in Templates)
            {
                template.IsVisible = true;
            }
        }
        else
        {
            var search = SearchText.ToLowerInvariant();
            foreach (var template in Templates)
            {
                template.IsVisible = template.Name.ToLowerInvariant().Contains(search) ||
                                    template.DisplayName.ToLowerInvariant().Contains(search) ||
                                    template.Description.ToLowerInvariant().Contains(search);
            }
        }
    }

    [RelayCommand]
    private void AddTemplate()
    {
        if (SelectedQuestType == null || SelectedDifficulty == null)
        {
            StatusMessage = "Please select a quest type and difficulty first";
            return;
        }

        EditName = $"New{SelectedQuestType.Name}Template";
        EditDisplayName = $"New {SelectedQuestType.DisplayName} Quest";
        EditDescription = "Complete this quest to earn rewards";
        EditRarityWeight = 10;
        EditBaseGoldReward = 100;
        EditBaseXpReward = 100;
        EditLocation = "Location";
        EditMinQuantity = 1;
        EditMaxQuantity = 1;
        DetectedPlaceholders.Clear();
        PreviewDescription = EditDescription;
        
        SelectedTemplate = null;
        IsEditMode = true;
        StatusMessage = "Adding new quest template...";
    }

    [RelayCommand]
    private void EditTemplate(QuestTemplateViewModel? template)
    {
        if (template == null) return;

        EditName = template.Name;
        EditDisplayName = template.DisplayName;
        EditDescription = template.Description;
        EditRarityWeight = template.RarityWeight;
        EditBaseGoldReward = template.BaseGoldReward;
        EditBaseXpReward = template.BaseXpReward;
        
        // Load additional properties from JSON
        var json = template.SourceJson;
        EditLocation = json?["location"]?.ToString() ?? "";
        EditMinQuantity = json?["minQuantity"]?.Value<int>() ?? 1;
        EditMaxQuantity = json?["maxQuantity"]?.Value<int>() ?? 1;

        UpdatePlaceholders();
        UpdatePreview();

        SelectedTemplate = template;
        IsEditMode = true;
        StatusMessage = $"Editing: {template.DisplayName}";
    }

    partial void OnEditDescriptionChanged(string value)
    {
        UpdatePlaceholders();
        UpdatePreview();
    }

    private void UpdatePlaceholders()
    {
        DetectedPlaceholders.Clear();

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

    private void UpdatePreview()
    {
        PreviewDescription = EditDescription;
        
        // Replace placeholders with sample values
        var sampleValues = new Dictionary<string, string>
        {
            { "quantity", EditMaxQuantity.ToString() },
            { "location", EditLocation },
            { "itemName", "Ancient Artifact" },
            { "enemyType", "Goblin" },
            { "npcName", "Village Elder" },
            { "animalType", "Wolf" },
            { "gold", EditBaseGoldReward.ToString() },
            { "xp", EditBaseXpReward.ToString() }
        };

        foreach (var kvp in sampleValues)
        {
            PreviewDescription = PreviewDescription.Replace($"{{{kvp.Key}}}", kvp.Value);
        }
    }

    [RelayCommand]
    private void SaveTemplate()
    {
        if (SelectedQuestType == null || SelectedDifficulty == null)
        {
            StatusMessage = "No quest type/difficulty selected";
            return;
        }

        if (string.IsNullOrWhiteSpace(EditName))
        {
            StatusMessage = "Name is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(EditDescription))
        {
            StatusMessage = "Description is required";
            return;
        }

        try
        {
            // Create template JSON
            var newTemplate = new JObject
            {
                ["name"] = EditName,
                ["displayName"] = EditDisplayName,
                ["rarityWeight"] = EditRarityWeight,
                ["questType"] = SelectedQuestType.Name,
                ["difficulty"] = ExtractDifficultyLevel(SelectedDifficulty.Name),
                ["baseGoldReward"] = EditBaseGoldReward,
                ["baseXpReward"] = EditBaseXpReward,
                ["description"] = EditDescription
            };

            // Add optional properties
            if (!string.IsNullOrWhiteSpace(EditLocation))
                newTemplate["location"] = EditLocation;

            if (EditMinQuantity > 0)
                newTemplate["minQuantity"] = EditMinQuantity;

            if (EditMaxQuantity > 0)
                newTemplate["maxQuantity"] = EditMaxQuantity;

            var templatesArray = _rootJson!["components"]![SelectedQuestType.Name]![SelectedDifficulty.Name] as JArray;

            if (SelectedTemplate != null)
            {
                // Update existing
                var index = Templates.IndexOf(SelectedTemplate);
                templatesArray![index] = newTemplate;

                SelectedTemplate.Name = EditName;
                SelectedTemplate.DisplayName = EditDisplayName;
                SelectedTemplate.Description = EditDescription;
                SelectedTemplate.RarityWeight = EditRarityWeight;
                SelectedTemplate.BaseGoldReward = EditBaseGoldReward;
                SelectedTemplate.BaseXpReward = EditBaseXpReward;
                SelectedTemplate.SourceJson = newTemplate;

                StatusMessage = $"Updated template: {EditDisplayName}";
            }
            else
            {
                // Add new
                templatesArray!.Add(newTemplate);

                var vm = new QuestTemplateViewModel
                {
                    Name = EditName,
                    DisplayName = EditDisplayName,
                    Description = EditDescription,
                    RarityWeight = EditRarityWeight,
                    BaseGoldReward = EditBaseGoldReward,
                    BaseXpReward = EditBaseXpReward,
                    SourceJson = newTemplate,
                    IsVisible = true
                };
                Templates.Add(vm);

                // Update count
                if (SelectedDifficulty != null)
                    SelectedDifficulty.TemplateCount = templatesArray.Count;

                StatusMessage = $"Added template: {EditDisplayName}";
            }

            IsDirty = true;
            IsEditMode = false;
            Log.Information("Saved quest template: {Name}", EditName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save quest template");
            StatusMessage = $"Error saving template: {ex.Message}";
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditMode = false;
        SelectedTemplate = null;
        StatusMessage = "Edit cancelled";
    }

    [RelayCommand]
    private void DeleteTemplate(QuestTemplateViewModel? template)
    {
        if (template == null) return;

        PendingDeleteTemplate = template;
        ConfirmationMessage = $"Delete quest template '{template.DisplayName}'?";
        ShowDeleteConfirmation = true;
    }

    [RelayCommand]
    private void ConfirmDelete()
    {
        if (PendingDeleteTemplate == null || SelectedQuestType == null || SelectedDifficulty == null)
        {
            ShowDeleteConfirmation = false;
            return;
        }

        try
        {
            var templatesArray = _rootJson!["components"]![SelectedQuestType.Name]![SelectedDifficulty.Name] as JArray;
            var index = Templates.IndexOf(PendingDeleteTemplate);
            
            templatesArray!.RemoveAt(index);
            Templates.RemoveAt(index);

            // Update count
            SelectedDifficulty.TemplateCount = templatesArray.Count;

            IsDirty = true;
            StatusMessage = $"Deleted template: {PendingDeleteTemplate.DisplayName}";
            Log.Information("Deleted quest template: {Name}", PendingDeleteTemplate.Name);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to delete quest template");
            StatusMessage = $"Error deleting template: {ex.Message}";
        }
        finally
        {
            PendingDeleteTemplate = null;
            ShowDeleteConfirmation = false;
        }
    }

    [RelayCommand]
    private void CancelDelete()
    {
        PendingDeleteTemplate = null;
        ShowDeleteConfirmation = false;
        StatusMessage = "Delete cancelled";
    }

    [RelayCommand]
    private void CloneTemplate(QuestTemplateViewModel? template)
    {
        if (template == null || SelectedQuestType == null || SelectedDifficulty == null) return;

        EditName = $"{template.Name}Copy";
        EditDisplayName = $"{template.DisplayName} (Copy)";
        EditDescription = template.Description;
        EditRarityWeight = template.RarityWeight;
        EditBaseGoldReward = template.BaseGoldReward;
        EditBaseXpReward = template.BaseXpReward;

        var json = template.SourceJson;
        EditLocation = json?["location"]?.ToString() ?? "";
        EditMinQuantity = json?["minQuantity"]?.Value<int>() ?? 1;
        EditMaxQuantity = json?["maxQuantity"]?.Value<int>() ?? 1;

        UpdatePlaceholders();
        UpdatePreview();

        SelectedTemplate = null;
        IsEditMode = true;
        StatusMessage = $"Cloning template: {template.DisplayName}";
    }

    [RelayCommand]
    private async Task SaveFile()
    {
        if (_rootJson == null) return;

        try
        {
            var filePath = _jsonEditorService.GetFilePath(_storedFileName);
            
            // Update metadata
            if (_rootJson["metadata"] is JObject metadata)
            {
                metadata["last_updated"] = DateTime.Now.ToString("yyyy-MM-dd");
                
                // Count total templates
                int totalTemplates = 0;
                if (_rootJson["components"] is JObject components)
                {
                    foreach (var questType in components.Properties())
                    {
                        if (questType.Value is JObject difficulties)
                        {
                            foreach (var difficulty in difficulties.Properties())
                            {
                                if (difficulty.Value is JArray templates)
                                {
                                    totalTemplates += templates.Count;
                                }
                            }
                        }
                    }
                }
                metadata["total_templates"] = totalTemplates;
            }

            var json = _rootJson.ToString(Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json);

            IsDirty = false;
            StatusMessage = $"Saved quest templates successfully";
            Log.Information("Saved quest templates: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            var filePath = _jsonEditorService.GetFilePath(_storedFileName);
            Log.Error(ex, "Failed to save quest templates: {FilePath}", filePath);
            StatusMessage = $"Error saving file: {ex.Message}";
        }
    }

    private static string FormatQuestTypeName(string name)
    {
        return char.ToUpper(name[0]) + name.Substring(1);
    }

    private static string FormatDifficultyName(string name)
    {
        // "easy_fetch" -> "Easy Fetch"
        var parts = name.Split('_');
        return string.Join(" ", parts.Select(p => char.ToUpper(p[0]) + p.Substring(1)));
    }

    private static string ExtractDifficultyLevel(string difficultyName)
    {
        // "easy_fetch" -> "easy"
        return difficultyName.Split('_')[0];
    }
}

// Supporting classes
public partial class QuestTypeNode : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<QuestDifficultyNode> _difficulties = new();
}

public partial class QuestDifficultyNode : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private int _templateCount;

    [ObservableProperty]
    private QuestTypeNode? _parentQuestType;
}

public partial class QuestTemplateViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private int _rarityWeight;

    [ObservableProperty]
    private int _baseGoldReward;

    [ObservableProperty]
    private int _baseXpReward;

    [ObservableProperty]
    private bool _isVisible = true;

    public JObject? SourceJson { get; set; }
}
