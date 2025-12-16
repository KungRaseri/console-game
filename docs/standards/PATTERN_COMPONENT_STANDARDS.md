# Pattern System Component Standards

**Date:** December 16, 2025  
**Version:** 1.0  
**Status:** 📋 Definition Phase

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
| `base` | items array | The core item/name from the items list |
| `item` | items array | Alias for `base` |

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
    "last_updated": "2025-12-16"
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
    "component_keys": ["component_key1", "component_key2", "component_key3"],
    "pattern_tokens": ["base", "component_key1", "component_key2", "component_key3"]
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
    "component_keys": ["material", "quality", "enchantment"],
    "pattern_tokens": ["base", "material", "quality", "enchantment"]
  }
}
```

---

#### 3. prefixes.json / suffixes.json - Stat Modifiers

**Purpose:** Item modifiers with stat bonuses/penalties

**Structure:**

```json
{
  "items": [
    {
      "name": "internal_name",
      "displayName": "Display Name",
      "traits": {
        "stat1": 5,
        "stat2": "value",
        "stat3": true
      }
    }
  ],
  "metadata": {
    "description": "Prefix/suffix modifiers",
    "version": "1.0"
  }
}
```

**Example (weapons/prefixes.json):**

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
    }
  ],
  "metadata": {
    "description": "Weapon prefix modifiers",
    "version": "1.0"
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

- `last_updated` - Timestamp of last save (YYYY-MM-DD format)
- `component_keys` - Array extracted from `components` object keys
- `pattern_tokens` - Array extracted from `patterns` + "base" token
- `total_patterns` - Count of patterns array length
- `total_items` - Count of items array length (if applicable)
- `[category]_count` - Count of category types (e.g., `weapon_types: 7`)

#### Optional Auto-Generated Fields

- `type` - File type hint ("reference_data", "catalog", "generation") - inferred from structure
- Custom statistics as needed

#### ContentBuilder Implementation

**On Save:**
1. Extract component keys from `components` object
2. Parse all patterns and extract unique tokens
3. Add "base" token to pattern_tokens if patterns exist
4. Count arrays and objects for statistics
5. Set `last_updated` to current date
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
  "last_updated": "2025-12-16",
  "component_keys": ["material", "quality", "descriptive", "enchantment", "title"],
  "pattern_tokens": ["base", "material", "quality", "descriptive", "enchantment", "title"],
  "total_patterns": 11,
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
    "component_keys": ["material", "quality", "descriptive", "enchantment", "title"],
    "pattern_tokens": ["base", "material", "quality", "descriptive", "enchantment", "title"]
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

**Pattern-Based Name Generation Flow:**

```
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

### 1. General Category

#### Colors (`general/colors.json`)

**Current Structure:** Mixed (has items, components, AND patterns)  
**Action Required:** ⚠️ NEEDS REVIEW - Inconsistent structure

**Current Issues:**
1. ❌ Pattern "material (gemstone/metal colors)" has comments - not parseable!
2. ❌ Token mismatch - Pattern uses `"base_color"` but component is `"base_colors"` (plural)
3. ❌ Unclear if this is reference data or name generation

**Decision Needed:** Is this file for:
- **Option A:** Reference data (colors used by other files)
- **Option B:** Color name generation (procedural color names)

**Recommended Structure (Option A - Reference Data):**

```json
{
  "components": {
    "base_colors": ["red", "blue", "green", "yellow", ...],
    "modifiers": ["dark", "light", "bright", "pale", ...],
    "materials": ["crimson", "scarlet", "azure", "emerald", ...]
  },
  "metadata": {
    "description": "Color components for use in other files",
    "version": "1.0",
    "type": "reference_data"
  }
}
```

**Status:** ⏳ Needs standardization

#### Adjectives (`general/adjectives.json`)

**Current Structure:** Categorized lists (no standard wrapper)  
**Action Required:** ⚠️ NEEDS STANDARDIZATION

**Current Issues:**
1. ❌ Missing standard structure (no `components` wrapper, no `metadata`)
2. ✅ IS reference data - these ARE components used by other files

