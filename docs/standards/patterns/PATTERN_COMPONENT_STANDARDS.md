# Pattern System Component Standards

**Date:** December 16, 2025  
**Version:** 1.1  
**Status:** ✅ Phase 1 Complete - Ready for Phase 2 Implementation

## Executive Summary

**All 113 JSON data files have been standardized** with consistent metadata, proper structure, and weight-based rarity system.

### Completion Status

| Category | Files | Status | Structure Types |
|----------|-------|--------|-----------------|
| **General** | 9 | ✅ Complete | Component Library, Pattern Generation, Configuration |
| **Items** | 17 | ✅ Complete | Catalogs, Names, Prefixes, Suffixes, Materials |
| **Enemies** | 59 | ✅ Complete | 13 enemy types with full trait systems |
| **NPCs** | 14 | ✅ Complete | Names, Occupations, Personalities, Dialogue |
| **Quests** | 14 | ✅ Complete | Templates, Objectives, Rewards, Locations |
| **TOTAL** | **113** | **✅ 100%** | All files standardized |

### Next Phase

**Phase 2: ContentBuilder Integration** - Update the WPF tool to support all standardized file types:

- Pattern validation for names.json files
- Live example preview with weight-based rarity
- Auto-generated metadata on save
- Support for all 113 files

### What Was Standardized

**Every JSON file now includes:**

1. **Metadata Block** - Consistent metadata with auto-generated fields:
   - `description` - Human-readable file purpose
   - `version` - Schema version number
   - `lastUpdated` - Timestamp (YYYY-MM-DD)
   - `type` - File type classification
   - Auto-generated counts (componentKeys, patternTokens, total_items, etc.)

2. **Structure Standardization** - All files follow one of these patterns:
   - **Pattern Generation** (names.json) - components + patterns for procedural generation
   - **Item/Enemy Catalogs** (types.json) - type-level traits + item arrays with stats
   - **Prefix/Suffix Modifiers** - rarity-organized stat modifiers
   - **Component Libraries** - categorized reference data (no patterns)
   - **Configuration Files** - game settings and rules

3. **Weight-Based Rarity** - All components and items have `rarityWeight` values:
   - Components contribute to emergent rarity calculation
   - No hardcoded rarity tiers in most files
   - Rarity emerges from combined component weights
   - Configured via `general/rarity_config.json`

4. **Consistent Naming** - Component keys match pattern tokens exactly:
   - Singular keys (material, not materials)
   - Semantic names (descriptive, not prefix_desc)
   - Universal components (material, quality, descriptive) work across categories
   - Category-specific components only where needed

---

## Purpose

This document defines the standard component keys and patterns for all JSON data files in the game. These standards ensure:

- Pattern tokens match component keys exactly
- Consistent naming across all categories
- Runtime pattern execution works correctly
- ContentBuilder can validate patterns

## Standard Component Keys

### Universal Prefix Components

These can be used across ANY category (items, enemies, NPCs, etc.):

| Component Key | Description | Examples | Use Case |
|--------------|-------------|----------|----------|
| `material` | Physical material/substance | Iron, Steel, Wood, Cloth, Stone | Weapons, armor, items |
| `quality` | Craftsmanship/condition level | Fine, Superior, Masterwork, Ruined | All items, equipment |
| `descriptive` | Special attributes/adjectives | Ancient, Enchanted, Cursed, Blazing | All categories |
| `size` | Size modifiers | Small, Large, Massive, Tiny, Gigantic | Items, enemies |
| `color` | Color descriptors | Red, Blue, Golden, Silver, Dark | Items, enemies, NPCs |
| `origin` | Creator/source | Elven, Dwarven, Dragon, Demonic, Celestial | Items, enemies |
| `condition` | State/age | Worn, Pristine, Ruined, Ancient, New | Items only |

### Universal Suffix Components

These typically start with "of" and add flavor:

| Component Key | Description | Examples | Use Case |
|--------------|-------------|----------|----------|
| `enchantment` | Magical properties | of Slaying, of Power, of Speed, of Fire | Magical items |
| `title` | Named/legendary items | of the Dragon, of the Hero, of Legends | Legendary items, NPCs |
| `purpose` | Intended use/function | of Defense, of Battle, of Healing | Functional items |

### Special Tokens

| Token | Resolves To | Description |
|-------|-------------|-------------|
| `base` | types.json items array | Picks random item.name from corresponding types.json file |

**Note:** The `base` token is the **only** special token. It always resolves to a random item from the corresponding `types.json` file's items array.

### Category-Specific Components

Organizational components (nested objects, not used in patterns):

| Component Key | Description | Example Structure |
|--------------|-------------|-------------------|
| `weapon_types` | Weapon categories | `{"swords": [...], "axes": [...]}` |
| `armor_types` | Armor slot categories | `{"helmets": [...], "chest": [...]}` |
| `enemy_types` | Enemy categories | `{"beasts": [...], "undead": [...]}` |
| `profession_types` | NPC professions | `{"merchants": [...], "warriors": [...]}` |

## Pattern Syntax

### Basic Rules

```
pattern ::= token | token " + " pattern
token   ::= "base" | component_key
```

- Tokens separated by ` + ` (space + plus + space)
- Tokens must match component keys EXACTLY (case-sensitive)
- Special token `base` picks from items array
- Invalid tokens are skipped with warning

### Pattern Examples

| Pattern | Example Output | Use Case |
|---------|---------------|----------|
| `base` | "Longsword" | Simple base item |
| `material + base` | "Iron Longsword" | Common items |
| `quality + material + base` | "Fine Steel Longsword" | Uncommon items |
| `descriptive + base` | "Ancient Longsword" | Special items |
| `base + enchantment` | "Longsword of Slaying" | Magical items |
| `quality + descriptive + base + title` | "Masterwork Enchanted Longsword of the Dragon" | Legendary items |

### Pattern Guidelines

1. **Simplest patterns first** - Start with `base`, then `material + base`, etc.
2. **Prefixes before base** - `quality + material + base`, not `base + quality + material`
3. **Suffixes after base** - `base + enchantment`, not `enchantment + base`
4. **Logical ordering** - `quality + material + base` (quality describes material, material describes base)
5. **Progressive complexity** - Common → Uncommon → Rare → Epic → Legendary

## Standard File Structure

### Structure by File Type

Different file types have different structures based on their purpose:

---

#### 1. types.json - Item/Enemy Catalog

**Purpose:** Base items with individual stats and shared type traits

**Structure:**

```json
{
  "[category]_types": {
    "[type_name]": {
      "items": [
        {
          "name": "ItemName1",
          "stat1": "value1",
          "stat2": 10,
          "stat3": true
        },
        {
          "name": "ItemName2",
          "stat1": "value2",
          "stat2": 15
        }
      ],
      "traits": {
        "sharedProperty1": "value",
        "sharedProperty2": "value"
      }
    }
  },
  "metadata": {
    "description": "Item catalog description",
    "version": "1.0",
    "lastUpdated": "2025-12-16"
  }
}
```

**Example (weapons/types.json):**

```json
{
  "weapon_types": {
    "swords": {
      "items": [
        { "name": "Shortsword", "damage": "1d6", "weight": 2.0, "value": 10 },
        { "name": "Longsword", "damage": "1d8", "weight": 3.0, "value": 15 }
      ],
      "traits": {
        "damageType": "slashing",
        "slot": "mainhand",
        "category": "sword"
      }
    }
  },
  "metadata": {
    "description": "Weapon type catalog with base stats",
    "version": "1.0"
  }
}
```

---

#### 2. names.json - Name Generation

**Purpose:** Pattern-based procedural name generation

**Structure:**

```json
{
  "components": {
    "component_key1": ["Value1", "Value2", "Value3"],
    "component_key2": ["Value1", "Value2"],
    "component_key3": ["Value1", "Value2"]
  },
  "patterns": [
    "base",
    "component_key1 + base",
    "component_key1 + component_key2 + base + component_key3"
  ],
  "metadata": {
    "description": "Name generation patterns and components",
    "version": "1.0",
    "componentKeys": ["component_key1", "component_key2", "component_key3"],
    "patternTokens": ["base", "component_key1", "component_key2", "component_key3"]
  }
}
```

**Note:** The `base` token resolves from corresponding `types.json` file.

**Example (weapons/names.json):**

```json
{
  "components": {
    "material": ["Iron", "Steel", "Mithril"],
    "quality": ["Fine", "Superior", "Masterwork"],
    "enchantment": ["of Slaying", "of Power"]
  },
  "patterns": [
    "base",
    "material + base",
    "quality + material + base + enchantment"
  ],
  "metadata": {
    "description": "Weapon name generation",
    "componentKeys": ["material", "quality", "enchantment"],
    "patternTokens": ["base", "material", "quality", "enchantment"]
  }
}
```

---

#### 3. names.json - Unified Naming with Traits (RECOMMENDED v4.0+)

**Purpose:** **Unified pattern-based name generation with prefix/suffix support and trait assignment**

**Philosophy:** Merge prefixes, suffixes, and base names into a single file. Traits are assigned to components and applied when patterns are resolved.

**Structure:**

```json
{
  "components": {
    "prefix": [
      {
        "value": "Component Name",
        "rarityWeight": 50,
        "traits": {
          "stat1": { "value": 5, "type": "number" },
          "stat2": { "value": "text", "type": "string" }
        }
      }
    ],
    "material": [...],
    "quality": [...],
    "suffix": [...]
  },
  "patterns": [
    {
      "pattern": "base",
      "weight": 100,
      "example": "Longsword"
    },
    {
      "pattern": "prefix + material + base + suffix",
      "weight": 10,
      "example": "Blessed Mithril Longsword of Fire"
    }
  ],
  "metadata": {
    "description": "Unified naming system with traits",
    "version": "4.0",
    "type": "pattern_generation",
    "supportsTraits": true,
    "componentKeys": ["prefix", "material", "quality", "suffix"],
    "patternTokens": ["base", "prefix", "material", "quality", "suffix"]
  }
}
```

**Component Structure:**

Each component in ANY component group can have:
- `value` (string, required) - The actual text (e.g., "Iron", "Blessed", "of Fire")
- `rarityWeight` (number, required) - Rarity contribution (1-100, higher = rarer)
- `traits` (object, optional) - Stat modifiers applied when this component is selected

**Trait Object Format:**

```json
"traits": {
  "durability": { "value": 100, "type": "number" },
  "damageBonus": { "value": 5, "type": "number" },
  "element": { "value": "fire", "type": "string" },
  "isLegendary": { "value": true, "type": "boolean" }
}
```

**Trait Types:**
- `number` - Numeric stats (damage, durability, weight, etc.)
- `string` - Text values (element types, categories, etc.)
- `boolean` - Flags (isLegendary, isCursed, etc.)

**Trait Merging Rules (when pattern has multiple components):**

1. **Numbers**: Take HIGHEST value
   - Example: durability 100 + durability 200 = durability 200
2. **Strings**: LAST WINS (right-to-left in pattern)
   - Example: element "fire" + element "ice" = element "ice"
3. **Booleans**: OR logic (true if ANY component has true)
   - Example: isCursed false + isCursed true = isCursed true
4. **Arrays**: CONCAT unique values
   - Example: tags ["melee"] + tags ["magic"] = tags ["melee", "magic"]

**Example (weapons/names.json v4.0):**

```json
{
  "components": {
    "prefix": [
      {
        "value": "Rusty",
        "rarityWeight": 50,
        "traits": {
          "durability": { "value": 50, "type": "number" },
          "damageMultiplier": { "value": 0.8, "type": "number" }
        }
      },
      {
        "value": "Blessed",
        "rarityWeight": 15,
        "traits": {
          "holyDamage": { "value": 5, "type": "number" },
          "damageVsUndead": { "value": 10, "type": "number" }
        }
      }
    ],
    "material": [
      {
        "value": "Iron",
        "rarityWeight": 10,
        "traits": {
          "weightMultiplier": { "value": 1.0, "type": "number" },
          "durability": { "value": 100, "type": "number" }
        }
      },
      {
        "value": "Mithril",
        "rarityWeight": 50,
        "traits": {
          "weightMultiplier": { "value": 0.5, "type": "number" },
          "durability": { "value": 300, "type": "number" },
          "damageBonus": { "value": 5, "type": "number" }
        }
      }
    ],
    "quality": [
      {
        "value": "Fine",
        "rarityWeight": 15,
        "traits": {
          "durability": { "value": 120, "type": "number" }
        }
      }
    ],
    "suffix": [
      {
        "value": "of Slaying",
        "rarityWeight": 50,
        "traits": {
          "damageBonus": { "value": 5, "type": "number" }
        }
      },
      {
        "value": "of Fire",
        "rarityWeight": 30,
        "traits": {
          "fireDamage": { "value": 8, "type": "number" },
          "element": { "value": "fire", "type": "string" }
        }
      }
    ]
  },
  "patterns": [
    {
      "pattern": "base",
      "weight": 100,
      "example": "Longsword"
    },
    {
      "pattern": "material + base",
      "weight": 50,
      "example": "Iron Longsword"
    },
    {
      "pattern": "prefix + base",
      "weight": 30,
      "example": "Rusty Longsword"
    },
    {
      "pattern": "base + suffix",
      "weight": 30,
      "example": "Longsword of Slaying"
    },
    {
      "pattern": "material + base + suffix",
      "weight": 20,
      "example": "Mithril Longsword of Fire"
    },
    {
      "pattern": "prefix + material + base + suffix",
      "weight": 5,
      "example": "Blessed Mithril Longsword of Fire"
    }
  ],
  "metadata": {
    "description": "Unified weapon naming with traits",
    "version": "4.0",
    "type": "pattern_generation",
    "supportsTraits": true,
    "componentKeys": ["prefix", "material", "quality", "suffix"],
    "patternTokens": ["base", "prefix", "material", "quality", "suffix"],
    "totalPatterns": 6,
    "notes": [
      "Base token resolves from items/weapons/types.json",
      "Traits are merged when pattern is resolved",
      "Emergent rarity from combined component weights"
    ]
  }
}
```

**Generated Name Example:**

Pattern: `"prefix + material + base + suffix"` → `"Blessed Mithril Longsword of Fire"`

**Applied Traits (merged):**

```json
{
  "holyDamage": 5,           // from "Blessed" prefix
  "damageVsUndead": 10,      // from "Blessed" prefix
  "weightMultiplier": 0.5,   // from "Mithril" material
  "durability": 300,         // from "Mithril" material (highest of 50, 100, 300)
  "damageBonus": 5,          // from "Mithril" material
  "fireDamage": 8,           // from "of Fire" suffix
  "element": "fire"          // from "of Fire" suffix
}
```

**Emergent Rarity Calculation:**

- Blessed (weight 15) × Mithril (weight 50) × of Fire (weight 30) = Combined weight score
- Maps to rarity tier via `general/rarity_config.json`
- Result: **Epic** or **Legendary** tier item

**Benefits of Unified System:**

✅ **One file** instead of three (names.json, prefixes.json, suffixes.json)  
✅ **Consistent structure** - All components use same format  
✅ **Trait ownership** - Clear which component provides which stats  
✅ **Pattern flexibility** - Same component can be prefix OR material in different patterns  
✅ **Better ContentBuilder UX** - See all naming parts in one view  
✅ **Emergent complexity** - Traits and rarity emerge from combinations  

**Migration from v3.0 (separate files):**

If you have existing separate prefix/suffix files:

