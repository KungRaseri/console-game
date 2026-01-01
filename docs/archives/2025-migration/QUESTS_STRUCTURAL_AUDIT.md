# Quests Domain - Structural Audit Report

**Date**: December 29, 2025  
**Auditor**: GitHub Copilot (Claude Sonnet 4.5)  
**Scope**: Complete analysis of quests domain for JSON v4.0/v4.1 compliance, reference standards, and organizational structure

---

## Executive Summary

The quests domain is **partially organized** with recent v4.0 consolidation work completed, but requires:
1. ✅ **DUPLICATE FILE CLEANUP** - quest_templates.json duplicates catalog.json content (480 lines)
2. ⚠️ **REFERENCE SYSTEM NEEDED** - No @domain/ references (items, enemies, NPCs hardcoded)
3. ⚠️ **SPLIT SUBDIRECTORY FILES** - objectives/, rewards/, locations/ have both root AND subdirectory versions
4. ✅ **JSON v4.0 COMPLIANT** - All files follow metadata standards
5. ⚠️ **MISSING STANDARDS** - No quest_giver assignments, no faction requirements

---

## Current File Structure (18 JSON files, ~4,218 total lines)

### Root Files (3)
```
quests/
├── catalog.json (1,082 lines) - Consolidated templates + locations ✅
├── objectives.json (563 lines) - Consolidated primary/secondary/hidden ✅
└── rewards.json (480 lines) - Consolidated items/gold/experience ✅
```

### Subdirectory Files (11 + 4 .cbconfig)
```
quests/
├── templates/
│   ├── quest_templates.json (480 lines) ⚠️ DUPLICATE of catalog.json
│   └── .cbconfig.json
├── objectives/
│   ├── primary.json (229 lines) ⚠️ DUPLICATE of objectives.json
│   ├── secondary.json (205 lines) ⚠️ DUPLICATE of objectives.json
│   ├── hidden.json (184 lines) ⚠️ DUPLICATE of objectives.json
│   └── .cbconfig.json
├── rewards/
│   ├── items.json (222 lines) ⚠️ DUPLICATE of rewards.json
│   ├── gold.json (153 lines) ⚠️ DUPLICATE of rewards.json
│   ├── experience.json (156 lines) ⚠️ DUPLICATE of rewards.json
│   └── .cbconfig.json
└── locations/
    ├── wilderness.json (195 lines) ⚠️ DUPLICATE of catalog.json
    ├── towns.json (179 lines) ⚠️ DUPLICATE of catalog.json
    ├── dungeons.json (274 lines) ⚠️ DUPLICATE of catalog.json
    └── .cbconfig.json
```

---

## 🔍 Key Findings

### 1. DUPLICATE FILES ISSUE ⚠️ CRITICAL

**Problem**: Consolidation to v4.0 created root files BUT left old subdirectory files intact

| Root File | Subdirectory Duplicates | Total Duplicate Lines |
|-----------|------------------------|----------------------|
| catalog.json (1,082) | quest_templates.json (480), wilderness.json (195), towns.json (179), dungeons.json (274) | 1,128 lines |
| objectives.json (563) | primary.json (229), secondary.json (205), hidden.json (184) | 618 lines |
| rewards.json (480) | items.json (222), gold.json (153), experience.json (156) | 531 lines |

**Total Duplication**: ~2,277 lines (54% of total codebase!)

**Evidence from catalog.json metadata**:
```json
"notes": "Consolidated from quest_templates.json, wilderness.json, towns.json, 
         and dungeons.json..."
```

**Recommendation**: DELETE all subdirectory JSON files (keep only .cbconfig.json)

---

### 2. JSON v4.0 Compliance ✅ GOOD

**All 3 root files follow JSON v4.0 standards**:

#### catalog.json ✅
```json
{
  "metadata": {
    "version": "4.0",
    "lastUpdated": "2025-12-18",
    "type": "hierarchical_catalog",
    "description": "Quest templates and location catalog..."
  }
}
```

