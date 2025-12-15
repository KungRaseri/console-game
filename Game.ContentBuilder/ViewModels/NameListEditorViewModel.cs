using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Game.ContentBuilder.Models;
using Game.ContentBuilder.Services;
using Newtonsoft.Json;
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
            
            // Parse the JSON structure which might have nested "items" wrapper
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            
            if (jsonObject == null)
            {
                StatusMessage = "Failed to load data - file is empty or invalid";
                Log.Warning("Failed to deserialize {FileName}", _fileName);
                return;
            }

            // Check if there's an "items" wrapper (new structure)
            Dictionary<string, object>? sourceData = null;
            if (jsonObject.ContainsKey("items"))
            {
                // New structure: { "items": { "swords": [...], "axes": [...] } }
                var itemsJson = JsonConvert.SerializeObject(jsonObject["items"]);
                sourceData = JsonConvert.DeserializeObject<Dictionary<string, object>>(itemsJson);
            }
            else
            {
                // Old structure: { "swords": [...], "axes": [...] }
                sourceData = jsonObject;
            }

            if (sourceData == null)
            {
                StatusMessage = "Failed to parse data structure";
                Log.Warning("Failed to parse nested structure in {FileName}", _fileName);
                return;
            }

            Categories.Clear();

            // Convert dictionary to category list, only processing arrays
            foreach (var kvp in sourceData)
            {
                var categoryName = kvp.Key;
                var value = kvp.Value;

                // Try to deserialize as a list of strings
                try
                {
                    var valueJson = value is string ? (string)value : JsonConvert.SerializeObject(value);
                    var names = JsonConvert.DeserializeObject<List<string>>(valueJson);
                    
                    if (names != null)
                    {
                        Categories.Add(new NameListCategory(categoryName, names));
                    }
                }
                catch (JsonSerializationException)
                {
                    // Skip entries that aren't string arrays (like "variants" which is a nested object)
                    Log.Debug("Skipping non-array category '{CategoryName}' in {FileName}", categoryName, _fileName);
                    continue;
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
            // Convert category list back to dictionary structure
            var categoryData = new Dictionary<string, List<string>>();

            foreach (var category in Categories)
            {
                categoryData[category.Name] = category.Names.ToList();
            }

            // Wrap in "items" object to match the expected structure
            var data = new Dictionary<string, object>
            {
                ["items"] = categoryData
            };

            var filePath = System.IO.Path.Combine("items", _fileName);
            _jsonEditorService.Save(filePath, data);

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
