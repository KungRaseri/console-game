# Spells System Architecture Design

**Status**: Design Document  
**Purpose**: Technical specification for the learnable magic system with Pathfinder 2e magical traditions

**Related Documents**:
- [spells-system.md](../features/spells-system.md) - Feature overview
- [skills-system-design.md](./skills-system-design.md) - Magic skill architecture
- [abilities-system.md](../features/abilities-system.md) - Distinction from class abilities
- [ROADMAP.md](../ROADMAP.md) - Development priorities

---

## Executive Summary

**Current State**: Spell system redesigned to use Pathfinder 2e magical traditions instead of Elder Scrolls schools.

**Target State**: Comprehensive learnable magic system where spells are acquired through spellbooks, scrolls, trainers, and quests. Spell effectiveness scales with magical tradition skills. Four magical traditions (Arcane, Divine, Occult, Primal) with spell ranks from Cantrip (0) to 10th rank.

**Scope**: Complete spell system infrastructure
- Data models for spells, spellbooks, and character spell knowledge
- JSON catalog structure for spell definitions across all traditions
- Spell acquisition system (spellbooks, scrolls, trainers)
- Casting mechanics with skill checks, success rates, and failure modes
- Spell effect calculation scaling with tradition skills
- Integration with combat, inventory, and skills systems
- Mana cost calculation with skill-based efficiency
- Cantrips (rank 0 spells) are free to cast

**Magical Traditions** (Pathfinder 2e):
- **Arcane**: Study-based magic (raw power, force, transmutation)
- **Divine**: Faith-based magic (healing, protection, holy power)
- **Occult**: Mental/psychic magic (mind control, fear, illusions)
- **Primal**: Nature-based magic (elements, beasts, growth)

**Out of Scope** (Future Enhancements):
- Heightening (casting spells at higher ranks)
- Spell crafting/customization
- Spell combinations and synergies
- Metamagic modifiers
- Ritual spells requiring preparation
- Spell research and discovery

---

## 1. Data Model Architecture

### 1.1 Spell Model (RealmEngine.Shared.Models.Spell.cs)

```csharp
namespace RealmEngine.Shared.Models;

/// <summary>
/// Defines a spell's properties and requirements.
/// </summary>
public class Spell
{
    /// <summary>
    /// Unique identifier matching JSON catalog (e.g., "fireball", "greater-heal").
    /// </summary>
    public required string SpellId { get; set; }
    
    /// <summary>
    /// Display name for UI.
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Full description of spell effects.
    /// </summary>
    public required string Description { get; set; }
    
    /// <summary>
    /// Magical tradition (Arcane, Divine, Occult, Primal).
    /// </summary>
    public required MagicalTradition Tradition { get; set; }
    
    /// <summary>
    /// Spell rank: 0 (Cantrip) through 10.
    /// </summary>
    public required int Rank { get; set; }
    
    /// <summary>
    /// Display name for rank (e.g., "Cantrip", "1st", "3rd").
    /// </summary>
    public required string RankName { get; set; }
    
    /// <summary>
    /// Minimum tradition skill rank required to learn (0-100).
    /// </summary>
    public int MinimumSkillRank { get; set; }
    
    /// <summary>
    /// Base mana cost before skill modifiers (0 for cantrips).
    /// </summary>
    public int BaseManaCost { get; set; }
    
    /// <summary>
    /// Spell effect type (Damage, Heal, Buff, Summon, etc.).
    /// </summary>
    public SpellEffectType EffectType { get; set; }
    
    /// <summary>
    /// Base effect value (damage amount, healing amount, etc.).
    /// </summary>
    public int BaseEffectValue { get; set; }
    
    /// <summary>
    /// Area of effect radius in meters (0 for single target).
    /// </summary>
    public int AreaOfEffect { get; set; } = 0;
    
    /// <summary>
    /// Duration in turns (0 for instant effects).
    /// </summary>
    public int Duration { get; set; } = 0;
    
    /// <summary>
    /// Cooldown in turns (0 for no cooldown).
    /// </summary>
    public int Cooldown { get; set; } = 0;
    
    /// <summary>
    /// Can this spell be cast outside of combat?
    /// </summary>
    public bool CanCastOutOfCombat { get; set; } = false;
    
    /// <summary>
    /// Rarity weight for spellbook/scroll generation (1-100).
    /// </summary>
    public int RarityWeight { get; set; } = 50;
}

public enum SpellSchool
{
    Destruction,    // Offensive magic (fire, ice, lightning, arcane)
    Restoration,    // Healing and curing
    Alteration,     // Buffs, shields, transmutation
    Conjuration,    // Summoning creatures and objects
    Illusion,       // Mind magic (charm, fear, invisibility)
    Mysticism       // Detection, teleportation, divination
}

public enum SpellTier
{
    Novice,         // Rank 0-20
    Apprentice,     // Rank 20-40
    Adept,          // Rank 40-60
    Expert,         // Rank 60-80
    Master          // Rank 80-100
}

public enum SpellEffectType
{
    Damage,             // Direct damage (HP reduction)
    DamageOverTime,     // DoT effect (burning, bleeding)
    Heal,               // Restore HP
    HealOverTime,       // HoT effect (regeneration)
    Buff,               // Increase stats temporarily
    Debuff,             // Decrease enemy stats
    Shield,             // Absorb damage
    Summon,             // Summon creature ally
    CrowdControl,       // Stun, freeze, fear
    Dispel,             // Remove effects
    Utility             // Non-combat effects (light, detect)
}
```

### 1.2 Character Spell Knowledge (RealmEngine.Shared.Models.Character.cs)

Add to Character model:

```csharp
/// <summary>
/// Spells the character has learned, keyed by SpellId.
/// </summary>
public Dictionary<string, CharacterSpell> LearnedSpells { get; set; } = new();

/// <summary>
/// Spell cooldowns remaining (SpellId → turns remaining).
/// </summary>
public Dictionary<string, int> SpellCooldowns { get; set; } = new();
```

