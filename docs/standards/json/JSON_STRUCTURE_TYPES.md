# JSON Structure Types v4.0

**Version**: 4.0  
**Last Updated**: December 27, 2025  
**Status**: Standard

## Overview

This document defines the five core JSON structure types used throughout the game data system. Each type serves a specific purpose and follows a standardized schema to ensure consistency, maintainability, and compatibility with the ContentBuilder UI.

---

## 1. CATALOG Structure

### Purpose
Define discrete, selectable entities with properties and rarity-weighted selection.

### Use Cases
- Abilities, enemies, items, weapons, armor
- Any entity that players/NPCs can acquire or encounter
- Entities with statistical properties and gameplay effects

### Schema
```json
{
  "metadata": {
    "description": "string (required)",
    "version": "4.0 (required)",
    "lastUpdated": "YYYY-MM-DD (required)",
    "type": "string ending with _catalog (required)",
    "supportsTraits": "boolean (optional)",
    "notes": ["array of strings (optional)"],
    "totalItems": "number (optional)"
  },
  "items": [
    {
      "name": "string (required, unique identifier)",
      "displayName": "string (optional, human-readable)",
      "description": "string (required)",
      "rarityWeight": "number (required, higher = more common)",
      "traits": {
        "traitName": {
          "value": "any type",
          "type": "string describing value type"
        }
      },
      "customFields": "any additional properties"
    }
  ]
}
```

### Key Requirements
- All items MUST have `name` and `rarityWeight`
- Use `rarityWeight` for selection probability (NOT "weight")
- Probability formula: `selectionChance = 100 / rarityWeight`
- Lower rarityWeight = rarer items (500 = ultra-rare, 5 = very common)
- Physical "weight" (item mass) is allowed as a separate property

### Examples
**Simple Catalog**:
```json
{
  "metadata": {
    "description": "Basic weapon catalog",
    "version": "4.0",
    "lastUpdated": "2025-12-27",
    "type": "weapon_catalog",
    "supportsTraits": true
  },
  "items": [
    {
      "name": "Iron Sword",
      "description": "A basic iron sword",
      "rarityWeight": 5,
      "baseDamage": 10,
      "weight": 3.5,
      "traits": {
        "damageType": {"value": "slashing", "type": "string"},
        "durability": {"value": 100, "type": "number"}
      }
    }
  ]
}
```

---

## 2. PATTERN_GENERATION Structure

### Purpose
Procedural name generation using component assembly and pattern templates.

### Use Cases
- All `names.json` files
- Dynamic item/NPC/ability name generation
- Trait-based name modification

### Schema
```json
{
  "metadata": {
    "version": "4.0 (required)",
    "type": "pattern_generation (required)",
    "supportsTraits": "true (required)",
    "lastUpdated": "YYYY-MM-DD (required)",
    "description": "string (required)",
    "notes": ["array (optional)"]
  },
  "components": {
    "componentType": [
      {
        "value": "string (required)",
        "traits": {
          "traitName": {
            "value": "any type",
            "type": "string"
          }
        }
      }
    ]
  },
  "patterns": [
    {
      "value": "string with {tokens} (required)",
      "rarityWeight": "number (required)",
      "traits": {
        "optional pattern-specific traits"
      }
    }
  ]
}
```

### Key Requirements
- `supportsTraits` MUST be `true`
- Component tokens: `{base}`, `{prefix}`, `{suffix}`, `{quality}`, etc.
- External references: `[@materialRef/weapon]`, `[@materialRef/armor]`
- NO "example" fields allowed
- Use `rarityWeight` in patterns (NOT "weight")

### Pattern Syntax
- `{componentType}` - References a component array
- `[@reference/path]` - External catalog reference
- Combine multiple tokens: `{prefix} {base} {suffix}`

