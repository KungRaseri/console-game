# Draft Finalization Analysis

**Date:** December 16, 2025  
**Status:** 🔍 Analysis Complete

## Executive Summary

After analyzing the actual JSON files in the codebase, I've discovered that the draft proposals don't match the **current reality** of the file structure. This document clarifies the actual structure and provides finalized standards.

## Key Discovery: Actual vs Proposed Structure

### ❌ Proposed Structure (from drafts)

```json
{
  "items": [
    { "name": "flaming", "displayName": "Flaming", "traits": {...} }
  ],
  "metadata": {...}
}
```

### ✅ Actual Structure (in codebase)

```json
{
  "common": {
    "Rusty": {
      "displayName": "Rusty",
      "traits": {
        "damageMultiplier": { "value": 0.8, "type": "number" }
      }
    }
  },
  "uncommon": {...},
  "rare": {...}
}
```

**Key Differences:**

1. ⚠️ **No `items` array** - Files use rarity-based nested objects instead
2. ⚠️ **Rarity organization** - Top-level keys are rarity tiers (common, uncommon, rare, etc.)
3. ⚠️ **Trait value objects** - All values wrapped in `{ value, type }` format
4. ⚠️ **No metadata** - Current files don't have metadata sections
5. ⚠️ **No separate types.json** - Base items are in names.json weapon_types

## Actual File Inventory

### Items Category

| File | Exists | Structure | Needs Standardization |
|------|--------|-----------|----------------------|
| `items/weapons/names.json` | ✅ | Pattern Generation (HybridArray) | ✅ Already done |
| `items/weapons/prefixes.json` | ✅ | Rarity-organized modifiers | ❌ Keep as-is |
| `items/weapons/suffixes.json` | ✅ | Rarity-organized modifiers | ❌ Keep as-is |
| `items/armor/names.json` | ✅ | Unknown | ⏳ To review |
| `items/armor/materials.json` | ✅ | Rarity-organized modifiers | ❌ Keep as-is |
| `items/armor/prefixes.json` | ✅ | Rarity-organized modifiers | ❌ Keep as-is |
| `items/armor/suffixes.json` | ✅ | Rarity-organized modifiers | ❌ Keep as-is |
| `items/enchantments/*` | ✅ | Unknown | ⏳ To review |
| `items/consumables/*` | ✅ | Unknown | ⏳ To review |
| `items/materials/*` | ✅ | Unknown | ⏳ To review |

### Enemies Category

| File | Exists | Structure | Needs Standardization |
|------|--------|-----------|----------------------|
| `enemies/beasts/names.json` | ✅ | Unknown | ⏳ To review |
| `enemies/beasts/prefixes.json` | ✅ | Rarity-organized modifiers (likely) | ⏳ To review |
| `enemies/beasts/suffixes.json` | ✅ | Rarity-organized modifiers (likely) | ⏳ To review |
| `enemies/beasts/traits.json` | ✅ | Unknown | ⏳ To review |
| `enemies/demons/*` | ✅ | Unknown | ⏳ To review |
| `enemies/dragons/*` | ✅ | Unknown | ⏳ To review |
| `enemies/undead/*` | ✅ | Unknown | ⏳ To review |
| `enemies/elementals/*` | ✅ | Unknown | ⏳ To review |
| `enemies/humanoids/*` | ✅ | Unknown | ⏳ To review |
| Others (goblinoids, insects, orcs, plants, reptilians, trolls, vampires) | ✅ | Unknown | ⏳ To review |

## Finalized Standards by File Type

### 1. Pattern Generation Files (names.json)

**Standard:** HybridArray structure with components and patterns

**Structure:**

```json
{
  "items": ["Item1", "Item2", ...],  // OR weapon_types nested object
  "components": {
    "material": ["Iron", "Steel", ...],
    "quality": ["Fine", "Superior", ...]
  },
  "patterns": [
    "base",
    "material + base",
    "quality + material + base"
  ],
  "metadata": {
    "description": "...",
    "version": "1.0",
    "last_updated": "2025-12-16",
    "component_keys": [...],
    "pattern_tokens": [...]
  }
}
```

**Files using this structure:**

- ✅ `items/weapons/names.json` - Already standardized
- ⏳ `items/armor/names.json` - To review
- ⏳ `enemies/*/names.json` - To review for all enemy types

**Action:** Add metadata to files that don't have it

---

### 2. Stat Modifier Files (prefixes.json, suffixes.json, materials.json)

**Standard:** Rarity-organized modifiers (**NO CHANGES NEEDED**)

**Structure:**

