# Trait Standards v1.0

**Version:** 1.0  
**Date:** December 28, 2025  
**Status:** ACTIVE  
**Purpose:** Standardized trait formats and value enums for all game entities

---

## Overview

**Traits** are inherent properties of game entities (enemies, items, NPCs) that describe their fundamental characteristics. This document defines:

- **Trait Formats** - How traits are structured (boolean, numeric, string enum)
- **Type-Level vs Individual** - Traits that apply to categories vs specific items
- **Value Standards** - Standardized enums and numeric scales
- **Attribute Traits** - All attribute-like traits use numeric values (1-20+)

---

## Core Principles

1. **Typed Values** - Use appropriate data types (boolean, number, string enum)
2. **Numeric Attributes** - All attribute traits (intelligence, strength, etc.) are numeric 1-20+
3. **Type-Level Defaults** - Common traits at category level, overrides at item level
4. **Boolean for Inherent Properties** - Use booleans for true/false characteristics (undead, magical)
5. **Abilities over Traits** - Special powers are passive/active abilities, not individual boolean traits

---

## Trait Taxonomy

### Categories of Traits

Traits are organized into **7 major categories** based on their conceptual domain:

#### 1. Physical Traits
**Definition:** Tangible, observable characteristics of an entity's physical form

| Trait | Type | Values | Scope |
|-------|------|--------|-------|
| `size` | string enum | tiny, small, medium, large, huge, gargantuan | Type-Level |
| `weight` | number | Physical weight in lbs | Individual |
| `appearance` | string | Description text | Individual |
| `flying` | boolean | Can fly | Type-Level |
| `incorporeal` | boolean | Non-physical form | Type-Level |
| `armored` | boolean | Natural armor | Type-Level |

**Examples:**
- Dragons: `size: "huge"`, `flying: true`
- Ghosts: `incorporeal: true`
- Beetles: `armored: true`

---

#### 2. Mental/Attribute Traits
**Definition:** Cognitive and mental characteristics (always numeric 1-20+)

| Trait | Type | Range | Scope |
|-------|------|-------|-------|
| `intelligence` | number | 1-20+ | Type-Level |
| `wisdom` | number | 1-20+ | Type-Level or Individual |
| `charisma` | number | 1-20+ | Type-Level or Individual |
| `perception` | number | 1-20+ | Type-Level or Individual |

**Design Rule:** All D&D-style attributes MUST be numeric, never string enums

**Current Implementation:** Only `intelligence` is currently used across all enemy catalogs

**Future Expansion:** wisdom, charisma, perception can be added when game mechanics require them

---

#### 3. Combat Traits
**Definition:** Properties that affect combat behavior and damage interactions

| Trait | Type | Values | Scope |
|-------|------|--------|-------|
| `behavior` | string enum | passive, aggressive, tactical, etc. | Type-Level |
| `damageType` | string enum | physical, fire, slashing, etc. | Type-Level or Individual |
| `vulnerability` | string or array | Damage type(s) that cause extra damage | Type-Level |
| `immunity` | array | Damage type(s) that don't affect entity | Type-Level |
| `resistance` | array | Damage type(s) with reduced effect | Type-Level |
| `regeneration` | boolean | Passive health regeneration | Type-Level |

**Examples:**
- Trolls: `regeneration: true`, `vulnerability: "fire"`
- Fire Elementals: `immunity: ["fire"]`, `vulnerability: "cold"`
- Tactical Enemies: `behavior: "tactical"`

---

#### 4. Social/Organizational Traits
**Definition:** How entities relate to others of their kind

| Trait | Type | Values | Scope |
|-------|------|--------|-------|
| `social` | string enum | solitary, pack, tribal, clan, hive_mind, etc. | Type-Level |
| `alignment` | string enum | good, evil, lawful_good, etc. | Type-Level |
| `aristocratic` | boolean | Noble hierarchy | Type-Level |
| `intelligence_level` | derived | Based on intelligence number | Calculated |

**Examples:**
- Wolves: `social: "pack"`
- Vampires: `social: "aristocratic"`
- Kobolds: `social: "tribal"`

---