### Examples
**Basic Pattern Generation**:
```json
{
  "metadata": {
    "version": "4.0",
    "type": "pattern_generation",
    "supportsTraits": true,
    "lastUpdated": "2025-12-27",
    "description": "Weapon name generation"
  },
  "components": {
    "prefix": [
      {
        "value": "Sharp",
        "traits": {
          "damageBonus": {"value": 5, "type": "number"}
        }
      }
    ],
    "base": [
      {
        "value": "Sword",
        "traits": {}
      }
    ],
    "suffix": [
      {
        "value": "of Fire",
        "traits": {
          "elementalDamage": {"value": "fire", "type": "string"}
        }
      }
    ]
  },
  "patterns": [
    {
      "value": "{base}",
      "rarityWeight": 40,
      "traits": {}
    },
    {
      "value": "{prefix} {base}",
      "rarityWeight": 35,
      "traits": {}
    },
    {
      "value": "{base} {suffix}",
      "rarityWeight": 20,
      "traits": {}
    },
    {
      "value": "{prefix} {base} {suffix}",
      "rarityWeight": 5,
      "traits": {}
    }
  ]
}
```

**External References**:
```json
{
  "patterns": [
    {
      "value": "[@materialRef/weapon] {base}",
      "rarityWeight": 30,
      "traits": {}
    }
  ]
}
```

---

## 3. COMPONENT_LIBRARY Structure

### Purpose
Reusable data arrays without complex selection logic or gameplay properties.

### Use Cases
- Adjectives, colors, sounds, verbs, textures
- UI element lists (icons, themes)
- Simple lookup tables
- Flavor text collections

### Schema
```json
{
  "metadata": {
    "version": "4.0 (required)",
    "type": "component_library (required)",
    "lastUpdated": "YYYY-MM-DD (required)",
    "description": "string (required)",
    "notes": ["array (optional)"],
    "totalComponents": "number (optional)"
  },
  "components": {
    "categoryName": [
      "value1",
      "value2",
      "value3"
    ],
    "anotherCategory": [
      "valueA",
      "valueB"
    ]
  }
}
```

### Key Requirements
- Simple string arrays (no complex objects unless needed)
- No `rarityWeight` (use uniform random selection)
- Categorized by logical groups
- Can be referenced by other JSON files

### Examples
**Simple Component Library**:
```json
{
  "metadata": {
    "version": "4.0",
    "type": "component_library",
    "lastUpdated": "2025-12-27",
    "description": "Descriptive adjectives for item generation",
    "totalComponents": 25
  },
  "components": {
    "positive": [
      "Magnificent",
      "Exquisite",
      "Pristine",
      "Flawless"
    ],
    "negative": [
      "Broken",
      "Damaged",
      "Worn"
    ],
    "size": [
      "Tiny",
      "Massive",
      "Colossal"
    ]
  }
}
```

**Complex Component Library** (with objects):
```json
{
  "metadata": {
    "version": "4.0",
    "type": "component_library",
    "lastUpdated": "2025-12-27",
    "description": "NPC personality traits"
  },
  "components": {
    "personality": [
      {
        "trait": "Brave",
        "opposite": "Cowardly",
        "description": "Faces danger without fear"
      },
      {
        "trait": "Greedy",
        "opposite": "Generous",
        "description": "Hoards wealth and resources"
      }
    ]
  }
}
```

---

## 4. CONFIG Structure

### Purpose
Game rules, configuration values, and system settings.

### Use Cases
- Rarity tier definitions
- Drop rate tables
- Level-up progression tables
- Game balance parameters
- System constants

### Schema
```json
{
  "metadata": {
    "version": "4.0 (required)",
    "type": "config (required)",
    "lastUpdated": "YYYY-MM-DD (required)",
    "description": "string (required)",
    "notes": ["array (optional)"]
  },
  "settings": {
    "categoryName": {
      "settingKey": "value",
      "nestedSettings": {
        "key": "value"
      }
    }
  }
}
```

### Key Requirements
- Flat or nested key-value structure
- No `rarityWeight` (these are constants, not selectable entities)
- Document units and ranges in metadata notes
- Use semantic naming for setting keys

### Examples
**Rarity Configuration**:
```json
{
  "metadata": {
    "version": "4.0",
    "type": "config",
    "lastUpdated": "2025-12-27",
    "description": "Rarity tier definitions and drop rates",
    "notes": [
      "rarityWeight ranges define tiers",
      "dropRateMultiplier affects loot quality"
    ]
  },
  "settings": {
    "rarityTiers": {
      "common": {
        "weightRange": [1, 10],
        "color": "white",
        "dropRateMultiplier": 1.0
      },
      "uncommon": {
        "weightRange": [11, 50],
        "color": "green",
        "dropRateMultiplier": 0.7
      },
      "rare": {
        "weightRange": [51, 150],
        "color": "blue",
        "dropRateMultiplier": 0.3
      },
      "epic": {
        "weightRange": [151, 350],
        "color": "purple",
        "dropRateMultiplier": 0.1
      },
      "legendary": {
        "weightRange": [351, 1000],
        "color": "orange",
        "dropRateMultiplier": 0.01
      }
    },
    "levelScaling": {
      "xpPerLevel": 100,
      "statIncreasePerLevel": 5,
      "maxLevel": 100
    }
  }
}
```

