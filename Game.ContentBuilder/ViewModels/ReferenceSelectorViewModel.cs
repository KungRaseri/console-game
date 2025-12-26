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
    private string _selectedReferenceType = "material";

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
        // Try multiple path resolution strategies
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var dataPath = Path.Combine(baseDir, "Data", "Json");
        
        // Check if actual catalog files exist, not just the directory
        var testCatalogPath = Path.Combine(dataPath, "items", "materials", "catalog.json");
        
        if (!File.Exists(testCatalogPath))
        {
            MainWindow.AddLog($"Test catalog not found at: {testCatalogPath}");
            // Navigate up from bin/Debug/net9.0-windows to solution root
            var binFolder = baseDir; // e.g., C:\...\Game.ContentBuilder\bin\Debug\net9.0-windows
            var projectFolder = Directory.GetParent(binFolder)?.Parent?.Parent?.FullName; // e.g., C:\...\Game.ContentBuilder
            MainWindow.AddLog($"Project folder: {projectFolder}");
            
            if (projectFolder != null)
            {
                var solutionFolder = Directory.GetParent(projectFolder)?.FullName; // e.g., C:\...\console-game
                MainWindow.AddLog($"Solution folder: {solutionFolder}");
                
                if (solutionFolder != null)
                {
                    var gameDataPath = Path.Combine(solutionFolder, "Game.Data", "Data", "Json");
                    MainWindow.AddLog($"Trying Game.Data path: {gameDataPath}");
                    
                    if (Directory.Exists(gameDataPath))
                    {
                        dataPath = gameDataPath;
                        MainWindow.AddLog($"SUCCESS - Found Game.Data path!");
                    }
                    else
                    {
                        MainWindow.AddLog($"[ERROR] Game.Data path does not exist!");
                    }
                }
            }
        }
        else
        {
            MainWindow.AddLog($"Test catalog found in bin directory");
        }
        
        _dataRootPath = dataPath;
        MainWindow.AddLog($"ReferenceSelectorViewModel initialized with data path: {_dataRootPath}");
        
        // Define available reference types
        ReferenceTypes.Add(new ReferenceTypeOption("material", "Materials", "Diamond", "items/materials/catalog.json"));
        ReferenceTypes.Add(new ReferenceTypeOption("weapon", "Weapons", "Sword", "items/weapons/catalog.json"));
        ReferenceTypes.Add(new ReferenceTypeOption("armor", "Armor", "Shield", "items/armor/catalog.json"));
        ReferenceTypes.Add(new ReferenceTypeOption("consumable", "Consumables", "Potion", "items/consumables/catalog.json"));
        ReferenceTypes.Add(new ReferenceTypeOption("enemy", "Enemies", "Skull", "enemies/*/catalog.json"));
        ReferenceTypes.Add(new ReferenceTypeOption("npc", "NPCs", "Account", "npcs/catalog.json"));
        ReferenceTypes.Add(new ReferenceTypeOption("quest", "Quests", "QuestMarker", "quests/catalog.json"));
        ReferenceTypes.Add(new ReferenceTypeOption("general", "General", "FormatListBulleted", "general/*.json"));

        if (initialReferenceType != null)
        {
            SelectedReferenceType = initialReferenceType;
        }

        LoadCatalog();
    }

    partial void OnSelectedReferenceTypeChanged(string value)
    {
        MainWindow.AddLog($"OnSelectedReferenceTypeChanged: {value}");
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
        MainWindow.AddLog($"LoadCatalog called for type: {SelectedReferenceType}");
        MainWindow.AddLog($"Categories collection before clear: {Categories.Count}");
        Categories.Clear();
        SelectedCategory = null;
        SelectedItem = null;
        MainWindow.AddLog($"Categories cleared. Count: {Categories.Count}");

        try
        {
            var refType = ReferenceTypes.FirstOrDefault(r => r.Key == SelectedReferenceType);
            if (refType == null)
            {
                MainWindow.AddLog($"Reference type not found: {SelectedReferenceType}");
                return;
            }
            MainWindow.AddLog($"Found reference type: {refType.DisplayName}, path: {refType.CatalogPath}");
            

            // Handle special cases first
            if (SelectedReferenceType == "general")
            {
                LoadGeneralWordLists();
                return;
            }

            // Handle wildcards for enemies (multiple category folders)
            if (refType.CatalogPath.Contains("*"))
            {
                LoadEnemyCatalogs();
                return;
            }

            var catalogPath = Path.Combine(_dataRootPath, refType.CatalogPath);
            MainWindow.AddLog($"Looking for catalog at: {catalogPath}");

            if (!File.Exists(catalogPath))
            {
                Log.Warning("Catalog not found: {Path}", catalogPath);
                MainWindow.AddLog($"[ERROR] Catalog file not found: {catalogPath}");
                return;
            }
            MainWindow.AddLog("Catalog file found, reading...");

            var json = File.ReadAllText(catalogPath);
            var catalog = JObject.Parse(json);

            // Parse based on reference type
            if (SelectedReferenceType == "material")
            {
                LoadMaterialsCatalog(catalog);
            }
            else if (SelectedReferenceType == "weapon" || SelectedReferenceType == "armor" || SelectedReferenceType == "consumable")
            {
                LoadEquipmentCatalog(catalog);
            }
            else if (SelectedReferenceType == "npc")
            {
                LoadNpcCatalog(catalog);
            }
            else if (SelectedReferenceType == "quest")
            {
                LoadQuestCatalog(catalog);
            }
            else
            {
                LoadGenericCatalog(catalog);
            }
            
            MainWindow.AddLog($"LoadCatalog complete. Final Categories.Count: {Categories.Count}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load catalog for {Type}", SelectedReferenceType);
            MainWindow.AddLog($"[ERROR] Exception in LoadCatalog: {ex.Message}");
        }
    }

    private void LoadMaterialsCatalog(JObject catalog)
    {
        MainWindow.AddLog("LoadMaterialsCatalog called");
        var materialTypes = catalog["material_types"] as JObject;
        if (materialTypes == null)
        {
            MainWindow.AddLog("[ERROR] material_types not found in catalog");
            return;
        }
        MainWindow.AddLog($"Found {materialTypes.Count} material categories");

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
                MainWindow.AddLog($"Added category: {categoryName} with {categoryNode.Items.Count} items");
            }
        }
        MainWindow.AddLog($"LoadMaterialsCatalog complete. Total categories: {Categories.Count}");
    }

    private void LoadEquipmentCatalog(JObject catalog)
    {
        // Determine the types key based on reference type
        string typesKey = SelectedReferenceType switch
        {
            "weapon" => "weapon_types",
            "armor" => "armor_types",
            "consumable" => "consumable_types",
            _ => SelectedReferenceType + "_types"
        };
        
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

    private void LoadNpcCatalog(JObject catalog)
    {
        var npcTypes = catalog["npc_types"] as JObject;
        if (npcTypes == null) return;

        foreach (var category in npcTypes)
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

    private void LoadQuestCatalog(JObject catalog)
    {
        var questTypes = catalog["quest_types"] as JObject;
        if (questTypes == null) return;

        foreach (var category in questTypes)
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
                    var traits = item["description"]?.ToString() ?? "";
                    categoryNode.Items.Add(new ReferenceItem(name, $"{categoryName}/{name}", traits));
                }
            }

            if (categoryNode.Items.Count > 0)
            {
                Categories.Add(categoryNode);
            }
        }
    }

    private void LoadGeneralWordLists()
    {
        var generalPath = Path.Combine(_dataRootPath, "general");
        if (!Directory.Exists(generalPath))
        {
            Log.Warning("General folder not found: {Path}", generalPath);
            return;
        }

        foreach (var jsonFile in Directory.GetFiles(generalPath, "*.json"))
        {
            try
            {
                var categoryName = Path.GetFileNameWithoutExtension(jsonFile);
                var json = File.ReadAllText(jsonFile);
                var data = JArray.Parse(json);

                var categoryNode = new ReferenceCategory(categoryName, categoryName);

                foreach (var item in data.OfType<JValue>())
                {
                    var value = item.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        categoryNode.Items.Add(new ReferenceItem(value, $"{categoryName}/{value}", ""));
                    }
                }

                if (categoryNode.Items.Count > 0)
                {
                    Categories.Add(categoryNode);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load general word list: {File}", jsonFile);
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
            // Generate reference in format: @materialRef/metals or @itemRef/weapons
            SelectedReference = $"{SelectedReferenceType}Ref/{SelectedCategory.Key}";
        }
        else if (SelectedCategory != null)
        {
            // Just category selected: @materialRef/metals
            SelectedReference = $"{SelectedReferenceType}Ref/{SelectedCategory.Key}";
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