1. Add `prefix` and `suffix` component groups to names.json
2. Convert prefix items: `{name, traits, rarityWeight}` → `{value, rarityWeight, traits}`
3. Convert suffix items: same conversion
4. Add patterns that use prefix/suffix tokens
5. Mark old prefix/suffix files as deprecated
6. Update metadata: `version: "4.0"`, `supportsTraits: true`

---

#### 4. prefixes.json / suffixes.json - Stat Modifiers (LEGACY - Deprecated in v4.0+)

**⚠️ NOTE:** This structure is **deprecated** as of v4.0. New implementations should use the unified naming system (see section 3 above).

**Purpose:** Item modifiers with stat bonuses/penalties (legacy separate file approach)

**Structure:**

```json
{
  "items": [
    {
      "name": "internal_name",
      "displayName": "Display Name",
      "rarityWeight": 50,
      "traits": {
        "stat1": { "value": 5, "type": "number" },
        "stat2": { "value": "text", "type": "string" }
      }
    }
  ],
  "metadata": {
    "description": "Prefix/suffix modifiers",
    "version": "1.0",
    "type": "prefix_modifier"
  }
}
```

**Example (weapons/prefixes.json - LEGACY):**

```json
{
  "items": [
    {
      "name": "flaming",
      "displayName": "Flaming",
      "rarityWeight": 30,
      "traits": {
        "bonusDamage": { "value": 5, "type": "number" },
        "damageType": { "value": "fire", "type": "string" },
        "value": { "value": 50, "type": "number" }
      }
    }
  ],
  "metadata": {
    "description": "Weapon prefix modifiers",
    "version": "1.0",
    "type": "prefix_modifier"
  }
}
```

---

#### 4. Reference Data Files

**Purpose:** Component sources for other files (no pattern generation)

**Structure:**

```json
{
  "components": {
    "category1": ["Value1", "Value2", "Value3"],
    "category2": ["Value1", "Value2"],
    "category3": ["Value1", "Value2"]
  },
  "metadata": {
    "description": "Reference data description",
    "version": "1.0",
    "type": "reference_data"
  }
}
```

**Example (general/adjectives.json):**

```json
{
  "components": {
    "positive": ["Magnificent", "Exquisite", "Pristine"],
    "negative": ["Broken", "Damaged", "Ruined"],
    "size": ["Tiny", "Small", "Large", "Huge"]
  },
  "metadata": {
    "description": "Adjective components for use in other files",
    "version": "1.0",
    "type": "reference_data"
  }
}
```

---

### Metadata Field Specification

**Philosophy:** Metadata should be **auto-generated** by ContentBuilder to avoid manual maintenance errors.

#### User-Defined Fields (Editable in UI)

- `description` - Human-written explanation of file purpose
- `version` - Schema version (e.g., "1.0", "2.0") - user increments on breaking changes

#### Auto-Generated Fields (Computed on Save)

- `lastUpdated` - Timestamp of last save (YYYY-MM-DD format)
- `componentKeys` - Array extracted from `components` object keys
- `patternTokens` - Array extracted from `patterns` + "base" token
- `totalPatterns` - Count of patterns array length
- `total_items` - Count of items array length (if applicable)
- `[category]_count` - Count of category types (e.g., `weapon_types: 7`)

#### Optional Auto-Generated Fields

- `type` - File type hint ("reference_data", "catalog", "generation") - inferred from structure
- Custom statistics as needed

#### ContentBuilder Implementation

**On Save:**

1. Extract component keys from `components` object
2. Parse all patterns and extract unique tokens
3. Add "base" token to patternTokens if patterns exist
4. Count arrays and objects for statistics
5. Set `lastUpdated` to current date
6. Preserve user-defined `description` and `version`
7. Generate complete metadata object
8. Save JSON with auto-generated metadata

**UI Design:**

```
┌─ Metadata ─────────────────────────────────────┐
│ Description: [Weapon name generation...      ] │ ← User editable
│ Version:     [2.0                            ] │ ← User editable
│                                                 │
│ Auto-Generated (read-only):                    │
│   Last Updated: 2025-12-16                     │
│   Component Keys: material, quality, ...       │
│   Pattern Tokens: base, material, quality, ... │
│   Total Patterns: 11                           │
└─────────────────────────────────────────────────┘
```

**Example Auto-Generated Metadata:**

```json
"metadata": {
  "description": "Weapon name generation with pattern-based system",
  "version": "2.0",
  "lastUpdated": "2025-12-16",
  "componentKeys": ["material", "quality", "descriptive", "enchantment", "title"],
  "patternTokens": ["base", "material", "quality", "descriptive", "enchantment", "title"],
  "totalPatterns": 11,
  "total_items": 59,
  "weapon_types": 7
}
```

**Benefits:**

- ✅ No manual maintenance required
- ✅ Always accurate and up-to-date
- ✅ Real-time pattern validation
- ✅ Eliminates user errors
- ✅ Automatic timestamping

---

## File Type Guide: When to Use Each File

### Overview

Game data is organized into **specialized file types**, each serving a specific purpose:

| File Type | Purpose | Contains | Used By | Example |
|-----------|---------|----------|---------|---------|
| **types.json** | Item/enemy catalog with base stats | Items array + base traits | Pattern `base` token, runtime stat lookup | `weapons/types.json` |
| **names.json** | Name generation components | Components + patterns | Pattern executor for name generation | `weapons/names.json` |
| **prefixes.json** | Stat modifiers (applied before base) | Prefix name + stat bonuses/penalties | Runtime item generation | `weapons/prefixes.json` |
| **suffixes.json** | Stat modifiers (applied after base) | Suffix name + stat bonuses/penalties | Runtime item generation | `enchantments/suffixes.json` |

---

### types.json - Item/Enemy Catalog

**Purpose:** Define base items/enemies and their inherent properties

**Structure:**

```json
{
  "weapon_types": {
    "swords": {
      "items": [
        {
          "name": "Shortsword",
          "damage": "1d6",
          "weight": 2.0,
          "value": 10,
          "rarity": "common"
        },
        {
          "name": "Longsword",
          "damage": "1d8",
          "weight": 3.0,
          "value": 15,
          "rarity": "common"
        },
        {
          "name": "Greatsword",
          "damage": "2d6",
          "weight": 6.0,
          "value": 50,
          "rarity": "uncommon",
          "twoHanded": true
        }
      ],
      "traits": {
        "damageType": "slashing",
        "slot": "mainhand",
        "category": "sword"
      }
    },
    "axes": {
      "items": [
        {
          "name": "Handaxe",
          "damage": "1d6",
          "weight": 2.0,
          "value": 10,
          "rarity": "common"
        },
        {
          "name": "Battleaxe",
          "damage": "1d8",
          "weight": 4.0,
          "value": 20,
          "rarity": "common",
          "twoHanded": true
        },
        {
          "name": "Greataxe",
          "damage": "1d12",
          "weight": 7.0,
          "value": 60,
          "rarity": "uncommon",
          "twoHanded": true
        }
      ],
      "traits": {
        "damageType": "slashing",
        "slot": "mainhand",
        "category": "axe"
      }
    }
  }
}
```

**When to Use:**

- ✅ Defining base item/enemy lists with individual stats (weapons, armor, creatures)
- ✅ Setting shared traits for entire categories (damageType, slot, category)
- ✅ Organizing items into logical groups
- ✅ Specifying unique stats per item (damage, weight, value, rarity)

**Pattern Integration:**

- `base` token resolves by picking from `types.json → [category] → items[]`
- Each item is an object with `name` property and individual stats
- Runtime picks random item object, uses `name` for pattern, inherits all stats
- Type-level traits apply to ALL items in that category

**Examples:**

- `items/weapons/types.json` - All weapon types (swords, axes, bows, etc.)
- `items/armor/types.json` - All armor types (helmets, chest, legs, etc.)
- `enemies/beasts/types.json` - All beast types (wolves, bears, cats, etc.)

---

### names.json - Name Generation Components

**Purpose:** Define components and patterns for procedurally generating names

**Structure:**

```json
{
  "components": {
    "material": ["Iron", "Steel", "Mithril", "Adamantine"],
    "quality": ["Fine", "Superior", "Masterwork", "Legendary"],
    "descriptive": ["Ancient", "Enchanted", "Cursed", "Blessed"],
    "enchantment": ["of Slaying", "of Power", "of Speed"],
    "title": ["of the Dragon", "of the Hero", "of Legends"]
  },
  "patterns": [
    "base",
    "material + base",
    "quality + material + base",
    "descriptive + base + title",
    "quality + material + base + enchantment"
  ],
  "metadata": {
    "description": "Weapon name generation",
    "componentKeys": ["material", "quality", "descriptive", "enchantment", "title"],
    "patternTokens": ["base", "material", "quality", "descriptive", "enchantment", "title"]
  }
}
```

**When to Use:**

- ✅ Creating procedural name generation systems
- ✅ Defining descriptive components (materials, qualities, adjectives)
- ✅ Specifying name patterns from simple to legendary

**Pattern Integration:**

- Components are the **building blocks** (material, quality, etc.)
- Patterns are the **templates** ("material + base", "quality + material + base")
- Runtime picks random component values and assembles them via pattern

**Key Rules:**

- Component keys must match pattern tokens EXACTLY
- Use simple, semantic key names (material, not prefix_material)
- Patterns progress from simple → complex (base → legendary)
- `base` token pulls from types.json

**Examples:**

- `items/weapons/names.json` - Weapon name generation
- `enemies/beasts/names.json` - Beast name generation
- `npcs/titles/names.json` - NPC title generation (if using patterns)

---

### prefixes.json - Stat Modifiers (Before Base Name)

**Purpose:** Define prefixes that add stats/traits and appear BEFORE base name

**Structure:**

```json
{
  "items": [
    {
      "name": "flaming",
      "displayName": "Flaming",
      "traits": {
        "bonusDamage": 5,
        "damageType": "fire",
        "value": 50
      }
    },
    {
      "name": "keen",
      "displayName": "Keen",
      "traits": {
        "criticalChance": 10,
        "value": 30
      }
    }
  ]
}
```

**When to Use:**

- ✅ Modifiers that enhance base item stats
- ✅ Properties applied BEFORE base name ("Flaming Longsword")
- ✅ Temporary or permanent enchantments

**Pattern Integration:**

- **NOT used in pattern execution** (patterns use names.json components)
- Runtime picks prefix randomly based on item rarity
- Applied **before** base name: "Flaming" + "Longsword" = "Flaming Longsword"

**Runtime Usage:**

```csharp
var prefix = prefixData.PickRandom();
var baseWeapon = typesData.PickRandom();
var finalName = $"{prefix.displayName} {baseWeapon}";
var finalStats = baseWeapon.traits + prefix.traits; // Additive
```

**Examples:**

- `items/weapons/prefixes.json` - Weapon prefixes (Flaming, Keen, Brutal)
- `items/armor/prefixes.json` - Armor prefixes (Reinforced, Hardened, Light)
- `enemies/beasts/prefixes.json` - Enemy prefixes (Dire, Alpha, Rabid)

---

### suffixes.json - Stat Modifiers (After Base Name)

**Purpose:** Define suffixes that add stats/traits and appear AFTER base name

**Structure:**

```json
{
  "items": [
    {
      "name": "of_slaying",
      "displayName": "of Slaying",
      "traits": {
        "bonusDamage": 10,
        "damageVs": "undead",
        "value": 100
      }
    },
    {
      "name": "of_the_dragon",
      "displayName": "of the Dragon",
      "traits": {
        "bonusDamage": 15,
        "damageType": "fire",
        "legendary": true,
        "value": 500
      }
    }
  ]
}
```

**When to Use:**

- ✅ Modifiers that enhance base item stats
- ✅ Properties applied AFTER base name ("Longsword of Slaying")
- ✅ Named/legendary item modifiers

**Pattern Integration:**

- **NOT used in pattern execution** (patterns use names.json components)
- Runtime picks suffix randomly based on item rarity
- Applied **after** base name: "Longsword" + "of Slaying" = "Longsword of Slaying"

**Runtime Usage:**

```csharp
var baseWeapon = typesData.PickRandom();
var suffix = suffixData.PickRandom();
var finalName = $"{baseWeapon} {suffix.displayName}";
var finalStats = baseWeapon.traits + suffix.traits; // Additive
```

**Examples:**

- `items/enchantments/suffixes.json` - Enchantment suffixes (of Slaying, of Power)
- `items/weapons/suffixes.json` - Weapon-specific suffixes (of the Warrior)
- `npcs/titles/suffixes.json` - NPC title suffixes (the Great, the Wise)

---

### File Relationships & Data Flow

**Visual Data Flow Diagram:**

```
┌─────────────────────────────────────────────────────────────┐
│                    Pattern-Based Generation                  │
└─────────────────────────────────────────────────────────────┘

Step 1: Load Base Items
┌──────────────────┐
│  types.json      │  Pick random type (e.g., "swords")
│  ┌──────────┐    │    ↓
│  │ swords   │───────→ Pick random item object
│  │ axes     │    │    ↓
│  │ bows     │    │  { name: "Longsword", damage: "1d8", ... }
│  └──────────┘    │
└──────────────────┘
         │
         ↓
Step 2: Select Pattern
┌──────────────────┐
│  names.json      │  Pick random pattern
│  ┌──────────┐    │    ↓
│  │ patterns │───────→ "quality + material + base + enchantment"
│  └──────────┘    │
└──────────────────┘
         │
         ↓
Step 3: Resolve Pattern Tokens
┌──────────────────┐
│  names.json      │  For each token in pattern:
│  ┌──────────┐    │  • "quality"     → Pick from components.quality
│  │components│───────→ "material"    → Pick from components.material
│  │  quality │    │  • "base"        → Use item.name from Step 1
│  │  material│    │  • "enchantment" → Pick from components.enchantment
│  │  ...     │    │
│  └──────────┘    │
└──────────────────┘
         │
         ↓
Step 4: Assemble Final Name
    "Fine Steel Longsword of Slaying"
         │
         ↓
Step 5: Inherit Stats & Traits
┌─────────────────────────────────────────────────┐
│ Final Item:                                     │
│   name: "Fine Steel Longsword of Slaying"      │
│   damage: "1d8"          (from types.json)      │
│   weight: 3.0            (from types.json)      │
│   value: 15              (from types.json)      │
│   damageType: "slashing" (from types.json)      │
│   slot: "mainhand"       (from types.json)      │
└─────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                   Stat-Based Modifiers                       │
└─────────────────────────────────────────────────────────────┘

Step 1: Base Item (from types.json)
    Longsword: damage: "1d8", value: 15
         │
         ↓
Step 2: Roll Rarity → Rare
         │
         ↓
Step 3: Apply Prefix (optional)
┌──────────────────┐
│ prefixes.json    │  Pick "Flaming"
│  ┌──────────┐    │    ↓
│  │ Flaming  │───────→ bonusDamage: +5, damageType: "fire", value: +50
│  │ Keen     │    │
│  └──────────┘    │
└──────────────────┘
         │
         ↓
Step 4: Apply Suffix (optional)
┌──────────────────┐
│ suffixes.json    │  Pick "of Slaying"
│  ┌──────────┐    │    ↓
│  │of Slaying│───────→ bonusDamage: +10 (vs undead), value: +100
│  │of Power  │    │
│  └──────────┘    │
└──────────────────┘
         │
         ↓
Step 5: Calculate Final Stats
┌─────────────────────────────────────────────────┐
│ Final Item:                                     │
│   name: "Flaming Longsword of Slaying"         │
│   damage: 1d8+5 (fire)   (base + prefix)       │
│   bonusVsUndead: +10     (suffix)               │
│   value: 165             (15 + 50 + 100)        │
│   slot: "mainhand"       (base)                 │
└─────────────────────────────────────────────────┘
```

