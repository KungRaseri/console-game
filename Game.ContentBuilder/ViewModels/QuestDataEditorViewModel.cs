using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Game.ContentBuilder.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Game.ContentBuilder.ViewModels;

/// <summary>
/// ViewModel for editing Quest v4.0 objectives.json and rewards.json
/// Supports hierarchical structure: metadata → components → categories → items
/// </summary>
public partial class QuestDataEditorViewModel : ObservableObject
{
    private readonly JsonEditorService _jsonEditorService;
    private readonly string _storedFileName;
    private JObject? _rootJson;
    private string? _dataType; // "objectives" or "rewards"

    [ObservableProperty]
    private string _title = "Quest Data Editor";

    [ObservableProperty]
    private string _version = "";

    [ObservableProperty]
    private string _description = "";

    [ObservableProperty]
    private ObservableCollection<QuestDataCategoryGroup> _categoryGroups = new();

    [ObservableProperty]
    private QuestDataCategoryGroup? _selectedCategoryGroup;

    [ObservableProperty]
    private QuestDataCategory? _selectedCategory;

    [ObservableProperty]
    private ObservableCollection<QuestDataItemViewModel> _items = new();

    [ObservableProperty]
    private QuestDataItemViewModel? _selectedItem;

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private string _statusMessage = "";

    // Edit fields
    [ObservableProperty]
    private string _editName = "";

    [ObservableProperty]
    private string _editDisplayName = "";

    [ObservableProperty]
    private string _editDescription = "";

    [ObservableProperty]
    private int _editRarityWeight = 100;

    [ObservableProperty]
    private string _editCategory = "";

    [ObservableProperty]
    private string? _editModifierFormula;

    [ObservableProperty]
    private int? _editMinValue;

    [ObservableProperty]
    private int? _editMaxValue;

    [ObservableProperty]
    private string? _editItemType;

    [ObservableProperty]
    private bool _isEditing;

    private QuestDataItemViewModel? _editingItem;

    public ICommand SaveCommand { get; }
    public ICommand NewItemCommand { get; }
    public ICommand EditItemCommand { get; }
    public ICommand SaveItemCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand DeleteItemCommand { get; }

    public QuestDataEditorViewModel(JsonEditorService jsonEditorService, string fileName)
    {
        _jsonEditorService = jsonEditorService;
        _storedFileName = fileName;

        SaveCommand = new RelayCommand(Save, () => IsDirty);
        NewItemCommand = new RelayCommand(NewItem);
        EditItemCommand = new RelayCommand<QuestDataItemViewModel>(EditItem);
        SaveItemCommand = new RelayCommand(SaveItem, () => IsEditing);
        CancelEditCommand = new RelayCommand(CancelEdit, () => IsEditing);
        DeleteItemCommand = new RelayCommand<QuestDataItemViewModel>(DeleteItem);

        LoadData();
    }

