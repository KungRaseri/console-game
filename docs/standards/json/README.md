# JSON Data Standards

**Purpose:** Comprehensive standards for all JSON data files in the game  
**Location:** `Game.Data/Data/Json/`  
**Last Updated:** December 27, 2025

---

## Quick Reference

### Core Standards
| Standard | Document | Purpose |
|----------|----------|---------|
| **Structure Types** | [JSON_STRUCTURE_TYPES.md](JSON_STRUCTURE_TYPES.md) | All 5 structure types (CATALOG, PATTERN_GENERATION, etc.) |
| **References** | [JSON_REFERENCE_STANDARDS.md](JSON_REFERENCE_STANDARDS.md) | Unified reference system for linking JSON files |

### Structure Types
| Structure Type | Standard Document | Purpose |
|----------------|-------------------|---------|
| CATALOG | [CATALOG_JSON_STANDARD.md](CATALOG_JSON_STANDARD.md) | Selectable entities (abilities, items, enemies) |
| PATTERN_GENERATION | [NAMES_JSON_STANDARD.md](NAMES_JSON_STANDARD.md) | Procedural name generation |
| COMPONENT_LIBRARY | [JSON_STRUCTURE_TYPES.md#3-component_library-structure](JSON_STRUCTURE_TYPES.md#3-component_library-structure) | Reusable data arrays (adjectives, colors) |
| CONFIG | [JSON_STRUCTURE_TYPES.md#4-config-structure](JSON_STRUCTURE_TYPES.md#4-config-structure) | Game rules and settings |
| HIERARCHICAL_CATALOG | [JSON_STRUCTURE_TYPES.md#5-hierarchical_catalog-structure](JSON_STRUCTURE_TYPES.md#5-hierarchical_catalog-structure) | Multi-category catalogs (NPCs, quests) |

### Special Files
| File Type | Standard Document | Purpose |
|-----------|------------------|---------|
| `.cbconfig.json` | [CBCONFIG_STANDARD.md](CBCONFIG_STANDARD.md) | ContentBuilder UI configuration |

---

## Structure Type Overview

The game uses **5 core JSON structure types**, each designed for specific data patterns:

### 1. CATALOG - Selectable Entities
**Purpose:** Game entities with stats and rarity-based selection  
**Examples:** abilities, enemies, items, weapons, armor  
**Key Feature:** `rarityWeight` for weighted random selection

### 2. PATTERN_GENERATION - Procedural Names
**Purpose:** Dynamic name generation from component patterns  
**Examples:** All `names.json` files  
**Key Feature:** `{token}` syntax and trait support

### 3. COMPONENT_LIBRARY - Data Arrays
**Purpose:** Simple reusable data collections  
**Examples:** adjectives, colors, sounds, verbs  
**Key Feature:** Categorized arrays without complex selection

### 4. CONFIG - Game Settings
**Purpose:** Rules, constants, and configuration values  
**Examples:** rarity_config, progression tables  
**Key Feature:** Key-value settings without rarityWeight

### 5. HIERARCHICAL_CATALOG - Multi-Category
**Purpose:** Complex entities with multiple logical groups  
**Examples:** NPCs (backgrounds + occupations), quests (templates + locations)  
**Key Feature:** Top-level categories with separate items arrays

**📘 See [JSON_STRUCTURE_TYPES.md](JSON_STRUCTURE_TYPES.md) for detailed schemas and examples**

---

## Legacy Documentation

### .cbconfig.json - UI Configuration

**What:** Configures folder display in ContentBuilder WPF application  
**Contains:** Icon, display name, description, sort order, file icons  
**Required:** Yes, for every data folder  
**Purpose:** Navigation and organization in editing tool

**Example:**
```json
{
  "icon": "Sword",
  "displayName": "Weapons",
  "sortOrder": 1,
  "fileIcons": {
    "names": "FormatListBulleted",
    "catalog": "ShapeOutline"
  }
}
```

---

### names.json - Pattern Generation

**What:** Procedural name generation through component patterns  
**Contains:** Metadata, components (with traits), patterns  
**Required:** For categories that need procedural naming  
**Purpose:** Generate "Fine Steel Longsword of Fire" from patterns

**Structure:**
```
names.json
├── metadata (version, componentKeys, patternTokens)
├── components
│   ├── prefix (Sharp, Ancient, Cursed)
│   ├── quality (Fine, Superior, Masterwork)
│   ├── descriptive (Blazing, Holy, Shadow)
│   └── suffix (of Fire, of Power, of the Dragon)
└── patterns
    ├── "{quality} {base}"
    ├── "{prefix} {material} {base}"
    └── "{prefix} {material} {base} {suffix}"
```

---

### catalog.json - Item/Enemy Catalog

**What:** Base item/enemy definitions that `{base}` token resolves to  
**Contains:** Metadata, *_types with traits + items array  
**Required:** For all item/enemy categories  
**Purpose:** Define "Longsword" stats that patterns reference

**Structure:**
```
catalog.json
├── metadata (version, counts)
└── weapon_types
    ├── swords
    │   ├── traits (damageType, slot, category)
    │   └── items
    │       ├── Longsword (damage, weight, value, rarityWeight)
    │       ├── Shortsword
    │       └── Greatsword
    └── axes
        ├── traits
        └── items
```

---

## How They Work Together

### The Generation Pipeline

1. **Pattern Selected** from `names.json` patterns array
   - Example: `"{quality} {material} {base}"`

2. **Components Selected** based on rarityWeight
   - quality → "Fine" (rarityWeight: 20)
   - material → "Steel" (rarityWeight: 25)

3. **Base Item Selected** from `catalog.json`
   - base → "Longsword" (damage: 1d8, rarityWeight: 5)

4. **Traits Merged** from components
   - Fine: durability +30, valueMultiplier 1.3x
   - Steel: durability +50, weight 1.0x

5. **Final Item Created**
   ```json
   {
     "name": "Fine Steel Longsword",
     "damage": "1d8",
     "damageType": "slashing",
     "durability": 150,
     "valueMultiplier": 1.3,
     "rarityWeight": 50
   }
   ```

---

## Standard File Locations

### Typical Category Structure

```
items/weapons/
├── .cbconfig.json       ← UI configuration
├── names.json           ← Pattern generation
└── catalog.json         ← Base weapon definitions

items/armor/
├── .cbconfig.json
├── names.json
└── catalog.json

enemies/trolls/
├── .cbconfig.json
├── names.json           ← Troll name patterns
├── catalog.json         ← Troll stat definitions
├── abilities_names.json ← Ability patterns (prefixed)
└── abilities_catalog.json ← Ability definitions
```

---

## Key Concepts

### Component Keys vs Pattern Tokens

**Component Keys:** Actual groups defined in components section
- Example: `["prefix", "quality", "suffix"]`
- Used for: Validation in ContentBuilder

**Pattern Tokens:** ALL tokens patterns can reference
- Example: `["base", "prefix", "material", "quality", "suffix"]`
- Includes: `base` (from catalog), `material` (external), components

### Type-Level vs Item-Level Properties

**Type-Level (traits):** Shared by ALL items in type
- Example: All swords have `damageType: "slashing"`

**Item-Level (items):** Unique to each item
- Example: Longsword `damage: "1d8"` vs Shortsword `damage: "1d6"`

### Weight-Based Rarity

**Selection Probability:**
```
Probability = 100 / rarityWeight
```

**Emergent Rarity:** Final item rarity = sum of component weights

---

## v4.0 Trait System

### What Are Traits?

Properties applied when components are selected:

```json
{
  "value": "Sharp",
  "rarityWeight": 30,
  "traits": {
    "damageBonus": {"value": 4, "type": "number"},
    "criticalMultiplier": {"value": 1.5, "type": "number"}
  }
}
```

### Trait Merging Rules

- **Numbers:** Take highest value
- **Strings:** Take last defined
- **Booleans:** Use OR (any true = true)

---

## Common Patterns

### Standard Weapon Category

**names.json:** Pattern generation
```json
{
  "componentKeys": ["prefix", "quality", "descriptive", "suffix"],
  "patternTokens": ["base", "prefix", "material", "quality", "descriptive", "suffix"],
  "patterns": [
    {"pattern": "base", "rarityWeight": 100},
    {"pattern": "{quality} {base}", "rarityWeight": 40},
    {"pattern": "{prefix} {material} {base} {suffix}", "rarityWeight": 3}
  ]
}
```

**catalog.json:** Base definitions
```json
{
  "weapon_types": {
    "swords": {
      "traits": {
        "damageType": "slashing",
        "slot": "mainhand"
      },
      "items": [
        {"name": "Longsword", "damage": "1d8", "rarityWeight": 5}
      ]
    }
  }
}
```

---

## Validation Checklist

### Before Committing JSON Changes:

**names.json:**
- [ ] `metadata.supportsTraits: true` for v4.0
- [ ] All componentKeys exist in components section
- [ ] Patterns use "rarityWeight" (not "weight")
- [ ] No "example" field in patterns
- [ ] Traits follow structure (value, type)
- [ ] No "base" or "material" in components section

**catalog.json:**
- [ ] Each type has traits and items array
- [ ] All items have name and rarityWeight
- [ ] Type traits are shared properties
- [ ] Item stats are individual properties
- [ ] rarityWeight is positive number

**.cbconfig.json:**
- [ ] Icon is valid MaterialDesign name (PascalCase)
- [ ] sortOrder is unique within parent folder
- [ ] File icon mappings use base filenames

**All Files:**
- [ ] Metadata at top with required fields
- [ ] lastUpdated in YYYY-MM-DD format
- [ ] Build successful (`dotnet build`)

---

## Migration Guides

### v3 to v4 (names.json)

1. Add `"supportsTraits": true` to metadata
2. Add `traits` object to all components
3. Change pattern "weight" → "rarityWeight"
4. Remove "example" field from patterns
5. Update version to "4.0"

### Adding New Category

1. Create folder in `Game.Data/Data/Json/`
2. Create `.cbconfig.json` with icon and sortOrder
3. Create `catalog.json` with base definitions
4. Create `names.json` with pattern generation
5. Test in ContentBuilder

---

## Resources

- **MaterialDesign Icons:** https://materialdesignicons.com/
- **Weight-Based Rarity System:** `../WEIGHT_BASED_RARITY_SYSTEM.md`
- **Pattern Component Standards:** `../PATTERN_COMPONENT_STANDARDS.md`

---

## Support

For questions about JSON standards:
1. Check the specific standard document (CBCONFIG, NAMES, CATALOG)
2. Review examples in existing JSON files
3. Test changes in ContentBuilder before committing
4. Run `dotnet build` to validate JSON parsing

---

## Document Index

- [.cbconfig.json Standard](CBCONFIG_STANDARD.md) - UI configuration
- [names.json Standard](NAMES_JSON_STANDARD.md) - Pattern generation
- [catalog.json Standard](CATALOG_JSON_STANDARD.md) - Item/enemy catalog
