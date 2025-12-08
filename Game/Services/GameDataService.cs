using System.Text.Json;
using Game.Data.Models;
using Serilog;

namespace Game.Services;

/// <summary>
/// Service for loading and managing game data from JSON files.
/// Provides caching and easy access to all game data.
/// </summary>
public class GameDataService
{
    private static GameDataService? _instance;
    private static readonly object _lock = new();
    
    private readonly string _dataPath;
    
    // Item data
    public WeaponPrefixData WeaponPrefixes { get; private set; } = new();
    public WeaponNameData WeaponNames { get; private set; } = new();
    public ArmorMaterialData ArmorMaterials { get; private set; } = new();
    public EnchantmentSuffixData EnchantmentSuffixes { get; private set; } = new();
    
    // Material properties for items
    public MetalData Metals { get; private set; } = new();
    public LeatherData Leathers { get; private set; } = new();
    public WoodData Woods { get; private set; } = new();
    public GemstoneData Gemstones { get; private set; } = new();
    
    // Enemy data - names
    public EnemyNameData BeastNames { get; private set; } = new();
    public EnemyNameData UndeadNames { get; private set; } = new();
    public EnemyNameData DemonNames { get; private set; } = new();
    public EnemyNameData ElementalNames { get; private set; } = new();
    public DragonNameData DragonNames { get; private set; } = new();
    public HumanoidNameData HumanoidNames { get; private set; } = new();
    
    // Enemy data - prefixes with traits
    public EnemyPrefixData BeastPrefixes { get; private set; } = new();
    public EnemyPrefixData UndeadPrefixes { get; private set; } = new();
    public EnemyPrefixData DemonPrefixes { get; private set; } = new();
    public EnemyPrefixData ElementalPrefixes { get; private set; } = new();
    public EnemyPrefixData DragonPrefixes { get; private set; } = new();
    public EnemyPrefixData HumanoidPrefixes { get; private set; } = new();
    
    // Dragon color data
    public DragonColorData DragonColors { get; private set; } = new();
    
    // NPC data
    public FantasyNameData FantasyNames { get; private set; } = new();
    public OccupationData Occupations { get; private set; } = new();
    public DialogueTemplateData DialogueTemplates { get; private set; } = new();
    public DialogueTraitsData DialogueTraits { get; private set; } = new();
    
    // Quest data
    public QuestTemplatesData QuestTemplates { get; private set; } = new();
    
    // General data
    public AdjectiveData Adjectives { get; private set; } = new();
    public MaterialData Materials { get; private set; } = new();
    
    private GameDataService(string dataPath)
    {
        _dataPath = dataPath;
        LoadAllData();
    }
    
    /// <summary>
    /// Get the singleton instance of GameDataService.
    /// </summary>
    public static GameDataService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Default to looking in Data/Json relative to the executable
                        var basePath = AppDomain.CurrentDomain.BaseDirectory;
                        var dataPath = Path.Combine(basePath, "Data", "Json");
                        _instance = new GameDataService(dataPath);
                    }
                }
            }
            return _instance;
        }
    }
    
    /// <summary>
    /// Initialize the service with a custom data path (useful for testing).
    /// </summary>
    public static void Initialize(string dataPath)
    {
        lock (_lock)
        {
            _instance = new GameDataService(dataPath);
        }
    }
    
    /// <summary>
    /// Reload all data from JSON files (useful for hot-reloading during development).
    /// </summary>
    public void Reload()
    {
        LoadAllData();
        Log.Information("Game data reloaded from {DataPath}", _dataPath);
    }
    
    private void LoadAllData()
    {
        try
        {
            // Load item data
            WeaponPrefixes = LoadJson<WeaponPrefixData>("items/weapon_prefixes.json");
            WeaponNames = LoadJson<WeaponNameData>("items/weapon_names.json");
            ArmorMaterials = LoadJson<ArmorMaterialData>("items/armor_materials.json");
            EnchantmentSuffixes = LoadJson<EnchantmentSuffixData>("items/enchantment_suffixes.json");
            
            // Load material properties
            Metals = LoadJson<MetalData>("items/metals.json");
            Leathers = LoadJson<LeatherData>("items/leathers.json");
            Woods = LoadJson<WoodData>("items/woods.json");
            Gemstones = LoadJson<GemstoneData>("items/gemstones.json");
            
            // Load enemy data - names
            BeastNames = LoadJson<EnemyNameData>("enemies/beast_names.json");
            UndeadNames = LoadJson<EnemyNameData>("enemies/undead_names.json");
            DemonNames = LoadJson<EnemyNameData>("enemies/demon_names.json");
            ElementalNames = LoadJson<EnemyNameData>("enemies/elemental_names.json");
            DragonNames = LoadJson<DragonNameData>("enemies/dragon_names.json");
            HumanoidNames = LoadJson<HumanoidNameData>("enemies/humanoid_names.json");
            
            // Load enemy data - prefixes with traits
            BeastPrefixes = LoadJson<EnemyPrefixData>("enemies/beast_prefixes.json");
            UndeadPrefixes = LoadJson<EnemyPrefixData>("enemies/undead_prefixes.json");
            DemonPrefixes = LoadJson<EnemyPrefixData>("enemies/demon_prefixes.json");
            ElementalPrefixes = LoadJson<EnemyPrefixData>("enemies/elemental_prefixes.json");
            DragonPrefixes = LoadJson<EnemyPrefixData>("enemies/dragon_prefixes.json");
            HumanoidPrefixes = LoadJson<EnemyPrefixData>("enemies/humanoid_prefixes.json");
            
            // Load dragon color data
            DragonColors = LoadJson<DragonColorData>("enemies/dragon_colors.json");
            
            // Load NPC data
            FantasyNames = LoadJson<FantasyNameData>("npcs/fantasy_names.json");
            Occupations = LoadJson<OccupationData>("npcs/occupations.json");
            DialogueTemplates = LoadJson<DialogueTemplateData>("npcs/dialogue_templates.json");
            DialogueTraits = LoadJson<DialogueTraitsData>("npcs/dialogue_traits.json");
            
            // Load quest data
            QuestTemplates = LoadJson<QuestTemplatesData>("quests/quest_templates.json");
            
            // Load general data
            Adjectives = LoadJson<AdjectiveData>("general/adjectives.json");
            Materials = LoadJson<MaterialData>("general/materials.json");
            
            Log.Information("Successfully loaded all game data from {DataPath}", _dataPath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load game data from {DataPath}", _dataPath);
            throw;
        }
    }
    
    private T LoadJson<T>(string relativePath) where T : new()
    {
        var fullPath = Path.Combine(_dataPath, relativePath);
        
        if (!File.Exists(fullPath))
        {
            Log.Warning("Data file not found: {FilePath}. Using empty data.", fullPath);
            return new T();
        }
        
        try
        {
            var json = File.ReadAllText(fullPath);
            var data = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            });
            
            return data ?? new T();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to parse JSON file: {FilePath}", fullPath);
            throw new InvalidOperationException($"Failed to load {relativePath}", ex);
        }
    }
    
    /// <summary>
    /// Get a random item from a list.
    /// </summary>
    public static T GetRandom<T>(List<T> list)
    {
        if (list == null || list.Count == 0)
            throw new InvalidOperationException("Cannot get random item from empty list");
        
        return list[Random.Shared.Next(list.Count)];
    }
}