### 1.3 Character Spell (RealmEngine.Shared.Models.CharacterSpell.cs)

```csharp
namespace RealmEngine.Shared.Models;

/// <summary>
/// Represents a spell a character has learned.
/// </summary>
public class CharacterSpell
{
    /// <summary>
    /// Reference to spell definition.
    /// </summary>
    public required string SpellId { get; set; }
    
    /// <summary>
    /// When was this spell learned (for statistics).
    /// </summary>
    public DateTime LearnedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Number of times successfully cast.
    /// </summary>
    public int TimesCast { get; set; } = 0;
    
    /// <summary>
    /// Number of times casting failed.
    /// </summary>
    public int TimesFizzled { get; set; } = 0;
    
    /// <summary>
    /// Is this spell favorited for quick access?
    /// </summary>
    public bool IsFavorite { get; set; } = false;
}
```

### 1.4 Spellbook Item (RealmEngine.Shared.Models.Items.Spellbook.cs)

```csharp
namespace RealmEngine.Shared.Models.Items;

/// <summary>
/// Consumable item that teaches a spell.
/// </summary>
public class Spellbook : Item
{
    /// <summary>
    /// Spell this spellbook teaches.
    /// </summary>
    public required string SpellId { get; set; }
    
    /// <summary>
    /// Can only be used once (consumed on learning).
    /// </summary>
    public override bool IsConsumable => true;
}
```

### 1.5 Scroll Item (RealmEngine.Shared.Models.Items.Scroll.cs)

```csharp
namespace RealmEngine.Shared.Models.Items;

/// <summary>
/// Single-use item that casts a spell without knowing it.
/// </summary>
public class Scroll : Item
{
    /// <summary>
    /// Spell this scroll casts.
    /// </summary>
    public required string SpellId { get; set; }
    
    /// <summary>
    /// Consumed when used.
    /// </summary>
    public override bool IsConsumable => true;
    
    /// <summary>
    /// Number of charges (typically 1).
    /// </summary>
    public int Charges { get; set; } = 1;
}
```

---

## 2. JSON Catalog Structure

### 2.1 Spells Catalog (Data/spells/catalog.json)

