# Abilities System Architecture Design

**Status**: Design Document  
**Purpose**: Technical specification for class and species-specific active powers

**Related Documents**:
- [abilities-system.md](../features/abilities-system.md) - Feature overview
- [skills-system-design.md](./skills-system-design.md) - Passive skill progression
- [spells-system-design.md](./spells-system-design.md) - Learnable magic system
- [ROADMAP.md](../ROADMAP.md) - Development priorities

---

## Executive Summary

**Current State**: 383 abilities in 4 JSON v4.2 catalogs organized by activation type (active, passive, reactive, ultimate). Abilities have tier system (1-5) and consistent trait-based structure. JSON data complete, code integration pending.

**Implementation Status**:
- ✅ JSON catalogs complete (abilities/active/catalog.json, passive, reactive, ultimate)
- ✅ 383 abilities organized: 177 active, 131 passive, 36 reactive, 39 ultimate
- ✅ v4.2 standards applied (selectionWeight, traits with type annotations)
- ✅ Tier system defined (1-5 based on selectionWeight)
- ⏳ Code model integration pending
- ⏳ Class-ability associations pending
- ⏳ Combat integration pending
- ⏳ Ability unlocking system pending

**Scope**: Complete abilities system implementation
- Character model integration for ability storage
- Define class-specific ability sets and progressions
- Implement ability unlocking system (level-gated by tier)
- Combat integration for ability usage with resource management
- Cooldown tracking and ability state management
- Ability effect execution (damage, healing, buffs, debuffs)
- Clear distinction from Skills (passive proficiencies) and Spells (learnable magic)

**Out of Scope** (Future Enhancements):
- Ability upgrades and talent trees
- Species-specific racial abilities
- Ability customization (modify effects)
- Ability combo system
- Party-wide support abilities

---

## 1. System Philosophy & Distinctions

### 1.1 Three-Pillar Progression

**Skills (Passive Proficiencies)** - 54 total, ranks 0-100:
- Improve through practice-based XP
- Always active, no manual trigger
- Universal (everyone has same 54 skill list)
- Categories: Attribute (24), Weapon (10), Armor (4), Magic (16), Profession (12)
- Example: "Light Blades Rank 50" = +25% damage with daggers/short swords

**Abilities (Special Powers)** - 383 total, tiers 1-5:
- Granted by class/progression
- Active, passive, or reactive activation
- Class-specific (though some overlap possible)
- Categories: Active (177), Passive (131), Reactive (36), Ultimate (39)
- Example: "Charge" = Active ability, rush enemy, stun, bonus damage

**Spells (Learnable Magic)** - 144 total, ranks 0-10:
- Must be learned from spellbooks/trainers/scrolls
- Active cast, tradition-dependent
- Universal access (anyone with tradition skill can learn)
- Traditions: Arcane (36), Divine (36), Occult (36), Primal (36)
- Example: "Fireball" = Rank 3 Arcane spell, requires Arcane skill

### 1.2 When to Use Each System

| Scenario | Use | Example |
|----------|-----|---------|
| Passive damage bonus | **Skill** | Light Blades +0.5% damage per rank |
| Active gap-closer | **Ability** | Charge (Warrior active ability) |
| Cast fireball | **Spell** | Fireball (Rank 3 Arcane spell) |
| Always-on class trait | **Passive Ability** | Warrior's Might (+10% max HP) |
| Buff anyone can learn | **Spell** | Haste (Rank 3 Arcane spell) |
| Class-specific burst | **Ability** | Backstab (Rogue active ability) |
| Counter-attack on hit | **Reactive Ability** | Riposte (triggers when blocked) |
| Game-changing power | **Ultimate Ability** | Time Stop (tier 5 ultimate) |
| Magic tradition unlock | **Magic Skill** | Arcane skill unlocks Arcane spells |
| Spell type boost | **Magic Skill** | Force Magic boosts force spells |

### 1.3 Design Principles

**Memorable**: Each ability should feel distinct and impactful
**Iconic**: Classes defined by signature abilities (Warrior = Charge, Rogue = Backstab)
**Tactical**: Abilities create meaningful combat decisions (when to use ultimate)
**Limited**: Resource costs and cooldowns prevent spam
**Progressive**: New abilities unlock as character grows

---

## 2. Data Model Architecture

### 2.1 Ability Model (RealmEngine.Shared.Models.Ability.cs)

**Current Implementation** (EXISTS, needs enhancement):

