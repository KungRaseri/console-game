# Complete ContentBuilder Structure Audit

**Date:** December 16, 2025  
**Status:** 🔧 Analysis Complete

## Structure Analysis

### Current Issues Found

| Category | File | Current Structure | Should Be | Reason |
|----------|------|------------------|-----------|---------|
| **Enemy Names** |
| enemies/beasts/names.json | Unknown | HybridArray | Has components: prefixes, creatures, variants |
| enemies/demons/names.json | Unknown | HybridArray | Has components: prefixes, creatures, variants |
| enemies/dragons/names.json | Unknown | HybridArray | Has components: prefixes, types, colors, variants |
| enemies/elementals/names.json | Unknown | HybridArray | Has components: prefixes, creatures, variants |
| enemies/humanoids/names.json | Unknown | HybridArray | Has components: factions, roles, professions, variants |
| enemies/undead/names.json | Unknown | HybridArray | Has components: prefixes, creatures, variants |
| **General** |
| general/adjectives.json | Unknown | HybridArray | Has components: positive, negative, size, appearance, condition |
| general/materials.json | Unknown | HybridArray | Has components: metals, natural, precious, magical |
| **Items** |
| items/armor/materials.json | RarityBased | **ItemPrefix** | Has rarity → item → traits structure |
| **NPCs** |
| npcs/dialogue/templates.json | Unknown | HybridArray | Has components: greetings, farewells, quests, merchants, etc. |
| npcs/names/first_names.json | Unknown | HybridArray | Has components: male, female, patterns, metadata |
| npcs/occupations/common.json | RarityBased | HybridArray | Mixed structure with categories |
| npcs/occupations/criminal.json | Unknown | FlatItem? | Single item structure |
| npcs/occupations/magical.json | Unknown | FlatItem? | Single item structure |
| npcs/occupations/noble.json | Unknown | FlatItem? | Single item structure |
| **Quests** |
| quests/templates/*.json | Unknown | Need to check | Difficulty-based structure |

## Recommended Structure Assignments

### Pattern 1: Names with Components
**Structure:** HybridArray  
**Files:**
- All enemy name files (beasts, demons, dragons, elementals, humanoids, undead)
- npcs/names/first_names.json
- npcs/names/last_names.json (already correct)

**Example:**
```json
{
  "components": {
    "prefixes": [...],
    "creatures": [...],
    "variants": [...]
  },
  "patterns": [
    "prefix + creature",
    "creature + variant"
  ]
}
```

### Pattern 2: Grouped String Arrays
**Structure:** HybridArray  
**Files:**
- general/adjectives.json
- general/materials.json
- npcs/dialogue/templates.json

**Example:**
```json
{
  "components": {
    "positive": [...],
    "negative": [...],
    "size": [...]
  }
}
```

### Pattern 3: Rarity-Based Items with Traits
**Structure:** ItemPrefix (or FlatItem if no rarity)  
**Files:**
- items/armor/materials.json (has rarity levels)

**Example:**
```json
{
  "common": {
    "ItemName": {
      "displayName": "...",
      "traits": {...}
    }
  }
}
```

## Complete Corrected Mapping

### General
- ✅ colors.json → HybridArray
- ✅ smells.json → HybridArray
- ✅ sounds.json → HybridArray
- ✅ textures.json → HybridArray
- ✅ time_of_day.json → HybridArray
- ✅ verbs.json → HybridArray
- ✅ weather.json → HybridArray
- 🔧 adjectives.json → NameList → **HybridArray**
- 🔧 materials.json → NameList → **HybridArray**

### Items - Weapons
- ✅ names.json → HybridArray
- ✅ prefixes.json → ItemPrefix
- ✅ suffixes.json → HybridArray

### Items - Armor
- ✅ names.json → HybridArray
- ✅ prefixes.json → HybridArray
- ✅ suffixes.json → HybridArray
- 🔧 materials.json → FlatItem → **ItemPrefix** (has rarity levels!)

### Items - Consumables
- ✅ names.json → HybridArray
- ✅ effects.json → HybridArray
- ✅ rarities.json → HybridArray

### Items - Enchantments
- ✅ prefixes.json → HybridArray
- ✅ effects.json → HybridArray
- ✅ suffixes.json → ItemSuffix

### Items - Materials
- ✅ metals.json → FlatItem
- ✅ woods.json → FlatItem
- ✅ leathers.json → FlatItem
- ✅ gemstones.json → FlatItem

### Enemies - All Types
Pattern for all enemy types:
- 🔧 names.json → NameList → **HybridArray** (has components!)
- ✅ prefixes.json → ItemPrefix
- ✅ traits.json → HybridArray
- ✅ suffixes.json → HybridArray

Affected files:
- enemies/beasts/names.json
- enemies/demons/names.json
- enemies/dragons/names.json
- enemies/elementals/names.json
- enemies/humanoids/names.json
- enemies/undead/names.json

### NPCs - Dialogue
- ✅ farewells.json → HybridArray
- ✅ greetings.json → HybridArray
- ✅ rumors.json → HybridArray
- 🔧 templates.json → None → **HybridArray**
- ✅ traits.json → FlatItem

### NPCs - Names
- 🔧 first_names.json → None → **HybridArray**
- ✅ last_names.json → HybridArray

### NPCs - Occupations
- 🔧 common.json → None → **HybridArray**
- 🔧 criminal.json → None → **HybridArray**
- 🔧 magical.json → None → **HybridArray**
- 🔧 noble.json → None → **HybridArray**

### NPCs - Personalities
- ✅ backgrounds.json → HybridArray
- ✅ quirks.json → HybridArray
- ✅ traits.json → HybridArray

### Quests - Locations
- ✅ dungeons.json → HybridArray
- ✅ towns.json → HybridArray
- ✅ wilderness.json → HybridArray

### Quests - Objectives
- ✅ hidden.json → HybridArray
- ✅ primary.json → HybridArray
- ✅ secondary.json → HybridArray

### Quests - Rewards
- ✅ experience.json → HybridArray
- ✅ gold.json → HybridArray
- ✅ items.json → HybridArray

### Quests - Templates
- 🔧 delivery.json → None → **Need to check structure**
- 🔧 escort.json → None → **Need to check structure**
- 🔧 fetch.json → None → **Need to check structure**
- 🔧 investigate.json → None → **Need to check structure**
- 🔧 kill.json → None → **Need to check structure**

## Summary of Required Changes

**Total files needing updates:** 19

1. ✅ Already correct: 74 files
2. 🔧 Need fixing: 19 files

### Changes Needed:
1. General (2 files) - adjectives, materials
2. Items/Armor (1 file) - materials  
3. Enemies names (6 files) - all enemy name files
4. NPCs Dialogue (1 file) - templates
5. NPCs Names (1 file) - first_names
6. NPCs Occupations (4 files) - common, criminal, magical, noble
7. Quests Templates (5 files) - need to inspect structure

## Next Steps

1. ✅ Verify quest template structures
2. ✅ Update MainViewModel.cs with correct editor assignments
3. ✅ Test each changed category in ContentBuilder
4. ✅ Document the canonical patterns
