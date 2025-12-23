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

    // v4 metadata
    [ObservableProperty]
    private NameListMetadata _metadata = new();

    // v4 pattern tokens
    [ObservableProperty]
    private ObservableCollection<string> _patternTokens = new();

    // v4 componentKeys
    [ObservableProperty]
    private ObservableCollection<string> _componentKeys = new();

    // Dynamic components: key = component name, value = list of NameComponent
    [ObservableProperty]
    private ObservableCollection<string> _componentNames = new();
    [ObservableProperty]
    private Dictionary<string, ObservableCollection<NameComponent>> _components = new();

    // UI selection
    [ObservableProperty]
    private NameComponent? _selectedTitle;
    [ObservableProperty]
    private NameComponent? _selectedFirstName;
    [ObservableProperty]
    private NameComponent? _selectedSurname;
    [ObservableProperty]
    private NameComponent? _selectedSuffix;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    public NameListEditorViewModel(JsonEditorService jsonEditorService, string fileName)
    {
        _jsonEditorService = jsonEditorService;
        _fileName = fileName;
        LoadData();
    }

    /// <summary>
    /// Loads v4 names.json data (metadata, patternTokens, componentKeys, components)
    /// </summary>
    private void LoadData()
    {
        try
        {
            var filePath = _fileName;
            var json = System.IO.File.ReadAllText(_jsonEditorService.GetFilePath(filePath));
            var root = JObject.Parse(json);

            // Metadata
            JObject? metadataObj = null;
            if (root["metadata"] is JObject mObj)
            {
                metadataObj = mObj;
                Metadata = mObj.ToObject<NameListMetadata>() ?? new NameListMetadata();
            }

            // Pattern tokens (support both camelCase and snake_case, root and metadata)
            PatternTokens.Clear();
            JArray? patternTokensArr =
                (root["patternTokens"] as JArray) ??
                (root["patternTokens"] as JArray) ??
                (metadataObj != null ? (metadataObj["patternTokens"] as JArray) : null) ??
                (metadataObj != null ? (metadataObj["patternTokens"] as JArray) : null);
            if (patternTokensArr != null)
            {
                foreach (var token in patternTokensArr)
                    PatternTokens.Add(token.ToString());
            }

            // Component keys (support both camelCase and snake_case, root and metadata)
            ComponentKeys.Clear();
            JArray? componentKeysArr =
                (root["componentKeys"] as JArray) ??
                (root["componentKeys"] as JArray) ??
                (metadataObj != null ? (metadataObj["componentKeys"] as JArray) : null) ??
                (metadataObj != null ? (metadataObj["componentKeys"] as JArray) : null);
            if (componentKeysArr != null)
            {
                foreach (var key in componentKeysArr)
                    ComponentKeys.Add(key.ToString());
            }

            // Dynamic components
            Components.Clear();
            ComponentNames.Clear();
            if (root["components"] is JObject compsObj)
            {
                foreach (var prop in compsObj.Properties())
                {
                    var name = prop.Name;
                    ComponentNames.Add(name);
                    var arr = prop.Value as JArray;
                    var list = new ObservableCollection<NameComponent>();
                    if (arr != null)
                    {
                        foreach (var obj in arr)
                        {
                            var comp = obj.ToObject<NameComponent>();
                            if (comp != null) list.Add(comp);
                        }
                    }
                    Components[name] = list;
                }
            }

            StatusMessage = $"Loaded v4 names.json from {filePath}";
            Log.Information("Loaded v4 names.json from {FileName}", filePath);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading data: {ex.Message}";
            Log.Error(ex, "Failed to load {FileName}", _fileName);
        }
    }

    /// <summary>
    /// Saves changes back to JSON file (v4 pattern-based structure)
    /// </summary>
    [RelayCommand]
    private void Save()
    {
        try
        {
            var root = new JObject();
            // Metadata
            root["metadata"] = JObject.FromObject(Metadata);
            // Pattern tokens
            root["patternTokens"] = new JArray(PatternTokens);
            // Component keys
            root["componentKeys"] = new JArray(ComponentKeys);
            // Components (dynamic)
            var componentsObj = new JObject();
            foreach (var kvp in Components)
            {
                var arr = new JArray();
                foreach (var comp in kvp.Value)
                {
                    arr.Add(JObject.FromObject(comp));
                }
                componentsObj[kvp.Key] = arr;
            }
            root["components"] = componentsObj;
            // Write to file
            var filePath = _fileName;
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
        StatusMessage = "Changes cancelled - data reloaded";
    }
}