**Pattern-Based Name Generation Flow:**

```text
1. Pick item type from types.json
   ↓
2. Pick random item object from that type's items array
   ↓
3. Pick pattern from names.json
   ↓
4. For each token in pattern:
   - If token is "base" → use item.name from step 2
   - Else → pick random from names.json components[token]
   ↓
5. Assemble final name
   ↓
6. Inherit item stats (damage, weight, value) + type traits (damageType, slot)
```

**Example:**

```
types.json → Pick "swords" → Pick item object: { name: "Longsword", damage: "1d8", weight: 3.0, value: 15 }
names.json pattern → "quality + material + base + enchantment"
names.json components → quality: "Fine", material: "Steel", enchantment: "of Slaying"
Final name → "Fine Steel Longsword of Slaying"
Final stats → damage: "1d8", weight: 3.0, value: 15, damageType: "slashing", slot: "mainhand"
```

**Stat-Based Item Generation Flow:**

```
1. Pick item type from types.json
   ↓
2. Pick random item object from that type's items array (get base stats)
   ↓
3. Roll for rarity (common, rare, legendary, etc.)
   ↓
4. Based on rarity, pick prefix from prefixes.json (if applicable)
   ↓
5. Based on rarity, pick suffix from suffixes.json (if applicable)
   ↓
6. Calculate final stats: item stats + type traits + prefix modifiers + suffix modifiers
   ↓
7. Assemble final name: [prefix] + item.name + [suffix]
```

**Example:**

```
types.json → Pick "swords" → Pick item: { name: "Longsword", damage: "1d8", weight: 3.0, value: 15 }
Type traits → damageType: "slashing", slot: "mainhand"
Rarity roll → Rare
prefixes.json → "Flaming" (bonusDamage: +5, damageType: "fire", value: +50)
suffixes.json → "of Slaying" (bonusDamage: +10 vs undead, value: +100)
Final stats → damage: 1d8+5 (fire), bonusVsUndead: +10, weight: 3.0, value: 165, slot: "mainhand"
Final name → "Flaming Longsword of Slaying"
```

   ↓
4. Based on rarity, pick suffix from suffixes.json (if applicable)
   ↓
5. Calculate final stats: base + prefix + suffix
   ↓
6. Assemble final name: [prefix] + base + [suffix]

```

**Example:**
```

types.json → Longsword (damage: 1d8, value: 15)
Rarity roll → Rare
prefixes.json → "Flaming" (bonusDamage: +5, value: +50)
suffixes.json → "of Slaying" (bonusDamage: +10 vs undead, value: +100)
Final stats → damage: 1d8+5, bonusVsUndead: +10, value: 165
Final name → "Flaming Longsword of Slaying"

