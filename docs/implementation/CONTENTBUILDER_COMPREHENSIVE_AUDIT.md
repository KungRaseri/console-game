# ContentBuilder Comprehensive JSON Structure Audit

**Date:** December 2024  
**Status:** 🔄 IN PROGRESS  
**Scope:** Verify all 93 JSON files are properly configured in ContentBuilder

---

## Executive Summary

### Files Found vs. Configured
- **Total JSON Files:** 93 files
- **Configured in ContentBuilder:** 77 files (82.8%)
- **Missing from ContentBuilder:** 16 files (17.2%)
- **Likely Misconfigured:** 8-12 files (editor type mismatch)

### Critical Issues
1. ❌ **16 files not visible in ContentBuilder UI**
2. ⚠️ **Enemy name files using wrong editor** (NameList vs. custom structure)
3. ⚠️ **Quest templates using wrong editor** (FlatItem vs. grouped structure)
4. ⚠️ **Enchantment suffixes using wrong editor** (FlatItem vs. grouped structure)
5. ⚠️ **Some occupation files using wrong editor** (FlatItem vs. direct array)

---

## Configuration Analysis

### ✅ Correctly Configured (59 files - 63%)

**General Data (7 files) - HYBRID pattern → HybridArray editor:**
- ✅ `general/colors.json`
- ✅ `general/smells.json`
- ✅ `general/sounds.json`
- ✅ `general/textures.json`
- ✅ `general/time_of_day.json`
- ✅ `general/verbs.json`
- ✅ `general/weather.json`

**Items - Weapons (2 files):**
- ✅ `items/weapons/names.json` - NESTED_CATEGORIES → NameList ✓ FIXED
- ✅ `items/weapons/suffixes.json` - HYBRID → HybridArray ✓

**Items - Armor (3 files):**
- ✅ `items/armor/names.json` - HYBRID → HybridArray ✓
- ✅ `items/armor/prefixes.json` - HYBRID → HybridArray ✓
- ✅ `items/armor/suffixes.json` - HYBRID → HybridArray ✓

**Items - Consumables (3 files) - HYBRID → HybridArray:**
- ✅ `items/consumables/names.json`
- ✅ `items/consumables/effects.json`
- ✅ `items/consumables/rarities.json`

**Items - Enchantments (2 files):**
- ✅ `items/enchantments/prefixes.json` - HYBRID → HybridArray ✓
- ✅ `items/enchantments/effects.json` - HYBRID → HybridArray ✓

**Items - Materials (4 files) - ITEM_TRAITS → FlatItem:**
- ✅ `items/materials/metals.json`
- ✅ `items/materials/woods.json`
- ✅ `items/materials/leathers.json`
- ✅ `items/materials/gemstones.json`

**Enemies - Traits & Suffixes (26 files) - HYBRID → HybridArray:**
- ✅ `enemies/beasts/traits.json` & `enemies/beasts/suffixes.json`
- ✅ `enemies/demons/traits.json` & `enemies/demons/suffixes.json`
- ✅ `enemies/dragons/traits.json` & `enemies/dragons/suffixes.json`
- ✅ `enemies/elementals/traits.json` & `enemies/elementals/suffixes.json`
- ✅ `enemies/humanoids/traits.json` & `enemies/humanoids/suffixes.json`
- ✅ `enemies/undead/traits.json` & `enemies/undead/suffixes.json`
- ✅ `enemies/vampires/traits.json` & `enemies/vampires/suffixes.json`
- ✅ `enemies/goblinoids/traits.json` & `enemies/goblinoids/suffixes.json`
- ✅ `enemies/orcs/traits.json` & `enemies/orcs/suffixes.json`
- ✅ `enemies/insects/traits.json` & `enemies/insects/suffixes.json`
- ✅ `enemies/plants/traits.json` & `enemies/plants/suffixes.json`
- ✅ `enemies/reptilians/traits.json` & `enemies/reptilians/suffixes.json`
- ✅ `enemies/trolls/traits.json` & `enemies/trolls/suffixes.json`

**NPCs (8 files):**
- ✅ `npcs/names/first_names.json` - HYBRID → HybridArray ✓
- ✅ `npcs/names/last_names.json` - HYBRID → HybridArray ✓
- ✅ `npcs/personalities/traits.json` - HYBRID → HybridArray ✓
- ✅ `npcs/personalities/quirks.json` - HYBRID → HybridArray ✓
- ✅ `npcs/personalities/backgrounds.json` - HYBRID → HybridArray ✓
- ✅ `npcs/dialogue/greetings.json` - HYBRID → HybridArray ✓
- ✅ `npcs/dialogue/farewells.json` - HYBRID → HybridArray ✓
- ✅ `npcs/dialogue/rumors.json` - HYBRID → HybridArray ✓