```csharp
namespace RealmEngine.Shared.Models;

/// <summary>
/// Represents a character ability (offensive, defensive, support, passive, etc.).
/// </summary>
public class Ability
{
    // CORE IDENTIFICATION
    public string Id { get; set; } = string.Empty;              // "basic-attack", "charge"
    public string Name { get; set; } = string.Empty;            // Internal name
    public string DisplayName { get; set; } = string.Empty;     // UI display
    public string Description { get; set; } = string.Empty;     // What it does
    
    // CATEGORIZATION
    public AbilityTypeEnum Type { get; set; }                   // Offensive, Defensive, etc.
    public int RarityWeight { get; set; } = 1;                  // Generation rarity
    
    // RESOURCE COSTS
    public int ManaCost { get; set; } = 0;                      // Mana consumed
    public int Cooldown { get; set; } = 0;                      // Turns before reuse
    
    // EFFECT PROPERTIES
    public string? BaseDamage { get; set; }                     // "2d6+4"
    public int? Range { get; set; }                             // Attack range
    public int? Duration { get; set; }                          // Effect duration
    
    // REQUIREMENTS
    public int RequiredLevel { get; set; } = 1;                 // Level unlock
    public List<string> AllowedClasses { get; set; } = new();   // Class restrictions
    public List<string> RequiredItemIds { get; set; } = new();  // Equipment needed
    
    // BEHAVIOR
    public bool IsPassive { get; set; } = false;                // Always active?
    public Dictionary<string, object> Traits { get; set; } = new(); // Additional properties
}

public enum AbilityTypeEnum
{
    Offensive,      // Damage-dealing
    Defensive,      // Damage mitigation
    Support,        // Healing/buffs
    Utility,        // Non-combat effects
    Passive,        // Always-on effects
    Ultimate        // Powerful with long cooldown
}
```

**NEW: Character Ability Tracking**:

```csharp
namespace RealmEngine.Shared.Models;

/// <summary>
/// Tracks a character's knowledge and usage of an ability.
/// </summary>
public class CharacterAbility
{
    /// <summary>
    /// Reference to ability definition.
    /// </summary>
    public required string AbilityId { get; set; }
    
    /// <summary>
    /// When character learned/unlocked this ability.
    /// </summary>
    public DateTime UnlockedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Number of times successfully used.
    /// </summary>
    public int TimesUsed { get; set; } = 0;
    
    /// <summary>
    /// Current cooldown remaining (turns).
    /// </summary>
    public int CooldownRemaining { get; set; } = 0;
    
    /// <summary>
    /// Is this ability favorited for quick access?
    /// </summary>
    public bool IsFavorite { get; set; } = false;
    
    /// <summary>
    /// Ability rank/upgrade level (future).
    /// </summary>
    public int Rank { get; set; } = 1;
}
```

### 2.2 Character Integration (RealmEngine.Shared.Models.Character.cs)

Add to Character model:

```csharp
/// <summary>
/// Abilities the character has unlocked, keyed by AbilityId.
/// </summary>
public Dictionary<string, CharacterAbility> UnlockedAbilities { get; set; } = new();
```

### 2.3 Class Ability Sets (RealmEngine.Shared.Models.CharacterClass.cs)

**EXISTING** (already in codebase):

```csharp
public class CharacterClass
{
    // ... existing properties ...
    
    /// <summary>
    /// List of ability reference IDs granted to this class at creation.
    /// Format: "@abilities/active/offensive:charge", "@abilities/passive:warrior-vitality"
    /// </summary>
    public List<string> StartingAbilityIds { get; set; } = new();
}
```

**NEW: Level-Gated Unlocks**:

```csharp
/// <summary>
/// Abilities unlocked at specific levels.
/// </summary>
public Dictionary<int, List<string>> AbilityUnlocksByLevel { get; set; } = new();
// Example: { 5: ["@abilities/active:whirlwind"], 10: ["@abilities/ultimate:last-stand"] }
```

---

## 3. JSON Catalog Structure

### 3.1 Implemented Structure ✅

**Current Directory Structure**:
```
abilities/
├── .cbconfig.json
├── active/
│   ├── .cbconfig.json
│   └── catalog.json (177 abilities)
├── passive/
│   ├── .cbconfig.json
│   └── catalog.json (131 abilities)
├── reactive/
│   ├── .cbconfig.json
│   └── catalog.json (36 abilities)
└── ultimate/
    ├── .cbconfig.json
    └── catalog.json (39 abilities)
```

**Total**: 383 abilities across 4 catalogs

### 3.2 JSON v4.2 Structure

**Example Active Ability**:
```json
{
  "metadata": {
    "version": "4.2",
    "type": "abilities_active_catalog",
    "totalAbilities": 177,
    "lastUpdated": "2026-01-06"
  },
  "offensive": {
    "description": "Direct damage abilities",
    "abilities": [{
      "slug": "charge",
      "name": "Charge",
      "displayName": "Charge",
      "description": "Rush at an enemy, dealing damage and stunning them briefly",
      "tier": 1,
      "selectionWeight": 40,
      "traits": {
        "baseDamage": {"value": "2d6+STR", "type": "string"},
        "cooldown": {"value": 10, "type": "number"},
        "range": {"value": 20, "type": "number"},
        "manaCost": {"value": 15, "type": "number"},
        "statusEffect": {"value": "stunned", "type": "string"},
        "statusDuration": {"value": 1, "type": "number"},
        "damageType": {"value": "physical", "type": "string"}
      }
    }]
  }
}
```

**Example Passive Ability**:
```json
{
  "slug": "warriors-might",
  "name": "Warrior's Might",
  "displayName": "Warrior's Might",
  "description": "Permanent increase to maximum health",
  "tier": 1,
  "selectionWeight": 45,
  "traits": {
    "healthBonus": {"value": 0.10, "type": "number"},
    "bonusType": {"value": "percentage", "type": "string"}
  }
}
```