#### 5. Elemental/Supernatural Traits
**Definition:** Magical and otherworldly properties

| Trait | Type | Values | Scope |
|-------|------|--------|-------|
| `element` | string enum | fire, water, earth, air, lightning, ice | Type-Level |
| `undead` | boolean | Animated dead | Type-Level |
| `magical` | boolean | Uses magic | Type-Level |
| `shapeshifter` | boolean | Can change form | Type-Level |
| `breathType` | string enum | fire, acid, lightning, poison | Individual |
| `unique` | boolean | Special/boss enemy | Type-Level or Individual |

**Examples:**
- Undead: `undead: true`, `vulnerability: "radiant"`
- Dragons: `breathType: "fire"` (individual), `element: "fire"` (type)
- Vampires: `shapeshifter: true`, `magical: true`

---

#### 6. Environmental Traits
**Definition:** Habitat and environmental preferences

| Trait | Type | Values | Scope |
|-------|------|--------|-------|
| `habitat` | string or array | forest, mountain, cave, water, underground | Type-Level |
| `rooted` | boolean | Cannot move (plants) | Type-Level |
| `aquatic` | boolean | Lives in water | Type-Level |
| `nocturnal` | boolean | Active at night | Type-Level |

**Examples:**
- Wolves: `habitat: "forest"`
- Plants: `rooted: true`, `vulnerability: "fire"`
- Fish Creatures: `aquatic: true`

---

#### 7. Mechanical/Game Traits (Items)
**Definition:** Game system properties for items and equipment

| Trait | Type | Values | Scope |
|-------|------|--------|-------|
| `slot` | string enum | mainhand, offhand, head, chest, consumable | Type-Level |
| `category` | string enum | weapon, armor, potion, scroll | Type-Level |
| `stackable` | boolean | Can stack in inventory | Type-Level |
| `oneUse` | boolean | Consumed on use | Type-Level |
| `twoHanded` | boolean | Requires both hands | Individual |
| `skillType` | string enum | blade, axe, bow, magic | Type-Level |

**Examples:**
- Weapons: `slot: "mainhand"`, `category: "weapon"`
- Potions: `stackable: true`, `oneUse: true`
- Greatswords: `twoHanded: true`

---

### Types of Traits by Scope

#### Type-Level Traits
**Applied to ALL items in a category**

**Characteristics:**
- Defined in `traits` object at category level
- Inherited by all items in category
- Can be overridden by individual items
- Define the "default" for the category

**Examples:**
```json
{
  "goblinoid_types": {
    "goblins": {
      "traits": {
        "category": "goblinoid",
        "size": "small",           // All goblins are small
        "intelligence": 8,         // Average goblin intelligence
        "social": "tribal"         // All goblins are tribal
      }
    }
  }
}
```

**Best Practices:**
- Use for inherent category properties
- Use for properties shared by 90%+ of items
- Override exceptions at individual level

---

#### Individual Traits
**Specific to one item, overrides type-level**

**Characteristics:**
- Defined directly on item object
- Overrides type-level trait of same name
- Used for exceptions and unique properties
- Higher priority than type-level

**Examples:**
```json
{
  "items": [
    {
      "name": "Naga",
      "size": "large",              // Overrides type-level "medium"
      "damageType": "magical",      // Item-specific trait
      "breathType": "lightning"     // Individual-only trait
    }
  ]
}
```

**Best Practices:**
- Use sparingly for exceptions
- Document why override is needed
- Avoid overriding core category traits

---

### Types of Traits by Data Type

#### Boolean Traits
**True/false properties**

**When to Use:**
- Inherent yes/no characteristics
- Type-level flags (undead, magical, flying)
- NOT for special powers (use abilities instead)

**Examples:**
- ✅ `undead: true` - Inherent property
- ✅ `regeneration: true` - Natural ability (also needs passive ability for mechanics)
- ✅ `flying: true` - Physical capability
- ❌ `petrifying_gaze: true` - This is a special power, use ability instead

---

#### Numeric Traits
**Quantifiable measurements**

**When to Use:**
- Attributes (intelligence, strength, dexterity) - Always 1-20+
- Resistances/percentages - 0-100
- Stats that match game mechanics - Variable scale
- Counts and quantities

