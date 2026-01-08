# JSON Data Standard v5.0

**Status**: ACTIVE  
**Date**: January 8, 2026  
**Scope**: Universal structure for ALL game data JSON files

---

## Overview

Version 5.0 introduces a **trait array system** that standardizes data structure across all game domains (enemies, items, classes, abilities, quests, etc.). This provides:

1. **Trait arrays** - Key-value pairs instead of nested objects
2. **Type inheritance** - Type-level traits apply to all items
3. **Consistent hierarchy** - Same structure for all catalogs
4. **Universal rarity** - Numerical rarity (1-100) maps to tiers

---

## Core Principles

### 1. Trait Arrays (Key-Value Pairs)

**All gameplay data uses trait arrays:**
```json
"traits": [
  { "key": "level", "value": 3 },
  { "key": "health", "value": 30 },
  { "key": "attack", "value": 8 }
]
```

**NOT nested objects:**
```json
"traits": {
  "level": 3,
  "health": 30
}
```

### 2. Root-Level vs Traits

**Root level = Meta/Structural:**
- `slug` - Unique identifier
- `name` - Display name
- `rarity` - Numerical value (1-100) 
- `rarityWeight` - Spawn/selection weight (catalogs)
- `selectionWeight` - Pattern selection weight (names.json)

**Traits = Gameplay:**
- Stats: level, health, attack, defense, speed
- Attributes: strength, dexterity, constitution, intelligence, wisdom, charisma
- Combat: abilities, resistances, vulnerabilities
- Behavior: category, size, damageType, habitat

### 3. Type Inheritance

Type-level traits are **inherited** by all items in that type:

```json
"wolves": {
  "traits": [
    { "key": "category", "value": "beast" },
    { "key": "size", "value": "medium" },
    { "key": "damageType", "value": "piercing" }
  ],
  "items": [
    {
      "slug": "wolf",
      "name": "Wolf",
      "rarity": 15,
      "rarityWeight": 5,
      "traits": [
        { "key": "level", "value": 3 },
        { "key": "health", "value": 30 }
      ]
    }
  ]
}
```

The "Wolf" inherits `category`, `size`, and `damageType` from type-level.

---

## Universal Rarity System

**Numerical rarity (1-100) maps to tiers via `rarity_config.json`:**

| Rarity Value | Tier | Stat Multiplier | Spawn Rate |
|-------------|------|-----------------|------------|
| 1-20 | Common | 1.0x | 60% |
| 21-40 | Uncommon | 1.3x | 25% |
| 41-60 | Rare | 1.7x | 10% |
| 61-80 | Elite | 2.5x | 4% |
| 81-100 | Legendary | 4.0x | 1% |

**Usage:**
- **Items**: Rarity calculated from component weights
- **Enemies**: Rarity directly assigned, stats scale by multiplier
- **All domains**: Same tier classification and color coding

---

## File Structure Template

### Metadata Block
```json
{
  "metadata": {
    "description": "Brief description of this catalog",
    "version": "5.0",
    "lastUpdated": "2026-01-08",
    "type": "enemy_catalog | item_catalog | ability_catalog | etc.",
    "total_types": 4,
    "total_items": 20
  }
}
```