**Class Progression Config**:
```json
{
  "metadata": {
    "version": "4.0",
    "type": "config",
    "lastUpdated": "2025-12-27",
    "description": "Warrior class progression table"
  },
  "settings": {
    "baseStats": {
      "health": 120,
      "mana": 50,
      "strength": 15,
      "intelligence": 5
    },
    "levelProgression": {
      "healthPerLevel": 10,
      "manaPerLevel": 2,
      "strengthPerLevel": 2,
      "intelligencePerLevel": 0.5
    },
    "abilityUnlocks": {
      "1": ["Basic Attack", "Defend"],
      "5": ["Power Strike"],
      "10": ["Whirlwind"],
      "20": ["Berserker Rage"]
    }
  }
}
```

---

## 5. HIERARCHICAL_CATALOG Structure

### Purpose
Multi-level categorized entities with complex relationships and subcategories.

### Use Cases
- NPCs (backgrounds + occupations)
- Quests (templates + locations + objectives)
- Classes (archetypes + subclasses)
- Enemies (by type/family)

### Schema
```json
{
  "metadata": {
    "description": "string (required)",
    "version": "4.0 (required)",
    "lastUpdated": "YYYY-MM-DD (required)",
    "type": "hierarchical_catalog (required)",
    "componentKeys": ["array of top-level keys"],
    "notes": ["array (optional)"]
  },
  "categoryName": {
    "metadata": {
      "description": "string (optional)",
      "notes": ["category-specific notes"]
    },
    "items": [
      {
        "name": "string (required)",
        "rarityWeight": "number (required)",
        "traits": {},
        "customFields": "category-specific properties"
      }
    ]
  },
  "anotherCategory": {
    "metadata": {...},
    "items": [...]
  }
}
```

### Key Requirements
- Top-level categories each have their own `items` array
- Each category can have its own `metadata`
- All items within categories follow standard catalog item schema
- Use `componentKeys` in top-level metadata to list categories
- Categories represent logical groupings (not just arbitrary splits)

### Examples
**NPC Backgrounds + Occupations**:
```json
{
  "metadata": {
    "type": "hierarchical_catalog",
    "version": "4.0",
    "description": "NPC backgrounds and occupations",
    "lastUpdated": "2025-12-27",
    "componentKeys": ["backgrounds", "occupations"],
    "notes": [
      "NPCs can have both background AND occupation",
      "Example: 'Noble-born Blacksmith'"
    ]
  },
  "backgrounds": {
    "metadata": {
      "description": "Past history and origins"
    },
    "items": [
      {
        "name": "Noble",
        "displayName": "Noble",
        "description": "Born into wealth and privilege",
        "rarityWeight": 100,
        "startingGold": "5d20",
        "socialClass": "upper"
      }
    ]
  },
  "occupations": {
    "metadata": {
      "description": "Current profession and skills"
    },
    "items": [
      {
        "name": "Blacksmith",
        "displayName": "Blacksmith",
        "description": "Works metal into weapons and armor",
        "rarityWeight": 20,
        "hasShop": true,
        "shopInventory": ["weapons", "armor"]
      }
    ]
  }
}
```

**Quest Templates + Locations**:
```json
{
  "metadata": {
    "type": "hierarchical_catalog",
    "version": "4.0",
    "description": "Quest templates and locations",
    "lastUpdated": "2025-12-27",
    "componentKeys": ["templates", "locations"]
  },
  "templates": {
    "metadata": {
      "description": "Quest type templates"
    },
    "items": [
      {
        "name": "fetch_herbs",
        "questType": "fetch",
        "difficulty": "easy",
        "rarityWeight": 5,
        "objectives": ["Collect {count} {item} from {location}"],
        "baseReward": {
          "gold": "2d10",
          "xp": 50
        }
      }
    ]
  },
  "locations": {
    "metadata": {
      "description": "Quest locations"
    },
    "items": [
      {
        "name": "Dark Forest",
        "locationType": "wilderness",
        "dangerLevel": "medium",
        "rarityWeight": 15
      }
    ]
  }
}
```