```json
{
  "metadata": {
    "version": "1.0",
    "type": "spells_catalog",
    "description": "All learnable spells organized by school",
    "totalSpells": 30,
    "lastUpdated": "Current Date"
  },
  "schools": {
    "destruction": [
      {
        "id": "fireball",
        "name": "Fireball",
        "description": "Hurl a ball of fire that explodes on impact, damaging all enemies in area",
        "tier": "Novice",
        "minimumSkillRank": 0,
        "baseManaСost": 15,
        "effectType": "Damage",
        "baseEffectValue": 30,
        "damageElement": "Fire",
        "areaOfEffect": 5,
        "duration": 0,
        "cooldown": 0,
        "canCastOutOfCombat": false,
        "rarityWeight": 80
      },
      {
        "id": "flame-wall",
        "name": "Flame Wall",
        "description": "Create a wall of fire that damages and applies burning DoT",
        "tier": "Apprentice",
        "minimumSkillRank": 20,
        "baseManaСost": 30,
        "effectType": "DamageOverTime",
        "baseEffectValue": 50,
        "damageElement": "Fire",
        "dotDamagePerTurn": 10,
        "areaOfEffect": 8,
        "duration": 3,
        "cooldown": 2,
        "canCastOutOfCombat": false,
        "rarityWeight": 60
      },
      {
        "id": "ice-spike",
        "name": "Ice Spike",
        "description": "Launch a spike of ice that damages and slows target",
        "tier": "Novice",
        "minimumSkillRank": 0,
        "baseManaСost": 12,
        "effectType": "Damage",
        "baseEffectValue": 25,
        "damageElement": "Ice",
        "secondaryEffect": "Slow",
        "areaOfEffect": 0,
        "duration": 0,
        "cooldown": 0,
        "canCastOutOfCombat": false,
        "rarityWeight": 80
      },
      {
        "id": "chain-lightning",
        "name": "Chain Lightning",
        "description": "Strike target with lightning that chains to nearby enemies",
        "tier": "Expert",
        "minimumSkillRank": 60,
        "baseManaСost": 75,
        "effectType": "Damage",
        "baseEffectValue": 150,
        "damageElement": "Lightning",
        "chainTargets": 3,
        "areaOfEffect": 0,
        "duration": 0,
        "cooldown": 3,
        "canCastOutOfCombat": false,
        "rarityWeight": 20
      },
      {
        "id": "meteor",
        "name": "Meteor",
        "description": "Call down a meteor from the sky for massive AoE damage",
        "tier": "Master",
        "minimumSkillRank": 80,
        "baseManaСost": 120,
        "effectType": "Damage",
        "baseEffectValue": 300,
        "damageElement": "Fire",
        "areaOfEffect": 15,
        "duration": 0,
        "cooldown": 5,
        "canCastOutOfCombat": false,
        "rarityWeight": 5
      }
    ],
    "restoration": [
      {
        "id": "healing",
        "name": "Healing",
        "description": "Restore health to target ally",
        "tier": "Novice",
        "minimumSkillRank": 0,
        "baseManaСost": 12,
        "effectType": "Heal",
        "baseEffectValue": 40,
        "areaOfEffect": 0,
        "duration": 0,
        "cooldown": 0,
        "canCastOutOfCombat": true,
        "rarityWeight": 90
      },
      {
        "id": "greater-heal",
        "name": "Greater Heal",
        "description": "Restore significant health to target",
        "tier": "Apprentice",
        "minimumSkillRank": 20,
        "baseManaСost": 28,
        "effectType": "Heal",
        "baseEffectValue": 100,
        "areaOfEffect": 0,
        "duration": 0,
        "cooldown": 0,
        "canCastOutOfCombat": true,
        "rarityWeight": 70
      },
      {
        "id": "regeneration",
        "name": "Regeneration",
        "description": "Target heals over time for multiple turns",
        "tier": "Apprentice",
        "minimumSkillRank": 25,
        "baseManaСost": 25,
        "effectType": "HealOverTime",
        "baseEffectValue": 20,
        "healPerTurn": 20,
        "areaOfEffect": 0,
        "duration": 5,
        "cooldown": 1,
        "canCastOutOfCombat": true,
        "rarityWeight": 65
      },
      {
        "id": "full-heal",
        "name": "Full Heal",
        "description": "Restore target to maximum health",
        "tier": "Adept",
        "minimumSkillRank": 40,
        "baseManaСost": 50,
        "effectType": "Heal",
        "baseEffectValue": 9999,
        "areaOfEffect": 0,
        "duration": 0,
        "cooldown": 3,
        "canCastOutOfCombat": true,
        "rarityWeight": 40
      },
      {
        "id": "cure-poison",
        "name": "Cure Poison",
        "description": "Remove poison effects from target",
        "tier": "Novice",
        "minimumSkillRank": 5,
        "baseManaСost": 10,
        "effectType": "Dispel",
        "dispelType": "Poison",
        "areaOfEffect": 0,
        "duration": 0,
        "cooldown": 0,
        "canCastOutOfCombat": true,
        "rarityWeight": 75
      }
    ],
    "alteration": [
      {
        "id": "mana-shield",
        "name": "Mana Shield",
        "description": "Create a magical barrier that absorbs damage",
        "tier": "Novice",
        "minimumSkillRank": 0,
        "baseManaСost": 20,
        "effectType": "Shield",
        "baseEffectValue": 50,
        "areaOfEffect": 0,
        "duration": 5,
        "cooldown": 2,
        "canCastOutOfCombat": false,
        "rarityWeight": 80
      },
      {
        "id": "stoneskin",
        "name": "Stoneskin",
        "description": "Harden skin to increase defense dramatically",
        "tier": "Apprentice",
        "minimumSkillRank": 20,
        "baseManaСost": 35,
        "effectType": "Buff",
        "buffType": "Defense",
        "baseEffectValue": 50,
        "areaOfEffect": 0,
        "duration": 5,
        "cooldown": 2,
        "canCastOutOfCombat": false,
        "rarityWeight": 60
      },
      {
        "id": "strength-of-steel",
        "name": "Strength of Steel",
        "description": "Increase physical damage dealt",
        "tier": "Adept",
        "minimumSkillRank": 40,
        "baseManaСost": 45,
        "effectType": "Buff",
        "buffType": "Damage",
        "baseEffectValue": 50,
        "areaOfEffect": 0,
        "duration": 5,
        "cooldown": 2,
        "canCastOutOfCombat": false,
        "rarityWeight": 50
      },
      {
        "id": "feather-fall",
        "name": "Feather Fall",
        "description": "Fall slowly, preventing fall damage",
        "tier": "Novice",
        "minimumSkillRank": 10,
        "baseManaСost": 8,
        "effectType": "Utility",
        "utilityType": "Exploration",
        "areaOfEffect": 0,
        "duration": 10,
        "cooldown": 0,
        "canCastOutOfCombat": true,
        "rarityWeight": 70
      }
    ],
    "conjuration": [
      {
        "id": "summon-wolf",
        "name": "Summon Wolf",
        "description": "Summon a spectral wolf to fight alongside you",
        "tier": "Novice",
        "minimumSkillRank": 0,
        "baseManaСost": 25,
        "effectType": "Summon",
        "summonCreatureId": "spectral-wolf",
        "areaOfEffect": 0,
        "duration": 5,
        "cooldown": 3,
        "canCastOutOfCombat": false,
        "rarityWeight": 75
      },
      {
        "id": "summon-bear",
        "name": "Summon Bear",
        "description": "Summon a powerful bear ally",
        "tier": "Apprentice",
        "minimumSkillRank": 20,
        "baseManaСost": 45,
        "effectType": "Summon",
        "summonCreatureId": "spectral-bear",
        "areaOfEffect": 0,
        "duration": 5,
        "cooldown": 4,
        "canCastOutOfCombat": false,
        "rarityWeight": 55
      },
      {
        "id": "summon-elemental",
        "name": "Summon Elemental",
        "description": "Summon a powerful elemental creature",
        "tier": "Adept",
        "minimumSkillRank": 40,
        "baseManaСost": 70,
        "effectType": "Summon",
        "summonCreatureId": "fire-elemental",
        "areaOfEffect": 0,
        "duration": 8,
        "cooldown": 5,
        "canCastOutOfCombat": false,
        "rarityWeight": 40
      },
      {
        "id": "bound-sword",
        "name": "Bound Sword",
        "description": "Conjure a magical sword from thin air",
        "tier": "Novice",
        "minimumSkillRank": 5,
        "baseManaСost": 15,
        "effectType": "Summon",
        "summonItemType": "Weapon",
        "areaOfEffect": 0,
        "duration": 10,
        "cooldown": 2,
        "canCastOutOfCombat": true,
        "rarityWeight": 70
      }
    ],
    "illusion": [
      {
        "id": "invisibility",
        "name": "Invisibility",
        "description": "Become invisible to enemies",
        "tier": "Novice",
        "minimumSkillRank": 0,
        "baseManaСost": 18,
        "effectType": "Buff",
        "buffType": "Stealth",
        "baseEffectValue": 100,
        "areaOfEffect": 0,
        "duration": 30,
        "cooldown": 3,
        "canCastOutOfCombat": true,
        "rarityWeight": 75
      },
      {
        "id": "charm-person",
        "name": "Charm Person",
        "description": "Make target friendly temporarily",
        "tier": "Apprentice",
        "minimumSkillRank": 20,
        "baseManaСost": 40,
        "effectType": "CrowdControl",
        "ccType": "Charm",
        "areaOfEffect": 0,
        "duration": 3,
        "cooldown": 4,
        "canCastOutOfCombat": false,
        "rarityWeight": 60
      },
      {
        "id": "fear",
        "name": "Fear",
        "description": "Frighten target, causing them to flee",
        "tier": "Novice",
        "minimumSkillRank": 10,
        "baseManaСost": 20,
        "effectType": "CrowdControl",
        "ccType": "Fear",
        "areaOfEffect": 0,
        "duration": 2,
        "cooldown": 2,
        "canCastOutOfCombat": false,
        "rarityWeight": 70
      },
      {
        "id": "mass-charm",
        "name": "Mass Charm",
        "description": "Charm all enemies in area",
        "tier": "Master",
        "minimumSkillRank": 80,
        "baseManaСost": 160,
        "effectType": "CrowdControl",
        "ccType": "Charm",
        "areaOfEffect": 10,
        "duration": 3,
        "cooldown": 6,
        "canCastOutOfCombat": false,
        "rarityWeight": 10
      }
    ],
    "mysticism": [
      {
        "id": "detect-life",
        "name": "Detect Life",
        "description": "Sense nearby living creatures",
        "tier": "Novice",
        "minimumSkillRank": 0,
        "baseManaСost": 10,
        "effectType": "Utility",
        "utilityType": "Detection",
        "detectionRange": 30,
        "areaOfEffect": 0,
        "duration": 10,
        "cooldown": 1,
        "canCastOutOfCombat": true,
        "rarityWeight": 80
      },
      {
        "id": "detect-magic",
        "name": "Detect Magic",
        "description": "Reveal magical items and enchantments",
        "tier": "Novice",
        "minimumSkillRank": 5,
        "baseManaСost": 12,
        "effectType": "Utility",
        "utilityType": "Detection",
        "detectionType": "Magic",
        "areaOfEffect": 0,
        "duration": 15,
        "cooldown": 1,
        "canCastOutOfCombat": true,
        "rarityWeight": 75
      },
      {
        "id": "recall",
        "name": "Recall",
        "description": "Teleport back to last town visited",
        "tier": "Apprentice",
        "minimumSkillRank": 25,
        "baseManaСost": 50,
        "effectType": "Utility",
        "utilityType": "Teleportation",
        "areaOfEffect": 0,
        "duration": 0,
        "cooldown": 0,
        "canCastOutOfCombat": true,
        "rarityWeight": 50
      },
      {
        "id": "teleport",
        "name": "Teleport",
        "description": "Instantly travel to any known location",
        "tier": "Expert",
        "minimumSkillRank": 60,
        "baseManaСost": 80,
        "effectType": "Utility",
        "utilityType": "Teleportation",
        "areaOfEffect": 0,
        "duration": 0,
        "cooldown": 5,
        "canCastOutOfCombat": true,
        "rarityWeight": 25
      }
    ]
  }
}
```

