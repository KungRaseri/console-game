using System.Collections.ObjectModel;
using System.IO;
using Game.ContentBuilder.Models;
using Serilog;

namespace Game.ContentBuilder.Services;

/// <summary>
/// Service for dynamically building the file tree from the actual JSON files on disk
/// </summary>
public class FileTreeService
{
    private readonly string _dataPath;

    // Icon mappings for categories and file types
    private static readonly Dictionary<string, string> CategoryIcons = new()
    {
        // Top-level categories
        { "general", "Earth" },
        { "items", "Sword" },
        { "enemies", "Skull" },
        { "npcs", "AccountGroup" },
        { "quests", "BookOpenVariant" },
        { "locations", "MapMarker" },
        { "world", "Earth" },
        
        // Items subcategories
        { "weapons", "SwordCross" },
        { "armor", "ShieldOutline" },
        { "consumables", "BottleTonicPlus" },
        { "enchantments", "AutoFix" },
        { "materials", "HammerWrench" },
        
        // Enemy types
        { "beasts", "Paw" },
        { "demons", "Fire" },
        { "dragons", "Creation" },
        { "elementals", "Water" },
        { "humanoids", "HumanGreeting" },
        { "undead", "Ghost" },
        { "vampires", "VampireFangs" },
        { "goblinoids", "MonsterGoblin" },
        { "orcs", "MonsterOrc" },
        { "insects", "Bug" },
        { "plants", "Sprout" },
        { "reptilians", "Snake" },
        { "trolls", "MonsterTroll" },
        
        // NPC subcategories
        { "names", "CardAccountDetails" },
        { "personalities", "EmoticonHappy" },
        { "dialogue", "CommentText" },
        { "occupations", "Briefcase" },
        
        // Quest subcategories
        { "objectives", "ChecklistCheck" },
        { "rewards", "TreasureChest" },
        { "templates", "FileDocument" }
    };

    private static readonly Dictionary<string, string> FileIcons = new()
    {
        { "names", "FormatListBulleted" },
        { "prefixes", "FileEdit" },
        { "suffixes", "FileEdit" },
        { "traits", "Star" },
        { "colors", "Palette" },
        { "materials", "HammerWrench" },
        { "effects", "Sparkles" },
        { "rarities", "Star" },
        { "metals", "Anvil" },
        { "woods", "Tree" },
        { "leathers", "Leather" },
        { "gemstones", "Diamond" },
        { "greetings", "HandWave" },
        { "farewells", "HandPeace" },
        { "rumors", "Microphone" },
        { "backgrounds", "Book" },
        { "quirks", "Puzzle" },
        { "templates", "FileDocument" },
        { "first_names", "Account" },
        { "last_names", "AccountGroup" },
        { "common", "AccountTie" },
        { "criminal", "Incognito" },
        { "magical", "AutoFix" },
        { "noble", "Crown" },
        { "primary", "Numeric1Circle" },
        { "secondary", "Numeric2Circle" },
        { "hidden", "EyeOff" },
        { "gold", "CurrencyUsd" },
        { "experience", "ChartLine" },
        { "items", "Gift" },
        { "towns", "TownHall" },
        { "dungeons", "CastleOutline" },
        { "wilderness", "Forest" },
        { "smells", "Flower" },
        { "sounds", "VolumeHigh" },
        { "textures", "Texture" },
        { "time_of_day", "ClockOutline" },
        { "weather", "WeatherPartlyCloudy" },
        { "verbs", "Run" },
        { "adjectives", "FormatListBulleted" }
    };

    public FileTreeService(string dataPath)
    {
        _dataPath = dataPath;
    }

    /// <summary>
    /// Builds the complete category tree by scanning the file system
    /// </summary>
    public ObservableCollection<CategoryNode> BuildCategoryTree()
    {
        Log.Information("Building category tree from: {DataPath}", _dataPath);
        
        var categories = new ObservableCollection<CategoryNode>();

        if (!Directory.Exists(_dataPath))
        {
            Log.Error("Data path does not exist: {DataPath}", _dataPath);
            return categories;
        }

        // Get all top-level directories
        var topLevelDirs = Directory.GetDirectories(_dataPath);
        
        foreach (var dir in topLevelDirs.OrderBy(d => d))
        {
            var category = BuildCategoryNode(dir, _dataPath);
            
            if (category != null && (category.Children.Any() || category.Tag != null))
            {
                categories.Add(category);
            }
        }

        Log.Information("Category tree built - {Count} top-level categories, {FileCount} total files",
            categories.Count, CountFiles(categories));
        
        return categories;
    }

