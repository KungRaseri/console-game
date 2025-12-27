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

    /// <summary>
    /// Text representation of notes for TextBox binding
    /// </summary>
    public string MetadataNotesText
    {
        get => string.Join(Environment.NewLine, MetadataNotes);
        set
        {
            MetadataNotes.Clear();
            if (!string.IsNullOrWhiteSpace(value))
            {
                var lines = value.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    MetadataNotes.Add(line.Trim());
                }
            }
            OnPropertyChanged(nameof(MetadataNotesText));
        }
    }

    // Type Catalogs (weapon_types, armor_types, etc.)
    [ObservableProperty]
    private ObservableCollection<TypeCatalog> _typeCatalogs = new();

    [ObservableProperty]
    private TypeCatalog? _selectedCatalog;

    [ObservableProperty]
    private TypeCategory? _selectedCategory;

    [ObservableProperty]
    private TypeItem? _selectedItem;

    /// <summary>
    /// Returns true when a category is selected but no item is selected
    /// </summary>
    public bool ShowCategoryTraits => SelectedCategory != null && SelectedItem == null;

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    partial void OnSelectedCategoryChanged(TypeCategory? value)
    {
        OnPropertyChanged(nameof(ShowCategoryTraits));
    }

    partial void OnSelectedItemChanged(TypeItem? value)
    {
        OnPropertyChanged(nameof(ShowCategoryTraits));
    }

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

        // Check for flat "items" array structure (new abilities format)
        if (_jsonData["items"] is JArray flatItemsArray)
        {
            var catalog = new TypeCatalog
            {
                Name = "abilities",
                Categories = new ObservableCollection<TypeCategory>()
            };

            var category = new TypeCategory
            {
                Name = "All Abilities",
                Items = new ObservableCollection<TypeItem>(),
                Traits = new ObservableCollection<TraitItem>()
            };

            foreach (var itemToken in flatItemsArray)
            {
                if (itemToken is JObject itemObj)
                {
                    var item = LoadTypeItem(itemObj);
                    category.Items.Add(item);
                }
            }

            catalog.Categories.Add(category);
            TypeCatalogs.Add(catalog);
            return;
        }

        // Handle nested *_types structure (old format)
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
                        Items = new ObservableCollection<TypeItem>(),
                        Traits = new ObservableCollection<TraitItem>()
                    };

                    var categoryObj = categoryProp.Value as JObject;
                    if (categoryObj == null) continue;

                    // Load category-level traits
                    var traitsObj = categoryObj["traits"] as JObject;
                    if (traitsObj != null)
                    {
                        foreach (var traitProp in traitsObj.Properties())
                        {
                            category.Traits.Add(new TraitItem
                            {
                                Key = traitProp.Name,
                                Value = traitProp.Value.ToString()
                            });
                        }
                    }

                    // Load items array
                    var itemsArray = categoryObj["items"] as JArray;
                    if (itemsArray != null)
                    {
                        foreach (var itemToken in itemsArray)
                        {
                            if (itemToken is JObject itemObj)
                            {
                                var item = LoadTypeItem(itemObj);
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

    /// <summary>
    /// Loads a TypeItem from a JObject (extracted for reuse)
    /// </summary>
    private TypeItem LoadTypeItem(JObject itemObj)
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

        // Load traits (handle both object and array formats)
        if (itemObj["traits"] is JArray traitsArray)
        {
            foreach (var trait in traitsArray)
            {
                item.Traits.Add(trait.ToString());
            }
        }
        else if (itemObj["traits"] is JObject traitsObj)
        {
            // New format: traits are objects with value/type
            foreach (var traitProp in traitsObj.Properties())
            {
                var traitValue = traitProp.Value["value"]?.ToString() ?? traitProp.Value.ToString();
                item.Traits.Add($"{traitProp.Name}: {traitValue}");
            }
        }

        return item;
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
    private void AddTrait()
    {
        if (SelectedCategory == null) return;

        var newTrait = new TraitItem
        {
            Key = $"trait_{SelectedCategory.Traits.Count + 1}",
            Value = "value"
        };

        SelectedCategory.Traits.Add(newTrait);
        IsDirty = true;
        StatusMessage = "Added new trait";
    }

    [RelayCommand]
    private void RemoveTrait(string? key)
    {
        if (SelectedCategory == null || key == null) return;

        var trait = SelectedCategory.Traits.FirstOrDefault(t => t.Key == key);
        if (trait != null)
        {
            SelectedCategory.Traits.Remove(trait);
            IsDirty = true;
            StatusMessage = "Removed trait";
        }
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

    [ObservableProperty]
    private ObservableCollection<TraitItem> _traits = new();
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

/// <summary>
/// Represents a trait key-value pair
/// </summary>
public partial class TraitItem : ObservableObject
{
    [ObservableProperty]
    private string _key = string.Empty;

    [ObservableProperty]
    private string _value = string.Empty;
}


