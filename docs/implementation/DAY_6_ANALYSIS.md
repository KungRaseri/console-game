# Day 6: Enemy, NPC & Quest Editor Analysis

**Date**: December 14, 2025  
**Status**: 🔄 In Progress  
**Goal**: Analyze all enemy, NPC, and quest JSON files to determine editor requirements

---

## File Inventory

### Enemy Files (13 files)
**Location**: `RealmEngine.Shared/Data/Json/enemies/`

| File | Type | Structure Analysis |
|------|------|-------------------|
| beast_names.json | Names | Array structure: `{ "prefixes": [], "creatures": [] }` |
| beast_prefixes.json | Prefixes | ? (need to check) |
| demon_names.json | Names | ? (need to check) |
| demon_prefixes.json | Prefixes | ? (need to check) |
| dragon_colors.json | Colors | ? (need to check) |
| dragon_names.json | Names | ? (need to check) |
| dragon_prefixes.json | Prefixes | ? (need to check) |
| elemental_names.json | Names | ? (need to check) |
| elemental_prefixes.json | Prefixes | ? (need to check) |
| humanoid_names.json | Names | ? (need to check) |
| humanoid_prefixes.json | Prefixes | ? (need to check) |
| undead_names.json | Names | ? (need to check) |
| undead_prefixes.json | Prefixes | ? (need to check) |

### NPC Files (4 files)
**Location**: `RealmEngine.Shared/Data/Json/npcs/`

| File | Type | Structure Analysis |
|------|------|-------------------|
| fantasy_names.json | Names | Array structure: `{ "male": [], "female": [] }` ✅ |
| occupations.json | Occupations | 3-level structure: `{ "category": { "item": { "displayName": "", "traits": {} } } }` ✅ |
| dialogue_templates.json | Dialogue | ? (need to check) |
| dialogue_traits.json | Traits | ? (need to check) |

### Quest Files (1 file)
**Location**: `RealmEngine.Shared/Data/Json/quests/`

| File | Type | Structure Analysis |
|------|------|-------------------|
| quest_templates.json | Templates | 3-level structure: `{ "difficulty": { "item": { "displayName": "", "traits": {} } } }` ✅ |

---

## Initial Findings

### beast_names.json Structure
```json
{
  "prefixes": [
    "Dire",
    "Wild",
    "Rabid",
    ...
  ],
  "creatures": [
    "Wolf",
    "Bear",
    "Boar",
    ...
  ]
}
```
**Editor**: NameListEditor (array structure) ✅ **CAN REUSE**

### fantasy_names.json Structure
```json
{
  "male": [
    "Aldric",
    "Theron",
    ...
  ],
  "female": [
    "Seraphina",
    "Lyra",
    ...
  ]
}
```
**Editor**: NameListEditor (array structure) ✅ **CAN REUSE**

### occupations.json Structure
```json
{
  "merchants": {
    "Merchant": {
      "displayName": "Merchant",
      "traits": {
        "shopDiscount": { "value": 5, "type": "number" },
        "sellPriceBonus": { "value": 5, "type": "number" },
        ...
      }
    }
  }
}
```
**Editor**: ItemEditor (3-level hierarchy) ✅ **CAN REUSE**

### quest_templates.json Structure
```json
{
  "Kill": {
    "Easy": {
      "SlayBeasts": {
        "displayName": "Slay the Beasts",
        "traits": {
          "questType": { "value": "kill", "type": "string" },
          "difficulty": { "value": "easy", "type": "string" },
          ...
        }
      }
    }
  }
}
```
**Editor**: ItemEditor (3-level hierarchy) ✅ **CAN REUSE**

---

## Next Steps

1. ✅ Check beast_names.json - NameListEditor compatible
2. ✅ Check fantasy_names.json - NameListEditor compatible
3. ✅ Check occupations.json - ItemEditor compatible
4. ✅ Check quest_templates.json - ItemEditor compatible
5. ⏳ Check remaining 11 enemy files for structure patterns
6. ⏳ Check 2 dialogue files for structure patterns
7. ⏳ Map all files to appropriate editors
8. ⏳ Update TreeView with all files
9. ⏳ Test each editor type

---

## Editor Mapping Strategy

Based on initial analysis, we have 3 editor types already built:

### NameListEditor (Array Structure)
**Use For**:
- beast_names.json (prefixes, creatures) ✅
- fantasy_names.json (male, female) ✅
- Potentially: All other *_names.json files
- Potentially: All *_prefixes.json files if they're arrays

### ItemEditor (3-Level Hierarchy)
**Use For**:
- occupations.json (category → item → traits) ✅
- quest_templates.json (type → difficulty → template → traits) ✅
- Potentially: dialogue files if they have traits

### FlatItemEditor (2-Level Flat)
**Use For**:
- Currently: metals, woods, leathers, gemstones
- Potentially: dragon_colors.json if it has traits but no category

---

## Expected Outcome

**Goal**: Reuse existing 3 editors for all 18 files (13 enemy + 4 NPC + 1 quest)

**No new editors needed** if structures match existing patterns!

This would mean:
- **100% code reuse** (only TreeView configuration changes)
- **Fast implementation** (hours instead of days)
- **Consistent UX** (users learn once, apply everywhere)

---

## Status

**Current Task**: Checking remaining file structures  
**Files Analyzed**: 4/18 (22%)  
**Files Remaining**: 14/18 (78%)
