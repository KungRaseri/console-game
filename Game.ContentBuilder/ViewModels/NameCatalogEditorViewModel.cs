using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Game.ContentBuilder.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Game.ContentBuilder.ViewModels;

/// <summary>
/// ViewModel for editing name catalog files (first_names.json, last_names.json)
/// Supports bulk name editing with category management
/// </summary>
public partial class NameCatalogEditorViewModel : ObservableObject
{
    private readonly JsonEditorService _jsonEditorService;
    private readonly string _storedFileName;
    private JObject? _rootJson;
    
    private static readonly Regex NameValidationRegex = new(@"^[a-zA-Z'-]+$", RegexOptions.Compiled);
    private const int MinNameLength = 2;
    private const int MaxNameLength = 30;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<NameCategory> _categories = new();

    [ObservableProperty]
    private NameCategory? _selectedCategory;

    [ObservableProperty]
    private ObservableCollection<string> _selectedNames = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private int _totalNameCount;

    [ObservableProperty]
    private string _newCategoryName = string.Empty;

    [ObservableProperty]
    private string _newNameInput = string.Empty;

    [ObservableProperty]
    private string _bulkNamesInput = string.Empty;

    // Confirmation dialog state
    [ObservableProperty]
    private bool _showDeleteCategoryConfirmation;

    [ObservableProperty]
    private bool _showDeleteNamesConfirmation;

    [ObservableProperty]
    private string _confirmationMessage = string.Empty;

    [ObservableProperty]
    private NameCategory? _pendingDeleteCategory;

    public NameCatalogEditorViewModel(JsonEditorService jsonEditorService, string fileName)
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

            LoadCategories();
            UpdateTotalCount();

