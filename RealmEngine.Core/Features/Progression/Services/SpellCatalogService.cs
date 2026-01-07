using RealmEngine.Data.Services;
using RealmEngine.Shared.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace RealmEngine.Core.Features.Progression.Services;

/// <summary>
/// Service for loading and accessing spell definitions from spells/catalog.json.
/// Provides spell metadata organized by magical tradition (Arcane, Divine, Occult, Primal).
/// </summary>
public class SpellCatalogService
{
    private readonly GameDataCache _dataCache;
    private readonly ILogger<SpellCatalogService> _logger;
    private readonly Dictionary<string, Spell> _spells = new();
    private readonly Dictionary<MagicalTradition, List<string>> _spellsByTradition = new();
    private bool _initialized;

    public SpellCatalogService(GameDataCache dataCache, ILogger<SpellCatalogService>? logger = null)
    {
        _dataCache = dataCache ?? throw new ArgumentNullException(nameof(dataCache));
        _logger = logger ?? NullLogger<SpellCatalogService>.Instance;
    }

    /// <summary>
    /// Initialize by loading spells catalog.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            _logger.LogWarning("SpellCatalogService already initialized");
            return;
        }

        try
        {
            var catalogPath = "spells/catalog";
            var catalogData = await _dataCache.GetFileAsJsonAsync(catalogPath);
            
            if (catalogData == null)
            {
                throw new InvalidOperationException($"Failed to load spells catalog from {catalogPath}");
            }

            ParseSpellCatalog(catalogData);
            
            _initialized = true;
            _logger.LogInformation("SpellCatalogService initialized with {Count} spells", _spells.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize SpellCatalogService");
            throw;
        }
    }

    /// <summary>
    /// Get spell by ID.
    /// </summary>
    public Spell? GetSpell(string spellId)
    {
        EnsureInitialized();
        return _spells.GetValueOrDefault(spellId);
    }

    /// <summary>
    /// Get all spells.
    /// </summary>
    public IReadOnlyDictionary<string, Spell> GetAllSpells()
    {
        EnsureInitialized();
        return _spells;
    }

    /// <summary>
    /// Get spells by magical tradition.
    /// </summary>
    public List<Spell> GetSpellsByTradition(MagicalTradition tradition)
    {
        EnsureInitialized();
        
        if (!_spellsByTradition.TryGetValue(tradition, out var spellIds))
        {
            return new List<Spell>();
        }

        return spellIds.Select(id => _spells[id]).ToList();
    }

    /// <summary>
    /// Get spells by rank (0-10).
    /// </summary>
    public List<Spell> GetSpellsByRank(int rank)
    {
        EnsureInitialized();
        return _spells.Values.Where(s => s.Rank == rank).ToList();
    }

    /// <summary>
    /// Get learnable spells for a character based on their magic skills.
    /// </summary>
    public List<Spell> GetLearnableSpells(Character character)
    {
        EnsureInitialized();
        
        var learnable = new List<Spell>();

        foreach (var spell in _spells.Values)
        {
            // Check if character has the tradition skill
            var traditionSkillId = GetTraditionSkillId(spell.Tradition);
            if (!character.Skills.TryGetValue(traditionSkillId, out var skill))
            {
                continue; // Don't have this tradition
            }

            // Can learn spells up to character's skill rank (with some tolerance)
            if (spell.MinimumSkillRank <= skill.CurrentRank + 10)
            {
                learnable.Add(spell);
            }
        }

        return learnable;
    }

    /// <summary>
    /// Get tradition skill ID from tradition enum.
    /// </summary>
    public string GetTraditionSkillId(MagicalTradition tradition)
    {
        return tradition switch
        {
            MagicalTradition.Arcane => "arcane",
            MagicalTradition.Divine => "divine",
            MagicalTradition.Occult => "occult",
            MagicalTradition.Primal => "primal",
            _ => "arcane"
        };
    }

    private void ParseSpellCatalog(JObject catalog)
    {
        var spellTypes = catalog["spell_types"] as JObject;
        if (spellTypes == null)
        {
            _logger.LogError("spell_types not found in catalog");
            return;
        }

        foreach (var traditionProp in spellTypes.Properties())
        {
            var traditionName = traditionProp.Name;
            var tradition = ParseTradition(traditionName);
            
            var traditionData = traditionProp.Value as JObject;
            if (traditionData == null) continue;

            var items = traditionData["items"] as JArray;
            if (items == null) continue;

            if (!_spellsByTradition.ContainsKey(tradition))
            {
                _spellsByTradition[tradition] = new List<string>();
            }

            foreach (var item in items)
            {
                try
                {
                    var spell = ParseSpell(item, tradition);
                    if (spell != null)
                    {
                        _spells[spell.SpellId] = spell;
                        _spellsByTradition[tradition].Add(spell.SpellId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse spell: {Item}", item);
                }
            }
        }
    }

    private Spell? ParseSpell(JToken item, MagicalTradition tradition)
    {
        var slug = item["slug"]?.ToString();
        if (string.IsNullOrEmpty(slug))
        {
            _logger.LogWarning("Spell missing slug, skipping");
            return null;
        }

        var spell = new Spell
        {
            SpellId = slug,
            Name = item["name"]?.ToString() ?? slug,
            DisplayName = item["displayName"]?.ToString() ?? item["name"]?.ToString() ?? slug,
            Description = item["description"]?.ToString() ?? "",
            Tradition = tradition,
            Rank = item["rank"]?.Value<int>() ?? 0,
            SelectionWeight = item["selectionWeight"]?.Value<int>() ?? 50
        };

        // Parse traits
        var traits = item["traits"] as JObject;
        if (traits != null)
        {
            spell.MinimumSkillRank = GetTraitValueInt(traits, "minimumSkillRank", 0);
            spell.ManaCost = GetTraitValueInt(traits, "manaCost", 0);
            spell.Range = GetTraitValueInt(traits, "range", 0);
            spell.AreaOfEffect = GetTraitValueInt(traits, "areaOfEffect", 0);
            spell.Duration = GetTraitValueInt(traits, "duration", 0);
            spell.Cooldown = GetTraitValueInt(traits, "cooldown", 0);
            spell.BaseEffectValue = GetTraitValueString(traits, "baseDamage", "");
            
            if (string.IsNullOrEmpty(spell.BaseEffectValue))
            {
                spell.BaseEffectValue = GetTraitValueString(traits, "baseHealing", "");
            }

            // Parse effect type
            var damageType = GetTraitValueString(traits, "damageType", "");
            var effectType = GetTraitValueString(traits, "effectType", "");
            
            if (!string.IsNullOrEmpty(damageType))
            {
                spell.EffectType = SpellEffectType.Damage;
            }
            else if (effectType.Contains("heal", StringComparison.OrdinalIgnoreCase))
            {
                spell.EffectType = SpellEffectType.Heal;
            }
            else if (effectType.Contains("buff", StringComparison.OrdinalIgnoreCase))
            {
                spell.EffectType = SpellEffectType.Buff;
            }
            else if (effectType.Contains("debuff", StringComparison.OrdinalIgnoreCase) || 
                     effectType.Contains("curse", StringComparison.OrdinalIgnoreCase))
            {
                spell.EffectType = SpellEffectType.Debuff;
            }
            else if (effectType.Contains("summon", StringComparison.OrdinalIgnoreCase))
            {
                spell.EffectType = SpellEffectType.Summon;
            }
            else if (effectType.Contains("control", StringComparison.OrdinalIgnoreCase))
            {
                spell.EffectType = SpellEffectType.Control;
            }
            else if (effectType.Contains("protect", StringComparison.OrdinalIgnoreCase) || 
                     effectType.Contains("shield", StringComparison.OrdinalIgnoreCase))
            {
                spell.EffectType = SpellEffectType.Protection;
            }
            else
            {
                spell.EffectType = SpellEffectType.Utility;
            }

            // Store all traits
            foreach (var trait in traits.Properties())
            {
                var traitValue = trait.Value as JObject;
                if (traitValue != null && traitValue["value"] != null)
                {
                    spell.Traits[trait.Name] = ExtractTraitValue(traitValue["value"]!);
                }
            }
        }

        return spell;
    }

    private MagicalTradition ParseTradition(string traditionName)
    {
        return traditionName.ToLower() switch
        {
            "arcane" => MagicalTradition.Arcane,
            "divine" => MagicalTradition.Divine,
            "occult" => MagicalTradition.Occult,
            "primal" => MagicalTradition.Primal,
            _ => MagicalTradition.Arcane
        };
    }

    private int GetTraitValueInt(JObject traits, string name, int defaultValue)
    {
        var trait = traits[name] as JObject;
        return trait?["value"]?.Value<int>() ?? defaultValue;
    }

    private string GetTraitValueString(JObject traits, string name, string defaultValue)
    {
        var trait = traits[name] as JObject;
        return trait?["value"]?.ToString() ?? defaultValue;
    }

    private object ExtractTraitValue(JToken value)
    {
        switch (value.Type)
        {
            case JTokenType.Integer:
                return value.Value<int>();
            case JTokenType.Float:
                return value.Value<double>();
            case JTokenType.String:
                return value.Value<string>() ?? "";
            case JTokenType.Boolean:
                return value.Value<bool>();
            default:
                return value.ToString();
        }
    }

    private void EnsureInitialized()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("SpellCatalogService not initialized. Call InitializeAsync() first.");
        }
    }
}
