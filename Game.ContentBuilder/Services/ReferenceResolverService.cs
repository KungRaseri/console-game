using Newtonsoft.Json.Linq;
using Serilog;
using System.IO;
using System.Text.RegularExpressions;

namespace Game.ContentBuilder.Services;

/// <summary>
/// Service for resolving JSON v4.1 references
/// Reference format: @domain/path/category:item-name[filters]?.property.nested
/// </summary>
public class ReferenceResolverService
{
    private readonly string _dataRootPath;
    private readonly Dictionary<string, JObject> _catalogCache;
    private static readonly Regex ReferencePattern = new(@"^@(?<domain>[\w-]+)/(?<path>[\w-]+(/[\w-]+)*)/(?<category>[\w-]+):(?<item>[\w-*]+)(?<optional>\?)?(?<property>\.[\w.]+)?$", RegexOptions.Compiled);

    public ReferenceResolverService(string dataRootPath)
    {
        _dataRootPath = dataRootPath;
        _catalogCache = new Dictionary<string, JObject>();
        Log.Information("ReferenceResolverService initialized with path: {Path}", dataRootPath);
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

        return new ReferenceComponents
        {
            Domain = match.Groups["domain"].Value,
            Path = match.Groups["path"].Value,
            Category = match.Groups["category"].Value,
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
            var catalog = LoadCatalog(components.Domain, components.Path);
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
                return SelectRandomItemFromCategory(catalog, components.Category);
            }

            // Find specific item
            var item = FindItemInCatalog(catalog, components.Category, components.ItemName);
            if (item == null)
            {
                if (components.IsOptional)
                    return null;
                
                Log.Warning("Item not found: {ItemName} in {Category}", components.ItemName, components.Category);
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
            var catalog = LoadCatalog(domain, path);
            if (catalog == null)
                return references;

            var components = catalog["components"]?[category] as JArray;
            if (components == null)
                return references;

            foreach (var item in components)
            {
                var name = item["name"]?.ToString();
                if (!string.IsNullOrEmpty(name))
                {
                    references.Add($"@{domain}/{path}/{category}:{name}");
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
            var catalog = LoadCatalog(domain, path);
            if (catalog == null)
                return new List<string>();

            var componentKeys = catalog["componentKeys"] as JArray;
            if (componentKeys == null)
                return new List<string>();

            return componentKeys.Select(k => k.ToString()).ToList();
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
            var content = File.ReadAllText(catalogPath);
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

    private JObject? LoadCatalog(string domain, string path)
    {
        var cacheKey = $"{domain}/{path}";
        
        if (_catalogCache.TryGetValue(cacheKey, out var cached))
            return cached;

        var catalogPath = Path.Combine(_dataRootPath, domain, path, "catalog.json");
        
        if (!File.Exists(catalogPath))
        {
            Log.Warning("Catalog file not found: {Path}", catalogPath);
            return null;
        }

        try
        {
            var content = File.ReadAllText(catalogPath);
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

    private JToken? FindItemInCatalog(JObject catalog, string category, string itemName)
    {
        var components = catalog["components"]?[category] as JArray;
        if (components == null)
            return null;

        foreach (var item in components)
        {
            if (item["name"]?.ToString() == itemName)
                return item;
        }

        return null;
    }

    private JToken? SelectRandomItemFromCategory(JObject catalog, string category)
    {
        var components = catalog["components"]?[category] as JArray;
        if (components == null || components.Count == 0)
            return null;

        // Calculate weighted random selection based on rarityWeight
        var items = components.ToList();
        var totalWeight = items.Sum(i => i["rarityWeight"]?.Value<int>() ?? 1);
        var random = new Random().Next(0, totalWeight);
        
        var cumulativeWeight = 0;
        foreach (var item in items)
        {
            cumulativeWeight += item["rarityWeight"]?.Value<int>() ?? 1;
            if (random < cumulativeWeight)
                return item;
        }

        // Fallback to last item
        return items.LastOrDefault();
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
