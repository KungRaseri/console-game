using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentValidation;
using Game.ContentBuilder.Models;
using Game.ContentBuilder.Services;
using Game.ContentBuilder.Validators;
using Newtonsoft.Json;
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
            var filePath = System.IO.Path.Combine("items", _fileName);
            var json = System.IO.File.ReadAllText(_jsonEditorService.GetFilePath(filePath));
            
            // Parse the nested JSON structure
            var rawData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, ItemPrefixRaw>>>(json);
            
            if (rawData == null)
            {
                StatusMessage = "Failed to load data - file is empty or invalid";
                Log.Warning("Failed to deserialize {FileName}", _fileName);
                return;
            }

            Items.Clear();

            // Convert the nested dictionary structure to flat list
            foreach (var rarityGroup in rawData)
            {
                var rarity = rarityGroup.Key; // "common", "uncommon", etc.
                
                foreach (var item in rarityGroup.Value)
                {
                    var name = item.Key; // "Rusty", "Steel", etc.
                    var data = item.Value;

                    var prefixSuffix = new ItemPrefixSuffix(name, data.DisplayName ?? name, rarity);

                    // Convert traits
                    if (data.Traits != null)
                    {
                        foreach (var trait in data.Traits)
                        {
                            prefixSuffix.Traits.Add(new ItemTrait(
                                trait.Key,
                                trait.Value.Value ?? 0,
                                trait.Value.Type ?? "number"
                            ));
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

            // Convert flat list back to nested dictionary structure
            var data = new Dictionary<string, Dictionary<string, ItemPrefixRaw>>();

            foreach (var item in Items)
            {
                // Ensure rarity group exists
                if (!data.ContainsKey(item.Rarity))
                {
                    data[item.Rarity] = new Dictionary<string, ItemPrefixRaw>();
                }

                // Convert traits back to dictionary format
                var traits = new Dictionary<string, TraitValue>();
                foreach (var trait in item.Traits)
                {
                    traits[trait.Key] = new TraitValue
                    {
                        Value = trait.Value,
                        Type = trait.Type
                    };
                }

                // Add item to rarity group
                data[item.Rarity][item.Name] = new ItemPrefixRaw
                {
                    DisplayName = item.DisplayName,
                    Traits = traits
                };
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