**Recommended Structure:**

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
    "description": "Adjective components for use in other files",
    "version": "1.0",
    "type": "reference_data"
  }
}
```

**Status:** ⏳ Needs standardization

#### Materials (`general/materials.json`)

**Current Structure:** Unknown (needs review)  
**Action Required:** 📋 To be reviewed

**Expected:** Reference data (components for other files)

**Recommended Structure:**

```json
{
  "components": {
    "metals": ["Iron", "Steel", "Gold", "Silver", ...],
    "precious": ["Diamond", "Ruby", "Emerald", "Sapphire", ...],
    "natural": ["Wood", "Stone", "Leather", "Bone", ...]
  },
  "metadata": {
    "description": "Material components for use in other files",
    "version": "1.0",
    "type": "reference_data"
  }
}
```

**Status:** ⏳ Needs review and standardization

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

#### Weapons - Prefixes (`items/weapons/prefixes.json`)

**Current Structure:** ItemPrefix (prefix/suffix pairs with traits)  
**Action Required:** ⏳ Review structure

**Expected Structure:**

```json
{
  "items": [
    {
      "name": "flaming",
      "displayName": "Flaming",
      "traits": {
        "bonusDamage": 5,
        "damageType": "fire"
      }
    }
  ]
}
```

**Status:** ⏳ Needs review - check if this should have patterns or stay as ItemPrefix

#### Armor - Materials (`items/armor/materials.json`)

**Current Structure:** Unknown  
**Action Required:** 📋 To be reviewed

**Proposed Component Keys:**

- `material` - Armor materials (Cloth, Leather, Chainmail, Plate)
- `quality` - Craftsmanship levels
- `descriptive` - Special attributes
- `enchantment` - Magical properties
- `armor_types` - Category organization (helmets, chest, legs)

**Status:** 📋 Awaiting review

#### Enchantments - Suffixes (`items/enchantments/suffixes.json`)

**Current Structure:** Unknown  
**Action Required:** 📋 To be reviewed

**Expected Structure:** ItemSuffix (similar to prefixes)

**Status:** 📋 Awaiting review

#### Materials (`items/materials/*.json`)

Files: `metals.json`, `leathers.json`, `woods.json`, `gemstones.json`

**Current Structure:** Unknown  
**Action Required:** 📋 To be reviewed

**Expected:** These are likely FlatItem or NameList (component sources, not generated)

**Status:** 📋 Awaiting review

---

### 3. Enemies Category

#### Beasts - Names (`enemies/beasts/names.json`)

**Current Structure:** Unknown  
**Action Required:** 📋 To be reviewed

**Proposed Component Keys:**

- `size` - Size descriptors (Giant, Dire, Alpha)
- `color` - Color variants (Black, White, Red)
- `descriptive` - Special attributes (Ancient, Rabid, Enraged)
- `origin` - Regional origin (Mountain, Forest, Desert)
- `title` - Named beasts (of the Night, of the Wild)
- `beast_types` - Category organization (wolves, bears, cats)

**Proposed Patterns:**

```json
[
  "base",
  "size + base",
  "color + base",
  "descriptive + base",
  "size + descriptive + base",
  "base + title"
]
```

**Status:** 📋 Awaiting review

#### Undead - Names (`enemies/undead/names.json`)

**Current Structure:** Unknown  
**Action Required:** 📋 To be reviewed

**Proposed Component Keys:**

- `descriptive` - Undead attributes (Risen, Cursed, Ancient)
- `origin` - Former identity (Warrior, Mage, King)
- `title` - Named undead (of the Crypt, of Darkness)
- `undead_types` - Category organization (skeleton, zombie, ghost)

**Status:** 📋 Awaiting review

#### Demons - Names (`enemies/demons/names.json`)

**Current Structure:** Unknown  
**Action Required:** 📋 To be reviewed

**Status:** 📋 Awaiting review

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

### Phase 1: Review Current State ⏳

- [x] ✅ General/Colors - No changes needed
- [x] ✅ General/Adjectives - No changes needed
- [x] ✅ General/Materials - No changes needed
- [x] ✅ Items/Weapons/Names - Standardized
- [ ] 📋 Items/Weapons/Prefixes - Review structure
- [ ] 📋 Items/Armor/Materials - Review structure
- [ ] 📋 Items/Enchantments/Suffixes - Review structure
- [ ] 📋 Items/Materials/* - Review structure
- [ ] 📋 Enemies/Beasts/Names - Review structure
- [ ] 📋 Enemies/Undead/Names - Review structure
- [ ] 📋 Enemies/Demons/Names - Review structure
- [ ] 📋 Enemies/Elementals/Names - Review structure
- [ ] 📋 Enemies/Dragons/Names - Review structure
- [ ] 📋 Enemies/Dragons/Colors - Review structure
- [ ] 📋 Enemies/Humanoids/Names - Review structure
- [ ] 📋 Enemies/*/Prefixes - Review structure
- [ ] 📋 NPCs/Names/FirstNames - Review structure
- [ ] 📋 NPCs/Occupations - Review structure
- [ ] 📋 NPCs/Dialogue/Templates - Review structure
- [ ] 📋 NPCs/Dialogue/Traits - Review structure
- [ ] 📋 NPCs/Titles - Review structure
- [ ] 📋 Quests/Templates - Review structure

### Phase 2: Standardize Files

Will be populated as we review each file.

### Phase 3: Update ContentBuilder

- [ ] Update PatternExampleGenerator (if needed)
- [ ] Add pattern validation
- [ ] Add live example preview
- [ ] Test all standardized files

### Phase 4: Runtime Implementation

- [ ] Create PatternExecutor service
- [ ] Update data models
- [ ] Update generators
- [ ] Write tests

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

---

## Pattern Testing Guide

When testing a pattern in ContentBuilder:

1. **Verify tokens exist** - All tokens must match component keys or be `base`
2. **Check examples** - Generate 5-10 examples, ensure they make sense
3. **Test edge cases** - Empty components, missing tokens
4. **Validate output** - Names should be grammatically correct and logical
5. **Check variety** - Multiple examples should be different

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
