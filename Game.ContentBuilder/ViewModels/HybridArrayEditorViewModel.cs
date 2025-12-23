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
/// ViewModel for editing hybrid array JSON files
/// Structure: { "items": [], "components": {}, "patterns": [], "metadata": {} }
/// </summary>
public partial class HybridArrayEditorViewModel : ObservableObject
{
    private readonly JsonEditorService _jsonEditorService;
    private readonly string _fileName;
    private JArray? _itemsData;
    private JObject? _componentsData;

    [ObservableProperty]
    private ObservableCollection<string> _items = new();

    [ObservableProperty]
    private ObservableCollection<ComponentGroup> _componentGroups = new();

    [ObservableProperty]
    private ObservableCollection<PatternComponent> _patterns = new();

    // Track whether this file type has items (some files like names.json don't have items)
    [ObservableProperty]
    private bool _hasItemsArray = true;

    [ObservableProperty]
    private string? _selectedItem;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddComponentCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteComponentGroupCommand))]
    private ComponentGroup? _selectedComponentGroup;

    [ObservableProperty]
    private PatternComponent? _selectedPattern;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddItemCommand))]
    private string _newItemInput = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddPatternCommand))]
    private string _newPatternInput = string.Empty;

    [ObservableProperty]
    private ValidationResult? _newPatternValidation;

    [ObservableProperty]
    private ObservableCollection<string> _liveExamples = new();

    [ObservableProperty]
    private string _newComponentGroupName = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddComponentCommand))]
    private string _newComponentInput = string.Empty;

    [ObservableProperty]
    private string _fileDisplayName = string.Empty;

    [ObservableProperty]
    private int _totalItemsCount;

    [ObservableProperty]
    private int _totalComponentsCount;

    [ObservableProperty]
    private int _totalPatternsCount;

    // Selected tab index (0=Items, 1=Components, 2=Patterns)
    // For pattern_generation files (no items), default to Components tab (index 1)
    [ObservableProperty]
    private int _selectedTabIndex;

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

    [ObservableProperty]
    private string _componentKeysDisplay = string.Empty;

    [ObservableProperty]
    private string _patternTokensDisplay = string.Empty;

    #endregion

    public HybridArrayEditorViewModel(JsonEditorService jsonEditorService, string fileName)
    {
        _jsonEditorService = jsonEditorService;
        _fileName = fileName;
        _fileDisplayName = System.IO.Path.GetFileNameWithoutExtension(fileName);
        
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            var json = System.IO.File.ReadAllText(_jsonEditorService.GetFilePath(_fileName));
            var data = JObject.Parse(json);

            Items.Clear();
            ComponentGroups.Clear();
            Patterns.Clear();

            // Store raw JSON for pattern example generation
            _itemsData = data["items"] as JArray;
            _componentsData = data["components"] as JObject;

            // Determine if this file type has items array
            // Pattern generation files (names.json) don't have items - base token resolves from types.json
            var metadataType = data["metadata"]?["type"]?.ToString() ?? data["_metadata"]?["type"]?.ToString();
            HasItemsArray = metadataType != "pattern_generation" && metadataType != "component_library";

            // Set default tab: Components tab (1) for pattern_generation files, Items tab (0) for others
            SelectedTabIndex = HasItemsArray ? 0 : 1;

            // Load items array (if present)
            if (data["items"] is JArray itemsArray && itemsArray.Count > 0)
            {
                HasItemsArray = true; // Override if items actually exist
                foreach (var item in itemsArray)
                {
                    if (item.Type == JTokenType.String)
                    {
                        Items.Add(item.ToString());
                    }
                    else if (item.Type == JTokenType.Object)
                    {
                        // Handle complex items (objects with properties)
                        Items.Add(item.ToString(Formatting.None));
                    }
                }
            }
            else
            {
                // No items in file - likely a pattern generation file
                HasItemsArray = false;
            }

            // Load components object
            if (data["components"] is JObject componentsObj)
            {
                foreach (var prop in componentsObj.Properties())
                {
                    var componentList = new ObservableCollection<string>();
                    
                    if (prop.Value is JArray componentArray)
                    {
                        foreach (var component in componentArray)
                        {
                            if (component.Type == JTokenType.String)
                            {
                                componentList.Add(component.ToString());
                            }
                            else if (component.Type == JTokenType.Object)
                            {
                                // Handle object components (e.g., {value: "Iron", rarityWeight: 10})
                                var obj = component as JObject;
                                
                                // Try to get the display value
                                if (obj?["value"] != null)
                                {
                                    // Weight-based component: display as "Value (weight: N)"
                                    var value = obj["value"]?.ToString();
                                    var weight = obj["rarityWeight"]?.ToString();
                                    componentList.Add(weight != null ? $"{value} (weight: {weight})" : value ?? "");
                                }
                                else
                                {
                                    // Unknown structure: show as JSON
                                    componentList.Add(component.ToString(Formatting.None));
                                }
                            }
                        }
                    }

                    ComponentGroups.Add(new ComponentGroup
                    {
                        Name = prop.Name,
                        Components = componentList
                    });
                }
            }

            // Load patterns array with examples and validation
            if (data["patterns"] is JArray patternsArray)
            {
                foreach (var pattern in patternsArray)
                {
                    if (pattern.Type == JTokenType.String)
                    {
                        var patternStr = pattern.ToString();
                        var validation = PatternValidator.Validate(patternStr, ComponentGroups);
                        var example = PatternExampleGenerator.GenerateExample(patternStr, _itemsData, _componentsData);
                        Patterns.Add(new PatternComponent(patternStr, example, validation));
                    }
                }
            }

            // Load metadata
            if (data["metadata"] is JObject metadataObj)
            {
                MetadataDescription = metadataObj["description"]?.ToString() ?? string.Empty;
                MetadataVersion = metadataObj["version"]?.ToString() ?? "1.0";
                
                // Handle notes - can be string or array
                if (metadataObj["notes"] is JArray notesArray)
                {
                    // Convert array to multi-line string
                    Notes = string.Join(Environment.NewLine, notesArray.Select(n => n.ToString()));
                }
                else
                {
                    Notes = metadataObj["notes"]?.ToString() ?? string.Empty;
                }
                
                LastUpdated = metadataObj["lastUpdated"]?.ToString() ?? DateTime.Now.ToString("yyyy-MM-dd");
                FileType = metadataObj["type"]?.ToString() ?? string.Empty;
                
                // Display component keys and pattern tokens as comma-separated lists
                ComponentKeysDisplay = metadataObj["componentKeys"] is JArray componentKeys
                    ? string.Join(", ", componentKeys.Select(k => k.ToString()))
                    : string.Empty;
                    
                PatternTokensDisplay = metadataObj["patternTokens"] is JArray patternTokens
                    ? string.Join(", ", patternTokens.Select(t => t.ToString()))
                    : string.Empty;
            }

            UpdateCounts();
            StatusMessage = $"Loaded {FileDisplayName}: {TotalItemsCount} items, {TotalComponentsCount} components, {TotalPatternsCount} patterns";
            Log.Information("Loaded {FileName}: {Items} items, {Components} components, {Patterns} patterns", 
                _fileName, TotalItemsCount, TotalComponentsCount, TotalPatternsCount);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading data: {ex.Message}";
            Log.Error(ex, "Failed to load {FileName}", _fileName);
        }
    }

    private void UpdateCounts()
    {
        TotalItemsCount = Items.Count;
        TotalComponentsCount = ComponentGroups.Sum(g => g.Components.Count);
        TotalPatternsCount = Patterns.Count;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        try
        {
            var data = new JObject();

            // Save items array (only if not empty)
            if (Items.Count > 0)
            {
                var itemsArray = new JArray();
                foreach (var item in Items)
                {
                    // Try to parse as JSON object, otherwise treat as string
                    if (item.TrimStart().StartsWith('{'))
                    {
                        try
                        {
                            itemsArray.Add(JObject.Parse(item));
                        }
                        catch
                        {
                            itemsArray.Add(item);
                        }
                    }
                    else
                    {
                        itemsArray.Add(item);
                    }
                }
                data["items"] = itemsArray;
            }

            // Save components object
            var componentsObj = new JObject();
            foreach (var group in ComponentGroups)
            {
                var componentArray = new JArray();
                foreach (var component in group.Components)
                {
                    // Try to parse as JSON object, otherwise treat as string
                    if (component.TrimStart().StartsWith("{"))
                    {
                        try
                        {
                            componentArray.Add(JObject.Parse(component));
                        }
                        catch
                        {
                            componentArray.Add(component);
                        }
                    }
                    else
                    {
                        componentArray.Add(component);
                    }
                }
                componentsObj[group.Name] = componentArray;
            }
            data["components"] = componentsObj;

            // Save patterns array (extract pattern strings only, not examples)
            var patternsArray = new JArray(Patterns.Select(p => p.Pattern));
            data["patterns"] = patternsArray;

            // Generate metadata using MetadataGenerator
            var metadata = MetadataGenerator.Generate(
                description: MetadataDescription,
                version: MetadataVersion,
                notes: Notes,
                componentGroups: ComponentGroups,
                patterns: Patterns,
                items: Items
            );
            data["metadata"] = metadata;

            // Update read-only display properties from generated metadata
            LastUpdated = metadata["lastUpdated"]?.ToString() ?? DateTime.Now.ToString("yyyy-MM-dd");
            FileType = metadata["type"]?.ToString() ?? string.Empty;
            ComponentKeysDisplay = metadata["componentKeys"] is JArray componentKeys
                ? string.Join(", ", componentKeys.Select(k => k.ToString()))
                : string.Empty;
            PatternTokensDisplay = metadata["patternTokens"] is JArray patternTokens
                ? string.Join(", ", patternTokens.Select(t => t.ToString()))
                : string.Empty;

            var json = data.ToString(Formatting.Indented);
            System.IO.File.WriteAllText(_jsonEditorService.GetFilePath(_fileName), json);

            StatusMessage = $"Saved {FileDisplayName} successfully";
            Log.Information("Saved {FileName}", _fileName);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving data: {ex.Message}";
            Log.Error(ex, "Failed to save {FileName}", _fileName);
        }
    }

    private bool CanSave() => Items.Count > 0 || ComponentGroups.Count > 0 || Patterns.Count > 0;

    #region Items Commands

    [RelayCommand(CanExecute = nameof(CanAddItem))]
    private void AddItem()
    {
        if (!string.IsNullOrWhiteSpace(NewItemInput))
        {
            Items.Add(NewItemInput.Trim());
            NewItemInput = string.Empty;
            UpdateCounts();
            StatusMessage = $"Added item. Total: {Items.Count}";
        }
    }

    private bool CanAddItem() => !string.IsNullOrWhiteSpace(NewItemInput);

    [RelayCommand]
    private void DeleteItem(string? item)
    {
        if (item != null && Items.Contains(item))
        {
            Items.Remove(item);
            if (SelectedItem == item)
                SelectedItem = null;
            UpdateCounts();
            StatusMessage = $"Deleted item. Total: {Items.Count}";
        }
    }

    #endregion

    #region Components Commands

    [RelayCommand(CanExecute = nameof(CanAddComponentGroup))]
    private void AddComponentGroup()
    {
        if (!string.IsNullOrWhiteSpace(NewComponentGroupName))
        {
            var newGroup = new ComponentGroup
            {
                Name = NewComponentGroupName.Trim(),
                Components = new ObservableCollection<string>()
            };
            ComponentGroups.Add(newGroup);
            NewComponentGroupName = string.Empty;
            UpdateCounts();
            StatusMessage = $"Added component group. Total groups: {ComponentGroups.Count}";
        }
    }

    private bool CanAddComponentGroup() => !string.IsNullOrWhiteSpace(NewComponentGroupName);

    [RelayCommand(CanExecute = nameof(CanDeleteComponentGroup))]
    private void DeleteComponentGroup()
    {
        if (SelectedComponentGroup != null)
        {
            ComponentGroups.Remove(SelectedComponentGroup);
            SelectedComponentGroup = null;
            UpdateCounts();
            StatusMessage = $"Deleted component group. Total groups: {ComponentGroups.Count}";
        }
    }

    private bool CanDeleteComponentGroup() => SelectedComponentGroup != null;

    [RelayCommand(CanExecute = nameof(CanAddComponent))]
    private void AddComponent()
    {
        if (SelectedComponentGroup != null && !string.IsNullOrWhiteSpace(NewComponentInput))
        {
            SelectedComponentGroup.Components.Add(NewComponentInput.Trim());
            NewComponentInput = string.Empty;
            UpdateCounts();
            StatusMessage = $"Added component to {SelectedComponentGroup.Name}. Total: {SelectedComponentGroup.Components.Count}";
        }
    }

    private bool CanAddComponent() => SelectedComponentGroup != null && !string.IsNullOrWhiteSpace(NewComponentInput);

    [RelayCommand]
    private void DeleteComponent(string? component)
    {
        if (SelectedComponentGroup != null && component != null && SelectedComponentGroup.Components.Contains(component))
        {
            SelectedComponentGroup.Components.Remove(component);
            UpdateCounts();
            StatusMessage = $"Deleted component from {SelectedComponentGroup.Name}. Total: {SelectedComponentGroup.Components.Count}";
        }
    }

    #endregion

    #region Patterns Commands

    // Real-time validation and live examples as user types
    partial void OnNewPatternInputChanged(string value)
    {
        // Validate pattern
        NewPatternValidation = PatternValidator.Validate(value, ComponentGroups);
        
        // Generate live examples
        GenerateLiveExamples();
    }

    private void GenerateLiveExamples()
    {
        if (string.IsNullOrWhiteSpace(NewPatternInput))
        {
            LiveExamples.Clear();
            return;
        }

        var examples = PatternExampleGenerator.GenerateMultipleExamples(
            NewPatternInput,
            _itemsData,
            _componentsData,
            count: 5
        );

        LiveExamples.Clear();
        foreach (var example in examples)
        {
            LiveExamples.Add(example);
        }
    }

    [RelayCommand]
    private void RefreshExamples()
    {
        GenerateLiveExamples();
    }

    [RelayCommand(CanExecute = nameof(CanAddPattern))]
    private void AddPattern()
    {
        if (!string.IsNullOrWhiteSpace(NewPatternInput))
        {
            var patternStr = NewPatternInput.Trim();
            var validation = PatternValidator.Validate(patternStr, ComponentGroups);
            var example = PatternExampleGenerator.GenerateExample(patternStr, _itemsData, _componentsData);
            Patterns.Add(new PatternComponent(patternStr, example, validation));
            NewPatternInput = string.Empty;
            LiveExamples.Clear(); // Clear examples when pattern is added
            UpdateCounts();
            StatusMessage = $"Added pattern. Total: {Patterns.Count}";
        }
    }

    private bool CanAddPattern() => 
        !string.IsNullOrWhiteSpace(NewPatternInput) &&
        (NewPatternValidation == null || NewPatternValidation.Level != ValidationLevel.Error);

    [RelayCommand]
    private void DeletePattern(PatternComponent? pattern)
    {
        if (pattern != null && Patterns.Contains(pattern))
        {
            Patterns.Remove(pattern);
            if (SelectedPattern == pattern)
                SelectedPattern = null;
            UpdateCounts();
            StatusMessage = $"Deleted pattern. Total: {Patterns.Count}";
        }
    }

    #endregion
}