**Quests - Objectives, Rewards, Locations (9 files) - HYBRID → HybridArray:**
- ✅ `quests/objectives/primary.json`
- ✅ `quests/objectives/secondary.json`
- ✅ `quests/objectives/hidden.json`
- ✅ `quests/rewards/gold.json`
- ✅ `quests/rewards/experience.json`
- ✅ `quests/rewards/items.json`
- ✅ `quests/locations/towns.json`
- ✅ `quests/locations/dungeons.json`
- ✅ `quests/locations/wilderness.json`

---

### ⚠️ Potentially Misconfigured (12 files - 13%)

**Enemy Names (6 files) - UNKNOWN pattern but configured as NameList:**
- ⚠️ `enemies/beasts/names.json` - Structure: `{prefixes:[], creatures:[], variants:{}}`
  - **Current:** EditorType.NameList
  - **Should Be:** Custom EnemyNameEditor OR enhanced NameList
  - **Issue:** NameList expects `{category:[]}` but file has `{prefixes:[], creatures:[], variants:{}}`

- ⚠️ `enemies/demons/names.json` - Same issue
- ⚠️ `enemies/dragons/names.json` - Same issue
- ⚠️ `enemies/elementals/names.json` - Same issue
- ⚠️ `enemies/humanoids/names.json` - Same issue
- ⚠️ `enemies/undead/names.json` - Same issue

**Quest Templates (5 files) - UNKNOWN pattern but configured as FlatItem:**
- ⚠️ `quests/templates/kill.json` - Structure: `{Easy:{quest:traits}, Medium:{}, Hard:{}}`
  - **Current:** EditorType.FlatItem
  - **Should Be:** Custom DifficultyGroupedEditor OR enhanced FlatItem
  - **Issue:** FlatItem expects `{item: {displayName, traits}}` but file has difficulty groupings

- ⚠️ `quests/templates/delivery.json` - Same issue
- ⚠️ `quests/templates/escort.json` - Same issue
- ⚠️ `quests/templates/fetch.json` - Same issue
- ⚠️ `quests/templates/investigate.json` - Same issue

**Enchantment Suffixes (1 file) - UNKNOWN pattern but configured as FlatItem:**
- ⚠️ `items/enchantments/suffixes.json` - Structure: `{power:{}, protection:{}, wisdom:{}...}`
  - **Current:** EditorType.FlatItem
  - **Should Be:** Custom CategoryGroupedEditor OR enhanced FlatItem
  - **Issue:** FlatItem expects flat `{item: traits}` but file has category groupings

---

### ❌ Missing from ContentBuilder (16 files - 17%)

**General (2 files):**
- ❌ `general/adjectives.json` - CATEGORY_ARRAYS: `{positive:[], negative:[], ...}`
  - **Should Be:** EditorType.NameList (can handle category arrays)
  
- ❌ `general/materials.json` - CATEGORY_ARRAYS: `{natural:[], synthetic:[], ...}`
  - **Should Be:** EditorType.NameList

**Enemy Prefixes (6 files) - RARITY_ITEM_TRAITS:**
- ❌ `enemies/beasts/prefixes.json` - Currently configured as FlatItem
  - **Actual Structure:** `{common:{item:traits}, uncommon:{}, rare:{}}`
  - **Issue:** May work with FlatItem but won't preserve rarity groupings properly
  
- ❌ `enemies/demons/prefixes.json` - Same issue (configured as FlatItem)
- ❌ `enemies/dragons/prefixes.json` - Same issue (configured as FlatItem)
- ❌ `enemies/elementals/prefixes.json` - Same issue (configured as FlatItem)
- ❌ `enemies/humanoids/prefixes.json` - Same issue (configured as FlatItem)
- ❌ `enemies/undead/prefixes.json` - Same issue (configured as FlatItem)

**Items (2 files):**
- ❌ `items/weapons/prefixes.json` - RARITY_ITEM_TRAITS
  - **Structure:** `{common:{}, uncommon:{}, rare:{}, epic:{}, legendary:{}}`
  - **Should Be:** Custom RarityItemEditor OR enhanced FlatItem
  
