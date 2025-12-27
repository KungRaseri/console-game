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
            var normalizedPath = filePath.Replace('\\', '/').ToLowerInvariant();
            var json = File.ReadAllText(filePath);
            var root = JObject.Parse(json);

            // Check for abilities domain files (new architecture)
            if (normalizedPath.Contains("/abilities/"))
            {
                if (fileName == "catalog.json")
                    return JsonFileType.GenericCatalog;  // abilities/passive/defensive/catalog.json
                
                if (fileName == "names.json")
                    return JsonFileType.NamesFile;  // abilities/passive/defensive/names.json
            }

            // Check if filename contains patterns (more flexible matching)
            if (fileName.Contains("names") && fileName.EndsWith(".json"))
            {
                return JsonFileType.NamesFile;
            }

            if (fileName.Contains("catalog") && fileName.EndsWith(".json"))
            {
                if (directoryName == "quests")
                    return JsonFileType.QuestCatalog;  // Planned v4.0
                
                return JsonFileType.GenericCatalog;
            }

            switch (fileName)
            {
                // Active v4.0 Files
                case "abilities.json":
                    return JsonFileType.AbilityCatalog;

                // Planned v4.0 Files
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
            // Active v4.0 Editors
            JsonFileType.NamesFile => EditorType.NameListEditor,
            JsonFileType.GenericCatalog => EditorType.CatalogEditor,
            
            // Planned v4.0 Editors (not implemented yet)
            JsonFileType.QuestCatalog => EditorType.QuestEditor,
            JsonFileType.QuestData => EditorType.QuestEditor,
            
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
/// Types of JSON files in the game data structure (v4.0)
/// </summary>
public enum JsonFileType
{
    Unknown,
    
    // Active v4.0 File Types
    NamesFile,          // names.json - pattern generation files
    AbilityCatalog,     // abilities.json - ability catalog files
    GenericCatalog,     // Generic catalogs (occupations, traits, dialogue, items, etc.)
    
    // Planned v4.0 File Types
    QuestCatalog,       // Quest catalog.json (v4.0: templates + locations)
    QuestData           // Quest objectives, rewards, locations (v4.0)
}