            IsDirty = false;
            StatusMessage = $"Loaded {FileName} - {TotalNameCount} names";
            Log.Information("Loaded name catalog: {FilePath}, Total names: {Count}", filePath, TotalNameCount);
        }
        catch (Exception ex)
        {
            var filePath = _jsonEditorService.GetFilePath(_storedFileName);
            Log.Error(ex, "Failed to load name catalog: {FilePath}", filePath);
            StatusMessage = $"Error loading file: {ex.Message}";
        }
    }

    private void LoadCategories()
    {
        Categories.Clear();

        if (_rootJson == null) return;

        var categoriesObj = _rootJson["categories"] as JObject;
        if (categoriesObj == null) return;

        foreach (var categoryProp in categoriesObj.Properties())
        {
            var categoryName = categoryProp.Name;
            var namesArray = categoryProp.Value as JArray;
            
            if (namesArray == null) continue;

            var namesList = new ObservableCollection<string>();
            foreach (var name in namesArray)
            {
                var nameStr = name.ToString();
                if (!string.IsNullOrWhiteSpace(nameStr))
                {
                    namesList.Add(nameStr);
                }
            }

            Categories.Add(new NameCategory
            {
                Name = categoryName,
                Names = namesList
            });
        }
    }

    private void UpdateTotalCount()
    {
        TotalNameCount = Categories.Sum(c => c.Names.Count);
    }

    partial void OnSelectedCategoryChanged(NameCategory? value)
    {
        SelectedNames.Clear();
        SearchText = string.Empty;
    }

    partial void OnSearchTextChanged(string value)
    {
        // Search functionality can be implemented in View with CollectionViewSource filtering
    }

    [RelayCommand]
    private void AddCategory()
    {
        if (string.IsNullOrWhiteSpace(NewCategoryName))
        {
            StatusMessage = "Category name cannot be empty";
            return;
        }

        if (Categories.Any(c => c.Name.Equals(NewCategoryName, StringComparison.OrdinalIgnoreCase)))
        {
            StatusMessage = "Category already exists";
            return;
        }

        var newCategory = new NameCategory
        {
            Name = NewCategoryName,
            Names = new ObservableCollection<string>()
        };

        Categories.Add(newCategory);
        SelectedCategory = newCategory;
        NewCategoryName = string.Empty;
        IsDirty = true;
        StatusMessage = $"Added category: {newCategory.Name}";
        Log.Information("Added category: {CategoryName}", newCategory.Name);
    }

    [RelayCommand]
    private void RemoveCategory()
    {
        if (SelectedCategory == null)
        {
            StatusMessage = "No category selected";
            return;
        }

        var categoryName = SelectedCategory.Name;
        var nameCount = SelectedCategory.Names.Count;

        if (nameCount > 0)
        {
            // Show confirmation dialog in UI
            PendingDeleteCategory = SelectedCategory;
            ConfirmationMessage = $"Category '{categoryName}' contains {nameCount} names. Delete anyway?";
            ShowDeleteCategoryConfirmation = true;
        }
        else
        {
            // No names, delete immediately
            ExecuteRemoveCategory(SelectedCategory);
        }
    }

    [RelayCommand]
    private void ConfirmDeleteCategory()
    {
        if (PendingDeleteCategory != null)
        {
            ExecuteRemoveCategory(PendingDeleteCategory);
            PendingDeleteCategory = null;
        }
        ShowDeleteCategoryConfirmation = false;
    }

    [RelayCommand]
    private void CancelDeleteCategory()
    {
        PendingDeleteCategory = null;
        ShowDeleteCategoryConfirmation = false;
        StatusMessage = "Delete cancelled";
    }

    private void ExecuteRemoveCategory(NameCategory category)
    {
        Categories.Remove(category);
        SelectedCategory = Categories.FirstOrDefault();
        UpdateTotalCount();
        IsDirty = true;
        StatusMessage = $"Removed category: {category.Name}";
        Log.Information("Removed category: {CategoryName}", category.Name);
    }

    [RelayCommand]
    private void AddName()
    {
        if (SelectedCategory == null)
        {
            StatusMessage = "No category selected";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewNameInput))
        {
            StatusMessage = "Name cannot be empty";
            return;
        }

        var validationError = ValidateName(NewNameInput);
        if (validationError != null)
        {
            StatusMessage = validationError;
            return;
        }

        var trimmedName = NewNameInput.Trim();

        if (SelectedCategory.Names.Contains(trimmedName, StringComparer.OrdinalIgnoreCase))
        {
            StatusMessage = "Name already exists in this category";
            return;
        }

        SelectedCategory.Names.Add(trimmedName);
        UpdateTotalCount();
        NewNameInput = string.Empty;
        IsDirty = true;
        StatusMessage = $"Added: {trimmedName}";
        Log.Information("Added name: {Name} to category: {Category}", trimmedName, SelectedCategory.Name);
    }

    [RelayCommand]
    private void BulkAddNames()
    {
        if (SelectedCategory == null)
        {
            StatusMessage = "No category selected";
            return;
        }

        if (string.IsNullOrWhiteSpace(BulkNamesInput))
        {
            StatusMessage = "No names to add";
            return;
        }

        var names = BulkNamesInput
            .Split(new[] { '\n', '\r', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(n => n.Trim())
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var addedCount = 0;
        var skippedCount = 0;
        var errorCount = 0;
        var errors = new List<string>();

        foreach (var name in names)
        {
            var validationError = ValidateName(name);
            if (validationError != null)
            {
                errorCount++;
                errors.Add($"{name}: {validationError}");
                continue;
            }

            if (SelectedCategory.Names.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                skippedCount++;
                continue;
            }

            SelectedCategory.Names.Add(name);
            addedCount++;
        }

        UpdateTotalCount();
        BulkNamesInput = string.Empty;

        if (addedCount > 0)
            IsDirty = true;

        var statusParts = new List<string>();
        if (addedCount > 0) statusParts.Add($"{addedCount} added");
        if (skippedCount > 0) statusParts.Add($"{skippedCount} duplicates skipped");
        if (errorCount > 0) statusParts.Add($"{errorCount} errors");

        StatusMessage = string.Join(", ", statusParts);

        if (errors.Any())
        {
            var errorMessage = string.Join("\n", errors.Take(10));
            if (errors.Count > 10)
                errorMessage += $"\n... and {errors.Count - 10} more errors";
            
            StatusMessage = $"Validation errors: {errorCount} names had errors";
            Log.Warning("Bulk add validation errors: {ErrorMessage}", errorMessage);
        }

        Log.Information("Bulk add to {Category}: {Added} added, {Skipped} skipped, {Errors} errors",
            SelectedCategory.Name, addedCount, skippedCount, errorCount);
    }

    [RelayCommand]
    private void DeleteSelectedNames()
    {
        if (SelectedCategory == null || SelectedNames.Count == 0)
        {
            StatusMessage = "No names selected";
            return;
        }

        var count = SelectedNames.Count;
        ConfirmationMessage = $"Delete {count} selected name(s)?";
        ShowDeleteNamesConfirmation = true;
    }

    [RelayCommand]
    private void ConfirmDeleteNames()
    {
        if (SelectedCategory == null || SelectedNames.Count == 0)
        {
            ShowDeleteNamesConfirmation = false;
            return;
        }

        var namesToRemove = SelectedNames.ToList();
        var count = namesToRemove.Count;
        
        foreach (var name in namesToRemove)
        {
            SelectedCategory.Names.Remove(name);
        }

        SelectedNames.Clear();
        UpdateTotalCount();
        IsDirty = true;
        StatusMessage = $"Deleted {count} name(s)";
        ShowDeleteNamesConfirmation = false;
        Log.Information("Deleted {Count} names from {Category}", count, SelectedCategory.Name);
    }

    [RelayCommand]
    private void CancelDeleteNames()
    {
        ShowDeleteNamesConfirmation = false;
        StatusMessage = "Delete cancelled";
    }

    [RelayCommand]
    private void SortCategory()
    {
        if (SelectedCategory == null)
        {
            StatusMessage = "No category selected";
            return;
        }

        var sorted = SelectedCategory.Names.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
        SelectedCategory.Names.Clear();
        foreach (var name in sorted)
        {
            SelectedCategory.Names.Add(name);
        }

        IsDirty = true;
        StatusMessage = $"Sorted {sorted.Count} names alphabetically";
        Log.Information("Sorted category: {Category}", SelectedCategory.Name);
    }

    [RelayCommand]
    private async Task SaveFile()
    {
        if (_rootJson == null) return;

        try
        {
            var filePath = _jsonEditorService.GetFilePath(_storedFileName);

            // Rebuild categories object
            var categoriesObj = new JObject();
            foreach (var category in Categories)
            {
                var namesArray = new JArray(category.Names);
                categoriesObj[category.Name] = namesArray;
            }

            _rootJson["categories"] = categoriesObj;

            // Write to file
            var json = _rootJson.ToString(Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json);

            IsDirty = false;
            StatusMessage = $"Saved {FileName} - {TotalNameCount} names";
            Log.Information("Saved name catalog: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            var filePath = _jsonEditorService.GetFilePath(_storedFileName);
            Log.Error(ex, "Failed to save name catalog: {FilePath}", filePath);
            StatusMessage = $"Error saving file: {ex.Message}";
        }
    }

    private static string? ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Name cannot be empty";

        var trimmed = name.Trim();

        if (trimmed.Length < MinNameLength)
            return $"Name must be at least {MinNameLength} characters";

        if (trimmed.Length > MaxNameLength)
            return $"Name cannot exceed {MaxNameLength} characters";

        if (!NameValidationRegex.IsMatch(trimmed))
            return "Name can only contain letters, hyphens, and apostrophes";

        return null;
    }
}

/// <summary>
/// Represents a category of names (e.g., male_common, female_noble)
/// </summary>
public partial class NameCategory : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _names = new();

    public int Count => Names.Count;
}
