using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Game.ContentBuilder.Models;
using Game.ContentBuilder.Services;
using Game.ContentBuilder.Validators;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Game.ContentBuilder.ViewModels;

/// <summary>
/// ViewModel for editing flat item files (metals, woods, leathers, gemstones)
/// These files have structure: { "ItemName": { "displayName": "...", "traits": {...} } }
/// No rarity levels - items are at root level
/// </summary>
public partial class FlatItemEditorViewModel : ObservableObject
{
    private readonly JsonEditorService _jsonEditorService;
    private readonly string _fileName;
    private readonly ItemPrefixSuffixValidator _validator;

    [ObservableProperty]
    private ObservableCollection<ItemPrefixSuffix> _items = new();

    [ObservableProperty]
    private ItemPrefixSuffix? _selectedItem;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _validationErrors = string.Empty;

    [ObservableProperty]
    private bool _hasValidationErrors = false;

    #region Metadata Properties

    // User-editable metadata fields
    [ObservableProperty]
    private string _metadataDescription = string.Empty;

    [ObservableProperty]
    private string _metadataVersion = "1.0";

    [ObservableProperty]
    private string _notes = string.Empty;

    // Auto-generated read-only fields (displayed but not editable)
    [ObservableProperty]
    private string _lastUpdated = DateTime.Now.ToString("yyyy-MM-dd");

    [ObservableProperty]
    private string _fileType = string.Empty;

    #endregion

    public FlatItemEditorViewModel(JsonEditorService jsonEditorService, string fileName)
    {
        _jsonEditorService = jsonEditorService;
        _fileName = fileName;
        _validator = new ItemPrefixSuffixValidator();
        
        LoadData();
    }

    /// <summary>
    /// Loads data from JSON file (flat structure)
    /// </summary>
    private void LoadData()
    {
        try
        {
            var filePath = _fileName; // fileName already includes full path like "items/weapons/prefixes.json"
            var json = System.IO.File.ReadAllText(_jsonEditorService.GetFilePath(filePath));
            
            // Parse using JObject to handle metadata
            var data = JObject.Parse(json);
            
            // Extract the item data (skip metadata)
            var rawData = new Dictionary<string, ItemPrefixRaw>();
            foreach (var prop in data.Properties())
            {
                if (prop.Name == "metadata") continue; // Skip metadata
                
                var itemData = prop.Value.ToObject<ItemPrefixRaw>();
                if (itemData != null)
                {
                    rawData[prop.Name] = itemData;
                }
            }
            
            if (rawData.Count == 0)
            {
                StatusMessage = "Failed to load data - file is empty or invalid";
                Log.Warning("Failed to deserialize {FileName}", _fileName);
                return;
            }

            Items.Clear();

            // Convert the flat dictionary to list (no rarity levels)
            foreach (var item in rawData)
            {
                var name = item.Key; // "Iron", "Oak", etc.
                var itemData = item.Value;

                // Use "material" as default rarity for flat items (not displayed, just for compatibility)
                var flatItem = new ItemPrefixSuffix(name, itemData.DisplayName ?? name, "material");

                // Convert traits
                if (itemData.Traits != null)
                {
                    foreach (var trait in itemData.Traits)
                    {
                        flatItem.Traits.Add(new ItemTrait(
                            trait.Key,
                            trait.Value.Value ?? 0,
                            trait.Value.Type ?? "number"
                        ));
                    }
                }

                // Subscribe to property changes for validation
                flatItem.PropertyChanged += (s, e) =>
                {
                    if (s == SelectedItem && SelectedItem != null)
                    {
                        ValidateItem(SelectedItem);
                    }
                };

                Items.Add(flatItem);
            }

            // Load metadata
            if (data["metadata"] is JObject metadataObj)
            {
                MetadataDescription = metadataObj["description"]?.ToString() ?? string.Empty;
                MetadataVersion = metadataObj["version"]?.ToString() ?? "1.0";
                
                // Handle notes - can be string or array
                if (metadataObj["notes"] is JArray notesArray)
                {
                    Notes = string.Join(Environment.NewLine, notesArray.Select(n => n.ToString()));
                }
                else
                {
                    Notes = metadataObj["notes"]?.ToString() ?? string.Empty;
                }
                
                LastUpdated = metadataObj["lastUpdated"]?.ToString() ?? DateTime.Now.ToString("yyyy-MM-dd");
                FileType = metadataObj["type"]?.ToString() ?? string.Empty;
            }

            StatusMessage = $"Loaded {Items.Count} items from {_fileName}";
            Log.Information("Loaded {Count} items from {FileName}", Items.Count, _fileName);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading data: {ex.Message}";
            Log.Error(ex, "Failed to load {FileName}", _fileName);
        }
    }