```

**Key Insight:** Pattern execution (names.json) and stat modifiers (prefixes/suffixes.json) are **separate systems**:
- **names.json** → Procedural name generation via templates
- **prefixes/suffixes.json** → Stat bonuses with display names

You can use BOTH:
- Generate name via pattern: "Fine Steel Longsword of Power"
- OR apply prefix+suffix: "Flaming Longsword of Slaying"
- OR mix: Generate base via pattern, then add random prefix/suffix

---

### Decision Matrix: Which File Should I Use?

**Question: I want to...**

| Task | File Type | Reason |
|------|-----------|--------|
| Define a new weapon type (Rapier, Katana) | `types.json` | Base item catalog |
| Set default damage/stats for all swords | `types.json` | Base traits by type |
| Create a new adjective for names (Shimmering, Ethereal) | `names.json` | Component for patterns |
| Add a new pattern (material + size + base) | `names.json` | Pattern template |
| Create a prefix that adds fire damage (Flaming) | `prefixes.json` | Stat modifier before name |
| Create a suffix for legendary items (of the Titans) | `suffixes.json` | Stat modifier after name |
| Define beast subtypes (wolves, bears, cats) | `types.json` | Category organization |
| Add dragon colors (Red, Blue, Black) | `types.json` (items) OR `names.json` (component) | Depends on use case |

**Use types.json when:**
- ✅ Defining the actual items/enemies that exist
- ✅ Setting base stats/traits for categories
- ✅ Organizing content into logical groups

**Use names.json when:**
- ✅ Creating procedural name generation
- ✅ Defining descriptive components (adjectives, materials, etc.)
- ✅ Specifying name patterns

**Use prefixes.json when:**
- ✅ Adding stat modifiers that appear BEFORE the base name
- ✅ Enhancing items with bonuses (damage, defense, etc.)

**Use suffixes.json when:**
- ✅ Adding stat modifiers that appear AFTER the base name
- ✅ Creating named/legendary item variants

---

## Category-by-Category Standards

### 1. General Category ✅ COMPLETE (9/9 files)

**Standardization Date:** December 16, 2025  
**File Types:** Component Library (3), Pattern Generation (6)

---

#### Component Library Files (3)

These files provide categorized reference data used by other files. They do NOT generate procedural content.

##### Adjectives (`general/adjectives.json`) ✅

**Structure:** Component Library  
**Status:** ✅ STANDARDIZED

**Component Keys:**
- `positive` - Positive adjectives (Magnificent, Exquisite, Pristine)
- `negative` - Negative adjectives (Broken, Damaged, Ruined)
- `size` - Size descriptors (Tiny, Small, Large, Huge)
- `appearance` - Appearance adjectives (Shining, Glowing, Sparkling)
- `condition` - Condition states (New, Old, Ancient, Pristine)

**Current Structure:**

```json
{
  "components": {
    "positive": ["Magnificent", "Exquisite", "Pristine", ...],
    "negative": ["Broken", "Damaged", "Ruined", ...],
    "size": ["Tiny", "Small", "Large", "Huge", ...],
    "appearance": ["Shining", "Glowing", "Sparkling", ...],
    "condition": ["New", "Old", "Ancient", "Pristine", ...]
  },
  "metadata": {
    "description": "Adjective components for descriptive text generation",
    "version": "1.0",
    "type": "component_library",
    "lastUpdated": "2025-12-16",
    "componentKeys": ["positive", "negative", "size", "appearance", "condition"],
    "component_counts": {
      "positive": 10,
      "negative": 10,
      "size": 8,
      "appearance": 10,
      "condition": 8
    }
  }
}
```

**Usage:** Referenced by items, enemies, NPCs for descriptive text

---

##### Materials (`general/materials.json`) ✅

**Structure:** Component Library  
**Status:** ✅ STANDARDIZED

**Component Keys:**

- `metals` - Metallic materials (Iron, Steel, Gold, Silver)
- `precious` - Precious materials (Diamond, Ruby, Emerald, Sapphire)
- `natural` - Natural materials (Wood, Stone, Leather, Bone)
- `magical` - Magical materials (Ethereal, Astral, Void, Shadow)

**Current Structure:**

```json
{
  "components": {
    "metals": ["Iron", "Steel", "Bronze", "Copper", ...],
    "precious": ["Diamond", "Ruby", "Sapphire", "Emerald", ...],
    "natural": ["Wood", "Stone", "Bone", "Leather", ...],
    "magical": ["Ethereal", "Astral", "Void", "Shadow", ...]
  },
  "metadata": {
    "description": "Material components for item crafting and descriptions",
    "version": "1.0",
    "type": "component_library",
    "lastUpdated": "2025-12-16",
    "componentKeys": ["metals", "precious", "natural", "magical"],
    "component_counts": {
      "metals": 10,
      "precious": 10,
      "natural": 9,
      "magical": 10
    }
  }
}
```

**Usage:** Referenced by items for material-based generation

---

##### Verbs (`general/verbs.json`) ✅

**Structure:** Component Library (converted from broken Pattern Generation)  
**Status:** ✅ STANDARDIZED

**Component Keys:**

- `combat_offensive` - Offensive combat actions (attacks, strikes, slashes)
- `combat_defensive` - Defensive combat actions (blocks, parries, dodges)
- `magic` - Magical actions (casts, conjures, summons)
- `healing` - Healing actions (heals, mends, restores)
- `movement` - Movement actions (moves, walks, runs)
- `stealth` - Stealth actions (sneaks, creeps, stalks)
- `interaction` - Interaction actions (opens, closes, examines)
- `communication` - Communication actions (speaks, shouts, whispers)

**Current Structure:**

```json
{
  "components": {
    "combat_offensive": ["attacks", "strikes", "slashes", ...],
    "combat_defensive": ["blocks", "parries", "dodges", ...],
    "magic": ["casts", "conjures", "summons", ...],
    "healing": ["heals", "mends", "restores", ...],
    "movement": ["moves", "walks", "runs", ...],
    "stealth": ["sneaks", "creeps", "stalks", ...],
    "interaction": ["opens", "closes", "examines", ...],
    "communication": ["speaks", "shouts", "whispers", ...]
  },
  "metadata": {
    "description": "Categorized action verbs for combat, magic, and interactions",
    "version": "1.0",
    "type": "component_library",
    "lastUpdated": "2025-12-16",
    "componentKeys": [...],
    "component_counts": {...}
  }
}
```

**Usage:** Referenced by combat, NPCs, and narrative systems

**Note:** Previously had broken patterns referencing non-existent components (`verb`, `adverb`, `preposition`). Converted to Component Library type.

---

#### Pattern Generation Files (6)

These files generate procedural descriptions using pattern-based templates. They can also serve as component sources for other files.

##### Colors (`general/colors.json`) ✅

**Structure:** Pattern Generation  
**Status:** ✅ STANDARDIZED

**Component Keys:**

- `base_color` - Primary colors (red, blue, green, yellow)
- `modifier` - Color modifiers (dark, light, bright, pale)
- `material` - Material-based colors (crimson, scarlet, azure, emerald)

**Patterns:**

- `base_color` → "red"
- `modifier + base_color` → "dark red"
- `material` → "crimson"

**Current Structure:**

```json
{
  "components": {
    "base_color": ["red", "blue", "green", "yellow", ...],
    "modifier": ["dark", "light", "bright", "pale", ...],
    "material": ["crimson", "scarlet", "azure", "emerald", ...]
  },
  "patterns": [
    "base_color",
    "modifier + base_color",
    "material"
  ],
  "metadata": {
    "description": "Color name generation with base colors, modifiers, and materials",
    "version": "1.0",
    "type": "pattern_generation",
    "lastUpdated": "2025-12-16",
    "componentKeys": ["base_color", "modifier", "material"],
    "pattern_count": 3,
    "patternTokens": ["base_color", "modifier", "material"],
    "component_counts": {...}
  }
}
```

**Usage:** Generate dynamic color descriptions for items, enemies, environments

**Fixed Issues:**

- ✅ Removed pattern comment: `"material (gemstone/metal colors)"` → `"material"`
- ✅ Fixed component keys: `base_colors` → `base_color` (singular)
- ✅ Removed duplicate `items` array

---

##### Smells (`general/smells.json`) ✅

**Structure:** Pattern Generation  
**Status:** ✅ STANDARDIZED

**Component Keys:**

- `pleasant` - Pleasant smells (fragrant, fresh, floral)
- `unpleasant` - Unpleasant smells (musty, acrid, pungent)
- `natural` - Natural smells (earthy, woody, mossy)
- `intensity` - Intensity modifiers (faint, mild, strong)

**Patterns:**

- `pleasant` → "fragrant"
- `unpleasant` → "musty"
- `natural` → "earthy"
- `intensity + pleasant` → "faint fragrant"
- `intensity + unpleasant` → "strong acrid"

**Usage:** Generate environmental and atmospheric descriptions

**Fixed Issues:**

- ✅ Removed broken pattern: `"smell + smell (combination)"`
- ✅ Fixed token references to actual component keys
- ✅ Removed `items` array

---

##### Sounds (`general/sounds.json`) ✅

**Structure:** Pattern Generation  
**Status:** ✅ STANDARDIZED

**Component Keys:**

- `base_sound` - Core sounds (echoing, whisper, roar, clang)
- `volume` - Volume levels (silent, quiet, loud, thunderous)
- `nature` - Sound nature (metallic, wooden, liquid, magical)
- `combat` - Combat sounds (clang, crash, clash, thud)
- `ambient` - Ambient sounds (rustle, whisper, murmur, chirp)
- `intensity` - Intensity descriptors (gentle, harsh, sharp)

**Patterns:**

- `base_sound` → "whisper"
- `volume + base_sound` → "loud roar"
- `nature + base_sound` → "metallic clang"
- `combat` → "crash"
- `ambient` → "rustle"

**Usage:** Generate combat and environmental sound descriptions

**Fixed Issues:**

- ✅ Added `base_sound` component (was missing)
- ✅ Fixed token mismatch: `sound` → `base_sound`
- ✅ Removed `items` array

---

##### Textures (`general/textures.json`) ✅

**Structure:** Pattern Generation  
**Status:** ✅ STANDARDIZED

**Component Keys:**

- `surface_quality` - Surface feel (rough, smooth, polished)
- `temperature` - Temperature feel (cold, warm, hot)
- `moisture` - Moisture level (dry, damp, wet, slimy)
- `hardness` - Hardness level (soft, firm, hard, brittle)
- `organic` - Organic textures (leathery, scaly, furry)

**Patterns:**

- `surface_quality` → "smooth"
- `surface_quality + moisture` → "smooth damp"
- `temperature + surface_quality` → "cold smooth"
- `organic` → "leathery"

**Usage:** Generate tactile item and environmental descriptions

**Fixed Issues:**

- ✅ Fixed token: `texture` → `surface_quality`
- ✅ Removed `items` array

---

##### Time of Day (`general/time_of_day.json`) ✅

**Structure:** Pattern Generation  
**Status:** ✅ STANDARDIZED

**Component Keys:**

- `period` - Time periods (dawn, morning, midday, evening)
- `modifier` - Time modifiers (early, late, deep, high)
- `descriptor` - Descriptive phrases (first light, golden hour)

**Patterns:**

- `period` → "dawn"
- `modifier + period` → "early morning"
- `descriptor` → "first light"

**Usage:** Generate time-based narrative and environmental descriptions

**Fixed Issues:**

- ✅ Fixed component keys: `periods`, `modifiers`, `descriptors` → singular
- ✅ Removed `items` array

---

##### Weather (`general/weather.json`) ✅

**Structure:** Pattern Generation  
**Status:** ✅ STANDARDIZED

**Component Keys:**

- `precipitation` - Precipitation types (clear, rainy, snowy)
- `wind` - Wind levels (calm, breezy, gusty)
- `sky_condition` - Sky states (clear, cloudy, overcast)
- `temperature` - Temperature levels (freezing, cold, warm)
- `severity` - Weather severity (mild, severe, extreme)
- `special` - Special weather (stormy, thunderous, blizzard)

**Patterns:**

- `precipitation` → "rainy"
- `sky_condition` → "cloudy"
- `temperature + precipitation` → "cold rainy"
- `severity + precipitation` → "severe snowy"
- `special` → "stormy"

**Usage:** Generate dynamic weather and environmental conditions

**Fixed Issues:**

- ✅ Fixed token: `condition` → `precipitation` / `sky_condition`
- ✅ Removed `items` array

---

#### General Category Summary

**Files Standardized:** 9/9 ✅  
**Component Library:** 3 files (adjectives, materials, verbs)  
**Pattern Generation:** 6 files (colors, smells, sounds, textures, time_of_day, weather)  
**Total Components:** 41 component keys  
**Total Patterns:** 29 patterns  
**Metadata Fields:** All files have auto-generated metadata

**Key Changes Applied:**

- ✅ Removed all `items` arrays from Pattern Generation files (7 files)
- ✅ Fixed all broken patterns (removed comments, fixed token mismatches)
- ✅ Standardized component keys (singular, match pattern tokens)
- ✅ Added metadata to all files
- ✅ Converted verbs.json from broken Pattern Generation to Component Library

**Documentation:**

- See `docs/standards/GENERAL_FILES_AUDIT.md` for detailed analysis
- See `docs/standards/GENERAL_FILES_COMPLETE.md` for completion summary
- See `docs/standards/GENERAL_FILES_IMPLEMENTATION_PLAN.md` for implementation details

---

### 2. Items Category

#### Weapons - Names (`items/weapons/names.json`)

**Current Structure:** HybridArray  
**Action Required:** ✅ COMPLETE - Already standardized

**Component Keys:**

- `material` - Weapon materials (Iron, Steel, Mithril)
- `quality` - Craftsmanship levels (Fine, Superior, Masterwork)
- `descriptive` - Special attributes (Ancient, Enchanted, Cursed)
- `enchantment` - Magical properties (of Slaying, of Power)
- `title` - Named weapons (of the Dragon, of the Hero)
- `weapon_types` - Category organization (swords, axes, bows)

**Standard Patterns:**

```json
[
  "base",
  "material + base",
  "quality + base",
  "descriptive + base",
  "quality + material + base",
  "descriptive + material + base",
  "base + enchantment",
  "base + title",
  "material + base + enchantment",
  "quality + material + base + enchantment",
  "descriptive + base + title"
]
```

**Status:** ✅ Standardized on 2025-12-16

---

#### Weapons - Prefixes/Suffixes (`items/weapons/prefixes.json`, `suffixes.json`) ✅ FINALIZED

**Current Structure:** Rarity-organized stat modifiers  
**File Type:** Stat Modifiers (not pattern-based)

**Actual Structure:**

```json
{
  "common": {
    "Rusty": {
      "displayName": "Rusty",
      "traits": {
        "damageMultiplier": { "value": 0.8, "type": "number" },
        "durability": { "value": 50, "type": "number" }
      }
    }
  },
  "uncommon": {
    "Iron": {
      "displayName": "Iron",
      "traits": {
        "damageBonus": { "value": 2, "type": "number" },
        "durability": { "value": 100, "type": "number" }
      }
    }
  },
  "rare": {
    "Flame-Blessed": {
      "displayName": "Flame-Blessed",
      "traits": {
        "damageBonus": { "value": 10, "type": "number" },
        "fireDamage": { "value": 5, "type": "number" }
      }
    }
  }
}
```

**Key Features:**

- **Rarity-based organization** - Prefixes organized by rarity tier
- **Stat modifiers only** - No pattern generation, pure stat bonuses
- **Trait system** - All values wrapped in `{ value, type }` objects
- **No metadata needed** - Structure is self-explanatory

**Standard:**

- ✅ Keep current structure (rarity-organized)
- ✅ No patterns (these are stat modifiers, not generators)
- ✅ No metadata needed (simple lookup structure)
- ✅ Runtime picks from rarity tier matching item rarity

**Status:** ✅ Finalized - structure matches existing files, no changes needed

---

#### Armor - Materials (`items/armor/materials.json`) ✅ FINALIZED

**Current Structure:** Rarity-organized material modifiers  
**File Type:** Material Modifiers (similar to prefixes, but for armor materials)

**Actual Structure:**

```json
{
  "common": {
    "Cloth": {
      "displayName": "Cloth",
      "traits": {
        "armorRating": { "value": 1, "type": "number" },
        "weight": { "value": 2, "type": "number" },
        "durability": { "value": 40, "type": "number" }
      }
    },
    "Leather": {
      "displayName": "Leather",
      "traits": {
        "armorRating": { "value": 3, "type": "number" },
        "weight": { "value": 5, "type": "number" },
        "dexterityBonus": { "value": 1, "type": "number" }
      }
    }
  },
  "rare": {
    "Steel": {
      "displayName": "Steel",
      "traits": {
        "armorRating": { "value": 12, "type": "number" },
        "defenseBonus": { "value": 5, "type": "number" }
      }
    }
  }
}
```

**Key Features:**

- **Stat modifiers only** - No pattern generation, pure stat bonuses
- **Applied before base name** - "Flaming" + "Longsword" = "Flaming Longsword"
- **Rarity-based selection** - Runtime picks prefix based on item rarity
- **Additive bonuses** - bonusDamage: +5 adds to base weapon damage

**Usage:**

```csharp
var prefix = prefixData.PickRandom();
var baseWeapon = typesData.PickRandom();
var finalName = $"{prefix.displayName} {baseWeapon.name}";
var finalStats = baseWeapon.stats + prefix.traits; // Merge stats
```

**Status:** 📋 Draft proposal - confirm this matches actual file structure

---

#### Armor - Names (`items/armor/names.json`) 📋 DRAFT

**Current Structure:** Unknown  
**Action Required:** Create pattern generation file for armor names

**Proposed Structure:** names.json - Pattern Generation

**Component Keys:**

- `material` - Armor materials (Cloth, Leather, Iron, Steel, Mithril, Dragonscale)
- `quality` - Craftsmanship levels (Crude, Standard, Fine, Superior, Masterwork, Legendary)
- `descriptive` - Special attributes (Reinforced, Blessed, Cursed, Ancient, Enchanted)
- `enchantment` - Magical properties (of Protection, of Resistance, of the Guardian)
- `title` - Named armor (of the Paladin, of the Dragon Knight, of Legends)
- `armor_types` - Category organization (helmets, chest, legs, boots, gloves, shields)

**Proposed Patterns:**

```json
{
  "components": {
    "material": ["Cloth", "Leather", "Iron", "Steel", "Mithril", "Dragonscale"],
    "quality": ["Crude", "Standard", "Fine", "Superior", "Masterwork", "Legendary"],
    "descriptive": ["Reinforced", "Blessed", "Cursed", "Ancient", "Enchanted"],
    "enchantment": ["of Protection", "of Resistance", "of the Guardian", "of Warding"],
    "title": ["of the Paladin", "of the Dragon Knight", "of Legends"],
    "armor_types": {
      "helmets": ["Helm", "Cap", "Crown", "Circlet"],
      "chest": ["Tunic", "Vest", "Breastplate", "Cuirass"],
      "legs": ["Pants", "Greaves", "Leggings"],
      "boots": ["Boots", "Shoes", "Sabatons"],
      "gloves": ["Gloves", "Gauntlets", "Bracers"],
      "shields": ["Buckler", "Shield", "Tower Shield", "Kite Shield"]
    }
  },
  "patterns": [
    "base",
    "material + base",
    "quality + base",
    "quality + material + base",
    "descriptive + base",
    "descriptive + material + base",
    "base + enchantment",
    "material + base + enchantment",
    "quality + material + base + enchantment",
    "descriptive + material + base + title"
  ],
  "metadata": {
    "description": "Armor name generation with pattern-based system",
    "version": "1.0"
  }
}
```

**Example Outputs:**

- Common: "Leather Tunic", "Iron Helm"
- Uncommon: "Fine Steel Breastplate"
- Rare: "Reinforced Mithril Breastplate of Protection"
- Legendary: "Masterwork Ancient Dragonscale Cuirass of the Dragon Knight"

**Status:** 📋 Draft proposal - create this file during Items category standardization

---

#### Armor - Types (`items/armor/types.json`) 📋 DRAFT

**Current Structure:** Unknown  
**Action Required:** Create item catalog for armor types

**Proposed Structure:** types.json - Item Catalog

```json
{
  "armor_types": {
    "helmets": {
      "items": [
        { "name": "Cap", "armor": 1, "weight": 0.5, "value": 5, "rarity": "common" },
        { "name": "Helm", "armor": 3, "weight": 2.0, "value": 25, "rarity": "common" },
        { "name": "Crown", "armor": 2, "weight": 1.0, "value": 100, "rarity": "rare" }
      ],
      "traits": {
        "slot": "head",
        "armorType": "helmet"
      }
    },
    "chest": {
      "items": [
        { "name": "Tunic", "armor": 2, "weight": 1.0, "value": 10, "rarity": "common" },
        { "name": "Breastplate", "armor": 6, "weight": 15.0, "value": 100, "rarity": "uncommon" },
        { "name": "Cuirass", "armor": 8, "weight": 20.0, "value": 200, "rarity": "rare" }
      ],
      "traits": {
        "slot": "chest",
        "armorType": "body"
      }
    }
  }
}
```

**Status:** 📋 Draft proposal

---

#### Enchantments - Suffixes (`items/enchantments/suffixes.json`) 📋 DRAFT

**Current Structure:** Unknown  
**Action Required:** Standardize as stat modifiers

**Proposed Structure:** suffixes.json - Stat Modifiers (After Base Name)

```json
{
  "items": [
    {
      "name": "of_slaying",
      "displayName": "of Slaying",
      "traits": {
        "bonusDamage": 10,
        "damageVs": "all",
        "value": 100,
        "rarity": "rare"
      }
    },
    {
      "name": "of_the_dragon",
      "displayName": "of the Dragon",
      "traits": {
        "bonusDamage": 15,
        "damageType": "fire",
        "legendary": true,
        "value": 500,
        "rarity": "legendary"
      }
    },
    {
      "name": "of_protection",
      "displayName": "of Protection",
      "traits": {
        "bonusArmor": 5,
        "value": 75,
        "rarity": "uncommon"
      }
    },
    {
      "name": "of_speed",
      "displayName": "of Speed",
      "traits": {
        "attackSpeed": 15,
        "movementSpeed": 10,
        "value": 80,
        "rarity": "uncommon"
      }
    }
  ],
  "metadata": {
    "description": "Enchantment suffix modifiers for items",
    "version": "1.0"
  }
}
```

**Key Features:**

- **Stat modifiers only** - Applied after base name
- **Magical properties** - bonusDamage, special effects
- **Rarity-based** - Higher rarity = stronger bonuses

**Usage:**

```csharp
var suffix = suffixData.PickRandom();
var baseWeapon = typesData.PickRandom();
var finalName = $"{baseWeapon.name} {suffix.displayName}";
var finalStats = baseWeapon.stats + suffix.traits; // Merge stats
```

**Status:** 📋 Draft proposal

---

#### Materials (`items/materials/*.json`) 📋 DRAFT

Files: `metals.json`, `leathers.json`, `woods.json`, `gemstones.json`

**Current Structure:** Unknown  
**Action Required:** Likely Component Library (reference data)

**Expected:** These should be Component Library files (no patterns), providing categorized material lists

**Proposed Consolidation:** Consider merging into `general/materials.json` as:

```json
{
  "components": {
    "metals": ["Iron", "Steel", "Bronze", "Copper", "Mithril", "Adamantine"],
    "precious": ["Diamond", "Ruby", "Sapphire", "Emerald", "Amethyst"],
    "natural": ["Wood", "Stone", "Bone", "Leather", "Hide"],
    "magical": ["Ethereal", "Astral", "Void", "Shadow", "Celestial"]
  }
}
```

**Status:** 📋 Needs review - check if separate files needed or can consolidate

---

### 3. Enemies Category 📋 DRAFT

#### Beasts - Names (`enemies/beasts/names.json`) 📋 DRAFT

**Current Structure:** Unknown  
**Action Required:** Create pattern generation file

**Proposed Component Keys:**

- `size` - Size descriptors (Giant, Dire, Alpha, Elder, Young)
- `color` - Color variants (Black, White, Red, Gray, Brown, Golden)
- `descriptive` - Special attributes (Ancient, Rabid, Enraged, Feral, Savage, Wild)
- `origin` - Regional origin (Mountain, Forest, Desert, Arctic, Swamp)
- `title` - Named beasts (of the Night, of the Wild, of the Hunt)
- `beast_types` - Category organization (wolves, bears, cats, boars, etc.)

**Proposed Patterns:**

```json
{
  "components": {
    "size": ["Giant", "Dire", "Alpha", "Elder", "Young", "Massive"],
    "color": ["Black", "White", "Red", "Gray", "Brown", "Golden", "Silver"],
    "descriptive": ["Ancient", "Rabid", "Enraged", "Feral", "Savage", "Wild", "Cursed"],
    "origin": ["Mountain", "Forest", "Desert", "Arctic", "Swamp", "Plains"],
    "title": ["of the Night", "of the Wild", "of the Hunt", "of the Moon"],
    "beast_types": {
      "wolves": ["Wolf", "Hound", "Jackal"],
      "bears": ["Bear", "Grizzly", "Cave Bear"],
      "cats": ["Panther", "Lion", "Tiger", "Leopard"],
      "boars": ["Boar", "Hog", "Razorback"]
    }
  },
  "patterns": [
    "base",
    "size + base",
    "color + base",
    "descriptive + base",
    "origin + base",
    "size + color + base",
    "size + descriptive + base",
    "descriptive + color + base",
    "base + title",
    "size + descriptive + base + title"
  ]
}
```

**Example Outputs:**

- Common: "Wolf", "Giant Wolf"
- Uncommon: "Black Wolf", "Dire Wolf"
- Rare: "Ancient Mountain Wolf"
- Legendary: "Elder Savage Black Wolf of the Night"

**Status:** 📋 Draft proposal

---

#### Beasts - Types (`enemies/beasts/types.json`) 📋 DRAFT

**Proposed Structure:**

```json
{
  "beast_types": {
    "wolves": {
      "items": [
        { "name": "Wolf", "health": 25, "damage": "1d6", "speed": 40, "level": 3 },
        { "name": "Dire Wolf", "health": 50, "damage": "2d6", "speed": 45, "level": 8 }
      ],
      "traits": {
        "category": "beast",
        "classification": "wolf",
        "abilities": ["pack_tactics", "keen_hearing"]
      }
    }
  }
}
```

**Status:** 📋 Draft proposal

---

#### Undead - Names (`enemies/undead/names.json`) 📋 DRAFT

**Proposed Component Keys:**

- `descriptive` - Undead attributes (Risen, Cursed, Ancient, Restless, Vengeful, Hollow)
- `origin` - Former identity (Warrior, Mage, King, Knight, Priest, Noble)
- `title` - Named undead (of the Crypt, of Darkness, of the Grave, of Despair)
- `condition` - State (Decaying, Skeletal, Spectral, Withered, Rotting)
- `undead_types` - Category organization (skeleton, zombie, ghost, wraith, vampire)

**Proposed Patterns:**

```json
{
  "components": {
    "descriptive": ["Risen", "Cursed", "Ancient", "Restless", "Vengeful", "Hollow"],
    "origin": ["Warrior", "Mage", "King", "Knight", "Priest", "Noble", "Peasant"],
    "title": ["of the Crypt", "of Darkness", "of the Grave", "of Despair"],
    "condition": ["Decaying", "Skeletal", "Spectral", "Withered", "Rotting"],
    "undead_types": {
      "skeletons": ["Skeleton", "Bones", "Skeletal Warrior"],
      "zombies": ["Zombie", "Ghoul", "Corpse"],
      "ghosts": ["Ghost", "Wraith", "Specter", "Shade"],
      "vampires": ["Vampire", "Vampire Lord", "Bloodsucker"]
    }
  },
  "patterns": [
    "base",
    "descriptive + base",
    "condition + base",
    "origin + base",
    "descriptive + origin + base",
    "base + title",
    "descriptive + base + title",
    "condition + origin + base + title"
  ]
}
```

**Example Outputs:**

- Common: "Skeleton", "Zombie"
- Uncommon: "Risen Skeleton", "Cursed Zombie"
- Rare: "Ancient Warrior Skeleton", "Spectral Knight"
- Legendary: "Cursed Vampire Lord of Darkness"

**Status:** 📋 Draft proposal

---

#### Demons - Names (`enemies/demons/names.json`) 📋 DRAFT

**Proposed Component Keys:**

- `rank` - Demonic hierarchy (Lesser, Greater, Arch, Prime, Lord)
- `aspect` - Demonic nature (Fire, Shadow, Blood, Chaos, Corruption)
- `descriptive` - Attributes (Twisted, Infernal, Vile, Malevolent, Wrathful)
- `title` - Named demons (of the Abyss, of Torment, of Destruction)
- `demon_types` - Category organization (imp, fiend, demon, devil, archfiend)

**Proposed Patterns:**

```json
[
  "base",
  "rank + base",
  "aspect + base",
  "descriptive + base",
  "rank + aspect + base",
  "descriptive + rank + base",
  "base + title",
  "rank + aspect + base + title"
]
```

**Status:** 📋 Draft proposal

---

#### Elementals - Names (`enemies/elementals/names.json`) 📋 DRAFT

**Proposed Component Keys:**

- `element` - Element type (Fire, Water, Earth, Air, Lightning, Ice, Storm)
- `size` - Size modifier (Lesser, Greater, Prime, Elder, Primal)
- `descriptive` - Attributes (Raging, Ancient, Bound, Enraged, Calm)
- `title` - Named elementals (of the Volcano, of the Storm, of the Deep)
- `elemental_types` - Category organization

**Proposed Patterns:**

```json
[
  "element + base",
  "size + element + base",
  "descriptive + element + base",
  "size + descriptive + element + base",
  "element + base + title"
]
```

**Example Outputs:**

- "Fire Elemental"
- "Greater Fire Elemental"
- "Raging Fire Elemental"
- "Elder Raging Fire Elemental of the Volcano"

**Status:** 📋 Draft proposal

---

#### Dragons - Names (`enemies/dragons/names.json`) 📋 DRAFT

**Proposed Component Keys:**

- `color` - Dragon color (Red, Blue, Black, Green, White, Gold, Silver, Bronze)
- `age` - Age category (Wyrmling, Young, Adult, Ancient, Primordial)
- `descriptive` - Attributes (Wise, Cruel, Mighty, Cunning, Greedy)
- `title` - Named dragons (of the Mountain, of Destruction, Worldeater)

**Proposed Patterns:**

```json
[
  "color + base",
  "age + color + base",
  "descriptive + color + base",
  "age + descriptive + color + base",
  "base + title",
  "age + color + base + title"
]
```

**Example Outputs:**

- "Red Dragon"
- "Ancient Red Dragon"
- "Cruel Red Dragon"
- "Ancient Wise Gold Dragon of the Mountain"

**Status:** 📋 Draft proposal

---

#### Dragons - Colors (`enemies/dragons/colors.json`) 📋 DRAFT

**Expected:** Component Library or types.json with dragon properties by color

**Proposed Structure:**

```json
{
  "components": {
    "chromatic": ["Red", "Blue", "Black", "Green", "White"],
    "metallic": ["Gold", "Silver", "Bronze", "Copper", "Brass"],
    "gem": ["Amethyst", "Emerald", "Sapphire", "Topaz", "Crystal"]
  }
}
```

**OR as types.json:**

```json
{
  "dragon_colors": {
    "red": {
      "breathWeapon": "fire",
      "damageType": "fire",
      "alignment": "evil"
    },
    "gold": {
      "breathWeapon": "fire",
      "damageType": "fire",
      "alignment": "good"
    }
  }
}
```

**Status:** 📋 Draft proposal - needs review

---

#### Humanoids - Names (`enemies/humanoids/names.json`) 📋 DRAFT

**Proposed Component Keys:**

- `profession` - Role/job (Warrior, Mage, Assassin, Archer, Berserker, Cleric)
- `faction` - Group affiliation (Bandit, Guard, Cultist, Mercenary, Raider)
- `rank` - Hierarchy (Captain, Chief, Elite, Veteran, Novice, Master)
- `descriptive` - Attributes (Veteran, Rogue, Fallen, Corrupt, Zealous)
- `title` - Named humanoids (the Bloodthirsty, the Wise, the Brave)

**Proposed Patterns:**

```json
[
  "profession",
  "rank + profession",
  "faction + profession",
  "descriptive + profession",
  "rank + faction + profession",
  "descriptive + rank + profession",
  "profession + title"
]
```

**Example Outputs:**

- "Warrior"
- "Captain Warrior"
- "Bandit Warrior"
- "Veteran Captain Guard"
- "Warrior the Bloodthirsty"

**Status:** 📋 Draft proposal

---

#### Enemy Prefixes (`enemies/*/prefixes.json`) 📋 DRAFT

**Expected:** Stat modifiers for enemies (similar to weapon prefixes)

**Proposed Structure:**

```json
{
  "items": [
    {
      "name": "enraged",
      "displayName": "Enraged",
      "traits": {
        "bonusDamage": 5,
        "health": -10,
        "attackSpeed": 10
      }
    },
    {
      "name": "armored",
      "displayName": "Armored",
      "traits": {
        "bonusArmor": 5,
        "movementSpeed": -5
      }
    }
  ]
}
```

**Status:** 📋 Draft proposal - check if enemies use prefixes

---

### 4. NPCs Category 📋 DRAFT

#### Names - First Names (`npcs/names/first_names.json`) 📋 DRAFT

**Current Structure:** Unknown  
**Action Required:** Likely Component Library (no patterns)

**Expected Structure:** Component Library - Name Lists

```json
{
  "components": {
    "male": ["John", "William", "Robert", "James", "Michael", "David"],
    "female": ["Mary", "Elizabeth", "Sarah", "Jennifer", "Linda", "Patricia"],
    "surnames": ["Smith", "Johnson", "Williams", "Brown", "Jones", "Miller"],
    "fantasy_male": ["Thorin", "Aragorn", "Gandalf", "Elrond"],
    "fantasy_female": ["Arwen", "Galadriel", "Eowyn", "Luthien"]
  },
  "metadata": {
    "description": "NPC name components by gender and style",
    "version": "1.0",
    "type": "component_library"
  }
}
```

**Key Features:**

- **No patterns** - This is a component source, not generation
- **Categorized** - By gender and naming style
- **Referenced by** - NPC generators for character creation

**Status:** 📋 Draft proposal - likely no patterns needed

---

#### Occupations (`npcs/occupations/common.json`) 📋 DRAFT

**Expected:** Component Library - Occupation Lists

```json
{
  "components": {
    "merchant": ["Shopkeeper", "Trader", "Merchant", "Vendor", "Peddler"],
    "craftsman": ["Blacksmith", "Carpenter", "Tailor", "Cobbler", "Mason"],
    "service": ["Innkeeper", "Bartender", "Cook", "Servant", "Cleaner"],
    "guard": ["Guard", "Soldier", "Watchman", "Sentry"],
    "scholar": ["Scribe", "Librarian", "Scholar", "Teacher", "Sage"],
    "religious": ["Priest", "Monk", "Cleric", "Acolyte", "Bishop"]
  }
}
```

**Status:** 📋 Draft proposal

---

#### Dialogue Templates (`npcs/dialogue/templates.json`) 📋 DRAFT

**Expected:** Pattern Generation for dialogue

```json
{
  "components": {
    "greeting": ["Hello", "Greetings", "Welcome", "Good day"],
    "farewell": ["Goodbye", "Farewell", "Safe travels", "May the gods watch over you"],
    "quest_intro": ["I have a task for you", "Can you help me?", "I need your assistance"],
    "shop_greeting": ["What can I get you?", "Browse my wares", "Looking to buy or sell?"]
  },
  "patterns": [
    "greeting",
    "greeting + quest_intro",
    "shop_greeting"
  ]
}
```

**Status:** 📋 Draft proposal - may not need patterns

---

#### Dialogue Traits (`npcs/dialogue/traits.json`) 📋 DRAFT

**Expected:** Component Library - Personality traits

```json
{
  "components": {
    "friendly": ["cheerful", "warm", "welcoming", "kind"],
    "hostile": ["cold", "angry", "dismissive", "rude"],
    "neutral": ["businesslike", "professional", "formal"],
    "quirky": ["eccentric", "odd", "peculiar", "strange"]
  }
}
```

**Status:** 📋 Draft proposal

---

#### Titles (`npcs/titles/titles.json`) 📋 DRAFT

**Expected:** Pattern Generation OR Component Library

**Option A: Component Library (Simple)**

```json
{
  "components": {
    "nobility": ["Lord", "Lady", "Duke", "Duchess", "Baron", "Baroness"],
    "military": ["Captain", "General", "Commander", "Sergeant"],
    "religious": ["Father", "Mother", "Bishop", "Cardinal"],
    "academic": ["Professor", "Doctor", "Master", "Scholar"]
  }
}
```

**Option B: Pattern Generation (Complex)**

```json
{
  "components": {
    "rank": ["Lord", "Lady", "Sir", "Dame"],
    "profession": ["Master", "Grand Master", "Arch"],
    "origin": ["of the North", "of Winterfell", "of the Mountain"],
    "achievement": ["Dragonslayer", "Kingmaker", "the Wise", "the Brave"]
  },
  "patterns": [
    "rank",
    "profession",
    "rank + origin",
    "rank + achievement",
    "profession + achievement"
  ]
}
```

**Example Outputs:**

- "Lord"
- "Master"
- "Lord of the North"
- "Sir Dragonslayer"
- "Grand Master the Wise"

**Status:** 📋 Draft proposal - decide on simple vs pattern-based

---

### 5. Quests Category 📋 DRAFT

#### Templates (`quests/templates.json`) 📋 DRAFT

**Current Structure:** Unknown  
**Action Required:** Likely complex structured data (not pattern-based)

**Expected:** Quest templates with structure, not simple patterns

```json
{
  "quest_templates": {
    "fetch": {
      "title": "Retrieve [item] from [location]",
      "description": "Bring me [item] from [location] and I'll reward you.",
      "objectives": ["obtain_item", "return_to_quest_giver"],
      "rewards": ["gold", "experience", "item"]
    },
    "kill": {
      "title": "Slay [enemy_count] [enemy_type]",
      "description": "[enemy_type] are threatening [location]. Kill [enemy_count] of them.",
      "objectives": ["kill_enemies"],
      "rewards": ["gold", "experience"]
    }
  }
}
```

**Key Features:**

- **Structured templates** - Not simple name generation
- **Variable substitution** - [item], [location], [enemy_type]
- **Complex logic** - Objectives, rewards, conditionals

**Status:** 📋 Draft proposal - may need separate specification document

**Note:** Quest system may be too complex for simple pattern system. Consider dedicated quest generation system.

#### Elementals - Names (`enemies/elementals/names.json`)

**Current Structure:** Unknown  
**Action Required:** 📋 To be reviewed

**Proposed Component Keys:**

- `element` - Element type (Fire, Water, Earth, Air, Lightning)
- `size` - Size modifier (Lesser, Greater, Prime)
- `descriptive` - Attributes (Raging, Ancient, Bound)
- `elemental_types` - Category organization

**Status:** 📋 Awaiting review

#### Dragons - Names (`enemies/dragons/names.json`)

**Current Structure:** Unknown  
**Action Required:** 📋 To be reviewed

**Proposed Component Keys:**

- `color` - Dragon color (Red, Blue, Black, Gold)
- `age` - Age category (Wyrmling, Adult, Ancient)
- `descriptive` - Attributes (Wise, Cruel, Mighty)
- `title` - Named dragons (of the Mountain, of Destruction)

**Status:** 📋 Awaiting review

#### Dragons - Colors (`enemies/dragons/colors.json`)

**Current Structure:** Unknown  
**Action Required:** 📋 To be reviewed

**Expected:** Likely FlatItem (reference data with properties)

**Status:** 📋 Awaiting review

#### Humanoids - Names (`enemies/humanoids/names.json`)

**Current Structure:** Unknown  
**Action Required:** 📋 To be reviewed

**Proposed Component Keys:**

- `profession` - Role/job (Warrior, Mage, Assassin, Archer)
- `faction` - Group affiliation (Bandit, Guard, Cultist)
- `rank` - Hierarchy (Captain, Chief, Elite)
- `descriptive` - Attributes (Veteran, Rogue, Fallen)

**Status:** 📋 Awaiting review

#### Enemy Prefixes (`enemies/*/prefixes.json`)

**Current Structure:** Unknown (probably ItemPrefix with traits)  
**Action Required:** 📋 To be reviewed

**Expected:** These likely stay as ItemPrefix (not patterns)

**Status:** 📋 Awaiting review

---

### 4. NPCs Category

#### Names - First Names (`npcs/names/first_names.json`)

**Current Structure:** Unknown  
**Action Required:** 📋 To be reviewed

**Expected Structure:** NameList (simple categorized lists)

```json
{
  "items": {
    "male": ["John", "William", "Robert"],
    "female": ["Mary", "Elizabeth", "Sarah"],
    "surnames": ["Smith", "Johnson", "Williams"]
  }
}
```

**Status:** 📋 Awaiting review - likely no patterns needed (component source)

#### Occupations (`npcs/occupations/common.json`)

**Current Structure:** Unknown  
**Action Required:** 📋 To be reviewed

**Expected:** Likely FlatItem or NameList (reference data)

**Status:** 📋 Awaiting review

#### Dialogue Templates (`npcs/dialogue/templates.json`)

**Current Structure:** Unknown  
**Action Required:** 📋 To be reviewed

**Expected:** Likely NameList (categorized dialogue strings)

**Status:** 📋 Awaiting review

#### Dialogue Traits (`npcs/dialogue/traits.json`)

**Current Structure:** Unknown  
**Action Required:** 📋 To be reviewed

**Status:** 📋 Awaiting review

#### Titles (`npcs/titles/titles.json`)

**Current Structure:** Unknown  
**Action Required:** 📋 To be reviewed

**Proposed Component Keys (if HybridArray):**

- `rank` - Social rank (Lord, Lady, Sir, Dame)
- `profession` - Professional title (Master, Apprentice)
- `origin` - Geographic title (of the North, of Winterfell)
- `achievement` - Earned title (Dragonslayer, Kingmaker)

**Status:** 📋 Awaiting review

---

### 5. Quests Category

#### Templates (`quests/templates.json`)

**Current Structure:** Unknown  
**Action Required:** 📋 To be reviewed

**Expected:** Complex structured data (not likely using patterns)

**Status:** 📋 Awaiting review

---

## Migration Checklist

### Phase 1: Review Current State ✅ COMPLETE

**All JSON files have been standardized with metadata, proper structure, and weight-based rarity system.**

**General Category:** ✅ COMPLETE (9/9 files)

- [x] ✅ general/adjectives.json - Component Library
- [x] ✅ general/colors.json - Pattern Generation
- [x] ✅ general/rarity_config.json - Configuration File
- [x] ✅ general/smells.json - Pattern Generation
- [x] ✅ general/sounds.json - Pattern Generation
- [x] ✅ general/textures.json - Pattern Generation
- [x] ✅ general/time_of_day.json - Pattern Generation
- [x] ✅ general/verbs.json - Component Library
- [x] ✅ general/weather.json - Pattern Generation

**Items Category:** ✅ COMPLETE (17/17 files)

- [x] ✅ items/armor/names.json - Pattern Generation
- [x] ✅ items/armor/prefixes.json - Prefix Modifiers
- [x] ✅ items/armor/suffixes.json - Suffix Modifiers
- [x] ✅ items/armor/types.json - Item Catalog
- [x] ✅ items/consumables/effects.json - Component Library
- [x] ✅ items/consumables/names.json - Pattern Generation
- [x] ✅ items/consumables/rarities.json - Configuration
- [x] ✅ items/consumables/types.json - Item Catalog
- [x] ✅ items/enchantments/effects.json - Component Library
- [x] ✅ items/enchantments/prefixes.json - Prefix Modifiers
- [x] ✅ items/enchantments/suffixes.json - Suffix Modifiers
- [x] ✅ items/materials/names.json - Pattern Generation
- [x] ✅ items/materials/types.json - Item Catalog
- [x] ✅ items/weapons/names.json - Pattern Generation
- [x] ✅ items/weapons/prefixes.json - Prefix Modifiers
- [x] ✅ items/weapons/suffixes.json - Suffix Modifiers
- [x] ✅ items/weapons/types.json - Item Catalog

**Enemies Category:** ✅ COMPLETE (59/59 files)

- [x] ✅ enemies/beasts/* (5 files: names, prefixes, suffixes, traits, types)
- [x] ✅ enemies/demons/* (5 files: names, prefixes, suffixes, traits, types)
- [x] ✅ enemies/dragons/* (6 files: colors, names, prefixes, suffixes, traits, types)
- [x] ✅ enemies/elementals/* (5 files: names, prefixes, suffixes, traits, types)
- [x] ✅ enemies/goblinoids/* (4 files: names, suffixes, traits, types)
- [x] ✅ enemies/humanoids/* (5 files: names, prefixes, suffixes, traits, types)
- [x] ✅ enemies/insects/* (4 files: names, suffixes, traits, types)
- [x] ✅ enemies/orcs/* (4 files: names, suffixes, traits, types)
- [x] ✅ enemies/plants/* (4 files: names, suffixes, traits, types)
- [x] ✅ enemies/reptilians/* (4 files: names, suffixes, traits, types)
- [x] ✅ enemies/trolls/* (4 files: names, suffixes, traits, types)
- [x] ✅ enemies/undead/* (5 files: names, prefixes, suffixes, traits, types)
- [x] ✅ enemies/vampires/* (4 files: names, suffixes, traits, types)

**NPCs Category:** ✅ COMPLETE (14/14 files)

- [x] ✅ npcs/dialogue/farewells.json - Component Library
- [x] ✅ npcs/dialogue/greetings.json - Component Library
- [x] ✅ npcs/dialogue/rumors.json - Component Library
- [x] ✅ npcs/dialogue/templates.json - Template Library
- [x] ✅ npcs/dialogue/traits.json - Component Library
- [x] ✅ npcs/names/first_names.json - Name Catalog
- [x] ✅ npcs/names/last_names.json - Name Catalog
- [x] ✅ npcs/occupations/common.json - Component Library
- [x] ✅ npcs/occupations/criminal.json - Component Library
- [x] ✅ npcs/occupations/magical.json - Component Library
- [x] ✅ npcs/occupations/noble.json - Component Library
- [x] ✅ npcs/personalities/backgrounds.json - Component Library
- [x] ✅ npcs/personalities/quirks.json - Component Library
- [x] ✅ npcs/personalities/traits.json - Component Library

**Quests Category:** ✅ COMPLETE (14/14 files)

- [x] ✅ quests/locations/dungeons.json - Component Library
- [x] ✅ quests/locations/towns.json - Component Library
- [x] ✅ quests/locations/wilderness.json - Component Library
- [x] ✅ quests/objectives/hidden.json - Component Library
- [x] ✅ quests/objectives/primary.json - Component Library
- [x] ✅ quests/objectives/secondary.json - Component Library
- [x] ✅ quests/rewards/experience.json - Configuration
- [x] ✅ quests/rewards/gold.json - Configuration
- [x] ✅ quests/rewards/items.json - Component Library
- [x] ✅ quests/templates/delivery.json - Quest Templates
- [x] ✅ quests/templates/escort.json - Quest Templates
- [x] ✅ quests/templates/fetch.json - Quest Templates
- [x] ✅ quests/templates/investigate.json - Quest Templates
- [x] ✅ quests/templates/kill.json - Quest Templates

**Overall Progress:** 113/113 files standardized (100%) ✅

### Phase 2: Standardize Files ✅ COMPLETE

**All categories standardized - December 16, 2025**

- ✅ General Category (9 files) - Component libraries and pattern generation
- ✅ Items Category (17 files) - Names, types, prefixes, suffixes, materials
- ✅ Enemies Category (59 files) - All enemy types with full trait systems
- ✅ NPCs Category (14 files) - Names, occupations, personalities, dialogue
- ✅ Quests Category (14 files) - Templates, objectives, rewards, locations

**Key Achievements:**

- ✅ All files have standardized metadata (description, version, lastUpdated, type)
- ✅ Pattern Generation files use components + patterns structure
- ✅ Item/Enemy Catalogs use type-level traits + item arrays
- ✅ Prefix/Suffix Modifiers use rarity-organized trait structure
- ✅ Weight-based rarity system implemented across all categories
- ✅ Auto-generated metadata fields (componentKeys, patternTokens, totals)

### Phase 3: Update ContentBuilder 📋 NEXT PHASE

- [ ] Update file type detection to recognize all standard structures
- [ ] Add pattern validation for names.json files
- [ ] Add live example preview for pattern-based generation
- [ ] Support weight-based rarity visualization
- [ ] Add metadata auto-generation on save
- [ ] Test all 113 standardized files

### Phase 4: Runtime Implementation 📋 PLANNED

- [ ] Create PatternExecutor service for runtime pattern resolution
- [ ] Update ItemGenerator to use types.json catalogs
- [ ] Update EnemyGenerator to use types.json catalogs
- [ ] Implement weight-based rarity calculation
- [ ] Update all generators to use new data structures
- [ ] Write comprehensive integration tests

---

## Notes & Decisions

### 2025-12-16

**Decision 1:** Use simple component keys (e.g., `material`, `quality`) instead of explicit prefixes (e.g., `prefix_material`)

- **Rationale:** More readable patterns, easier to type, more intuitive
- **Trade-off:** Less explicit about position, but position is implied by order in pattern
- **Rule:** Position in pattern determines prefix vs suffix behavior

**Decision 2:** Component keys are flexible - create semantic keys as needed

- **Core Insight:** `enchantment` and `title` are just component keys like `material` or `quality`
- **Semantic Meaning:** Derived from key name, pattern position, and content
- **Universal Keys:** Standards define common keys (material, quality, etc.)
- **Custom Keys:** Add category-specific keys as needed (emotion, temperature, age, rarity, etc.)
- **Enchantment vs Title:** Two separate components because they represent different concepts:
  - `enchantment` = magical property ("of Slaying", "of Power")
  - `title` = legendary name ("of the Dragon", "of the Hero")

**Decision 3:** Separate item types into dedicated types.json files

- **Rationale:** Cleaner separation, easier editing, single source of truth for item stats
- **Structure:** types.json contains item catalog + base traits, names.json contains generation components
- **Base Token:** Patterns resolve `base` by picking from types.json items
- **Traits:** Base traits (damage, slot, etc.) stored in types.json alongside items
- **Example Structure:**

  ```json
  // items/weapons/types.json
  {
    "weapon_types": {
      "swords": {
        "items": ["Longsword", "Shortsword", "Greatsword"],
        "traits": {
          "damageType": "slashing",
          "damageRange": "1d8",
          "slot": "mainhand",
          "twoHanded": false
        }
      }
    }
  }
  
  // items/weapons/names.json
  {
    "components": {
      "material": ["Iron", "Steel", "Mithril"],
      "quality": ["Fine", "Superior", "Masterwork"],
      "enchantment": ["of Slaying", "of Power"]
    },
    "patterns": [
      "base",
      "material + base",
      "quality + material + base + enchantment"
    ]
  }
  ```

**Decision 4:** Trait/stat distribution across files

- **types.json** → Base traits (what ALL items of this type have)
- **prefixes.json** → Stat modifiers (what "Flaming" adds: +5 fire damage)
- **suffixes.json** → Stat modifiers (what "of Slaying" adds: +10 vs undead)
- **Runtime Calculation:** Final stats = base traits + prefix modifiers + suffix modifiers

**Decision 5:** Use item-level stats within types.json for maximum flexibility

- **Structure:** Type-level traits (shared properties) + item-level stats (unique properties)
- **Type-Level Traits:** Properties ALL items of this type share (damageType, slot, category)
- **Item-Level Stats:** Properties unique to each item (damage, weight, value, rarity)
- **Items Format:** Array of objects with `name` + individual stats
- **Stat Variance:** Start with fixed stats, allow for range notation (e.g., "1d6-1d10")
- **Applies To:** ALL categories (weapons, armor, enemies, NPCs, items)
- **Example:**
  
  ```json
  {
    "weapon_types": {
      "swords": {
        "items": [
          { "name": "Shortsword", "damage": "1d6", "weight": 2.0, "value": 10 },
          { "name": "Longsword", "damage": "1d8", "weight": 3.0, "value": 15 },
          { "name": "Greatsword", "damage": "2d6", "weight": 6.0, "value": 50, "twoHanded": true }
        ],
        "traits": {
          "damageType": "slashing",
          "slot": "mainhand",
          "category": "sword"
        }
      }
    }
  }
  ```

**Decision 6:** General Files - Remove `items` arrays, use Pattern Generation

- **Date:** December 16, 2025
- **Context:** General files (colors, smells, sounds, etc.) had duplicate `items` arrays
- **Decision:** Remove `items` arrays entirely, use components directly for pattern generation
- **Rationale:**
  - ✅ Eliminates duplication between `items` and `components`
  - ✅ Components ARE the data source - no need for separate list
  - ✅ Consistent with names.json approach (`base` token resolves from types.json)
  - ✅ Simpler structure, easier to maintain
- **Applied To:** colors, smells, sounds, textures, time_of_day, weather (6 files)
- **Exception:** verbs.json converted to Component Library (no patterns)

**Decision 7:** General Files - Component Library vs Pattern Generation

- **Date:** December 16, 2025
- **Component Library:** Reference data only, no procedural generation
  - Files: adjectives, materials, verbs
  - Purpose: Provide categorized lists for other files to reference
  - Structure: Components only, no patterns
  - metadata.type: "component_library"
- **Pattern Generation:** Procedural content generation
  - Files: colors, smells, sounds, textures, time_of_day, weather
  - Purpose: Generate dynamic descriptive text via patterns
  - Structure: Components + patterns
  - metadata.type: "pattern_generation"
- **Rationale:** Different purposes require different structures

**Decision 8:** General Files - Convert verbs.json to Component Library

- **Date:** December 16, 2025
- **Context:** verbs.json had broken patterns referencing non-existent components (`verb`, `adverb`, `preposition`)
- **Decision:** Convert from Pattern Generation to Component Library
- **Rationale:**
  - ✅ Patterns were completely broken and meaningless
  - ✅ Verbs are better used as categorized reference data
  - ✅ Combat/magic/movement categories are more useful than pattern generation
  - ✅ Runtime can pick specific verb types (offensive, defensive, etc.)
- **Result:** Removed `items` and `patterns`, kept categorized `components`

**Decision 9:** General Files - Singular component keys for Pattern Generation

- **Date:** December 16, 2025
- **Context:** Some files had plural keys (`periods`, `modifiers`) that didn't match singular tokens in patterns
- **Decision:** All component keys in Pattern Generation files must be singular and match pattern tokens EXACTLY
- **Examples:**
  - ❌ Component: `base_colors`, Pattern: `base_color` (mismatch)
  - ✅ Component: `base_color`, Pattern: `base_color` (match)
  - ❌ Component: `periods`, Pattern: `period` (mismatch)
  - ✅ Component: `period`, Pattern: `period` (match)
- **Applied To:** colors, time_of_day (fixed plural keys)
- **Rationale:** Eliminates token resolution errors, makes patterns more predictable

---

---

## Cross-File Component References

### Overview

Files can reference components from other files to avoid duplication and maintain consistency across the game data. This is especially useful for shared data like materials, colors, and adjectives.

### Reference Syntax

**Format:** `@category/filename:component_key`

**Structure:**

- `@` - Indicates a cross-file reference
- `category/filename` - Path to the source file (relative to Data/Json folder)
- `:component_key` - Specific component array to reference

### Examples

**Basic Reference:**

```json
{
  "components": {
    "material": "@general/materials:metals",
    "color": "@general/colors:base_color",
    "quality": ["Fine", "Superior", "Masterwork"]
  }
}
```

**Multiple References:**

```json
{
  "components": {
    "metal_material": "@general/materials:metals",
    "natural_material": "@general/materials:natural",
    "positive_adjective": "@general/adjectives:positive",
    "size": "@general/adjectives:size"
  }
}
```

### When to Use References

**✅ Good Use Cases:**

- **Shared vocabularies** - Materials, colors, sizes used across multiple categories
- **Consistency enforcement** - All weapons use same material list
- **Centralized updates** - Change materials.json once, affects all referencing files
- **Reducing duplication** - Don't copy ["Iron", "Steel", ...] 20 times

**❌ Avoid References For:**

- **Category-specific data** - Weapon types, spell schools, enemy abilities
- **Highly customized lists** - When you need a subset or modified version
- **Performance-critical paths** - References add a lookup step

### Resolution Behavior

**At Runtime (Game):**

1. PatternExecutor loads referenced file
2. Extracts specified component array
3. Caches result for performance
4. Uses cached data for pattern resolution

**In ContentBuilder:**

1. On file load, detect `@` references
2. Load referenced files automatically
3. Display referenced data as read-only
4. Show warning if referenced file missing
5. Allow "inline" action to convert reference to local copy

### Reference Validation

**Valid References:**

- ✅ `@general/materials:metals` - Exists in general/materials.json
- ✅ `@items/weapons/names:material` - Cross-category reference
- ✅ `@general/adjectives:positive` - Standard reference

**Invalid References:**

- ❌ `@general/materials:invalid_key` - Component key doesn't exist
- ❌ `@missing/file:key` - Referenced file doesn't exist
- ❌ `general/materials:metals` - Missing `@` prefix
- ❌ `@general/materials` - Missing `:component_key`

**ContentBuilder Validation:**

- Warn on file load if reference invalid
- Show "broken reference" indicator in UI
- Offer "fix" action to inline the data or correct path
- Prevent save if critical references broken

### Example Use Case: Weapon Names

**Before (Duplication):**

```json
// items/weapons/names.json
{
  "components": {
    "material": ["Iron", "Steel", "Bronze", "Mithril", "Adamantine"],
    "quality": ["Fine", "Superior", "Masterwork"]
  }
}

// items/armor/names.json
{
  "components": {
    "material": ["Iron", "Steel", "Bronze", "Mithril", "Adamantine"],
    "quality": ["Fine", "Superior", "Masterwork"]
  }
}
```

**After (References):**

```json
// general/materials.json
{
  "components": {
    "metals": ["Iron", "Steel", "Bronze", "Mithril", "Adamantine"]
  }
}

// items/weapons/names.json
{
  "components": {
    "material": "@general/materials:metals",
    "quality": ["Fine", "Superior", "Masterwork"]
  }
}

// items/armor/names.json
{
  "components": {
    "material": "@general/materials:metals",
    "quality": ["Fine", "Superior", "Masterwork"]
  }
}
```

**Benefits:**

- ✅ Update materials once, applies everywhere
- ✅ Consistent material lists across all items
- ✅ Smaller file sizes (references vs full arrays)
- ✅ Clear source of truth (general/materials.json)

### ContentBuilder UI Support

**Reference Display:**

```
┌─ Components ────────────────────────────────┐
│ material: @general/materials:metals [📎]    │
│   → [Iron, Steel, Bronze, ...] (10 items)  │
│   [View Source] [Inline]                    │
│                                              │
│ quality: Local Data                         │
│   [Fine, Superior, Masterwork]              │
│   [+] Add  [-] Remove                       │
└──────────────────────────────────────────────┘
```

**Actions:**

- **[📎] Reference Icon** - Indicates this is a reference
- **[View Source]** - Opens the referenced file
- **[Inline]** - Converts reference to local copy (for customization)
- **[Change Reference]** - Update the reference path

### Implementation Notes

**File Format:**

References are stored as strings in the JSON:

```json
{
  "components": {
    "material": "@general/materials:metals"
  }
}
```

**Runtime Loading:**

```csharp
public class ComponentResolver
{
    private Dictionary<string, List<string>> _cache = new();
    
    public List<string> Resolve(string componentValue)
    {
        // Check if it's a reference
        if (componentValue.StartsWith("@"))
        {
            return LoadReference(componentValue);
        }
        
        // It's an array, parse normally
        return JsonConvert.DeserializeObject<List<string>>(componentValue);
    }
    
    private List<string> LoadReference(string reference)
    {
        // Cache check
        if (_cache.TryGetValue(reference, out var cached))
            return cached;
        
        // Parse: @category/file:key
        var parts = reference.TrimStart('@').Split(':');
        var filePath = parts[0]; // "general/materials"
        var componentKey = parts[1]; // "metals"
        
        // Load and cache
        var data = LoadJsonFile(filePath);
        var component = data["components"][componentKey].ToObject<List<string>>();
        _cache[reference] = component;
        
        return component;
    }
}
```

### Migration Strategy

**Phase 1: Identify Duplicates**

- Scan all JSON files for duplicate component arrays
- Report: "material array appears in 15 files"
- Suggest: "Move to general/materials.json"

**Phase 2: Create Reference Files**

- Create general/materials.json with all unique materials
- Create general/colors.json with all unique colors
- Create general/adjectives.json with all unique adjectives

**Phase 3: Replace with References**

- Update each file to use `@general/materials:metals` instead of local array
- Validate all references resolve correctly
- Test in ContentBuilder and runtime

**Phase 4: Gradual Adoption**

- Start with General category (already done)
- Migrate Items category next
- Enemies, NPCs, Quests follow
- Monitor for issues, rollback if needed

### Future Enhancements

**Potential Features:**

- **Reference Chains** - `@file1:key1` references `@file2:key2`
- **Partial References** - `@file:key[0:5]` (first 5 items only)
- **Combined References** - `["@file:key1", "@file:key2"]` (merge two sources)
- **Reference Filtering** - `@file:key[rarity>2]` (advanced queries)
- **Bidirectional Linking** - ContentBuilder shows "used by 12 files"

**For Now:**

Keep it simple: Direct references only, no chaining or filtering.

---

## Weight-Based Rarity System

### Overview

Rarity in this game is **emergent** - it's calculated from the **combined weights** of all components used to create an item. This creates a flexible, realistic system where:

- Materials contribute to rarity (rare materials = rare items)
- Prefixes/suffixes add rarity weight (powerful modifiers = rarer)
- Base items have minimal rarity impact (anyone can craft a "Longsword")
- Combinations determine final rarity (Mythril + Dragonbone + Fire = Epic)

### Rarity Weight Thresholds

**Master Rarity Table:**

| Rarity | Weight Range | Color | Hex Code | Drop Rate | Description |
|--------|--------------|-------|----------|-----------|-------------|
| **Common** | 0-20 | Gray | `#808080` | 60% | Basic materials, simple items |
| **Uncommon** | 21-50 | Green | `#00FF00` | 25% | Quality materials, minor enchantments |
| **Rare** | 51-100 | Blue | `#0000FF` | 10% | Exotic materials, notable enchantments |
| **Epic** | 101-200 | Purple | `#A020F0` | 4% | Legendary materials, powerful magic |
| **Legendary** | 201+ | Orange | `#FFA500` | 1% | Mythical materials, artifact-level |

**Note:** These thresholds are configurable and can be adjusted based on game balance testing.

---

### Component Weight Multipliers

Different component types contribute differently to final rarity:

| Component Type | Multiplier | Rationale |
|----------------|------------|-----------|
| `material` | 1.0× | Materials have full impact (Mythril vs Iron matters) |
| `prefix` | 1.0× | Prefixes define power level (Dragonbone is epic-tier) |
| `suffix` | 0.8× | Suffixes add flavor but less power impact |
| `base` | 0.5× | Base items have minimal rarity (Longsword vs Shortsword) |
| `quality` | 1.2× | Quality matters MORE (Masterwork vs Ruined) |
| `descriptive` | 1.0× | Special attributes (Ancient, Cursed, etc.) |

**Example Calculation:**

```
"Masterwork Mythril Longsword of Fire"

Components:
  quality:    "Masterwork" (weight 40) × 1.2 = 48
  material:   "Mythril" (weight 50) × 1.0    = 50
  base:       "Longsword" (weight 10) × 0.5  = 5
  suffix:     "of Fire" (weight 40) × 0.8    = 32
                                              ───
Total Weight: 135

Rarity Mapping: 135 → Epic tier (101-200 range)
Result: "Masterwork Mythril Longsword of Fire" (Epic)
```

---

### JSON Structure Changes

#### types.json - Add Weights to Base Items

**OLD Structure:**

```json
{
  "weapon_types": {
    "swords": {
      "items": [
        { "name": "Shortsword", "damage": "1d6", "weight": 2.0 },
        { "name": "Longsword", "damage": "1d8", "weight": 3.0 }
      ],
      "traits": {
        "damageType": "slashing",
        "slot": "mainhand"
      }
    }
  }
}
```

**NEW Structure (with rarity weights):**

```json
{
  "weapon_types": {
    "swords": {
      "items": [
        {
          "name": "Shortsword",
          "damage": "1d6",
          "weight": 2.0,
          "rarityWeight": 5
        },
        {
          "name": "Longsword",
          "damage": "1d8",
          "weight": 3.0,
          "rarityWeight": 10
        },
        {
          "name": "Katana",
          "damage": "1d10",
          "weight": 2.5,
          "rarityWeight": 35
        }
      ],
      "traits": {
        "damageType": "slashing",
        "slot": "mainhand"
      }
    }
  }
}
```

**Note:** Added `rarityWeight` field to track rarity contribution (separate from physical `weight`)

---

#### names.json - Components Get Weights (NO items array)

**OLD Structure (simple arrays):**

```json
{
  "components": {
    "material": ["Iron", "Steel", "Mythril"]
  }
}
```

**NEW Structure (objects with weights):**

```json
{
  "components": {
    "material": [
      { "name": "Iron", "rarityWeight": 5 },
      { "name": "Steel", "rarityWeight": 10 },
      { "name": "Mythril", "rarityWeight": 50 },
      { "name": "Adamantine", "rarityWeight": 75 },
      { "name": "Void Crystal", "rarityWeight": 250 }
    ],
    "quality": [
      { "name": "Ruined", "rarityWeight": 2 },
      { "name": "Fine", "rarityWeight": 10 },
      { "name": "Superior", "rarityWeight": 25 },
      { "name": "Masterwork", "rarityWeight": 40 },
      { "name": "Legendary", "rarityWeight": 90 }
    ],
    "enchantment": [
      { "name": "of Sharpness", "rarityWeight": 15 },
      { "name": "of Fire", "rarityWeight": 40 },
      { "name": "of the Dragon", "rarityWeight": 85 },
      { "name": "of Divine Power", "rarityWeight": 200 }
    ]
  },
  "patterns": [
    "base",
    "material + base",
    "quality + material + base",
    "quality + material + base + enchantment"
  ],
  "metadata": {
    "description": "Weapon name patterns with rarityWeight-based components",
    "version": "2.0",
    "componentKeys": ["material", "quality", "enchantment"],
    "patternTokens": ["base", "material", "quality", "enchantment"]
  }
}
```

**IMPORTANT:** names.json does NOT contain an items array. Base items are stored in types.json. The `base` token resolves from types.json.

---

#### prefixes.json / suffixes.json - Flatten Structure + Add Weights

**OLD Structure (rarity-organized tiers):**

```json
{
  "common": {
    "Rusty": {
      "displayName": "Rusty",
      "traits": { "damageMultiplier": { "value": 0.8, "type": "number" } }
    }
  },
  "rare": {
    "Mythril": {
      "displayName": "Mythril",
      "traits": { "damageBonus": { "value": 5, "type": "number" } }
    }
  }
}
```

**NEW Structure (flat with weights):**

```json
{
  "prefixes": {
    "Rusty": {
      "displayName": "Rusty",
      "rarityWeight": 2,
      "traits": {
        "damageMultiplier": { "value": 0.8, "type": "number" },
        "durability": { "value": 50, "type": "number" }
      }
    },
    "Steel": {
      "displayName": "Steel",
      "rarityWeight": 10,
      "traits": {
        "damageBonus": { "value": 3, "type": "number" },
        "durability": { "value": 120, "type": "number" }
      }
    },
    "Mythril": {
      "displayName": "Mythril",
      "rarityWeight": 50,
      "traits": {
        "damageBonus": { "value": 5, "type": "number" },
        "durability": { "value": 150, "type": "number" },
        "glowEffect": { "value": true, "type": "boolean" }
      }
    },
    "Dragonbone": {
      "displayName": "Dragonbone",
      "rarityWeight": 80,
      "traits": {
        "damageBonus": { "value": 10, "type": "number" },
        "fireResist": { "value": 25, "type": "number" },
        "durability": { "value": 200, "type": "number" }
      }
    }
  },
  "metadata": {
    "description": "Weapon prefixes with rarityWeight-based rarity and stat modifiers",
    "version": "2.0",
    "lastUpdated": "2025-12-16"
  }
}
```

**Key Changes:**

- ❌ **Removed** rarity tier organization (common/rare/epic)
- ✅ **Added** `weight` field to each prefix/suffix
- ✅ **Flattened** structure (all prefixes at same level)
- ✅ **Kept** trait system with `{ value, type }` objects

---

### IMPORTANT: Weight vs RarityWeight Naming Convention

**Consistent Property Name: `rarityWeight`**

ALL rarity-related weight fields use the property name `rarityWeight` for consistency:

| Location | Property Name | Purpose |
|----------|---------------|---------|
| **types.json** items | `rarityWeight` | Base item rarity contribution |
| **names.json** components | `rarityWeight` | Component rarity contribution |
| **prefixes.json** | `rarityWeight` | Prefix rarity contribution |
| **suffixes.json** | `rarityWeight` | Suffix rarity contribution |

**Physical Weight vs Rarity Weight:**

```json
// types.json - Base items have BOTH
{
  "items": [
    {
      "name": "Longsword",
      "weight": 3.0,           // Physical weight (encumbrance, kg)
      "rarityWeight": 10       // Rarity contribution (number)
    }
  ]
}

// names.json - Components have ONLY rarityWeight
{
  "components": {
    "material": [
      {
        "name": "Mythril",
        "rarityWeight": 50      // Rarity contribution only
      }
    ]
  }
}
```

**Why `rarityWeight` everywhere?**

- ✅ Consistent property name across all files
- ✅ Clear distinction from physical `weight` (encumbrance)
- ✅ Explicit purpose (rarity calculation, not physics)
- ✅ Prevents confusion in code (`item.rarityWeight` always means rarity)

**Algorithm Usage:**

```pseudocode
// Components from names.json
totalWeight += component.rarityWeight * multiplier

// Base items from types.json
totalWeight += item.rarityWeight * multiplier

// Prefixes/Suffixes
totalWeight += prefix.rarityWeight * multiplier
```

---

### Rarity Configuration File

**New File: `general/rarity_config.json`**

```json
{
  "thresholds": {
    "common": {
      "min": 0,
      "max": 20,
      "color": "#808080",
      "displayName": "Common",
      "dropRate": 0.60,
      "glowEffect": false
    },
    "uncommon": {
      "min": 21,
      "max": 50,
      "color": "#00FF00",
      "displayName": "Uncommon",
      "dropRate": 0.25,
      "glowEffect": false
    },
    "rare": {
      "min": 51,
      "max": 100,
      "color": "#0000FF",
      "displayName": "Rare",
      "dropRate": 0.10,
      "glowEffect": true
    },
    "epic": {
      "min": 101,
      "max": 200,
      "color": "#A020F0",
      "displayName": "Epic",
      "dropRate": 0.04,
      "glowEffect": true
    },
    "legendary": {
      "min": 201,
      "max": 999,
      "color": "#FFA500",
      "displayName": "Legendary",
      "dropRate": 0.01,
      "glowEffect": true
    }
  },
  "weight_multipliers": {
    "material": 1.0,
    "prefix": 1.0,
    "suffix": 0.8,
    "base": 0.5,
    "quality": 1.2,
    "descriptive": 1.0,
    "enchantment": 0.8,
    "title": 0.6
  },
  "metadata": {
    "description": "Rarity system configuration with weight thresholds, colors, and drop rates",
    "version": "1.0",
    "lastUpdated": "2025-12-16"
  }
}
```

---

### Weight-Based Pattern Execution Algorithm

**Updated Pattern Execution with Weight Calculation:**

```pseudocode
function executePattern(pattern, components, typesJson, prefixes, suffixes):
    let name = ""
    let totalWeight = 0
    let appliedTraits = {}
    
    // Parse pattern: "quality + material + base + enchantment"
    tokens = pattern.split(" + ").map(trim)
    
    for each token in tokens:
        if token == "base":
            // Pick random base item from types.json
            item = selectRandomFromTypes(typesJson)
            name += item.name + " "
            totalWeight += item.rarityWeight * config.multipliers.base
            
        else if components[token] exists:
            // Pick random component from names.json components
            component = selectRandom(components[token])
            name += component.name + " "
            totalWeight += component.rarityWeight * config.multipliers[token]
            
            // If this component has a matching prefix/suffix with traits
            if prefixes[component.name] exists:
                appliedTraits = mergeTraits(appliedTraits, prefixes[component.name].traits)
            
        else:
            // Invalid token - skip with warning
            log.warn("Unknown token: " + token)
    
    // Calculate final rarity tier from total weight
    rarity = getRarityTier(totalWeight)
    
    return {
        name: name.trim(),
        weight: totalWeight,
        rarity: rarity,
        traits: appliedTraits,
        color: config.thresholds[rarity].color,
        glowEffect: config.thresholds[rarity].glowEffect
    }

function selectRandomFromTypes(typesJson):
    // Select random type category (e.g., "swords")
    typeCategory = selectRandom(typesJson.weapon_types)
    // Select random item from that category's items array
    item = selectRandom(typeCategory.items)
    return item

function getRarityTier(weight):
    for each tier in config.thresholds:
        if weight >= tier.min AND weight <= tier.max:
            return tier.name
    
    // Fallback
    return "common"

function mergeTraits(existing, new):
    // Combine trait values (sum bonuses, override booleans, etc.)
    merged = existing.clone()
    
    for each trait in new:
        if merged[trait] exists:
            if trait.type == "number":
                merged[trait].value += new[trait].value  // Stack bonuses
            else:
                merged[trait] = new[trait]  // Override
        else:
            merged[trait] = new[trait]
    
    return merged
```

---

### Loot Generation with Weight Targeting

**Loot Table Configuration:**

```json
{
  "loot_tables": {
    "common_chest": {
      "targetWeight": { "min": 5, "max": 25 },
      "allowedComponents": ["all"],
      "count": { "min": 1, "max": 3 }
    },
    "rare_chest": {
      "targetWeight": { "min": 60, "max": 120 },
      "allowedComponents": ["all"],
      "count": { "min": 1, "max": 2 }
    },
    "boss_drop_epic": {
      "targetWeight": { "min": 120, "max": 180 },
      "allowedComponents": ["all"],
      "count": 1
    },
    "legendary_quest_reward": {
      "targetWeight": { "min": 220, "max": 400 },
      "allowedComponents": ["legendary_only"],
      "count": 1
    }
  }
}
```

**Loot Generation Algorithm:**

```pseudocode
function generateLoot(lootTable):
    targetMin = lootTable.targetWeight.min
    targetMax = lootTable.targetWeight.max
    targetWeight = random(targetMin, targetMax)
    
    // Build item to approximately match target weight
    remainingWeight = targetWeight
    selectedComponents = []
    
    // Strategy: Pick components whose weights sum near target
    
    // 1. Pick material (usually highest weight contributor)
    materialWeight = remainingWeight * 0.4  // Allocate 40% to material
    material = selectClosestWeight(components.material, materialWeight)
    selectedComponents.add(material)
    remainingWeight -= material.weight * multipliers.material
    
    // 2. Pick quality
    qualityWeight = remainingWeight * 0.3
    quality = selectClosestWeight(components.quality, qualityWeight)
    selectedComponents.add(quality)
    remainingWeight -= quality.weight * multipliers.quality
    
    // 3. Pick enchantment if weight remaining
    if remainingWeight > 20:
        enchantment = selectClosestWeight(components.enchantment, remainingWeight)
        selectedComponents.add(enchantment)
    
    // 4. Pick base item (low weight, any will do)
    base = selectRandom(items)
    
    // 5. Generate name via pattern
    pattern = selectPattern(selectedComponents)  // e.g., "quality + material + base + enchantment"
    item = executePattern(pattern, selectedComponents, base)
    
    return item

function selectClosestWeight(componentArray, targetWeight):
    // Find component with rarityWeight closest to target
    closest = componentArray[0]
    minDiff = abs(closest.rarityWeight - targetWeight)
    
    for each component in componentArray:
        diff = abs(component.rarityWeight - targetWeight)
        if diff < minDiff:
            closest = component
            minDiff = diff
    
    return closest
```

---

### Weight Ranges by Component Type

**Recommended Weight Ranges:**

| Component Type | Common | Uncommon | Rare | Epic | Legendary |
|----------------|--------|----------|------|------|-----------|
| **Materials** | 2-10 | 11-30 | 31-70 | 71-150 | 151+ |
| **Prefixes** | 2-10 | 11-30 | 31-70 | 71-150 | 151+ |
| **Suffixes** | 5-15 | 16-35 | 36-65 | 66-120 | 121+ |
| **Quality** | 2-12 | 13-28 | 29-50 | 51-90 | 91+ |
| **Base Items** | 5-10 | 11-20 | 21-40 | 41-80 | 81+ |
| **Enchantments** | 10-18 | 19-40 | 41-75 | 76-140 | 141+ |

**Examples:**

**Common Materials:** Iron (5), Copper (3), Wood (4), Leather (6)  
**Rare Materials:** Mythril (50), Adamantine (65), Crystal (55)  
**Legendary Materials:** Void Crystal (250), Divine Essence (300), Dragon Heart (275)

**Common Prefixes:** Rusty (2), Old (3), Simple (5)  
**Epic Prefixes:** Dragonbone (80), Ancient (75), Celestial (100)  
**Legendary Prefixes:** Godforged (220), Eternal (250), Void-Blessed (280)
Rare:       "Fine Steel Longsword"
Epic:       "Masterwork Ancient Steel Longsword"
Legendary:  "Masterwork Enchanted Mithril Longsword of the Dragon"

```

**Bad Pattern Progression:**

```text
Common:     "Longsword"
Uncommon:   "Small Small Longsword" ❌ (duplicate token)
Rare:       "Steel Small Small Longsword" ❌ (meaningless complexity)
Epic:       "Steel Steel Steel Longsword" ❌ (repetitive)
Legendary:  "Longsword Steel" ❌ (backwards grammar)
```

### Rarity Weighting (Future Enhancement)

**Optional: Weight patterns by rarity:**

```json
{
  "patterns": [
    {
      "pattern": "base",
      "weight": 100,
      "rarity": "common"
    },
    {
      "pattern": "quality + material + base + enchantment + title",
      "weight": 1,
      "rarity": "legendary"
    }
  ]
}
```

**For Now:** Keep patterns as simple strings. Rarity is determined by token count.

---

## Pattern Execution Algorithm

### Runtime Pattern Resolution

**High-Level Overview:**

The pattern system takes a pattern string (e.g., `"quality + material + base"`) and resolves it into a final name by picking random values from component arrays.

### Pseudocode Algorithm

```pseudocode
function ExecutePattern(pattern, items, components, random):
    // Step 1: Parse pattern into tokens
    tokens = pattern.split(" + ").map(trim)
    
    // Step 2: Resolve each token to a value
    parts = []
    for each token in tokens:
        value = ResolveToken(token, items, components, random)
        if value is not null:
            parts.add(value)
    
    // Step 3: Assemble final name
    if parts.isEmpty():
        // Fallback: random base item
        return random.pick(items)
    
    return parts.join(" ")

function ResolveToken(token, items, components, random):
    // Special token: "base"
    if token == "base":
        if items.isEmpty():
            return null
        return random.pick(items)
    
    // Component lookup
    if components.contains(token):
        componentArray = components[token]
        if componentArray.isEmpty():
            warn("Component '{token}' is empty")
            return null
        return random.pick(componentArray)
    
    // Cross-file reference (if supported)
    if token.startsWith("@"):
        return ResolveReference(token, random)
    
    // Token not found
    warn("Pattern token '{token}' not found in components")
    return null

function ResolveReference(reference, random):
    // Parse: @category/file:key
    parts = reference.removePrefix("@").split(":")
    filePath = parts[0]  // "general/materials"
    componentKey = parts[1]  // "metals"
    
    // Load referenced file (with caching)
    data = LoadJsonFile(filePath)
    componentArray = data.components[componentKey]
    
    if componentArray.isEmpty():
        warn("Referenced component '{reference}' is empty")
        return null
    
    return random.pick(componentArray)
```

### Detailed Implementation Steps

**Step 1: Pattern Parsing**

```text
Input:  "quality + material + base + enchantment"
Output: ["quality", "material", "base", "enchantment"]

Logic:
- Split by " + " delimiter (note: space + plus + space)
- Trim whitespace from each token
- Validate: no empty tokens
```

**Step 2: Token Resolution**

```text
For each token:
  1. Check if token == "base"
     → Yes: Pick random from items array
     → No: Continue to step 2
  
  2. Check if token exists in components object
     → Yes: Pick random from components[token] array
     → No: Continue to step 3
  
  3. Check if token starts with "@" (cross-file reference)
     → Yes: Load referenced file and resolve
     → No: Continue to step 4
  
  4. Token not found
     → Log warning: "Token 'xyz' not found"
     → Skip this token (graceful degradation)
     → Continue with remaining tokens
```

**Step 3: Name Assembly**

```text
Input:  ["Fine", "Steel", "Longsword", "of Slaying"]
Output: "Fine Steel Longsword of Slaying"

Logic:
- Join all parts with single space
- No special formatting needed
- Return final string
```

### Error Handling & Fallbacks

**Graceful Degradation:**

```text
Pattern: "invalid1 + quality + invalid2 + base"

Resolution:
- invalid1 → Not found, skip (log warning)
- quality  → "Fine"
- invalid2 → Not found, skip (log warning)
- base     → "Longsword"

Result: "Fine Longsword"
```

**Empty Components:**

```text
Pattern: "quality + material + base"
Components: { quality: [], material: ["Steel"], ... }

Resolution:
- quality → Empty array, skip
- material → "Steel"
- base → "Longsword"

Result: "Steel Longsword"
```

**All Tokens Invalid:**

```text
Pattern: "invalid1 + invalid2 + invalid3"

Resolution:
- All tokens fail to resolve
- parts array is empty
- Fallback: Return random item from items array

Result: "Longsword" (random base item)
```

### Performance Considerations

**Optimization Strategies:**

1. **Pattern Caching** - Pre-parse patterns into token arrays
2. **Component Caching** - Cache referenced file data
3. **Fast Path** - Special handling for "base" token
4. **Lazy Loading** - Only load referenced files when first needed

**Expected Performance:**

- Simple pattern (1-2 tokens): <0.1ms
- Complex pattern (5+ tokens): <0.5ms
- With cross-file references: <1ms (after caching)
- Target: 10,000 names per second on modern hardware

### Implementation in C #

For the complete C# implementation details, see:

- **#file:PATTERN_STANDARDIZATION_PLAN.md** - Phase 4: Runtime Implementation
- Includes: `PatternExecutor` class with full code
- Includes: Unit tests and integration tests
- Includes: Performance benchmarks

### ContentBuilder Pattern Validation

ContentBuilder should validate patterns in real-time using the same algorithm:

1. Parse pattern into tokens
2. Check each token exists in components
3. Show warnings for invalid tokens
4. Generate live examples to verify output
5. Display token count and suggested rarity

**For detailed ContentBuilder implementation, see:**

- **#file:PATTERN_STANDARDIZATION_PLAN.md** - Phase 3: Update ContentBuilder

---

## Metadata Auto-Generation Specification

### Overview

Metadata should be automatically generated by ContentBuilder on file save to eliminate manual maintenance errors and ensure consistency.

### User-Editable Fields

These fields are managed by the user in the ContentBuilder UI:

| Field | Type | Description | Example |
|-------|------|-------------|---------|
| `description` | string | Human-written explanation of file purpose | "Weapon name generation with pattern-based system" |
| `version` | string | Schema version (user increments on breaking changes) | "1.0", "2.0", "2.1" |

### Auto-Generated Fields

These fields are computed by ContentBuilder on every save:

| Field | Type | Description | Source |
|-------|------|-------------|--------|
| `lastUpdated` | string | Timestamp of last save (YYYY-MM-DD) | Current date |
| `componentKeys` | string[] | Array of component object keys | Extract from `components` object |
| `patternTokens` | string[] | Array of unique tokens used in patterns | Parse all `patterns` + add "base" |
| `totalPatterns` | number | Count of patterns | `patterns.length` |
| `total_items` | number | Count of items (if applicable) | `items.length` |
| `type` | string | File type classification | Inferred from structure |

### ContentBuilder UI Design

```text
┌─ Metadata ─────────────────────────────────────────────┐
│ Description: [Weapon name generation...             ] │ ← User editable
│ Version:     [2.0                                    ] │ ← User editable
│                                                         │
│ Auto-Generated (read-only):                            │
│   Last Updated:     2025-12-16                         │
│   Component Keys:   material, quality, descriptive,... │
│   Pattern Tokens:   base, material, quality, ...       │
│   Total Patterns:   11                                 │
│   Total Items:      59                                 │
│   File Type:        pattern_generation                 │
└─────────────────────────────────────────────────────────┘
```

### Auto-Generation Process

**Trigger:** On file save in ContentBuilder

**Process:**

1. Extract component keys from `components` object
2. Parse all patterns and extract unique tokens
3. Add "base" token to `patternTokens` if patterns exist
4. Count arrays and objects for statistics
5. Set `lastUpdated` to current UTC date (YYYY-MM-DD)
6. Preserve user-defined `description` and `version`
7. Generate complete metadata object
8. Save JSON with auto-generated metadata

**User Manual Edits:**

- ✅ **Allowed:** User can edit `description` and `version` in JSON
- ⚠️ **Warning:** All other metadata fields will be overwritten on next save in ContentBuilder
- 💡 **Recommendation:** Only edit metadata through ContentBuilder UI

### Implementation Details

For detailed implementation of metadata auto-generation, including:

- C# `MetadataGenerator` class
- ViewModel integration
- UI bindings
- Validation rules

See **#file:PATTERN_STANDARDIZATION_PLAN.md** - Section 3.1: Auto-Generated Metadata System

---

## Pattern Testing Guide

When testing a pattern in ContentBuilder:

1. **Verify tokens exist** - All tokens must match component keys or be `base`
2. **Check examples** - Generate 5-10 examples, ensure they make sense
3. **Test edge cases** - Empty components, missing tokens
4. **Validate output** - Names should be grammatically correct and logical
5. **Check variety** - Multiple examples should be different

---

## Quick Reference

### Document Sections

- **Executive Summary** - Current status and next phase (top of document)
- **Standard Component Keys** - Universal components usable across all categories
- **Pattern Syntax** - How to write patterns (token + token format)
- **Standard File Structure** - File types and when to use each
- **File Type Guide** - When to use types.json vs names.json vs prefixes.json
- **Category Standards** - Detailed breakdown for General, Items, Enemies, NPCs, Quests
- **Migration Checklist** - Complete list of all 113 files and their status
- **Cross-File References** - How to reference components from other files
- **Weight-Based Rarity** - How the emergent rarity system works
- **Pattern Testing Guide** - How to validate patterns work correctly

### File Type Quick Reference

| File Type | Extension | Purpose | Example |
|-----------|-----------|---------|---------|
| **Pattern Generation** | names.json | Procedural name generation | items/weapons/names.json |
| **Item Catalog** | types.json | Base items with stats | items/weapons/types.json |
| **Prefix Modifiers** | prefixes.json | Stat bonuses (before name) | items/weapons/prefixes.json |
| **Suffix Modifiers** | suffixes.json | Stat bonuses (after name) | items/enchantments/suffixes.json |
| **Component Library** | *.json | Reference data (no patterns) | general/adjectives.json |
| **Configuration** | *_config.json | Game rules and settings | general/rarity_config.json |

### Pattern Token Quick Reference

| Token | Resolves To | Example |
|-------|-------------|---------|
| `base` | Random item from types.json | "Longsword" |
| `material` | Material component | "Steel" |
| `quality` | Quality component | "Fine" |
| `descriptive` | Descriptive component | "Ancient" |
| `enchantment` | Enchantment component | "of Slaying" |
| `title` | Title component | "of the Dragon" |

### All 113 Files Status

✅ **100% Complete** - All files standardized with metadata, proper structure, and weight-based rarity

- ✅ General: 9 files
- ✅ Items: 17 files
- ✅ Enemies: 59 files (13 types: beasts, demons, dragons, elementals, goblinoids, humanoids, insects, orcs, plants, reptilians, trolls, undead, vampires)
- ✅ NPCs: 14 files
- ✅ Quests: 14 files

---

**End of Pattern System Component Standards v1.1**

**Good Pattern Examples:**

- ✅ `"material + base"` → "Iron Longsword", "Steel Axe", "Mithril Bow"
- ✅ `"quality + material + base"` → "Fine Iron Longsword", "Superior Steel Axe"

**Bad Pattern Examples:**

- ❌ `"base + material"` → "Longsword Iron" (backwards)
- ❌ `"mat + base"` → "[mat?] Longsword" (token doesn't exist)
- ❌ `"quality quality + base"` → "Fine Fine Longsword" (duplicate token)

---

## Reference: Component Key Quick Lookup

**Prefixes (before base):**

- `material`, `quality`, `descriptive`, `size`, `color`, `origin`, `condition`

**Suffixes (after base):**

- `enchantment`, `title`, `purpose`

**Special:**

- `base` / `item`

**Categories (organizational, not in patterns):**

- `weapon_types`, `armor_types`, `enemy_types`, `profession_types`, etc.
