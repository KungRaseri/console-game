using Newtonsoft.Json.Linq;

namespace Game.ContentBuilder.Services;

/// <summary>
/// Generates examples for patterns based on available components and items
/// </summary>
public class PatternExampleGenerator
{
    /// <summary>
    /// Generate an example for a pattern like "material + base" using actual data
    /// </summary>
    public static string GenerateExample(string pattern, JArray? items, JObject? components)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return string.Empty;

        try
        {
            // Split pattern into tokens (e.g., "material + base" -> ["material", "base"])
            var tokens = pattern.Split(new[] { " + " }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(t => t.Trim())
                                .ToArray();

            if (tokens.Length == 0)
                return string.Empty;

            var exampleParts = new List<string>();

            foreach (var token in tokens)
            {
                var value = ResolveToken(token, items, components);
                if (!string.IsNullOrEmpty(value))
                {
                    exampleParts.Add(value);
                }
            }

            return exampleParts.Count > 0 
                ? string.Join(" ", exampleParts) 
                : "(no data available)";
        }
        catch
        {
            return "(invalid pattern)";
        }
    }

    private static string? ResolveToken(string token, JArray? items, JObject? components)
    {
        var tokenLower = token.ToLowerInvariant();

        // Special tokens for base items
        if (tokenLower == "base" || tokenLower == "item")
        {
            if (items != null && items.Count > 0)
            {
                return GetFirstStringValue(items);
            }
            return "[no items]";
        }

        // Direct component lookup (exact match)
        if (components != null && components[token] is JArray directArray && directArray.Count > 0)
        {
            return GetFirstStringValue(directArray);
        }

        // Component not found - return placeholder
        return $"[{token}?]";
    }

    private static string? GetFirstStringValue(JArray array)
    {
        var first = array.FirstOrDefault();
        if (first == null)
            return null;

        if (first.Type == JTokenType.String)
            return first.ToString();

        if (first.Type == JTokenType.Object)
        {
            // Try to get a name or displayName property
            var obj = first as JObject;
            if (obj?["name"] != null)
                return obj["name"]?.ToString();
            if (obj?["displayName"] != null)
                return obj["displayName"]?.ToString();
            
            // Return first property value
            var firstProp = obj?.Properties().FirstOrDefault();
            return firstProp?.Value?.ToString();
        }

        return first.ToString();
    }
}