**Example Reactive Ability**:
```json
{
  "slug": "riposte",
  "name": "Riposte",
  "displayName": "Riposte",
  "description": "Counter-attack when you successfully block",
  "tier": 2,
  "selectionWeight": 75,
  "traits": {
    "triggerCondition": {"value": "onBlock", "type": "string"},
    "baseDamage": {"value": "1d8+DEX", "type": "string"},
    "damageType": {"value": "physical", "type": "string"}
  }
}
```

**Example Ultimate Ability**:
```json
{
  "slug": "time-stop",
  "name": "Time Stop",
  "displayName": "Time Stop",
  "description": "Stop time for all enemies, allowing multiple free actions",
  "tier": 5,
  "selectionWeight": 500,
  "traits": {
    "duration": {"value": 3, "type": "number"},
    "cooldown": {"value": 300, "type": "number"},
    "manaCost": {"value": 150, "type": "number"},
    "effectType": {"value": "time_manipulation", "type": "string"}
  }
}
```

### 3.3 Tier System

Abilities are tiered by power level:

- **Tier 1 (Basic)**: selectionWeight < 50, available at level 1
- **Tier 2 (Common)**: selectionWeight 50-99, available at level 5+
- **Tier 3 (Uncommon)**: selectionWeight 100-199, available at level 10+
- **Tier 4 (Rare)**: selectionWeight 200-399, available at level 15+
- **Tier 5 (Epic/Legendary)**: selectionWeight 400+ or Ultimate type, available at level 20+

### 3.4 Ability Distribution

**By Activation Type**:
- Active: 177 (46%)
  - Offensive: 88
  - Defensive: 34
  - Support: 27
  - Utility: 28
  - Control: 8
  - Summon: 4
  - Mobility: 2
- Passive: 131 (34%)
  - General: 16
  - Offensive: 38
  - Defensive: 39
  - Leadership: 24
  - Environmental: 22
  - Mobility: 7
  - Sensory: 1
- Reactive: 36 (9%)
  - Offensive: 14
  - Defensive: 12
  - Utility: 10
- Ultimate: 39 (10%)
  - All tier 5
│       ├── catalog.json
│       └── names.json
├── passive/
│   └── sensory/
│       ├── catalog.json
│       └── names.json
├── reactive/
│   ├── offensive/
│   │   ├── catalog.json
│   │   └── names.json
│   ├── defensive/
│   │   ├── catalog.json
│   │   └── names.json
│   └── utility/
│       ├── catalog.json
│       └── names.json
└── ultimate/
    ├── catalog.json
    └── names.json
```

**Issues**:
- No class-specific organization
- "reactive" is unclear concept (should be "defensive"?)
- Mix of enemy abilities and player abilities
- No clear level-gating structure

### 3.2 Proposed Reorganization

**Option A: Class-Based Organization** (RECOMMENDED)

```
abilities/
├── warrior/
│   ├── catalog.json (charge, shield-bash, whirlwind, execute, battle-cry, last-stand)
│   ├── names.json
│   └── .cbconfig.json
├── rogue/
│   ├── catalog.json (backstab, evasion, poison-strike, vanish, shadow-step, assassination)
│   ├── names.json
│   └── .cbconfig.json
├── mage/
│   ├── catalog.json (arcane-missiles, mana-shield, frost-nova, blink, spell-steal, meteor)
│   ├── names.json
│   └── .cbconfig.json
├── cleric/
│   ├── catalog.json (smite, heal, divine-shield, cleanse, blessing, divine-intervention)
│   ├── names.json
│   └── .cbconfig.json
├── ranger/
│   ├── catalog.json (power-shot, trap, hunters-mark, camouflage, pet-summon, arrow-storm)
│   ├── names.json
│   └── .cbconfig.json
├── paladin/
│   ├── catalog.json (holy-strike, protective-aura, divine-smite, lay-on-hands, consecration, judgment)
│   ├── names.json
│   └── .cbconfig.json
└── shared/
    ├── catalog.json (abilities multiple classes can use)
    ├── names.json
    └── .cbconfig.json
