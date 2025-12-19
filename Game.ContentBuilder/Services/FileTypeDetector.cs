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

            // Special case: quests/catalog.json is v4.0 quest catalog
            if (fileName == "catalog.json" && directoryName == "quests")
            {
                return JsonFileType.QuestCatalog;
            }

            // Check metadata first
            if (root["metadata"] is JObject metadata)
            {
                var type = metadata["type"]?.ToString();
                
                return type switch
                {
                    "pattern_generation" => JsonFileType.NamesFile,
                    "item_catalog" => JsonFileType.TypesFile,
                    "material_catalog" => JsonFileType.MaterialCatalog,
                    "ability_catalog" => JsonFileType.AbilityCatalog,
                    "occupation_catalog" or 
                    "personality_trait_catalog" or 
                    "dialogue_style_catalog" or 
                    "dialogue_template_catalog" or 
                    "rumor_template_catalog" or 
                    "quirk_catalog" or 
                    "background_catalog" or 
                    "component_library" or 
                    "pattern_components" or 
                    "reference_data" => JsonFileType.GenericCatalog,
                    "name_catalog" or "surname_catalog" => JsonFileType.NameCatalog,
                    "quest_template_catalog" => JsonFileType.QuestTemplate,
                    var s when s?.StartsWith("quest_") == true => JsonFileType.QuestData,
                    "configuration" => JsonFileType.Configuration,
                    "component_catalog" => JsonFileType.ComponentCatalog,
                    _ => DetectByStructure(root, fileName)
                };
            }

            // Fallback to structure-based detection
            return DetectByStructure(root, fileName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to detect file type: {FilePath}", filePath);
            return JsonFileType.Unknown;
        }
    }

    /// <summary>
    /// Detects file type based on JSON structure
    /// </summary>
    private static JsonFileType DetectByStructure(JObject root, string fileName)
    {
        // Check for names.json structure: metadata + components + patterns
        if (root["components"] != null && root["patterns"] != null)
        {
            return JsonFileType.NamesFile;
        }

        // Check for catalog.json structure: metadata + *_types (e.g., weapon_types, armor_types)
        var typeKeys = root.Properties()
            .Where(p => p.Name.EndsWith("_types", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (typeKeys.Any())
        {
            return JsonFileType.TypesFile;
        }

        // Check for component catalog (like materials/names.json)
        if (root["components"] != null && !root.ContainsKey("patterns"))
        {
            return JsonFileType.ComponentCatalog;
        }

        // Check for material catalog (materials/catalog.json)
        if (root["material_types"] != null)
        {
            return JsonFileType.MaterialCatalog;
        }

        // Legacy detection based on filename patterns
        if (fileName.Contains("names"))
            return JsonFileType.NamesFile;
        
        if (fileName.Contains("types"))
            return JsonFileType.TypesFile;
        
        if (fileName.Contains("prefix") || fileName.Contains("suffix"))
            return JsonFileType.PrefixSuffix;
        
        if (fileName.Contains("trait"))
            return JsonFileType.Traits;

        return JsonFileType.Unknown;
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
            JsonFileType.NamesFile => EditorType.NamesEditor,
            JsonFileType.TypesFile => EditorType.ItemCatalogEditor,
            JsonFileType.AbilityCatalog => EditorType.AbilitiesEditor,
            JsonFileType.GenericCatalog => EditorType.CatalogEditor,
            JsonFileType.NameCatalog => EditorType.NameCatalogEditor,
            JsonFileType.QuestCatalog => EditorType.QuestCatalogEditor,
            JsonFileType.QuestData => EditorType.QuestDataEditor,
            JsonFileType.Configuration => EditorType.ConfigEditor,
            JsonFileType.ComponentCatalog => EditorType.ComponentEditor,
            JsonFileType.MaterialCatalog => EditorType.MaterialEditor,
            JsonFileType.PrefixSuffix => EditorType.ItemPrefix,
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

        var supportsTraits = metadata["supports_traits"]?.Value<bool>() ?? false;
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
    TypesFile,          // catalog.json - item catalog files
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

