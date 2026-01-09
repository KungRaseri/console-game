using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using Serilog;

namespace RealmEngine.Shared.Services;

/// <summary>
/// Resolves cross-file references in JSON data (e.g., materialRef, itemRef, enemyRef)
/// Thread-safe singleton with caching for performance.
/// </summary>
public class DataReferenceResolver
{
    private static readonly Lazy<DataReferenceResolver> _instance = new(() => new DataReferenceResolver());
    /// <summary>Gets the singleton instance of the DataReferenceResolver.</summary>
    public static DataReferenceResolver Instance => _instance.Value;

    private readonly ConcurrentDictionary<string, JObject> _catalogCache = new();
    private readonly string _dataRoot;

    private DataReferenceResolver()
    {
        // Default data root - can be overridden
        _dataRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Json");
    }

    /// <summary>
    /// Initialize with custom data root path
    /// </summary>
    public void Initialize(string dataRootPath)
    {
        if (!Directory.Exists(dataRootPath))
        {
            throw new DirectoryNotFoundException($"Data root path not found: {dataRootPath}");
        }

        // Clear cache and reinitialize
        _catalogCache.Clear();
        Log.Information("DataReferenceResolver initialized with root: {DataRoot}", dataRootPath);
    }

    /// <summary>
    /// Resolves a material reference and returns combined traits for the specified item type
    /// Format: "materials/category/materialName" (e.g., "materials/metals/Iron")
    /// </summary>
    public Dictionary<string, object>? ResolveMaterial(string materialRef, string itemType)
    {
        if (string.IsNullOrWhiteSpace(materialRef))
            return null;

        try
        {
            // Parse reference: materials/metals/Iron
            var parts = materialRef.Split('/');
            if (parts.Length != 3 || parts[0] != "materials")
            {
                Log.Warning("Invalid material reference format: {MaterialRef}. Expected: materials/category/name", materialRef);
                return null;
            }

            var category = parts[1]; // metals, woods, leathers, gemstones
            var materialName = parts[2]; // Iron, Oak, etc.

            // Load materials catalog
            var catalog = LoadCatalog("items/materials/catalog.json");
            if (catalog == null)
                return null;

            // Navigate: material_types.{category}.items[]
            var categoryNode = catalog["material_types"]?[category] as JObject;
            if (categoryNode == null)
            {
                Log.Warning("Material category not found: {Category} in {MaterialRef}", category, materialRef);
                return null;
            }

            var items = categoryNode["items"] as JArray;
            if (items == null)
                return null;

            // Find material by name
            var material = items
                .OfType<JObject>()
                .FirstOrDefault(m => m["name"]?.ToString() == materialName);

            if (material == null)
            {
                Log.Warning("Material not found: {MaterialName} in category {Category}", materialName, category);
                return null;
            }

            // Combine traits: shared traits + itemTypeTraits[itemType]
            var combinedTraits = new Dictionary<string, object>();

            // Add shared traits
            var sharedTraits = material["traits"] as JObject;
            if (sharedTraits != null)
            {
                foreach (var trait in sharedTraits.Properties())
                {
                    combinedTraits[trait.Name] = ConvertJTokenToObject(trait.Value);
                }
            }

            // Add item-type-specific traits
            var itemTypeTraits = material["itemTypeTraits"]?[itemType] as JObject;
            if (itemTypeTraits != null)
            {
                foreach (var trait in itemTypeTraits.Properties())
                {
                    combinedTraits[trait.Name] = ConvertJTokenToObject(trait.Value);
                }
            }

            Log.Debug("Resolved material {MaterialRef} for {ItemType}: {TraitCount} traits",
                materialRef, itemType, combinedTraits.Count);

            return combinedTraits;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to resolve material reference: {MaterialRef}", materialRef);
            return null;
        }
    }

