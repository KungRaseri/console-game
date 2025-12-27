using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using Serilog;
using Game.ContentBuilder.Services;
using Game.Shared.Data.Models;

namespace Game.ContentBuilder.ViewModels;

/// <summary>
/// ViewModel for editing abilities.json files (ability_catalog)
/// Handles enemy abilities with name, displayName, description, and rarity
/// </summary>
public partial class AbilitiesEditorViewModel : ObservableObject
{
    private readonly JsonEditorService _jsonEditorService;
    private readonly string _storedFileName;
    private JObject? _jsonData;

    [ObservableProperty]
    private string _fileName = string.Empty;

    // Metadata (read-only display)
    [ObservableProperty]
    private string _metadataVersion = string.Empty;

    [ObservableProperty]
    private string _metadataType = string.Empty;

    [ObservableProperty]
    private string _metadataDescription = string.Empty;

    [ObservableProperty]
    private string _metadataUsage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _metadataNotes = new();

    // Abilities
    [ObservableProperty]
    private ObservableCollection<AbilityItem> _abilities = new();

    [ObservableProperty]
    private ObservableCollection<AbilityItem> _filteredAbilities = new();

    [ObservableProperty]
    private AbilityItem? _selectedAbility;

    // Editing
    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _editName = string.Empty;

    [ObservableProperty]
    private string _editDisplayName = string.Empty;

    [ObservableProperty]
    private string _editDescription = string.Empty;

    [ObservableProperty]
    private int _editRarityWeight = 10;

    // Search/Filter
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _filterRarity = "All";

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // V4 Format Support
    [ObservableProperty]
    private bool _isV4Format;

    [ObservableProperty]
    private string _selectedCategory = "All";

    private AbilityCatalogData? _catalogData;
    private AbilityNamesData? _namesData;

    // V4 Categories (standardized across all enemy types)
    public ObservableCollection<string> AvailableCategories { get; } = new()
    {
        "All", "offensive", "defensive", "control", "utility", "legendary"
    };

    public ObservableCollection<string> AvailableRarities { get; } = new()
    {
        "All", "Common", "Uncommon", "Rare", "Epic", "Legendary"
    };

    public AbilitiesEditorViewModel(JsonEditorService jsonEditorService, string fileName)
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
            // Detect v4 format: Check if abilities_catalog.json and abilities_names.json exist
            var directory = Path.GetDirectoryName(_jsonEditorService.GetFilePath(_storedFileName));
            var catalogPath = Path.Combine(directory!, "abilities_catalog.json");
            var namesPath = Path.Combine(directory!, "abilities_names.json");

            IsV4Format = File.Exists(catalogPath) && File.Exists(namesPath);

            if (IsV4Format)
            {
                LoadV4Data(catalogPath, namesPath);
            }
            else
            {
                LoadV3Data();
            }

