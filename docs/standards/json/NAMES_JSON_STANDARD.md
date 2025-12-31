# names.json Standard (Pattern Generation)

**Version:** 4.0  
**Date:** December 27, 2025  
**Purpose:** Procedural name generation via component-based patterns

---

## Overview

The `names.json` file is a **pattern generation file** that procedurally creates names by combining components through patterns. It is the core of the v4.0 naming system with trait support.

**Key Concepts:**
- **Components** - Building blocks (prefix, quality, descriptive, suffix, etc.)
- **Patterns** - Templates that combine components (`{quality} {base}`)
- **Traits** - Properties applied when components are selected (v4.0+)
- **Weight-Based Rarity** - Components have rarityWeight determining selection probability
- **Emergent Rarity** - Final item rarity emerges from combined component weights

---

## File Location

**Naming Convention:** Always `names.json`

```
Game.Data/Data/Json/
├── items/
│   ├── weapons/
│   │   └── names.json          ← Weapon pattern generation
│   ├── armor/
│   │   └── names.json          ← Armor pattern generation
├── enemies/
│   ├── trolls/
│   │   └── names.json          ← Troll name patterns
│   │   └── abilities_names.json ← Troll ability patterns (prefixed)
```

**Naming Pattern:**
- Standard: `names.json`
- Prefixed for sub-systems: `abilities_names.json`, `dialogue_names.json`

---

## Standard Structure

```json
{
  "metadata": {
    "description": "Brief description of pattern generation purpose",
    "version": "4.0",
    "lastUpdated": "YYYY-MM-DD",
    "type": "pattern_generation",
    "supportsTraits": true,
    "componentKeys": ["array", "of", "component", "keys"],
    "patternTokens": ["base", "array", "of", "tokens"],
    "totalPatterns": 10,
    "raritySystem": "weight-based",
    "notes": ["Array", "of", "implementation", "notes"]
  },
  "components": {
    "component_name": [
      {
        "value": "Component Value",
        "rarityWeight": 10,
        "traits": {
          "traitName": {
            "value": 123,
            "type": "number"
          }
        }
      }
    ]
  },
  "patterns": [
    {
      "pattern": "{token1} {token2}",
      "rarityWeight": 50
    }
  ]
}
```

---

## Metadata Section

### Required Fields

| Field | Type | Description | Example |
|-------|------|-------------|---------|
| `description` | string | Purpose of this file | "Weapon name generation with traits" |
| `version` | string | Schema version | "4.0" |
| `lastUpdated` | string | ISO date (YYYY-MM-DD) | "2025-12-27" |
| `type` | string | Always "pattern_generation" | "pattern_generation" |
| `supportsTraits` | boolean | v4.0+ trait system | true |
| `componentKeys` | array | Component group names | ["prefix", "quality", "suffix"] |
| `patternTokens` | array | Valid tokens including base | ["base", "prefix", "quality"] |
| `totalPatterns` | number | Count of patterns | 18 |
| `raritySystem` | string | Rarity calculation method | "weight-based" |

### Optional Fields

| Field | Type | Description |
|-------|------|-------------|
| `notes` | array | Implementation notes/docs |
| `usage` | string | Runtime usage instructions |
| Custom fields | any | Domain-specific metadata |

### Example Metadata

```json
{
  "metadata": {
    "description": "Unified weapon naming system with prefix/suffix support and trait assignment (v4.0)",
    "version": "4.0",
    "lastUpdated": "2025-12-27",
    "type": "pattern_generation",
    "supportsTraits": true,
    "componentKeys": [
      "prefix",
      "quality",
      "descriptive",
      "suffix"
    ],
    "patternTokens": [
      "base",
      "prefix",
      "material",
      "quality",
      "descriptive",
      "suffix"
    ],
    "totalPatterns": 18,
    "raritySystem": "weight-based",
    "notes": [
      "Base token resolves from items/weapons/catalog.json",
      "References use v4.1 syntax: @items/materials/metals:steel (not old [@materialRef] syntax)",
      "Traits are applied when components are selected in patterns",
      "Trait merging: numbers take highest, strings take last, booleans use OR",
      "Emergent rarity calculated from combined component weights"
    ]
  }
}
```

---

## Components Section

### Structure

Components are organized by semantic group (prefix, quality, descriptive, etc.). Each component has:

```json
{
  "components": {
    "component_key": [
      {
        "value": "Display text",
        "rarityWeight": 10,
        "traits": {
          "traitName": {
            "value": 123,
            "type": "number|string|boolean"
          }
        }
      }
    ]
  }
}
```

