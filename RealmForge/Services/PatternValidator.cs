using Game.ContentBuilder.Models;

namespace Game.ContentBuilder.Services;

/// <summary>
/// Validation levels for pattern validation
/// </summary>
public enum ValidationLevel
{
    Valid,
    Warning,
    Error
}

/// <summary>
/// Result of pattern validation
/// </summary>
public class ValidationResult
{
    public ValidationLevel Level { get; set; }
    public string Message { get; set; }
    public List<string> InvalidTokens { get; set; }

    public ValidationResult(ValidationLevel level, string message)
    {
        Level = level;
        Message = message;
        InvalidTokens = new();
    }

    public ValidationResult(ValidationLevel level, string message, List<string> invalidTokens)
    {
        Level = level;
        Message = message;
        InvalidTokens = invalidTokens;
    }
}

/// <summary>
/// Validates pattern syntax and token references
/// </summary>
public static class PatternValidator
{
    /// <summary>
    /// Validate a pattern against available component groups
    /// </summary>
    /// <param name="pattern">Pattern to validate (e.g., "material + base")</param>
    /// <param name="componentGroups">Available component groups</param>
    /// <returns>ValidationResult with level, message, and invalid tokens</returns>
    public static ValidationResult Validate(string pattern, IEnumerable<ComponentGroup> componentGroups)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return new ValidationResult(ValidationLevel.Error, "Pattern cannot be empty");
        }

        // Split pattern by " + " separator
        var tokens = pattern.Split(new[] { " + " }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim())
                            .ToList();

        if (tokens.Count == 0)
        {
            return new ValidationResult(ValidationLevel.Error, "Pattern must contain at least one token");
        }

        // Get available component keys
        var availableKeys = componentGroups
            .Where(g => !g.Name.EndsWith("_types")) // Exclude organizational categories
            .Select(g => g.Name)
            .ToHashSet();

        // Validate each token
        var invalidTokens = new List<string>();
        var hasBase = false;

        foreach (var token in tokens)
        {
            // Special tokens
            if (token == "base" || token == "item")
            {
                hasBase = true;
                continue;
            }

            // Check if token matches a component key
            if (!availableKeys.Contains(token))
            {
                invalidTokens.Add(token);
            }
        }

        // Determine validation level and message
        if (invalidTokens.Count > 0)
        {
            if (!hasBase && invalidTokens.Count == tokens.Count)
            {
                // All tokens are invalid and no base token
                return new ValidationResult(
                    ValidationLevel.Error,
                    $"Invalid tokens: {string.Join(", ", invalidTokens)}. No valid tokens found.",
                    invalidTokens
                );
            }
            else
            {
                // Some tokens are invalid, but pattern will still work (graceful degradation)
                return new ValidationResult(
                    ValidationLevel.Warning,
                    $"Unknown tokens (will be skipped): {string.Join(", ", invalidTokens)}",
                    invalidTokens
                );
            }
        }

        if (!hasBase)
        {
            // Valid component tokens but no base token
            return new ValidationResult(
                ValidationLevel.Warning,
                "Pattern has no 'base' token - will generate component-only names"
            );
        }

        // All tokens are valid
        return new ValidationResult(ValidationLevel.Valid, "✓ Valid pattern");
    }

    /// <summary>
    /// Check if a pattern is valid (has at least one valid token)
    /// </summary>
    public static bool IsValid(string pattern, IEnumerable<ComponentGroup> componentGroups)
    {
        var result = Validate(pattern, componentGroups);
        return result.Level != ValidationLevel.Error;
    }

    /// <summary>
    /// Get validation icon for UI display
    /// </summary>
    public static string GetValidationIcon(ValidationLevel level)
    {
        return level switch
        {
            ValidationLevel.Valid => "CheckCircle",
            ValidationLevel.Warning => "AlertCircle",
            ValidationLevel.Error => "CloseCircle",
            _ => "HelpCircle"
        };
    }

    /// <summary>
    /// Get validation color name for UI display
    /// </summary>
    public static string GetValidationColor(ValidationLevel level)
    {
        return level switch
        {
            ValidationLevel.Valid => "Green",
            ValidationLevel.Warning => "Orange",
            ValidationLevel.Error => "Red",
            _ => "Gray"
        };
    }
}