---

## 3. Spell Acquisition System

### 3.1 Learning from Spellbooks

```csharp
public class SpellLearningService
{
    private readonly ISpellCatalogService _spellCatalog;
    private readonly ISkillProgressionService _skillProgression;
    
    /// <summary>
    /// Attempt to learn a spell from a spellbook.
    /// </summary>
    public SpellLearningResult LearnSpellFromBook(Character character, string spellId)
    {
        // Get spell definition
        var spell = _spellCatalog.GetSpell(spellId);
        if (spell == null)
        {
            return new SpellLearningResult 
            { 
                Success = false, 
                Message = "Spellbook is corrupted or unreadable." 
            };
        }
        
        // Check if already learned
        if (character.LearnedSpells.ContainsKey(spellId))
        {
            return new SpellLearningResult 
            { 
                Success = false, 
                Message = $"You already know {spell.Name}." 
            };
        }
        
        // Check skill requirement
        var schoolSkillId = GetSchoolSkillId(spell.School);
        if (!character.Skills.TryGetValue(schoolSkillId, out var magicSkill))
        {
            return new SpellLearningResult 
            { 
                Success = false, 
                Message = "You have no magical training." 
            };
        }
        
        if (magicSkill.CurrentRank < spell.MinimumSkillRank)
        {
            return new SpellLearningResult 
            { 
                Success = false, 
                Message = $"{spell.Name} requires {spell.School} skill rank {spell.MinimumSkillRank}, but you only have rank {magicSkill.CurrentRank}." 
            };
        }
        
        // Learn the spell!
        character.LearnedSpells[spellId] = new CharacterSpell
        {
            SpellId = spellId,
            LearnedDate = DateTime.UtcNow
        };
        
        return new SpellLearningResult 
        { 
            Success = true, 
            Message = $"You learned {spell.Name}!" 
        };
    }
    
    private string GetSchoolSkillId(SpellSchool school)
    {
        return school switch
        {
            SpellSchool.Destruction => "destruction",
            SpellSchool.Restoration => "restoration",
            SpellSchool.Alteration => "alteration",
            SpellSchool.Conjuration => "conjuration",
            SpellSchool.Illusion => "illusion",
            SpellSchool.Mysticism => "mysticism",
            _ => throw new ArgumentException($"Unknown school: {school}")
        };
    }
}

public class SpellLearningResult
{
    public bool Success { get; set; }
    public required string Message { get; set; }
}
```

### 3.2 Casting from Scrolls

```csharp
/// <summary>
/// Cast a spell from a scroll without knowing it.
/// Scrolls always succeed (no skill check).
/// </summary>
public SpellCastResult CastFromScroll(Character caster, Scroll scroll, CombatContext context)
{
    var spell = _spellCatalog.GetSpell(scroll.SpellId);
    if (spell == null)
    {
        return new SpellCastResult 
        { 
            Success = false, 
            Message = "The scroll crumbles to dust as you try to read it." 
        };
    }
    
    // Scrolls always succeed (no fizzle chance)
    var result = ExecuteSpellEffect(caster, spell, context, skillModifier: 0);
    
    // Consume scroll charge
    scroll.Charges--;
    
    return result;
}
```

---