- ❌ `items/armor/materials.json` - RARITY_ITEM_TRAITS (configured as FlatItem)
  - **May work but won't show rarity structure properly**

**NPCs (4 files):**
- ❌ `npcs/dialogue/templates.json` - CATEGORY_ARRAYS
  - **Current:** EditorType.HybridArray (MISMATCH)
  - **Actual:** `{greeting:[], question:[], statement:[]}`
  - **Should Be:** EditorType.NameList
  
- ❌ `npcs/dialogue/traits.json` - ITEM_TRAITS
  - **Current:** EditorType.FlatItem ✓ (CORRECT)
  
- ❌ `npcs/occupations/common.json` - RARITY_ITEM_TRAITS
  - **Current:** EditorType.FlatItem (may not preserve rarity structure)
  
- ❌ `npcs/occupations/criminal.json` - ARRAY pattern
  - **Current:** EditorType.FlatItem (WRONG)
  - **Actual:** Direct array `["Thief", "Assassin", ...]`
  - **Should Be:** Custom SimpleArrayEditor
  
- ❌ `npcs/occupations/magical.json` - ARRAY pattern (same issue)
- ❌ `npcs/occupations/noble.json` - ARRAY pattern (same issue)

**Enemy Special Case:**
- ❌ `enemies/dragons/colors.json` - ITEM_TRAITS
  - **Current:** EditorType.FlatItem ✓ (CORRECT)

---

## Pattern → Editor Mapping

### Current Editor Types (5)
1. **ItemPrefix/ItemSuffix** - Legacy, rarely used
2. **FlatItem** - `{item: {displayName, traits}}` - 6-10 files
3. **NameList** - `{category: []}` or `{items: {category: []}}` - 10 files
4. **HybridArray** - `{items:[], components:{}, patterns:[], metadata:{}}` - 59 files
5. **None** - Folder nodes only

### Pattern Coverage Analysis

| Pattern | Files | Current Editor | Works? | Fix Needed |
|---------|-------|----------------|--------|------------|
| **HYBRID** | 59 | HybridArray | ✅ Yes | None |
| **ITEM_TRAITS** | 6 | FlatItem | ✅ Yes | None |
| **NESTED_CATEGORIES** | 1 | NameList | ✅ Yes | None (fixed) |
| **RARITY_ITEM_TRAITS** | 9 | FlatItem | ⚠️ Partial | New editor or enhance FlatItem |
| **CATEGORY_ARRAYS** | 3 | NameList or missing | ⚠️ Partial | Add missing files |
| **ARRAY** | 3 | FlatItem (wrong) | ❌ No | New SimpleArrayEditor |
| **UNKNOWN (Enemy Names)** | 6 | NameList | ❌ No | New EnemyNameEditor |
| **UNKNOWN (Grouped Items)** | 6 | FlatItem | ❌ No | New GroupedItemEditor |

---

## Recommended Fixes

### Priority 1: Add Missing Files (16 files)
**Quick wins - use existing editors:**

1. Add to General category:
   ```csharp
   // Already have these slots, just missing entries:
   new CategoryNode { Name = "Adjectives", ... EditorType.NameList, Tag = "general/adjectives.json" }
   new CategoryNode { Name = "Materials", ... EditorType.NameList, Tag = "general/materials.json" }
   ```

2. Fix NPC Dialogue Templates:
   ```csharp
   // Change from HybridArray to NameList
   new CategoryNode { Name = "Templates", ... EditorType.NameList, Tag = "npcs/dialogue/templates.json" }
   ```

### Priority 2: Fix Simple Array Files (3 files)
**Create SimpleArrayEditor:**

Files:
- `npcs/occupations/criminal.json`
- `npcs/occupations/magical.json`
- `npcs/occupations/noble.json`

Structure: `["Item1", "Item2", ...]`

**Solution:** Create new `SimpleArrayEditorViewModel` and `SimpleArrayEditorView`
- Load: Parse JSON array
- Edit: Simple list box with add/remove/reorder
- Save: Write back as JSON array

### Priority 3: Fix Enemy Names (6 files)
**Create EnemyNameEditor or enhance NameList:**

Structure:
```json
{
  "prefixes": ["Dire", "Wild", ...],
  "creatures": ["Wolf", "Bear", ...],
  "variants": {
    "adjectives": ["Fierce", "Rabid", ...]
  }
}
```

**Option A:** Enhance NameListEditor to handle `variants` object
**Option B:** Create specialized EnemyNameEditor with 3 sections