### Component Object Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `value` | string | ✅ Yes | Display text (e.g., "Sharp", "Ancient") |
| `rarityWeight` | number | ✅ Yes | Selection probability (higher = rarer) |
| `traits` | object | ⭐ v4.0+ | Properties applied to generated item |

### Example Components

```json
{
  "components": {
    "prefix": [
      {
        "value": "Rusty",
        "rarityWeight": 50,
        "traits": {
          "durability": {
            "value": 50,
            "type": "number"
          },
          "damageMultiplier": {
            "value": 0.8,
            "type": "number"
          }
        }
      },
      {
        "value": "Sharp",
        "rarityWeight": 30,
        "traits": {
          "damageBonus": {
            "value": 4,
            "type": "number"
          },
          "criticalMultiplier": {
            "value": 1.5,
            "type": "number"
          }
        }
      }
    ],
    "quality": [
      {
        "value": "Fine",
        "rarityWeight": 20,
        "traits": {
          "durability": {
            "value": 130,
            "type": "number"
          },
          "valueMultiplier": {
            "value": 1.3,
            "type": "number"
          }
        }
      }
    ],
    "suffix": [
      {
        "value": "of Fire",
        "rarityWeight": 40,
        "traits": {
          "fireDamage": {
            "value": 10,
            "type": "number"
          },
          "glowing": {
            "value": true,
            "type": "boolean"
          },
          "element": {
            "value": "fire",
            "type": "string"
          }
        }
      }
    ]
  }
}
```

---

## Traits System (v4.0+)

### Trait Structure

Each trait is an object with:
```json
{
  "traitName": {
    "value": 123,
    "type": "number|string|boolean"
  }
}
```

### Trait Types

| Type | Description | Example |
|------|-------------|---------|
| `number` | Numeric values | `{"damage": {"value": 15, "type": "number"}}` |
| `string` | Text values | `{"element": {"value": "fire", "type": "string"}}` |
| `boolean` | True/false flags | `{"glowing": {"value": true, "type": "boolean"}}` |

### Trait Merging Rules

When multiple components provide the same trait:

**Numbers:** Take the **highest** value
```json
// prefix: {"damage": 10}, suffix: {"damage": 15}
// Result: {"damage": 15}
```

**Strings:** Take the **last** defined value
```json
// prefix: {"element": "fire"}, suffix: {"element": "ice"}
// Result: {"element": "ice"}
```

**Booleans:** Use **OR** logic (any true = true)
```json
// prefix: {"glowing": false}, suffix: {"glowing": true}
// Result: {"glowing": true}
```

### Common Trait Names

**Weapons:**
- `damageBonus`, `damageMultiplier`, `criticalMultiplier`
- `durability`, `repairCost`
- `element`, `damageType`
- `glowing`, `cursed`, `blessed`

**Armor:**
- `armorBonus`, `armorMultiplier`
- `resistFire`, `resistCold`, `resistAll`
- `weightMultiplier`, `dodgeBonus`
- `selfRepair`, `indestructible`

**Enemies:**
- `healthMultiplier`, `attackBonus`
- `speed`, `intelligence`
- `regeneration`, `vulnerability`

---

## Patterns Section

### Structure

Patterns are templates that combine tokens:

```json
{
  "patterns": [
    {
      "pattern": "{token1} {token2} {token3}",
      "rarityWeight": 50
    }
  ]
}
```

### Pattern Object Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `pattern` | string | ✅ Yes | Token template | |
| `rarityWeight` | number | ✅ Yes | Selection probability |

### Token Syntax

**Component Tokens:** Resolve from `components` section (use curly braces)
```
{prefix}       → "Sharp"
{quality}      → "Fine"
{descriptive}  → "Ancient"
{suffix}       → "of Fire"
```

**Base Token:** Resolves from `catalog.json` (use curly braces)
```
{base}         → "Longsword" (from weapons/catalog.json)
```

**External References (v4.1 Syntax):** Pull from other catalog files using reference syntax
```
@items/materials/metals:steel  → "Steel" (from materials/catalog.json)
@items/materials/metals:*      → Random metal (rarityWeight-based selection)
@abilities/active/offensive:*  → Random offensive ability
```

