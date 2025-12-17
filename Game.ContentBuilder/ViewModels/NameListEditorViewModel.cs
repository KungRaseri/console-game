using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Game.ContentBuilder.Models;
using Game.ContentBuilder.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Game.ContentBuilder.ViewModels;

/// <summary>
/// ViewModel for editing name list files (weapon_names.json)
/// Structure: { "category": ["name1", "name2", ...] }
/// </summary>
public partial class NameListEditorViewModel : ObservableObject
{
    private readonly JsonEditorService _jsonEditorService;
    private readonly string _fileName;

    [ObservableProperty]
    private ObservableCollection<NameListCategory> _categories = new();

    [ObservableProperty]
    private NameListCategory? _selectedCategory;

    [ObservableProperty]
    private string? _selectedName;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _newNameInput = string.Empty;

    // Metadata properties (user-editable)
    [ObservableProperty]
    private string _metadataDescription = string.Empty;

    [ObservableProperty]
    private string _metadataVersion = string.Empty;

    [ObservableProperty]
    private string _metadataTags = string.Empty;

    [ObservableProperty]
    private string _metadataNotes = string.Empty;

    // Metadata properties (auto-generated, read-only display)
    [ObservableProperty]
    private string _metadataLastUpdated = string.Empty;

    [ObservableProperty]
    private string _metadataFileType = string.Empty;

    [ObservableProperty]
    private string _metadataTotalItems = string.Empty;

    public NameListEditorViewModel(JsonEditorService jsonEditorService, string fileName)
    {
        _jsonEditorService = jsonEditorService;
        _fileName = fileName;
        
        LoadData();
    }

    /// <summary>
    /// Loads data from JSON file (array-based structure)
    /// </summary>
    private void LoadData()
    {
        try
        {
            var filePath = _fileName; // fileName already includes full path like "items/weapons/names.json"
            var json = System.IO.File.ReadAllText(_jsonEditorService.GetFilePath(filePath));
            
            // Parse as JObject to extract metadata
            var root = JObject.Parse(json);
            
            // Extract metadata if present
            if (root["_metadata"] is JObject metadata)
            {
                MetadataDescription = metadata["description"]?.ToString() ?? string.Empty;
                MetadataVersion = metadata["version"]?.ToString() ?? string.Empty;
                MetadataLastUpdated = metadata["last_updated"]?.ToString() ?? string.Empty;
                MetadataFileType = metadata["type"]?.ToString() ?? string.Empty;
                
                // Tags (comma-separated string)
                if (metadata["tags"] is JArray tagsArray)
                {
                    MetadataTags = string.Join(", ", tagsArray.Select(t => t.ToString()));
                }
                else
                {
                    MetadataTags = string.Empty;
                }
                
                // Notes (handle both string and array formats)
                if (metadata["notes"] is JArray notesArray)
                {
                    MetadataNotes = string.Join("\n", notesArray.Select(n => n.ToString()));
                }
                else if (metadata["notes"] is JValue notesValue)
                {
                    MetadataNotes = notesValue.ToString() ?? string.Empty;
                }
                else
                {
                    MetadataNotes = string.Empty;
                }
                
                // Totals (for display)
                if (metadata["totals"] is JObject totals)
                {
                    MetadataTotalItems = totals["total_items"]?.ToString() ?? "0";
                }
                else
                {
                    MetadataTotalItems = "0";
                }
            }

            Categories.Clear();

            // Determine data source (items wrapper or direct)
            JObject? sourceData = null;
            if (root["items"] is JObject itemsObj)
            {
                // New structure: { "items": { "swords": [...], "axes": [...] } }
                sourceData = itemsObj;
            }
            else
            {
                // Old structure: { "swords": [...], "axes": [...] } - exclude _metadata
                sourceData = root;
            }

            if (sourceData == null)
            {
                StatusMessage = "Failed to parse data structure";
                Log.Warning("Failed to parse nested structure in {FileName}", _fileName);
                return;
            }

            // Convert dictionary to category list, only processing arrays
            foreach (var prop in sourceData.Properties().Where(p => p.Name != "_metadata"))
            {
                var categoryName = prop.Name;
                
                // Only process if the value is an array
                if (prop.Value is JArray namesArray)
                {
                    var names = namesArray.Select(t => t.ToString()).ToList();
                    Categories.Add(new NameListCategory(categoryName, names));
                }
                else
                {
                    // Skip entries that aren't string arrays (like "variants" which is a nested object)
                    Log.Debug("Skipping non-array category '{CategoryName}' in {FileName}", categoryName, _fileName);
                }
            }

            StatusMessage = $"Loaded {Categories.Count} categories from {_fileName}";
            Log.Information("Loaded {Count} categories from {FileName}", Categories.Count, _fileName);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading data: {ex.Message}";
            Log.Error(ex, "Failed to load {FileName}", _fileName);
        }
    }