### Priority 4: Fix Grouped Item Files (6 files)
**Create GroupedItemEditor:**

Files:
- `items/enchantments/suffixes.json` (category grouped)
- `quests/templates/kill.json` (difficulty grouped)
- `quests/templates/delivery.json`
- `quests/templates/escort.json`
- `quests/templates/fetch.json`
- `quests/templates/investigate.json`

Structure:
```json
{
  "Group1": {
    "Item1": {"displayName": "...", "traits": {...}},
    "Item2": {...}
  },
  "Group2": {...}
}
```

**Solution:** Create `GroupedItemEditorViewModel`
- TreeView: Groups → Items
- Edit: Similar to FlatItem but preserve groupings

### Priority 5: Review Rarity Files (9 files)
**Test or enhance FlatItem:**

Files using RARITY_ITEM_TRAITS pattern with FlatItem editor:
- 6 enemy prefix files
- `items/weapons/prefixes.json`
- `items/armor/materials.json`
- `npcs/occupations/common.json`

**Action:** Test if FlatItemEditor preserves `{rarity: {item: traits}}` structure
- If YES: ✅ No changes needed
- If NO: Create RarityItemEditor or enhance FlatItem

---

## Implementation Plan

### Phase 1: Quick Fixes (30 minutes)
1. ✅ Add missing General files (adjectives, materials) - 2 files
2. ✅ Fix NPC dialogue templates editor type - 1 file
3. ✅ Total quick wins: 3 files fixed

### Phase 2: Simple Array Editor (1 hour)
1. Create `SimpleArrayEditorViewModel.cs`
2. Create `SimpleArrayEditorView.xaml`
3. Add to MainViewModel switch case
4. Test with 3 occupation files

### Phase 3: Enemy Name Editor (1.5 hours)
**Decision point:** Enhance NameList vs. new editor

**Option A - Enhance NameList** (60 min):
- Modify NameListEditorViewModel to detect `variants` object
- Add UI section for variants
- Test with 6 enemy name files

**Option B - New EnemyNameEditor** (90 min):
- Create dedicated ViewModel/View
- 3-section UI: Prefixes, Creatures, Variants
- More specialized but cleaner

### Phase 4: Grouped Item Editor (2 hours)
1. Create `GroupedItemEditorViewModel.cs`
2. Create `GroupedItemEditorView.xaml` with TreeView
3. Handle both category and difficulty groupings
4. Test with 6 files (1 enchantment + 5 quest templates)

### Phase 5: Rarity Review & Testing (1 hour)
1. Test FlatItemEditor with rarity files
2. If issues, create RarityItemEditor
3. Verify all 9 rarity files work correctly

### Phase 6: Comprehensive Testing (1 hour)
1. Open ContentBuilder
2. Navigate to each of 93 files
3. Test open, edit, save for each
4. Document any remaining issues

**Total Estimated Time:** 6-7 hours

---

## Success Criteria

✅ **All 93 JSON files visible in ContentBuilder tree**  
✅ **All files open without errors**  
✅ **Correct editor type for each file's structure**  
✅ **Edit and save preserve JSON structure**  
✅ **No data loss on save/reload cycle**

---

## Next Steps

1. **Immediate:** Add 3 missing files to MainViewModel (Phase 1)
2. **This session:** Create SimpleArrayEditor (Phase 2)
3. **Follow-up:** Enemy name and grouped item editors (Phase 3-4)
4. **Final:** Comprehensive testing (Phase 6)

---

## Files to Review

**MainViewModel.cs:**
- Line 65-444: InitializeCategories() - all CategoryNode definitions
- Line 446-549: OnSelectedCategoryChanged() - editor loading logic

**Editor ViewModels:**
- NameListEditorViewModel.cs - Can handle NESTED_CATEGORIES and CATEGORY_ARRAYS
- FlatItemEditorViewModel.cs - Handles ITEM_TRAITS, possibly RARITY_ITEM_TRAITS
- HybridArrayEditorViewModel.cs - Handles HYBRID pattern (59 files)

**Missing Editors:**
- SimpleArrayEditorViewModel.cs - Needed for direct arrays
- EnemyNameEditorViewModel.cs - Needed for enemy name structure
- GroupedItemEditorViewModel.cs - Needed for category/difficulty grouped items

---

**Document Status:** Ready for implementation  
**Created By:** GitHub Copilot  
**Last Updated:** December 2024