**Scales:**
- **1-20+**: Attributes (intelligence, wisdom, charisma)
- **0-100**: Percentages (fireResistance: 50 = 50% reduction)
- **Game Stats**: speed, weight, etc. (match existing stat scale)

**Examples:**
```json
{
  "intelligence": 14,        // Attribute (1-20+)
  "fireResistance": 50,      // Percentage (0-100)
  "speed": 15,               // Game stat
  "weight": 3.5              // Physical measurement
}
```

---

#### String Enum Traits
**Categorical values from predefined set**

**When to Use:**
- Property has discrete categories
- Values are descriptive, not numeric
- Need to query by category (e.g., "all fire elementals")

**Benefits:**
- Self-documenting
- Easy to filter/query
- Prevents typos

**Examples:**
```json
{
  "size": "medium",          // From: tiny, small, medium, large, huge, gargantuan
  "behavior": "aggressive",  // From: passive, aggressive, tactical, etc.
  "element": "fire"          // From: fire, water, earth, air
}
```

**Validation:** All enums should be documented in this standard

---

#### Array Traits
**Multiple values of same type**

**When to Use:**
- Entity has multiple values for same property
- Immunities, vulnerabilities, habitats
- NOT for different types of properties

**Examples:**
```json
{
  "immunity": ["fire", "poison"],           // Multiple immunities
  "habitat": ["forest", "mountain"],        // Lives in multiple places
  "vulnerability": "radiant"                // Single value (string is fine)
}
```

**Array vs Single Value:**
- If entity typically has 0-1 values: use single value (string or null)
- If entity typically has 2+ values: use array
- Arrays are never empty (omit property instead)

---

### Types of Traits by Mutability

#### Inherent Traits (Immutable)
**Permanent properties that define entity's nature**

**Characteristics:**
- Cannot change during gameplay
- Define what the entity IS, not what it DOES
- Typically type-level traits

**Examples:**
- `undead: true` - Cannot become alive
- `size: "large"` - Size doesn't change (normally)
- `category: "dragon"` - Can't change type
- `incorporeal: true` - Ghost is always incorporeal

**Rule:** 99% of traits in current system are inherent

---

#### Dynamic Traits (Mutable) - FUTURE
**Properties that can change during gameplay**

**Not currently implemented, but future consideration:**
- Status effects (poisoned, paralyzed)
- Temporary buffs/debuffs
- Environmental effects

**Design Note:** These will likely use a separate `effects` or `conditions` system rather than traits

---

#### Conditional Traits - FUTURE
**Active only under certain conditions**

**Not currently implemented, examples:**
- `nightBonus: +2` - Only at night
- `underwaterBreathing: true` - When in water
- `rageBuff: {condition: "health < 50%", effect: "+damage"}`

**Design Note:** May be implemented as part of abilities system rather than traits

---

### Trait Inheritance and Override Rules

#### Priority Order (Highest to Lowest)

1. **Individual Item Trait** - Highest priority
2. **Type-Level Trait** - Default for category
3. **System Default** - Implicit defaults (e.g., `size: "medium"` if not specified)

#### Override Examples

```json
{
  "troll_types": {
    "common_trolls": {
      "traits": {
        "size": "large",           // Type-level default
        "intelligence": 5,
        "regeneration": true
      },
      "items": [
        {
          "name": "Troll",
          "rarityWeight": 12
          // Inherits: size=large, intelligence=5, regeneration=true
        },
        {
          "name": "Ancient Troll",
          "size": "huge",          // OVERRIDES type-level "large"
          "intelligence": 8,       // OVERRIDES type-level 5
          "rarityWeight": 50
          // Inherits: regeneration=true
          // Overrides: size, intelligence
        }
      ]
    }
  }
}
```

#### Merge vs Replace

**Replace (Current System):**
- Individual trait completely replaces type-level trait
- No merging of values
- Example: `size: "huge"` replaces `size: "large"`

**Merge (Not Implemented):**
- Arrays could theoretically merge
- Example: `immunity: ["fire"]` + `immunity: ["poison"]` → `["fire", "poison"]`
- Currently NOT supported - individual completely replaces type-level

