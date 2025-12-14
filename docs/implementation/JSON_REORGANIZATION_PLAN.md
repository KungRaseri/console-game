# JSON Data Reorganization Plan

**Project**: Game.Shared Data Structure Refactoring  
**Date**: December 14, 2025  
**Status**: ✅ **COMPLETE**  
**Goal**: Organize 26 JSON files into logical folder hierarchy

---

## 📊 **Completion Summary**

### Final Statistics
- **Files Moved**: 28 original files → ~100 files in new structure
- **Folders Created**: 31 hierarchical folders
- **Placeholders Added**: 70+ files for future expansion
- **Code Updated**: GameDataService.cs - all 28 LoadJson() calls updated
- **Build Status**: ✅ Successful (all 4 projects)
- **Test Status**: ✅ 1560/1573 passing (99.2%)
- **Git Commit**: c4a6e7b (reorganization), 3889a31 (test fix)

### Issues Resolved
1. ✅ Test data path updated (Game.Shared vs Game/Shared)
2. ✅ Enemy trait tests fixed (32 failures → 0 failures)
3. ✅ Quest templates split into 5 type-specific files
4. ✅ NPC occupations split into 4 category files

### Remaining Work
- 13 pre-existing CharacterViewServiceTests failures (console input mocking - documented)
- ContentBuilder needs path updates
- Placeholder files need real data population (optional expansion)

---

## 📋 Table of Contents

