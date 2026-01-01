using System.Collections.ObjectModel;
using System.IO;
using Game.ContentBuilder.Models;
using RealmEngine.Data.Services;
using Newtonsoft.Json;
using Serilog;

namespace Game.ContentBuilder.Services;

/// <summary>
/// Service for dynamically building the file tree from the actual JSON files on disk
/// </summary>
public class FileTreeService
{
    private readonly string _dataPath;
    private readonly GameDataCache? _dataCache;
    private readonly Dictionary<string, FolderConfig> _configCache = new();

    // Fallback icon mappings (used if no .cbconfig.json found)
    private static readonly Dictionary<string, string> CategoryIcons = new()
    {
        // Top-level categories
        { "general", "Earth" },
        { "items", "Sword" },
        { "enemies", "Skull" },
        { "npcs", "AccountGroup" },
        { "quests", "BookOpenVariant" },
        { "locations", "MapMarker" },
        { "abilities", "AutoFix" },
        { "classes", "ShieldAccount" },
        { "world", "Earth" },
        { "social", "AccountGroup" },
        { "organizations", "OfficeBuildingOutline" },
        
        // World subcategories
        { "regions", "MapMarkerPath" },
        { "environments", "WeatherPartlyCloudy" },
        { "settlements", "TownHall" },
        { "points_of_interest", "MapMarker" },
        
        // Social subcategories
        { "dialogue", "CommentText" },
        { "relationships", "HeartMultiple" },
        { "personalities", "EmoticonHappy" },
        { "backgrounds", "BookOpenPageVariant" },
        
        // Organizations subcategories
        { "factions", "ShieldStar" },
        { "guilds", "BankOutline" },
        { "shops", "StoreOutline" },
        { "businesses", "OfficeBuildingMarker" },
        
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
        
        // Quest subcategories
        { "objectives", "ChecklistCheck" },
        { "rewards", "TreasureChest" }
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

    public FileTreeService(string dataPath, GameDataCache? dataCache = null)
    {
        _dataPath = dataPath;
        _dataCache = dataCache;
        
        if (_dataCache != null)
        {
            Log.Information("FileTreeService initialized with cache - fast config loading enabled");
        }
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

        // Load folder config if available
        var config = LoadFolderConfig(directoryPath);

        var node = new CategoryNode
        {
            Name = config?.DisplayName ?? FormatDisplayName(dirName),
            Icon = config?.Icon ?? GetIcon(dirName, isDirectory: true),
            Children = new ObservableCollection<CategoryNode>()
        };

        // Get all JSON files in this directory (excluding .cbconfig.json)
        var jsonFiles = Directory.GetFiles(directoryPath, "*.json")
            .Where(f => !Path.GetFileName(f).Equals(".cbconfig.json", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        node.FileCount = jsonFiles.Length;

        foreach (var file in jsonFiles.OrderBy(f => f))
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var relativeFilePath = Path.GetRelativePath(basePath, file).Replace("\\", "/");

            // Use file type detector to determine editor type
            var editorType = FileTypeDetector.GetEditorType(file);

            // Get icon from config or fallback
            var fileIcon = config?.FileIcons.GetValueOrDefault(fileName)
                ?? config?.DefaultFileIcon
                ?? GetIcon(fileName, isDirectory: false);

            var fileNode = new CategoryNode
            {
                Name = FormatDisplayName(fileName),
                Icon = fileIcon,
                EditorType = editorType,
                Tag = relativeFilePath,
                FileCount = 0,
                TotalFileCount = 0  // Files don't have children
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

        // Calculate total file count (this directory + all subdirectories)
        node.TotalFileCount = node.FileCount + node.Children
            .Where(c => c.Tag == null) // Only count directory nodes
            .Sum(c => c.TotalFileCount);

        return node;
    }

    /// <summary>
    /// Loads folder configuration from .cbconfig.json if it exists
    /// </summary>
    private FolderConfig? LoadFolderConfig(string directoryPath)
    {
        var configPath = Path.Combine(directoryPath, ".cbconfig.json");

        if (!File.Exists(configPath))
            return null;

        try
        {
            var startTime = DateTime.Now;
            string? json = null;
            
            // Try cache first for fast access
            if (_dataCache != null)
            {
                var relativePath = Path.GetRelativePath(_dataPath, configPath);
                var cachedFile = _dataCache.GetFile(relativePath);
                
                if (cachedFile != null)
                {
                    var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                    Log.Debug("⚡ Config CACHE HIT: {Directory} ({Time:F3}ms)", Path.GetFileName(directoryPath), elapsed);
                    json = cachedFile.JsonData.ToString();
                }
                else
                {
                    Log.Debug("💾 Config CACHE MISS: {Directory} - loading from disk", Path.GetFileName(directoryPath));
                    json = File.ReadAllText(configPath);
                }
            }
            else
            {
                // Fallback to file I/O
                json = File.ReadAllText(configPath);
            }
            
            var config = JsonConvert.DeserializeObject<FolderConfig>(json);

            if (config != null)
            {
                _configCache[directoryPath] = config;
                var totalElapsed = (DateTime.Now - startTime).TotalMilliseconds;
                Log.Debug("Loaded folder config for: {Directory} ({Time:F1}ms)", Path.GetFileName(directoryPath), totalElapsed);
            }

            return config;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load folder config: {ConfigPath}", configPath);
            return null;
        }
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
