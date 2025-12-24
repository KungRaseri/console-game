using System.Collections.ObjectModel;
using System.IO;
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

    [ObservableProperty]
    private Dictionary<string, ObservableCollection<NameComponentBase>> _components = new();

    [ObservableProperty]
    private ObservableCollection<NamePatternBase> _patterns = new();

    [ObservableProperty]
    private NamePatternBase? _selectedPattern;

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
            Log.Information("Loading names.json from {FilePath}", filePath);
            
            var json = System.IO.File.ReadAllText(_jsonEditorService.GetFilePath(filePath));
            Log.Debug("Read {Length} characters from file", json.Length);
            
            var root = JObject.Parse(json);
            Log.Debug("Parsed JSON successfully");

            // Metadata
            JObject? metadataObj = null;
            if (root["metadata"] is JObject mObj)
            {
                metadataObj = mObj;
                Metadata = mObj.ToObject<NameListMetadata>() ?? new NameListMetadata();
                Log.Debug("Loaded metadata");
            }

            // Dynamic components
            Components.Clear();
            ComponentNames.Clear();
            Log.Debug("Loading components...");
            if (root["components"] is JObject compsObj)
            {
                Log.Debug("Found components object with {Count} properties", compsObj.Properties().Count());
                foreach (var prop in compsObj.Properties())
                {
                    var name = prop.Name;
                    ComponentNames.Add(name);
                    var list = new ObservableCollection<NameComponentBase>();
                    if (prop.Value is JArray arr)
                    {
                        for (int i = 0; i < arr.Count; i++)
                        {
                            try
                            {
                                var obj = arr[i] as JObject;
                                if (obj == null) continue;
                                
                                // Manual deserialization to avoid converter recursion
                                NameComponentBase? comp = null;
                                
                                // Check for NPC-specific fields
                                if (obj.ContainsKey("gender") || obj.ContainsKey("preferredSocialClass") || obj.ContainsKey("weightMultiplier"))
                                {
                                    comp = new NpcNameComponent
                                    {
                                        Value = obj["value"]?.ToString() ?? string.Empty,
                                        RarityWeight = obj["rarityWeight"]?.Value<int>() ?? 0,
                                        Gender = obj["gender"]?.ToString(),
                                        PreferredSocialClass = obj["preferredSocialClass"]?.ToString(),
                                        WeightMultiplier = obj["weightMultiplier"]?.ToObject<Dictionary<string, double>>()
                                    };
                                }
                                // Check for Item-specific fields
                                else if (obj.ContainsKey("traits"))
                                {
                                    var itemComp = new ItemNameComponent
                                    {
                                        Value = obj["value"]?.ToString() ?? string.Empty,
                                        RarityWeight = obj["rarityWeight"]?.Value<int>() ?? 0,
                                        Traits = new Dictionary<string, JObject>()
                                    };
                                    
                                    if (obj["traits"] is JObject traitsObj)
                                    {
                                        foreach (var trait in traitsObj)
                                        {
                                            itemComp.Traits[trait.Key] = trait.Value as JObject ?? new JObject();
                                        }
                                    }
                                    comp = itemComp;
                                }
                                // Default/Basic component (just value + rarityWeight)
                                else
                                {
                                    comp = new ItemNameComponent
                                    {
                                        Value = obj["value"]?.ToString() ?? string.Empty,
                                        RarityWeight = obj["rarityWeight"]?.Value<int>() ?? 0
                                    };
                                }
                                
                                if (comp != null)
                                {
                                    list.Add(comp);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "Failed to deserialize component {Index} of {Total} for {Name}. JSON: {Json}", i + 1, arr.Count, name, arr[i].ToString());
                                // Continue to next item instead of failing completely
                            }
                        }
                    }
                    Components[name] = list;
                }
                Log.Information("Loaded {Count} component groups with {Total} total components", Components.Count, Components.Values.Sum(c => c.Count));
            }

            // Dynamic patterns (can be string array OR object array)
            Patterns.Clear();
            if (root["patterns"] is JArray patternsArr)
            {
                for (int i = 0; i < patternsArr.Count; i++)
                {
                    try
                    {
                        NamePatternBase? pattern = null;
                        
                        // Check if it's a simple string (like enemies/beasts)
                        if (patternsArr[i].Type == JTokenType.String)
                        {
                            var templateStr = patternsArr[i].ToString();
                            // Strip curly braces: {base} -> base, {title} {first_name} -> title first_name
                            templateStr = System.Text.RegularExpressions.Regex.Replace(templateStr, @"\{([^}]+)\}", "$1");
                            
                            pattern = new ItemNamePattern
                            {
                                PatternTemplate = templateStr,
                                Weight = 10, // Default weight for string patterns
                                Description = null
                            };
                        }
                        // Object pattern (NPCs, Items with metadata)
                        else if (patternsArr[i] is JObject obj)
                        {
                            var templateStr = obj["template"]?.ToString() ?? obj["pattern"]?.ToString() ?? string.Empty;
                            // Strip curly braces from template
                            templateStr = System.Text.RegularExpressions.Regex.Replace(templateStr, @"\{([^}]+)\}", "$1");
                            
                            // Check for NPC-specific fields
                            if (obj.ContainsKey("socialClass") || obj.ContainsKey("requiresTitle") || obj.ContainsKey("excludeTitles"))
                            {
                                pattern = new NpcNamePattern
                                {
                                    PatternTemplate = templateStr,
                                    Weight = obj["rarityWeight"]?.Value<int>() ?? obj["weight"]?.Value<int>() ?? 0,
                                    Description = obj["description"]?.ToString(),
                                    SocialClass = obj["socialClass"]?.ToObject<ObservableCollection<string>>() ?? new ObservableCollection<string>(),
                                    RequiresTitle = obj["requiresTitle"]?.Value<bool>(),
                                    ExcludeTitles = obj["excludeTitles"]?.Value<bool>()
                                };
                            }
                            // Default/Item pattern
                            else
                            {
                                pattern = new ItemNamePattern
                                {
                                    PatternTemplate = templateStr,
                                    Weight = obj["rarityWeight"]?.Value<int>() ?? obj["weight"]?.Value<int>() ?? 0,
                                    Description = obj["description"]?.ToString()
                                };
                            }
                        }
                        
                        if (pattern != null)
                        {
                            Patterns.Add(pattern);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to deserialize pattern {Index} of {Total}. JSON: {Json}", i + 1, patternsArr.Count, patternsArr[i].ToString());
                        // Continue to next item instead of failing completely
                    }
                }
                Log.Information("Loaded {Count} patterns", Patterns.Count);
            }

            // Generate examples for patterns
            GeneratePatternExamples();

            StatusMessage = $"Loaded v4 names.json from {filePath}";
            Log.Information("Successfully loaded v4 names.json from {FileName}", filePath);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading data: {ex.Message}";
            Log.Error(ex, "Failed to load {FileName}", _fileName);
        }
    }

    /// <summary>
    /// Generates example names for each pattern by randomly selecting components
    /// </summary>
    private void GeneratePatternExamples()
    {
        var random = new Random();
        
        // Load base names from catalog.json in the same directory
        var baseNames = LoadBaseNamesFromCatalog();
        
        foreach (var pattern in Patterns)
        {
            try
            {
                var examples = new List<string>();
                var usedExamples = new HashSet<string>(); // Track used examples to avoid duplicates
                
                // Generate up to 3 unique example names
                int maxAttempts = 20; // Prevent infinite loops if components are limited
                int attempts = 0;
                
                while (examples.Count < 3 && attempts < maxAttempts)
                {
                    attempts++;
                    string example = pattern.PatternTemplate;
                    
                    // Replace + and - with spaces for cleaner display
                    example = example.Replace("+", " ").Replace("-", " ");
                    
                    // Split pattern by spaces
                    var tokens = example.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    
                    // Build the example by replacing tokens
                    var exampleParts = new List<string>();
                    foreach (var token in tokens)
                    {
                        string componentKey = token.Trim();
                        
                        // Special handling for "base" token (references catalog, not components)
                        if (componentKey.Equals("base", StringComparison.OrdinalIgnoreCase))
                        {
                            if (baseNames.Count > 0)
                            {
                                exampleParts.Add(baseNames[random.Next(baseNames.Count)]);
                            }
                            else
                            {
                                exampleParts.Add("[base]");
                            }
                        }
                        // Check if we have this component group
                        else if (Components.TryGetValue(componentKey, out var componentList) && componentList.Count > 0)
                        {
                            var randomComponent = componentList[random.Next(componentList.Count)];
                            exampleParts.Add(randomComponent.Value);
                        }
                        // For unmatched tokens, wrap in square brackets to show they're placeholders
                        else
                        {
                            exampleParts.Add($"[{componentKey}]");
                        }
                    }
                    
                    var generatedExample = string.Join(" ", exampleParts);
                    
                    // Only add if it's unique
                    if (usedExamples.Add(generatedExample))
                    {
                        examples.Add(generatedExample);
                    }
                }
                
                pattern.GeneratedExamples = string.Join(", ", examples);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to generate examples for pattern: {Template}", pattern.PatternTemplate);
                pattern.GeneratedExamples = "(examples unavailable)";
            }
        }
    }

    /// <summary>
    /// Loads base names from catalog.json in the same directory as the names.json file
    /// </summary>
    private List<string> LoadBaseNamesFromCatalog()
    {
        var baseNames = new List<string>();
        
        try
        {
            // Get the directory of the current names.json file
            string directory = Path.GetDirectoryName(_fileName) ?? string.Empty;
            string catalogPath = Path.Combine(directory, "catalog.json");
            
            if (!File.Exists(catalogPath))
            {
                Log.Debug("No catalog.json found at {Path}, using placeholders", catalogPath);
                return new List<string> { "Wolf", "Bear", "Spider", "Dragon", "Goblin", "Troll" };
            }
            
            string catalogJson = File.ReadAllText(catalogPath);
            var catalog = JObject.Parse(catalogJson);
            
            // Extract names from catalog structure
            // Structure: { "beast_types": { "wolves": { "items": [{ "name": "Wolf" }] } } }
            // Or: { "item_types": { "swords": { "items": [{ "name": "Longsword" }] } } }
            // Or: { "npc_types": { "merchants": { "items": [{ "name": "Merchant" }] } } }
            
            // Try common top-level keys
            var topLevelKeys = new[] { "beast_types", "item_types", "npc_types", "types" };
            
            foreach (var topKey in topLevelKeys)
            {
                if (catalog[topKey] is JObject typesObj)
                {
                    foreach (var categoryProp in typesObj.Properties())
                    {
                        if (categoryProp.Value["items"] is JArray items)
                        {
                            foreach (var item in items)
                            {
                                string? name = item["name"]?.ToString();
                                if (!string.IsNullOrEmpty(name))
                                {
                                    baseNames.Add(name);
                                }
                            }
                        }
                    }
                }
            }
            
            if (baseNames.Count > 0)
            {
                Log.Debug("Loaded {Count} base names from {Path}", baseNames.Count, catalogPath);
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load base names from catalog.json, using placeholders");
            baseNames = new List<string> { "Wolf", "Bear", "Spider", "Dragon", "Goblin", "Troll" };
        }
        
        return baseNames;
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
            // Components (dynamic object with arrays)
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
            // Patterns (simple array)
            var patternsArr = new JArray();
            foreach (var pattern in Patterns)
            {
                patternsArr.Add(JObject.FromObject(pattern));
            }
            root["patterns"] = patternsArr;
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

    /// <summary>
    /// Adds a new pattern
    /// </summary>
    [RelayCommand]
    private void AddPattern()
    {
        var newPattern = new ItemNamePattern
        {
            PatternTemplate = "base",
            Weight = 10,
            Description = "New pattern"
        };
        Patterns.Add(newPattern);
        SelectedPattern = newPattern;
        StatusMessage = "Added new pattern";
    }

    /// <summary>
    /// Removes the selected pattern
    /// </summary>
    [RelayCommand]
    private void RemovePattern()
    {
        if (SelectedPattern != null && Patterns.Contains(SelectedPattern))
        {
            Patterns.Remove(SelectedPattern);
            SelectedPattern = null;
            StatusMessage = "Pattern removed";
        }
    }

    /// <summary>
    /// Adds a new component to a specified group
    /// </summary>
    [RelayCommand]
    private void AddComponent(string? groupName)
    {
        if (string.IsNullOrEmpty(groupName))
        {
            // Create a new group
            groupName = "new_group";
            int counter = 1;
            while (Components.ContainsKey(groupName))
            {
                groupName = $"new_group_{counter++}";
            }
            Components[groupName] = new ObservableCollection<NameComponentBase>();
            ComponentNames.Add(groupName);
        }

        if (Components.TryGetValue(groupName, out var list))
        {
            var newComponent = new ItemNameComponent
            {
                Value = "New Component",
                RarityWeight = 10
            };
            list.Add(newComponent);
            StatusMessage = $"Added component to {groupName}";
        }
    }

    /// <summary>
    /// Removes a component from a group
    /// </summary>
    [RelayCommand]
    private void RemoveComponent(object? parameter)
    {
        if (parameter is NameComponentBase component)
        {
            foreach (var kvp in Components)
            {
                if (kvp.Value.Contains(component))
                {
                    kvp.Value.Remove(component);
                    StatusMessage = $"Component removed from {kvp.Key}";
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Removes an entire component group
    /// </summary>
    [RelayCommand]
    private void RemoveComponentGroup(string? groupName)
    {
        if (!string.IsNullOrEmpty(groupName) && Components.ContainsKey(groupName))
        {
            Components.Remove(groupName);
            ComponentNames.Remove(groupName);
            StatusMessage = $"Removed group: {groupName}";
        }
    }

    /// <summary>
    /// Adds a new component group
    /// </summary>
    [RelayCommand]
    private void AddComponentGroup()
    {
        string groupName = "new_group";
        int counter = 1;
        while (Components.ContainsKey(groupName))
        {
            groupName = $"new_group_{counter++}";
        }
        Components[groupName] = new ObservableCollection<NameComponentBase>();
        ComponentNames.Add(groupName);
        StatusMessage = $"Added group: {groupName}";
    }
}
