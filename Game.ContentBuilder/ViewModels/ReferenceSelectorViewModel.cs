using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Collections.ObjectModel;
using System.IO;

namespace Game.ContentBuilder.ViewModels;

public partial class ReferenceSelectorViewModel : ObservableObject
{
    [ObservableProperty]
    private string _selectedReferenceType = "materials";

    [ObservableProperty]
    private ObservableCollection<ReferenceTypeOption> _referenceTypes = new();

    [ObservableProperty]
    private ObservableCollection<ReferenceCategory> _categories = new();

    [ObservableProperty]
    private ReferenceCategory? _selectedCategory;

    [ObservableProperty]
    private ReferenceItem? _selectedItem;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string? _selectedReference;

    [ObservableProperty]
    private string _previewText = string.Empty;

    private readonly string _dataRootPath;

    public ReferenceSelectorViewModel(string? initialReferenceType = null)
    {
        _dataRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Json");
        
        // Define available reference types
        ReferenceTypes.Add(new ReferenceTypeOption("materials", "Materials", "Diamond", "materials/catalog.json"));
        ReferenceTypes.Add(new ReferenceTypeOption("weapons", "Weapons", "Sword", "weapons/catalog.json"));
        ReferenceTypes.Add(new ReferenceTypeOption("armor", "Armor", "Shield", "armor/catalog.json"));
        ReferenceTypes.Add(new ReferenceTypeOption("enemies", "Enemies", "Skull", "enemies/*/catalog.json"));
        ReferenceTypes.Add(new ReferenceTypeOption("npcs", "NPCs", "Account", "npcs/catalog.json"));
        ReferenceTypes.Add(new ReferenceTypeOption("items", "Items", "Package", "items/catalog.json"));

        if (initialReferenceType != null)
        {
            SelectedReferenceType = initialReferenceType;
        }

        LoadCatalog();
    }

    partial void OnSelectedReferenceTypeChanged(string value)
    {
        LoadCatalog();
    }

    partial void OnSelectedCategoryChanged(ReferenceCategory? value)
    {
        UpdateSelectedReference();
        UpdatePreview();
    }

    partial void OnSelectedItemChanged(ReferenceItem? value)
    {
        UpdateSelectedReference();
        UpdatePreview();
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterItems();
    }

