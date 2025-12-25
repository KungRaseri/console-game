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

    // Validation and Stats
    [ObservableProperty]
    private ObservableCollection<string> _validationErrors = new();

    [ObservableProperty]
    private bool _hasValidationErrors;

    [ObservableProperty]
    private int _totalComponentCount;

    [ObservableProperty]
    private int _totalPatternCount;

    [ObservableProperty]
    private Dictionary<string, int> _componentCounts = new();

    // Search/Filter
    [ObservableProperty]
    private string _patternSearchText = string.Empty;

    [ObservableProperty]
    private string _componentSearchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<NamePatternBase> _filteredPatterns = new();

    public NameListEditorViewModel(JsonEditorService jsonEditorService, string fileName)
    {
        _jsonEditorService = jsonEditorService;
        _fileName = fileName;
        
        // Watch for search text changes
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(PatternSearchText))
            {
                ApplyPatternFilter();
            }
        };
        
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
                                        TraitsJson = new Dictionary<string, JObject>()
                                    };
                                    
                                    if (obj["traits"] is JObject traitsObj)
                                    {
                                        foreach (var trait in traitsObj)
                                        {
                                            itemComp.TraitsJson[trait.Key] = trait.Value as JObject ?? new JObject();
                                            
                                            // Parse trait into ComponentTrait for editing
                                            var traitData = trait.Value as JObject;
                                            if (traitData != null)
                                            {
                                                itemComp.Traits.Add(new ComponentTrait
                                                {
                                                    Name = trait.Key,
                                                    Value = traitData["value"]?.ToString() ?? string.Empty,
                                                    Type = traitData["type"]?.ToString() ?? "number"
                                                });
                                            }
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
            
            // Always add default {base} pattern first (readonly)
            var basePattern = new ItemNamePattern
            {
                PatternTemplate = "{base}",
                Weight = 100,
                Description = "Default base pattern (readonly)",
                IsReadOnly = true
            };
            Patterns.Add(basePattern);
            ParsePatternIntoTokens(basePattern);
            
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
                            
                            // Skip if this is the default {base} pattern (already added)
                            if (templateStr == "{base}" || templateStr == "base")
                            {
                                continue;
                            }
                            
                            // Remove + symbols if present (legacy format)
                            templateStr = templateStr.Replace(" + ", " ").Replace("+", " ");
                            
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
                            
                            // Skip if this is the default {base} pattern (already added)
                            if (templateStr == "{base}" || templateStr == "base")
                            {
                                continue;
                            }
                            
                            // Remove + symbols if present (legacy format)
                            templateStr = templateStr.Replace(" + ", " ").Replace("+", " ");
                            
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
                            ParsePatternIntoTokens(pattern);
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

            // Update statistics and validation
            UpdateStats();
            ValidateData();
            ApplyPatternFilter();

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
                // Skip readonly patterns (they're auto-generated)
                if (pattern.IsReadOnly)
                    continue;
                    
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
            PatternTemplate = "{base}",
            Weight = 10,
            Description = "New pattern"
        };
        
        // Add the base token
        newPattern.Tokens.Add(new PatternToken
        {
            Value = "base",
            Type = PatternTokenType.Component
        });
        
        Patterns.Add(newPattern);
        SelectedPattern = newPattern;
        StatusMessage = "Added new pattern with {base} token";
    }

    /// <summary>
    /// Removes the selected pattern
    /// </summary>
    [RelayCommand]
    private void RemovePattern()
    {
        if (SelectedPattern != null && Patterns.Contains(SelectedPattern))
        {
            // Prevent deletion of readonly patterns
            if (SelectedPattern.IsReadOnly)
            {
                StatusMessage = "Cannot remove readonly pattern";
                return;
            }
            
            Patterns.Remove(SelectedPattern);
            SelectedPattern = null;
            UpdateStats();
            ValidateData();
            StatusMessage = "Pattern removed";
        }
    }

    /// <summary>
    /// Duplicates a pattern (creates a copy with same tokens and weight)
    /// </summary>
    [RelayCommand]
    private void DuplicatePattern(NamePatternBase? pattern)
    {
        var patternToDuplicate = pattern ?? SelectedPattern;
        if (patternToDuplicate == null)
        {
            StatusMessage = "Select a pattern to duplicate";
            return;
        }

        // Create a copy
        var duplicate = new ItemNamePattern
        {
            PatternTemplate = patternToDuplicate.PatternTemplate,
            Weight = patternToDuplicate.Weight,
            Description = patternToDuplicate.Description
        };

        // Copy tokens
        foreach (var token in patternToDuplicate.Tokens)
        {
            duplicate.Tokens.Add(new PatternToken
            {
                Value = token.Value,
                Type = token.Type
            });
        }

        // Insert after the original
        var index = Patterns.IndexOf(patternToDuplicate);
        if (index >= 0 && index < Patterns.Count - 1)
        {
            Patterns.Insert(index + 1, duplicate);
        }
        else
        {
            Patterns.Add(duplicate);
        }

        SelectedPattern = duplicate;
        UpdateStats();
        StatusMessage = $"Duplicated pattern";
    }

    /// <summary>
    /// Regenerates example names for a specific pattern or all patterns
    /// </summary>
    [RelayCommand]
    private void RegenerateExamples(NamePatternBase? pattern)
    {
        if (pattern != null)
        {
            // Regenerate for single pattern
            GenerateExampleForPattern(pattern);
            StatusMessage = $"Regenerated examples for pattern";
        }
        else
        {
            // Regenerate for all patterns
            GeneratePatternExamples();
            StatusMessage = "Regenerated all pattern examples";
        }
    }

    /// <summary>
    /// Generates examples for a single pattern
    /// </summary>
    private void GenerateExampleForPattern(NamePatternBase pattern)
    {
        try
        {
            var random = new Random();
            var baseNames = LoadBaseNamesFromCatalog();
            var examples = new List<string>();
            var usedExamples = new HashSet<string>();
            
            int maxAttempts = 20;
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
                    string componentKey = token.Trim().Replace("{", "").Replace("}", "");
                    
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
                        exampleParts.Add(randomComponent.Value ?? string.Empty);
                    }
                    // For unmatched tokens, wrap in square brackets to show they're placeholders
                    else if (!string.IsNullOrEmpty(componentKey))
                    {
                        exampleParts.Add($"[{componentKey}]");
                    }
                }
                
                var generatedExample = string.Join(" ", exampleParts);
                
                // Only add if it's unique
                if (!string.IsNullOrEmpty(generatedExample) && usedExamples.Add(generatedExample))
                {
                    examples.Add(generatedExample);
                }
            }
            
            pattern.GeneratedExamples = string.Join(", ", examples);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to generate example for pattern");
            pattern.GeneratedExamples = "(generation failed)";
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

    /// <summary>
    /// Adds a new trait to a component
    /// </summary>
    [RelayCommand]
    private void AddComponentTrait(ItemNameComponent? component)
    {
        if (component == null) return;

        var newTrait = new ComponentTrait
        {
            Name = $"trait_{component.Traits.Count + 1}",
            Value = "1",
            Type = "number"
        };
        component.Traits.Add(newTrait);
        StatusMessage = "Added trait to component";
    }

    /// <summary>
    /// Removes a trait from a component
    /// </summary>
    [RelayCommand]
    private void RemoveComponentTrait(object? parameter)
    {
        if (parameter is not ComponentTrait trait) return;

        foreach (var kvp in Components)
        {
            foreach (var comp in kvp.Value)
            {
                if (comp is ItemNameComponent itemComp && itemComp.Traits.Contains(trait))
                {
                    itemComp.Traits.Remove(trait);
                    StatusMessage = "Removed trait from component";
                    return;
                }
            }
        }
    }

    [RelayCommand]
    private void InsertComponentToken(object? parameter)
    {
        // Parameter can be either:
        // - A tuple (componentKey, pattern) from button with MultiBinding
        // - Just componentKey string (legacy, uses SelectedPattern)
        
        string? componentKey = null;
        NamePatternBase? targetPattern = null;

        if (parameter is Tuple<string, object> tuple)
        {
            componentKey = tuple.Item1;
            targetPattern = tuple.Item2 as NamePatternBase;
        }
        else if (parameter is string key)
        {
            componentKey = key;
            targetPattern = SelectedPattern;
        }

        if (string.IsNullOrEmpty(componentKey) || targetPattern == null)
        {
            StatusMessage = "Select a pattern and component first";
            return;
        }

        // Prevent adding to readonly patterns
        if (targetPattern.IsReadOnly)
        {
            StatusMessage = "Cannot modify readonly base pattern";
            return;
        }

        // Add component token as a badge
        targetPattern.Tokens.Add(new PatternToken
        {
            Value = componentKey,
            Type = PatternTokenType.Component
        });

        // Update the pattern template string
        UpdatePatternTemplateFromTokens(targetPattern);
        StatusMessage = $"Added component: {{{componentKey}}}";
    }

    [RelayCommand]
    private void InsertReferenceToken(object? parameter)
    {
        // Parameter can be either:
        // - A tuple (token, pattern) from button with MultiBinding
        // - Just token string (legacy, uses SelectedPattern)
        
        string? token = null;
        NamePatternBase? targetPattern = null;

        if (parameter is Tuple<string, object> tuple)
        {
            token = tuple.Item1;
            targetPattern = tuple.Item2 as NamePatternBase;
        }
        else if (parameter is string tokenStr)
        {
            token = tokenStr;
            targetPattern = SelectedPattern;
        }

        if (string.IsNullOrEmpty(token) || targetPattern == null)
        {
            StatusMessage = "Select a pattern first";
            return;
        }

        // Prevent adding to readonly patterns
        if (targetPattern.IsReadOnly)
        {
            StatusMessage = "Cannot modify readonly base pattern";
            return;
        }

        // Add reference token as a badge
        targetPattern.Tokens.Add(new PatternToken
        {
            Value = token,
            Type = PatternTokenType.Reference
        });

        // Update the pattern template string
        UpdatePatternTemplateFromTokens(targetPattern);
        StatusMessage = $"Added reference: {token}";
    }

    [RelayCommand]
    private void BrowseReference(object? parameter)
    {
        // Parameter should be the current pattern from the DataTemplate
        var targetPattern = parameter as NamePatternBase ?? SelectedPattern;
        
        if (targetPattern == null)
        {
            StatusMessage = "Select a pattern first to browse references";
            return;
        }

        try
        {
            var dialog = new Views.ReferenceSelectorDialog()
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true && dialog.SelectedReference != null)
            {
                // Add reference token as a badge
                targetPattern.Tokens.Add(new PatternToken
                {
                    Value = $"@{dialog.SelectedReference}",
                    Type = PatternTokenType.Reference
                });
                
                UpdatePatternTemplateFromTokens(targetPattern);
                StatusMessage = $"Added reference: @{dialog.SelectedReference}";
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open reference selector");
            StatusMessage = "Failed to open reference selector";
        }
    }

    /// <summary>
    /// Removes a token badge
    /// </summary>
    [RelayCommand]
    private void RemoveToken(PatternToken? token)
    {
        if (token == null || SelectedPattern == null)
            return;

        // Prevent removing base tokens - they're required
        if (token.IsBase)
        {
            StatusMessage = "Cannot remove {base} token - it's required for all patterns";
            return;
        }

        SelectedPattern.Tokens.Remove(token);
        UpdatePatternTemplateFromTokens();
        StatusMessage = $"Removed token: {token.DisplayText}";
    }

    /// <summary>
    /// Updates the PatternTemplate string from the Tokens collection
    /// </summary>
    public void UpdatePatternTemplateFromTokens(NamePatternBase? pattern = null)
    {
        var targetPattern = pattern ?? SelectedPattern;
        if (targetPattern == null)
            return;

        var parts = targetPattern.Tokens.Select(t => t.DisplayText);
        targetPattern.PatternTemplate = string.Join(" ", parts);
    }

    /// <summary>
    /// Parses a pattern template string into tokens (for loading existing patterns)
    /// </summary>
    private void ParsePatternIntoTokens(NamePatternBase pattern)
    {
        pattern.Tokens.Clear();

        if (string.IsNullOrWhiteSpace(pattern.PatternTemplate))
            return;

        // Parse the template string
        var template = pattern.PatternTemplate;
        var parts = new List<string>();
        var currentPart = "";
        bool inBraces = false;
        bool inReference = false;

        for (int i = 0; i < template.Length; i++)
        {
            char c = template[i];

            if (c == '{')
            {
                if (!string.IsNullOrEmpty(currentPart.Trim()))
                {
                    parts.Add(currentPart.Trim());
                    currentPart = "";
                }
                inBraces = true;
                currentPart = "{";
            }
            else if (c == '}' && inBraces)
            {
                currentPart += c;
                parts.Add(currentPart);
                currentPart = "";
                inBraces = false;
            }
            else if (c == '@')
            {
                if (!string.IsNullOrEmpty(currentPart.Trim()))
                {
                    parts.Add(currentPart.Trim());
                    currentPart = "";
                }
                inReference = true;
                currentPart = "@";
            }
            else if (c == ' ' && !inBraces && inReference)
            {
                if (!string.IsNullOrEmpty(currentPart))
                {
                    parts.Add(currentPart);
                    currentPart = "";
                    inReference = false;
                }
            }
            else if (c == ' ' && !inBraces && !inReference)
            {
                if (!string.IsNullOrEmpty(currentPart.Trim()))
                {
                    parts.Add(currentPart.Trim());
                    currentPart = "";
                }
            }
            else
            {
                currentPart += c;
            }
        }

        // Add remaining part
        if (!string.IsNullOrEmpty(currentPart.Trim()))
        {
            parts.Add(currentPart.Trim());
        }

        // Convert parts to tokens
        foreach (var part in parts)
        {
            if (part.StartsWith("{") && part.EndsWith("}"))
            {
                // Component token
                var componentKey = part.Substring(1, part.Length - 2);
                pattern.Tokens.Add(new PatternToken
                {
                    Value = componentKey,
                    Type = PatternTokenType.Component
                });
            }
            else if (part.StartsWith("@"))
            {
                // Reference token
                pattern.Tokens.Add(new PatternToken
                {
                    Value = part,
                    Type = PatternTokenType.Reference
                });
            }
            else
            {
                // Plain text token
                pattern.Tokens.Add(new PatternToken
                {
                    Value = part,
                    Type = PatternTokenType.PlainText
                });
            }
        }
    }

    /// <summary>
    /// Updates component counts and statistics
    /// </summary>
    private void UpdateStats()
    {
        ComponentCounts.Clear();
        TotalComponentCount = 0;

        foreach (var kvp in Components)
        {
            var count = kvp.Value.Count;
            ComponentCounts[kvp.Key] = count;
            TotalComponentCount += count;
        }

        TotalPatternCount = Patterns.Count;

        // Calculate weight percentages for patterns
        var totalWeight = Patterns.Sum(p => p.Weight);
        if (totalWeight > 0)
        {
            foreach (var pattern in Patterns)
            {
                pattern.WeightPercentage = (double)pattern.Weight / totalWeight * 100.0;
            }
        }
    }

    /// <summary>
    /// Validates data and updates validation errors collection
    /// </summary>
    private void ValidateData()
    {
        ValidationErrors.Clear();

        // Check for empty metadata
        if (string.IsNullOrWhiteSpace(Metadata.Version))
            ValidationErrors.Add("Warning: No version specified in metadata");

        // Check for patterns without {base}
        foreach (var pattern in Patterns)
        {
            if (!pattern.Tokens.Any(t => t.IsBase))
            {
                ValidationErrors.Add($"Error: Pattern '{pattern.PatternTemplate}' missing {{base}} token");
            }

            // Check for invalid weight
            if (pattern.Weight <= 0)
            {
                ValidationErrors.Add($"Warning: Pattern '{pattern.PatternTemplate}' has weight {pattern.Weight}");
            }
        }

        // Check for duplicate component values
        foreach (var kvp in Components)
        {
            var duplicates = kvp.Value
                .GroupBy(c => c.Value?.ToLowerInvariant())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            foreach (var dup in duplicates)
            {
                ValidationErrors.Add($"Warning: Duplicate '{dup}' in {kvp.Key} component");
            }
        }

        // Check for empty components referenced in patterns
        var referencedComponents = Patterns
            .SelectMany(p => p.Tokens)
            .Where(t => t.Type == PatternTokenType.Component)
            .Select(t => t.Value)
            .Distinct()
            .ToList();

        foreach (var comp in referencedComponents)
        {
            if (!Components.ContainsKey(comp) || Components[comp].Count == 0)
            {
                ValidationErrors.Add($"Error: Pattern references empty component '{comp}'");
            }
        }

        HasValidationErrors = ValidationErrors.Any();
    }

    /// <summary>
    /// Applies search filter to patterns
    /// </summary>
    private void ApplyPatternFilter()
    {
        FilteredPatterns.Clear();

        if (string.IsNullOrWhiteSpace(PatternSearchText))
        {
            // No filter - show all
            foreach (var pattern in Patterns)
            {
                FilteredPatterns.Add(pattern);
            }
        }
        else
        {
            var searchLower = PatternSearchText.ToLowerInvariant();
            foreach (var pattern in Patterns)
            {
                // Search in template, description, and examples
                bool matches = pattern.PatternTemplate.ToLowerInvariant().Contains(searchLower) ||
                               (pattern.Description?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                               (pattern.GeneratedExamples?.ToLowerInvariant().Contains(searchLower) ?? false);

                if (matches)
                {
                    FilteredPatterns.Add(pattern);
                }
            }
        }
    }

    /// <summary>
    /// Clears the pattern search filter
    /// </summary>
    [RelayCommand]
    private void ClearPatternFilter()
    {
        PatternSearchText = string.Empty;
    }
}
