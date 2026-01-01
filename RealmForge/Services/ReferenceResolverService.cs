using Newtonsoft.Json.Linq;
using Serilog;
using System.IO;
using System.Text.RegularExpressions;
using Game.Data.Services;

namespace Game.ContentBuilder.Services;

/// <summary>
/// Service for resolving JSON v4.1 references
/// Reference format: @domain/path/category:item-name[filters]?.property.nested
/// </summary>
public class ReferenceResolverService
{
    private readonly string _dataRootPath;
    private readonly GameDataCache? _dataCache;
    private readonly Dictionary<string, JObject> _catalogCache;
    private static readonly Regex ReferencePattern = new(@"^@(?<domain>[\w-]+)/(?<path>[\w-]+(/[\w-]+)*):(?<item>[\w-*\s]+)(?<optional>\?)?(?<property>\.[\w.]+)?$", RegexOptions.Compiled);

    public ReferenceResolverService(string dataRootPath, GameDataCache? dataCache = null)
    {
        _dataRootPath = dataRootPath;
        _dataCache = dataCache;
        _catalogCache = new Dictionary<string, JObject>();
        Log.Information("ReferenceResolverService initialized with path: {Path} (cache: {HasCache})", dataRootPath, dataCache != null);
    }

    /// <summary>
    /// Validates reference syntax
    /// </summary>
    public bool IsValidReference(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return false;

        return ReferencePattern.IsMatch(reference);
    }

    /// <summary>
    /// Parses a reference string into components
    /// </summary>
    public ReferenceComponents? ParseReference(string reference)
    {
        var match = ReferencePattern.Match(reference);
        if (!match.Success)
            return null;

        var fullPath = match.Groups["path"].Value;
        
        // Split path into path and category
        // Format can be either:
        // - domain/category:item (e.g., @abilities/passive:weapon-mastery)
        // - domain/path/category:item (e.g., @items/weapons/swords:Longsword)
        // - domain/path/subpath/category:item (e.g., @abilities/active/offensive:basic-attack)
        
        string path;
        string category;
        
        var pathParts = fullPath.Split('/');
        if (pathParts.Length == 1)
        {
            // Single segment: this IS the category, path is empty
            path = "";
            category = pathParts[0];
        }
        else
        {
            // Multiple segments: last segment is category, rest is path
            category = pathParts[^1];
            path = string.Join("/", pathParts[..^1]);
        }

        return new ReferenceComponents
        {
            Domain = match.Groups["domain"].Value,
            Path = path,
            Category = category,
            ItemName = match.Groups["item"].Value,
            IsOptional = match.Groups["optional"].Success,
            Property = match.Groups["property"].Success ? match.Groups["property"].Value.TrimStart('.') : null,
            IsWildcard = match.Groups["item"].Value == "*"
        };
    }

    /// <summary>
    /// Resolves a reference to its JSON data
    /// </summary>
    public JToken? ResolveReference(string reference)
    {
        var components = ParseReference(reference);
        if (components == null)
        {
            Log.Warning("Invalid reference syntax: {Reference}", reference);
            return null;
        }

        try
        {
            // Try to load catalog with progressive path resolution
            var (catalog, actualCategory) = LoadCatalogWithCategory(components.Domain, components.Path, components.Category);
            
            if (catalog == null)
            {
                if (components.IsOptional)
                    return null;
                
                Log.Warning("Catalog not found for reference: {Reference}", reference);
                return null;
            }

            // Handle wildcard selection
            if (components.IsWildcard)
            {
                return SelectRandomItemFromCategory(catalog, actualCategory);
            }

            // Find specific item
            var item = FindItemInCatalog(catalog, actualCategory, components.ItemName);
            
            if (item == null)
            {
                if (components.IsOptional)
                    return null;
                
                Log.Warning("Item not found: {ItemName} in {Category}", components.ItemName, actualCategory);
                return null;
            }

            // Apply property access if specified
            if (!string.IsNullOrEmpty(components.Property))
            {
                return GetNestedProperty(item, components.Property);
            }

            return item;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error resolving reference: {Reference}", reference);
            if (components.IsOptional)
                return null;
            throw;
        }
    }