#### objectives.json ✅
```json
{
  "metadata": {
    "version": "4.0",
    "lastUpdated": "2025-12-18",
    "type": "hierarchical_catalog",
    "description": "Quest objectives catalog..."
  }
}
```

#### rewards.json ✅
```json
{
  "metadata": {
    "version": "4.0",
    "lastUpdated": "2025-12-18",
    "type": "hierarchical_catalog",
    "description": "Quest rewards catalog..."
  }
}
```

**Status**: ✅ All root files compliant, subdirectory files need validation before deletion

---

### 3. Reference System Analysis ⚠️ NEEDS WORK

**Current State**: NO @domain/ references found in any quest files

#### Missing References by Domain

**Items Domain** (should use `@items/...`):
- catalog.json: `"itemType": "herb"` → Should be `"itemType": "@items/consumables/herbs:healing-herb"`
- rewards.json: `"description": "Random uncommon quality weapon"` → Should reference specific item pool
- objectives.json: `"Recover the {item_name}"` → Should support `@items/...` syntax

**Enemies Domain** (should use `@enemies/...`):
- catalog.json: `"targetType": "goblin"` (hardcoded)
- objectives.json: `"Defeat {enemy_name}"` → Should use `@enemies/humanoid:goblin-warrior`

**NPCs Domain** (should use `@npcs/...`):
- **MISSING ENTIRELY**: No quest_giver field
- **MISSING**: No faction_requirement field
- **MISSING**: No reputation_required field

**Locations Domain** (NEW - needs @locations/...):
- catalog.json has 51 locations but no reference system
- Currently: `"location": "Wilderness"` (string)
- Should be: `"location": "@locations/wilderness/forests:dark-forest"`

#### Reference Opportunities

| Quest Field | Current Format | Should Use |
|-------------|---------------|-----------|
| itemType | "herb" (string) | @items/consumables/herbs |
| targetType | "goblin" (string) | @enemies/humanoid:goblin-warrior |
| location | "Wilderness" (string) | @locations/wilderness/forests:dark-forest |
| quest_giver | (missing) | @npcs/merchants:general-merchant |
| item_reward | "UncommonWeapon" (string) | @items/weapons/swords:iron-longsword |

---

### 4. Organizational Comparison

#### vs NPCs Domain ✅ (Recently Reorganized)
- **NPCs**: 10 subdirectories by social class → ✅ EXCELLENT
- **Quests**: Flat root files + duplicate subdirs → ⚠️ NEEDS CLEANUP

#### vs Enemies Domain ✅ (Well Organized)
- **Enemies**: 13 subdirectories by creature type → ✅ EXCELLENT
- **Quests**: 3 root files + 4 duplicate subdirs → ⚠️ ACCEPTABLE (after cleanup)

#### vs Items Domain ✅ (Well Organized)
- **Items**: 5 subdirectories by item category → ✅ EXCELLENT
- **Quests**: Single catalog approach → ✅ ACCEPTABLE (quests are fewer)

**Recommendation**: Keep 3 root files (catalog, objectives, rewards) as primary structure. Quests don't need subdirectories like NPCs/enemies/items because:
- Only 27 quest templates (vs 56 NPCs, 100+ enemies)
- Quest types naturally group in single file (fetch, kill, escort, etc.)
- Locations/objectives/rewards are support data, not main entities

---

### 5. File Size Analysis

| File | Lines | Status | Action Needed |
|------|-------|--------|---------------|
| catalog.json | 1,082 | ✅ Manageable | Keep as primary |
| objectives.json | 563 | ✅ Good | Keep as primary |
| rewards.json | 480 | ✅ Good | Keep as primary |
| quest_templates.json | 480 | ⚠️ DUPLICATE | DELETE |
| primary.json | 229 | ⚠️ DUPLICATE | DELETE |
| items.json | 222 | ⚠️ DUPLICATE | DELETE |
| secondary.json | 205 | ⚠️ DUPLICATE | DELETE |
| wilderness.json | 195 | ⚠️ DUPLICATE | DELETE |
| hidden.json | 184 | ⚠️ DUPLICATE | DELETE |
| towns.json | 179 | ⚠️ DUPLICATE | DELETE |
| experience.json | 156 | ⚠️ DUPLICATE | DELETE |
| gold.json | 153 | ⚠️ DUPLICATE | DELETE |
| dungeons.json | 274 | ⚠️ DUPLICATE | DELETE |

