using Newtonsoft.Json.Linq;
using Game.ContentBuilder.Models;

namespace Game.ContentBuilder.Services;

/// <summary>
/// Generates metadata for JSON data files automatically
/// </summary>
public static class MetadataGenerator
{
    /// <summary>
    /// Generate complete metadata object with user-defined and auto-generated fields
    /// </summary>
    /// <param name="description">User-provided description of the file</param>
    /// <param name="version">User-provided version string (e.g., "1.0")</param>
    /// <param name="notes">Optional user notes (can be null or empty)</param>
    /// <param name="componentGroups">Component groups from the file</param>
    /// <param name="patterns">Pattern list from the file</param>
    /// <param name="items">Items array from the file (optional)</param>
    /// <returns>JObject containing all metadata fields</returns>
    public static JObject Generate(
        string description,
        string version,
        string? notes,
        IEnumerable<ComponentGroup> componentGroups,
        IEnumerable<PatternItem> patterns,
        IEnumerable<string> items)
    {
        var componentGroupsList = componentGroups.ToList();
        var patternsList = patterns.ToList();
        var itemsList = items.ToList();

        var metadata = new JObject
        {
            // User-defined fields
            ["description"] = description,
            ["version"] = version,
            
            // Auto-generated fields
            ["lastUpdated"] = DateTime.Now.ToString("yyyy-MM-dd"),
            ["type"] = InferFileType(componentGroupsList, patternsList, itemsList),
            ["componentKeys"] = new JArray(ExtractComponentKeys(componentGroupsList)),
            ["patternTokens"] = new JArray(ExtractPatternTokens(patternsList)),
            ["totalPatterns"] = patternsList.Count,
            ["total_items"] = itemsList.Count
        };

        // Add notes if provided
        if (!string.IsNullOrWhiteSpace(notes))
        {
            metadata["notes"] = notes;
        }

        // Add category counts if there are nested types
        var categoryTypes = componentGroupsList
            .Where(g => g.Name.EndsWith("_types"))
            .ToList();

        foreach (var categoryType in categoryTypes)
        {
            // Count items in category (nested structure)
            metadata[$"{categoryType.Name}_count"] = categoryType.Components.Count;
        }

        return metadata;
    }

    /// <summary>
    /// Extract component keys from component groups (excluding category types)
    /// </summary>
    private static IEnumerable<string> ExtractComponentKeys(List<ComponentGroup> componentGroups)
    {
        return componentGroups
            .Where(g => !g.Name.EndsWith("_types")) // Exclude organizational categories
            .Select(g => g.Name)
            .OrderBy(name => name);
    }

    /// <summary>
    /// Extract all unique tokens used in patterns, including "base"
    /// </summary>
    private static IEnumerable<string> ExtractPatternTokens(List<PatternItem> patterns)
    {
        var tokens = new HashSet<string> { "base" }; // Always include "base" token

        foreach (var pattern in patterns)
        {
            if (string.IsNullOrWhiteSpace(pattern.Pattern))
                continue;

            // Split pattern by " + " separator
            var parts = pattern.Pattern.Split(new[] { " + " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var token = part.Trim();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    tokens.Add(token);
                }
            }
        }

        return tokens.OrderBy(t => t);
    }

    /// <summary>
    /// Infer the file type based on structure
    /// </summary>
    private static string InferFileType(List<ComponentGroup> componentGroups, List<PatternItem> patterns, List<string> items)
    {
        // Has patterns = pattern generation file
        if (patterns.Count > 0)
            return "pattern_generation";

        // Has components but no patterns = component library
        if (componentGroups.Count > 0 && patterns.Count == 0)
            return "component_library";

        // Has items but no components/patterns = item catalog
        if (items.Count > 0 && componentGroups.Count == 0)
            return "item_catalog";

        // Default
        return "data_file";
    }
}