---

### Trait Design Guidelines

#### When to Add a New Trait

**Add a trait when:**
- ✅ Property is inherent to entity (part of what it IS)
- ✅ Property affects game mechanics
- ✅ Property is needed for filtering/querying
- ✅ Property applies to multiple entities
- ✅ Property is relatively static

**DON'T add a trait when:**
- ❌ Property is an action (use ability instead)
- ❌ Property is temporary (use status effect)
- ❌ Property is unique to one item (maybe just a note/description)
- ❌ Property is calculated (derive it from other values)

#### Naming Conventions

**Boolean Traits:**
- Use adjectives: `magical`, `undead`, `incorporeal`
- NOT verbs: ❌ `canFly` → ✅ `flying`
- NOT questions: ❌ `isUndead` → ✅ `undead`

**Numeric Traits:**
- Use nouns: `intelligence`, `fireResistance`, `weight`
- CamelCase for compound names: `fireResistance`, `maxHealth`

**String Enum Traits:**
- Use nouns: `size`, `behavior`, `element`
- Lowercase values: `"medium"`, `"aggressive"`, `"fire"`
- Underscores for compounds: `"hive_mind"`, `"lawful_good"`

**Array Traits:**
- Use plural nouns: `immunity`, `vulnerability` (not `immunities`, `vulnerabilities`)
- OR use descriptive singular: `habitat`, `element`

---

## Trait Formats

### Format 1: Boolean (Type-Level)

**Use Case:** Inherent properties that apply to entire category

```json
"traits": {
  "undead": true,
  "magical": true,
  "regeneration": true,
  "incorporeal": true,
  "shapeshifter": true,
  "flying": true,
  "armored": true
}
```

**Examples:**
- `undead: true` - All items in "undead" category are undead
- `regeneration: true` - All trolls regenerate health
- `magical: true` - Category uses magic

---

### Format 2: Numeric (Attributes)

**Use Case:** Measurable properties, especially attributes (1-20+ scale)

```json
"traits": {
  "intelligence": 14,
  "strength": 18,
  "dexterity": 12,
  "fireResistance": 50,
  "speed": 15
}
```

**Scale Guidelines:**
- **1-20+**: D&D-style attributes (intelligence, wisdom, charisma)
- **1-100**: Percentages (resistances, chances)
- **Stat values**: Match game stat scale (speed, defense, etc.)

---

### Format 3: String Enum (Categorical)

**Use Case:** Categorical properties with predefined values

```json
"traits": {
  "category": "humanoid",
  "size": "medium",
  "behavior": "opportunistic",
  "damageType": "physical",
  "alignment": "evil",
  "element": "fire"
}
```

**Always use string enums** when property has discrete categories

---

### Format 4: Array (Multiple Values)

**Use Case:** Multiple values of same type (immunities, vulnerabilities)

```json
"traits": {
  "immunity": ["fire", "poison"],
  "vulnerability": "radiant",
  "habitat": ["forest", "mountain"]
}
```

---

## Standard Trait Values

### Size Values

| Value | Description | Examples |
|-------|-------------|----------|
| `tiny` | Very small creatures | Rats, fairies |
| `small` | Child-sized | Halflings, goblins |
| `medium` | Human-sized (default) | Humans, elves, orcs |
| `large` | Horse-sized | Ogres, bears, trolls |
| `huge` | House-sized | Giants, young dragons |
| `gargantuan` | Colossal | Ancient dragons, titans |

---

### Intelligence Values (Numeric 1-20+)

**All intelligence traits use numeric values on 1-20+ scale**

| Range | Category | Examples |
|-------|----------|----------|
| 1-2 | Mindless | Zombies (1), Skeletons (2) |
| 3-5 | Animal | Beasts (3-5), Plants (2-4) |
| 6-9 | Below Average | Trolls (5-8), Goblins (8) |
| 10-12 | Average | Humanoids (10), Orcs (8-10) |
| 13-15 | Above Average | Hobgoblins (12), Cultists (14) |
| 16-17 | Brilliant | Yuan-ti (16), Dragons (16-18) |
| 18-19 | Genius | Vampires (18), Devils (18) |
| 20+ | Legendary | Ancient Dragons (20), Liches (20) |

