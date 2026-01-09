using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Generators;

/// <summary>
/// Utility class for composing entity names from pattern-based components.
/// Supports Enemy, NPC, Ability, and other domain name generation.
/// </summary>
public class NameComposer
{
    private readonly ILogger<NameComposer> _logger;
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of the <see cref="NameComposer"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public NameComposer(ILogger<NameComposer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = new Random();
    }

    /// <summary>
    /// Composes a name from a pattern and components dictionary.
    /// </summary>
    /// <param name="pattern">Pattern string with tokens like "{size} {base} {title}"</param>
    /// <param name="components">Components dictionary from names.json</param>
    /// <param name="componentValues">Output dictionary of resolved component values</param>
    /// <returns>The composed name string</returns>
    public string ComposeName(string pattern, JToken components, out Dictionary<string, string> componentValues)
    {
        componentValues = new Dictionary<string, string>();
        
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return string.Empty;
        }

        var result = pattern;
        
        // Find all tokens in the pattern like {size}, {base}, {title}
        var tokens = System.Text.RegularExpressions.Regex.Matches(pattern, @"\{([^}]+)\}");
        
        foreach (System.Text.RegularExpressions.Match match in tokens)
        {
            var token = match.Groups[1].Value; // e.g., "size", "base", "title"
            var componentArray = components?[token];
            
            if (componentArray != null && componentArray.Any())
            {
                // Select random component by weight
                var selectedComponent = GetRandomWeightedComponent(componentArray);
                if (selectedComponent != null)
                {
                    var value = GetStringProperty(selectedComponent, "value") 
                              ?? GetStringProperty(selectedComponent, "name") 
                              ?? token;
                    
                    // Store the resolved value for this component
                    componentValues[token] = value;
                    
                    // Replace token with actual value
                    result = result.Replace($"{{{token}}}", value);
                }
            }
            else
            {
                // No components found for this token, remove it
                result = result.Replace($"{{{token}}}", "");
            }
        }
        
        // If pattern is just "base" without braces, return the base directly
        if (pattern == "base" && components?["base"] != null)
        {
            var baseComponents = components["base"];
            if (baseComponents != null)
            {
                var selectedComponent = GetRandomWeightedComponent(baseComponents);
                if (selectedComponent != null)
                {
                    var value = GetStringProperty(selectedComponent, "value") 
                              ?? GetStringProperty(selectedComponent, "name") 
                              ?? "";
                    componentValues["base"] = value;
                    return value;
                }
            }
        }
        
        // Clean up extra spaces
        result = System.Text.RegularExpressions.Regex.Replace(result.Trim(), @"\s+", " ");
        