    /// <summary>
    /// Gets all available references for a domain/category
    /// </summary>
    public List<string> GetAvailableReferences(string domain, string path, string category)
    {
        var references = new List<string>();
        
        try
        {
            // Normalize path: treat "." as empty string
            if (path == ".")
                path = "";
            
            var catalog = LoadCatalog(domain, path);
            if (catalog == null)
                return references;

            // Find items in the category
            var items = FindCategoryItems(catalog, category);

            if (items == null)
                return references;

            // Build reference strings
            var pathPart = string.IsNullOrEmpty(path) ? "" : $"{path}/";
            foreach (var item in items)
            {
                var name = item["name"]?.ToString();
                if (!string.IsNullOrEmpty(name))
                {
                    references.Add($"@{domain}/{pathPart}{category}:{name}");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting available references for {Domain}/{Path}/{Category}", domain, path, category);
        }

        return references;
    }

    /// <summary>
    /// Gets all domains from the data directory
    /// </summary>
    public List<string> GetAvailableDomains()
    {
        try
        {
            return Directory.GetDirectories(_dataRootPath)
                .Select(d => Path.GetFileName(d))
                .Where(d => !string.IsNullOrEmpty(d))
                .ToList()!;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting available domains");
            return new List<string>();
        }
    }

    /// <summary>
    /// Gets all categories for a domain/path
    /// </summary>
    public List<string> GetAvailableCategories(string domain, string path)
    {
        try
        {
            // Normalize path: treat "." as empty string
            if (path == ".")
                path = "";
            
            var catalog = LoadCatalog(domain, path);
            if (catalog == null)
                return new List<string>();

            // Look for *_types structures (ability_types, class_types, enemy_types, etc.)
            var categories = new List<string>();
            foreach (var prop in catalog.Properties())
            {
                if (prop.Name != "metadata" && prop.Name.EndsWith("_types") && prop.Value is JObject types)
                {
                    categories.AddRange(types.Properties().Select(p => p.Name));
                }
            }

            // If we found categories from _types, return them
            if (categories.Any())
                return categories;

            // Fallback: if there's a root items array, use special marker "items"
            if (catalog["items"] is JArray)
            {
                return new List<string> { "items" };
            }

            return new List<string>();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting available categories for {Domain}/{Path}", domain, path);
            return new List<string>();
        }
    }

    /// <summary>
    /// Validates that all references in a catalog are valid
    /// </summary>
    public List<ValidationError> ValidateCatalogReferences(string catalogPath)
    {
        var errors = new List<ValidationError>();

        try
        {
            var startTime = DateTime.Now;
            string? content = null;

            // Try cache first
            if (_dataCache != null)
            {
                var relativePath = Path.GetRelativePath(_dataRootPath, catalogPath);
                var cachedFile = _dataCache.GetFile(relativePath);
                if (cachedFile != null)
                {
                    var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                    Log.Debug("⚡ ValidateCatalogReferences CACHE HIT: {Path} ({Time:F3}ms)", relativePath, elapsed);
                    content = cachedFile.JsonData.ToString();
                }
            }

            // Fallback to disk
            if (content == null)
            {
                Log.Debug("💾 ValidateCatalogReferences CACHE MISS: Loading from disk: {Path}", catalogPath);
                content = File.ReadAllText(catalogPath);
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                Log.Debug("💾 Disk load completed: {Path} ({Time:F3}ms)", catalogPath, elapsed);
            }

            var json = JObject.Parse(content);

            // Find all references recursively
            var references = FindAllReferences(json);

            foreach (var (reference, location) in references)
            {
                if (!IsValidReference(reference))
                {
                    errors.Add(new ValidationError
                    {
                        Location = location,
                        Message = $"Invalid reference syntax: {reference}",
                        Severity = ErrorSeverity.Error
                    });
                    continue;
                }

                var resolved = ResolveReference(reference);
                if (resolved == null && !reference.Contains("?"))
                {
                    errors.Add(new ValidationError
                    {
                        Location = location,
                        Message = $"Reference cannot be resolved: {reference}",
                        Severity = ErrorSeverity.Warning
                    });
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add(new ValidationError
            {
                Location = catalogPath,
                Message = $"Error validating catalog: {ex.Message}",
                Severity = ErrorSeverity.Error
            });
        }

        return errors;
    }

    #region Private Helper Methods

    /// <summary>
    /// Loads a catalog file with progressive path resolution.
    /// Tries full path first, then progressively removes segments from the end,
    /// treating removed segments as categories within the catalog structure.
    /// Returns the catalog and the actual category to search within it.
    /// </summary>
    private (JObject? Catalog, string? Category) LoadCatalogWithCategory(string domain, string path, string category)
    {
        // Build the full path including category
        var fullPath = string.IsNullOrEmpty(path) 
            ? category 
            : $"{path}/{category}";

        // Try progressively shorter paths
        var pathParts = fullPath.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
        
        for (int i = pathParts.Count; i >= 0; i--)
        {
            var tryPath = i == 0 
                ? "" 
                : string.Join("/", pathParts.Take(i));
            
            var catalog = LoadCatalog(domain, tryPath);
            if (catalog != null)
            {
                // Found a catalog! The remaining path segments indicate internal category structure
                var remainingSegments = pathParts.Skip(i).ToList();
                
                if (remainingSegments.Count == 0)
                {
                    // Catalog found at full path - look for *_types structure
                    // e.g., abilities/active/offensive -> check for ability_types.offensive
                    if (pathParts.Count > 0)
                    {
                        var inferredCategory = pathParts[^1];
                        var typeKeys = catalog.Properties()
                            .Where(p => p.Name != "metadata" && p.Name.EndsWith("_types"))
                            .Select(p => p.Name)
                            .ToList();
                        
                        if (typeKeys.Count > 0)
                        {
                            var typeKey = typeKeys[0];
                            var typesObj = catalog[typeKey] as JObject;
                            if (typesObj != null && typesObj[inferredCategory] != null)
                            {
                                // Found the category in the types structure
                                return (catalog, inferredCategory);
                            }
                        }
                    }
                    
                    // No types structure - items are at root level
                    return (catalog, null);
                }
                else
                {
                    // Catalog found at partial path - remaining segments are internal categories
                    // e.g., items/weapons/catalog.json with internal weapon_types.swords structure
                    var actualCategory = remainingSegments[^1];  // Use last segment as category
                    return (catalog, actualCategory);
                }
            }
        }

        return (null, category);
    }

    private JObject? LoadCatalog(string domain, string path)
    {
        var cacheKey = $"{domain}/{path}";
        
        if (_catalogCache.TryGetValue(cacheKey, out var cached))
            return cached;

        // Handle empty path (catalog at domain root)
        var catalogPath = string.IsNullOrEmpty(path)
            ? Path.Combine(_dataRootPath, domain, "catalog.json")
            : Path.Combine(_dataRootPath, domain, path, "catalog.json");
        
        try
        {
            var startTime = DateTime.Now;
            string? content = null;

            // Try cache first
            if (_dataCache != null)
            {
                var relativePath = Path.GetRelativePath(_dataRootPath, catalogPath);
                var cachedFile = _dataCache.GetFile(relativePath);
                if (cachedFile != null)
                {
                    var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                    Log.Debug("⚡ LoadCatalog CACHE HIT: {Path} ({Time:F3}ms)", relativePath, elapsed);
                    content = cachedFile.JsonData.ToString();
                }
            }

            // Fallback to disk
            if (content == null)
            {
                if (!File.Exists(catalogPath))
                {
                    Log.Warning("Catalog file not found: {Path}", catalogPath);
                    return null;
                }

                Log.Debug("💾 LoadCatalog CACHE MISS: Loading from disk: {Path}", catalogPath);
                content = File.ReadAllText(catalogPath);
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                Log.Debug("💾 Disk load completed: {Path} ({Time:F3}ms)", catalogPath, elapsed);
            }

            var catalog = JObject.Parse(content);
            _catalogCache[cacheKey] = catalog;
            return catalog;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading catalog: {Path}", catalogPath);
            return null;
        }
    }

    private JToken? FindItemInCatalog(JObject catalog, string? category, string itemName)
    {
        
        // If no category specified, search root items array directly
        if (string.IsNullOrEmpty(category))
        {
            var rootItems = catalog["items"] as JArray;
            if (rootItems != null)
            {
                foreach (var item in rootItems)
                {
                    if (item["name"]?.ToString() == itemName)
                        return item;
                }
            }
            return null;
        }

        // Try *_types structure: ability_types.offensive.items, class_types.warrior.items, etc.
        foreach (var prop in catalog.Properties())
        {
            if (prop.Name != "metadata" && prop.Name.EndsWith("_types") && prop.Value is JObject typesObj)
            {
                var categoryData = typesObj[category];
                if (categoryData != null)
                {
                    var items = categoryData["items"] as JArray;
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            if (item["name"]?.ToString() == itemName)
                                return item;
                        }
                    }
                }
            }
        }

        // Try using FindCategoryItems as fallback
        var legacyItems = FindCategoryItems(catalog, category);
        if (legacyItems != null)
        {
            foreach (var item in legacyItems)
            {
                if (item["name"]?.ToString() == itemName)
                    return item;
            }
        }

        return null;
    }

    private JToken? SelectRandomItemFromCategory(JObject catalog, string? category)
    {
        JArray? items = null;

        // If no category specified, use root items array
        if (string.IsNullOrEmpty(category))
        {
            items = catalog["items"] as JArray;
        }
        else
        {
            // Try v4.0 structure first
            items = FindCategoryItems(catalog, category);
        }

        if (items == null || items.Count == 0)
            return null;

        // Calculate weighted random selection based on rarityWeight
        var itemsList = items.ToList();
        var totalWeight = itemsList.Sum(i => i["rarityWeight"]?.Value<int>() ?? 1);
        var random = new Random().Next(0, totalWeight);
        
        var cumulativeWeight = 0;
        foreach (var item in itemsList)
        {
            cumulativeWeight += item["rarityWeight"]?.Value<int>() ?? 1;
            if (random < cumulativeWeight)
                return item;
        }

        // Fallback to last item
        return itemsList.LastOrDefault();
    }

    private JToken? GetNestedProperty(JToken item, string propertyPath)
    {
        var properties = propertyPath.Split('.');
        var current = item;

        foreach (var prop in properties)
        {
            current = current[prop];
            if (current == null)
                return null;
        }

        return current;
    }

    private JArray? FindCategoryItems(JObject catalog, string category)
    {
        // Pattern 1: *_types[category].items structure (ability_types, class_types, etc.)
        foreach (var prop in catalog.Properties())
        {
            if (prop.Name != "metadata" && prop.Name.EndsWith("_types") && prop.Value is JObject types)
            {
                var categoryData = types[category];
                if (categoryData != null)
                {
                    var items = categoryData["items"] as JArray;
                    if (items != null)
                        return items;
                }
            }
        }

        // Pattern 2: Root-level items array (for simple catalogs)
        var rootItems = catalog["items"] as JArray;
        if (rootItems != null && rootItems.Count > 0)
        {
            return rootItems;
        }

        return null;
    }

    private List<(string Reference, string Location)> FindAllReferences(JToken token, string currentPath = "$")
    {
        var references = new List<(string, string)>();

        if (token is JValue value && value.Type == JTokenType.String)
        {
            var str = value.ToString();
            if (str.StartsWith("@"))
            {
                references.Add((str, currentPath));
            }
        }
        else if (token is JArray array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                references.AddRange(FindAllReferences(array[i], $"{currentPath}[{i}]"));
            }
        }
        else if (token is JObject obj)
        {
            foreach (var property in obj.Properties())
            {
                references.AddRange(FindAllReferences(property.Value, $"{currentPath}.{property.Name}"));
            }
        }

        return references;
    }

    #endregion

    /// <summary>
    /// Clears the catalog cache (useful when catalogs are modified)
    /// </summary>
    public void ClearCache()
    {
        _catalogCache.Clear();
        Log.Information("Reference resolver cache cleared");
    }
}

/// <summary>
/// Parsed components of a reference
/// </summary>
public class ReferenceComponents
{
    public string Domain { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public bool IsOptional { get; set; }
    public bool IsWildcard { get; set; }
    public string? Property { get; set; }
}

/// <summary>
/// Validation error
/// </summary>
public class ValidationError
{
    public string Location { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ErrorSeverity Severity { get; set; }
}

public enum ErrorSeverity
{
    Warning,
    Error
}