**Conversion from Legacy String Enums:**
- `"none"` → 1
- `"low"` → 3-8 (depends on creature type)
- `"medium"` → 10-12
- `"high"` → 14-16
- `"very_high"` → 18
- `"genius"` → 20+

---

### Behavior Values

| Value | Description | Combat Pattern |
|-------|-------------|----------------|
| `passive` | Won't attack unless provoked | Defensive only |
| `defensive` | Defends territory | Fights when approached |
| `opportunistic` | Attacks weak targets | Bandits, scavengers |
| `aggressive` | Attacks on sight | Most monsters |
| `tactical` | Uses strategy | Military units |
| `cunning` | Sets traps, ambushes | Devils, assassins |
| `territorial` | Guards specific areas | Bears, elementals |
| `predatory` | Hunts prey | Wolves, vampires |
| `legendary` | Boss-level behavior | Named enemies |
| `chaotic` | Unpredictable | Lesser demons |
| `calculating` | Methodical planning | Liches, masterminds |

---

### Damage Type Values

| Value | Description | Source |
|-------|-------------|--------|
| `physical` | Generic physical | Most melee |
| `slashing` | Bladed weapons | Swords, claws |
| `piercing` | Stabbing weapons | Arrows, fangs |
| `bludgeoning` | Blunt weapons | Clubs, fists |
| `fire` | Fire damage | Fire spells, dragons |
| `cold` | Ice/frost damage | Ice spells, elementals |
| `lightning` | Electric damage | Lightning spells |
| `poison` | Toxic damage | Venom, poison gas |
| `acid` | Corrosive damage | Acid breath |
| `magical` | Generic magic | Magic attacks |
| `radiant` | Holy/light damage | Divine spells |
| `necrotic` | Death/shadow damage | Undead, curses |
| `psychic` | Mind damage | Psionics |

---

### Alignment Values (Optional)

| Value | Description |
|-------|-------------|
| `good` | Benevolent |
| `evil` | Malevolent |
| `lawful_good` | Honorable heroes |
| `lawful_evil` | Tyrannical, follows rules |
| `chaotic_good` | Freedom fighters |
| `chaotic_evil` | Destructive chaos |
| `neutral` | Balanced or indifferent |

---

### Social Values

| Value | Description | Examples |
|-------|-------------|----------|
| `solitary` | Lives alone | Bears, hermits |
| `pack` | Small groups | Wolves |
| `tribal` | Tribe structure | Goblins, lizardfolk |
| `clan` | Clan-based | Orcs, dwarves |
| `military` | Organized ranks | Hobgoblins |
| `hive_mind` | Shared consciousness | Insects |
| `aristocratic` | Noble hierarchy | Vampires |
| `cult` | Religious group | Yuan-ti |
| `leadership` | Has leaders | Goblin King |

---

### Element Values (Elementals)

| Value | Description |
|-------|-------------|
| `fire` | Fire elementals |
| `water` | Water elementals |
| `earth` | Earth elementals |
| `air` | Air elementals |
| `lightning` | Lightning/storm |
| `ice` | Ice elementals |

---

## Type-Level vs Individual Traits

### Type-Level Traits

**Applied to ALL items in a category** (defined in `traits` object)

```json
{
  "vampire_types": {
    "lesser_vampires": {
      "traits": {
        "category": "vampire",
        "size": "medium",
        "intelligence": 14,
        "undead": true,
        "regeneration": true,
        "vulnerability": "radiant"
      }
    }
  }
}
```

**All lesser vampires inherit these traits:**
- Vampire category
- Medium size
- Intelligence 14
- Undead
- Regeneration
- Radiant vulnerability

---

### Individual Traits (Overrides)

**Item-specific traits override or extend type-level traits**

```json
{
  "items": [
    {
      "name": "Naga",
      "size": "large",              // Overrides type-level "medium"
      "damageType": "magical",      // Item-specific trait
      "rarityWeight": 45
    }
  ]
}
```

**Override Priority:** Individual > Type-Level

---

## Special Powers: Abilities vs Traits