```

**Benefits**:
- Clear class identity
- Easy to balance per-class
- Natural for level-unlock organization
- Players understand "Warrior abilities" vs "Mage abilities"

### 3.3 Ability Catalog Schema (abilities/{class}/catalog.json)

```json
{
  "metadata": {
    "version": "4.1",
    "type": "ability_catalog",
    "description": "Warrior class abilities",
    "class": "warrior",
    "totalAbilities": 8,
    "lastUpdated": "Current Date"
  },
  "abilities": [
    {
      "id": "charge",
      "name": "Charge",
      "displayName": "Charge",
      "description": "Rush at target enemy, dealing bonus damage and stunning briefly",
      "type": "Offensive",
      "rarityWeight": 1,
      "baseDamage": "1d8+4",
      "manaCost": 15,
      "cooldown": 3,
      "range": 15,
      "duration": 0,
      "requiredLevel": 1,
      "isStartingAbility": true,
      "traits": {
        "damageType": { "value": "physical", "type": "string" },
        "statusEffect": { "value": "stun", "type": "string" },
        "statusDuration": { "value": 1, "type": "number" },
        "movementType": { "value": "dash", "type": "string" }
      }
    },
    {
      "id": "shield-bash",
      "name": "Shield Bash",
      "displayName": "Shield Bash",
      "description": "Bash enemy with shield, interrupting attacks and stunning",
      "type": "Defensive",
      "rarityWeight": 1,
      "baseDamage": "1d6+2",
      "manaCost": 10,
      "cooldown": 2,
      "range": 0,
      "duration": 0,
      "requiredLevel": 1,
      "isStartingAbility": true,
      "requiredItems": ["@items/armor/shields"],
      "traits": {
        "damageType": { "value": "physical", "type": "string" },
        "statusEffect": { "value": "stun", "type": "string" },
        "statusDuration": { "value": 1, "type": "number" },
        "interrupt": { "value": true, "type": "boolean" }
      }
    },
    {
      "id": "whirlwind",
      "name": "Whirlwind",
      "displayName": "Whirlwind Attack",
      "description": "Spin weapon hitting all nearby enemies",
      "type": "Offensive",
      "rarityWeight": 1,
      "baseDamage": "2d6+3",
      "manaCost": 25,
      "cooldown": 4,
      "range": 0,
      "duration": 0,
      "requiredLevel": 5,
      "isStartingAbility": false,
      "traits": {
        "damageType": { "value": "physical", "type": "string" },
        "areaOfEffect": { "value": 5, "type": "number" },
        "targetsAll": { "value": true, "type": "boolean" }
      }
    },
    {
      "id": "execute",
      "name": "Execute",
      "displayName": "Execute",
      "description": "Deliver finishing blow, bonus damage against low-health enemies",
      "type": "Offensive",
      "rarityWeight": 1,
      "baseDamage": "3d8+8",
      "manaCost": 20,
      "cooldown": 5,
      "range": 0,
      "duration": 0,
      "requiredLevel": 10,
      "isStartingAbility": false,
      "traits": {
        "damageType": { "value": "physical", "type": "string" },
        "lowHealthBonus": { "value": 2.0, "type": "number" },
        "lowHealthThreshold": { "value": 0.30, "type": "number" }
      }
    },
    {
      "id": "battle-cry",
      "name": "Battle Cry",
      "displayName": "Battle Cry",
      "description": "Roar increasing damage and defense temporarily",
      "type": "Support",
      "rarityWeight": 1,
      "manaCost": 20,
      "cooldown": 6,
      "duration": 5,
      "requiredLevel": 12,
      "isStartingAbility": false,
      "traits": {
        "damageBonus": { "value": 0.25, "type": "number" },
        "defenseBonus": { "value": 0.20, "type": "number" },
        "selfBuff": { "value": true, "type": "boolean" }
      }
    },
    {
      "id": "iron-will",
      "name": "Iron Will",
      "displayName": "Iron Will",
      "description": "Warrior's resolve grants passive bonus to max health",
      "type": "Passive",
      "rarityWeight": 1,
      "isPassive": true,
      "requiredLevel": 1,
      "isStartingAbility": true,
      "traits": {
        "healthBonus": { "value": 0.10, "type": "number" },
        "alwaysActive": { "value": true, "type": "boolean" }
      }
    },
    {
      "id": "last-stand",
      "name": "Last Stand",
      "displayName": "Last Stand",
      "description": "When taking lethal damage, survive at 1 HP and heal to 50%",
      "type": "Ultimate",
      "rarityWeight": 1,
      "manaCost": 50,
      "cooldown": 20,
      "requiredLevel": 20,
      "isStartingAbility": false,
      "traits": {
        "healPercent": { "value": 0.50, "type": "number" },
        "triggerOnLethal": { "value": true, "type": "boolean" },
        "ultimateAbility": { "value": true, "type": "boolean" }
      }
    }
  ]
}
```

---

## 4. Class Ability Sets

### 4.1 Warrior Abilities

**Theme**: Melee dominance, durability, crowd control

| Ability | Type | Level | Mana | Cooldown | Effect |
|---------|------|-------|------|----------|--------|
| **Charge** | Offensive | 1 (Start) | 15 | 3 | Dash to enemy, damage + stun |
| **Shield Bash** | Defensive | 1 (Start) | 10 | 2 | Stun + interrupt (requires shield) |
| **Iron Will** | Passive | 1 (Start) | 0 | N/A | +10% max HP (always active) |
| **Whirlwind** | Offensive | 5 | 25 | 4 | AoE damage all nearby enemies |
| **Execute** | Offensive | 10 | 20 | 5 | High damage vs low-health targets |
| **Battle Cry** | Support | 12 | 20 | 6 | +25% damage, +20% defense (5 turns) |
| **Last Stand** | Ultimate | 20 | 50 | 20 | Survive lethal hit, heal to 50% |

### 4.2 Rogue Abilities

**Theme**: Burst damage, mobility, stealth

| Ability | Type | Level | Mana | Cooldown | Effect |
|---------|------|-------|------|----------|--------|
| **Backstab** | Offensive | 1 (Start) | 15 | 3 | Massive damage from stealth/behind |
| **Evasion** | Defensive | 1 (Start) | 10 | 2 | Dodge next attack (100% avoid) |
| **Shadow Affinity** | Passive | 1 (Start) | 0 | N/A | +15% critical chance |
| **Poison Strike** | Offensive | 5 | 20 | 4 | Damage + poison DoT (3 turns) |
| **Vanish** | Utility | 10 | 25 | 8 | Enter stealth, drop threat |
| **Shadow Step** | Utility | 12 | 15 | 3 | Teleport behind target |
| **Assassination** | Ultimate | 20 | 60 | 25 | Instant kill if target <30% HP |

### 4.3 Mage Abilities

**Theme**: Arcane mastery, ranged damage, mana manipulation

| Ability | Type | Level | Mana | Cooldown | Effect |
|---------|------|-------|------|----------|--------|
| **Arcane Missiles** | Offensive | 1 (Start) | 12 | 2 | 5 weak projectiles, auto-hit |
| **Mana Shield** | Defensive | 1 (Start) | 20 | 4 | Absorb 50 damage with mana |
| **Arcane Affinity** | Passive | 1 (Start) | 0 | N/A | +20% max mana |
| **Frost Nova** | Offensive | 5 | 25 | 5 | AoE damage + freeze (1 turn) |
| **Blink** | Utility | 10 | 15 | 3 | Short-range teleport (escape) |
| **Spell Steal** | Utility | 12 | 30 | 6 | Copy enemy buff |
| **Meteor** | Ultimate | 20 | 100 | 30 | Massive AoE fire damage |

### 4.4 Cleric Abilities

**Theme**: Holy magic, healing, divine protection

| Ability | Type | Level | Mana | Cooldown | Effect |
|---------|------|-------|------|----------|--------|
| **Smite** | Offensive | 1 (Start) | 12 | 2 | Holy damage (2× vs undead) |
| **Heal** | Support | 1 (Start) | 15 | 3 | Restore 50 HP to target |
| **Divine Grace** | Passive | 1 (Start) | 0 | N/A | +10% healing effectiveness |
| **Divine Shield** | Defensive | 5 | 30 | 8 | Invulnerable for 2 turns |
| **Cleanse** | Support | 10 | 20 | 4 | Remove all debuffs |
| **Blessing** | Support | 12 | 25 | 6 | +15% all stats (5 turns) |
| **Divine Intervention** | Ultimate | 20 | 80 | 25 | Full heal + remove all effects |

### 4.5 Ranger Abilities

**Theme**: Archery, traps, nature magic

| Ability | Type | Level | Mana | Cooldown | Effect |
|---------|------|-------|------|----------|--------|
| **Power Shot** | Offensive | 1 (Start) | 15 | 2 | High-damage arrow |
| **Trap** | Utility | 1 (Start) | 20 | 4 | Set trap (damage + slow) |
| **Keen Senses** | Passive | 1 (Start) | 0 | N/A | +20% critical damage |
| **Hunter's Mark** | Utility | 5 | 10 | 3 | +30% damage vs marked target |
| **Camouflage** | Utility | 10 | 15 | 5 | Stealth in wilderness |
| **Pet Summon** | Support | 12 | 40 | 10 | Summon animal companion |
| **Arrow Storm** | Ultimate | 20 | 70 | 20 | Multiple arrows, AoE damage |

### 4.6 Paladin Abilities

**Theme**: Holy warrior, balanced offense/defense

| Ability | Type | Level | Mana | Cooldown | Effect |
|---------|------|-------|------|----------|--------|
| **Holy Strike** | Offensive | 1 (Start) | 15 | 2 | Damage + heal self 25% |
| **Protective Aura** | Defensive | 1 (Start) | 20 | 5 | -20% damage taken (5 turns) |
| **Righteous Vigor** | Passive | 1 (Start) | 0 | N/A | +15% healing received |
| **Divine Smite** | Offensive | 5 | 25 | 3 | High holy damage |
| **Lay on Hands** | Support | 10 | 30 | 6 | Strong single-target heal (80 HP) |
| **Consecration** | Utility | 12 | 35 | 7 | AoE holy damage zone (3 turns) |
| **Judgment** | Ultimate | 20 | 90 | 25 | Execute + AoE holy explosion |

---

## 5. Ability Unlocking System

### 5.1 Starting Abilities

**At Character Creation**:
- Each class receives 3 starting abilities automatically
- 2 active abilities + 1 passive ability
- Defines immediate playstyle

```csharp
public class CharacterCreationService
{
    public Character CreateCharacter(string className, string characterName)
    {
        var character = new Character { Name = characterName, Class = className };
        var characterClass = _classCatalog.GetClass(className);
        
        // Grant starting abilities
        foreach (var abilityRef in characterClass.StartingAbilityIds)
        {
            var abilityId = _referenceResolver.ResolveReference(abilityRef);
            character.UnlockedAbilities[abilityId] = new CharacterAbility
            {
                AbilityId = abilityId,
                UnlockedDate = DateTime.UtcNow
            };
        }
        
        return character;
    }
}
```

### 5.2 Level-Based Unlocks

**Unlock Schedule**:
- **Level 5**: First additional ability
- **Level 10**: Second additional ability
- **Level 12**: Third additional ability
- **Level 20**: Ultimate ability unlocked

```csharp
public class LevelUpService
{
    public LevelUpResult ProcessLevelUp(Character character)
    {
        character.Level++;
        
        var newAbilities = new List<string>();
        var characterClass = _classCatalog.GetClass(character.Class);
        
        // Check if any abilities unlock at this level
        if (characterClass.AbilityUnlocksByLevel.TryGetValue(character.Level, out var abilitiesToUnlock))
        {
            foreach (var abilityRef in abilitiesToUnlock)
            {
                var abilityId = _referenceResolver.ResolveReference(abilityRef);
                character.UnlockedAbilities[abilityId] = new CharacterAbility
                {
                    AbilityId = abilityId,
                    UnlockedDate = DateTime.UtcNow
                };
                newAbilities.Add(abilityId);
            }
        }
        
        return new LevelUpResult
        {
            NewLevel = character.Level,
            AbilitiesUnlocked = newAbilities
        };
    }
}
```

### 5.3 Class Definition with Unlocks

```json
{
  "id": "warrior",
  "name": "Warrior",
  "startingAbilityIds": [
    "@abilities/warrior:charge",
    "@abilities/warrior:shield-bash",
    "@abilities/warrior:iron-will"
  ],
  "abilityUnlocksByLevel": {
    "5": ["@abilities/warrior:whirlwind"],
    "10": ["@abilities/warrior:execute"],
    "12": ["@abilities/warrior:battle-cry"],
    "20": ["@abilities/warrior:last-stand"]
  }
}
```

---

## 6. Combat Integration

### 6.1 Ability Usage in Combat

```csharp
public class CombatService
{
    public async Task<CombatResult> UseAbilityAsync(string abilityId, ICombatTarget target)
    {
        // Verify ability is unlocked
        if (!_player.UnlockedAbilities.TryGetValue(abilityId, out var charAbility))
        {
            return new CombatResult 
            { 
                Success = false, 
                Message = "You don't know that ability!" 
            };
        }
        
        var ability = _abilityCatalog.GetAbility(abilityId);
        if (ability == null)
        {
            return new CombatResult { Success = false, Message = "Ability not found!" };
        }
        
        // Check cooldown
        if (charAbility.CooldownRemaining > 0)
        {
            return new CombatResult 
            { 
                Success = false, 
                Message = $"{ability.DisplayName} is on cooldown ({charAbility.CooldownRemaining} turns)." 
            };
        }
        
        // Check mana cost
        if (_player.CurrentMana < ability.ManaCost)
        {
            return new CombatResult 
            { 
                Success = false, 
                Message = $"Not enough mana! {ability.DisplayName} costs {ability.ManaCost} mana." 
            };
        }
        
        // Check equipment requirements
        if (ability.RequiredItemIds.Any() && !HasRequiredEquipment(_player, ability))
        {
            return new CombatResult 
            { 
                Success = false, 
                Message = $"{ability.DisplayName} requires specific equipment." 
            };
        }
        
        // Consume mana
        _player.CurrentMana -= ability.ManaCost;
        
        // Execute ability effect
        var result = await ExecuteAbilityEffectAsync(ability, _player, target);
        
        // Update statistics
        charAbility.TimesUsed++;
        
        // Apply cooldown
        charAbility.CooldownRemaining = ability.Cooldown;
        
        // Enemy turn
        await ProcessEnemyTurnsAsync();
        
        // Decrement cooldowns
        DecrementAbilityCooldowns(_player);
        
        return result;
    }
    
