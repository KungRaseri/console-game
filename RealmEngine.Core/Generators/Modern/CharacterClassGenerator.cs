using RealmEngine.Data.Services;
using Game.Shared.Data.Models;
using Game.Shared.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace RealmEngine.Core.Generators.Modern;

/// <summary>
/// Generates CharacterClass instances from classes/catalog.json using GameDataCache.
/// </summary>
public class CharacterClassGenerator
{
    private readonly GameDataCache _dataCache;
    private readonly ILogger<CharacterClassGenerator> _logger;
    private ClassCatalogData? _classCatalog;

    public CharacterClassGenerator(GameDataCache dataCache, ILogger<CharacterClassGenerator> logger)
    {
        _dataCache = dataCache ?? throw new ArgumentNullException(nameof(dataCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Loads the classes catalog from JSON.
    /// </summary>
    private ClassCatalogData LoadCatalog()
    {
        if (_classCatalog != null)
            return _classCatalog;

        var cachedFile = _dataCache.GetFile("classes/catalog.json");
        if (cachedFile == null)
        {
            _logger.LogError("Failed to load classes/catalog.json from cache");
            throw new InvalidOperationException("Classes catalog not found in cache");
        }

        try
        {
            var catalogData = JsonSerializer.Deserialize<ClassCatalogData>(cachedFile.JsonData.ToString());
            if (catalogData == null)
            {
                _logger.LogError("Failed to deserialize classes/catalog.json");
                throw new InvalidOperationException("Failed to deserialize classes catalog");
            }

            _classCatalog = catalogData;
            _logger.LogInformation("Loaded classes catalog with {Count} categories", catalogData.ClassTypes.Count);
            return catalogData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing classes/catalog.json");
            throw;
        }
    }

    /// <summary>
    /// Gets all classes from all categories.
    /// </summary>
    public List<CharacterClass> GetAllClasses()
    {
        var catalog = LoadCatalog();
        var classes = new List<CharacterClass>();

        foreach (var (categoryKey, category) in catalog.ClassTypes)
        {
            foreach (var classData in category.Items)
            {
                var characterClass = MapToCharacterClass(classData, categoryKey, category.Metadata);
                classes.Add(characterClass);
            }
        }

        _logger.LogInformation("Generated {Count} character classes", classes.Count);
        return classes;
    }

    /// <summary>
    /// Gets a specific class by name (searches all categories).
    /// </summary>
    public CharacterClass? GetClassByName(string name)
    {
        var catalog = LoadCatalog();

        foreach (var (categoryKey, category) in catalog.ClassTypes)
        {
            var classData = category.Items.FirstOrDefault(c => 
                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                c.DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (classData != null)
            {
                return MapToCharacterClass(classData, categoryKey, category.Metadata);
            }
        }

        _logger.LogWarning("Class not found: {Name}", name);
        return null;
    }

    /// <summary>
    /// Gets all classes in a specific category (e.g., "warrior", "mage").
    /// </summary>
    public List<CharacterClass> GetClassesByCategory(string categoryName)
    {
        var catalog = LoadCatalog();

        if (!catalog.ClassTypes.TryGetValue(categoryName, out var category))
        {
            _logger.LogWarning("Class category not found: {Category}", categoryName);
            return new List<CharacterClass>();
        }

        var classes = category.Items
            .Select(c => MapToCharacterClass(c, categoryName, category.Metadata))
            .ToList();

        _logger.LogInformation("Found {Count} classes in category {Category}", classes.Count, categoryName);
        return classes;
    }

    /// <summary>
    /// Maps ClassItemData to CharacterClass model.
    /// </summary>
    private CharacterClass MapToCharacterClass(ClassItemData data, string categoryKey, ClassCategoryMetadata? categoryMetadata)
    {
        var characterClass = new CharacterClass
        {
            Id = $"{categoryKey}:{data.Name}",
            Name = data.Name,
            DisplayName = data.DisplayName,
            Description = data.Description,
            RarityWeight = data.RarityWeight,
            IsSubclass = data.IsSubclass,
            ParentClassId = ParseParentClassReference(data.ParentClass),
            StartingAbilities = ParseAbilityReferences(data.StartingAbilities),
            Traits = data.Traits ?? new Dictionary<string, object>()
        };

        // Map starting stats
        if (data.StartingStats != null)
        {
            characterClass.StartingHealth = data.StartingStats.Health;
            characterClass.StartingMana = data.StartingStats.Mana;
            characterClass.BonusStrength = data.StartingStats.Strength;
            characterClass.BonusDexterity = data.StartingStats.Dexterity;
            characterClass.BonusConstitution = data.StartingStats.Constitution;
            characterClass.BonusIntelligence = data.StartingStats.Intelligence;
            characterClass.BonusWisdom = data.StartingStats.Wisdom;
            characterClass.BonusCharisma = data.StartingStats.Charisma;
        }

        // Copy primary attributes from category metadata
        if (categoryMetadata != null)
        {
            characterClass.PrimaryAttributes = new List<string>(categoryMetadata.PrimaryStats);
        }

        return characterClass;
    }

    /// <summary>
    /// Parses parent class reference like "@classes/warriors:fighter" to "warriors:fighter".
    /// </summary>
    private string? ParseParentClassReference(string? reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return null;

        // Reference format: @classes/warriors:fighter
        // Extract: warriors:fighter
        if (reference.StartsWith("@classes/"))
        {
            return reference.Substring("@classes/".Length);
        }

        _logger.LogWarning("Invalid parent class reference format: {Reference}", reference);
        return reference; // Return as-is if format is unexpected
    }

    /// <summary>
    /// Parses ability reference array like ["@abilities/active/support:heal", "@abilities/active/support:bless"].
    /// Returns list of ability IDs like "active/support:heal", "active/support:bless".
    /// </summary>
    private List<string> ParseAbilityReferences(List<string>? referencesList)
    {
        if (referencesList == null || !referencesList.Any())
            return new List<string>();

        var abilityIds = new List<string>();

        foreach (var reference in referencesList)
        {
            if (reference.StartsWith("@abilities/"))
            {
                // Extract: active/support:heal from @abilities/active/support:heal
                var abilityId = reference.Substring("@abilities/".Length);
                abilityIds.Add(abilityId);
            }
            else
            {
                _logger.LogWarning("Invalid ability reference format: {Reference}", reference);
                abilityIds.Add(reference); // Add as-is
            }
        }

        return abilityIds;
    }
}