---

## Structure Selection Guide

### Decision Tree

```
Is it game configuration/rules/constants?
├─ YES → CONFIG
└─ NO ↓

Does it define selectable game entities?
├─ YES → Are there multiple logical categories of entities?
│   ├─ YES → HIERARCHICAL_CATALOG
│   └─ NO → CATALOG
└─ NO ↓

Does it generate procedural names/text?
├─ YES → PATTERN_GENERATION
└─ NO ↓

Is it simple data arrays for reference?
└─ YES → COMPONENT_LIBRARY
```

### Quick Reference Table

| Structure Type | Has rarityWeight | Has Traits | Multiple Categories | Selection Logic |
|----------------|------------------|------------|---------------------|-----------------|
| CATALOG | ✅ Required | Optional | No | Weighted random |
| PATTERN_GENERATION | ✅ In patterns | ✅ Required | In components | Pattern assembly |
| COMPONENT_LIBRARY | ❌ No | ❌ No | ✅ Multiple | Uniform random |
| CONFIG | ❌ No | ❌ No | ✅ Settings groups | Direct lookup |
| HIERARCHICAL_CATALOG | ✅ Per item | Optional | ✅ Top-level | Weighted per category |

---

## Common Fields Reference

### Metadata (All Types)
```json
{
  "metadata": {
    "version": "4.0",              // REQUIRED - Always "4.0"
    "type": "structure_type",      // REQUIRED - One of the 5 types
    "lastUpdated": "YYYY-MM-DD",   // REQUIRED - ISO date
    "description": "string",       // REQUIRED - Purpose of file
    "notes": ["string array"]      // OPTIONAL - Additional context
  }
}
```

### rarityWeight Best Practices
- **5-10**: Very common (basic items, common enemies)
- **15-35**: Common (standard equipment, frequent encounters)
- **50-100**: Uncommon (quality items, special enemies)
- **150-350**: Rare (magical items, boss variants)
- **500+**: Ultra-rare (legendary items, unique enemies)

### Trait Format (CATALOG & PATTERN_GENERATION)
```json
{
  "traits": {
    "traitName": {
      "value": "actual value (any type)",
      "type": "string|number|boolean|object|array"
    }
  }
}
```

---

## Validation Checklist

### All Files
- [ ] `metadata.version` is "4.0"
- [ ] `metadata.type` matches structure type
- [ ] `metadata.lastUpdated` is valid ISO date (YYYY-MM-DD)
- [ ] `metadata.description` exists and is descriptive
- [ ] File uses `rarityWeight` (NOT "weight") where applicable

### CATALOG Specific
- [ ] All items have `name` and `rarityWeight`
- [ ] `metadata.type` ends with `_catalog`
- [ ] Items have meaningful `description` fields

### PATTERN_GENERATION Specific
- [ ] `metadata.supportsTraits` is `true`
- [ ] `components` object exists with at least one category
- [ ] `patterns` array exists with valid token syntax
- [ ] NO "example" fields anywhere
- [ ] External references use `[@reference/path]` syntax

### COMPONENT_LIBRARY Specific
- [ ] `components` object with categorized arrays
- [ ] NO `rarityWeight` in items
- [ ] Arrays contain simple values (unless complex objects needed)

### CONFIG Specific
- [ ] `settings` object exists
- [ ] Values are constants (not selectable entities)
- [ ] Units/ranges documented in metadata notes

### HIERARCHICAL_CATALOG Specific
- [ ] `metadata.componentKeys` lists all top-level categories
- [ ] Each category has `items` array
- [ ] All items follow standard catalog schema

---

## Version History

### v4.0 (2025-12-27)
- Defined five core structure types
- Established schema standards for each type
- Created validation requirements
- Added decision tree and selection guide

---

## Related Documentation

- [NAMES_JSON_STANDARD.md](./NAMES_JSON_STANDARD.md) - PATTERN_GENERATION details
- [CATALOG_JSON_STANDARD.md](./CATALOG_JSON_STANDARD.md) - CATALOG details
- [CBCONFIG_STANDARD.md](./CBCONFIG_STANDARD.md) - ContentBuilder UI configuration
- [README.md](./README.md) - JSON standards overview