    private void DecrementAbilityCooldowns(Character character)
    {
        foreach (var ability in character.UnlockedAbilities.Values)
        {
            if (ability.CooldownRemaining > 0)
            {
                ability.CooldownRemaining--;
            }
        }
    }
}
```

### 6.2 Ability Effect Execution

```csharp
private async Task<CombatResult> ExecuteAbilityEffectAsync(
    Ability ability, 
    Character caster, 
    ICombatTarget target)
{
    switch (ability.Type)
    {
        case AbilityTypeEnum.Offensive:
            return ExecuteOffensiveAbility(ability, caster, target);
            
        case AbilityTypeEnum.Defensive:
            return ExecuteDefensiveAbility(ability, caster);
            
        case AbilityTypeEnum.Support:
            return ExecuteSupportAbility(ability, caster, target);
            
        case AbilityTypeEnum.Ultimate:
            return ExecuteUltimateAbility(ability, caster, target);
            
        default:
            throw new NotImplementedException($"Ability type {ability.Type} not implemented");
    }
}

private CombatResult ExecuteOffensiveAbility(
    Ability ability, 
    Character caster, 
    ICombatTarget target)
{
    // Calculate damage with skill bonuses
    var baseDamage = ParseDiceRoll(ability.BaseDamage);
    var skillBonus = GetSkillDamageBonus(caster, ability);
    var totalDamage = (int)(baseDamage * (1.0 + skillBonus));
    
    // Check for special conditions (Execute low-health bonus)
    if (ability.Id == "execute" && target.CurrentHealth < target.MaxHealth * 0.30)
    {
        totalDamage = (int)(totalDamage * 2.0); // Double damage
    }
    
    // Apply damage
    target.TakeDamage(totalDamage);
    
    // Apply status effects
    if (ability.Traits.TryGetValue("statusEffect", out var effect))
    {
        ApplyStatusEffect(target, effect.ToString(), ability);
    }
    
    return new CombatResult 
    { 
        Success = true, 
        Message = $"{ability.DisplayName} hits {target.Name} for {totalDamage} damage!",
        TotalDamage = totalDamage
    };
}
```

---

## 7. Service Architecture

### 7.1 AbilityCatalogService

```csharp
public interface IAbilityCatalogService
{
    Ability? GetAbility(string abilityId);
    List<Ability> GetAbilitiesByClass(string className);
    List<Ability> GetAbilitiesByType(AbilityTypeEnum type);
    List<Ability> GetStartingAbilities(string className);
    List<Ability> GetUnlockableAbilities(string className, int level);
}