    private void LoadData()
    {
        try
        {
            var filePath = _jsonEditorService.GetFilePath(_storedFileName);
            var json = File.ReadAllText(filePath);
            _rootJson = JObject.Parse(json);

            var metadata = _rootJson["metadata"] as JObject;
            if (metadata != null)
            {
                Version = metadata["version"]?.ToString() ?? "Unknown";
                Description = metadata["description"]?.ToString() ?? "";
                
                var type = metadata["type"]?.ToString() ?? "";
                _dataType = type.Contains("objectives") ? "objectives" :
                           type.Contains("rewards") ? "rewards" : "data";
                
                Title = _dataType == "objectives" ? "Quest Objectives Editor" :
                       _dataType == "rewards" ? "Quest Rewards Editor" : "Quest Data Editor";
            }

            LoadCategories();
            
            Log.Information("Loaded quest data file: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            var filePath = _jsonEditorService.GetFilePath(_storedFileName);
            Log.Error(ex, "Failed to load quest data: {FilePath}", filePath);
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private void LoadCategories()
    {
        CategoryGroups.Clear();

        var components = _rootJson?["components"] as JObject;
        if (components == null) return;

        foreach (var componentProp in components.Properties())
        {
            var componentName = componentProp.Name;
            var componentObj = componentProp.Value as JObject;
            if (componentObj == null) continue;

            var group = new QuestDataCategoryGroup
            {
                Name = FormatName(componentName),
                ComponentKey = componentName,
                Categories = new ObservableCollection<QuestDataCategory>()
            };

            foreach (var categoryProp in componentObj.Properties())
            {
                var categoryNode = new QuestDataCategory
                {
                    Name = FormatName(categoryProp.Name),
                    CategoryKey = categoryProp.Name,
                    ParentGroup = group,
                    ItemCount = (categoryProp.Value as JArray)?.Count ?? 0
                };

                group.Categories.Add(categoryNode);
            }

            CategoryGroups.Add(group);
        }
    }

    partial void OnSelectedCategoryGroupChanged(QuestDataCategoryGroup? value)
    {
        if (value != null && value.Categories.Any())
        {
            SelectedCategory = value.Categories[0];
        }
    }

    partial void OnSelectedCategoryChanged(QuestDataCategory? value)
    {
        if (value != null)
        {
            LoadItemsForCategory(value);
        }
    }

    private void LoadItemsForCategory(QuestDataCategory category)
    {
        Items.Clear();

        try
        {
            var components = _rootJson?["components"] as JObject;
            var componentObj = components?[category.ParentGroup.ComponentKey] as JObject;
            var itemsArray = componentObj?[category.CategoryKey] as JArray;

            if (itemsArray == null) return;

            foreach (var item in itemsArray.OfType<JObject>())
            {
                Items.Add(CreateItemViewModel(item, category));
            }

            StatusMessage = $"Loaded {Items.Count} items from {category.Name}";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load items for category: {Category}", category.Name);
            StatusMessage = $"Error loading items: {ex.Message}";
        }
    }

    private QuestDataItemViewModel CreateItemViewModel(JObject item, QuestDataCategory category)
    {
        return new QuestDataItemViewModel
        {
            Name = item["name"]?.ToString() ?? "",
            DisplayName = item["displayName"]?.ToString() ?? "",
            Description = item["description"]?.ToString() ?? "",
            RarityWeight = item["rarityWeight"]?.ToObject<int>() ?? 100,
            Category = category.CategoryKey,
            ModifierFormula = item["modifierFormula"]?.ToString(),
            MinValue = item["minValue"]?.ToObject<int>(),
            MaxValue = item["maxValue"]?.ToObject<int>(),
            ItemType = item["itemType"]?.ToString(),
            JsonData = item
        };
    }

    private void NewItem()
    {
        if (SelectedCategory == null)
        {
            StatusMessage = "Select a category first";
            return;
        }

        _editingItem = null;
        EditName = "";
        EditDisplayName = "";
        EditDescription = "";
        EditRarityWeight = 100;
        EditCategory = SelectedCategory.CategoryKey;
        EditModifierFormula = null;
        EditMinValue = null;
        EditMaxValue = null;
        EditItemType = null;
        IsEditing = true;
        StatusMessage = "Creating new item...";
    }

    private void EditItem(QuestDataItemViewModel? item)
    {
        if (item == null) return;

        _editingItem = item;
        EditName = item.Name;
        EditDisplayName = item.DisplayName;
        EditDescription = item.Description;
        EditRarityWeight = item.RarityWeight;
        EditCategory = item.Category;
        EditModifierFormula = item.ModifierFormula;
        EditMinValue = item.MinValue;
        EditMaxValue = item.MaxValue;
        EditItemType = item.ItemType;
        IsEditing = true;
        StatusMessage = $"Editing: {item.DisplayName}";
    }

    private void SaveItem()
    {
        if (string.IsNullOrWhiteSpace(EditName))
        {
            StatusMessage = "Name is required";
            return;
        }

        try
        {
            var components = _rootJson?["components"] as JObject;
            if (components == null || SelectedCategory == null) return;

            var componentObj = components[SelectedCategory.ParentGroup.ComponentKey] as JObject;
            var itemsArray = componentObj?[SelectedCategory.CategoryKey] as JArray;

            if (itemsArray == null) return;

            var itemJson = new JObject
            {
                ["name"] = EditName,
                ["displayName"] = EditDisplayName,
                ["description"] = EditDescription,
                ["rarityWeight"] = EditRarityWeight
            };

            if (!string.IsNullOrWhiteSpace(EditModifierFormula))
                itemJson["modifierFormula"] = EditModifierFormula;
            
            if (EditMinValue.HasValue)
                itemJson["minValue"] = EditMinValue.Value;
            
            if (EditMaxValue.HasValue)
                itemJson["maxValue"] = EditMaxValue.Value;
            
            if (!string.IsNullOrWhiteSpace(EditItemType))
                itemJson["itemType"] = EditItemType;

            if (_editingItem != null)
            {
                // Update existing
                var index = itemsArray.IndexOf(_editingItem.JsonData);
                if (index >= 0)
                {
                    itemsArray[index] = itemJson;
                    _editingItem.Name = EditName;
                    _editingItem.DisplayName = EditDisplayName;
                    _editingItem.Description = EditDescription;
                    _editingItem.RarityWeight = EditRarityWeight;
                    _editingItem.JsonData = itemJson;
                }
            }
            else
            {
                // Add new
                itemsArray.Add(itemJson);
                var newItem = CreateItemViewModel(itemJson, SelectedCategory);
                Items.Add(newItem);
            }

            IsDirty = true;
            IsEditing = false;
            StatusMessage = "Item saved (in memory - click Save to write to file)";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save item");
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private void CancelEdit()
    {
        IsEditing = false;
        _editingItem = null;
        StatusMessage = "Edit cancelled";
    }

    private void DeleteItem(QuestDataItemViewModel? item)
    {
        if (item == null || SelectedCategory == null) return;

        try
        {
            var components = _rootJson?["components"] as JObject;
            var componentObj = components?[SelectedCategory.ParentGroup.ComponentKey] as JObject;
            var itemsArray = componentObj?[SelectedCategory.CategoryKey] as JArray;

            if (itemsArray == null) return;

            itemsArray.Remove(item.JsonData);
            Items.Remove(item);
            IsDirty = true;
            StatusMessage = $"Deleted: {item.DisplayName}";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to delete item");
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private void Save()
    {
        try
        {
            var filePath = _jsonEditorService.GetFilePath(_storedFileName);
            var json = _rootJson?.ToString(Formatting.Indented);
            if (json != null)
            {
                File.WriteAllText(filePath, json);
                IsDirty = false;
                StatusMessage = $"Saved to {Path.GetFileName(filePath)}";
                Log.Information("Saved quest data: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            var filePath = _jsonEditorService.GetFilePath(_storedFileName);
            Log.Error(ex, "Failed to save quest data: {FilePath}", filePath);
            StatusMessage = $"Error saving: {ex.Message}";
        }
    }

    private static string FormatName(string name)
    {
        return string.Concat(name.Select((c, i) => 
            i > 0 && char.IsUpper(c) ? " " + c : c.ToString()))
            .Replace("_", " ")
            .Replace("  ", " ")
            .Trim();
    }
}

public partial class QuestDataCategoryGroup : ObservableObject
{
    [ObservableProperty]
    private string _name = "";

    public string ComponentKey { get; set; } = "";
    public ObservableCollection<QuestDataCategory> Categories { get; set; } = new();
}

public partial class QuestDataCategory : ObservableObject
{
    [ObservableProperty]
    private string _name = "";

    public string CategoryKey { get; set; } = "";
    public QuestDataCategoryGroup ParentGroup { get; set; } = null!;
    public int ItemCount { get; set; }
}

public partial class QuestDataItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private string _displayName = "";

    [ObservableProperty]
    private string _description = "";

    [ObservableProperty]
    private int _rarityWeight = 100;

    public string Category { get; set; } = "";
    public string? ModifierFormula { get; set; }
    public int? MinValue { get; set; }
    public int? MaxValue { get; set; }
    public string? ItemType { get; set; }
    public JObject JsonData { get; set; } = new();
}