**Reference Syntax Rules:**
- **Internal components** use `{token}` - defined in same file's components section
- **External references** use `@domain/path/category:item-name` - pull from other catalog files
- References use `@` prefix with domain/path navigation
- Use `:*` for random selection based on rarityWeight
- Use `:item-name` to reference specific items
- Optional `?` suffix for nullable references (returns null instead of error)

### Pattern Examples

```json
{
  "patterns": [
    {
      "pattern": "{base}",
      "rarityWeight": 100
    },
    {
      "pattern": "{quality} {base}",
      "rarityWeight": 40
    },
    {
      "pattern": "@items/materials/metals:* {base}",
      "rarityWeight": 50
    },
    {
      "pattern": "{prefix} {base}",
      "rarityWeight": 30
    },
    {
      "pattern": "{descriptive} {base}",
      "rarityWeight": 35
    },
    {
      "pattern": "{base} {suffix}",
      "rarityWeight": 30
    },
    {
      "pattern": "{quality} @items/materials/metals:* {base}",
      "rarityWeight": 15
    },
    {
      "pattern": "{prefix} @items/materials/metals:* {base} {suffix}",
      "rarityWeight": 3
    }
  ]
}
```

**Generated Names:**
- "Longsword" (base only)
- "Fine Longsword" (quality + base)
- "Steel Longsword" (material reference + base)
- "Sharp Longsword" (prefix + base)
- "Ancient Longsword" (descriptive + base)
- "Longsword of Fire" (base + suffix)
- "Fine Steel Longsword" (quality + material + base)
- "Sharp Steel Longsword of Fire" (prefix + material + base + suffix)

---

## v4.1 Reference System in Patterns

### Using References in Pattern Generation

Patterns can include v4.1 references to pull data from other catalog files at generation time.

**Syntax:** `@domain/path/category:item-name[filters]?.property`

### Reference Use Cases

**Material References:**
```json
{
  "pattern": "@items/materials/metals:* {base}",
  "rarityWeight": 50
}
```
Generates: "Steel Longsword", "Iron Dagger", "Mithril Axe"

**Ability Injection:**
```json
{
  "pattern": "{base} of @abilities/active/offensive:*.name",
  "rarityWeight": 30
}
```
Generates: "Sword of Fireball", "Staff of Lightning Bolt"

**Enemy Variant Generation:**
```json
{
  "pattern": "@world/regions:* {base}",
  "rarityWeight": 40
}
```
Generates: "Mountain Troll", "Swamp Troll", "Forest Troll"

### Reference Filters in Patterns

**Filter by Property:**
```json
{
  "pattern": "@items/materials/metals:*[rarityWeight>=50] {base}",
  "rarityWeight": 20
}
```
Only selects rare metals (rarityWeight ≥ 50)

**Filter by Type:**
```json
{
  "pattern": "@items/materials:*[type=metal,weight<10] {base}",
  "rarityWeight": 30
}
```
Only lightweight metals

### Benefits

✅ **Dynamic Content** - Materials/abilities change without updating patterns  
✅ **Consistency** - Same material properties across all items  
✅ **Filtering** - Target specific subsets (rare, heavy, magical)  
✅ **Cross-Domain** - Pull from any catalog (items, abilities, world, etc.)

### Reference Documentation

For complete reference system documentation, see:  
**[docs/standards/json/JSON_REFERENCE_STANDARDS.md](JSON_REFERENCE_STANDARDS.md)**

---

## Component Keys vs Pattern Tokens

### Component Keys

**Definition:** Actual component groups defined in `components` section

**Purpose:** Validation - ContentBuilder checks patterns use valid keys

**Listed in:** `metadata.componentKeys`

**Example:**
```json
{
  "componentKeys": ["prefix", "quality", "descriptive", "suffix"],
  "components": {
    "prefix": [...],
    "quality": [...],
    "descriptive": [...],
    "suffix": [...]
  }
}
```

### Pattern Tokens

**Definition:** All tokens that can appear in patterns (includes base + external refs)

**Purpose:** Documentation - shows ALL valid tokens for pattern creation

**Listed in:** `metadata.patternTokens`

**Example:**
```json
{
  "patternTokens": ["base", "prefix", "quality", "descriptive", "suffix"],
  "patterns": [
    {"pattern": "{base}", "rarityWeight": 100},
    {"pattern": "{quality} {base}", "rarityWeight": 40},
    {"pattern": "@items/materials/metals:* {base}", "rarityWeight": 50}
  ]
}
```

**Note:** `patternTokens` includes `base` but NOT external references like `@items/materials/metals:*` since those are dynamic lookups, not fixed tokens.