### ❌ DON'T Use Individual Boolean Traits

**Bad Example:**
```json
{
  "name": "Basilisk",
  "petrifying_gaze": true,    // ❌ Special power as boolean trait
  "venomous": true            // ❌ Attack ability as trait
}
```

### ✅ DO Use Passive/Active Abilities

**Good Example:**
```json
{
  "name": "Basilisk",
  "abilities": [
    "@abilities/active/offensive:petrifying-gaze",
    "@abilities/active/offensive:poison-bite"
  ]
}
```

**Guideline:** If it's an action or special power, it's an ability. If it's an inherent property (undead, incorporeal), it's a trait.

---

## Common Trait Patterns

### Enemy Traits

**Typical enemy type-level traits:**

```json
"traits": {
  "category": "humanoid",      // Entity type
  "size": "medium",            // Physical size
  "intelligence": 10,          // 1-20+ numeric
  "behavior": "aggressive",    // Combat behavior
  "social": "clan",            // Social structure
  "damageType": "physical",    // Primary damage type
  "vulnerability": "fire",     // Weakness
  "regeneration": true         // Special property
}
```

---

### Item Traits

**Typical weapon/armor traits:**

```json
"traits": {
  "slot": "mainhand",          // Equipment slot
  "category": "weapon",        // Item type
  "damageType": "slashing",    // Damage type
  "skillType": "blade",        // Required skill
  "twoHanded": false           // Weapon property
}
```

---

### Consumable Traits

**Typical potion/scroll traits:**

```json
"traits": {
  "slot": "consumable",        // Inventory slot
  "category": "potion",        // Consumable type
  "stackable": true,           // Can stack in inventory
  "oneUse": true              // Consumed on use
}
```

---

## Trait Validation Rules

### Required Type-Level Traits (Enemies)

All enemy types **MUST** have:
- `category` (string)
- `size` (string enum)
- `intelligence` (number 1-20+)
- `behavior` (string enum)

### Optional Common Traits

- `social` - Social structure
- `damageType` - Primary damage type
- `vulnerability` - Weakness
- `immunity` - Damage immunities
- `alignment` - Moral alignment
- Boolean flags (undead, magical, etc.)

---

## Implementation Status

### ✅ Completed (December 28, 2025)

**All 13 enemy catalogs standardized:**
- ✅ Intelligence converted to numeric (1-20+)
- ✅ Individual boolean traits removed (use abilities)
- ✅ Type-level traits consistent
- ✅ All builds passing

**Catalogs Updated:**
1. enemies/humanoids/catalog.json (4 types, 14 enemies)
2. enemies/dragons/catalog.json (5 types, 13 dragons)
3. enemies/beasts/catalog.json (4 types, 15 beasts)
4. enemies/undead/catalog.json (4 types, 14 undead)
5. enemies/goblinoids/catalog.json (4 types, 12 goblinoids)
6. enemies/orcs/catalog.json (3 types, 11 orcs)
7. enemies/demons/catalog.json (4 types, 14 demons)
8. enemies/elementals/catalog.json (4 types, 12 elementals)
9. enemies/trolls/catalog.json (3 types, 10 trolls)
10. enemies/insects/catalog.json (4 types, 14 insects)
11. enemies/plants/catalog.json (4 types, 13 plants)
12. enemies/reptilians/catalog.json (4 types, 13 reptilians)
13. enemies/vampires/catalog.json (3 types, 11 vampires)

**Total Impact:**
- 163 enemies standardized
- 48 enemy types with numeric intelligence
- Zero breaking changes

---

### ⏸️ Deferred

**Item catalogs await item power system design:**
- items/weapons/catalog.json
- items/armor/catalog.json
- items/consumables/catalog.json
- items/materials/catalog.json

**Future features:**
- Enhanced trait objects (temporary buffs)
- Trait stacking system
- C# validation rules

---

## Examples

### Example 1: Dragon Type

