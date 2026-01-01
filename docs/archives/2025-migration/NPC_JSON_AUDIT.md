# NPC JSON Files Audit - Structure Issues

**Date**: 2025-12-18
**Scope**: `Game.Data/Data/Json/npcs/` directory

## Summary

Audit of NPC-related JSON files to identify:
1. `notes` fields that should be in `metadata`
2. Structural inconsistencies with project standards
3. Potential file consolidation opportunities

---

## 1. npcs/names/ - First Names & Last Names

### Issues Identified

#### 1a. ✅ **CRITICAL: `notes` should be in `metadata`**

**Files Affected:**
- `first_names.json` - Has `notes` object at root level
- `last_names.json` - Has `notes` object at root level

**Current Structure:**
```json
{
  "metadata": { ... },
  "components": { ... },
  "patterns": [...],
  "name_prefixes": { ... },
  "name_suffixes": { ... },
  "notes": {              // ❌ Should be in metadata
    "usage": "...",
    "generation": "...",
    ...
  }
}
```

**Should Be:**
```json
{
  "metadata": {
    "description": "...",
    "version": "2.0",
    "notes": {           // ✅ Inside metadata
      "usage": "...",
      "generation": "..."
    }
  },
  "components": { ... }
}
```

---

#### 1b. ✅ **STRUCTURAL: `name_prefixes` and `name_suffixes` placement**

**Current Location:** Root level in `first_names.json`

**Discussion Points:**
```json
// In first_names.json:
"name_prefixes": {
  "male": ["Ald", "Thor", "Ced", ...],
  "female": ["Ser", "Ly", "El", ...]
},
"name_suffixes": {
  "male": ["ric", "ron", "dric", ...],
  "female": ["aphina", "ra", "ara", ...]
}
```

**Options:**
- **Option A**: Move to `metadata.generation_patterns`
- **Option B**: Create separate `name_generation.json` file
- **Option C**: Move to `components` as special category
- **Option D**: Keep at root but rename to `pattern_components` (like last_names.json does)

**Recommendation**: Last names already has `pattern_components` at root level with similar data. For consistency:
- Either rename `name_prefixes/suffixes` → `pattern_components` in first_names
- OR move ALL pattern data to a shared `name_generation.json` file

---

#### 1c. ✅ **CONSOLIDATION: Combine first_names.json + last_names.json?**

**Current State:**
- `first_names.json` (82 lines) - Male/female first names
- `last_names.json` (133 lines) - Surnames across all cultures

**Proposal:** Combine into single `names.json`

**Pros:**
- Single source of truth for NPC naming
- Easier to manage relationships between first/last names
- Reduces file count
- Matches pattern of other consolidated files

**Cons:**
- Larger file (215 lines total)
- Both loaded even if only need first or last names

**Structure if Combined:**
```json
{
  "metadata": {
    "description": "Complete NPC name catalog - first and last names",
    "version": "3.0",
    "type": "name_catalog",
    "categories": ["first_names", "surnames"],
    "notes": { ... }
  },
  "components": {
    "first_names": {
      "male_common": [...],
      "male_noble": [...],
      "female_common": [...],
      "female_noble": [...]
    },
    "surnames": {
      "fantasy_prestigious": [...],
      "nordic_patronymic": [...],
      "celtic_traditional": [...]
    }
  },
  "pattern_components": {
    "first_name_patterns": { ... },
    "surname_patterns": { ... }
  }
}
```

**Recommendation**: **YES, combine them**. Benefits outweigh costs, especially since NPC generation typically needs both simultaneously.

---

## 2. npcs/dialogue/ - All Dialogue Files

### Issues Identified

#### 2a. ✅ **CRITICAL: `notes` in metadata for ALL dialogue files**

**Files Affected:**
- ✅ `dialogue_styles.json` - No `notes` field (GOOD!)
- ❌ `greetings.json` - Has `notes` at root level
- ❌ `farewells.json` - Has `notes` at root level
- ❌ `rumors.json` - Has `notes` at root level
- ❌ `templates.json` - Has `notes` at root level

**Current Structure (greetings/farewells/rumors):**
```json
{
  "metadata": { ... },
  "components": { ... },
  "notes": {              // ❌ Should be in metadata
    "raritySystem": "weight-based",
    "componentKeys": [...],
    "weight_ranges": { ... }
  }
}
```

**Should Be:**
```json
{
  "metadata": {
    "description": "...",
    "version": "2.0",
    "notes": {           // ✅ Inside metadata
      "raritySystem": "weight-based",
      "weight_ranges": { ... }
    }
  },
  "components": { ... }
}
```

