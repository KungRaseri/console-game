# Pattern Standardization Work Session

**Date:** December 16, 2025  
**Participants:** User + AI  
**Goal:** Review and standardize all JSON data files to use consistent pattern system

## Session Progress

### Current Status: Standards Finalization ✅
### Files Reviewed: 1/93
### Files Standardized: 1/93
### Standards Finalized: YES

---

## Session History

### Phase 1: Standards Definition ✅ COMPLETE

#### Decisions Made

1. **Simple Component Keys** ✅
   - Use simple names: `material`, `quality`, `enchantment`
   - NOT: `prefix_material`, `prefixes_quality`
   - Position in pattern implies prefix vs suffix

2. **Component Flexibility** ✅
   - Components are just semantic keys
   - Universal keys defined: material, quality, descriptive, size, color, origin, condition, enchantment, title, purpose
   - Can add custom keys as needed: emotion, temperature, age, rarity

3. **Enchantment vs Title Separation** ✅
   - `enchantment` = magical properties ("of Slaying", "of Power")
   - `title` = legendary names ("of the Dragon", "of the Hero")
   - Different semantic meaning → separate components

4. **types.json Separation** ✅
   - Item catalog in types.json (separate from names.json)
   - Items array with individual stats per item
   - Type-level traits for shared properties
   - Pattern `base` token resolves from types.json

5. **Item-Level Stats Structure** ✅
   - Items are objects with `name` + individual stats
   - Type traits are shared across all items
   - Fixed stats for now, allow range notation later
   - Applies to ALL categories (weapons, armor, enemies, NPCs)

#### Documents Created

- ✅ `PATTERN_COMPONENT_STANDARDS.md` - Complete standards reference (990+ lines)
  - Universal component key definitions
  - Pattern syntax rules and examples
  - File Type Guide (types.json, names.json, prefixes.json, suffixes.json)
  - Pattern vs stat-based data flow diagrams
  - Decision matrix for file selection
  - Category-by-category breakdown of all 93 files

- ✅ `TYPES_JSON_STRUCTURE.md` - types.json structure specification (460+ lines)
  - Item-level stats vs type-level traits explained
  - Complete examples: weapons, armor, enemies
  - Integration with patterns and modifiers
  - Benefits and migration checklist
  - Q&A section

- ✅ `PATTERN_STANDARDIZATION_PLAN.md` - 4-phase implementation plan
  - Phase 1: Data standardization
  - Phase 2: ContentBuilder updates
  - Phase 3: Runtime implementation
  - Phase 4: Documentation

- ✅ `WORK_SESSION.md` - This file (progress tracker)

---

## Working Notes

### Completed ✅

1. **items/weapons/names.json** - Standardized (template file)
   - Renamed `prefixes_material` → `material`
   - Renamed `prefixes_quality` → `quality`
   - Renamed `prefixes_descriptive` → `descriptive`
   - Split `suffixes_enchantment` → `enchantment` + `title`
   - Added comprehensive pattern set (11 patterns)
   - Updated metadata
   - **Note:** This file uses OLD structure (items array of strings). Will need migration to new types.json structure.

---

## Phase 2: File Migration Plan

### Strategy

We have TWO migration paths:

#### Path A: Names.json Files (Pattern Generation)
- Migrate to new structure WITHOUT types separation (for now)
- Focus on component key standardization
- Add/update patterns
- Files: enemies/*/names.json, npcs/*/names.json

#### Path B: Types.json Creation (Item Catalogs)
- Create NEW types.json files
- Migrate items arrays to item objects with stats
- Extract type-level traits
- Files: items/weapons/types.json, items/armor/types.json, enemies/*/types.json

### Recommended Order

1. **Finish names.json standardization first** (simpler, no structural changes)
   - Focus on component keys and patterns
   - 20-30 files to update

2. **Then create types.json files** (structural changes)
   - Extract items from names.json → types.json
   - Add stats and traits
   - Update names.json to remove items array
   - 10-15 new files to create

---

## Next File to Review

Let's work through files category by category. Pick the next category:

### Option 1: Continue with Items
- [ ] items/weapons/prefixes.json
- [ ] items/armor/materials.json
- [ ] items/enchantments/suffixes.json
- [ ] items/materials/metals.json
- [ ] items/materials/leathers.json
- [ ] items/materials/woods.json
- [ ] items/materials/gemstones.json

### Option 2: Move to Enemies
- [ ] enemies/beasts/names.json
- [ ] enemies/undead/names.json
- [ ] enemies/demons/names.json
- [ ] enemies/elementals/names.json
- [ ] enemies/dragons/names.json
- [ ] enemies/dragons/colors.json
- [ ] enemies/humanoids/names.json

### Option 3: Review General Files
- [x] general/colors.json - No changes needed ✅
- [x] general/adjectives.json - No changes needed ✅
- [x] general/materials.json - No changes needed ✅

### Option 4: NPCs
- [ ] npcs/names/first_names.json
- [ ] npcs/occupations/common.json
- [ ] npcs/dialogue/templates.json
- [ ] npcs/dialogue/traits.json
- [ ] npcs/titles/titles.json

---

## Review Template

For each file we review, we'll follow this process:

### 1. Examine Current Structure
```bash
# Read the file
# Identify editor type (HybridArray, ItemPrefix, FlatItem, NameList)
# Note current component keys
# Note current patterns (if any)
```

### 2. Determine Action
- ✅ **Keep as-is** - No changes needed (reference data, components for others)
- 🔄 **Standardize** - Update component keys and patterns to match standard
- ❌ **Not applicable** - File doesn't use pattern system

### 3. If Standardizing
- List current component keys
- List proposed component keys (using standard names)
- List proposed patterns (from simple to complex)
- Update metadata

### 4. Document Decision
- Record in standards document
- Note any special cases or exceptions
- Add to checklist

---

## Quick Reference

### Standard Component Keys (Prefixes)
- `material`, `quality`, `descriptive`, `size`, `color`, `origin`, `condition`

### Standard Component Keys (Suffixes)
- `enchantment`, `title`, `purpose`

### Special Tokens
- `base`, `item`

### Pattern Structure
```
1. base
2. prefix + base
3. prefix + prefix + base
4. base + suffix
5. prefix + base + suffix
6. prefix + prefix + base + suffix
```

---

## Questions & Decisions Log

### Q: Should weapon/armor prefixes use patterns or stay as ItemPrefix?
**A:** TBD - Need to review structure

### Q: Are material files (metals, woods, etc.) HybridArray or NameList?
**A:** TBD - Need to review

### Q: Do NPC titles need patterns or are they simple lists?
**A:** TBD - Need to review

---

## File Count by Category

- **General:** 3 files (all reviewed ✅)
- **Items:** ~15-20 files
- **Enemies:** ~15 files
- **NPCs:** ~10 files
- **Quests:** ~3 files

**Total:** ~93 files in RealmEngine.Shared/Data/Json/

---

## User Preferences

Which category would you like to tackle next?
- Items (weapons, armor, etc.)
- Enemies (beasts, undead, etc.)
- NPCs (names, occupations, etc.)
- Just go file-by-file and let you see each one

**Your choice:** _____
