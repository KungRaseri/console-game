# JSON Data Standard v5.1

**Status**: ACTIVE  
**Date**: January 8, 2026  
**Scope**: Universal structure for ALL game data JSON files

---

## Overview

Version 5.1 introduces a **clear separation of attributes, stats, properties, and traits** that standardizes data structure across all game domains (enemies, items, classes, abilities, quests, etc.). This provides:

1. **Attributes** - The 6 core D&D-style attributes (STR, DEX, CON, INT, WIS, CHA)
2. **Stats** - Calculated values derived from attributes (health, damage, defense, speed)
3. **Properties** - Type-level descriptive characteristics (weaponType, size, behavior)
4. **Traits** - Item-level special modifiers and bonuses (socketCount, attribute bonuses)
5. **Formula-based stats** - Runtime calculation with attribute modifiers
6. **Universal rarity** - Numerical rarity (1-100) maps to tiers

---

## Core Principles

### 1. Four Distinct Sections

**Attributes (Always the standard 6):**
```json
"attributes": {
  "strength": 14,
  "dexterity": 15,
  "constitution": 14,
  "intelligence": 4,
  "wisdom": 12,
  "charisma": 6
}
```

**Stats (Calculated from attributes):**
```json
"stats": {
  "health": "constitution_mod * 2 + level * 5 + 10",
  "attack": "strength_mod + level",
  "defense": "10 + dexterity_mod + constitution_mod",
  "speed": "30 + dexterity_mod * 5"
}
```

**Properties (Type-level, descriptive):**
```json
"properties": {
  "size": "medium",
  "behavior": "pack",
  "habitat": "forest"
}
```

**Traits (Item-level, modifiers):**
```json
"traits": {
  "packLeader": true,
  "resistances": ["cold"],
  "legendary": true
}
```

### 2. D&D-Style Attribute Modifiers

**Modifier Calculation:** `(Attribute - 10) / 2` (rounded down)

Examples:
- STR 14 = +2 modifier (`strength_mod`)
- DEX 15 = +2 modifier (`dexterity_mod`)
- CON 14 = +2 modifier (`constitution_mod`)
- INT 4 = -3 modifier (`intelligence_mod`)

**In formulas, use `attribute_mod` notation:**
```json
"health": "constitution_mod * 2 + level * 5 + 10"
// Evaluates to: 2 * 2 + 3 * 5 + 10 = 29
```

### 3. Type-Level Properties

Properties shared by all items in a type go at type level (parsed from type key when redundant):

```json
"weapon_types": {
  "heavy-blades": {      // weaponType = "heavy-blades" (parsed from key)
    "properties": {
      "damageType": "slashing",
      "range": "melee",
      "slot": "mainhand",
      "skillReference": "@skills/weapon:heavy-blades"
    },
    "items": [ /* all heavy-blades */ ]
  }
}
```

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
    "version": "5.1",
    "lastUpdated": "2026-01-08",
    "type": "enemy_catalog | weapon_catalog | armor_catalog | etc.",
    "total_types": 4,
    "total_items": 20
  }
}
```

### Enemy Structure
```json
{
  "metadata": { ... },
  
  "enemy_types": {
    "wolves": {
      "properties": {
        "size": "medium",
        "behavior": "pack",
        "habitat": "forest"
      },
      "items": [
        {
          "slug": "wolf",
          "name": "Wolf",
          "rarity": 15,
          "rarityWeight": 5,
          
          "level": 3,
          "xp": 50,
          
          "attributes": {
            "strength": 14,
            "dexterity": 15,
            "constitution": 14,
            "intelligence": 4,
            "wisdom": 12,
            "charisma": 6
          },
          
          "stats": {
            "health": "constitution_mod * 2 + level * 5 + 10",
            "attack": "strength_mod + level",
            "defense": "10 + dexterity_mod + constitution_mod",
            "speed": "30 + dexterity_mod * 5"
          },
          
          "combat": {
            "abilities": ["@abilities/active/offensive:bite"],
            "abilityUnlocks": {
              "5": ["@abilities/active/support:pack-tactics"]
            }
          },
          
          "traits": {}
        }
      ]
    }
  }
}
```

### Weapon Structure
```json
{
  "weapon_types": {
    "heavy-blades": {
      "properties": {
        "damageType": "slashing",
        "range": "melee",
        "slot": "mainhand",
        "category": "melee_one_handed",
        "skillReference": "@skills/weapon:heavy-blades"
      },
      "items": [
        {
          "slug": "longsword",
          "name": "Longsword",
          "rarity": 20,
          "selectionWeight": 20,
          
          "attributes": {
            "strength": 10    // Requirement: need STR 10+ to equip
          },
          
          "stats": {
            "damage": {
              "min": 1,
              "max": 8,
              "modifier": "wielder.strength_mod"
            },
            "attackSpeed": 1.0,
            "weight": 3.0,
            "value": 15
          },
          
          "traits": {
            "strength": 1,       // Grants +1 STR when equipped
            "twoHanded": false,
            "socketCount": 1
          }
        }
      ]
    }
  }
}
```

---

## Formula Reference System

### Context-Aware References

**For items:**
- `wielder.attribute_mod` - Character wielding weapon
- `wearer.attribute_mod` - Character wearing armor
- `consumer.attribute_mod` - Character consuming item

**For abilities:**
- `caster.attribute_mod` - Character casting spell/using ability
- `target.attribute_mod` - Target of ability/spell

**For enemies:**
- `attribute_mod` - This enemy's attribute modifier
- `level` - This enemy's level

**Examples:**
```json
// Weapon damage scales with wielder
"damage": {
  "min": 1,
  "max": 8,
  "modifier": "wielder.strength_mod"
}

