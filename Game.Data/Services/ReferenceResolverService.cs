using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Game.Data.Services;

/// <summary>
/// Simplified service for resolving JSON v4.1 references in Game.Core generators
/// Reference format: @domain/path/category:item-name[filters]?.property.nested
/// </summary>
public class ReferenceResolverService
{
    private readonly GameDataCache _dataCache;
    private static readonly Regex ReferencePattern = new(@"^@(?<domain>[\w-]+)/(?<path>[\w-]+(/[\w-]+)*):(?<item>[\w-*\s]+)(?<optional>\?)?(?<property>\.[\w.]+)?$", RegexOptions.Compiled);

    public ReferenceResolverService(GameDataCache dataCache)
    {
        _dataCache = dataCache ?? throw new ArgumentNullException(nameof(dataCache));
    }

    /// <summary>
    /// Resolves a reference asynchronously - for compatibility with generators
    /// </summary>
    public async Task<object?> ResolveAsync(string reference)
    {
        return await Task.FromResult(Resolve(reference));
    }

    /// <summary>
    /// Resolves a reference synchronously
    /// </summary>
    public object? Resolve(string reference)
    {
        var components = ParseReference(reference);
        if (components == null)
        {
            Console.WriteLine($"Invalid reference syntax: {reference}");
            return null;
        }

        try
        {
            // Build the file path for the catalog
            var catalogPath = string.IsNullOrEmpty(components.Path)
                ? $"{components.Domain}/catalog.json"
                : $"{components.Domain}/{components.Path}/catalog.json";

            var catalogFile = _dataCache.GetFile(catalogPath);
            if (catalogFile?.JsonData == null)
            {
                if (components.IsOptional)
                    return null;
                
                Console.WriteLine($"Catalog not found for reference: {reference}");
                return null;
            }

            var catalog = catalogFile.JsonData;

            // Find the item
            var item = FindItemInCatalog(catalog, components.Category, components.ItemName);
            
            if (item == null)
            {
                if (components.IsOptional)
                    return null;
                
                Console.WriteLine($"Item not found: {components.ItemName} in {components.Category}");
                return null;
            }

            // For generators, return the resolved ID format
            var resolvedId = string.IsNullOrEmpty(components.Path)
                ? $"{components.Category}:{components.ItemName}"
                : $"{components.Path}/{components.Category}:{components.ItemName}";

            // Apply property access if specified
            if (!string.IsNullOrEmpty(components.Property))
            {
                var propertyValue = GetNestedProperty(item, components.Property);
                return propertyValue?.ToString() ?? resolvedId;
            }

            return resolvedId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error resolving reference {reference}: {ex.Message}");
            if (components.IsOptional)
                return null;
            throw;
        }
    }

    /// <summary>
    /// Parses a reference string into components
    /// </summary>
    private ReferenceComponents? ParseReference(string reference)
    {
        var match = ReferencePattern.Match(reference);
        if (!match.Success)
            return null;

        var fullPath = match.Groups["path"].Value;
        
        string path;
        string category;
        
        var pathParts = fullPath.Split('/');
        if (pathParts.Length == 1)
        {
            path = "";
            category = pathParts[0];
        }
        else
        {
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
            Property = match.Groups["property"].Success ? match.Groups["property"].Value.TrimStart('.') : null
        };
    }

    /// <summary>
    /// Finds an item in a catalog
    /// </summary>
    private JToken? FindItemInCatalog(JToken catalog, string? category, string itemName)
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
        foreach (var property in catalog.Children<JProperty>())
        {
            if (property.Name != "metadata" && property.Name.EndsWith("_types") && property.Value is JObject typesObj)
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

        return null;
    }

    /// <summary>
    /// Gets nested property from JSON token
    /// </summary>
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
}

/// <summary>
/// Parsed components of a reference
/// </summary>
internal class ReferenceComponents
{
    public string Domain { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public bool IsOptional { get; set; }
    public string? Property { get; set; }
}