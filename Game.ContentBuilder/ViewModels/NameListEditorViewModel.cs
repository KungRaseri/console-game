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
            var filePath = System.IO.Path.Combine("items", _fileName);
            var json = System.IO.File.ReadAllText(_jsonEditorService.GetFilePath(filePath));
            
            // Parse the array-based JSON structure: { "category": ["name1", "name2", ...] }
            var rawData = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
            
            if (rawData == null)
            {
                StatusMessage = "Failed to load data - file is empty or invalid";
                Log.Warning("Failed to deserialize {FileName}", _fileName);
                return;
            }

            Categories.Clear();

            // Convert dictionary to category list
            foreach (var category in rawData)
            {
                var categoryName = category.Key; // "swords", "axes", etc.
                var names = category.Value;

                Categories.Add(new NameListCategory(categoryName, names));
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
            var data = new Dictionary<string, List<string>>();

            foreach (var category in Categories)
            {
                data[category.Name] = category.Names.ToList();
            }

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
