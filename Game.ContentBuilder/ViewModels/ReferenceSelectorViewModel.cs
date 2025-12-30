using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Game.ContentBuilder.Services;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Collections.ObjectModel;
using System.IO;

namespace Game.ContentBuilder.ViewModels;

/// <summary>
/// ViewModel for selecting JSON v4.1 references across all domains
/// </summary>
public partial class ReferenceSelectorViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _selectedDomain;

    [ObservableProperty]
    private string? _selectedPath;

    [ObservableProperty]
    private string? _selectedCategory;

    [ObservableProperty]
    private ObservableCollection<DomainOption> _domains = new();

    [ObservableProperty]
    private ObservableCollection<PathOption> _paths = new();

    [ObservableProperty]
    private ObservableCollection<CategoryOption> _categories = new();

    [ObservableProperty]
    private ObservableCollection<ReferenceItem> _items = new();

    [ObservableProperty]
    private ReferenceItem? _selectedItem;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string? _selectedReference;

    [ObservableProperty]
    private string _previewText = string.Empty;

    [ObservableProperty]
    private bool _useWildcard;

    [ObservableProperty]
    private bool _isOptional;

    [ObservableProperty]
    private string? _propertyAccess;

    private readonly ReferenceResolverService _referenceResolver;
    private readonly string _dataRootPath;

    public ReferenceSelectorViewModel(string? initialDomain = null)
    {
        // Try multiple path resolution strategies
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var dataPath = Path.Combine(baseDir, "Data", "Json");

        // Navigate from bin/Debug/net9.0-windows to Game.Data/Data/Json
        if (!Directory.Exists(Path.Combine(dataPath, "items")))
        {
            MainWindow.AddLog($"Navigating from bin folder to find Game.Data");
            var projectFolder = Directory.GetParent(baseDir)?.Parent?.Parent?.FullName;
            if (projectFolder != null)
            {
                var solutionFolder = Directory.GetParent(projectFolder)?.FullName;
                if (solutionFolder != null)
                {
                    var gameDataPath = Path.Combine(solutionFolder, "Game.Data", "Data", "Json");
                    if (Directory.Exists(gameDataPath))
                    {
                        dataPath = gameDataPath;
                        MainWindow.AddLog($"Found Game.Data path: {dataPath}");
                    }
                }
            }
        }

        _dataRootPath = dataPath;
        _referenceResolver = new ReferenceResolverService(_dataRootPath, App.DataCache);

        MainWindow.AddLog($"ReferenceSelectorViewModel initialized with path: {_dataRootPath}");

        LoadDomains();

        if (initialDomain != null)
        {
            SelectedDomain = initialDomain;
        }
    }

    partial void OnSelectedDomainChanged(string? value)
    {
        MainWindow.AddLog($"Domain changed to: {value}");
        LoadPaths();
        UpdateSelectedReference();
    }

    partial void OnSelectedPathChanged(string? value)
    {
        MainWindow.AddLog($"Path changed to: {value}");
        LoadCategories();
        UpdateSelectedReference();
    }

    partial void OnSelectedCategoryChanged(string? value)
    {
        MainWindow.AddLog($"Category changed to: {value}");
        LoadItems();
        UpdateSelectedReference();
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

    partial void OnUseWildcardChanged(bool value)
    {
        UpdateSelectedReference();
    }

    partial void OnIsOptionalChanged(bool value)
    {
        UpdateSelectedReference();
    }

    partial void OnPropertyAccessChanged(string? value)
    {
        UpdateSelectedReference();
    }

    private void LoadDomains()
    {
        Domains.Clear();

        try
        {
            var domains = _referenceResolver.GetAvailableDomains();
            
            foreach (var domain in domains.OrderBy(d => d))
            {
                // Map domain to friendly names and icons
                var (displayName, icon) = GetDomainInfo(domain);
                Domains.Add(new DomainOption(domain, displayName, icon));
            }

            MainWindow.AddLog($"Loaded {Domains.Count} domains");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load domains");
            MainWindow.AddLog($"ERROR loading domains: {ex.Message}");
        }
    }

    private void LoadPaths()
    {
        Paths.Clear();
        Categories.Clear();
        Items.Clear();
        SelectedPath = null;
        SelectedCategory = null;
        SelectedItem = null;

        if (string.IsNullOrEmpty(SelectedDomain)) return;

        try
        {
            var domainPath = Path.Combine(_dataRootPath, SelectedDomain);
            if (!Directory.Exists(domainPath)) return;

            // Get all subdirectories that contain catalog.json
            var pathsWithCatalogs = Directory.GetDirectories(domainPath, "*", SearchOption.AllDirectories)
                .Where(dir => File.Exists(Path.Combine(dir, "catalog.json")))
                .Select(dir => Path.GetRelativePath(domainPath, dir).Replace("\\", "/"))
                .ToList();

            foreach (var path in pathsWithCatalogs.OrderBy(p => p))
            {
                var displayName = FormatDisplayName(path);
                Paths.Add(new PathOption(path, displayName));
            }

            MainWindow.AddLog($"Loaded {Paths.Count} paths for domain: {SelectedDomain}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load paths for domain {Domain}", SelectedDomain);
            MainWindow.AddLog($"ERROR loading paths: {ex.Message}");
        }
    }

    private void LoadCategories()
    {
        Categories.Clear();
        Items.Clear();
        SelectedCategory = null;
        SelectedItem = null;

        if (string.IsNullOrEmpty(SelectedDomain) || string.IsNullOrEmpty(SelectedPath)) return;

        try
        {
            var categories = _referenceResolver.GetAvailableCategories(SelectedDomain, SelectedPath);

            foreach (var category in categories.OrderBy(c => c))
            {
                var displayName = FormatDisplayName(category);
                Categories.Add(new CategoryOption(category, displayName));
            }

            MainWindow.AddLog($"Loaded {Categories.Count} categories for {SelectedDomain}/{SelectedPath}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load categories for {Domain}/{Path}", SelectedDomain, SelectedPath);
            MainWindow.AddLog($"ERROR loading categories: {ex.Message}");
        }
    }

    private void LoadItems()
    {
        Items.Clear();
        SelectedItem = null;

        if (string.IsNullOrEmpty(SelectedDomain) || string.IsNullOrEmpty(SelectedPath) || string.IsNullOrEmpty(SelectedCategory))
            return;

        try
        {
            var references = _referenceResolver.GetAvailableReferences(SelectedDomain, SelectedPath, SelectedCategory);

            foreach (var reference in references)
            {
                // Resolve item to get full data
                var itemData = _referenceResolver.ResolveReference(reference);
                if (itemData != null)
                {
                    var name = itemData["name"]?.ToString() ?? "Unknown";
                    var rarityWeight = itemData["rarityWeight"]?.ToString() ?? "N/A";
                    var description = itemData["description"]?.ToString() ?? "";

                    Items.Add(new ReferenceItem(name, reference, description, rarityWeight));
                }
            }

            MainWindow.AddLog($"Loaded {Items.Count} items for {SelectedDomain}/{SelectedPath}/{SelectedCategory}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load items for {Domain}/{Path}/{Category}", SelectedDomain, SelectedPath, SelectedCategory);
            MainWindow.AddLog($"ERROR loading items: {ex.Message}");
        }
    }

    private void FilterItems()
    {
        // Re-load items and filter
        LoadItems();

        if (string.IsNullOrWhiteSpace(SearchText)) return;

        var searchLower = SearchText.ToLowerInvariant();
        var filtered = Items.Where(i => i.Name.ToLowerInvariant().Contains(searchLower)).ToList();

        Items.Clear();
        foreach (var item in filtered)
        {
            Items.Add(item);
        }

        MainWindow.AddLog($"Filtered to {Items.Count} items matching: {SearchText}");
    }

    private void UpdateSelectedReference()
    {
        if (string.IsNullOrEmpty(SelectedDomain) || string.IsNullOrEmpty(SelectedPath) || string.IsNullOrEmpty(SelectedCategory))
        {
            SelectedReference = null;
            return;
        }

        // Build reference: @domain/path/category:item[filters]?.property
        var reference = $"@{SelectedDomain}/{SelectedPath}/{SelectedCategory}:";

        if (UseWildcard)
        {
            reference += "*";
        }
        else if (SelectedItem != null)
        {
            // Extract item name from reference
            var itemName = SelectedItem.Reference.Split(':').Last();
            reference += itemName;
        }
        else
        {
            SelectedReference = null;
            return;
        }

        // Add optional marker
        if (IsOptional)
        {
            reference += "?";
        }

        // Add property access
        if (!string.IsNullOrWhiteSpace(PropertyAccess))
        {
            if (!PropertyAccess.StartsWith("."))
            {
                reference += ".";
            }
            reference += PropertyAccess;
        }

        SelectedReference = reference;
        MainWindow.AddLog($"Updated reference: {SelectedReference}");
    }

    private void UpdatePreview()
    {
        if (SelectedReference == null)
        {
            PreviewText = string.Empty;
            return;
        }

        try
        {
            var resolved = _referenceResolver.ResolveReference(SelectedReference);
            if (resolved != null)
            {
                PreviewText = $"Reference: {SelectedReference}\n\n{resolved.ToString(Newtonsoft.Json.Formatting.Indented)}";
            }
            else
            {
                PreviewText = $"Reference: {SelectedReference}\n\n(Could not resolve)";
            }
        }
        catch (Exception ex)
        {
            PreviewText = $"Reference: {SelectedReference}\n\nError: {ex.Message}";
        }
    }

    private (string DisplayName, string Icon) GetDomainInfo(string domain)
    {
        return domain switch
        {
            "items" => ("Items", "Sword"),
            "enemies" => ("Enemies", "Skull"),
            "npcs" => ("NPCs", "AccountGroup"),
            "quests" => ("Quests", "BookOpenVariant"),
            "abilities" => ("Abilities", "AutoFix"),
            "classes" => ("Classes", "ShieldAccount"),
            "world" => ("World", "Earth"),
            "social" => ("Social", "AccountGroup"),
            "organizations" => ("Organizations", "OfficeBuildingOutline"),
            "general" => ("General", "FormatListBulleted"),
            _ => (FormatDisplayName(domain), "Folder")
        };
    }

    private string FormatDisplayName(string name)
    {
        // Convert snake_case and kebab-case to Title Case
        var parts = name.Split(new[] { '_', '-', '/' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", parts.Select(p => char.ToUpper(p[0]) + p[1..]));
    }
}

public record DomainOption(string Key, string DisplayName, string Icon);
public record PathOption(string Key, string DisplayName);
public record CategoryOption(string Key, string DisplayName);

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