    private CategoryNode? BuildCategoryNode(string directoryPath, string basePath)
    {
        var dirName = Path.GetFileName(directoryPath);
        
        var node = new CategoryNode
        {
            Name = FormatDisplayName(dirName),
            Icon = GetIcon(dirName, isDirectory: true),
            Children = new ObservableCollection<CategoryNode>()
        };

        // Get all JSON files in this directory
        var jsonFiles = Directory.GetFiles(directoryPath, "*.json");
        
        foreach (var file in jsonFiles.OrderBy(f => f))
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var relativeFilePath = Path.GetRelativePath(basePath, file).Replace("\\", "/");
            
            var fileNode = new CategoryNode
            {
                Name = FormatDisplayName(fileName),
                Icon = GetIcon(fileName, isDirectory: false),
                EditorType = DetermineEditorType(file, fileName),
                Tag = relativeFilePath
            };
            
            node.Children.Add(fileNode);
        }

        // Get all subdirectories
        var subdirs = Directory.GetDirectories(directoryPath);
        
        foreach (var subdir in subdirs.OrderBy(d => d))
        {
            var childNode = BuildCategoryNode(subdir, basePath);
            
            if (childNode != null && (childNode.Children.Any() || childNode.Tag != null))
            {
                node.Children.Add(childNode);
            }
        }

        return node;
    }

    private static string FormatDisplayName(string name)
    {
        // Convert snake_case and kebab-case to Title Case
        var parts = name.Split(new[] { '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", parts)
            .Replace("npcs", "NPCs", StringComparison.OrdinalIgnoreCase)
            .Split(' ')
            .Select(word => char.ToUpper(word[0]) + word[1..])
            .Aggregate((a, b) => a + " " + b);
    }

    private static string GetIcon(string name, bool isDirectory)
    {
        var key = name.ToLower().Replace(" ", "").Replace("_", "").Replace("-", "");
        
        // Try exact match first
        if (CategoryIcons.TryGetValue(key, out var categoryIcon))
            return categoryIcon;
        
        if (FileIcons.TryGetValue(name.ToLower(), out var fileIcon))
            return fileIcon;
        
        // Fallback icons
        return isDirectory ? "Folder" : "FileDocument";
    }

    private static EditorType DetermineEditorType(string filePath, string fileName)
    {
        try
        {
            // Read first few lines to determine structure
            var fileContent = File.ReadAllText(filePath);
            
            // Quick heuristic checks based on content
            if (fileContent.Contains("\"components\""))
            {
                // Enchantments or other component-based files
                if (fileName.Contains("prefix", StringComparison.OrdinalIgnoreCase))
                    return EditorType.ItemPrefix;
                if (fileName.Contains("suffix", StringComparison.OrdinalIgnoreCase))
                    return EditorType.ItemSuffix;
                return EditorType.HybridArray;
            }
            
            if (fileContent.Contains("\"categories\""))
            {
                // Name lists with categories
                return EditorType.NameList;
            }
            
            if (fileName.Contains("prefix", StringComparison.OrdinalIgnoreCase))
            {
                return EditorType.ItemPrefix;
            }
            
            if (fileName.Contains("suffix", StringComparison.OrdinalIgnoreCase))
            {
                return EditorType.ItemSuffix;
            }
            
            // Check for flat item structure (materials, occupations, etc.)
            if (fileContent.Contains("\"durability\"") || 
                fileContent.Contains("\"hardness\"") ||
                fileContent.Contains("\"income\"") ||
                fileContent.Contains("\"socialStatus\""))
            {
                return EditorType.FlatItem;
            }
            
            // Default to HybridArray for most files
            return EditorType.HybridArray;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Could not determine editor type for {FilePath}, defaulting to HybridArray", filePath);
            return EditorType.HybridArray;
        }
    }

    private int CountFiles(ObservableCollection<CategoryNode> nodes)
    {
        int count = 0;
        foreach (var node in nodes)
        {
            if (node.Tag != null)
                count++;
            count += CountFiles(node.Children);
        }
        return count;
    }
}