```json
{
  "common": {
    "ModifierName": {
      "displayName": "Display Name",
      "traits": {
        "statName": { "value": 10, "type": "number" },
        "anotherStat": { "value": "fire", "type": "string" }
      }
    }
  },
  "uncommon": {...},
  "rare": {...},
  "epic": {...},
  "legendary": {...}
}
```

**Files using this structure:**

- ✅ `items/weapons/prefixes.json` - Keep as-is
- ✅ `items/weapons/suffixes.json` - Keep as-is
- ✅ `items/armor/materials.json` - Keep as-is
- ✅ `items/armor/prefixes.json` - Keep as-is
- ✅ `items/armor/suffixes.json` - Keep as-is
- ⏳ `enemies/*/prefixes.json` - Likely same structure
- ⏳ `enemies/*/suffixes.json` - Likely same structure

**Action:** NO CHANGES NEEDED - structure works well for runtime

**Rationale:**

- ✅ Rarity organization makes runtime selection easy
- ✅ Trait value objects support type safety
- ✅ Structure is self-documenting
- ✅ No patterns needed (these are lookup tables)
- ✅ No metadata needed (structure is obvious)

---

### 3. Traits Files (traits.json)

**Standard:** Unknown - need to review actual structure

**Files:**

- `enemies/beasts/traits.json`
- Likely exists for other enemy types

**Action:** Review structure and document

---

## Recommendations

### Immediate Actions

**1. Update PATTERN_COMPONENT_STANDARDS.md**

Remove all draft proposals that don't match reality. Replace with:

**For Stat Modifier Files (prefixes/suffixes/materials):**

```markdown
### Stat Modifier Files

**File Type:** Rarity-Organized Modifiers  
**Standard:** Keep current structure, NO CHANGES NEEDED

**Structure:**
- Top-level keys: rarity tiers (common, uncommon, rare, epic, legendary)
- Second-level keys: modifier names
- Each modifier has: displayName + traits object
- All trait values wrapped in { value, type }

**No Standardization Required:**
- ✅ Structure is already optimal
- ✅ No patterns needed (lookup tables)
- ✅ No metadata needed (self-documenting)
- ✅ Runtime-friendly (easy rarity-based selection)

**Files:**
- items/weapons/prefixes.json
- items/weapons/suffixes.json
- items/armor/materials.json
- items/armor/prefixes.json
- items/armor/suffixes.json
- enemies/*/prefixes.json
- enemies/*/suffixes.json
```

**For Pattern Generation Files (names.json):**

```markdown
### Pattern Generation Files

**File Type:** HybridArray with components and patterns  
**Standard:** Add metadata to files missing it

**Standardization Required:**
- ✅ Verify components and patterns exist
- ✅ Add metadata section if missing
- ✅ Auto-generate component_keys and pattern_tokens
- ✅ Validate patterns reference valid component keys

**Files:**
- items/weapons/names.json ✅ Already done
- items/armor/names.json ⏳ To review
- enemies/*/names.json ⏳ To review (13+ files)
```

**2. Review Actual Files**

Before finalizing, we need to:

1. ✅ Read `items/armor/names.json` - Check if it has patterns or needs them
2. ✅ Read `enemies/beasts/names.json` - Check structure
3. ✅ Read `enemies/beasts/traits.json` - Understand traits system
4. ✅ Sample 2-3 other enemy name files - Verify consistency
5. ✅ Check if NPC files exist - May not have JSON data yet

**3. Simplify Standards Document**

Instead of detailed draft proposals for every file, use a **type-based approach**:

```markdown
## File Type Standards

### Type 1: Pattern Generation (*.names.json)
[Standard here]

### Type 2: Stat Modifiers (*.prefixes.json, *.suffixes.json, materials.json)
[Standard here]

### Type 3: Trait Definitions (*.traits.json)
[Standard here - after we review them]

### Type 4: Component Libraries (general/*.json)
[Standard here - already done]
```

This is cleaner and covers all 93 files without repeating structure 93 times.

---

## Cross-File Reference Implementation

### Phase 1: Analysis (Current)

✅ Understand actual file structures  
✅ Document file types  
⏳ Decide which files benefit from references  

### Phase 2: Design

**Question 1: Which files should support references?**

**Good candidates:**

- ✅ `names.json` files - Could reference `@general/materials:metals` instead of duplicating
- ✅ `Component Library` files - Could reference each other

**Bad candidates:**

- ❌ `prefixes.json` / `suffixes.json` - Rarity-organized, not component-based
- ❌ `materials.json` (armor) - Already rarity-organized modifiers, not components
- ❌ `traits.json` - Unknown structure, likely not component-based

**Question 2: What syntax should we use?**

**Option A: Simple @ syntax**