---

### Additional Observation: `templates.json` Structure

**File:** `npcs/dialogue/templates.json`

**Current Structure:**
```json
{
  "metadata": { ... },
  "templates": {        // ❌ Uses "templates" not "components"
    "greetings": [...],
    "merchants": [...],
    ...
  },
  "notes": { ... }      // ❌ Should be in metadata
}
```

**Issue:** Inconsistent property name - uses `templates` instead of standard `components`

**Recommendation:**
- Rename `templates` → `components` for consistency
- Move `notes` into `metadata`
- Consider if this file is even needed (it duplicates greetings/farewells?)

---

## 3. npcs/personalities/ - All Personality Files

### Issues Identified

#### 3a. ✅ **CRITICAL: `notes` in metadata for ALL personality files**

**Files Affected:**
- ❌ `backgrounds.json` - Has `notes` at root level
- ✅ `personality_traits.json` - No `notes` field (GOOD!)
- ❌ `quirks.json` - Has `notes` at root level

**Current Structure (backgrounds/quirks):**
```json
{
  "metadata": { ... },
  "components": { ... },
  "notes": {              // ❌ Should be in metadata
    "raritySystem": "weight-based",
    "weight_ranges": { ... }
  }
}
```

**Should Be:**
```json
{
  "metadata": {
    "description": "...",
    "version": "2.0",
    "notes": {           // ✅ Inside metadata
      "raritySystem": "weight-based",
      "weight_ranges": { ... }
    }
  },
  "components": { ... }
}
```

---

## Files Summary Table

| File | notes in metadata? | Structure Issues | Action Required |
|------|-------------------|------------------|-----------------|
| **npcs/names/** |
| `first_names.json` | ❌ Root level | name_prefixes/suffixes at root | Move notes; consolidate file |
| `last_names.json` | ❌ Root level | pattern_components at root | Move notes; consolidate file |
| **npcs/dialogue/** |
| `dialogue_styles.json` | ✅ No notes | None | None |
| `greetings.json` | ❌ Root level | None | Move notes to metadata |
| `farewells.json` | ❌ Root level | None | Move notes to metadata |
| `rumors.json` | ❌ Root level | None | Move notes to metadata |
| `templates.json` | ❌ Root level | Uses "templates" not "components" | Move notes; rename property |
| **npcs/personalities/** |
| `backgrounds.json` | ❌ Root level | None | Move notes to metadata |
| `personality_traits.json` | ✅ No notes | None | None |
| `quirks.json` | ❌ Root level | None | Move notes to metadata |

---

## Recommended Actions

### Phase 1: Critical Fixes (notes placement)
1. Move `notes` into `metadata` for:
   - `first_names.json`
   - `last_names.json`
   - `greetings.json`
   - `farewells.json`
   - `rumors.json`
   - `templates.json`
   - `backgrounds.json`
   - `quirks.json`

### Phase 2: Structural Improvements
1. **Combine first_names.json + last_names.json → names.json**
   - Merge components under `first_names` and `surnames` keys
   - Consolidate pattern_components
   - Update ContentBuilder tree structure

2. **Fix name generation patterns**
   - Standardize on `pattern_components` naming
   - Ensure consistency between first/last name patterns

3. **Fix templates.json**
   - Rename `templates` → `components`
   - Consider deprecating if redundant with greetings/farewells

### Phase 3: ContentBuilder Updates
1. Update NameCatalogEditor to handle combined names.json
2. Update tree structure to show "Names" → "First Names" / "Surnames"
3. Update all tests referencing these files

---

## Questions for User

1. **name_prefixes/suffixes**: Keep at root as `pattern_components` or move to metadata?
2. **Combine names files**: Proceed with combining first_names + last_names → names.json?
3. **templates.json**: Keep, fix, or delete? (appears to duplicate greetings/farewells)
4. **Priority**: Fix notes placement first, then structure? Or all at once?

---

## Impact Analysis

### Files to Modify: 8 total
- `first_names.json`
- `last_names.json`
- `greetings.json`
- `farewells.json`
- `rumors.json`
- `templates.json`
- `backgrounds.json`
- `quirks.json`

### Tests to Update:
- `NameCatalogEditorUITests.cs` (if names combined)
- Any generator tests reading these files
- ContentBuilder integration tests

### Code to Update:
- `NameCatalogEditorViewModel.cs` (if names combined)
- Any C# code reading `notes` from root level
- MainViewModel tree structure (if names combined)

---

**Next Steps:** Await user decisions on questions above, then proceed with fixes.