public class AbilityCatalogService : IAbilityCatalogService
{
    private Dictionary<string, Ability> _abilityCache = new();
    
    public AbilityCatalogService()
    {
        LoadAbilitiesFromJson();
    }
    
    private void LoadAbilitiesFromJson()
    {
        // Load from class-specific folders
        var classes = new[] { "warrior", "rogue", "mage", "cleric", "ranger", "paladin", "shared" };
        
        foreach (var className in classes)
        {
            var path = $"Data/abilities/{className}/catalog.json";
            if (!File.Exists(path)) continue;
            
            var json = File.ReadAllText(path);
            var catalog = JsonSerializer.Deserialize<AbilityCatalog>(json);
            
            foreach (var ability in catalog.Abilities)
            {
                _abilityCache[ability.Id] = ability;
            }
        }
    }
    
    public List<Ability> GetStartingAbilities(string className)
    {
        return _abilityCache.Values
            .Where(a => a.AllowedClasses.Contains(className) || !a.AllowedClasses.Any())
            .Where(a => a.RequiredLevel == 1 && a.Traits.ContainsKey("isStartingAbility"))
            .ToList();
    }
}
```

### 7.2 AbilityUsageService

Handles ability execution, cooldowns, and resource management (shown in section 6).

---

## 8. Testing Strategy

### 8.1 Unit Tests

```csharp
[Fact]
public void UseAbility_WithInsufficientMana_ShouldFail()
{
    // Arrange
    var character = new Character { CurrentMana = 5 };
    var ability = new Ability { Id = "charge", ManaCost = 15 };
    
    // Act
    var result = _combatService.UseAbilityAsync("charge", target).Result;
    
    // Assert
    result.Success.Should().BeFalse();
    result.Message.Should().Contain("Not enough mana");
}