## 4. Casting Mechanics

### 4.1 Cast Spell (Main Logic)

```csharp
public class SpellCastingService
{
    private readonly ISpellCatalogService _spellCatalog;
    private readonly ISkillProgressionService _skillProgression;
    
    /// <summary>
    /// Attempt to cast a learned spell.
    /// </summary>
    public SpellCastResult CastSpell(
        Character caster, 
        string spellId, 
        ICombatTarget target, 
        CombatContext context)
    {
        // Verify spell is known
        if (!caster.LearnedSpells.ContainsKey(spellId))
        {
            return new SpellCastResult 
            { 
                Success = false, 
                Message = "You don't know that spell!" 
            };
        }
        
        var spell = _spellCatalog.GetSpell(spellId);
        if (spell == null)
        {
            return new SpellCastResult { Success = false, Message = "Spell not found!" };
        }
        
        // Check cooldown
        if (caster.SpellCooldowns.TryGetValue(spellId, out var cooldownRemaining) && cooldownRemaining > 0)
        {
            return new SpellCastResult 
            { 
                Success = false, 
                Message = $"{spell.Name} is still cooling down ({cooldownRemaining} turns)." 
            };
        }
        
        // Calculate actual mana cost (reduced by skill)
        var magicSkill = GetMagicSkill(caster, spell.School);
        var actualManaCost = CalculateManaCost(spell, magicSkill);
        
        // Check mana
        if (caster.CurrentMana < actualManaCost)
        {
            return new SpellCastResult 
            { 
                Success = false, 
                Message = $"Not enough mana! {spell.Name} requires {actualManaCost} mana." 
            };
        }
        
        // Consume mana
        caster.CurrentMana -= actualManaCost;
        
        // Success check (based on skill vs requirement)
        var successResult = CheckCastSuccess(magicSkill, spell);
        
        if (!successResult.Success)
        {
            // Fizzle - spell fails but half mana is still consumed (already deducted above)
            caster.CurrentMana += actualManaCost / 2; // Refund half
            caster.LearnedSpells[spellId].TimesFizzled++;
            
            // Award small XP for trying
            _skillProgression.AwardSkillXP(caster, GetSchoolSkillId(spell.School), 2);
            
            return new SpellCastResult 
            { 
                Success = false, 
                Message = $"{spell.Name} fizzled! You couldn't maintain the spell." 
            };
        }
        
        // Cast successful! Execute effect
        var castResult = ExecuteSpellEffect(caster, spell, target, context, magicSkill.CurrentRank);
        
        // Update statistics
        caster.LearnedSpells[spellId].TimesCast++;
        
        // Award skill XP
        var xpAmount = CalculateSpellXP(spell);
        _skillProgression.AwardSkillXP(caster, GetSchoolSkillId(spell.School), xpAmount);
        
        // Apply cooldown
        if (spell.Cooldown > 0)
        {
            caster.SpellCooldowns[spellId] = spell.Cooldown;
        }
        
        return castResult;
    }
    
    /// <summary>
    /// Calculate mana cost with skill efficiency reduction.
    /// </summary>
    private int CalculateManaCost(Spell spell, CharacterSkill magicSkill)
    {
        // Skill reduces cost up to 50% at rank 100
        var ranksAboveRequirement = Math.Max(0, magicSkill.CurrentRank - spell.MinimumSkillRank);
        var costReduction = Math.Min(0.5, ranksAboveRequirement * 0.005); // -0.5% per rank, max 50%
        
        return (int)(spell.BaseManaСost * (1.0 - costReduction));
    }
    
    /// <summary>
    /// Check if spell cast succeeds.
    /// </summary>
    private CastSuccessResult CheckCastSuccess(CharacterSkill magicSkill, Spell spell)
    {
        var rankDifference = magicSkill.CurrentRank - spell.MinimumSkillRank;
        
        // Success rate formula:
        // - At minimum rank: 90% success
        // - 20 ranks above: 99% success
        // - 10 ranks below: 60% success (risky but possible)
        var baseSuccessRate = 0.90;
        var successRate = baseSuccessRate + (rankDifference * 0.005); // +0.5% per rank above requirement
        successRate = Math.Clamp(successRate, 0.60, 0.99);
        
        var roll = Random.Shared.NextDouble();
        var succeeded = roll < successRate;
        
        return new CastSuccessResult 
        { 
            Success = succeeded,
            SuccessRate = successRate
        };
    }
    
    /// <summary>
    /// Calculate spell XP award.
    /// </summary>
    private int CalculateSpellXP(Spell spell)
    {
        return spell.Tier switch
        {
            SpellTier.Novice => 8,
            SpellTier.Apprentice => 12,
            SpellTier.Adept => 18,
            SpellTier.Expert => 25,
            SpellTier.Master => 35,
            _ => 8
        };
    }
}
```

### 4.2 Spell Effect Execution