1. [Executive Summary](#executive-summary)
2. [Current vs. Proposed Structure](#current-vs-proposed-structure)
3. [Data Relationships Explained](#data-relationships-explained)
4. [Migration Phases](#migration-phases)
5. [Phase 1: Minimal Reorganization](#phase-1-minimal-reorganization)
6. [Phase 2: Full Migration](#phase-2-full-migration)
7. [Testing Strategy](#testing-strategy)
8. [Rollback Plan](#rollback-plan)
9. [Execution Checklist](#execution-checklist)

---

## 🎯 Executive Summary

### Problem
Currently, all 26 JSON data files exist in a flat structure at `Game.Shared/Data/Json/`, making it difficult to:
- Find related files quickly
- Understand data relationships
- Expand with new content types
- Organize modding/DLC content

### Solution
Reorganize files into a hierarchical folder structure that groups related data and allows for future expansion.

### Approach
**Single-Phase Migration**:
- **Complete reorganization** with full folder hierarchy in one go
- Includes all logical groupings AND future expansion slots
- Do it once, do it right - no incremental migrations needed

### Benefits
- ✅ **Clarity**: Logical grouping of related data
- ✅ **Scalability**: Room to add prefixes, suffixes, traits per category
- ✅ **Maintainability**: Easier to find and edit related files
- ✅ **Modding Support**: Clear structure for community content
- ✅ **Future-Proof**: Prepared for upcoming features (crafting, dialogue, etc.)

---

## 📁 Current vs. Proposed Structure

### Current Structure (Flat - 26 Files)

```
Game.Shared/Data/Json/
├── armor_materials.json              # Armor material definitions (Leather, Steel, etc.)
├── armor_names.json                  # Armor piece names ("Cuirass", "Helm")
├── beast_names.json                  # Beast enemy names
├── cloth.json                        # Cloth material list (for armor/items)
├── consumable_names.json             # Potion/food names
├── demon_names.json                  # Demon enemy names
├── dragon_names.json                 # Dragon enemy names
├── elemental_names.json              # Elemental enemy names
├── enchantment_suffixes.json         # Magic item suffixes ("of Fire", "of Wisdom")
├── gemstones.json                    # Gemstone material list
├── goblinoid_names.json              # Goblinoid enemy names
├── humanoid_names.json               # Humanoid enemy names
├── insect_names.json                 # Insect enemy names
├── leathers.json                     # Leather material list (for armor/items)
├── metals.json                       # Metal material list (for weapons/armor)
├── npc_first_names.json              # NPC first names
├── npc_last_names.json               # NPC last names
├── npc_occupations.json              # NPC job titles
├── orc_names.json                    # Orc enemy names
├── plant_names.json                  # Plant enemy names
├── quest_templates.json              # Quest generation templates
├── reptilian_names.json              # Reptilian enemy names
├── troll_names.json                  # Troll enemy names
├── undead_names.json                 # Undead enemy names
├── vampire_names.json                # Vampire enemy names
├── weapon_names.json                 # Weapon type names ("Longsword", "Dagger")
├── weapon_prefixes.json              # Weapon quality prefixes ("Rusty", "Fine", "Legendary")
└── woods.json                        # Wood material list (for weapons/items)
```

### Proposed Structure - Phase 1 (Complete Reorganization - Do It All!)

**NOTE**: We're implementing the FULL deep structure in one migration - no incremental phases!

```
Game.Shared/Data/Json/
├── enemies/
│   ├── beasts/
│   │   ├── names.json                # beast_names.json → moved
│   │   ├── prefixes.json             # FUTURE: "Rabid", "Alpha", "Starving"
│   │   ├── suffixes.json             # FUTURE: "of the Wild", "the Feral"
│   │   └── traits.json               # FUTURE: Pack Hunter, Nocturnal
│   ├── demons/
│   │   ├── names.json                # demon_names.json → moved
│   │   ├── prefixes.json             # FUTURE: "Infernal", "Dark", "Cursed"
│   │   ├── suffixes.json             # FUTURE: "of the Abyss", "the Damned"
│   │   ├── titles.json               # FUTURE: "Lord", "Duke", "Prince"
│   │   └── traits.json               # FUTURE: Fire Resistant, etc.
│   ├── dragons/
│   │   ├── names.json                # dragon_names.json → moved
│   │   ├── prefixes.json             # FUTURE
│   │   ├── suffixes.json             # FUTURE
│   │   └── traits.json               # FUTURE
│   ├── elementals/
│   │   ├── names.json                # elemental_names.json → moved
│   │   ├── prefixes.json             # FUTURE
│   │   ├── suffixes.json             # FUTURE
│   │   └── traits.json               # FUTURE
│   ├── goblinoids/
│   │   ├── names.json                # goblinoid_names.json → moved
│   │   ├── prefixes.json             # FUTURE
│   │   ├── suffixes.json             # FUTURE
│   │   └── traits.json               # FUTURE
│   ├── humanoids/
│   │   ├── names.json                # humanoid_names.json → moved
│   │   ├── prefixes.json             # FUTURE
│   │   ├── suffixes.json             # FUTURE
│   │   └── traits.json               # FUTURE
│   ├── insects/
│   │   ├── names.json                # insect_names.json → moved
│   │   ├── prefixes.json             # FUTURE
│   │   ├── suffixes.json             # FUTURE
│   │   └── traits.json               # FUTURE
│   ├── orcs/
│   │   ├── names.json                # orc_names.json → moved
│   │   ├── prefixes.json             # FUTURE
│   │   ├── suffixes.json             # FUTURE
│   │   └── traits.json               # FUTURE
│   ├── plants/
│   │   ├── names.json                # plant_names.json → moved
│   │   ├── prefixes.json             # FUTURE
│   │   ├── suffixes.json             # FUTURE
│   │   └── traits.json               # FUTURE
│   ├── reptilians/
│   │   ├── names.json                # reptilian_names.json → moved
│   │   ├── prefixes.json             # FUTURE
│   │   ├── suffixes.json             # FUTURE
│   │   └── traits.json               # FUTURE
│   ├── trolls/
│   │   ├── names.json                # troll_names.json → moved
│   │   ├── prefixes.json             # FUTURE
│   │   ├── suffixes.json             # FUTURE
│   │   └── traits.json               # FUTURE
│   ├── undead/
│   │   ├── names.json                # undead_names.json → moved
│   │   ├── prefixes.json             # FUTURE
│   │   ├── suffixes.json             # FUTURE
│   │   └── traits.json               # FUTURE
│   └── vampires/
│       ├── names.json                # vampire_names.json → moved
│       ├── prefixes.json             # FUTURE
│       ├── suffixes.json             # FUTURE
│       └── traits.json               # FUTURE
│
├── items/
│   ├── weapons/
│   │   ├── prefixes.json             # weapon_prefixes.json → moved
│   │   ├── names.json                # weapon_names.json → moved
│   │   └── suffixes.json             # FUTURE: magic weapon suffixes
│   ├── armor/
│   │   ├── materials.json            # armor_materials.json → moved
│   │   ├── names.json                # armor_names.json → moved
│   │   ├── prefixes.json             # FUTURE: "Reinforced", "Blessed"
│   │   └── suffixes.json             # FUTURE: "of Protection", etc.
│   ├── consumables/
│   │   ├── names.json                # consumable_names.json → moved
│   │   ├── effects.json              # FUTURE: potion effect definitions
│   │   └── rarities.json             # FUTURE: consumable rarity tiers
│   ├── materials/
│   │   ├── metals.json               # metals.json → moved
│   │   ├── woods.json                # woods.json → moved
│   │   ├── leathers.json             # leathers.json → moved
│   │   ├── gemstones.json            # gemstones.json → moved
│   │   └── cloth.json                # cloth.json → moved
│   └── enchantments/
│       ├── suffixes.json             # enchantment_suffixes.json → moved
│       ├── prefixes.json             # FUTURE: enchantment prefixes
│       └── effects.json              # FUTURE: specific enchantment effects
│
├── npcs/
│   ├── names/
│   │   ├── first_names.json          # npc_first_names.json → moved, renamed
│   │   └── last_names.json           # npc_last_names.json → moved, renamed
│   ├── occupations/
│   │   ├── common.json               # npc_occupations.json → moved, renamed
│   │   ├── noble.json                # FUTURE: Duke, Baron, Knight
│   │   ├── criminal.json             # FUTURE: Thief, Assassin, Smuggler
│   │   └── magical.json              # FUTURE: Wizard, Alchemist, Enchanter
│   ├── personalities/
│   │   ├── traits.json               # FUTURE: Friendly, Grumpy, Wise
│   │   ├── quirks.json               # FUTURE: Stutters, Nervous, Loud
│   │   └── backgrounds.json          # FUTURE: Veteran, Orphan, Noble
│   └── dialogue/
│       ├── greetings.json            # FUTURE: greeting templates
│       ├── farewells.json            # FUTURE: goodbye templates
│       └── rumors.json               # FUTURE: rumor templates
│
├── quests/
│   ├── templates/
│   │   ├── fetch.json                # Split from quest_templates.json
│   │   ├── kill.json                 # Split from quest_templates.json
│   │   ├── escort.json               # Split from quest_templates.json
│   │   └── explore.json              # Split from quest_templates.json
│   ├── objectives/
│   │   ├── primary.json              # FUTURE: main quest objectives
│   │   ├── secondary.json            # FUTURE: optional objectives
│   │   └── hidden.json               # FUTURE: secret objectives
│   ├── rewards/
│   │   ├── gold.json                 # FUTURE: gold reward tiers
│   │   ├── items.json                # FUTURE: item reward pools
│   │   └── experience.json           # FUTURE: XP reward formulas
│   └── locations/
│       ├── dungeons.json             # FUTURE: dungeon location names
│       ├── towns.json                # FUTURE: town names
│       └── wilderness.json           # FUTURE: wilderness area names
│
└── general/                          # FUTURE: Shared descriptive data
    ├── colors.json                   # FUTURE: "crimson", "azure", "emerald"
    ├── adjectives.json               # FUTURE: "ancient", "mysterious", "shattered"
    ├── verbs.json                    # FUTURE: "slashes", "crushes", "pierces"
    ├── weather.json                  # FUTURE: weather conditions
    ├── time_of_day.json              # FUTURE: time descriptors
    ├── sounds.json                   # FUTURE: ambient sounds
    ├── smells.json                   # FUTURE: smell descriptors
    └── textures.json                 # FUTURE: texture descriptions
```

**Files Moved**: 26 existing files  
**Files Split**: 2 files (quest_templates → 4 files, npc_occupations → 4 files)
**Placeholder Files**: ~70 files for future expansion
**Total Files After Migration**: ~100 files
**New Folders**: 5 top-level + many subfolders (~30 folders total)

---

## 🔗 Data Relationships Explained

### Understanding the Item Data Flow

There are **THREE TYPES** of item-related JSON files, each serving a different purpose:

#### 1. **Raw Materials** (`items/materials/*.json`)
**Purpose**: Lists of base materials available in the game world  
**Format**: Simple arrays or rarity-based lists  
**Usage**: Referenced by crafting system, item generation, world building

**Example - `items/materials/metals.json`**:
```json
{
  "common": [
    { "name": "Iron", "displayName": "Iron", "rarity": "Common", "traits": {...} },
    { "name": "Bronze", "displayName": "Bronze", "rarity": "Common", "traits": {...} }
  ],
  "uncommon": [
    { "name": "Steel", "displayName": "Steel", "rarity": "Uncommon", "traits": {...} }
  ],
  "rare": [
    { "name": "Mithril", "displayName": "Mithril", "rarity": "Rare", "traits": {...} }
  ]
}
```

**What it represents**: 
- These are the **raw materials** that exist in the game world
- Think of this as a "periodic table" of available metals
- Used for: Crafting recipes, loot drops, merchant inventories

#### 2. **Item Construction Templates** (`items/armor/materials.json`, `items/weapons/prefixes.json`)
**Purpose**: Define how raw materials **become** finished items with properties  
**Format**: Complex nested structures with traits and modifiers  
**Usage**: Item generator uses these to create actual equipment

**Example - `items/armor/materials.json`**:
```json
{
  "common": {
    "leather": {
      "name": "Leather",           // References items/materials/leathers.json
      "displayName": "Leather",
      "rarity": "Common",
      "traits": [
        { "name": "bonusStrength", "value": 1, "type": "number" },
        { "name": "bonusDefense", "value": 2, "type": "number" }
      ]
    },
    "bronze": {
      "name": "Bronze",
      "displayName": "Bronze",
      "rarity": "Common",
      "traits": [
        { "name": "bonusStrength", "value": 2, "type": "number" },
        { "name": "bonusDefense", "value": 3, "type": "number" }
      ]
    }
  },
  "uncommon": {
    "steel": {
      "name": "Steel",              // References items/materials/metals.json
      "displayName": "Steel",
      "rarity": "Uncommon",
      "traits": [
        { "name": "bonusStrength", "value": 3, "type": "number" },
        { "name": "bonusDefense", "value": 5, "type": "number" }
      ]
    }
  },
  "rare": {
    "mithril": {
      "name": "Mithril",
      "displayName": "Mithril",
      "rarity": "Rare",
      "traits": [
        { "name": "bonusStrength", "value": 5, "type": "number" },
        { "name": "bonusDefense", "value": 8, "type": "number" }
      ]
    }
  }
}
```

**What it represents**:
- **How materials become armor** with specific stat bonuses
- "Leather" from `leathers.json` + armor construction = "Leather Armor" with +1 STR, +2 DEF
- "Steel" from `metals.json` + armor construction = "Steel Armor" with +3 STR, +5 DEF

#### 3. **Item Names** (`items/armor/names.json`, `items/weapons/names.json`)
**Purpose**: The actual **piece names** for finished items  
**Format**: Simple string arrays  
**Usage**: Combined with materials to create final item names

**Example - `items/armor/names.json`**:
```json
[
  "Cuirass",
  "Helm",
  "Greaves",
  "Gauntlets",
  "Boots",
  "Pauldrons"
]
```

**What it represents**:
- The **type of armor piece** being created
- Combined with material: "Steel Cuirass", "Leather Helm", "Mithril Gauntlets"

### The Complete Item Generation Flow

```
Step 1: Select Material Type
  └─> items/materials/metals.json → "Steel" (raw material exists)

Step 2: Apply Material to Item Category
  └─> items/armor/materials.json → Find "Steel" entry with traits
      └─> Result: Steel gives +3 STR, +5 DEF when used as armor

Step 3: Select Item Piece Name
  └─> items/armor/names.json → "Cuirass"

Step 4: Combine Everything
  └─> Final Item: "Steel Cuirass" (+3 STR, +5 DEF, Uncommon rarity)
```

### Example Combinations

| Material (metals.json) | Template (armor/materials.json) | Name (armor/names.json) | Final Result |
|---|---|---|---|
| Iron (Common) | Iron → +1 STR, +3 DEF | Helm | "Iron Helm" (+1 STR, +3 DEF) |
| Steel (Uncommon) | Steel → +3 STR, +5 DEF | Cuirass | "Steel Cuirass" (+3 STR, +5 DEF) |
| Mithril (Rare) | Mithril → +5 STR, +8 DEF | Gauntlets | "Mithril Gauntlets" (+5 STR, +8 DEF) |

### Why This Separation?

1. **materials/*.json** (Raw Materials)
   - ✅ Can be used across multiple item types (weapons AND armor)
   - ✅ Can be displayed in loot, merchant inventories, crafting menus
   - ✅ Represents what exists in the game world

2. **armor/materials.json** (Construction Templates)
   - ✅ Defines how materials behave **specifically as armor**
   - ✅ Steel as armor gives different bonuses than steel as weapon
   - ✅ Same material, different application = different stats

3. **armor/names.json** (Piece Names)
   - ✅ Reusable across all materials
   - ✅ Every material can become every piece type
   - ✅ "Cuirass" + any material = valid armor piece

### Weapon Example (Same Pattern)

```
items/materials/metals.json         → "Steel" (raw material)
items/materials/woods.json          → "Oak" (raw material)

items/weapons/prefixes.json         → How materials become weapons with bonuses
  └─> "Steel" → +5 damage, +2 durability
  └─> "Oak" → +3 damage, +1 durability (wooden weapons)

items/weapons/names.json            → "Longsword", "Dagger", "Bow"

Final Results:
  - "Steel Longsword" (+5 damage, +2 durability)
  - "Oak Bow" (+3 damage, +1 durability)
```

### Visual Relationship Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                     RAW MATERIALS                           │
│  items/materials/metals.json, woods.json, leathers.json     │
│  ["Iron", "Steel", "Mithril", "Oak", "Leather", ...]       │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ├──────────────┬────────────────┐
                      ▼              ▼                ▼
         ┌─────────────────┐  ┌──────────────┐  ┌──────────────┐
         │ WEAPON TEMPLATE │  │ ARMOR TEMPLATE│ │CONSUMABLE...│
         │items/weapons/   │  │items/armor/   │  │             │
         │prefixes.json    │  │materials.json │  │             │
         │                 │  │               │  │             │
         │"Steel" → +5 DMG │  │"Steel"→+5 DEF │  │             │
         └────────┬────────┘  └───────┬───────┘  └─────────────┘
                  │                   │
                  ▼                   ▼
         ┌─────────────────┐  ┌──────────────┐
         │  WEAPON NAMES   │  │  ARMOR NAMES │
         │items/weapons/   │  │items/armor/  │
         │names.json       │  │names.json    │
         │                 │  │              │
         │["Longsword",    │  │["Cuirass",   │
         │ "Dagger", ...]  │  │ "Helm", ...] │
         └────────┬────────┘  └───────┬──────┘
                  │                   │
                  └──────────┬────────┘
                             ▼
                  ┌────────────────────┐
                  │   FINAL ITEM       │
                  │ "Steel Longsword"  │
                  │  +5 Damage         │
                  │  Uncommon Rarity   │
                  └────────────────────┘
```

### Key Insight

**The same raw material can be used differently across item types**:

- **Steel from `metals.json`**:
  - As weapon → `weapons/prefixes.json` → +5 damage
  - As armor → `armor/materials.json` → +5 defense
  - In crafting → Can be melted, forged, traded
  - In loot → Can drop as raw material

This separation allows:
1. ✅ **Crafting System**: Collect "Steel" material, forge into weapons or armor
2. ✅ **Consistency**: "Steel" always means the same raw material
3. ✅ **Flexibility**: Same material, different stats per item category
4. ✅ **Economy**: Materials can be traded, sold, found separately from items

---

## 📋 Migration Phase

### Phase 1: Complete Reorganization (DO IT ALL NOW!)
**Goal**: Full reorganization with future expansion in one migration  
**Timeline**: 1 day (4-6 hours)  
**Files Moved**: 26 existing files  
**New Structure**: Deep folder hierarchy ready for growth  
**Breaking Changes**: Moderate (path updates with backward compatibility)

**What Changes**:
- Create complete folder structure (items/weapons/, items/armor/, etc.)
- Move all 26 files to final locations
- Rename files (remove prefixes, clean names)
- Create placeholder empty files for future features
- Update all code paths with backward compatibility
- No incremental migrations - do it once, do it right!

**Why Single Phase**:
- ✅ Avoid migration fatigue (no "we'll do Phase 2 later" that never happens)
- ✅ Cleaner codebase immediately
- ✅ Ready for future features NOW
- ✅ Only update code paths once
- ✅ Only test once
- ✅ Team learns new structure once

**Risk Mitigation**:
- Git branch for safety
- **No backward compatibility** - clean break from old paths
- Comprehensive testing checklist
- Rollback plan ready (git revert)

---

## 🎯 Phase 1: Complete Reorganization (DETAILED PLAN)

### Overview

**This is the ONLY phase - we're doing the full reorganization in one go!**

We'll create the complete folder structure from "Phase 2" immediately, including:
- Deep folder hierarchy (items/weapons/, items/armor/, npcs/names/, etc.)
- All 26 existing files moved to final locations
- Placeholder empty files for future features (marked as FUTURE)
- All code paths updated once
- Comprehensive testing

**Timeline**: 1 day (4-6 hours of focused work)

### Step 1: Create Complete Folder Structure

### Step 1: Create Complete Folder Structure

**New Folders to Create** (full deep hierarchy):
```
Game.Shared/Data/Json/
├── enemies/
│   ├── beasts/
│   ├── demons/
│   ├── dragons/
│   ├── elementals/
│   ├── goblinoids/
│   ├── humanoids/
│   ├── insects/
│   ├── orcs/
│   ├── plants/
│   ├── reptilians/
│   ├── trolls/
│   ├── undead/
│   └── vampires/
├── items/
│   ├── weapons/
│   ├── armor/
│   ├── consumables/
│   ├── materials/
│   └── enchantments/
├── npcs/
│   ├── names/
│   ├── occupations/
│   ├── personalities/       # FUTURE
│   └── dialogue/            # FUTURE
├── quests/
│   ├── templates/
│   ├── objectives/          # FUTURE
│   ├── rewards/             # FUTURE
│   └── locations/           # FUTURE
└── general/                 # FUTURE (all files)
```

**PowerShell Commands**:
```powershell
$jsonRoot = "Game.Shared/Data/Json"

# Create top-level folders
New-Item -ItemType Directory -Path "$jsonRoot/enemies" -Force
New-Item -ItemType Directory -Path "$jsonRoot/items" -Force
New-Item -ItemType Directory -Path "$jsonRoot/npcs" -Force
New-Item -ItemType Directory -Path "$jsonRoot/quests" -Force
New-Item -ItemType Directory -Path "$jsonRoot/general" -Force

# Create enemy subfolders
$enemyTypes = @("beasts", "demons", "dragons", "elementals", "goblinoids", 
                "humanoids", "insects", "orcs", "plants", "reptilians", 
                "trolls", "undead", "vampires")
foreach ($type in $enemyTypes) {
    New-Item -ItemType Directory -Path "$jsonRoot/enemies/$type" -Force
}

# Create item subfolders
New-Item -ItemType Directory -Path "$jsonRoot/items/weapons" -Force
New-Item -ItemType Directory -Path "$jsonRoot/items/armor" -Force
New-Item -ItemType Directory -Path "$jsonRoot/items/consumables" -Force
New-Item -ItemType Directory -Path "$jsonRoot/items/materials" -Force
New-Item -ItemType Directory -Path "$jsonRoot/items/enchantments" -Force

# Create NPC subfolders
New-Item -ItemType Directory -Path "$jsonRoot/npcs/names" -Force
New-Item -ItemType Directory -Path "$jsonRoot/npcs/occupations" -Force
New-Item -ItemType Directory -Path "$jsonRoot/npcs/personalities" -Force
New-Item -ItemType Directory -Path "$jsonRoot/npcs/dialogue" -Force

# Create quest subfolders
New-Item -ItemType Directory -Path "$jsonRoot/quests/templates" -Force
New-Item -ItemType Directory -Path "$jsonRoot/quests/objectives" -Force
New-Item -ItemType Directory -Path "$jsonRoot/quests/rewards" -Force
New-Item -ItemType Directory -Path "$jsonRoot/quests/locations" -Force
```

### Step 2: Move Files to New Locations

**File Moves**:

| Old Path | New Path | Notes |
|----------|----------|-------|
| `beast_names.json` | `enemies/beasts/names.json` | Enemy subfolder |
| `demon_names.json` | `enemies/demons/names.json` | Enemy subfolder |
| `dragon_names.json` | `enemies/dragons/names.json` | Enemy subfolder |
| `elemental_names.json` | `enemies/elementals/names.json` | Enemy subfolder |
| `goblinoid_names.json` | `enemies/goblinoids/names.json` | Enemy subfolder |
| `humanoid_names.json` | `enemies/humanoids/names.json` | Enemy subfolder |
| `insect_names.json` | `enemies/insects/names.json` | Enemy subfolder |
| `orc_names.json` | `enemies/orcs/names.json` | Enemy subfolder |
| `plant_names.json` | `enemies/plants/names.json` | Enemy subfolder |
| `reptilian_names.json` | `enemies/reptilians/names.json` | Enemy subfolder |
| `troll_names.json` | `enemies/trolls/names.json` | Enemy subfolder |
| `undead_names.json` | `enemies/undead/names.json` | Enemy subfolder |
| `vampire_names.json` | `enemies/vampires/names.json` | Enemy subfolder |
| `cloth.json` | `items/materials/cloth.json` | Materials folder |
| `gemstones.json` | `items/materials/gemstones.json` | Materials folder |
| `leathers.json` | `items/materials/leathers.json` | Materials folder |
| `metals.json` | `items/materials/metals.json` | Materials folder |
| `woods.json` | `items/materials/woods.json` | Materials folder |
| `armor_materials.json` | `items/armor_materials.json` | Items folder |
| `armor_names.json` | `items/armor_names.json` | Items folder |
| `consumable_names.json` | `items/consumable_names.json` | Items folder |
| `enchantment_suffixes.json` | `items/enchantment_suffixes.json` | Items folder |
| `weapon_names.json` | `items/weapon_names.json` | Items folder |
| `weapon_prefixes.json` | `items/weapon_prefixes.json` | Items folder |
| `npc_first_names.json` | `npcs/first_names.json` | NPCs folder, renamed |
| `npc_last_names.json` | `npcs/last_names.json` | NPCs folder, renamed |
| `npc_occupations.json` | `npcs/occupations.json` | NPCs folder, renamed |
| `quest_templates.json` | `quests/templates.json` | Quests folder, renamed |

**PowerShell Commands**:
```powershell
$jsonRoot = "Game.Shared/Data/Json"

# Move enemy files
$enemyMoves = @{
    "beast_names.json" = "enemies/beasts/names.json"
    "demon_names.json" = "enemies/demons/names.json"
    "dragon_names.json" = "enemies/dragons/names.json"
    "elemental_names.json" = "enemies/elementals/names.json"
    "goblinoid_names.json" = "enemies/goblinoids/names.json"
    "humanoid_names.json" = "enemies/humanoids/names.json"
    "insect_names.json" = "enemies/insects/names.json"
    "orc_names.json" = "enemies/orcs/names.json"
    "plant_names.json" = "enemies/plants/names.json"
    "reptilian_names.json" = "enemies/reptilians/names.json"
    "troll_names.json" = "enemies/trolls/names.json"
    "undead_names.json" = "enemies/undead/names.json"
    "vampire_names.json" = "enemies/vampires/names.json"
}
foreach ($old in $enemyMoves.Keys) {
    Move-Item "$jsonRoot/$old" "$jsonRoot/$($enemyMoves[$old])"
}

# Move material files
$materialMoves = @{
    "cloth.json" = "items/materials/cloth.json"
    "gemstones.json" = "items/materials/gemstones.json"
    "leathers.json" = "items/materials/leathers.json"
    "metals.json" = "items/materials/metals.json"
    "woods.json" = "items/materials/woods.json"
}
foreach ($old in $materialMoves.Keys) {
    Move-Item "$jsonRoot/$old" "$jsonRoot/$($materialMoves[$old])"
}

# Move item files
$itemMoves = @{
    "armor_materials.json" = "items/armor_materials.json"
    "armor_names.json" = "items/armor_names.json"
    "consumable_names.json" = "items/consumable_names.json"
    "enchantment_suffixes.json" = "items/enchantment_suffixes.json"
    "weapon_names.json" = "items/weapon_names.json"
    "weapon_prefixes.json" = "items/weapon_prefixes.json"
}
foreach ($old in $itemMoves.Keys) {
    Move-Item "$jsonRoot/$old" "$jsonRoot/$($itemMoves[$old])"
}

# Move NPC files (with rename)
Move-Item "$jsonRoot/npc_first_names.json" "$jsonRoot/npcs/names/first_names.json"
Move-Item "$jsonRoot/npc_last_names.json" "$jsonRoot/npcs/names/last_names.json"
Move-Item "$jsonRoot/npc_occupations.json" "$jsonRoot/npcs/occupations/common.json"

# Note: quest_templates.json will be split in Step 3, not moved here
```

### Step 3: Split quest_templates.json into 4 Files

**What**: Split quest templates by quest type into separate files

**Source File**: `Game.Shared/Data/Json/quest_templates.json`

**Target Files**:
- `Game.Shared/Data/Json/quests/templates/fetch.json` - Fetch/retrieve quests
- `Game.Shared/Data/Json/quests/templates/kill.json` - Combat/elimination quests  
- `Game.Shared/Data/Json/quests/templates/escort.json` - Escort/protection quests
- `Game.Shared/Data/Json/quests/templates/explore.json` - Exploration/discovery quests

**How to Split**:

1. **Read the original file** and group by `type` field
2. **Create 4 new files** with filtered content
3. **Delete the original** file after confirming splits are correct

**Example Split Logic** (manual or scripted):

```csharp
// Read original file
var allTemplates = JsonSerializer.Deserialize<List<QuestTemplate>>(
    File.ReadAllText("Game.Shared/Data/Json/quest_templates.json"));

// Group by type
var fetchQuests = allTemplates.Where(q => q.Type == "fetch").ToList();
var killQuests = allTemplates.Where(q => q.Type == "kill").ToList();
var escortQuests = allTemplates.Where(q => q.Type == "escort").ToList();
var exploreQuests = allTemplates.Where(q => q.Type == "explore").ToList();

// Write to new files
File.WriteAllText("Game.Shared/Data/Json/quests/templates/fetch.json", 
    JsonSerializer.Serialize(fetchQuests, new JsonSerializerOptions { WriteIndented = true }));
File.WriteAllText("Game.Shared/Data/Json/quests/templates/kill.json", 
    JsonSerializer.Serialize(killQuests, new JsonSerializerOptions { WriteIndented = true }));
File.WriteAllText("Game.Shared/Data/Json/quests/templates/escort.json", 
    JsonSerializer.Serialize(escortQuests, new JsonSerializerOptions { WriteIndented = true }));
File.WriteAllText("Game.Shared/Data/Json/quests/templates/explore.json", 
    JsonSerializer.Serialize(exploreQuests, new JsonSerializerOptions { WriteIndented = true }));

// Verify files were created correctly, then delete original
File.Delete("Game.Shared/Data/Json/quest_templates.json");
```

**Manual Alternative**:
1. Open `quest_templates.json` in editor
2. Copy all "fetch" type quests → save to `quests/templates/fetch.json`
3. Copy all "kill" type quests → save to `quests/templates/kill.json`
4. Copy all "escort" type quests → save to `quests/templates/escort.json`
5. Copy all "explore" type quests → save to `quests/templates/explore.json`
6. Verify each file, then delete original

### Step 4: Split npc_occupations.json

**What**: Existing data becomes `common.json`, create 3 placeholder files

**Source File**: Already moved to `npcs/occupations/common.json` in Step 2

**Additional Files to Create**:
- `Game.Shared/Data/Json/npcs/occupations/noble.json` (placeholder)
- `Game.Shared/Data/Json/npcs/occupations/criminal.json` (placeholder)
- `Game.Shared/Data/Json/npcs/occupations/magical.json` (placeholder)

**Placeholder Content** (each file):
```json
[
  {
    "name": "PLACEHOLDER",
    "displayName": "Placeholder Occupation",
    "description": "Future: Add noble/criminal/magical occupations here",
    "rarity": "Common"
  }
]
```

**PowerShell to Create Placeholders**:
```powershell
$occupationsPath = "Game.Shared/Data/Json/npcs/occupations"

$placeholderContent = @'
[
  {
    "name": "PLACEHOLDER",
    "displayName": "Placeholder Occupation",
    "description": "Future: Add occupations here",
    "rarity": "Common"
  }
]
'@

Set-Content -Path "$occupationsPath/noble.json" -Value $placeholderContent
Set-Content -Path "$occupationsPath/criminal.json" -Value $placeholderContent
Set-Content -Path "$occupationsPath/magical.json" -Value $placeholderContent
```

### Step 5: Create All Placeholder Files (~70 files)

**What**: Create empty placeholder files for future expansion

**Placeholder File Format**:
```json
[
  {
    "name": "PLACEHOLDER",
    "displayName": "Placeholder Entry",
    "description": "Future: Add content here"
  }
]
```

**Categories to Create**:

**1. Enemy Prefixes/Suffixes/Traits** (13 enemy types × 3 files = 39 files):
```powershell
$enemyTypes = @("beasts", "demons", "dragons", "elementals", "goblinoids", 
                "humanoids", "insects", "orcs", "plants", "reptilians", 
                "trolls", "undead", "vampires")

$enemyPlaceholder = @'
[
  {
    "name": "PLACEHOLDER",
    "displayName": "Placeholder Prefix/Suffix",
    "description": "Future: Add enemy modifiers here"
  }
]
'@

foreach ($type in $enemyTypes) {
    $basePath = "Game.Shared/Data/Json/enemies/$type"
    Set-Content -Path "$basePath/prefixes.json" -Value $enemyPlaceholder
    Set-Content -Path "$basePath/suffixes.json" -Value $enemyPlaceholder
    Set-Content -Path "$basePath/traits.json" -Value $enemyPlaceholder
}
```

**2. Weapon Suffixes** (1 file):
```powershell
$weaponPlaceholder = @'
[
  {
    "name": "PLACEHOLDER",
    "displayName": "Placeholder Suffix",
    "description": "Future: Add weapon suffixes here",
    "bonuses": {}
  }
]
'@

Set-Content -Path "Game.Shared/Data/Json/items/weapons/suffixes.json" -Value $weaponPlaceholder
```

**3. Armor Prefixes** (1 file):
```powershell
Set-Content -Path "Game.Shared/Data/Json/items/armor/prefixes.json" -Value $weaponPlaceholder
```

**4. Consumable Effects/Rarities** (2 files):
```powershell
Set-Content -Path "Game.Shared/Data/Json/items/consumables/effects.json" -Value $weaponPlaceholder
Set-Content -Path "Game.Shared/Data/Json/items/consumables/rarities.json" -Value $weaponPlaceholder
```

**5. Enchantment Prefixes/Effects** (2 files):
```powershell
Set-Content -Path "Game.Shared/Data/Json/items/enchantments/prefixes.json" -Value $weaponPlaceholder
Set-Content -Path "Game.Shared/Data/Json/items/enchantments/effects.json" -Value $weaponPlaceholder
```

**6. NPC Personality/Dialogue Files** (6 files):
```powershell
$npcPlaceholder = @'
[
  {
    "name": "PLACEHOLDER",
    "displayName": "Placeholder NPC Data",
    "description": "Future: Add NPC content here"
  }
]
'@

Set-Content -Path "Game.Shared/Data/Json/npcs/personalities/traits.json" -Value $npcPlaceholder
Set-Content -Path "Game.Shared/Data/Json/npcs/personalities/quirks.json" -Value $npcPlaceholder
Set-Content -Path "Game.Shared/Data/Json/npcs/personalities/backgrounds.json" -Value $npcPlaceholder
Set-Content -Path "Game.Shared/Data/Json/npcs/dialogue/greetings.json" -Value $npcPlaceholder
Set-Content -Path "Game.Shared/Data/Json/npcs/dialogue/farewells.json" -Value $npcPlaceholder
Set-Content -Path "Game.Shared/Data/Json/npcs/dialogue/rumors.json" -Value $npcPlaceholder
```

**7. Quest Objectives/Rewards/Locations** (9 files):
```powershell
$questPlaceholder = @'
[
  {
    "name": "PLACEHOLDER",
    "displayName": "Placeholder Quest Data",
    "description": "Future: Add quest content here"
  }
]
'@

Set-Content -Path "Game.Shared/Data/Json/quests/objectives/primary.json" -Value $questPlaceholder
Set-Content -Path "Game.Shared/Data/Json/quests/objectives/secondary.json" -Value $questPlaceholder
Set-Content -Path "Game.Shared/Data/Json/quests/objectives/hidden.json" -Value $questPlaceholder
Set-Content -Path "Game.Shared/Data/Json/quests/rewards/gold.json" -Value $questPlaceholder
Set-Content -Path "Game.Shared/Data/Json/quests/rewards/items.json" -Value $questPlaceholder
Set-Content -Path "Game.Shared/Data/Json/quests/rewards/experience.json" -Value $questPlaceholder
Set-Content -Path "Game.Shared/Data/Json/quests/locations/dungeons.json" -Value $questPlaceholder
Set-Content -Path "Game.Shared/Data/Json/quests/locations/towns.json" -Value $questPlaceholder
Set-Content -Path "Game.Shared/Data/Json/quests/locations/wilderness.json" -Value $questPlaceholder
```

**8. General Descriptive Lists** (8 files):
```powershell
$generalPlaceholder = @'
[
  "PLACEHOLDER_WORD_1",
  "PLACEHOLDER_WORD_2",
  "PLACEHOLDER_WORD_3"
]
'@

Set-Content -Path "Game.Shared/Data/Json/general/colors.json" -Value $generalPlaceholder
Set-Content -Path "Game.Shared/Data/Json/general/adjectives.json" -Value $generalPlaceholder
Set-Content -Path "Game.Shared/Data/Json/general/verbs.json" -Value $generalPlaceholder
Set-Content -Path "Game.Shared/Data/Json/general/weather.json" -Value $generalPlaceholder
Set-Content -Path "Game.Shared/Data/Json/general/time_of_day.json" -Value $generalPlaceholder
Set-Content -Path "Game.Shared/Data/Json/general/sounds.json" -Value $generalPlaceholder
Set-Content -Path "Game.Shared/Data/Json/general/smells.json" -Value $generalPlaceholder
Set-Content -Path "Game.Shared/Data/Json/general/textures.json" -Value $generalPlaceholder
```

**Total Placeholder Files**: ~70 files

### Step 6: Update Game.Shared Project File

**Update `Game.Shared.csproj`** to include new file paths:

```xml
<ItemGroup>
  <!-- Enemy JSON Files -->
  <None Update="Data\Json\enemies\beasts\names.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
  <None Update="Data\Json\enemies\demons\names.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
  <!-- ... (all 13 enemy types) ... -->
  
  <!-- Material JSON Files -->
  <None Update="Data\Json\items\materials\cloth.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
  <!-- ... (all 5 materials) ... -->
  
  <!-- Item JSON Files -->
  <None Update="Data\Json\items\armor_materials.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
  <!-- ... (all 6 item files) ... -->
  
  <!-- NPC JSON Files -->
  <None Update="Data\Json\npcs\first_names.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
  <!-- ... (all 3 NPC files) ... -->
  
  <!-- Quest JSON Files -->
  <None Update="Data\Json\quests\templates.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

### Step 4: Update GameDataService Paths

**File**: `Game.Shared/Services/GameDataService.cs`

**Current Code**:
```csharp
private static string GetJsonFilePath(string fileName)
{
    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Json", fileName);
}
```

**New Code** (clean break - no backward compatibility):
```csharp
private static string GetJsonFilePath(string fileName)
{
    string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Json");
    
    // Map old filenames to new paths (for code that still uses old names)
    return fileName switch
    {
        // Enemy files
        "beast_names.json" => Path.Combine(baseDir, "enemies", "beasts", "names.json"),
        "demon_names.json" => Path.Combine(baseDir, "enemies", "demons", "names.json"),
        "dragon_names.json" => Path.Combine(baseDir, "enemies", "dragons", "names.json"),
        "elemental_names.json" => Path.Combine(baseDir, "enemies", "elementals", "names.json"),
        "goblinoid_names.json" => Path.Combine(baseDir, "enemies", "goblinoids", "names.json"),
        "humanoid_names.json" => Path.Combine(baseDir, "enemies", "humanoids", "names.json"),
        "insect_names.json" => Path.Combine(baseDir, "enemies", "insects", "names.json"),
        "orc_names.json" => Path.Combine(baseDir, "enemies", "orcs", "names.json"),
        "plant_names.json" => Path.Combine(baseDir, "enemies", "plants", "names.json"),
        "reptilian_names.json" => Path.Combine(baseDir, "enemies", "reptilians", "names.json"),
        "troll_names.json" => Path.Combine(baseDir, "enemies", "trolls", "names.json"),
        "undead_names.json" => Path.Combine(baseDir, "enemies", "undead", "names.json"),
        "vampire_names.json" => Path.Combine(baseDir, "enemies", "vampires", "names.json"),
        
        // Material files
        "cloth.json" => Path.Combine(baseDir, "items", "materials", "cloth.json"),
        "gemstones.json" => Path.Combine(baseDir, "items", "materials", "gemstones.json"),
        "leathers.json" => Path.Combine(baseDir, "items", "materials", "leathers.json"),
        "metals.json" => Path.Combine(baseDir, "items", "materials", "metals.json"),
        "woods.json" => Path.Combine(baseDir, "items", "materials", "woods.json"),
        
        // Item files
        "armor_materials.json" => Path.Combine(baseDir, "items", "armor", "materials.json"),
        "armor_names.json" => Path.Combine(baseDir, "items", "armor", "names.json"),
        "consumable_names.json" => Path.Combine(baseDir, "items", "consumables", "names.json"),
        "enchantment_suffixes.json" => Path.Combine(baseDir, "items", "enchantments", "suffixes.json"),
        "weapon_names.json" => Path.Combine(baseDir, "items", "weapons", "names.json"),
        "weapon_prefixes.json" => Path.Combine(baseDir, "items", "weapons", "prefixes.json"),
        
        // NPC files (renamed - remove npc_ prefix)
        "npc_first_names.json" => Path.Combine(baseDir, "npcs", "names", "first_names.json"),
        "npc_last_names.json" => Path.Combine(baseDir, "npcs", "names", "last_names.json"),
        "npc_occupations.json" => Path.Combine(baseDir, "npcs", "occupations", "common.json"),
        
        // Quest files (renamed - remove quest_ prefix)
        "quest_templates.json" => Path.Combine(baseDir, "quests", "templates", "fetch.json"), // Note: Now split
        
        // Default: assume new structure path
        _ => Path.Combine(baseDir, fileName)
    };
}
```

**Important Notes**:
- ❌ **No fallback logic** - old paths will fail immediately
- ✅ **Clean mapping** from old names to new locations
- ⚠️ **Quest templates split** - code may need to load all 4 files instead of 1
- 🔍 **Find all usages** of old filenames and update to use new paths directly

### Step 5: Update ContentBuilder TreeView

**File**: `Game.ContentBuilder/ViewModels/MainViewModel.cs`

**Current TreeView Structure**:
```csharp
private void InitializeCategories()
{
    Categories.Add(new CategoryNode
    {
        Name = "Items",
        Children = new ObservableCollection<CategoryNode>
        {
            new CategoryNode { Name = "Weapon Prefixes", FilePath = "weapon_prefixes.json", EditorType = EditorType.ItemEditor },
            // ...
        }
    });
}
```

**New TreeView Structure**:
```csharp
private void InitializeCategories()
{
    Categories.Add(new CategoryNode
    {
        Name = "Items",
        Children = new ObservableCollection<CategoryNode>
        {
            new CategoryNode
            {
                Name = "Materials",
                Children = new ObservableCollection<CategoryNode>
                {
                    new CategoryNode { Name = "Metals", FilePath = "items/materials/metals.json", EditorType = EditorType.FlatItemEditor },
                    new CategoryNode { Name = "Woods", FilePath = "items/materials/woods.json", EditorType = EditorType.FlatItemEditor },
                    new CategoryNode { Name = "Leathers", FilePath = "items/materials/leathers.json", EditorType = EditorType.FlatItemEditor },
                    new CategoryNode { Name = "Gemstones", FilePath = "items/materials/gemstones.json", EditorType = EditorType.FlatItemEditor },
                    new CategoryNode { Name = "Cloth", FilePath = "items/materials/cloth.json", EditorType = EditorType.FlatItemEditor }
                }
            },
            new CategoryNode { Name = "Weapon Prefixes", FilePath = "items/weapon_prefixes.json", EditorType = EditorType.ItemEditor },
            new CategoryNode { Name = "Armor Materials", FilePath = "items/armor_materials.json", EditorType = EditorType.ItemEditor },
            // ...
        }
    });
    
    Categories.Add(new CategoryNode
    {
        Name = "Enemies",
        Children = new ObservableCollection<CategoryNode>
        {
            new CategoryNode { Name = "Beasts", FilePath = "enemies/beasts/names.json", EditorType = EditorType.NameListEditor },
            new CategoryNode { Name = "Demons", FilePath = "enemies/demons/names.json", EditorType = EditorType.NameListEditor },
            new CategoryNode { Name = "Dragons", FilePath = "enemies/dragons/names.json", EditorType = EditorType.NameListEditor },
            // ... all 13 enemy types
        }
    });
    
    Categories.Add(new CategoryNode
    {
        Name = "NPCs",
        Children = new ObservableCollection<CategoryNode>
        {
            new CategoryNode { Name = "First Names", FilePath = "npcs/first_names.json", EditorType = EditorType.NameListEditor },
            new CategoryNode { Name = "Last Names", FilePath = "npcs/last_names.json", EditorType = EditorType.NameListEditor },
            new CategoryNode { Name = "Occupations", FilePath = "npcs/occupations.json", EditorType = EditorType.NameListEditor }
        }
    });
    
    Categories.Add(new CategoryNode
    {
        Name = "Quests",
        Children = new ObservableCollection<CategoryNode>
        {
            new CategoryNode { Name = "Templates", FilePath = "quests/templates.json", EditorType = EditorType.NameListEditor }
        }
    });
}
```

### Step 6: Update ContentBuilder JsonDataService

**File**: `Game.ContentBuilder/Services/JsonDataService.cs`

**Update path resolution** to use new structure:

```csharp
private string GetJsonFilePath(string relativePath)
{
    // relativePath is now like "items/materials/metals.json" or "enemies/beasts/names.json"
    return Path.Combine(_jsonRootPath, relativePath);
}
```

### Step 7: Build and Test

**Build Commands**:
```powershell
# Clean build
dotnet clean

# Build all projects
dotnet build

# Run tests
dotnet test

# Run Game
dotnet run --project Game

# Run ContentBuilder
dotnet run --project Game.ContentBuilder
```

**Test Checklist**:
- [ ] All 3 projects build without errors
- [ ] All tests pass (Game.Tests)
- [ ] Game launches and loads data correctly
- [ ] ContentBuilder launches
- [ ] TreeView shows all 26 files in new structure
- [ ] Can edit and save files from ContentBuilder
- [ ] Preview system still works with all 16 content types
- [ ] Generators create items/enemies/NPCs correctly

---

## 🚀 Phase 2: Full Migration (FUTURE - DETAILED PLAN)

### Step 1: Create Deep Folder Structure

**New Folders** (beyond Phase 1):
```
items/
├── weapons/              # NEW
├── armor/                # NEW
├── consumables/          # NEW
└── enchantments/         # NEW

npcs/
├── names/                # NEW (move existing files here)
├── occupations/          # NEW (move existing file here)
├── personalities/        # NEW (future)
└── dialogue/             # NEW (future)

quests/
├── templates/            # NEW (move existing file here)
├── objectives/           # NEW (future)
├── rewards/              # NEW (future)
└── locations/            # NEW (future)

general/                  # NEW (all future)
```

### Step 2: Move Files to Deep Structure

**File Moves**:

| Phase 1 Path | Phase 2 Path |
|--------------|--------------|
| `items/weapon_prefixes.json` | `items/weapons/prefixes.json` |
| `items/weapon_names.json` | `items/weapons/names.json` |
| `items/armor_materials.json` | `items/armor/materials.json` |
| `items/armor_names.json` | `items/armor/names.json` |
| `items/consumable_names.json` | `items/consumables/names.json` |
| `items/enchantment_suffixes.json` | `items/enchantments/suffixes.json` |
| `npcs/first_names.json` | `npcs/names/first_names.json` |
| `npcs/last_names.json` | `npcs/names/last_names.json` |
| `npcs/occupations.json` | `npcs/occupations/common.json` |
| `quests/templates.json` | `quests/templates/all.json` (or split by type) |

### Step 3: Create Future Expansion Files

**Placeholder Files to Create** (empty or with sample data):

```json
// items/weapons/suffixes.json (future)
{
  "common": [],
  "uncommon": [],
  "rare": [],
  "legendary": []
}

// enemies/beasts/prefixes.json (future)
{
  "common": ["Wild", "Feral", "Hungry"],
  "uncommon": ["Rabid", "Alpha", "Pack"],
  "rare": ["Ancient", "Dire", "Legendary"]
}

// npcs/personalities/traits.json (future)
[
  "Friendly",
  "Grumpy",
  "Wise",
  "Mysterious",
  "Cheerful"
]

// general/colors.json (future)
[
  "crimson",
  "azure",
  "emerald",
  "golden",
  "silver"
]
```

### Step 4: Update All Code References

**Files to Update**:
1. `Game.Shared/Services/GameDataService.cs` - Update all path mappings
2. `Game.ContentBuilder/ViewModels/MainViewModel.cs` - Update TreeView structure
3. `Game.ContentBuilder/Services/JsonDataService.cs` - Update path resolution
4. `Game/Generators/*.cs` - Update all generator path references
5. `Game.Shared.csproj` - Update all `<None Update>` entries

### Step 5: Build, Test, Document

**Same as Phase 1** but more extensive testing required.

---

## 🧪 Testing Strategy

### Unit Tests to Add

**New Test File**: `Game.Tests/Services/GameDataServicePathTests.cs`

```csharp
public class GameDataServicePathTests
{
    [Theory]
    [InlineData("beast_names.json", "enemies/beasts/names.json")]
    [InlineData("metals.json", "items/materials/metals.json")]
    [InlineData("npc_first_names.json", "npcs/first_names.json")]
    [InlineData("quest_templates.json", "quests/templates.json")]
    public void GetNewStructurePath_MapsOldFilenamesToNewPaths(string oldFilename, string expectedNewPath)
    {
        // Test that path mapping works correctly
    }
    
    [Fact]
    public void LoadData_LoadsFromNewStructure()
    {
        // Test that data loads correctly from new paths
    }
    
    [Fact]
    public void LoadData_FallsBackToOldStructure_WhenNewNotFound()
    {
        // Test backward compatibility
    }
}
```

### Integration Tests

**Test Scenarios**:
1. **Game Startup**: Verify all JSON files load correctly
2. **Item Generation**: Verify ItemGenerator creates items with new paths
3. **Enemy Generation**: Verify EnemyGenerator creates enemies with new paths
4. **NPC Generation**: Verify NpcGenerator creates NPCs with new paths
5. **Quest Generation**: Verify QuestGenerator creates quests with new paths
6. **ContentBuilder**: Verify all files appear in TreeView
7. **Edit & Save**: Verify editing and saving works with new paths
8. **Preview System**: Verify preview generates content correctly

### Manual Testing Checklist

#### Phase 1 Testing
- [ ] **Game Launch**
  - [ ] Game starts without errors
  - [ ] No file not found errors in logs
  - [ ] Main menu displays correctly

- [ ] **Item Generation**
  - [ ] Weapons generate with correct materials (metals, woods)
  - [ ] Armor generates with correct materials (leathers, metals, cloth)
  - [ ] Consumables generate with correct names
  - [ ] Enchanted items generate with suffixes

- [ ] **Enemy Generation**
  - [ ] All 13 enemy types generate correctly
  - [ ] Enemy names load from new paths
  - [ ] Enemy traits apply correctly

- [ ] **NPC Generation**
  - [ ] NPCs generate with first + last names
  - [ ] Occupations load correctly
  - [ ] NPC dialogue works

- [ ] **Quest Generation**
  - [ ] Quests generate from templates
  - [ ] Quest objectives populate
  - [ ] Quest rewards work

- [ ] **ContentBuilder Launch**
  - [ ] ContentBuilder starts without errors
  - [ ] TreeView shows new folder structure
  - [ ] All 26 files appear in tree
  - [ ] Folders expand/collapse correctly

- [ ] **ContentBuilder Editing**
  - [ ] Can select and open any file
  - [ ] Correct editor loads for file type
  - [ ] Can edit file contents
  - [ ] Save creates backup
  - [ ] Save updates JSON file
  - [ ] Reload discards changes

- [ ] **ContentBuilder Preview**
  - [ ] Preview window opens
  - [ ] All 16 content types generate
  - [ ] Generated content uses new paths
  - [ ] Copy All works

#### Phase 2 Testing
- [ ] All Phase 1 tests still pass
- [ ] New deep folder structure works
- [ ] Future expansion files load
- [ ] All path references updated

---

## 🔄 Rollback Plan

### If Migration Fails

**Option 1: Git Revert**
```powershell
git reset --hard HEAD~1
```

**Option 2: Manual Rollback**
```powershell
# Move files back to flat structure
# Restore old Game.Shared.csproj
# Restore old GameDataService.cs
# Restore old MainViewModel.cs (ContentBuilder)
```

**Option 3: Keep Both Structures Temporarily**
- Keep old flat files in place
- Add new folder structure alongside
- Use fallback logic in GameDataService
- Remove old files only after thorough testing

---

## ✅ Execution Checklist

### Pre-Migration
- [ ] Commit all current changes to git
- [ ] Create backup of `Game.Shared/Data/Json/` folder
- [ ] Document current file count (26 files)
- [ ] Run full test suite (all tests passing)
- [ ] Build all projects (zero warnings)

### Phase 1 Execution
- [ ] Create new folder structure (18 folders)
- [ ] Move all 26 files to new locations
- [ ] Rename 4 files (remove npc_/quest_ prefixes)
- [ ] Update `Game.Shared.csproj` with new paths
- [ ] Update `GameDataService.cs` with path mapping
- [ ] Update ContentBuilder `MainViewModel.cs` TreeView
- [ ] Update ContentBuilder `JsonDataService.cs` paths
- [ ] Build all projects (verify zero errors)
- [ ] Run test suite (verify all pass)
- [ ] Manual test: Launch Game
- [ ] Manual test: Generate items/enemies/NPCs
- [ ] Manual test: Launch ContentBuilder
- [ ] Manual test: Edit and save files
- [ ] Manual test: Preview system
- [ ] Commit changes with detailed message
- [ ] Update documentation

### Phase 2 Execution (Future)
- [ ] Review Phase 1 success
- [ ] Create Phase 2 branch
- [ ] Create deep folder structure
- [ ] Move files to deep structure
- [ ] Create future expansion files
- [ ] Update all code references
- [ ] Build all projects
- [ ] Run test suite
- [ ] Manual testing (all scenarios)
- [ ] Commit changes
- [ ] Merge to main

### Post-Migration
- [ ] Update README.md with new structure
- [ ] Update CONTENT_BUILDER_MVP.md
- [ ] Create migration summary document
- [ ] Delete old flat files (if keeping both)
- [ ] Tag release in git

---

## 📊 Success Metrics

### Phase 1 Success Criteria
- ✅ All 26 files moved to new locations
- ✅ Zero compiler warnings/errors
- ✅ All tests passing (1559+ tests)
- ✅ Game launches and generates content correctly
- ✅ ContentBuilder shows new tree structure
- ✅ Can edit and save all 26 files
- ✅ Preview system generates all 16 content types
- ✅ Build time < 5 seconds
- ✅ No performance regressions

### Phase 2 Success Criteria
- ✅ All Phase 1 criteria still met
- ✅ Deep folder structure created
- ✅ Future expansion files in place
- ✅ All code references updated
- ✅ Documentation updated
- ✅ Ready for modding/DLC

---

## 📝 Notes

### Design Decisions

**Why separate materials from item templates?**
- Materials are **raw resources** (what exists in the game world)
- Templates define **how materials become items** (stats, bonuses)
- Names define **what the item is called** (final product)
- This separation enables crafting, trading, and flexible item generation

**Why enemy subfolders?**
- Each enemy type can have unique properties (prefixes, suffixes, traits)
- Clear organization for future expansion
- Easy to add new enemy types
- Modding-friendly structure

**Why rename NPC/Quest files?**
- Remove redundant prefixes (`npc_`, `quest_`)
- Folder structure already indicates category
- Cleaner file names
- Consistent with other categories

**Prefix/Suffix Pairing Consistency**:
- ✅ **Design Rule**: Any category with `prefixes.json` MUST also have `suffixes.json`
- **Rationale**: Symmetry in naming patterns (prefix + base + suffix)
- **Examples**:
  - Enemies: "Rabid [Beast] of the Wild" → `prefixes.json` + `names.json` + `suffixes.json`
  - Items: "Flaming [Longsword] of Fire" → `prefixes.json` + `names.json` + `suffixes.json`
- **Applies to**: All 13 enemy types, weapons, armor (if we add prefixes later)
- **Future-Proof**: Even if suffixes are empty initially, the file exists for expansion

**Quest Templates Split**:
- Split `quest_templates.json` into 4 files by quest type (fetch, kill, escort, explore)
- Each file contains templates specific to that quest type
- Easier to add new quest types (just add new file)
- Cleaner than one massive templates file

**NPC Occupations Split**:
- Split `npc_occupations.json` into categories (common, noble, criminal, magical)
- Common occupations go in `common.json` (existing data)
- Future categories ready for expansion (noble.json, criminal.json, magical.json)
- Enables NPC class-based generation

### Future Considerations

**Modding Support**:
- Easy to add new folders for mod content
- Clear structure for community contributions
- Can load mods from separate `Mods/` folder

**DLC/Expansions**:
- New content fits into existing structure
- e.g., `enemies/demons/dlc1_names.json`
- Can version content by DLC

**Localization**:
- Can add `Data/Json/en-US/`, `Data/Json/es-ES/`, etc.
- Same structure, different languages

**Procedural Generation**:
- `general/` folder enables complex text generation
- Combine colors, adjectives, materials for unique descriptions
- "A crimson, ancient mithril longsword of fire"

**ContentBuilder Enhancements (Post-Migration)**:
- ✨ **File Creation**: Add "New File" button to create new JSON files
- ✨ **File Deletion**: Right-click to delete files from TreeView
- ✨ **Template System**: Create files from templates (e.g., "New Enemy Type")
- ✨ **Duplicate Detection**: Warn when creating duplicate entries
- ✨ **Batch Operations**: Apply changes across multiple files
- ✨ **Import/Export**: Import community content, export content packs
- ✨ **Search Across Files**: Find text across all JSON files
- ✨ **Validation Rules**: Define custom validation per file type

**Note**: The new folder structure makes these enhancements easier to implement. For example:
- Creating a new enemy type? Just create `enemies/newtype/` folder with standard files
- Adding a new material category? Create `items/materials/newmaterial.json`
- The consistent structure enables template-based file creation

---

## 🎯 Execution Instructions

**When ready to execute Phase 1**:

1. Review this entire document
2. Ensure all current work is committed
3. Create new branch: `git checkout -b json-reorganization-phase1`
4. Follow Step 1-7 in "Phase 1: Minimal Reorganization"
5. Run full test suite after each step
6. Commit frequently with descriptive messages
7. When all tests pass, create pull request
8. After review, merge to main
9. Tag release: `v1.1.0-reorganized-json`

**Ready to execute?** Let me know and I'll start implementing Phase 1!

---

**Document Version**: 1.0  
**Last Updated**: December 14, 2025  
**Status**: 📋 Planning Complete - Ready for Execution
