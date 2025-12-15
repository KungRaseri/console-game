using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

    [ObservableProperty]
    private ObservableCollection<string> _items = new();

    [ObservableProperty]
    private ObservableCollection<ComponentGroup> _componentGroups = new();

    [ObservableProperty]
    private ObservableCollection<string> _patterns = new();

    [ObservableProperty]
    private string? _selectedItem;

    [ObservableProperty]
    private ComponentGroup? _selectedComponentGroup;

    [ObservableProperty]
    private string? _selectedPattern;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _newItemInput = string.Empty;

    [ObservableProperty]
    private string _newPatternInput = string.Empty;

    [ObservableProperty]
    private string _newComponentGroupName = string.Empty;

    [ObservableProperty]
    private string _newComponentInput = string.Empty;

    [ObservableProperty]
    private string _fileDisplayName = string.Empty;

    [ObservableProperty]
    private int _totalItemsCount;

    [ObservableProperty]
    private int _totalComponentsCount;

    [ObservableProperty]
    private int _totalPatternsCount;

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

            // Load items array
            if (data["items"] is JArray itemsArray)
            {
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
                                componentList.Add(component.ToString(Formatting.None));
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

            // Load patterns array
            if (data["patterns"] is JArray patternsArray)
            {
                foreach (var pattern in patternsArray)
                {
                    if (pattern.Type == JTokenType.String)
                    {
                        Patterns.Add(pattern.ToString());
                    }
                }
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

            // Save items array
            var itemsArray = new JArray();
            foreach (var item in Items)
            {
                // Try to parse as JSON object, otherwise treat as string
                if (item.TrimStart().StartsWith("{"))
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

            // Save patterns array
            var patternsArray = new JArray(Patterns);
            data["patterns"] = patternsArray;

            // Save metadata (preserve if exists)
            var existingJson = System.IO.File.ReadAllText(_jsonEditorService.GetFilePath(_fileName));
            var existingData = JObject.Parse(existingJson);
            if (existingData["metadata"] != null)
            {
                data["metadata"] = existingData["metadata"];
            }

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

    [RelayCommand(CanExecute = nameof(CanDeleteItem))]
    private void DeleteItem()
    {
        if (SelectedItem != null)
        {
            Items.Remove(SelectedItem);
            SelectedItem = null;
            UpdateCounts();
            StatusMessage = $"Deleted item. Total: {Items.Count}";
        }
    }

    private bool CanDeleteItem() => SelectedItem != null;

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

    #endregion

    #region Patterns Commands

    [RelayCommand(CanExecute = nameof(CanAddPattern))]
    private void AddPattern()
    {
        if (!string.IsNullOrWhiteSpace(NewPatternInput))
        {
            Patterns.Add(NewPatternInput.Trim());
            NewPatternInput = string.Empty;
            UpdateCounts();
            StatusMessage = $"Added pattern. Total: {Patterns.Count}";
        }
    }

    private bool CanAddPattern() => !string.IsNullOrWhiteSpace(NewPatternInput);

    [RelayCommand(CanExecute = nameof(CanDeletePattern))]
    private void DeletePattern()
    {
        if (SelectedPattern != null)
        {
            Patterns.Remove(SelectedPattern);
            SelectedPattern = null;
            UpdateCounts();
            StatusMessage = $"Deleted pattern. Total: {Patterns.Count}";
        }
    }

    private bool CanDeletePattern() => SelectedPattern != null;

    #endregion
}

/// <summary>
/// Model for a component group (e.g., "base_colors", "modifiers")
/// </summary>
public partial class ComponentGroup : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _components = new();
}
