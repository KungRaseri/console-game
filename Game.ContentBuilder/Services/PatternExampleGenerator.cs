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

    /// <summary>
    /// Generate multiple unique examples for a pattern (for preview)
    /// </summary>
    public static List<string> GenerateMultipleExamples(string pattern, JArray? items, JObject? components, int count = 5)
    {
        var examples = new HashSet<string>(); // Use HashSet to avoid duplicates
        var maxAttempts = count * 10; // Try up to 10x the requested count to find unique examples
        var attempts = 0;

        while (examples.Count < count && attempts < maxAttempts)
        {
            attempts++;
            var example = GenerateExample(pattern, items, components);

            // Only add if it's not a placeholder/error message
            if (!string.IsNullOrWhiteSpace(example) &&
                !example.Contains("(no data available)") &&
                !example.Contains("(invalid pattern)") &&
                !example.Contains("[") && // Skip placeholders like "[material?]"
                !example.Contains("?]"))
            {
                examples.Add(example);
            }
        }

        return examples.ToList();
    }

    private static string? ResolveToken(string token, JArray? items, JObject? components)
    {
        var tokenLower = token.ToLowerInvariant();

        // Special tokens for base items
        if (tokenLower == "base" || tokenLower == "item")
        {
            if (items != null && items.Count > 0)
            {
                return GetRandomStringValue(items);
            }
            return "[no items]";
        }

        // Direct component lookup (exact match)
        if (components != null && components[token] is JArray directArray && directArray.Count > 0)
        {
            return GetRandomStringValue(directArray);
        }

        // Component not found - return placeholder
        return $"[{token}?]";
    }

    private static string? GetRandomStringValue(JArray array)
    {
        if (array.Count == 0)
            return null;

        // Pick a random item from the array
        var randomIndex = Random.Shared.Next(0, array.Count);
        var item = array[randomIndex];

        if (item.Type == JTokenType.String)
            return item.ToString();

        if (item.Type == JTokenType.Object)
        {
            var obj = item as JObject;

            // Try weight-based component format first: {value: "Iron", rarityWeight: 10}
            if (obj?["value"] != null)
                return obj["value"]?.ToString();

            // Try to get a name or displayName property
            if (obj?["name"] != null)
                return obj["name"]?.ToString();
            if (obj?["displayName"] != null)
                return obj["displayName"]?.ToString();

            // Return first property value
            var firstProp = obj?.Properties().FirstOrDefault();
            return firstProp?.Value?.ToString();
        }

        return item.ToString();
    }
}