    /// <summary>
    /// Saves changes back to JSON file (flat structure)
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        try
        {
            // Validate all items before saving
            if (!ValidateAllItems())
            {
                StatusMessage = "Validation failed - cannot save. Please fix errors.";
                return;
            }

            // Create root JObject
            var root = new JObject();

            // Convert list to flat dictionary structure (no rarity levels)
            foreach (var item in Items)
            {
                // Convert traits to dictionary format
                var traits = new JObject();
                foreach (var trait in item.Traits)
                {
                    traits[trait.Key] = new JObject
                    {
                        ["value"] = JToken.FromObject(trait.Value),
                        ["type"] = trait.Type
                    };
                }

                // Add item directly to root (no rarity grouping)
                root[item.Name] = new JObject
                {
                    ["displayName"] = item.DisplayName,
                    ["traits"] = traits
                };
            }

            // Generate and add metadata
            var metadata = new JObject
            {
                ["description"] = MetadataDescription,
                ["version"] = MetadataVersion,
                ["lastUpdated"] = DateTime.Now.ToString("yyyy-MM-dd"),
                ["type"] = "item_catalog",
                ["total_items"] = Items.Count
            };

            if (!string.IsNullOrWhiteSpace(Notes))
            {
                metadata["notes"] = Notes;
            }

            root["metadata"] = metadata;

            // Update read-only properties
            LastUpdated = metadata["lastUpdated"]?.ToString() ?? DateTime.Now.ToString("yyyy-MM-dd");
            FileType = metadata["type"]?.ToString() ?? string.Empty;

            // Save to file
            var json = root.ToString(Formatting.Indented);
            var filePath = _jsonEditorService.GetFilePath(_fileName);
            System.IO.File.WriteAllText(filePath, json);

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
        SelectedItem = null;
        StatusMessage = "Changes cancelled - data reloaded";
    }

    /// <summary>
    /// Adds a new item
    /// </summary>
    [RelayCommand]
    private void AddItem()
    {
        var newItem = new ItemPrefixSuffix("NewItem", "New Item", "material");
        Items.Add(newItem);
        SelectedItem = newItem;
        StatusMessage = "New item added";
    }

    /// <summary>
    /// Deletes the selected item
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeleteItem))]
    private void DeleteItem()
    {
        if (SelectedItem != null)
        {
            var itemToDelete = SelectedItem;
            SelectedItem = null;
            Items.Remove(itemToDelete);
            StatusMessage = $"Deleted {itemToDelete.Name}";
        }
    }

    private bool CanDeleteItem() => SelectedItem != null;

    partial void OnSelectedItemChanged(ItemPrefixSuffix? value)
    {
        DeleteItemCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        
        if (value != null)
        {
            ValidateItem(value);
            StatusMessage = $"Editing: {value.Name}";
        }
        else
        {
            ValidationErrors = string.Empty;
            HasValidationErrors = false;
            StatusMessage = "No item selected";
        }
    }

    /// <summary>
    /// Validates a single item and updates UI
    /// </summary>
    private void ValidateItem(ItemPrefixSuffix item)
    {
        var result = _validator.Validate(item);
        
        if (result.IsValid)
        {
            ValidationErrors = string.Empty;
            HasValidationErrors = false;
        }
        else
        {
            var errors = new StringBuilder();
            foreach (var error in result.Errors)
            {
                errors.AppendLine($"• {error.ErrorMessage}");
            }
            ValidationErrors = errors.ToString();
            HasValidationErrors = true;
        }

        SaveCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Validates all items in the collection
    /// </summary>
    private bool ValidateAllItems()
    {
        var allErrors = new List<string>();
        
        foreach (var item in Items)
        {
            var result = _validator.Validate(item);
            if (!result.IsValid)
            {
                allErrors.Add($"Item '{item.Name}':");
                foreach (var error in result.Errors)
                {
                    allErrors.Add($"  • {error.ErrorMessage}");
                }
            }
        }

        if (allErrors.Any())
        {
            ValidationErrors = string.Join(Environment.NewLine, allErrors);
            HasValidationErrors = true;
            return false;
        }

        ValidationErrors = string.Empty;
        HasValidationErrors = false;
        return true;
    }

    /// <summary>
    /// Can save if there are no validation errors
    /// </summary>
    private bool CanSave()
    {
        return Items.Any() && !HasValidationErrors;
    }
}
