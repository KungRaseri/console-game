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

    [ObservableProperty]
    private ObservableCollection<string> _componentNames = new();


    // Each component group is a list of dictionaries (dynamic fields)
    [ObservableProperty]
    private Dictionary<string, ObservableCollection<Dictionary<string, object>>> _components = new();

    [ObservableProperty]
    private ObservableCollection<string> _patternNames = new();


    // Each pattern group is a list of dictionaries (dynamic fields)
    [ObservableProperty]
    private Dictionary<string, ObservableCollection<Dictionary<string, object>>> _patterns = new();


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
            if (root["metadata"] is JObject mObj)
            {
                Metadata = mObj.ToObject<NameListMetadata>() ?? new NameListMetadata();
            }
            else
            {
                Metadata = new NameListMetadata();
            }

            // Components (dynamic, generic dictionaries)
            var newComponents = new Dictionary<string, ObservableCollection<Dictionary<string, object>>>();
            var newComponentNames = new ObservableCollection<string>();
            if (root["components"] is JObject compsObj)
            {
                foreach (var prop in compsObj.Properties())
                {
                    var name = prop.Name;
                    newComponentNames.Add(name);
                    var list = new ObservableCollection<Dictionary<string, object>>();
                    if (prop.Value is JArray arr)
                    {
                        foreach (var obj in arr)
                        {
                            if (obj is JObject jobj)
                            {
                                var dict = jobj.ToObject<Dictionary<string, object>>();
                                if (dict != null) list.Add(dict);
                            }
                        }
                    }
                    newComponents[name] = list;
                }
            }
            Components = newComponents;
            ComponentNames = newComponentNames;

            // Patterns (dynamic, generic dictionaries)
            var newPatterns = new Dictionary<string, ObservableCollection<Dictionary<string, object>>>();
            var newPatternNames = new ObservableCollection<string>();
            if (root["patterns"] is JObject patternsObj)
            {
                foreach (var prop in patternsObj.Properties())
                {
                    var name = prop.Name;
                    newPatternNames.Add(name);
                    var list = new ObservableCollection<Dictionary<string, object>>();
                    if (prop.Value is JArray arr)
                    {
                        foreach (var obj in arr)
                        {
                            if (obj is JObject jobj)
                            {
                                var dict = jobj.ToObject<Dictionary<string, object>>();
                                if (dict != null) list.Add(dict);
                            }
                        }
                    }
                    newPatterns[name] = list;
                }
            }
            Patterns = newPatterns;
            PatternNames = newPatternNames;

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
            // Components (dynamic, generic dictionaries)
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
            // Patterns (dynamic, generic dictionaries)
            var patternsObj = new JObject();
            foreach (var kvp in Patterns)
            {
                var arr = new JArray();
                foreach (var patt in kvp.Value)
                {
                    arr.Add(JObject.FromObject(patt));
                }
                patternsObj[kvp.Key] = arr;
            }
            root["patterns"] = patternsObj;
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