            ApplyFilters();
            IsDirty = false;
            Log.Information("Loaded abilities file: {FileName}, Format: {Format}", FileName, IsV4Format ? "v4.0" : "v3.0");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load abilities file: {FileName}", _storedFileName);
            StatusMessage = $"Error loading file: {ex.Message}";
        }
    }

    private void LoadV3Data()
    {
        var filePath = _jsonEditorService.GetFilePath(_storedFileName);
        var json = File.ReadAllText(filePath);
        _jsonData = JObject.Parse(json);

        LoadMetadata();
        LoadAbilities();

        StatusMessage = $"Loaded {FileName} (v3.0) - {Abilities.Count} abilities";
        Log.Information("Loaded v3.0 abilities.json file: {FilePath}", filePath);
    }

    private void LoadV4Data(string catalogPath, string namesPath)
    {
        // Load catalog data
        var catalogJson = File.ReadAllText(catalogPath);
        _catalogData = JsonSerializer.Deserialize<AbilityCatalogData>(catalogJson);

        // Load names data
        var namesJson = File.ReadAllText(namesPath);
        _namesData = JsonSerializer.Deserialize<AbilityNamesData>(namesJson);

        if (_catalogData == null)
        {
            StatusMessage = "Error: Failed to parse abilities_catalog.json";
            Log.Error("Failed to parse abilities_catalog.json");
            return;
        }

        // Load metadata from catalog
        MetadataVersion = _catalogData.Metadata.Version;
        MetadataType = _catalogData.Metadata.Type;
        MetadataDescription = _catalogData.Metadata.Description;
        MetadataUsage = _catalogData.Metadata.Usage ?? "";

        MetadataNotes.Clear();
        if (_namesData?.Metadata.Notes != null)
        {
            foreach (var note in _namesData.Metadata.Notes)
            {
                MetadataNotes.Add(note);
            }
        }

        // Load abilities from all ability types
        Abilities.Clear();
        foreach (var abilityType in _catalogData.AbilityTypes)
        {
            var category = abilityType.Key;
            foreach (var item in abilityType.Value.Items)
            {
                var ability = new AbilityItem
                {
                    Name = item.Name,
                    DisplayName = item.DisplayName,
                    Description = item.Description,
                    RarityWeight = item.RarityWeight,
                    Category = category
                };

                Abilities.Add(ability);
            }
        }

        StatusMessage = $"Loaded {FileName} (v4.0) - {Abilities.Count} abilities across {_catalogData.AbilityTypes.Count} categories";
        Log.Information("Loaded v4.0 abilities files: {CatalogPath}, {NamesPath}", catalogPath, namesPath);
    }

    private void LoadMetadata()
    {
        if (_jsonData == null) return;

        var metadata = _jsonData["metadata"] as JObject;
        if (metadata == null) return;

        MetadataVersion = metadata["version"]?.ToString() ?? "1.0";
        MetadataType = metadata["type"]?.ToString() ?? "ability_catalog";
        MetadataDescription = metadata["description"]?.ToString() ?? "";
        MetadataUsage = metadata["usage"]?.ToString() ?? "";

        MetadataNotes.Clear();
        var notes = metadata["notes"] as JArray;
        if (notes != null)
        {
            foreach (var note in notes)
            {
                MetadataNotes.Add(note.ToString());
            }
        }
    }

    private void LoadAbilities()
    {
        if (_jsonData == null) return;

        Abilities.Clear();

        var items = _jsonData["items"] as JArray;
        if (items == null) return;

        foreach (var item in items)
        {
            var ability = new AbilityItem
            {
                Name = item["name"]?.ToString() ?? "",
                DisplayName = item["displayName"]?.ToString() ?? "",
                Description = item["description"]?.ToString() ?? "",
                RarityWeight = item["rarityWeight"]?.ToObject<int>() ?? 10
            };

            Abilities.Add(ability);
        }

        Log.Debug("Loaded {Count} abilities from {FileName}", Abilities.Count, FileName);
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnFilterRarityChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        FilteredAbilities.Clear();

        var searchLower = SearchText?.ToLowerInvariant() ?? "";

        foreach (var ability in Abilities)
        {
            // Apply category filter (v4 only)
            if (IsV4Format && SelectedCategory != "All")
            {
                if (ability.Category != SelectedCategory)
                    continue;
            }

            // Apply rarity filter based on weight ranges
            if (FilterRarity != "All")
            {
                var rarityName = GetRarityName(ability.RarityWeight);
                if (rarityName != FilterRarity)
                    continue;
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchLower))
            {
                var matchesName = ability.Name?.ToLowerInvariant().Contains(searchLower) ?? false;
                var matchesDisplayName = ability.DisplayName?.ToLowerInvariant().Contains(searchLower) ?? false;
                var matchesDescription = ability.Description?.ToLowerInvariant().Contains(searchLower) ?? false;

                if (!matchesName && !matchesDisplayName && !matchesDescription)
                    continue;
            }

            FilteredAbilities.Add(ability);
        }

        var filterInfo = IsV4Format && SelectedCategory != "All"
            ? $" (Category: {SelectedCategory})"
            : "";
        StatusMessage = $"Showing {FilteredAbilities.Count} of {Abilities.Count} abilities{filterInfo}";
    }

    [RelayCommand]
    private void AddAbility()
    {
        EditName = "";
        EditDisplayName = "";
        EditDescription = "";
        EditRarityWeight = 10;
        IsEditing = true;
        StatusMessage = "Adding new ability...";
    }

    [RelayCommand]
    private void EditAbility()
    {
        if (SelectedAbility == null) return;

        EditName = SelectedAbility.Name;
        EditDisplayName = SelectedAbility.DisplayName;
        EditDescription = SelectedAbility.Description;
        EditRarityWeight = SelectedAbility.RarityWeight;
        IsEditing = true;
        StatusMessage = $"Editing {SelectedAbility.DisplayName}...";
    }

    [RelayCommand]
    private void SaveEdit()
    {
        if (string.IsNullOrWhiteSpace(EditName))
        {
            StatusMessage = "Error: Name is required";
            return;
        }

        if (SelectedAbility == null)
        {
            // Adding new ability
            var newAbility = new AbilityItem
            {
                Name = EditName,
                DisplayName = string.IsNullOrWhiteSpace(EditDisplayName) ? EditName : EditDisplayName,
                Description = EditDescription,
                RarityWeight = EditRarityWeight
            };

            Abilities.Add(newAbility);
            StatusMessage = $"Added ability: {newAbility.DisplayName}";
            Log.Information("Added new ability: {Name}", newAbility.Name);
        }
        else
        {
            // Updating existing ability
            SelectedAbility.Name = EditName;
            SelectedAbility.DisplayName = string.IsNullOrWhiteSpace(EditDisplayName) ? EditName : EditDisplayName;
            SelectedAbility.Description = EditDescription;
            SelectedAbility.RarityWeight = EditRarityWeight;
            StatusMessage = $"Updated ability: {SelectedAbility.DisplayName}";
            Log.Information("Updated ability: {Name}", SelectedAbility.Name);
        }

        IsEditing = false;
        IsDirty = true;
        ApplyFilters();
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        StatusMessage = "Edit cancelled";
    }

    [RelayCommand]
    private void DeleteAbility()
    {
        if (SelectedAbility == null) return;

        var abilityName = SelectedAbility.DisplayName;
        Abilities.Remove(SelectedAbility);
        SelectedAbility = null;
        IsDirty = true;
        ApplyFilters();
        StatusMessage = $"Deleted ability: {abilityName}";
        Log.Information("Deleted ability: {Name}", abilityName);
    }

    [RelayCommand]
    private void SaveFile()
    {
        if (IsV4Format)
        {
            // V4 format editing not yet fully implemented
            StatusMessage = "V4 format editing not yet fully implemented. Files are read-only for now.";
            Log.Warning("Attempted to save v4 format file, but editing not yet implemented");
            return;
        }

        if (_jsonData == null) return;

        try
        {
            var filePath = _jsonEditorService.GetFilePath(_storedFileName);

            // Update metadata total count
            var metadata = _jsonData["metadata"] as JObject;
            if (metadata != null)
            {
                metadata["total_abilities"] = Abilities.Count;
                metadata["lastUpdated"] = DateTime.Now.ToString("yyyy-MM-dd");
            }

            // Update items array
            var items = new JArray();
            foreach (var ability in Abilities)
            {
                var item = new JObject
                {
                    ["name"] = ability.Name,
                    ["displayName"] = ability.DisplayName,
                    ["description"] = ability.Description,
                    ["rarityWeight"] = ability.RarityWeight
                };
                items.Add(item);
            }

            _jsonData["items"] = items;

            // Save to file
            File.WriteAllText(filePath, _jsonData.ToString(Newtonsoft.Json.Formatting.Indented));

            IsDirty = false;
            StatusMessage = $"Saved {FileName} - {Abilities.Count} abilities";
            Log.Information("Saved abilities.json file: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save abilities.json file: {FileName}", _storedFileName);
            StatusMessage = $"Error saving file: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ReloadFile()
    {
        if (IsDirty)
        {
            StatusMessage = "Warning: Unsaved changes will be lost";
            // In a real app, would show a confirmation dialog
        }

        LoadData();
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
        FilterRarity = "All";
    }

    private static string GetRarityName(int weight)
    {
        return RarityConfigService.Instance.GetRarityName(weight);
    }
}

/// <summary>
/// Represents a single ability item
/// </summary>
public partial class AbilityItem : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private int _rarityWeight = 10;

    [ObservableProperty]
    private string _category = string.Empty; // v4 only: offensive, defensive, control, utility, legendary
}
