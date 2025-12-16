# General Files Standardization - Implementation Plan

**Date:** December 16, 2025  
**Status:** Ready for Implementation  
**Approach:** Option B - Fix Everything at Once

---

## ✅ **Locked Design Decisions**

### Decision 1: Remove `items` Arrays (Option 2B)
- **What:** Remove all `items` arrays from pattern generation files
- **Why:** Eliminates duplication, components ARE the data source
- **Impact:** 7 files (colors, smells, sounds, textures, time_of_day, weather, verbs)

### Decision 2: Use Pattern Generation (Option 2B-1)
- **What:** Pattern generation for descriptive files (colors, smells, sounds, textures, time_of_day, weather)
- **Why:** Generates dynamic variety for procedural descriptions
- **Impact:** 6 files

### Decision 3: Convert verbs.json to Component Library
- **What:** Remove broken patterns, keep as categorized reference data
- **Why:** Current patterns are meaningless (reference non-existent components)
- **Impact:** 1 file

---

## 📊 **Final File Type Distribution**

### Pattern Generation Files (6)
1. colors.json
2. smells.json
3. sounds.json
4. textures.json
5. time_of_day.json
6. weather.json

### Component Library Files (3)
1. adjectives.json
2. materials.json
3. verbs.json (converted)

---

## 🔧 **Standardization Checklist**

### Component Library Standard (3 files)

**Structure:**
```json
{
  "components": {
    "category1": ["Value1", "Value2", ...],
    "category2": ["Value1", "Value2", ...]
  },
  "metadata": {
    "description": "Human-readable description",
    "version": "1.0",
    "type": "component_library",
    "last_updated": "2025-12-16",
    "component_keys": ["category1", "category2"],
    "component_counts": {
      "category1": 10,
      "category2": 10
    }
  }
}
```

**Files to Standardize:**
- [ ] adjectives.json - Add `components` wrapper and `metadata`
- [ ] materials.json - Add `components` wrapper and `metadata`
- [ ] verbs.json - Remove `items` and `patterns`, add `metadata`

---

### Pattern Generation Standard (6 files)

**Structure:**
```json
{
  "components": {
    "component_key1": ["Value1", "Value2", ...],
    "component_key2": ["Value1", "Value2", ...]
  },
  "patterns": [
    "component_key1",
    "component_key2",
    "component_key1 + component_key2"
  ],
  "metadata": {
    "description": "Human-readable description",
    "version": "1.0",
    "type": "pattern_generation",
    "last_updated": "2025-12-16",
    "component_keys": ["component_key1", "component_key2"],
    "pattern_count": 3,
    "pattern_tokens": ["component_key1", "component_key2"],
    "component_counts": {
      "component_key1": 10,
      "component_key2": 10
    }
  }
}
```

**Critical Rules:**
1. ❌ NO `items` array
2. ✅ Component keys MUST be singular (match pattern tokens exactly)
3. ❌ Patterns MUST NOT have comments or annotations
4. ✅ All pattern tokens MUST match component keys exactly

**Files to Standardize:**
- [ ] colors.json - Fix patterns, remove `items`, fix component keys, add `metadata`
- [ ] smells.json - Fix patterns, remove `items`, fix component keys, add `metadata`
- [ ] sounds.json - Fix patterns, remove `items`, add base component, add `metadata`
- [ ] textures.json - Fix patterns, remove `items`, fix component keys, add `metadata`
- [ ] time_of_day.json - Remove `items`, fix component keys (singular), add `metadata`
- [ ] weather.json - Fix patterns, remove `items`, fix component keys, add `metadata`

---

## 🎯 **File-by-File Implementation Plan**

### 1. adjectives.json ✅ SIMPLE

**Current Issues:**
- Missing `components` wrapper
- Missing `metadata`

**Actions:**
1. Wrap existing structure in `components` object
2. Add `metadata` with auto-generated fields

**Estimated Time:** 2 minutes

---

### 2. materials.json ✅ SIMPLE

**Current Issues:**
- Missing `components` wrapper
- Missing `metadata`

**Actions:**
1. Wrap existing structure in `components` object
2. Add `metadata` with auto-generated fields

**Estimated Time:** 2 minutes

---

### 3. verbs.json ⚠️ MODERATE

**Current Issues:**
- Broken patterns (reference non-existent components)
- Has `items` array (should be removed)
- Missing `metadata`

**Actions:**
1. Remove `items` array
2. Remove `patterns` array completely
3. Keep `components` structure as-is (already well-organized)
4. Add `metadata` with `type: "component_library"`

**Estimated Time:** 3 minutes

---

### 4. colors.json ⚠️ COMPLEX

**Current Issues:**
- Pattern has comments: `"material (gemstone/metal colors)"` ❌
- Component key mismatch: `base_color` (pattern) vs `base_colors` (component)
- Has unnecessary `items` array
- Missing `metadata`

**Actions:**
1. Remove `items` array
2. Fix component keys to singular: `base_color`, `modifier`, `material`
3. Fix patterns: Remove ALL comments
4. Clean patterns:
   - `"base_color"`
   - `"modifier + base_color"`
   - `"material"`
5. Add `metadata` with auto-generated fields

**Estimated Time:** 5 minutes

---

### 5. smells.json ⚠️ COMPLEX