    /// <summary>
    /// Resolves an item reference from catalog
    /// Format: "domain/category/itemName" (e.g., "weapons/swords/Longsword")
    /// </summary>
    public JObject? ResolveItem(string itemRef)
    {
        if (string.IsNullOrWhiteSpace(itemRef))
            return null;

        try
        {
            // Parse reference: weapons/swords/Longsword
            var parts = itemRef.Split('/');
            if (parts.Length != 3)
            {
                Log.Warning("Invalid item reference format: {ItemRef}. Expected: domain/category/name", itemRef);
                return null;
            }

            var domain = parts[0]; // weapons, armor
            var category = parts[1]; // swords, axes
            var itemName = parts[2]; // Longsword

            // Load catalog
            var catalog = LoadCatalog($"items/{domain}/catalog.json");
            if (catalog == null)
                return null;

            // Navigate: {domain}_types.{category}.items[]
            var categoryNode = catalog[$"{domain}_types"]?[category] as JObject;
            if (categoryNode == null)
            {
                Log.Warning("Item category not found: {Category} in {ItemRef}", category, itemRef);
                return null;
            }

            var items = categoryNode["items"] as JArray;
            if (items == null)
                return null;

            // Find item by name
            var item = items
                .OfType<JObject>()
                .FirstOrDefault(i => i["name"]?.ToString() == itemName);

            if (item == null)
            {
                Log.Warning("Item not found: {ItemName} in category {Category}", itemName, category);
                return null;
            }

            return item;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to resolve item reference: {ItemRef}", itemRef);
            return null;
        }
    }

    /// <summary>
    /// Resolves an enemy reference from catalog
    /// Format: "category/enemyName" (e.g., "demons/Balrog")
    /// </summary>
    public JObject? ResolveEnemy(string enemyRef)
    {
        if (string.IsNullOrWhiteSpace(enemyRef))
            return null;

        try
        {
            // Parse reference: demons/Balrog
            var parts = enemyRef.Split('/');
            if (parts.Length != 2)
            {
                Log.Warning("Invalid enemy reference format: {EnemyRef}. Expected: category/name", enemyRef);
                return null;
            }

            var category = parts[0]; // demons, undead
            var enemyName = parts[1]; // Balrog

            // Load catalog
            var catalog = LoadCatalog($"enemies/{category}/catalog.json");
            if (catalog == null)
                return null;

            // Navigate based on structure - enemies may have different structures
            // Try common patterns
            var items = catalog[$"{category}_types"] as JArray;
            if (items != null)
            {
                var enemy = items
                    .OfType<JObject>()
                    .FirstOrDefault(e => e["name"]?.ToString() == enemyName);

                return enemy;
            }

            Log.Warning("Enemy not found: {EnemyName} in category {Category}", enemyName, category);
            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to resolve enemy reference: {EnemyRef}", enemyRef);
            return null;
        }
    }

    /// <summary>
    /// Generic resolver - attempts to resolve any reference type
    /// </summary>
    public JObject? Resolve(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return null;

        // Determine type from reference format
        if (reference.StartsWith("materials/"))
        {
            // Materials need context (itemType), so return null for generic resolve
            Log.Warning("Material references require itemType context. Use ResolveMaterial() instead.");
            return null;
        }

        // Try to resolve as item or enemy
        var parts = reference.Split('/');
        if (parts.Length == 3)
            return ResolveItem(reference);
        if (parts.Length == 2)
            return ResolveEnemy(reference);

        Log.Warning("Unknown reference format: {Reference}", reference);
        return null;
    }

    /// <summary>
    /// Clears the catalog cache (useful for testing or reloading)
    /// </summary>
    public void ClearCache()
    {
        _catalogCache.Clear();
        Log.Information("DataReferenceResolver cache cleared");
    }

    private JObject? LoadCatalog(string relativePath)
    {
        // Check cache first
        if (_catalogCache.TryGetValue(relativePath, out var cached))
            return cached;

        try
        {
            var fullPath = Path.Combine(_dataRoot, relativePath);
            if (!File.Exists(fullPath))
            {
                Log.Warning("Catalog file not found: {Path}", fullPath);
                return null;
            }

            var json = File.ReadAllText(fullPath);
            var catalog = JObject.Parse(json);

            // Cache for future use
            _catalogCache[relativePath] = catalog;

            Log.Debug("Loaded catalog: {Path}", relativePath);
            return catalog;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load catalog: {Path}", relativePath);
            return null;
        }
    }

    private object ConvertJTokenToObject(JToken token)
    {
        return token.Type switch
        {
            JTokenType.Integer => token.Value<long>(),
            JTokenType.Float => token.Value<double>(),
            JTokenType.Boolean => token.Value<bool>(),
            JTokenType.String => token.Value<string>() ?? string.Empty,
            _ => token.ToString()
        };
    }
}