[Fact]
public void LevelUp_ShouldUnlockAbilities()
{
    // Arrange
    var character = CreateWarrior(level: 4);
    
    // Act
    var result = _levelUpService.ProcessLevelUp(character); // Level 4 → 5
    
    // Assert
    character.Level.Should().Be(5);
    result.AbilitiesUnlocked.Should().Contain("whirlwind");
    character.UnlockedAbilities.Should().ContainKey("whirlwind");
}

[Fact]
public void ExecuteAbility_ShouldApplyCooldown()
{
    // Arrange
    var character = CreateWarrior();
    character.UnlockedAbilities["charge"] = new CharacterAbility 
    { 
        AbilityId = "charge", 
        CooldownRemaining = 0 
    };
    
    // Act
    _combatService.UseAbilityAsync("charge", enemy).Wait();
    
    // Assert
    character.UnlockedAbilities["charge"].CooldownRemaining.Should().Be(3);
}
```

### 8.2 Integration Tests

```csharp
[Fact]
public async Task CombatWithAbilities_ShouldDecrementCooldowns()
{
    // Arrange
    var character = CreateWarrior();
    var enemy = CreateEnemy();
    
    // Act - Use Charge
    await _combatService.UseAbilityAsync("charge", enemy);
    var cooldownAfterUse = character.UnlockedAbilities["charge"].CooldownRemaining;
    
    // Enemy turn passes (cooldown should decrease)
    await _combatService.PlayerAttackAsync(enemy); // Regular attack
    var cooldownAfterTurn = character.UnlockedAbilities["charge"].CooldownRemaining;
    
    // Assert
    cooldownAfterUse.Should().Be(3);
    cooldownAfterTurn.Should().Be(2); // Decreased by 1
}
```

---

## 9. Migration Strategy

### 9.1 Existing Abilities Audit

**Current Status**: 100+ ability files exist but are organized by type (active/passive/reactive), not by class.

**Migration Steps**:
1. **Audit Phase**: Review all existing abilities
   - Identify which are player abilities vs enemy abilities
   - Categorize by class affinity
   - Mark abilities for migration or deprecation
   
2. **Reorganization Phase**: Restructure directories
   - Create class-based folders (warrior/, rogue/, etc.)
   - Move relevant abilities to class folders
   - Create shared/ folder for universal abilities
   
3. **Data Cleanup Phase**: Update JSON schemas
   - Add `isStartingAbility` flag
   - Add `allowedClasses` restrictions
   - Add level requirements
   - Update references in class definitions

### 9.2 Backwards Compatibility

**Old Ability References**: Use migration mapping

```csharp
public class AbilityMigrationService
{
    private Dictionary<string, string> _migrationMap = new()
    {
        // Old path → New path
        ["@abilities/active/offensive:charge"] = "@abilities/warrior:charge",
        ["@abilities/active/offensive:backstab"] = "@abilities/rogue:backstab",
        // ... etc
    };
    