// Enemy health scales with own stats
"health": "constitution_mod * 2 + level * 5 + 10"

// Potion healing scales with consumer
"healing": "20 + consumer.constitution_mod * 2"
```

---

## Structured Damage Objects

For range-based values (weapon damage, ability damage), use structured format:

```json
"damage": {
  "min": 1,
  "max": 8,
  "modifier": "wielder.strength_mod"
}
```

**NOT string format:** `"damage": "1-8 + wielder.strength_mod"`

**Benefits:**
- Type-safe validation (min <= max)
- Easy to parse and evaluate
- Clear and explicit
- No regex needed

---

## Domain-Specific Examples

### Armor1"
- [ ] Update `metadata.lastUpdated` to "2026-01-08"
- [ ] Update `metadata.type` to match domain (enemy_catalog, weapon_catalog, etc.)

**Type Level:**
- [ ] Create `properties` object for shared descriptive characteristics
- [ ] Remove redundant properties that can be parsed from type key
- [ ] Create `items` array for type variants

**Item Level - Root:**
- [ ] Keep `slug`, `name`, `rarity` at root
- [ ] Keep `rarityWeight` or `selectionWeight` at root
- [ ] Add `level` and `xp` at root for enemies

**Item Level - Attributes:**
- [ ] Create `attributes` object with the 6 standard attributes
- [ ] For items: attributes = requirements (minimum to equip)
- [ ] For enemies: attributes = base stats for calculation

**Item Level - Stats:**
- [ ] Create `stats` object with calculated values
- [ ] Use formula strings with `attribute_mod` notation
- [ ] Use structured damage objects: `{ "min": 1, "max": 8, "modifier": "..." }`
- [ ] Include context references: `wielder.`, `wearer.`, `consumer.`, `caster.`

**Item Level - Combat (Enemies only):**
- [ ] Create `combat` object with `abilities` and `abilityUnlocks`
- [ ] Abilities reference using v4.1 format: `@abilities/...`

**Item Level - Traits:**
- [ ] Create `traits` object for special properties
- [ ] Attribute bonuses go here (strength: 2 grants +2 STR)
- [ ] Special flags (packLeader, legendary, socketCount, etc.)
- [ ] Keep empty object `{}` if no traits

**Rarity:**
- [ ] Use numerical `rarity` (1-100) instead of strings
- [ ] Ensure 5 tiers represented: common(1-20), uncommon(21-40), rare(41-60), elite(61-80), legendary
        "stats": {
          "defense": "5 + wearer.dexterity_mod",
          "dodgeBonus": "wearer.dexterity_mod * 0.5",
          "weight": 10.0,
          "value": 25
        },
        
        "traits": {
          "dexterity": 1,
          "constitution": 1,
          "stealthPenalty": 0
        }
      }
    ]
  }
}
```

### Consumables
```json
"consumable_types": {
  "potions": {
    "properties": {
      "consumableType": "potion",
      "slot": "consumable"
    },
    "items": [
      {
        "slug": "healing-potion",
        "name": "Healing Potion",
        "rarity": 10,
        "selectionWeight": 80,
        
        "attributes": {},
        
        "stats": {
          "healing": "20 + consumer.constitution_mod * 2",
          "duration": 0,
          "value": 10
        },
        
        "traits": {
          "stackSize": 20,
          "effectType": "healing"
        }
      }
    ]
  }
}
```

### Abilities
```json
"ability_types": {
  "offensive": {
    "properties": {
      "category": "active",
      "targetType": "enemy"
    },
    "items": [
      {
        "slug": "bite",
        "name": "Bite",
        "rarity": 10,
        
        "attributes": {},
        
        "stats": {
          "damage": {
            "min": 1,
            "max": 6,
            "modifier": "caster.strength_mod"
          },
          "manaCost": 0,
          "cooldown": 0
        },
        
        "traits": {
          "damageType": "piercing",
          "accuracy": "caster.dexterity_mod"
        }
      }
    ]
  }
}

---

## Migration Checklist

For each JSON catalog file:

**File Structure:**
- [ ] Update `metadata.version` to "5.0"
- [ ] Update `metadata.lastUpdated` to "2026-01-08"
- [ ] Rename top-level object to `{domain}_types` (e.g., `enemy_types`, `weapon_types`)

**Type Level:**
- [ ] larity** - Clear separation of attributes, stats, properties, traits
2. **Consistency** - Same structure across all domains
3. **Flexibility** - Formula-based stats easy to tune globally
4. **D&D Compatibility** - Standard attribute modifiers and calculations
5. **Type Safety** - Structured damage objects, explicit formulas
6. **Inheritance** - Type-level properties reduce duplication
7. **Extensibility** - Easy to add new stats, attributes, traits

---

## Validation Rules

**Required at File Level:**
- `metadata` object with version "5.1"
- At least one `{domain}_types` object

**Required at Type Level:**
- `properties` object (can be empty)
- `items` array with at least one item

**Required at Item Level:**
- `slug` (string, unique within type)
- `name` (string)
- `rarity` (number, 1-100)
- `rarityWeight` or `selectionWeight` (number, 1-100)
- `attributes` object (can be empty)
- `stats` object (can be empty)
- `traits` object (can be empty)

**Formula Validation:**
- Must reference valid attributes (`strength_mod`, `dexterity_mod`, etc.)
- Must use valid context (`wielder.`, `wearer.`, `consumer.`, `caster.`, `target.`)
- Damage objects must have `min`, `max`, and `modifier` fields
- `min` must be <= `max`

**Attribute Validation:**
- Must be one of: strength, dexterity, constitution, intelligence, wisdom, charisma
- Values must be positive integers (typically 1-30)
- Modifiers calculated as: `(value - 10) / 2` (rounded downadata to traits (conditions, modifiers, sources)

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
