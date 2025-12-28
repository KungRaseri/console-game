# Standards Discussion Notes

**Date:** December 16, 2025  
**Topic:** Clarifications on Pattern Component Standards

## Discussion Summary

### 1. Metadata Field Usefulness ✅

**Question:** How useful is the metadata field in JSON files?

**Answer:** VERY useful, but make it flexible with required/recommended/optional fields.

#### Metadata Field Specification

**Required for Validation (names.json files only):**
- `componentKeys` - Array of component key names for validation
- `patternTokens` - Array of valid tokens (including "base")

**Recommended for All Files:**
- `description` - Human-readable description of file purpose
- `version` - Schema version (e.g., "1.0", "2.0")
- `lastUpdated` - Date of last modification (YYYY-MM-DD)

**Optional:**
- `type` - File type hint ("reference_data", "catalog", "generation")
- Statistics (counts, totals) - Can be derived programmatically
- Custom properties as needed

#### Benefits

✅ **Validation** - ContentBuilder can validate patterns against componentKeys  
✅ **Documentation** - Clear purpose and version tracking  
✅ **Tooling** - Helps editors understand file structure  
✅ **Maintenance** - Track changes over time

#### Example

```json
"metadata": {
  "description": "Weapon name generation with pattern-based system",
  "version": "2.0",
  "lastUpdated": "2025-12-16",
  "componentKeys": ["material", "quality", "descriptive", "enchantment", "title"],
  "patternTokens": ["base", "material", "quality", "descriptive", "enchantment", "title"],
  "totalPatterns": 11
}
```

---

### 2. General Files DO Need Changes ⚠️

**Question:** Are we sure General files don't need changes?

**Answer:** NO - After review, General files NEED standardization!

#### Issues Found

##### colors.json ❌
1. Pattern `"material (gemstone/metal colors)"` has comments in string - NOT parseable!
2. Token mismatch: Pattern uses `"base_color"` but component is `"base_colors"` (plural)
3. Unclear if file is for reference data or name generation

**Decision Needed:** What is colors.json for?

**Option A - Reference Data (Recommended):**
```json
{
  "components": {
    "base_colors": ["red", "blue", "green", ...],
    "modifiers": ["dark", "light", "bright", ...],
    "materials": ["crimson", "scarlet", "azure", ...]
  },
  "metadata": {
    "description": "Color components for use in other files",
    "version": "1.0",
    "type": "reference_data"
  }
}
```

**Option B - Color Name Generation:**
```json
{
  "components": {
    "base_color": ["red", "blue", "green", ...],  // Singular!
    "modifier": ["dark", "light", "bright", ...]
  },
  "patterns": [
    "base_color",
    "modifier + base_color"
  ],
  "metadata": {
    "description": "Color name generation",
    "componentKeys": ["base_color", "modifier"],
    "patternTokens": ["base_color", "modifier"]
  }
}
```

##### adjectives.json ❌
1. Missing standard structure (no `components` wrapper)
2. Missing metadata

**Recommended Fix:**
```json
{
  "components": {
    "positive": ["Magnificent", "Exquisite", ...],
    "negative": ["Broken", "Damaged", ...],
    "size": ["Tiny", "Small", "Large", ...],
    "appearance": ["Shining", "Glowing", ...],
    "condition": ["New", "Old", "Ancient", ...]
  },
  "metadata": {
    "description": "Adjective components for use in other files",
    "version": "1.0",
    "type": "reference_data"
  }
}
```

##### materials.json 📋
Needs review - likely similar issues to adjectives.json

#### Action Items

- [ ] Review colors.json and decide: reference data OR name generation
- [ ] Fix colors.json patterns (remove comments, fix token names)
- [ ] Standardize adjectives.json structure
- [ ] Review and standardize materials.json
- [ ] Add metadata to all General files

---

### 3. Standard File Structure Section Updated ✅

**Question:** The Standard File Structure section needs updating.

**Answer:** DONE - Updated to show all file type structures!

#### New Structure Breakdown

The updated section now includes **4 file type structures:**

##### 1. types.json - Item/Enemy Catalog
- Items as objects with `name` + individual stats
- Type-level traits for shared properties
- Example: weapons/types.json, armor/types.json

##### 2. names.json - Name Generation
- Components object with categorized arrays
- Patterns array with template strings
- Metadata with componentKeys and patternTokens
- Example: weapons/names.json, enemies/beasts/names.json

##### 3. prefixes.json / suffixes.json - Stat Modifiers
- Items array of objects with name, displayName, traits
- Metadata with description
- Example: weapons/prefixes.json, enchantments/suffixes.json

##### 4. Reference Data Files
- Components object only (no patterns)
- Metadata with type: "reference_data"
- Example: general/adjectives.json, general/colors.json

#### Metadata Specification Added

Clear guidelines for:
- Required fields (validation)
- Recommended fields (documentation)
- Optional fields (statistics, custom)

---

## Updated Documents

### PATTERN_COMPONENT_STANDARDS.md

**Changes Made:**
1. ✅ Updated "Standard File Structure" section with 4 file types
2. ✅ Added metadata field specification
3. ✅ Updated General Category section with issues found
4. ✅ Changed status of General files from "✅ Keep as-is" to "⚠️ Needs review"

**New Sections:**
- Structure by File Type (types.json, names.json, prefixes/suffixes, reference data)
- Metadata Field Specification (required/recommended/optional)

**Status Changes:**
- colors.json: ✅ → ⚠️ (needs decision + fixes)
- adjectives.json: ✅ → ⚠️ (needs standardization)
- materials.json: ✅ → 📋 (needs review)

---

## Next Steps

### Immediate Actions

1. **Decide on colors.json purpose**
   - Reference data OR name generation?
   - Update structure accordingly

2. **Standardize General files**
   - colors.json - Fix patterns, add metadata
   - adjectives.json - Add components wrapper, add metadata
   - materials.json - Review and standardize

3. **Update Category-by-Category section**
   - Reflect new file type structures
   - Add metadata requirements
   - Update status indicators

### Migration Planning

After General files are standardized:

**Path A - Names.json Files (20-30 files):**
- Focus on component key standardization
- Add/update patterns
- Add metadata

**Path B - Types.json Creation (10-15 new files):**
- Extract items from names.json
- Add item-level stats
- Add type-level traits
- Create new files

---

## Questions for Further Discussion

1. **colors.json Decision:** Reference data or name generation?

2. **Metadata Enforcement:** Should ContentBuilder validate metadata fields?

3. **Migration Order:** General files first, then Items, then Enemies/NPCs?

4. **Reference Data Loading:** How does runtime load reference data vs generation data?

5. **Backward Compatibility:** Do we need migration/upgrade logic for old file formats?

---

## Key Decisions Reaffirmed

✅ Metadata is VERY useful - keep with required/recommended/optional tiers  
✅ General files NEED standardization - not ready as-is  
✅ Standard File Structure updated with 4 distinct types  
✅ Component keys are flexible semantic labels  
✅ types.json uses item-level stats (objects with name + stats)

---

## Status

- ✅ Discussion complete
- ✅ Standards document updated
- ⏳ General files need decisions
- ⏳ Ready to begin standardization after decisions finalized