    /// <summary>
    /// Saves changes back to JSON file (array-based structure)
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        try
        {
            // Build root JObject
            var root = new JObject();

            // Add metadata first
            var metadata = new JObject
            {
                ["description"] = MetadataDescription,
                ["version"] = MetadataVersion,
                ["last_updated"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                ["type"] = "name_list"
            };

            // Add tags (convert comma-separated string to array)
            if (!string.IsNullOrWhiteSpace(MetadataTags))
            {
                var tagsArray = new JArray(MetadataTags.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)));
                metadata["tags"] = tagsArray;
            }

            // Add notes (convert multi-line string to array)
            if (!string.IsNullOrWhiteSpace(MetadataNotes))
            {
                var notesArray = new JArray(MetadataNotes.Split('\n').Select(n => n.Trim()).Where(n => !string.IsNullOrEmpty(n)));
                metadata["notes"] = notesArray;
            }

            // Add totals
            var totalNames = Categories.Sum(c => c.Names.Count);
            metadata["totals"] = new JObject
            {
                ["total_items"] = totalNames,
                ["total_categories"] = Categories.Count
            };

            root["_metadata"] = metadata;

            // Add items (categories with name arrays)
            var itemsObj = new JObject();
            foreach (var category in Categories)
            {
                itemsObj[category.Name] = new JArray(category.Names);
            }
            root["items"] = itemsObj;

            // Write to file
            var filePath = _fileName; // fileName already includes full path
            var json = root.ToString(Formatting.Indented);
            System.IO.File.WriteAllText(_jsonEditorService.GetFilePath(filePath), json);

            StatusMessage = $"Saved changes to {_fileName}";
            Log.Information("Saved {FileName} successfully", _fileName);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving data: {ex.Message}";
            Log.Error(ex, "Failed to save {FileName}", _fileName);
        }
    }

    /// <summary>
    /// Cancels changes and reloads data
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        LoadData();
        SelectedCategory = null;
        SelectedName = null;
        NewNameInput = string.Empty;
        StatusMessage = "Changes cancelled - data reloaded";
    }

    /// <summary>
    /// Adds a new name to the selected category
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAddName))]
    private void AddName()
    {
        if (SelectedCategory != null && !string.IsNullOrWhiteSpace(NewNameInput))
        {
            SelectedCategory.Names.Add(NewNameInput.Trim());
            SelectedName = NewNameInput.Trim();
            NewNameInput = string.Empty;
            StatusMessage = $"Added '{SelectedName}' to {SelectedCategory.Name}";
            AddNameCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanAddName() => 
        SelectedCategory != null && !string.IsNullOrWhiteSpace(NewNameInput);

    /// <summary>
    /// Removes the selected name from the current category
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRemoveName))]
    private void RemoveName()
    {
        if (SelectedCategory != null && SelectedName != null)
        {
            SelectedCategory.Names.Remove(SelectedName);
            StatusMessage = $"Removed '{SelectedName}' from {SelectedCategory.Name}";
            SelectedName = null;
            RemoveNameCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanRemoveName() => 
        SelectedCategory != null && SelectedName != null;

    /// <summary>
    /// Adds a new category
    /// </summary>
    [RelayCommand]
    private void AddCategory()
    {
        var newCategory = new NameListCategory("new_category");
        Categories.Add(newCategory);
        SelectedCategory = newCategory;
        StatusMessage = "New category added";
    }

    /// <summary>
    /// Removes the selected category
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRemoveCategory))]
    private void RemoveCategory()
    {
        if (SelectedCategory != null)
        {
            var categoryToRemove = SelectedCategory;
            SelectedCategory = null;
            Categories.Remove(categoryToRemove);
            StatusMessage = $"Removed category '{categoryToRemove.Name}'";
            RemoveCategoryCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanRemoveCategory() => SelectedCategory != null;

    partial void OnSelectedCategoryChanged(NameListCategory? value)
    {
        RemoveNameCommand.NotifyCanExecuteChanged();
        AddNameCommand.NotifyCanExecuteChanged();
        RemoveCategoryCommand.NotifyCanExecuteChanged();
        
        if (value != null)
        {
            StatusMessage = $"Editing category: {value.Name} ({value.Names.Count} items)";
        }
        else
        {
            StatusMessage = "No category selected";
        }
    }

    partial void OnNewNameInputChanged(string value)
    {
        AddNameCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedNameChanged(string? value)
    {
        RemoveNameCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Can save if there are categories
    /// </summary>
    private bool CanSave()
    {
        return Categories.Any();
    }
}