```csharp
/// <summary>
/// Execute the spell's effect with skill scaling.
/// </summary>
private SpellCastResult ExecuteSpellEffect(
    Character caster, 
    Spell spell, 
    ICombatTarget target, 
    CombatContext context,
    int skillRank)
{
    // Calculate skill scaling: +1% per rank above requirement
    var ranksAboveRequirement = Math.Max(0, skillRank - spell.MinimumSkillRank);
    var powerMultiplier = 1.0 + (ranksAboveRequirement * 0.01);
    
    switch (spell.EffectType)
    {
        case SpellEffectType.Damage:
            return ExecuteDamage(caster, spell, target, context, powerMultiplier);
            
        case SpellEffectType.Heal:
            return ExecuteHeal(caster, spell, target, powerMultiplier);
            
        case SpellEffectType.Buff:
            return ExecuteBuff(caster, spell, target, context, powerMultiplier);
            
        case SpellEffectType.Summon:
            return ExecuteSummon(caster, spell, context);
            
        case SpellEffectType.CrowdControl:
            return ExecuteCrowdControl(caster, spell, target, context);
            
        case SpellEffectType.Utility:
            return ExecuteUtility(caster, spell, context);
            
        default:
            throw new NotImplementedException($"Effect type {spell.EffectType} not implemented");
    }
}

private SpellCastResult ExecuteDamage(
    Character caster, 
    Spell spell, 
    ICombatTarget target, 
    CombatContext context, 
    double powerMultiplier)
{
    var damage = (int)(spell.BaseEffectValue * powerMultiplier);
    
    if (spell.AreaOfEffect > 0)
    {
        // AoE damage - hit all enemies
        var enemiesHit = context.GetEnemiesInArea(spell.AreaOfEffect);
        foreach (var enemy in enemiesHit)
        {
            enemy.TakeDamage(damage);
        }
        
        return new SpellCastResult 
        { 
            Success = true, 
            Message = $"{spell.Name} hits {enemiesHit.Count} enemies for {damage} damage each!",
            TotalDamage = damage * enemiesHit.Count
        };
    }
    else
    {
        // Single target damage
        target.TakeDamage(damage);
        
        return new SpellCastResult 
        { 
            Success = true, 
            Message = $"{spell.Name} hits {target.Name} for {damage} damage!",
            TotalDamage = damage
        };
    }
}

private SpellCastResult ExecuteHeal(
    Character caster, 
    Spell spell, 
    ICombatTarget target, 
    double powerMultiplier)
{
    var healing = (int)(spell.BaseEffectValue * powerMultiplier);
    
    // Full Heal special case
    if (spell.SpellId == "full-heal" && target is Character character)
    {
        healing = character.MaxHealth - character.CurrentHealth;
    }
    
    target.Heal(healing);
    
    return new SpellCastResult 
    { 
        Success = true, 
        Message = $"{spell.Name} restores {healing} HP to {target.Name}!",
        TotalHealing = healing
    };
}
```

---

## 5. Skill Scaling System

### 5.1 Damage Spell Scaling

**Formula**: `finalDamage = baseEffectValue × (1.0 + (skillRank - minimumSkillRank) × 0.01)`

**Examples**:
- **Fireball** (base 30 damage, requires Rank 0):
  - Rank 0: 30 damage (1.0×)
  - Rank 20: 36 damage (1.2×)
  - Rank 50: 45 damage (1.5×)
  - Rank 100: 60 damage (2.0×)

- **Chain Lightning** (base 150 damage, requires Rank 60):
  - Rank 60: 150 damage (1.0×)
  - Rank 80: 180 damage (1.2×)
  - Rank 100: 210 damage (1.4×)

### 5.2 Healing Spell Scaling

**Formula**: Same as damage scaling

**Examples**:
- **Healing** (base 40 HP, requires Rank 0):
  - Rank 0: 40 HP
  - Rank 50: 60 HP
  - Rank 100: 80 HP

### 5.3 Mana Cost Reduction

**Formula**: `actualCost = baseCost × (1.0 - min(0.50, (skillRank - minimumSkillRank) × 0.005))`

**Examples**:
- **Fireball** (base 15 mana, requires Rank 0):
  - Rank 0: 15 mana
  - Rank 50: 11 mana (25% reduction)
  - Rank 100: 8 mana (50% reduction cap)

### 5.4 Success Rate Scaling

**Formula**: `successRate = 0.90 + (skillRank - minimumSkillRank) × 0.005` (clamped 0.60-0.99)

**Examples**:
- At minimum rank: 90% success
- 10 ranks above: 95% success
- 20+ ranks above: 99% success (cap)
- 10 ranks below: 60% success (risky)

---

## 6. Integration Points

### 6.1 Combat System Integration

```csharp
// In CombatService.cs

public async Task<CombatResult> PlayerCastSpellAsync(string spellId, string targetId)
{
    var target = GetTargetById(targetId);
    
    var castResult = _spellCastingService.CastSpell(
        _player, 
        spellId, 
        target, 
        _combatContext);
    
    if (!castResult.Success)
    {
        return new CombatResult 
        { 
            Message = castResult.Message, 
            ContinueCombat = true 
        };
    }
    
    // Spell succeeded - enemy turn next
    await ProcessEnemyTurnsAsync();
    
    // Decrement spell cooldowns
    DecrementSpellCooldowns(_player);
    
    return new CombatResult 
    { 
        Message = castResult.Message, 
        ContinueCombat = !_allEnemiesDefeated 
    };
}

private void DecrementSpellCooldowns(Character character)
{
    var spellsOnCooldown = character.SpellCooldowns.Keys.ToList();
    foreach (var spellId in spellsOnCooldown)
    {
        character.SpellCooldowns[spellId]--;
        if (character.SpellCooldowns[spellId] <= 0)
        {
            character.SpellCooldowns.Remove(spellId);
        }
    }
}
```

### 6.2 Inventory Integration

```csharp
// In InventoryService.cs

public async Task<UseItemResult> UseSpellbookAsync(Character character, Spellbook spellbook)
{
    var result = _spellLearningService.LearnSpellFromBook(character, spellbook.SpellId);
    
    if (result.Success)
    {
        // Remove spellbook from inventory (consumed)
        character.Inventory.RemoveItem(spellbook);
    }
    
    return new UseItemResult 
    { 
        Success = result.Success, 
        Message = result.Message 
    };
}

public async Task<UseItemResult> UseScrollAsync(Character character, Scroll scroll, CombatContext context)
{
    var result = _spellCastingService.CastFromScroll(character, scroll, context);
    
    if (scroll.Charges <= 0)
    {
        // Remove empty scroll
        character.Inventory.RemoveItem(scroll);
    }
    
    return new UseItemResult 
    { 
        Success = result.Success, 
        Message = result.Message 
    };
}
```

### 6.3 Skills System Integration

Magic skills from Skills System affect spell effectiveness:

**Skills Used**:
- `destruction` (Destruction school spells)
- `restoration` (Restoration school spells)
- `alteration` (Alteration school spells)
- `conjuration` (Conjuration school spells)
- `illusion` (Illusion school spells)
- `mysticism` (Mysticism school spells)

