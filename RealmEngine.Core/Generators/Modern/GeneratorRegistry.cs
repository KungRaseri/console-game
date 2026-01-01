using RealmEngine.Data.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace RealmEngine.Core.Generators.Modern;

/// <summary>
/// Central registry for all game content generators.
/// Provides unified access to all generator types with lazy initialization.
/// </summary>
public class GeneratorRegistry
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    
    // Lazy-loaded generators
    private AbilityGenerator? _abilityGenerator;
    private CharacterClassGenerator? _classGenerator;
    private EnemyGenerator? _enemyGenerator;
    private ItemGenerator? _itemGenerator;
    private NpcGenerator? _npcGenerator;
    private QuestGenerator? _questGenerator;
    private LocationGenerator? _locationGenerator;
    private OrganizationGenerator? _organizationGenerator;
    private DialogueGenerator? _dialogueGenerator;
    private EnchantmentGenerator? _enchantmentGenerator;

    public GeneratorRegistry(GameDataCache dataCache, ReferenceResolverService referenceResolver)
    {
        _dataCache = dataCache ?? throw new ArgumentNullException(nameof(dataCache));
        _referenceResolver = referenceResolver ?? throw new ArgumentNullException(nameof(referenceResolver));
    }

    /// <summary>
    /// Gets the ability generator (creates on first access).
    /// </summary>
    public AbilityGenerator Abilities
    {
        get
        {
            _abilityGenerator ??= new AbilityGenerator(_dataCache, _referenceResolver);
            return _abilityGenerator;
        }
    }

    /// <summary>
    /// Gets the character class generator (creates on first access).
    /// </summary>
    public CharacterClassGenerator Classes
    {
        get
        {
            if (_classGenerator == null)
            {
                var logger = NullLoggerFactory.Instance.CreateLogger<CharacterClassGenerator>();
                _classGenerator = new CharacterClassGenerator(_dataCache, logger);
            }
            return _classGenerator;
        }
    }

    /// <summary>
    /// Gets the enemy generator (creates on first access).
    /// </summary>
    public EnemyGenerator Enemies
    {
        get
        {
            _enemyGenerator ??= new EnemyGenerator(_dataCache, _referenceResolver);
            return _enemyGenerator;
        }
    }

    /// <summary>
    /// Gets the item generator (creates on first access).
    /// </summary>
    public ItemGenerator Items
    {
        get
        {
            _itemGenerator ??= new ItemGenerator(_dataCache, _referenceResolver);
            return _itemGenerator;
        }
    }

    /// <summary>
    /// Gets the NPC generator (creates on first access).
    /// </summary>
    public NpcGenerator Npcs
    {
        get
        {
            _npcGenerator ??= new NpcGenerator(_dataCache, _referenceResolver);
            return _npcGenerator;
        }
    }

    /// <summary>
    /// Gets the quest generator (creates on first access).
    /// </summary>
    public QuestGenerator Quests
    {
        get
        {
            _questGenerator ??= new QuestGenerator(_dataCache, _referenceResolver);
            return _questGenerator;
        }
    }

    /// <summary>
    /// Gets the location generator (creates on first access).
    /// </summary>
    public LocationGenerator Locations
    {
        get
        {
            _locationGenerator ??= new LocationGenerator(_dataCache, _referenceResolver);
            return _locationGenerator;
        }
    }

    /// <summary>
    /// Gets the organization generator (creates on first access).
    /// </summary>
    public OrganizationGenerator Organizations
    {
        get
        {
            _organizationGenerator ??= new OrganizationGenerator(_dataCache, _referenceResolver);
            return _organizationGenerator;
        }
    }

    /// <summary>
    /// Gets the dialogue generator (creates on first access).
    /// </summary>
    public DialogueGenerator Dialogue
    {
        get
        {
            _dialogueGenerator ??= new DialogueGenerator(_dataCache, _referenceResolver);
            return _dialogueGenerator;
        }
    }

    /// <summary>
    /// Gets the enchantment generator (creates on first access).
    /// </summary>
    public EnchantmentGenerator Enchantments
    {
        get
        {
            _enchantmentGenerator ??= new EnchantmentGenerator(_dataCache, _referenceResolver);
            return _enchantmentGenerator;
        }
    }

    /// <summary>
    /// Pre-initializes all generators for performance.
    /// </summary>
    public void WarmUp()
    {
        _ = Abilities;
        _ = Classes;
        _ = Enemies;
        _ = Items;
        _ = Npcs;
        _ = Quests;
        _ = Locations;
        _ = Organizations;
        _ = Dialogue;
        _ = Enchantments;
    }
}