    public string MigrateReference(string oldRef)
    {
        return _migrationMap.TryGetValue(oldRef, out var newRef) ? newRef : oldRef;
    }
}
```

---

## 10. UI/API Requirements

### 10.1 Query APIs

```csharp
// Get character's unlocked abilities
public GetAbilitiesResponse GetUnlockedAbilities(Character character)
{
    var abilities = character.UnlockedAbilities.Select(kvp =>
    {
        var ability = _abilityCatalog.GetAbility(kvp.Key);
        var charAbility = kvp.Value;
        
        return new AbilityInfo
        {
            AbilityId = ability.Id,
            Name = ability.DisplayName,
            Description = ability.Description,
            Type = ability.Type.ToString(),
            ManaCost = ability.ManaCost,
            CooldownRemaining = charAbility.CooldownRemaining,
            TimesUsed = charAbility.TimesUsed,
            IsFavorite = charAbility.IsFavorite
        };
    }).ToList();
    
    return new GetAbilitiesResponse { Abilities = abilities };
}

// Get usable abilities (not on cooldown, have mana)
public GetUsableAbilitiesResponse GetUsableAbilities(Character character)
{
    var usable = character.UnlockedAbilities
        .Where(kvp => kvp.Value.CooldownRemaining == 0)
        .Where(kvp =>
        {
            var ability = _abilityCatalog.GetAbility(kvp.Key);
            return character.CurrentMana >= ability.ManaCost;
        })
        .Select(kvp => _abilityCatalog.GetAbility(kvp.Key))
        .ToList();
    
    return new GetUsableAbilitiesResponse { Abilities = usable };
}
```

### 10.2 Command APIs

```csharp
// Use ability in combat
public async Task<UseAbilityResponse> UseAbilityAsync(UseAbilityRequest request)
{
    var result = await _combatService.UseAbilityAsync(
        request.AbilityId,
        request.Target);
    
    return new UseAbilityResponse
    {
        Success = result.Success,
        Message = result.Message,
        TotalDamage = result.TotalDamage
    };
}
```

---

## 11. Performance Considerations

- Ability catalog cached in memory (load once at startup)
- Character abilities use `Dictionary<string, CharacterAbility>` for O(1) lookup
- Cooldown decrements happen once per turn (not checked every frame)

---

## 12. Future Enhancements

### Advanced Features:
- **Ability Upgrades**: Improve abilities with use or skill ranks
- **Talent Trees**: Choose ability specializations
- **Ability Combos**: Chain abilities for synergy effects
- **Equipment-Granted Abilities**: Items unlock temporary abilities
- **Ability Customization**: Modify effects (more damage vs less cooldown)

### Extended Features:
- **Species Abilities**: Racial powers (Dwarf Stoneform, Elf Arcane Affinity)
- **Legendary Abilities**: Ultra-rare ultimate powers
- **Ability Transmog**: Visual customization of ability effects
- **Ability Achievements**: Rewards for mastery (use ability 1000 times)

---

## 13. Success Metrics

### Technical Metrics:
- All abilities load from JSON catalogs
- Ability usage triggers correctly in combat
- Cooldowns track and decrement accurately
- Resource costs enforced (mana)
- Level-gating works for unlocks

### Gameplay Metrics:
- Players use abilities regularly (not just basic attacks)
- Each class feels distinct through abilities
- Ultimate abilities feel impactful
- Tactical decisions matter (when to use cooldowns)
- Ability unlocks provide progression excitement

---

## 14. Documentation Deliverables

- [ ] This design document
- [ ] API documentation for `AbilityCatalogService`
- [ ] API documentation for `AbilityUsageService`
- [ ] JSON schema documentation for ability catalogs
- [ ] Migration guide for existing ability files
- [ ] Player-facing ability guide (per class)

---

## Implementation Readiness

**Design Status**: ✅ Ready for Implementation

**Dependencies**:
- Skills System (for skill bonuses on abilities)
- Combat System (ability usage context)
- Class definitions with starting/unlocking abilities

**Next Steps**:
1. Audit existing 100+ ability JSON files
2. Reorganize abilities by class (warrior/, rogue/, etc.)
3. Update JSON schemas with level requirements
4. Implement CharacterAbility tracking model
5. Integrate ability usage into CombatService
6. Create feature branch: `feature/abilities-system`

**Estimated Complexity**: High (requires JSON reorganization + combat integration)