```json
{
  "components": {
    "material": "@general/materials:metals"
  }
}
```

**Option B: Object syntax**

```json
{
  "components": {
    "material": {
      "$ref": "general/materials:metals"
    }
  }
}
```

**Recommendation:** Option A (simpler, JSON-LD inspired)

**Question 3: Where in the codebase should resolution happen?**

**Option A: Runtime (Game.Shared)**

- Pro: Works for both Game.Console and Game.ContentBuilder
- Pro: Single source of truth
- Con: Adds complexity to pattern execution

**Option B: ContentBuilder Only**

- Pro: Simpler runtime (no reference resolution)
- Pro: ContentBuilder "compiles" references into full data
- Pro: Saved JSON has resolved data (no runtime cost)
- Con: Two versions of JSON (with/without references)

**Recommendation:** Option B - ContentBuilder resolves references on save

### Phase 3: Implementation Plan

**Step 1: ContentBuilder Support**

1. Detect `@` references when loading JSON
2. Load referenced file from disk
3. Extract specified component array
4. Display in UI with special "reference" indicator
5. On save, write resolved data OR keep reference (user choice)

**Step 2: Reference UI**

```
┌─ Components ────────────────────────────────┐
│ material: @general/materials:metals [📎]    │
│   → Resolves to: [Iron, Steel, Bronze,...]│
│   [Edit Reference] [Inline Reference]       │
└──────────────────────────────────────────────┘
```

**Actions:**

- **[Edit Reference]** - Change path (e.g., metals → precious)
- **[Inline Reference]** - Convert to local array for customization

**Step 3: Validation**

- ✅ Warn if referenced file doesn't exist
- ✅ Warn if referenced component key doesn't exist
- ✅ Show "broken reference" icon
- ✅ Prevent save if critical references broken

---

## Next Steps

### Decision Points

**1. File Structure Finalization**

- ✅ Accept that prefixes/suffixes/materials use rarity-organized structure
- ✅ No changes needed to those files
- ⏳ Focus standardization on `names.json` files only

**2. Cross-File References**

- ⏳ Decide: ContentBuilder-only OR runtime resolution?
- ⏳ Decide: @ syntax OR $ref object syntax?
- ⏳ Decide: Which files benefit from references?

**3. Implementation Priority**

**Option A: Standards First**

1. Finalize standards document (remove wrong drafts)
2. Review actual file structures
3. Document patterns for names.json files
4. THEN implement references

**Option B: References First**

1. Prototype reference resolution in ContentBuilder
2. Test with one file (e.g., items/armor/names.json references @general/materials:metals)
3. Validate approach works
4. THEN standardize all files using references

**Recommendation:** Option A (Standards First)

- Clearer understanding before coding
- Prevents rework if design changes
- Documents intent for future developers

---

## Proposed Timeline

### Day 1: Finalize Standards (Today)

- ✅ Remove incorrect draft proposals
- ✅ Read actual armor/enemy files
- ✅ Document actual structures
- ✅ Update standards doc with reality
- ✅ Decide on cross-file reference approach

### Day 2: Implement References (If approved)

- ⏳ Add reference detection to ContentBuilder
- ⏳ Implement file loading and resolution
- ⏳ Create UI for reference display
- ⏳ Add validation

### Day 3: Standardize names.json Files

- ⏳ Review all names.json files
- ⏳ Add metadata where missing
- ⏳ Convert to use cross-file references (if implemented)
- ⏳ Test in ContentBuilder

### Day 4: Testing & Documentation

- ⏳ Generate examples for all patterns
- ⏳ Test runtime pattern execution
- ⏳ Update GDD and guides
- ⏳ Final review

---

## Questions for User

**Before we proceed, please confirm:**

1. **Stat Modifier Files:** Accept current rarity-organized structure? (NO CHANGES)
   - ✅ YES - Keep as-is, it works well
   - ❌ NO - Redesign to match draft proposals

2. **Cross-File References:** ContentBuilder-only OR runtime resolution?
   - Option A: ContentBuilder resolves on save (simpler runtime)
   - Option B: Runtime resolves on load (more flexible)

3. **Reference Syntax:** Which syntax?
   - Option A: `"material": "@general/materials:metals"` (simple)
   - Option B: `"material": { "$ref": "general/materials:metals" }` (explicit)

4. **Implementation Order:** Standards first OR prototype first?
   - Option A: Finalize all standards, THEN code
   - Option B: Prototype references, THEN standardize

5. **Scope:** Which files need standardization?
   - ✅ All names.json files (pattern generation)
   - ❌ Prefixes/suffixes/materials (already good)
   - ⏳ Traits files (after we review them)

Please provide answers, and I'll proceed with finalization!
