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
/// ViewModel for editing item prefixes/suffixes
/// </summary>
public partial class ItemEditorViewModel : ObservableObject
{
    private readonly JsonEditorService _jsonEditorService;
    private readonly string _fileName;
    private readonly ItemPrefixSuffixValidator _validator;

    [ObservableProperty]
    private ObservableCollection<ItemPrefixSuffix> _items = new();

    [ObservableProperty]
    private ItemPrefixSuffix? _selectedItem;

    [ObservableProperty]
    private ObservableCollection<string> _rarities = new() 
    { 
        "common", 
        "uncommon", 
        "rare", 
        "epic", 
        "legendary" 
    };

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _validationErrors = string.Empty;

    [ObservableProperty]
    private bool _hasValidationErrors = false;

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

    public ItemEditorViewModel(JsonEditorService jsonEditorService, string fileName)
    {
        _jsonEditorService = jsonEditorService;
        _fileName = fileName;
        _validator = new ItemPrefixSuffixValidator();
        
        LoadData();
    }

    /// <summary>
    /// Loads data from JSON file
    /// </summary>
    private void LoadData()
    {
        try
        {
            var filePath = _fileName; // fileName already includes full path like "items/weapons/prefixes.json"
            var json = System.IO.File.ReadAllText(_jsonEditorService.GetFilePath(filePath));
            
            // Parse as JObject to extract metadata
            var root = JObject.Parse(json);
            
            // Extract metadata if present
            if (root["_metadata"] is JObject metadata)
            {
                MetadataDescription = metadata["description"]?.ToString() ?? string.Empty;
                MetadataVersion = metadata["version"]?.ToString() ?? string.Empty;
                MetadataLastUpdated = metadata["lastUpdated"]?.ToString() ?? string.Empty;
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

            Items.Clear();

            // Convert the nested dictionary structure to flat list
            foreach (var rarityProp in root.Properties().Where(p => p.Name != "_metadata"))
            {
                var rarity = rarityProp.Name; // "common", "uncommon", etc.
                
                if (rarityProp.Value is JObject rarityObj)
                {
                    foreach (var itemProp in rarityObj.Properties())
                    {
                        var name = itemProp.Name; // "Rusty", "Steel", etc.
                        var itemObj = itemProp.Value as JObject;
                        
                        if (itemObj == null) continue;

                        var displayName = itemObj["displayName"]?.ToString() ?? name;
                        var prefixSuffix = new ItemPrefixSuffix(name, displayName, rarity);

                        // Convert traits
                        if (itemObj["traits"] is JObject traitsObj)
                        {
                            foreach (var traitProp in traitsObj.Properties())
                            {
                                var traitObj = traitProp.Value as JObject;
                                if (traitObj != null)
                                {
                                    var traitValue = traitObj["value"]?.ToObject<object>() ?? 0;
                                    var traitType = traitObj["type"]?.ToString() ?? "number";
                                    
                                    prefixSuffix.Traits.Add(new ItemTrait(
                                        traitProp.Name,
                                        traitValue,
                                        traitType
                                    ));
                                }
                            }
                        }

                        // Subscribe to property changes for validation
                        prefixSuffix.PropertyChanged += (s, e) =>
                        {
                            if (s == SelectedItem && SelectedItem != null)
                            {
                                ValidateItem(SelectedItem);
                            }
                        };

                        Items.Add(prefixSuffix);
                    }
                }
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
    /// Saves changes back to JSON file
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

            // Build root JObject
            var root = new JObject();

            // Add metadata first
            var metadata = new JObject
            {
                ["description"] = MetadataDescription,
                ["version"] = MetadataVersion,
                ["lastUpdated"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                ["type"] = "item_modifiers"
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
            var rarityBreakdown = Items.GroupBy(i => i.Rarity).ToDictionary(g => g.Key, g => g.Count());
            metadata["totals"] = new JObject
            {
                ["total_items"] = Items.Count,
                ["by_rarity"] = JObject.FromObject(rarityBreakdown)
            };

            root["_metadata"] = metadata;

            // Convert flat list back to nested rarity structure
            foreach (var rarityGroup in Items.GroupBy(i => i.Rarity))
            {
                var rarityObj = new JObject();
                
                foreach (var item in rarityGroup)
                {
                    // Convert traits to JObject
                    var traits = new JObject();
                    foreach (var trait in item.Traits)
                    {
                        traits[trait.Key] = new JObject
                        {
                            ["value"] = JToken.FromObject(trait.Value),
                            ["type"] = trait.Type
                        };
                    }

                    rarityObj[item.Name] = new JObject
                    {
                        ["displayName"] = item.DisplayName,
                        ["traits"] = traits
                    };
                }

                root[rarityGroup.Key] = rarityObj;
            }

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
        SelectedItem = null;
        StatusMessage = "Changes cancelled - data reloaded";
    }

    /// <summary>
    /// Adds a new item
    /// </summary>
    [RelayCommand]
    private void AddItem()
    {
        var newItem = new ItemPrefixSuffix("NewItem", "New Item", "common");
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
            StatusMessage = $"Editing: {value.Name} ({value.Rarity})";
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
