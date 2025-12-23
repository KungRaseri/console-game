using System.IO;
using Newtonsoft.Json.Linq;
using Serilog;
using Game.ContentBuilder.Models;

namespace Game.ContentBuilder.Services;

/// <summary>
/// Service for detecting JSON file types based on structure and metadata
/// </summary>
public static class FileTypeDetector
{
    /// <summary>
    /// Detects the type of a JSON file based on its structure
    /// </summary>
    /// <param name="filePath">Path to the JSON file</param>
    /// <returns>Detected file type</returns>
    public static JsonFileType DetectFileType(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Log.Warning("File not found for type detection: {FilePath}", filePath);
                return JsonFileType.Unknown;
            }

            var fileName = Path.GetFileName(filePath).ToLowerInvariant();
            var directoryName = Path.GetFileName(Path.GetDirectoryName(filePath) ?? "").ToLowerInvariant();
            var json = File.ReadAllText(filePath);
            var root = JObject.Parse(json);

            switch (fileName)
            {
                case "catalog.json" when directoryName == "quests":
                    return JsonFileType.QuestCatalog;

                case "catalog.json":
                    return JsonFileType.GenericCatalog;

                case "names.json":
                    return JsonFileType.NamesFile;

                case "abilities.json":
                    return JsonFileType.AbilityCatalog;

                case "traits.json":
                    return JsonFileType.Traits;

                case "quest_templates.json":
                    return JsonFileType.QuestTemplate;

                case "rewards.json":
                case "objectives.json":
                    return JsonFileType.QuestData;
            }

            return JsonFileType.Unknown;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to detect file type: {FilePath}", filePath);
            return JsonFileType.Unknown;
        }
    }

    /// <summary>
    /// Determines the appropriate editor type for a JSON file
    /// </summary>
    /// <param name="filePath">Path to the JSON file</param>
    /// <returns>Recommended editor type</returns>
    public static EditorType GetEditorType(string filePath)
    {
        var fileType = DetectFileType(filePath);

        return fileType switch
        {
            JsonFileType.NamesFile => EditorType.NameListEditor,
            JsonFileType.CatelogFile => EditorType.ItemCatalogEditor,
            JsonFileType.AbilityCatalog => EditorType.AbilitiesEditor,
            JsonFileType.GenericCatalog => EditorType.CatalogEditor,
            JsonFileType.NameCatalog => EditorType.NameCatalogEditor,
            JsonFileType.QuestCatalog => EditorType.QuestCatalogEditor,
            JsonFileType.QuestData => EditorType.QuestDataEditor,
            JsonFileType.Configuration => EditorType.ConfigEditor,
            JsonFileType.ComponentCatalog => EditorType.ComponentEditor,
            JsonFileType.MaterialCatalog => EditorType.MaterialEditor,
            JsonFileType.Traits => EditorType.TraitEditor,
            _ => EditorType.None
        };
    }

    /// <summary>
    /// Reads metadata from a JSON file
    /// </summary>
    /// <param name="filePath">Path to the JSON file</param>
    /// <returns>Metadata object or null</returns>
    public static JObject? ReadMetadata(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return null;

            var json = File.ReadAllText(filePath);
            var root = JObject.Parse(json);

            return root["metadata"] as JObject;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to read metadata from: {FilePath}", filePath);
            return null;
        }
    }

    /// <summary>
    /// Checks if a file has v4.0 trait support
    /// </summary>
    public static bool SupportsTraits(string filePath)
    {
        var metadata = ReadMetadata(filePath);
        if (metadata == null)
            return false;

        var supportsTraits = metadata["supportsTraits"]?.Value<bool>() ?? false;
        var version = metadata["version"]?.ToString() ?? "0.0";

        return supportsTraits || version.StartsWith("4.");
    }
}

/// <summary>
/// Types of JSON files in the game data structure
/// </summary>
public enum JsonFileType
{
    Unknown,
    NamesFile,          // names.json - pattern generation files
    CatelogFile,        // catalog.json - item catalog files
    AbilityCatalog,     // abilities.json - ability catalog files
    GenericCatalog,     // Generic catalogs (occupations, traits, dialogue, etc.)
    NameCatalog,        // Name lists (first_names, last_names)
    QuestTemplate,      // Quest template files (OLD - v3.x)
    QuestCatalog,       // Quest catalog.json (NEW - v4.0: templates + locations)
    QuestData,          // Quest objectives, rewards, locations
    Configuration,      // Configuration files (rarity_config, etc.)
    ComponentCatalog,   // Component catalogs (like materials/names.json)
    MaterialCatalog,    // Material type catalogs (materials/catalog.json)
    PrefixSuffix,       // Legacy prefix/suffix files
    Traits,             // Trait definition files
    General             // General configuration files
}