        return result;
    }

    /// <summary>
    /// Selects a random component from an array using rarityWeight.
    /// </summary>
    private JToken? GetRandomWeightedComponent(JToken components)
    {
        var componentList = components.ToList();
        if (!componentList.Any()) return null;

        var totalWeight = componentList.Sum(c => GetIntProperty(c, "rarityWeight", 1));
        var randomValue = _random.Next(1, totalWeight + 1);

        int currentWeight = 0;
        foreach (var component in componentList)
        {
            currentWeight += GetIntProperty(component, "rarityWeight", 1);
            if (randomValue <= currentWeight)
            {
                return component;
            }
        }

        return componentList.First();
    }

    /// <summary>
    /// Safely gets a string property from a JToken.
    /// </summary>
    private static string? GetStringProperty(JToken? token, string propertyName)
    {
        return token?[propertyName]?.Value<string>();
    }

    /// <summary>
    /// Safely gets an integer property from a JToken.
    /// </summary>
    private static int GetIntProperty(JToken? token, string propertyName, int defaultValue = 0)
    {
        var value = token?[propertyName]?.Value<int>();
        return value ?? defaultValue;
    }

    /// <summary>
    /// Selects a random pattern from a patterns array using rarityWeight.
    /// </summary>
    /// <param name="patterns">The patterns array.</param>
    /// <returns>The selected pattern.</returns>
    public JToken? GetRandomWeightedPattern(JToken patterns)
    {
        var patternList = patterns.ToList();
        if (!patternList.Any()) return null;

        var totalWeight = patternList.Sum(p => GetIntProperty(p, "rarityWeight", 1));
        var randomValue = _random.Next(1, totalWeight + 1);

        int currentWeight = 0;
        foreach (var pattern in patternList)
        {
            currentWeight += GetIntProperty(pattern, "rarityWeight", 1);
            if (randomValue <= currentWeight)
            {
                return pattern;
            }
        }

        return patternList.First();
    }

    /// <summary>
    /// Composes a name from a pattern and components, returning structured component data.
    /// This method categorizes components into prefixes and suffixes based on their position relative to {base}.
    /// </summary>
    /// <param name="pattern">Pattern string with tokens like "{size} {base} {title}"</param>
    /// <param name="components">Components dictionary from names.json</param>
    /// <returns>Tuple containing (composed name, base name, prefix components, suffix components)</returns>
    public (string name, string baseName, List<NameComponent> prefixes, List<NameComponent> suffixes) 
        ComposeNameWithComponents(string pattern, JToken components)
    {
        var prefixes = new List<NameComponent>();
        var suffixes = new List<NameComponent>();
        string baseName = string.Empty;
        
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return (string.Empty, string.Empty, prefixes, suffixes);
        }

        var result = pattern;
        var nameParts = new List<string>();
        bool foundBase = false;
        
        // Find all tokens in the pattern like {size}, {base}, {title}
        var tokens = System.Text.RegularExpressions.Regex.Matches(pattern, @"\{([^}]+)\}");
        
        foreach (System.Text.RegularExpressions.Match match in tokens)
        {
            var token = match.Groups[1].Value; // e.g., "size", "base", "title"
            
            // Special handling for {base} token - it's a positional marker
            if (token == "base")
            {
                foundBase = true;
                var baseComponentArray = components?[token];
                
                // If base component exists, resolve it; otherwise leave as placeholder
                if (baseComponentArray != null && baseComponentArray.Any())
                {
                    var selectedComponent = GetRandomWeightedComponent(baseComponentArray);
                    if (selectedComponent != null)
                    {
                        var value = GetStringProperty(selectedComponent, "value") 
                                  ?? GetStringProperty(selectedComponent, "name") 
                                  ?? token;
                        baseName = value;
                        nameParts.Add(value);
                        result = result.Replace($"{{{token}}}", value);
                    }
                }
                else
                {
                    // {base} exists in pattern but has no component - it's a placeholder
                    // Don't add to nameParts, just mark position for prefix/suffix categorization
                    result = result.Replace($"{{{token}}}", "");
                }
                continue;
            }
            
            var componentArray = components?[token];
            
            if (componentArray != null && componentArray.Any())
            {
                // Select random component by weight
                var selectedComponent = GetRandomWeightedComponent(componentArray);
                if (selectedComponent != null)
                {
                    var value = GetStringProperty(selectedComponent, "value") 
                              ?? GetStringProperty(selectedComponent, "name") 
                              ?? token;
                    
                    // Categorize component based on position relative to {base}
                    if (token == "base")
                    {
                        baseName = value;
                        foundBase = true;
                        nameParts.Add(value);
                    }
                    else if (!foundBase)
                    {
                        // Token appears before {base} → Prefix
                        prefixes.Add(new NameComponent { Token = token, Value = value });
                        nameParts.Add(value);
                    }
                    else
                    {
                        // Token appears after {base} → Suffix
                        suffixes.Add(new NameComponent { Token = token, Value = value });
                        nameParts.Add(value);
                    }
                    
                    // Replace token with actual value in result string
                    result = result.Replace($"{{{token}}}", value);
                }
            }
            else
            {
                // No components found for this token, remove it
                result = result.Replace($"{{{token}}}", "");
            }
        }
        
        // If pattern is just "base" without braces, return the base directly
        if (pattern == "base" && components?["base"] != null)
        {
            var baseComponents = components["base"];
            if (baseComponents != null)
            {
                var selectedComponent = GetRandomWeightedComponent(baseComponents);
                if (selectedComponent != null)
                {
                    var value = GetStringProperty(selectedComponent, "value") 
                              ?? GetStringProperty(selectedComponent, "name") 
                              ?? "";
                    baseName = value;
                    return (value, baseName, prefixes, suffixes);
                }
            }
        }
        
        // Clean up the composed name
        var composedName = string.Join(" ", nameParts.Where(p => !string.IsNullOrWhiteSpace(p)));
        
        return (composedName, baseName, prefixes, suffixes);
    }
}