    private void LoadCatalog()
    {
        Categories.Clear();
        SelectedCategory = null;
        SelectedItem = null;

        try
        {
            var refType = ReferenceTypes.FirstOrDefault(r => r.Key == SelectedReferenceType);
            if (refType == null) return;

            var catalogPath = Path.Combine(_dataRootPath, refType.CatalogPath);

            // Handle wildcards for enemies (multiple category folders)
            if (refType.CatalogPath.Contains("*"))
            {
                LoadEnemyCatalogs();
                return;
            }

            if (!File.Exists(catalogPath))
            {
                Log.Warning("Catalog not found: {Path}", catalogPath);
                return;
            }

            var json = File.ReadAllText(catalogPath);
            var catalog = JObject.Parse(json);

            // Parse based on reference type
            if (SelectedReferenceType == "materials")
            {
                LoadMaterialsCatalog(catalog);
            }
            else if (SelectedReferenceType == "weapons" || SelectedReferenceType == "armor")
            {
                LoadEquipmentCatalog(catalog);
            }
            else
            {
                LoadGenericCatalog(catalog);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load catalog for {Type}", SelectedReferenceType);
        }
    }

    private void LoadMaterialsCatalog(JObject catalog)
    {
        var materialTypes = catalog["material_types"] as JObject;
        if (materialTypes == null) return;

        foreach (var category in materialTypes)
        {
            var categoryName = category.Key;
            var categoryObj = category.Value as JObject;
            if (categoryObj == null) continue;

            var items = categoryObj["items"] as JArray;
            if (items == null) continue;

            var categoryNode = new ReferenceCategory(categoryName, categoryName);

            foreach (var item in items.OfType<JObject>())
            {
                var name = item["name"]?.ToString();
                if (name != null)
                {
                    var traits = ExtractTraits(item);
                    categoryNode.Items.Add(new ReferenceItem(name, $"{categoryName}/{name}", traits));
                }
            }

            if (categoryNode.Items.Count > 0)
            {
                Categories.Add(categoryNode);
            }
        }
    }

    private void LoadEquipmentCatalog(JObject catalog)
    {
        var typesKey = SelectedReferenceType == "weapons" ? "weapon_types" : "armor_types";
        var types = catalog[typesKey] as JObject;
        if (types == null) return;

        foreach (var category in types)
        {
            var categoryName = category.Key;
            var categoryObj = category.Value as JObject;
            if (categoryObj == null) continue;

            var items = categoryObj["items"] as JArray;
            if (items == null) continue;

            var categoryNode = new ReferenceCategory(categoryName, categoryName);

            foreach (var item in items.OfType<JObject>())
            {
                var name = item["name"]?.ToString();
                var weight = item["rarityWeight"]?.ToString();
                if (name != null)
                {
                    var traits = ExtractTraits(item);
                    categoryNode.Items.Add(new ReferenceItem(name, $"{categoryName}/{name}", traits, weight));
                }
            }

            if (categoryNode.Items.Count > 0)
            {
                Categories.Add(categoryNode);
            }
        }
    }

    private void LoadEnemyCatalogs()
    {
        var enemiesPath = Path.Combine(_dataRootPath, "enemies");
        if (!Directory.Exists(enemiesPath)) return;

        foreach (var categoryDir in Directory.GetDirectories(enemiesPath))
        {
            var catalogPath = Path.Combine(categoryDir, "catalog.json");
            if (!File.Exists(catalogPath)) continue;

            var categoryName = Path.GetFileName(categoryDir);
            var json = File.ReadAllText(catalogPath);
            var catalog = JObject.Parse(json);

            var categoryNode = new ReferenceCategory(categoryName, categoryName);

            // Enemy catalogs may have different structures - adapt as needed
            var enemyTypes = catalog[$"{categoryName}_types"] as JArray;
            if (enemyTypes != null)
            {
                foreach (var enemy in enemyTypes.OfType<JObject>())
                {
                    var name = enemy["name"]?.ToString();
                    if (name != null)
                    {
                        var traits = ExtractTraits(enemy);
                        categoryNode.Items.Add(new ReferenceItem(name, $"{categoryName}/{name}", traits));
                    }
                }
            }

            if (categoryNode.Items.Count > 0)
            {
                Categories.Add(categoryNode);
            }
        }
    }

    private void LoadGenericCatalog(JObject catalog)
    {
        // Generic loader for future catalog types
        foreach (var property in catalog.Properties())
        {
            if (property.Name.EndsWith("_types"))
            {
                var types = property.Value as JObject;
                if (types == null) continue;

                foreach (var category in types)
                {
                    var categoryName = category.Key;
                    var items = category.Value as JArray;
                    if (items == null) continue;

                    var categoryNode = new ReferenceCategory(categoryName, categoryName);

                    foreach (var item in items.OfType<JObject>())
                    {
                        var name = item["name"]?.ToString();
                        if (name != null)
                        {
                            var traits = ExtractTraits(item);
                            categoryNode.Items.Add(new ReferenceItem(name, $"{categoryName}/{name}", traits));
                        }
                    }

                    if (categoryNode.Items.Count > 0)
                    {
                        Categories.Add(categoryNode);
                    }
                }
            }
        }
    }

    private string ExtractTraits(JObject item)
    {
        var traits = item["traits"] as JObject;
        if (traits == null || traits.Count == 0) return string.Empty;

        var traitStrings = traits.Properties()
            .Select(p => $"{p.Name}: {p.Value}")
            .Take(5);

        return string.Join(", ", traitStrings);
    }

    private void FilterItems()
    {
        // Re-load catalog and filter
        LoadCatalog();

        if (string.IsNullOrWhiteSpace(SearchText)) return;

        var searchLower = SearchText.ToLowerInvariant();

        foreach (var category in Categories.ToList())
        {
            var filteredItems = category.Items
                .Where(i => i.Name.ToLowerInvariant().Contains(searchLower))
                .ToList();

            if (filteredItems.Count == 0)
            {
                Categories.Remove(category);
            }
            else
            {
                category.Items.Clear();
                foreach (var item in filteredItems)
                {
                    category.Items.Add(item);
                }
            }
        }
    }

    private void UpdateSelectedReference()
    {
        if (SelectedItem != null && SelectedCategory != null)
        {
            SelectedReference = $"{SelectedReferenceType}/{SelectedCategory.Key}/{SelectedItem.Name}";
        }
        else if (SelectedCategory != null)
        {
            SelectedReference = null;
        }
        else
        {
            SelectedReference = null;
        }
    }

    private void UpdatePreview()
    {
        if (SelectedItem != null)
        {
            PreviewText = $"Reference: {SelectedReference}\n\nTraits: {SelectedItem.Traits}";
        }
        else
        {
            PreviewText = string.Empty;
        }
    }
}

public record ReferenceTypeOption(string Key, string DisplayName, string Icon, string CatalogPath);

public class ReferenceCategory : ObservableObject
{
    public string Key { get; }
    public string DisplayName { get; }
    public ObservableCollection<ReferenceItem> Items { get; } = new();

    public ReferenceCategory(string key, string displayName)
    {
        Key = key;
        DisplayName = displayName;
    }
}

public class ReferenceItem
{
    public string Name { get; }
    public string Reference { get; }
    public string Traits { get; }
    public string? Weight { get; }

    public ReferenceItem(string name, string reference, string traits, string? weight = null)
    {
        Name = name;
        Reference = reference;
        Traits = traits;
        Weight = weight;
    }
}