**Recommendation**: Delete 10 duplicate JSON files, keep 3 root files + 4 .cbconfig.json

---

### 6. Missing Features Analysis

#### Quest Givers (MISSING) ⚠️
NPCs should be able to give quests, but there's no assignment system:
```json
// SHOULD HAVE:
{
  "name": "SlayDragon",
  "quest_giver": "@npcs/nobles:knight-errant",
  "quest_giver_types": ["noble", "military", "priest"],
  "min_reputation": 50
}
```

#### Faction Requirements (MISSING) ⚠️
Quests should require faction standing:
```json
// SHOULD HAVE:
{
  "name": "ThievesGuildHeist",
  "required_faction": "thieves_guild",
  "min_reputation": 25,
  "blocks_factions": ["city_guard", "merchants_guild"]
}
```

#### Location References (MISSING) ⚠️
Locations are strings, should be references:
```json
// CURRENT:
"location": "Wilderness"

// SHOULD BE:
"location": "@locations/wilderness/forests:dark-forest"
```

#### Item Rewards (MISSING REFERENCES) ⚠️
Item rewards are descriptive strings, not references:
```json
// CURRENT:
{
  "name": "UncommonWeapon",
  "description": "Random uncommon quality weapon"
}

// SHOULD BE:
{
  "name": "UncommonWeapon",
  "item_pool": [
    "@items/weapons/swords:iron-longsword",
    "@items/weapons/axes:battle-axe",
    "@items/weapons/maces:warhammer"
  ],
  "rarityFilter": "uncommon",
  "selectRandom": true
}
```

---

### 7. Quest Types Distribution

**From catalog.json (27 templates)**:
- fetch: 6 templates (easy=3, medium=2, hard=1)
- kill: 6 templates (easy=2, medium=2, hard=2)
- escort: 5 templates (easy=2, medium=2, hard=1)
- delivery: 5 templates (easy=2, medium=2, hard=1)
- investigate: 5 templates (easy=2, medium=2, hard=1)

**Distribution**: ✅ Well balanced across difficulty levels

---

### 8. Location Distribution

**From catalog.json (51 locations)**:

| Category | Count | Danger Levels |
|----------|-------|--------------|
| Wilderness | 16 | Low (4), Medium (5), High (4), Very High (3) |
| Towns | 19 | Outposts (4), Villages (5), Towns (4), Cities (3), Capitals (2), Special (1) |
| Dungeons | 16 | Easy (3), Medium (3), Hard (3), Very Hard (3), Epic (2), Legendary (2) |

**Total**: 51 locations across 3 categories ✅ Good variety

---

## 🎯 Recommendations

### Priority 1: CLEANUP DUPLICATES (1-2 hours) ⚠️ CRITICAL
1. **DELETE** 10 duplicate JSON files:
   - templates/quest_templates.json
   - objectives/primary.json, secondary.json, hidden.json
   - rewards/items.json, gold.json, experience.json
   - locations/wilderness.json, towns.json, dungeons.json
2. **KEEP** 4 .cbconfig.json files (templates, objectives, rewards, locations)
3. **UPDATE** .cbconfig.json descriptions to note consolidation
4. **BUILD VALIDATION** after deletion

