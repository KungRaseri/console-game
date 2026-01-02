using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace RealmEngine.Data.Services;

/// <summary>
/// Simplified service for resolving JSON v4.1 references in RealmEngine.Core generators
/// Reference format: @domain/path/category:item-name[filters]?.property.nested
/// </summary>
public class ReferenceResolverService
{
    private readonly GameDataCache _dataCache;
    private static readonly Regex ReferencePattern = new(@"^@(?<domain>[\w-]+)/(?<path>[\w-]+(/[\w-]+)*):(?<item>[\w-*\s]+)(?<filters>\[[^\]]+\])?(?<optional>\?)?(?<property>\.[\w.]+)?$", RegexOptions.Compiled);
    private readonly Random _random = new();

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
    /// Resolves a reference and returns the full JSON object instead of just the ID
    /// </summary>
    public async Task<JToken?> ResolveToObjectAsync(string reference)
    {
        return await Task.FromResult(ResolveToObject(reference));
    }

    /// <summary>
    /// Resolves a reference synchronously and returns the full JSON object
    /// </summary>
    public JToken? ResolveToObject(string reference)
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

            // Handle wildcard - return random item from category
            if (components.ItemName == "*")
            {
                var items = GetAllItemsInCategory(catalog, components.Category);
                
                // Apply filters if specified
                if (!string.IsNullOrEmpty(components.Filters))
                {
                    items = ApplyFilters(items, components.Filters);
                }
                
                // Return random item weighted by rarityWeight
                if (items.Count > 0)
                {
                    return SelectRandomWeighted(items);
                }
                return null;
            }
            
            // Find specific item and return the full object
            var item = FindItemInCatalog(catalog, components.Category, components.ItemName);
            
            if (item == null)
            {
                if (components.IsOptional)
                    return null;
                
                Console.WriteLine($"Item not found: {components.ItemName} in {components.Category}");
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
            Console.WriteLine($"Error resolving reference {reference}: {ex.Message}");
            if (components.IsOptional)
                return null;
            throw;
        }
    }

    /// <summary>
    /// Selects a random item from array using rarityWeight
    /// </summary>
    private JToken? SelectRandomWeighted(JArray items)
    {
        if (items.Count == 0) return null;
        if (items.Count == 1) return items[0];

        var totalWeight = 0;
        foreach (var item in items)
        {
            var weight = item["rarityWeight"]?.Value<int>() ?? 1;
            totalWeight += weight;
        }

        var randomValue = _random.Next(totalWeight);
        var currentWeight = 0;

        foreach (var item in items)
        {
            var weight = item["rarityWeight"]?.Value<int>() ?? 1;
            currentWeight += weight;
            if (randomValue < currentWeight)
            {
                return item;
            }
        }

        return items[^1];
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

            // Handle wildcard - return array of items
            if (components.ItemName == "*")
            {
                var items = GetAllItemsInCategory(catalog, components.Category);
                
                // Apply filters if specified
                if (!string.IsNullOrEmpty(components.Filters))
                {
                    items = ApplyFilters(items, components.Filters);
                }
                
                return items;
            }
            
            // Find specific item
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
            Filters = match.Groups["filters"].Success ? match.Groups["filters"].Value.Trim('[', ']') : null,
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

    /// <summary>
    /// Gets all items in a category
    /// </summary>
    private JArray GetAllItemsInCategory(JToken catalog, string? category)
    {
        var result = new JArray();
        
        if (string.IsNullOrEmpty(category))
        {
            var rootItems = catalog["items"] as JArray;
            if (rootItems != null)
            {
                foreach (var item in rootItems)
                    result.Add(item);
            }
            return result;
        }

        // Try *_types structure (ability_types.offensive.items, etc.)
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
                            result.Add(item);
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Applies filters to items array
    /// Format: property=value or property.nested=value or property EXISTS
    /// Special: property=true checks if property exists and is truthy (object/array/true)
    /// </summary>
    private JArray ApplyFilters(JArray items, string filters)
    {
        var result = new JArray();
        
        // Split multiple filters by & or comma
        var filterParts = filters.Split(new[] { '&', ',' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var item in items)
        {
            bool matchesAll = true;
            
            foreach (var filter in filterParts)
            {
                var trimmedFilter = filter.Trim();
                
                // Check for EXISTS operator (no = sign)
                var equalIndex = trimmedFilter.IndexOf('=');
                
                if (equalIndex < 0)
                {
                    // No = sign means check if property exists and is not null/empty
                    var actualValue = GetNestedProperty(item, trimmedFilter);
                    if (actualValue == null || actualValue.Type == JTokenType.Null)
                    {
                        matchesAll = false;
                        break;
                    }
                    continue;
                }
                
                var propertyPath = trimmedFilter.Substring(0, equalIndex).Trim();
                var expectedValue = trimmedFilter.Substring(equalIndex + 1).Trim();
                
                var actualValue2 = GetNestedProperty(item, propertyPath);
                
                // Handle "property=true" as an existence check
                // If expecting true and property is an object/array, treat as exists check
                if (expectedValue.ToLower() == "true")
                {
                    if (actualValue2 == null || actualValue2.Type == JTokenType.Null)
                    {
                        matchesAll = false;
                        break;
                    }
                    // If it's a boolean, check the actual value
                    if (actualValue2.Type == JTokenType.Boolean)
                    {
                        if (!actualValue2.Value<bool>())
                        {
                            matchesAll = false;
                            break;
                        }
                    }
                    // Otherwise, any non-null value (object, array, etc.) is considered "true"
                    continue;
                }
                
                // Compare values
                if (actualValue2 == null)
                {
                    matchesAll = false;
                    break;
                }
                
                var actualString = actualValue2.ToString();
                
                // Handle boolean comparisons for =false
                if (expectedValue.ToLower() == "false")
                {
                    if (actualValue2.Type == JTokenType.Boolean)
                    {
                        if (actualValue2.Value<bool>())
                        {
                            matchesAll = false;
                            break;
                        }
                    }
                    else
                    {
                        matchesAll = false;
                        break;
                    }
                }
                else if (actualString != expectedValue)
                {
                    matchesAll = false;
                    break;
                }
            }
            
            if (matchesAll)
                result.Add(item);
        }
        
        return result;
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
    public string? Filters { get; set; }
    public bool IsOptional { get; set; }
    public string? Property { get; set; }
}