**Key Difference:**
- `componentKeys` = ONLY groups in components section
- `patternTokens` = ALL tokens patterns can reference (includes `base` from catalog)
- External references (v4.1 `@domain/path:item`) are NOT listed in either - they're runtime lookups

---

## Enhancement System Fields (v4.2+)

### materialRef Field

**Purpose:** Link patterns to material catalog for procedural material selection during generation

**Type:** v4.1 reference string

**Location:** Pattern object (alongside `pattern` and `rarityWeight`)

**Syntax:**
```json
{
  "pattern": "{material} {base}",
  "rarityWeight": 50,
  "materialRef": "@items/materials/metals:*[itemTypeTraits.weapon EXISTS]"
}
```

**Examples:**
```json
// Random metal with weapon traits
"materialRef": "@items/materials/metals:*[itemTypeTraits.weapon EXISTS]"

// Random leather with armor traits
"materialRef": "@items/materials/leathers:*[itemTypeTraits.armor EXISTS]"

// Specific material
"materialRef": "@items/materials/metals:steel"

// Any material (no filter)
"materialRef": "@items/materials:*"

// Optional material (can be null)
"materialRef": "@items/materials/metals:*[rarityWeight<=50]?"
```

**Usage:**
1. Generator selects pattern
2. If pattern has `materialRef`, resolve reference
3. Material selected by rarityWeight from filtered pool
4. Material traits applied to item
5. Material name used in `{material}` token

**Notes:**
- Only applies to item names.json files (weapons, armor, etc.)
- Materials from `items/materials/catalog.json`
- Supports all v4.1 reference filters
- `{material}` token in pattern is replaced with material name

---

### enchantmentSlots Field

**Purpose:** Define enchantment positions and references for items during generation

**Type:** Array of enchantment slot objects

**Location:** Pattern object

**Syntax:**
```json
{
  "pattern": "{enchantment_prefix} {material} {base} {enchantment_suffix}",
  "rarityWeight": 10,
  "materialRef": "@items/materials/metals:*",
  "enchantmentSlots": [
    {
      "type": "prefix",
      "ref": "@items/enchantments:*[position=prefix]",
      "rarityWeightMax": 80
    },
    {
      "type": "suffix",
      "ref": "@items/enchantments:*[position=suffix]",
      "rarityWeightMax": 100
    }
  ]
}
```

**Enchantment Slot Object:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `type` | string | ✅ Yes | "prefix" or "suffix" |
| `ref` | string | ✅ Yes | v4.1 reference to enchantments |
| `rarityWeightMax` | number | ⭐ Optional | Max rarityWeight for enchantment selection |

**Slot Types:**
- **prefix** - Adjective form, placed before material+base: "{enchantment_prefix} {material} {base}"
- **suffix** - Noun phrase form, placed after base: "{material} {base} {enchantment_suffix}"

**Examples:**

**Single Prefix:**
```json
{
  "pattern": "{enchantment_prefix} {material} {base}",
  "rarityWeight": 30,
  "enchantmentSlots": [
    {
      "type": "prefix",
      "ref": "@items/enchantments:*[position=prefix]",
      "rarityWeightMax": 100
    }
  ]
}
```
Generates: "Flaming Steel Sword", "Frozen Iron Axe"

**Single Suffix:**
```json
{
  "pattern": "{material} {base} {enchantment_suffix}",
  "rarityWeight": 25,
  "enchantmentSlots": [
    {
      "type": "suffix",
      "ref": "@items/enchantments:*[position=suffix]",
      "rarityWeightMax": 150
    }
  ]
}
```
Generates: "Steel Sword of Fire", "Iron Axe of Strength"

**Both Prefix and Suffix:**
```json
{
  "pattern": "{enchantment_prefix} {material} {base} {enchantment_suffix}",
  "rarityWeight": 10,
  "enchantmentSlots": [
    {
      "type": "prefix",
      "ref": "@items/enchantments:*[position=prefix][rarityWeight<=80]"
    },
    {
      "type": "suffix",
      "ref": "@items/enchantments:*[position=suffix][rarityWeight<=100]"
    }
  ]
}
```
Generates: "Flaming Steel Sword of Strength", "Frozen Mithril Axe of the Bear"

**Usage:**
1. Generator selects pattern with enchantmentSlots
2. For each slot, resolve enchantment reference
3. Generate enchantment name from enchantments/names.json
4. Apply enchantment traits to item
5. Replace {enchantment_prefix} or {enchantment_suffix} tokens