**Current Issues:**
- Pattern has comments: `"smell + smell (combination)"` ❌
- Token `"smell"` doesn't match any component
- Has unnecessary `items` array
- Missing `metadata`

**Actions:**
1. Remove `items` array
2. Keep components as-is (already well-named)
3. Fix patterns - remove comment, use actual component keys:
   - `"pleasant"`
   - `"unpleasant"`
   - `"natural"`
   - `"intensity + pleasant"`
   - `"intensity + unpleasant"`
4. Add `metadata`

**Estimated Time:** 5 minutes

---

### 6. sounds.json ⚠️ COMPLEX

**Current Issues:**
- Token `"sound"` doesn't match any component key
- Has unnecessary `items` array
- Missing base sound component
- Missing `metadata`

**Actions:**
1. Move `items` content to new component: `base_sound`
2. Fix patterns to use actual component keys:
   - `"base_sound"`
   - `"volume + base_sound"`
   - `"nature + base_sound"`
   - `"intensity + base_sound"`
   - `"combat"` (standalone)
   - `"ambient"` (standalone)
3. Add `metadata`

**Estimated Time:** 5 minutes

---

### 7. textures.json ⚠️ COMPLEX

**Current Issues:**
- Token `"texture"` doesn't match any component key
- Has unnecessary `items` array (duplicates `surface_quality`)
- Missing `metadata`

**Actions:**
1. Remove `items` array
2. Keep components as-is (already well-organized)
3. Fix patterns to use actual component keys:
   - `"surface_quality"`
   - `"surface_quality + moisture"`
   - `"temperature + surface_quality"`
   - `"hardness + surface_quality"`
   - `"organic"` (standalone)
4. Add `metadata`

**Estimated Time:** 5 minutes

---

### 8. time_of_day.json ⚠️ MODERATE

**Current Issues:**
- Component keys are plural: `periods`, `modifiers`, `descriptors`
- Patterns use singular: `period`, `modifier`, `descriptor`
- Has unnecessary `items` array (duplicates `periods`)
- Missing `metadata`

**Actions:**
1. Remove `items` array
2. Fix component keys to singular: `period`, `modifier`, `descriptor`
3. Patterns are already correct (match singular)
4. Add `metadata`

**Estimated Time:** 4 minutes

---

### 9. weather.json ⚠️ COMPLEX

**Current Issues:**
- Token `"condition"` doesn't match any component key
- Has unnecessary `items` array
- Unclear pattern structure
- Missing `metadata`

**Actions:**
1. Remove `items` array
2. Keep components as-is (already well-organized)
3. Fix patterns to use actual component keys:
   - `"precipitation"`
   - `"temperature + precipitation"`
   - `"severity + precipitation"`
   - `"wind + precipitation"`
   - `"special"` (standalone)
4. Add `metadata`

**Estimated Time:** 5 minutes

---

## 📅 **Implementation Schedule**

### Phase 1: Simple Files (10 minutes)
1. ✅ adjectives.json (2 min)
2. ✅ materials.json (2 min)
3. ⚠️ verbs.json (3 min)
4. ⚠️ time_of_day.json (4 min)

### Phase 2: Complex Files (25 minutes)
5. ⚠️ colors.json (5 min)
6. ⚠️ smells.json (5 min)
7. ⚠️ sounds.json (5 min)
8. ⚠️ textures.json (5 min)
9. ⚠️ weather.json (5 min)

**Total Estimated Time:** 35 minutes

---

## ✅ **Validation Checklist**

After standardization, each file must pass:

### Pattern Generation Files
- [ ] NO `items` array
- [ ] All component keys are singular
- [ ] All pattern tokens match component keys exactly
- [ ] NO comments or annotations in patterns
- [ ] `metadata.type` = `"pattern_generation"`
- [ ] `metadata` has all auto-generated fields
- [ ] File is valid JSON

### Component Library Files
- [ ] NO `items` array
- [ ] NO `patterns` array
- [ ] Has `components` wrapper
- [ ] `metadata.type` = `"component_library"`
- [ ] `metadata` has all auto-generated fields
- [ ] File is valid JSON

---

## 🧪 **Testing Plan**

### Manual Testing
1. Open each file in ContentBuilder
2. Verify components display correctly
3. For Pattern Generation files: Generate pattern examples
4. Verify no crashes or errors

### Automated Testing
1. JSON schema validation
2. Pattern token validation (tokens must match component keys)
3. Metadata field validation

---

## 📝 **Documentation Updates**

After standardization complete:

1. **Update PATTERN_COMPONENT_STANDARDS.md**
   - Add General Files section
   - Add file type definitions
   - Add Component Library vs Pattern Generation guide

2. **Update WORK_SESSION.md**
   - Update progress: 10/93 files standardized (9 General + 1 weapons/names.json)
   - Add Decision 4: General Files Structure (Option 2B)
   - Add Decision 5: Pattern Generation vs Component Library

3. **Create GENERAL_FILES_STANDARD.md**
   - Standalone reference for General files
   - Examples of each type
   - Usage guidelines

---

## 🚀 **Ready to Execute**

**Confirmed Decisions:**
- ✅ Option 2B: Remove `items` arrays, use components directly
- ✅ Pattern Generation for descriptive files (6 files)
- ✅ Component Library for reference data (3 files)
- ✅ Convert verbs.json to Component Library

**Next Step:** Execute Phase 1 (Simple Files)

**Proceed with standardization?** 🎯