### Priority 2: ADD REFERENCE SYSTEM (3-4 hours) ⚠️ HIGH
1. **Quest Givers**: Add `quest_giver`, `quest_giver_types`, `min_reputation` fields
2. **Item References**: Replace item strings with @items/ references
3. **Enemy References**: Replace enemy strings with @enemies/ references
4. **Location References**: Create @locations/ reference system (NEW)
5. **Faction Requirements**: Add faction fields using npcs/relationships.json

### Priority 3: JSON V4.1 VALIDATION (30 minutes) ✅ LOW
- Verify all 3 root files have correct metadata
- Update lastUpdated timestamps to "2025-12-29"
- Add componentKeys if missing

### Priority 4: NEW FILE TYPE ANALYSIS (1 hour) 🔍
- **locations.json**: New catalog needed for location references
- **quest_chains.json**: Define quest series (Act 1, Act 2, etc.)
- **quest_requirements.json**: Prerequisites and unlocks

### Priority 5: DOCUMENTATION (30 minutes) 📖
- Create QUESTS_AUDIT_REPORT.md (this file)
- Create QUESTS_V4_MIGRATION_GUIDE.md
- Update existing docs with reference standards

---

## 📊 Metrics Summary

### Current State
- **Total Files**: 18 JSON files (14 data + 4 config)
- **Total Lines**: ~4,218 lines
- **Duplicate Lines**: ~2,277 lines (54%)
- **JSON v4.0 Compliance**: 3/3 root files ✅
- **Reference System**: 0% coverage ⚠️

### After Cleanup (Projected)
- **Total Files**: 8 JSON files (4 data + 4 config)
- **Total Lines**: ~2,125 lines (50% reduction)
- **Duplicate Lines**: 0 lines (0%)
- **JSON v4.0 Compliance**: 3/3 root files ✅
- **Reference System**: TBD (after Phase 3)

---

## 🚀 Next Steps

### Immediate Actions
1. ✅ **Create this audit report** (DONE)
2. ⏸️ **Get user approval** for duplicate deletion
3. ⏸️ **Delete duplicate files** (10 files)
4. ⏸️ **Update .cbconfig.json** descriptions
5. ⏸️ **Build validation** (verify no breakage)

### Follow-Up Work
6. ⏸️ **Create locations.json** (new catalog for @locations/ references)
7. ⏸️ **Add quest_giver fields** to all quest templates
8. ⏸️ **Add faction_requirement fields** where appropriate
9. ⏸️ **Replace item/enemy strings** with @domain/ references
10. ⏸️ **Create quest chains catalog** (optional, for story mode)

---

## 💡 Key Insights

### What Went Right ✅
1. **Recent v4.0 work consolidated** scattered files into 3 root catalogs
2. **Metadata standards followed** on all root files
3. **Good quest variety** (27 templates, 51 locations)
4. **Balanced difficulty distribution** across quest types

### What Needs Work ⚠️
1. **Duplicate files not deleted** after consolidation (2,277 wasted lines)
2. **No reference system** - everything is hardcoded strings
3. **Missing quest_giver assignments** - can't link quests to NPCs
4. **Missing faction requirements** - no faction reputation integration
5. **Location system needs standardization** - create @locations/ catalog

### Comparison to Other Domains
| Domain | Organization | JSON v4.0 | References | Status |
|--------|-------------|-----------|----------|--------|
| NPCs | ✅ Excellent (10 subdirs) | ✅ 100% | ✅ v4.1 refs | ✅ COMPLETE |
| Enemies | ✅ Excellent (13 subdirs) | ✅ 100% | ⚠️ Needs v4.1 | ⏸️ PARTIAL |
| Items | ✅ Excellent (5 subdirs) | ✅ 100% | ✅ v4.1 refs | ✅ COMPLETE |
| Quests | ⚠️ Needs cleanup (duplicates) | ✅ 100% | ❌ No refs | ⚠️ NEEDS WORK |

---

**Audit Complete**: December 29, 2025  
**Status**: ⏸️ AWAITING USER APPROVAL FOR CLEANUP  
**Estimated Work**: 5-7 hours total (cleanup + references + new catalogs)
