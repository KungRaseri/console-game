using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Game.ContentBuilder.Services;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Game.ContentBuilder.ViewModels;

/// <summary>
/// ViewModel for editing catalog.json files (item catalogs)
/// </summary>
public partial class CatalogEditorViewModel : ObservableObject
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

    // Type Catalogs (weapon_types, armor_types, etc.)
    [ObservableProperty]
    private ObservableCollection<TypeCatalog> _typeCatalogs = new();

    [ObservableProperty]
    private TypeCatalog? _selectedCatalog;

    [ObservableProperty]
    private TypeCategory? _selectedCategory;

    [ObservableProperty]
    private TypeItem? _selectedItem;

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public CatalogEditorViewModel(JsonEditorService jsonEditorService, string fileName)
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
            var filePath = _jsonEditorService.GetFilePath(_storedFileName);
            var json = File.ReadAllText(filePath);
            _jsonData = JObject.Parse(json);

            LoadMetadata();
            LoadTypeCatalogs();

            IsDirty = false;
            StatusMessage = $"Loaded {FileName}";
            Log.Information("Loaded catalog.json file: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            var filePath = _jsonEditorService.GetFilePath(_storedFileName);
            Log.Error(ex, "Failed to load catalog.json file: {FilePath}", filePath);
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

    private void LoadTypeCatalogs()
    {
        if (_jsonData == null) return;

        TypeCatalogs.Clear();

        foreach (var prop in _jsonData.Properties())
        {
            // Skip metadata
            if (prop.Name == "metadata") continue;

            // Look for *_types properties
            if (prop.Name.EndsWith("_types") && prop.Value is JObject catalogObj)
            {
                var catalog = new TypeCatalog
                {
                    Name = prop.Name,
                    Categories = new ObservableCollection<TypeCategory>()
                };

                foreach (var categoryProp in catalogObj.Properties())
                {
                    var category = new TypeCategory
                    {
                        Name = categoryProp.Name,
                        Items = new ObservableCollection<TypeItem>()
                    };

                    if (categoryProp.Value is JArray itemsArray)
                    {
                        foreach (var itemToken in itemsArray)
                        {
                            if (itemToken is JObject itemObj)
                            {
                                var item = new TypeItem
                                {
                                    Name = itemObj["name"]?.ToString() ?? string.Empty,
                                    RarityWeight = itemObj["rarityWeight"]?.ToObject<int>() ?? 1,
                                    Properties = new ObservableCollection<PropertyItem>()
                                };

                                // Load all other properties dynamically
                                foreach (var itemProp in itemObj.Properties())
                                {
                                    if (itemProp.Name == "name" || itemProp.Name == "rarityWeight" || itemProp.Name == "traits")
                                        continue;

                                    item.Properties.Add(new PropertyItem
                                    {
                                        Key = itemProp.Name,
                                        Value = itemProp.Value.ToString()
                                    });
                                }

                                // Load traits
                                var traits = itemObj["traits"] as JArray;
                                if (traits != null)
                                {
                                    foreach (var trait in traits)
                                    {
                                        item.Traits.Add(trait.ToString());
                                    }
                                }

                                category.Items.Add(item);
                            }
                        }
                    }

                    catalog.Categories.Add(category);
                }

                TypeCatalogs.Add(catalog);
            }
        }
    }

    [RelayCommand]
    private void AddCategory()
    {
        if (SelectedCatalog == null) return;

        var newCategory = new TypeCategory
        {
            Name = "new_category",
            Items = new ObservableCollection<TypeItem>()
        };

        SelectedCatalog.Categories.Add(newCategory);
        SelectedCategory = newCategory;
        IsDirty = true;
        StatusMessage = "Added new category";
    }

    [RelayCommand]
    private void RemoveCategory()
    {
        if (SelectedCatalog == null || SelectedCategory == null) return;

        SelectedCatalog.Categories.Remove(SelectedCategory);
        SelectedCategory = null;
        IsDirty = true;
        StatusMessage = "Removed category";
    }

    [RelayCommand]
    private void AddItem()
    {
        if (SelectedCategory == null) return;

        var newItem = new TypeItem
        {
            Name = "New Item",
            RarityWeight = 1
        };

        SelectedCategory.Items.Add(newItem);
        SelectedItem = newItem;
        IsDirty = true;
        StatusMessage = "Added new item";
    }

    [RelayCommand]
    private void RemoveItem()
    {
        if (SelectedCategory == null || SelectedItem == null) return;

        SelectedCategory.Items.Remove(SelectedItem);
        SelectedItem = null;
        IsDirty = true;
        StatusMessage = "Removed item";
    }

    [RelayCommand]
    private void AddProperty()
    {
        if (SelectedItem == null) return;

        var newProperty = new PropertyItem
        {
            Key = "new_property",
            Value = "value"
        };

        SelectedItem.Properties.Add(newProperty);
        IsDirty = true;
        StatusMessage = "Added new property";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_jsonData == null) return;

        try
        {
            var filePath = _jsonEditorService.GetFilePath(_storedFileName);
            
            // Rebuild type catalogs
            foreach (var catalog in TypeCatalogs)
            {
                var catalogObj = new JObject();

                foreach (var category in catalog.Categories)
                {
                    var itemsArray = new JArray();

                    foreach (var item in category.Items)
                    {
                        var itemObj = new JObject
                        {
                            ["name"] = item.Name,
                            ["rarityWeight"] = item.RarityWeight
                        };

                        // Add dynamic properties
                        foreach (var prop in item.Properties)
                        {
                            itemObj[prop.Key] = prop.Value;
                        }

                        // Add traits if present
                        if (item.Traits.Count > 0)
                        {
                            itemObj["traits"] = new JArray(item.Traits);
                        }

                        itemsArray.Add(itemObj);
                    }

                    catalogObj[category.Name] = itemsArray;
                }

                _jsonData[catalog.Name] = catalogObj;
            }

            // Write to file with formatting
            await File.WriteAllTextAsync(filePath, _jsonData.ToString(Newtonsoft.Json.Formatting.Indented));

            IsDirty = false;
            StatusMessage = $"Saved {FileName}";
            Log.Information("Saved catalog.json file: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            var filePath = _jsonEditorService.GetFilePath(_storedFileName);
            Log.Error(ex, "Failed to save catalog.json file: {FilePath}", filePath);
            StatusMessage = $"Error saving file: {ex.Message}";
        }
    }
}

/// <summary>
/// Represents a type catalog (e.g., weapon_types, armor_types)
/// </summary>
public partial class TypeCatalog : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private ObservableCollection<TypeCategory> _categories = new();
}

/// <summary>
/// Represents a category within a type catalog (e.g., swords, axes)
/// </summary>
public partial class TypeCategory : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private ObservableCollection<TypeItem> _items = new();
}

/// <summary>
/// Represents a specific item type
/// </summary>
public partial class TypeItem : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private int _rarityWeight = 1;

    [ObservableProperty]
    private ObservableCollection<PropertyItem> _properties = new();

    [ObservableProperty]
    private ObservableCollection<string> _traits = new();
}

/// <summary>
/// Represents a dynamic property of an item
/// </summary>
public partial class PropertyItem : ObservableObject
{
    [ObservableProperty]
    private string _key = string.Empty;

    [ObservableProperty]
    private string _value = string.Empty;
}