**XP Awards**:
- Cast spell: 8-35 XP (based on tier)
- Spell fizzles: 2 XP (consolation prize)

---

## 7. Service Architecture

### 7.1 SpellCatalogService

```csharp
public interface ISpellCatalogService
{
    Spell? GetSpell(string spellId);
    List<Spell> GetSpellsBySchool(SpellSchool school);
    List<Spell> GetSpellsByTier(SpellTier tier);
    List<Spell> GetLearnableSpells(Character character);
}

public class SpellCatalogService : ISpellCatalogService
{
    private Dictionary<string, Spell> _spellCache = new();
    
    public SpellCatalogService()
    {
        LoadSpellsFromJson();
    }
    
    private void LoadSpellsFromJson()
    {
        var json = File.ReadAllText("Data/spells/catalog.json");
        var catalog = JsonSerializer.Deserialize<SpellsCatalog>(json);
        
        foreach (var school in catalog.Schools.Values)
        {
            foreach (var spell in school)
            {
                _spellCache[spell.SpellId] = spell;
            }
        }
    }
    
    public Spell? GetSpell(string spellId)
    {
        _spellCache.TryGetValue(spellId, out var spell);
        return spell;
    }
    
    public List<Spell> GetLearnableSpells(Character character)
    {
        // Return spells character can learn (meets requirements, doesn't know yet)
        return _spellCache.Values
            .Where(s => !character.LearnedSpells.ContainsKey(s.SpellId))
            .Where(s => character.Skills.TryGetValue(GetSchoolSkillId(s.School), out var skill) 
                        && skill.CurrentRank >= s.MinimumSkillRank)
            .ToList();
    }
}
```

### 7.2 SpellLearningService

Handles learning spells from spellbooks and trainers (shown earlier in section 3.1).

### 7.3 SpellCastingService

Handles casting mechanics, success checks, mana costs (shown earlier in section 4).

---

## 8. Testing Strategy

### 8.1 Unit Tests

```csharp
[Fact]
public void CastSpell_WithInsufficientMana_ShouldFail()
{
    // Arrange
    var character = new Character { CurrentMana = 5 };
    var spell = new Spell { SpellId = "fireball", BaseManaСost = 15 };
    
    // Act
    var result = _spellCastingService.CastSpell(character, "fireball", target, context);
    
    // Assert
    result.Success.Should().BeFalse();
    result.Message.Should().Contain("Not enough mana");
}

[Fact]
public void LearnSpell_WithInsufficientSkill_ShouldFail()
{
    // Arrange
    var character = new Character();
    character.Skills["destruction"] = new CharacterSkill 
    { 
        SkillId = "destruction", 
        CurrentRank = 10 
    };
    
    // Act (trying to learn Expert spell with Rank 10)
    var result = _spellLearningService.LearnSpellFromBook(character, "chain-lightning");
    
    // Assert
    result.Success.Should().BeFalse();
    result.Message.Should().Contain("requires").And.Contain("rank 60");
}

[Theory]
[InlineData(0, 0.90)]    // At minimum rank: 90%
[InlineData(20, 1.00)]   // 20 above: capped at 99%
[InlineData(-10, 0.60)]  // 10 below: 60%
public void CalculateSuccessRate_ShouldScaleCorrectly(int rankDifference, double expectedRate)
{
    // Arrange
    var skill = new CharacterSkill { CurrentRank = 20 + rankDifference };
    var spell = new Spell { MinimumSkillRank = 20 };
    
    // Act
    var rate = _spellCastingService.CalculateSuccessRate(skill, spell);
    
    // Assert
    rate.Should().BeApproximately(expectedRate, 0.01);
}

[Fact]
public void SpellDamage_ShouldScaleWithSkillRank()
{
    // Arrange
    var spell = new Spell 
    { 
        BaseEffectValue = 30, 
        MinimumSkillRank = 0 
    };
    
    // Act
    var damageRank0 = CalculateDamage(spell, skillRank: 0);
    var damageRank50 = CalculateDamage(spell, skillRank: 50);
    var damageRank100 = CalculateDamage(spell, skillRank: 100);
    
    // Assert
    damageRank0.Should().Be(30);    // 1.0× multiplier
    damageRank50.Should().Be(45);   // 1.5× multiplier
    damageRank100.Should().Be(60);  // 2.0× multiplier
}
```

### 8.2 Integration Tests

```csharp
[Fact]
public async Task CombatWithSpells_ShouldAwardSkillXP()
{
    // Arrange
    var character = CreateTestCharacter();
    character.Skills["destruction"] = new CharacterSkill 
    { 
        SkillId = "destruction", 
        CurrentRank = 10, 
        CurrentXP = 0 
    };
    
    // Act
    await _combatService.PlayerCastSpellAsync("fireball", "enemy-1");
    
    // Assert
    character.Skills["destruction"].CurrentXP.Should().BeGreaterThan(0);
    character.Skills["destruction"].TotalXP.Should().Be(8); // Novice spell = 8 XP
}

[Fact]
public void LearnMultipleSpells_ShouldAllAppearInSpellbook()
{
    // Arrange
    var character = CreateTestCharacter();
    character.Skills["destruction"] = new CharacterSkill 
    { 
        CurrentRank = 50 
    };
    
    // Act
    _spellLearningService.LearnSpellFromBook(character, "fireball");
    _spellLearningService.LearnSpellFromBook(character, "ice-spike");
    _spellLearningService.LearnSpellFromBook(character, "flame-wall");
    
    // Assert
    character.LearnedSpells.Should().HaveCount(3);
    character.LearnedSpells.Should().ContainKeys("fireball", "ice-spike", "flame-wall");
}
```

---

## 9. UI/API Requirements

### 9.1 Query APIs

