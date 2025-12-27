using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Game.ContentBuilder.Services;

/// <summary>
/// Universal service for replacing {base} and other catalog-based tokens across all editors
/// </summary>
public class CatalogTokenService
{
  private readonly JsonEditorService _jsonEditorService;
  private readonly Random _random = new Random();

  public CatalogTokenService(JsonEditorService jsonEditorService)
  {
    _jsonEditorService = jsonEditorService;
  }

  /// <summary>
  /// Replaces {base} token with a random name from the catalog file
  /// </summary>
  /// <param name="text">Text containing {base} token</param>
  /// <param name="sourceFileName">Name of the source file (e.g., "weapons/names.json")</param>
  /// <returns>Text with {base} replaced, or original text if replacement fails</returns>
  public string ReplaceBaseToken(string text, string sourceFileName)
  {
    if (string.IsNullOrEmpty(text) || !text.Contains("{base}", StringComparison.OrdinalIgnoreCase))
    {
      return text;
    }

    var baseNames = LoadBaseNamesFromCatalog(sourceFileName);

    if (baseNames.Count == 0)
    {
      Log.Warning("No base names found for {FileName}, keeping {base} token", sourceFileName);
      return text;
    }

    // Replace {base} with random selection
    string selectedName = baseNames[_random.Next(baseNames.Count)];
    return text.Replace("{base}", selectedName, StringComparison.OrdinalIgnoreCase);
  }

  /// <summary>
  /// Replaces all catalog-based tokens in text
  /// </summary>
  /// <param name="text">Text containing tokens</param>
  /// <param name="sourceFileName">Name of the source file</param>
  /// <param name="customTokens">Optional custom token replacements (key: token name, value: replacement value)</param>
  /// <returns>Text with all tokens replaced</returns>
  public string ReplaceAllTokens(string text, string sourceFileName, Dictionary<string, string>? customTokens = null)
  {
    if (string.IsNullOrEmpty(text))
    {
      return text;
    }

    string result = text;

    // Replace {base} token
    result = ReplaceBaseToken(result, sourceFileName);

    // Replace custom tokens if provided
    if (customTokens != null)
    {
      foreach (var token in customTokens)
      {
        string tokenPattern = $"{{{token.Key}}}";
        result = result.Replace(tokenPattern, token.Value, StringComparison.OrdinalIgnoreCase);
      }
    }

    return result;
  }

  /// <summary>
  /// Loads base names from the catalog.json file in the same directory as the source file
  /// Supports both flat (items array) and nested (type_name → category → items) structures
  /// </summary>
  /// <param name="sourceFileName">Name of the source file (e.g., "weapons/names.json", "abilities/active/offensive/names.json")</param>
  /// <returns>List of names extracted from catalog</returns>
  public List<string> LoadBaseNamesFromCatalog(string sourceFileName)
  {
    var baseNames = new List<string>();

    try
    {
      // Get the full path first, then extract directory
      string fullPath = _jsonEditorService.GetFilePath(sourceFileName);
      string directory = Path.GetDirectoryName(fullPath) ?? string.Empty;
      string fileName = Path.GetFileName(sourceFileName);

      Log.Debug("LoadBaseNamesFromCatalog - Source: {FileName}, Directory: {Directory}", fileName, directory);

      // Determine the catalog filename based on convention
      // names.json → catalog.json
      // *_names.json → catalog.json (fallback)
      string catalogFileName = "catalog.json";

      string catalogPath = Path.Combine(directory, catalogFileName);

      if (!File.Exists(catalogPath))
      {
        Log.Warning("Catalog file not found: {Path}", catalogPath);
        return new List<string>();
      }

      string catalogJson = File.ReadAllText(catalogPath);
      var catalog = JObject.Parse(catalogJson);

      Log.Debug("Catalog root properties: {Props}", string.Join(", ", catalog.Properties().Select(p => p.Name)));

      // Extract names from catalog structure
      // New flat format: { "items": [{ "name": "..." }] }
      // Old nested format: { "weapon_types": { "swords": { "items": [{ "name": "..." }] } } }
      // NPC format: { "backgrounds": { "common_folk": { items: [...] } } }

      // First try flat "items" array at root (new format)
      if (catalog["items"] is JArray flatItems)
      {
        Log.Debug("Found flat items array with {Count} items", flatItems.Count);
        ExtractNamesFromArray(flatItems, baseNames);

        if (baseNames.Count > 0)
        {
          Log.Information("✓ Loaded {Count} base names from {CatalogFile} (flat structure)",
              baseNames.Count, Path.GetFileName(catalogPath));
          return baseNames;
        }
      }

      // Try nested structure - check ALL top-level properties (except metadata)
      foreach (var topProp in catalog.Properties())
      {
        // Skip metadata
        if (topProp.Name == "metadata") continue;

        if (topProp.Value is JObject typesObj)
        {
          Log.Debug("Checking nested structure under: {Key}", topProp.Name);

          foreach (var categoryProp in typesObj.Properties())
          {
            // Skip metadata within categories
            if (categoryProp.Name == "metadata") continue;

            if (categoryProp.Value is JObject categoryObj && categoryObj["items"] is JArray items)
            {
              ExtractNamesFromArray(items, baseNames);
            }
          }
        }
      }

      if (baseNames.Count > 0)
      {
        Log.Information("✓ Loaded {Count} base names from {CatalogFile} (nested structure)",
            baseNames.Count, Path.GetFileName(catalogPath));
        Log.Debug("Sample names: {Names}", string.Join(", ", baseNames.Take(5)));
      }
      else
      {
        Log.Warning("✗ No base names found in {CatalogFile}", Path.GetFileName(catalogPath));
      }
    }
    catch (Exception ex)
    {
      Log.Error(ex, "✗ Failed to load base names from catalog for {FileName}", sourceFileName);
    }

    return baseNames;
  }

  /// <summary>
  /// Extracts name values from a JArray of items
  /// </summary>
  private void ExtractNamesFromArray(JArray items, List<string> namesList)
  {
    foreach (var item in items)
    {
      string? name = item["name"]?.ToString();
      if (!string.IsNullOrEmpty(name))
      {
        namesList.Add(name);
      }
    }
  }

  /// <summary>
  /// Gets a random name from the catalog for preview purposes
  /// </summary>
  /// <param name="sourceFileName">Name of the source file</param>
  /// <returns>Random name from catalog, or "[base]" if none found</returns>
  public string GetRandomBaseName(string sourceFileName)
  {
    var baseNames = LoadBaseNamesFromCatalog(sourceFileName);

    if (baseNames.Count == 0)
    {
      return "[base]";
    }

    return baseNames[_random.Next(baseNames.Count)];
  }
}
