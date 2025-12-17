using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Game.ContentBuilder.ViewModels;

/// <summary>
/// ViewModel for editing names.json files (pattern generation)
/// </summary>
public partial class NamesEditorViewModel : ObservableObject
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

    // Components
    [ObservableProperty]
    private ObservableCollection<NameComponentGroup> _componentGroups = new();

    [ObservableProperty]
    private NameComponentGroup? _selectedComponentGroup;

    [ObservableProperty]
    private NameComponentItem? _selectedComponent;

    // Patterns
    [ObservableProperty]
    private ObservableCollection<NamePatternItem> _patterns = new();

    [ObservableProperty]
    private NamePatternItem? _selectedPattern;

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public void LoadFile(string filePath)
    {
        try
        {
            _filePath = filePath;
            FileName = Path.GetFileName(filePath);

            var json = File.ReadAllText(filePath);
            _jsonData = JObject.Parse(json);

            LoadMetadata();
            LoadComponents();
            LoadPatterns();

            IsDirty = false;
            StatusMessage = $"Loaded {FileName}";
            Log.Information("Loaded names.json file: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load names.json file: {FilePath}", filePath);
            StatusMessage = $"Error loading file: {ex.Message}";
        }
    }

    private void LoadMetadata()
    {
        if (_jsonData == null) return;

        var metadata = _jsonData["metadata"] as JObject;
        if (metadata == null) return;

        MetadataVersion = metadata["version"]?.ToString() ?? string.Empty;
        MetadataType = metadata["type"]?.ToString() ?? string.Empty;
        MetadataDescription = metadata["description"]?.ToString() ?? string.Empty;
        MetadataUsage = metadata["usage"]?.ToString() ?? string.Empty;

        MetadataNotes.Clear();
        var notes = metadata["notes"] as JArray;
        if (notes != null)
        {
            foreach (var note in notes)
            {
                var noteText = note.ToString();
                if (!string.IsNullOrWhiteSpace(noteText))
                {
                    MetadataNotes.Add(noteText);
                }
            }
        }
    }

    private void LoadComponents()
    {
        if (_jsonData == null) return;

        ComponentGroups.Clear();
        var components = _jsonData["components"] as JObject;
        if (components == null) return;

        foreach (var group in components.Properties())
        {
            var componentGroup = new NameComponentGroup
            {
                Name = group.Name,
                Components = new ObservableCollection<NameComponentItem>()
            };

            if (group.Value is JArray componentArray)
            {
                foreach (var comp in componentArray)
                {
                    if (comp is JObject compObj)
                    {
                        var component = new NameComponentItem
                        {
                            Value = compObj["value"]?.ToString() ?? string.Empty,
                            Weight = compObj["weight"]?.ToObject<int>() ?? 1
                        };

                        // Load traits if present
                        var traits = compObj["traits"] as JArray;
                        if (traits != null)
                        {
                            foreach (var trait in traits)
                            {
                                component.Traits.Add(trait.ToString());
                            }
                        }

                        componentGroup.Components.Add(component);
                    }
                }
            }

            ComponentGroups.Add(componentGroup);
        }
    }

    private void LoadPatterns()
    {
        if (_jsonData == null) return;

        Patterns.Clear();
        var patterns = _jsonData["patterns"] as JArray;
        if (patterns == null) return;

        foreach (var pattern in patterns)
        {
            if (pattern is JObject patternObj)
            {
                var patternItem = new NamePatternItem
                {
                    Pattern = patternObj["pattern"]?.ToString() ?? string.Empty,
                    Weight = patternObj["weight"]?.ToObject<int>() ?? 1,
                    Example = patternObj["example"]?.ToString() ?? string.Empty
                };

                Patterns.Add(patternItem);
            }
        }
    }

    [RelayCommand]
    private void AddComponentGroup()
    {
        var newGroup = new NameComponentGroup
        {
            Name = "new_group",
            Components = new ObservableCollection<NameComponentItem>()
        };

        ComponentGroups.Add(newGroup);
        SelectedComponentGroup = newGroup;
        IsDirty = true;
        StatusMessage = "Added new component group";
    }

    [RelayCommand]
    private void RemoveComponentGroup()
    {
        if (SelectedComponentGroup == null) return;

        ComponentGroups.Remove(SelectedComponentGroup);
        SelectedComponentGroup = null;
        IsDirty = true;
        StatusMessage = "Removed component group";
    }

    [RelayCommand]
    private void AddComponent()
    {
        if (SelectedComponentGroup == null) return;

        var newComponent = new NameComponentItem
        {
            Value = "new_component",
            Weight = 1
        };

        SelectedComponentGroup.Components.Add(newComponent);
        SelectedComponent = newComponent;
        IsDirty = true;
        StatusMessage = "Added new component";
    }

    [RelayCommand]
    private void RemoveComponent()
    {
        if (SelectedComponentGroup == null || SelectedComponent == null) return;

        SelectedComponentGroup.Components.Remove(SelectedComponent);
        SelectedComponent = null;
        IsDirty = true;
        StatusMessage = "Removed component";
    }

    [RelayCommand]
    private void AddPattern()
    {
        var newPattern = new NamePatternItem
        {
            Pattern = "{component}",
            Weight = 1,
            Example = "Example Name"
        };

        Patterns.Add(newPattern);
        SelectedPattern = newPattern;
        IsDirty = true;
        StatusMessage = "Added new pattern";
    }

    [RelayCommand]
    private void RemovePattern()
    {
        if (SelectedPattern == null) return;

        Patterns.Remove(SelectedPattern);
        SelectedPattern = null;
        IsDirty = true;
        StatusMessage = "Removed pattern";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_filePath == null || _jsonData == null) return;

        try
        {
            // Update components
            var componentsObj = new JObject();
            foreach (var group in ComponentGroups)
            {
                var componentArray = new JArray();
                foreach (var comp in group.Components)
                {
                    var compObj = new JObject
                    {
                        ["value"] = comp.Value,
                        ["weight"] = comp.Weight
                    };

                    if (comp.Traits.Count > 0)
                    {
                        compObj["traits"] = new JArray(comp.Traits);
                    }

                    componentArray.Add(compObj);
                }
                componentsObj[group.Name] = componentArray;
            }
            _jsonData["components"] = componentsObj;

            // Update patterns
            var patternsArray = new JArray();
            foreach (var pattern in Patterns)
            {
                patternsArray.Add(new JObject
                {
                    ["pattern"] = pattern.Pattern,
                    ["weight"] = pattern.Weight,
                    ["example"] = pattern.Example
                });
            }
            _jsonData["patterns"] = patternsArray;

            // Write to file with formatting
            await File.WriteAllTextAsync(_filePath, _jsonData.ToString(Newtonsoft.Json.Formatting.Indented));

            IsDirty = false;
            StatusMessage = $"Saved {FileName}";
            Log.Information("Saved names.json file: {FilePath}", _filePath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save names.json file: {FilePath}", _filePath);
            StatusMessage = $"Error saving file: {ex.Message}";
        }
    }
}

/// <summary>
/// Represents a group of components
/// </summary>
public partial class NameComponentGroup : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private ObservableCollection<NameComponentItem> _components = new();
}

/// <summary>
/// Represents a single component with traits
/// </summary>
public partial class NameComponentItem : ObservableObject
{
    [ObservableProperty]
    private string _value = string.Empty;

    [ObservableProperty]
    private int _weight = 1;

    [ObservableProperty]
    private ObservableCollection<string> _traits = new();
}

/// <summary>
/// Represents a name generation pattern
/// </summary>
public partial class NamePatternItem : ObservableObject
{
    [ObservableProperty]
    private string _pattern = string.Empty;

    [ObservableProperty]
    private int _weight = 1;

    [ObservableProperty]
    private string _example = string.Empty;
}