**Notes:**
- Enchantments from `items/enchantments/names.json`
- Each slot can filter by position and rarityWeight
- Multiple slots increase item rarity significantly
- Empty array = no enchantments

---

### gemSocketCount Field

**Purpose:** Define gem socket range for generated items

**Type:** Object with min/max properties

**Location:** Pattern object (optional)

**Syntax:**
```json
{
  "pattern": "{material} {base}",
  "rarityWeight": 50,
  "gemSocketCount": {
    "min": 1,
    "max": 3
  }
}
```

**GemSocketCount Object:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `min` | number | ✅ Yes | Minimum socket count (0+) |
| `max` | number | ✅ Yes | Maximum socket count |

**Examples:**

**No Sockets:**
```json
{
  "pattern": "{base}",
  "rarityWeight": 100,
  "gemSocketCount": {
    "min": 0,
    "max": 0
  }
}
```

**Fixed Socket Count:**
```json
{
  "pattern": "{material} {base}",
  "rarityWeight": 50,
  "gemSocketCount": {
    "min": 2,
    "max": 2
  }
}
```

**Variable Sockets:**
```json
{
  "pattern": "{enchantment_prefix} {material} {base}",
  "rarityWeight": 30,
  "gemSocketCount": {
    "min": 1,
    "max": 4
  }
}
```

**Notes:**
- If omitted, socket count calculated from total rarityWeight
- Explicit `gemSocketCount` overrides automatic calculation
- More sockets contribute to rarityWeight: `socketCount² × 5`
- Socket colors determined by item type (weapons=red/blue, armor=green/yellow)

---

### Pattern Example with All Enhancement Fields

```json
{
  "pattern": "{enchantment_prefix} {material} {base} {enchantment_suffix}",
  "rarityWeight": 10,
  "materialRef": "@items/materials/metals:*[rarityWeight>=20][itemTypeTraits.weapon EXISTS]",
  "enchantmentSlots": [
    {
      "type": "prefix",
      "ref": "@items/enchantments:*[position=prefix][rarityWeight<=100]",
      "rarityWeightMax": 100
    },
    {
      "type": "suffix",
      "ref": "@items/enchantments:*[position=suffix][element_suffix EXISTS]",
      "rarityWeightMax": 150
    }
  ],
  "gemSocketCount": {
    "min": 2,
    "max": 4
  }
}
```

**Generation Flow:**
1. Pattern selected (rarityWeight: 10)
2. Material: "Steel" from metals with weapon traits (rarityWeight: 15)
3. Base: "Longsword" from catalog (rarityWeight: 25)
4. Prefix enchantment: "Flaming" (rarityWeight: 25)
5. Suffix enchantment: "of Fire Strength" (rarityWeight: 55)
6. Socket count: 3 (random between 2-4)
7. Total rarityWeight: 10 + 15 + 25 + 25 + 55 + 45 = 175 → RARE
8. Final name: "Flaming Steel Longsword of Fire Strength"
9. Sockets: 3 empty sockets (2 red, 1 blue based on weapon type)

---

## Weight-Based Rarity System

### Selection Probability Formula

```
Probability = 100 / rarityWeight
```

| rarityWeight | Probability | Rarity Tier |
|--------------|-------------|-------------|
| 5 | 20.0% | Very Common |
| 10 | 10.0% | Common |
| 20 | 5.0% | Uncommon |
| 50 | 2.0% | Rare |
| 100 | 1.0% | Very Rare |
| 200 | 0.5% | Epic |

### Emergent Rarity

Final item rarity = sum of component weights:

**Example:** "Fine Steel Longsword of Fire"
```
quality:  "Fine" (rarityWeight 20)  → 20
material: "Steel" (rarityWeight 25) → 25
base:     "Longsword" (rarityWeight 5) → 5
suffix:   "of Fire" (rarityWeight 40) → 40
                                      ────
Total Weight: 90 → Rare tier
```

---

## Standard Component Keys

### Universal Components

These work across ALL domains (items, enemies):

| Key | Description | Examples |
|-----|-------------|----------|
| `prefix` | Leading modifier | Rusty, Sharp, Ancient, Cursed |
| `quality` | Craftsmanship level | Fine, Superior, Masterwork, Flawless |
| `descriptive` | Special attributes | Blazing, Holy, Shadow, Frozen |
| `suffix` | Trailing modifier | of Fire, of Power, of the Dragon |
| `material` | Physical material | Steel, Mithril, Dragonscale |
| `size` | Size modifier | Small, Large, Gigantic |
| `color` | Color descriptor | Red, Golden, Dark |
| `origin` | Creator/source | Elven, Dwarven, Demonic |

