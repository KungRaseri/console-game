using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Game.ContentBuilder.ViewModels;

/// <summary>
/// ViewModel for editing abilities.json files (ability_catalog)
/// Handles enemy abilities with name, displayName, description, and rarity
/// </summary>
public partial class AbilitiesEditorViewModel : ObservableObject
{
    private string? _filePath;
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
    private string _editRarity = "Common";

    // Search/Filter
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _filterRarity = "All";

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<string> AvailableRarities { get; } = new()
    {
        "All", "Common", "Uncommon", "Rare", "Epic", "Legendary"
    };

    public void LoadFile(string filePath)
    {
        try
        {
            _filePath = filePath;
            FileName = Path.GetFileName(filePath);

            var json = File.ReadAllText(filePath);
            _jsonData = JObject.Parse(json);

            LoadMetadata();
            LoadAbilities();
            ApplyFilters();

            IsDirty = false;
            StatusMessage = $"Loaded {FileName} - {Abilities.Count} abilities";
            Log.Information("Loaded abilities.json file: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load abilities.json file: {FilePath}", filePath);
            StatusMessage = $"Error loading file: {ex.Message}";
        }
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
                Rarity = item["rarity"]?.ToString() ?? "Common"
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

    private void ApplyFilters()
    {
        FilteredAbilities.Clear();

        var searchLower = SearchText?.ToLowerInvariant() ?? "";

        foreach (var ability in Abilities)
        {
            // Apply rarity filter
            if (FilterRarity != "All" && ability.Rarity != FilterRarity)
                continue;

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

        StatusMessage = $"Showing {FilteredAbilities.Count} of {Abilities.Count} abilities";
    }

    [RelayCommand]
    private void AddAbility()
    {
        EditName = "";
        EditDisplayName = "";
        EditDescription = "";
        EditRarity = "Common";
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
        EditRarity = SelectedAbility.Rarity;
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
                Rarity = EditRarity
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
            SelectedAbility.Rarity = EditRarity;
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
        if (_filePath == null || _jsonData == null) return;

        try
        {
            // Update metadata total count
            var metadata = _jsonData["metadata"] as JObject;
            if (metadata != null)
            {
                metadata["total_abilities"] = Abilities.Count;
                metadata["last_updated"] = DateTime.Now.ToString("yyyy-MM-dd");
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
                    ["rarity"] = ability.Rarity
                };
                items.Add(item);
            }

            _jsonData["items"] = items;

            // Save to file
            File.WriteAllText(_filePath, _jsonData.ToString(Newtonsoft.Json.Formatting.Indented));

            IsDirty = false;
            StatusMessage = $"Saved {FileName} - {Abilities.Count} abilities";
            Log.Information("Saved abilities.json file: {FilePath}", _filePath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save abilities.json file: {FilePath}", _filePath);
            StatusMessage = $"Error saving file: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ReloadFile()
    {
        if (_filePath == null) return;

        if (IsDirty)
        {
            StatusMessage = "Warning: Unsaved changes will be lost";
            // In a real app, would show a confirmation dialog
        }

        LoadFile(_filePath);
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
        FilterRarity = "All";
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
    private string _rarity = "Common";
}