```csharp
// Get character's spellbook
public GetSpellbookResponse GetLearnedSpells(Character character)
{
    var spells = character.LearnedSpells.Select(kvp =>
    {
        var spell = _spellCatalog.GetSpell(kvp.Key);
        var characterSpell = kvp.Value;
        
        return new SpellInfo
        {
            SpellId = spell.SpellId,
            Name = spell.Name,
            Description = spell.Description,
            School = spell.School.ToString(),
            Tier = spell.Tier.ToString(),
            ManaCost = CalculateManaCost(spell, GetMagicSkill(character, spell.School)),
            TimesCast = characterSpell.TimesCast,
            IsFavorite = characterSpell.IsFavorite,
            CooldownRemaining = character.SpellCooldowns.GetValueOrDefault(spell.SpellId, 0)
        };
    }).ToList();
    
    return new GetSpellbookResponse { Spells = spells };
}

// Get castable spells (in combat)
public GetCastableSpellsResponse GetCastableSpells(Character character)
{
    var castable = character.LearnedSpells
        .Where(kvp => !character.SpellCooldowns.ContainsKey(kvp.Key) || character.SpellCooldowns[kvp.Key] <= 0)
        .Where(kvp =>
        {
            var spell = _spellCatalog.GetSpell(kvp.Key);
            var manaCost = CalculateManaCost(spell, GetMagicSkill(character, spell.School));
            return character.CurrentMana >= manaCost;
        })
        .Select(kvp => _spellCatalog.GetSpell(kvp.Key))
        .ToList();
    
    return new GetCastableSpellsResponse { Spells = castable };
}

// Get spells available to learn
public GetLearnableSpellsResponse GetLearnableSpells(Character character)
{
    var learnable = _spellCatalog.GetLearnableSpells(character);
    
    return new GetLearnableSpellsResponse { Spells = learnable };
}
```

### 9.2 Command APIs

```csharp
// Cast spell in combat
public async Task<CastSpellResponse> CastSpellAsync(CastSpellRequest request)
{
    var result = _spellCastingService.CastSpell(
        request.Character,
        request.SpellId,
        request.Target,
        request.CombatContext);
    
    return new CastSpellResponse
    {
        Success = result.Success,
        Message = result.Message,
        TotalDamage = result.TotalDamage,
        TotalHealing = result.TotalHealing
    };
}

// Learn spell from spellbook
public async Task<LearnSpellResponse> LearnSpellAsync(LearnSpellRequest request)
{
    var result = _spellLearningService.LearnSpellFromBook(
        request.Character,
        request.SpellId);
    
    return new LearnSpellResponse
    {
        Success = result.Success,
        Message = result.Message
    };
}
```

---

## 10. Performance Considerations

### 10.1 Spell Catalog Caching
- Load all spells from JSON once at startup
- Cache in `Dictionary<string, Spell>` for O(1) lookup
- No repeated JSON parsing during gameplay

### 10.2 Spell Lookup Optimization
- Character's `LearnedSpells` uses `Dictionary<string, CharacterSpell>` for O(1) access
- Spell cooldowns also use `Dictionary<string, int>`
- No linear searches through spell lists

### 10.3 Memory Usage
- ~30 spells in catalog × ~500 bytes per spell = ~15 KB
- Character learned spells: ~10 spells × ~100 bytes = ~1 KB per character
- Minimal memory footprint

---

## 11. Backwards Compatibility

### 11.1 Save File Migration

Since spells are a new system, no migration needed. New save fields:
- `LearnedSpells` dictionary (empty for old saves)
- `SpellCooldowns` dictionary (empty for old saves)

Old saves will load with empty spellbooks (expected behavior).

---

## 12. Future Enhancements

### Advanced Features:
- **Spell Combinations**: Cast multiple spells for synergy effects
- **Metamagic**: Modify spells on-the-fly (quicken, empower, widen)
- **Spell Customization**: Adjust damage/cost/range trade-offs
- **Ritual Spells**: Powerful spells requiring out-of-combat preparation
- **Spell Research**: Create new spells through experimentation
- **Spell Familiarity**: Cast often-used spells more reliably
- **Dual-Casting**: Cast same spell with both hands for increased power
- **Spell Absorption**: Absorb enemy spells to restore mana

### Extended Features:
- **Spell Schools Mastery**: Unlock special perks for 100% school completion
- **Forbidden Magic**: Dark spells with risks and consequences
- **Elemental Weaknesses**: Enemies weak/resistant to specific elements
- **Spell Scrolls Crafting**: Create scrolls from known spells
- **Spellcasting Animations**: Visual effects for different spell schools

---

## 13. Success Metrics

### Technical Metrics:
- All 30 spells load from JSON catalog
- Spell casting triggers correctly in combat
- Skill checks calculate accurately
- XP awards for spellcasting work
- Cooldowns decrement properly
- Mana costs reduce with skill
- No performance issues with spell system

### Gameplay Metrics:
- Players learn 3-5 spells by level 10
- Players regularly use spells in combat (not just melee)
- Spell damage competitive with weapon damage at equivalent skill ranks
- Magic skills progress through spellcasting
- Players invest in multiple magic schools
- Spellcasters feel distinct from melee fighters

---

## 14. Documentation Deliverables

- [ ] This design document
- [ ] API documentation for `SpellCastingService`
- [ ] API documentation for `SpellLearningService`
- [ ] JSON catalog schema documentation
- [ ] Integration guide for future spell features
- [ ] Player-facing spell guide (game manual)

---

## Implementation Readiness

**Design Status**: ✅ Ready for Implementation

**Dependencies**:
- Skills System must be implemented first (magic skills required)
- Combat System integration points needed
- Inventory System integration for spellbooks/scrolls

**Next Steps**:
1. Review this design document
2. Implement Skills System first (provides magic skill foundation)
3. Begin Spells System implementation with Data Models & JSON
4. Create feature branch: `feature/spells-system`
5. Implement iteratively with TDD approach

**Estimated Complexity**: High (interconnected with Skills, Combat, Inventory systems)