### Domain-Specific Components

**Enemies:**
- `age` - Young, Elder, Ancient
- `habitat` - Cave, Mountain, Swamp
- `behavior` - Savage, Cunning, Frenzied

---

## Validation Rules

### ✅ Component Validation

1. **All componentKeys must exist** in components section
2. **All components must have** `value` and `rarityWeight`
3. **Traits must follow** trait structure (value, type)
4. **rarityWeight must be** positive number
5. **Trait types** must be "number", "string", or "boolean"

### ✅ Pattern Validation

1. **Pattern tokens** must match `{tokenName}` format
2. **All tokens** must be in `patternTokens` array
3. **rarityWeight** must be positive number
4. **base token** resolves from catalog.json (not in components)
5. **material token** resolves from materials/catalog.json (optional)

---

## Common Mistakes

### ❌ Don't Include Base in Components

**WRONG:**
```json
{
  "components": {
    "base": [
      {"value": "Longsword", "rarityWeight": 5}
    ]
  }
}
```

**RIGHT:**
- `base` resolves from `catalog.json`
- Not a component - it's the foundation

### ❌ Don't Include Material in Components

**WRONG:**
```json
{
  "components": {
    "material": [
      {"value": "Steel", "rarityWeight": 10}
    ]
  }
}
```

**RIGHT:**
- `material` resolves from `materials/catalog.json` via references
- Use v4.1 reference syntax in patterns: `@items/materials/metals:*` or `@items/materials/metals:steel`

### ❌ Don't Use "weight" Instead of "rarityWeight"

**WRONG:**
```json
{
  "pattern": "{prefix} {base}",
  "weight": 50
}
```

**RIGHT:**
```json
{
  "pattern": "{prefix} {base}",
  "rarityWeight": 50
}
```

### ❌ Don't Add "example" Field

**WRONG:**
```json
{
  "pattern": "{prefix} {base}",
  "rarityWeight": 50,
  "example": "Sharp Longsword"
}
```

**RIGHT:**
```json
{
  "pattern": "{prefix} {base}",
  "rarityWeight": 50
}
```

---

## Best Practices

### ✅ DO:

1. **Update lastUpdated** when modifying file
2. **Use semantic component keys** (prefix, quality, not prefix_mod)
3. **Provide trait values** for v4.0+ files
4. **Document special tokens** in notes (base, material)
5. **Balance rarityWeight** - common items 5-20, rare 50+
6. **Order patterns** from simple to complex
7. **Test pattern generation** in ContentBuilder

### ❌ DON'T:

1. **Don't hardcode rarity tiers** - let it emerge from weights
2. **Don't skip metadata fields** - they're required for tooling
3. **Don't mix v3 and v4** structures
4. **Don't duplicate component keys** - use one prefix group, not multiple
5. **Don't forget supportsTraits: true** for v4.0 files

---

## Migration from v3 to v4

### Changes in v4.0

1. **Added:** `supportsTraits: true` in metadata
2. **Added:** `traits` object to components
3. **Removed:** Hardcoded rarity tiers
4. **Changed:** "weight" → "rarityWeight"
5. **Removed:** "example" field from patterns

### Migration Checklist

- [ ] Add `supportsTraits: true` to metadata
- [ ] Add `traits` object to all components
- [ ] Change "weight" to "rarityWeight" in patterns
- [ ] Remove "example" field from patterns
- [ ] Update version to "4.0"
- [ ] Update lastUpdated date
- [ ] Test pattern generation

---

## Related Standards

- **catalog.json Standard** - Base item definitions that `{base}` resolves to
- **CBCONFIG Standard** - Folder configuration for ContentBuilder
- **Weight-Based Rarity System** - Detailed rarity calculation formulas

---

## Change Log

| Version | Date | Changes |
|---------|------|---------|
| 4.2 | 2025-12-31 | Added materialRef, enchantmentSlots, gemSocketCount for hybrid enhancement system |
| 4.0 | 2025-12-27 | Added trait system, removed hardcoded rarity |
| 3.0 | 2025-12-16 | Consolidated naming system |
| 2.0 | 2025-12-15 | Added weight-based rarity |
| 1.0 | 2025-12-10 | Initial pattern system |