```json
{
  "dragon_types": {
    "chromatic": {
      "traits": {
        "category": "dragon",
        "size": "huge",
        "intelligence": 16,
        "behavior": "territorial",
        "alignment": "evil"
      },
      "items": [
        {
          "name": "Red Dragon",
          "health": 300,
          "attack": 35,
          "breathType": "fire",
          "rarityWeight": 80,
          "abilities": [
            "@abilities/active/offensive:dragon-breath",
            "@abilities/active/offensive:claw-attack"
          ]
        }
      ]
    }
  }
}
```

**Trait Analysis:**
- ✅ intelligence: 16 (numeric)
- ✅ Type-level traits apply to all chromatic dragons
- ✅ breathType is item-specific (fire vs acid vs lightning)
- ✅ Dragon-breath is ability, not boolean trait

---

### Example 2: Vampire Type

```json
{
  "vampire_types": {
    "ancient_vampires": {
      "traits": {
        "category": "vampire",
        "size": "medium",
        "intelligence": 20,
        "behavior": "legendary",
        "undead": true,
        "regeneration": true,
        "magical": true,
        "shapeshifter": true,
        "vulnerability": "radiant"
      },
      "items": [
        {
          "name": "Vampire Progenitor",
          "health": 280,
          "attack": 60,
          "level": 18,
          "rarityWeight": 100,
          "abilities": [
            "@abilities/active/offensive:drain-life",
            "@abilities/active/offensive:blood-magic",
            "@abilities/ultimate:blood-god"
          ]
        }
      ]
    }
  }
}
```

**Trait Analysis:**
- ✅ intelligence: 20 (numeric, legendary level)
- ✅ Boolean traits are inherent (undead, regeneration)
- ✅ Special powers are abilities (drain-life, blood-magic)
- ✅ No individual boolean traits on items

---

### Example 3: Troll Type

```json
{
  "troll_types": {
    "common_trolls": {
      "traits": {
        "category": "troll",
        "size": "large",
        "intelligence": 5,
        "behavior": "aggressive",
        "damageType": "physical",
        "regeneration": true,
        "vulnerability": "fire"
      },
      "items": [
        {
          "name": "Troll",
          "health": 120,
          "attack": 30,
          "level": 8,
          "rarityWeight": 12,
          "abilities": [
            "@abilities/active/offensive:basic-attack",
            "@abilities/passive:regeneration"
          ]
        }
      ]
    }
  }
}
```

**Trait Analysis:**
- ✅ intelligence: 5 (numeric, low intelligence)
- ✅ regeneration: true at type level (all trolls regenerate)
- ✅ Regeneration also as passive ability (for game mechanics)
- ✅ Fire vulnerability defined at type level

---

## Migration Guide

### Converting String Intelligence to Numeric

**Before:**
```json
"traits": {
  "intelligence": "medium"
}
```

**After:**
```json
"traits": {
  "intelligence": 10
}
```

**Mapping:**
- `"none"` → 1
- `"low"` → 3-8 (context dependent)
- `"medium"` → 10-12
- `"high"` → 14-16
- `"very_high"` → 18
- `"genius"` → 20+

---

### Removing Individual Boolean Traits

**Before:**
```json
{
  "name": "Basilisk",
  "petrifying_gaze": true,
  "abilities": [...]
}
```

**After:**
```json
{
  "name": "Basilisk",
  "abilities": [
    "@abilities/active/offensive:petrifying-gaze",
    ...
  ]
}
```

**Process:**
1. Identify boolean traits that are special powers
2. Find or create corresponding ability
3. Add ability reference to abilities array
4. Remove boolean trait

---

## Related Standards

- **[CATALOG_JSON_STANDARD.md](CATALOG_JSON_STANDARD.md)** - Catalog structure and rarityWeight
- **[JSON_REFERENCE_STANDARDS.md](JSON_REFERENCE_STANDARDS.md)** - Ability reference syntax
- **[JSON_STRUCTURE_TYPES.md](JSON_STRUCTURE_TYPES.md)** - Overall JSON structure patterns

---

## Changelog

### v1.0 (December 28, 2025)
- ✅ Initial standard created
- ✅ All enemy catalogs standardized
- ✅ Intelligence converted to numeric (1-20+)
- ✅ Individual boolean traits removed
- ✅ Type-level trait patterns established
- ✅ 163 enemies validated with builds passing