### Type + Items Structure
```json
{
  "metadata": { ... },
  
  "enemy_types": {
    "wolves": {
      "traits": [
        { "key": "category", "value": "beast" },
        { "key": "size", "value": "medium" },
        { "key": "behavior", "value": "pack" },
        { "key": "damageType", "value": "piercing" },
        { "key": "habitat", "value": "forest" }
      ],
      "items": [
        {
          "slug": "wolf",
          "name": "Wolf",
          "rarity": 15,
          "rarityWeight": 5,
          "traits": [
            { "key": "level", "value": 3 },
            { "key": "xp", "value": 50 },
            { "key": "health", "value": 30 },
            { "key": "attack", "value": 8 },
            { "key": "defense", "value": 3 },
            { "key": "speed", "value": 15 },
            { "key": "strength", "value": 14 },
            { "key": "dexterity", "value": 15 },
            { "key": "constitution", "value": 14 },
            { "key": "intelligence", "value": 4 },
            { "key": "wisdom", "value": 12 },
            { "key": "charisma", "value": 6 },
            { "key": "abilities", "value": ["@abilities/active/offensive:bite"] },
            { "key": "abilityUnlocks", "value": { "5": ["@abilities/active/support:pack-tactics"] } }
          ]
        },
        {
          "slug": "timber-wolf",
          "name": "Timber Wolf",
          "rarity": 30,
          "rarityWeight": 20,
          "traits": [
            { "key": "level", "value": 5 },
            { "key": "xp", "value": 75 },
            { "key": "health", "value": 40 },
            { "key": "attack", "value": 10 }
          ]
        }
      ]
    },
    
    "goblins": {
      "traits": [
        { "key": "category", "value": "humanoid" },
        { "key": "size", "value": "small" }
      ],
      "items": [ /* goblin variants */ ]
    }
  }
}
```

---

## Domain-Specific Examples

### Enemies
```json
"enemy_types": {
  "wolves": {
    "traits": [ /* type-level traits */ ],
    "items": [ /* wolf variants */ ]
  }
}
```

### Items
```json
"weapon_types": {
  "swords": {
    "traits": [
      { "key": "weaponType", "value": "melee" },
      { "key": "damageType", "value": "slashing" },
      { "key": "twoHanded", "value": false }
    ],
    "items": [ /* sword variants */ ]
  }
}
```

### Abilities
```json
"ability_types": {
  "offensive": {
    "traits": [
      { "key": "category", "value": "active" },
      { "key": "targetType", "value": "enemy" }
    ],
    "items": [ /* offensive abilities */ ]
  }
}
```

### Classes
```json
"class_types": {
  "warriors": {
    "traits": [
      { "key": "primaryAttribute", "value": "strength" },
      { "key": "hitDiceType", "value": "d10" }
    ],
    "items": [ /* warrior subclasses */ ]
  }
}
```

---

## Migration Checklist

For each JSON catalog file:

**File Structure:**
- [ ] Update `metadata.version` to "5.0"
- [ ] Update `metadata.lastUpdated` to "2026-01-08"
- [ ] Rename top-level object to `{domain}_types` (e.g., `enemy_types`, `weapon_types`)

**Type Level:**
- [ ] Create `traits` array for each type
- [ ] Move descriptive/inherited properties to type `traits`
- [ ] Create `items` array for type variants

**Item Level:**
- [ ] Keep `slug`, `name`, `rarity`, `rarityWeight` at root
- [ ] Create `traits` array for each item
- [ ] Move ALL gameplay data into `traits` array as key-value pairs
- [ ] Convert nested objects to single key-value entries (arrays/objects as values)

**Rarity:**
- [ ] Use numerical `rarity` (1-100) instead of strings
- [ ] Ensure 5 tiers represented: common(1-20), uncommon(21-40), rare(41-60), elite(61-80), legendary(81-100)
- [ ] Add boss/quest-specific enemies at legendary tier (81-100)

---

## Benefits

1. **Consistency** - Same structure across all domains
2. **Flexibility** - Easy to add/modify traits without schema changes
3. **Inheritance** - Type-level traits reduce duplication
4. **Scalability** - Trait arrays support any number of properties
5. **Queryability** - Uniform key-value structure simplifies searches
6. **Extensibility** - Add metadata to traits (conditions, modifiers, sources)

---

## Validation Rules

**Required at File Level:**
- `metadata` object with version "5.0"
- At least one `{domain}_types` object

**Required at Type Level:**
- `traits` array (can be empty)
- `items` array with at least one item

**Required at Item Level:**
- `slug` (string, unique within type)
- `name` (string)
- `rarity` (number, 1-100)
- `rarityWeight` (number, 1-100) - for catalogs only
- `traits` array (can be empty)
