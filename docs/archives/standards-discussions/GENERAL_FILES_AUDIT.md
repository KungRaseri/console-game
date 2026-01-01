# General Files Audit & Standards

**Date:** December 16, 2025  
**Total Files:** 9  
**Location:** `RealmEngine.Shared/Data/Json/general/`

## Executive Summary

The General category contains **reference data** and **component libraries** used by other JSON files throughout the game. After auditing all 9 files, we've identified **2 distinct file types** with different purposes and structures.

---

## File Type Classification

### Type 1: Component Library (7 files)

**Purpose:** Categorized lists of values used as components in other files  
**Usage:** Referenced by items, enemies, NPCs for name generation  
**Structure:** Categorized arrays with no items list  
**Pattern Generation:** NO - these ARE the components

**Files:**

- `adjectives.json` ⭐
- `materials.json` ⭐

**Recommendation:** `verbs.json` should be Type 1 (currently broken)

### Type 2: Hybrid (Pattern Generation + Component Library) (6 files)

**Purpose:** Both generate content AND provide components for other files  
**Usage:** Can be used standalone OR as component source  
**Structure:** Has `items`, `components`, AND `patterns`  
**Pattern Generation:** YES - generates procedural content

**Files:**

- `colors.json` ⚠️ (has issues)
- `sounds.json` ⚠️ (token mismatch)
- `textures.json` ⚠️ (token mismatch)
- `time_of_day.json` ⚠️ (needs standardization)
- `weather.json` ⚠️ (token mismatch)
- `smells.json` ⚠️ (broken patterns)

---

## Detailed File Audit

### 1. adjectives.json ⭐

**Current Structure:**

```json
{
  "positive": [...],
  "negative": [...],
  "size": [...],
  "appearance": [...],
  "condition": [...]
}
```

**File Type:** Component Library  
**Purpose:** Provide adjectives for item/enemy/NPC descriptions  
**Issues:**

- ❌ Missing `components` wrapper
- ❌ Missing `metadata`
- ✅ Structure is clear and organized

**Recommended Standard Structure:**

```json
{
  "components": {
    "positive": ["Magnificent", "Exquisite", "Pristine", ...],
    "negative": ["Broken", "Damaged", "Ruined", ...],
    "size": ["Tiny", "Small", "Large", "Huge", ...],
    "appearance": ["Shining", "Glowing", "Sparkling", ...],
    "condition": ["New", "Old", "Ancient", ...]
  },
  "metadata": {
    "description": "Adjective components for descriptive text generation",
    "version": "1.0",
    "type": "component_library"
  }
}
```

**Action Required:** ✅ Standardize - Add wrappers and metadata

---

### 2. colors.json ⚠️

**Current Structure:**

```json
{
  "items": ["crimson", "scarlet", ...],
  "components": {
    "base_colors": ["red", "blue", ...],
    "modifiers": ["dark", "light", ...],
    "materials": ["crimson", "scarlet", ...]
  },
  "patterns": [
    "base_color",
    "modifier + base_color",
    "material (gemstone/metal colors)"  // ❌ INVALID!
  ]
}
```

**File Type:** Hybrid (Pattern Generation + Component Library)  
**Purpose:** Generate color names AND provide color components  
**Issues:**

1. ❌ Pattern `"material (gemstone/metal colors)"` has comments - NOT parseable!
2. ❌ Token mismatch: Pattern uses `"base_color"` but component is `"base_colors"` (plural)
3. ⚠️ Duplication: `items` and `components.materials` have overlapping values
4. ❌ Missing metadata

**Recommended Standard Structure:**

**Option A - Pattern Generation (Recommended):**

```json
{
  "components": {
    "base_color": ["red", "blue", "green", "yellow", ...],
    "modifier": ["dark", "light", "bright", "pale", ...],
    "material": ["crimson", "scarlet", "azure", "emerald", ...]
  },
  "patterns": [
    "base_color",
    "modifier + base_color",
    "material"
  ],
  "metadata": {
    "description": "Color name generation with base colors, modifiers, and materials",
    "version": "1.0",
    "type": "pattern_generation"
  }
}
```

**Option B - Component Library Only:**

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
    "type": "component_library"
  }
}
```

**Decision Needed:** Does the game need to GENERATE color names or just provide color components?

**Action Required:** ⚠️ Fix patterns, decide on purpose, standardize

---

### 3. materials.json ⭐

**Current Structure:**

```json
{
  "metals": [...],
  "precious": [...],
  "natural": [...],
  "magical": [...]
}
```

**File Type:** Component Library  
**Purpose:** Provide material names for items/equipment  
**Issues:**

- ❌ Missing `components` wrapper
- ❌ Missing `metadata`
- ✅ Clear categorization

**Recommended Standard Structure:**

```json
{
  "components": {
    "metals": ["Iron", "Steel", "Bronze", "Copper", ...],
    "precious": ["Diamond", "Ruby", "Sapphire", "Emerald", ...],
    "natural": ["Wood", "Stone", "Bone", "Leather", ...],
    "magical": ["Ethereal", "Astral", "Void", "Shadow", ...]
  },
  "metadata": {
    "description": "Material components for item crafting and descriptions",
    "version": "1.0",
    "type": "component_library"
  }
}
```

**Action Required:** ✅ Standardize - Add wrappers and metadata

---

### 4. smells.json

**Current Structure:**

```json
{
  "items": [...],
  "components": {
    "pleasant": [...],
    "unpleasant": [...],
    "natural": [...],
    "intensity": [...]
  },
  "patterns": [
    "smell",
    "intensity + smell",
    "smell + smell (combination)"  // ❌ Comment in pattern!
  ]
}
```

**File Type:** Hybrid (Pattern Generation + Component Library)  
**Purpose:** Generate smell descriptions AND provide smell components  
**Issues:**

- ❌ Pattern `"smell + smell (combination)"` has comments - NOT parseable!
- ❌ Token `"smell"` doesn't match any component key
- ❌ Missing metadata

**Recommended Standard Structure:**

```json
{
  "components": {
    "pleasant": ["fragrant", "fresh", "floral", ...],
    "unpleasant": ["musty", "acrid", "pungent", ...],
    "natural": ["earthy", "woody", "mossy", ...],
    "intensity": ["faint", "mild", "strong", "overpowering", ...]
  },
  "patterns": [
    "pleasant",
    "unpleasant",
    "natural",
    "intensity + pleasant",
    "intensity + unpleasant"
  ],
  "metadata": {
    "description": "Smell descriptions for environmental and item descriptions",
    "version": "1.0",
    "type": "pattern_generation"
  }
}
```

**Note:** Removed `items` array - components serve as the source

**Action Required:** ⚠️ Fix patterns, remove items, add metadata

---

### 5. sounds.json

**Current Structure:**
```json
{
  "items": ["echoing", "whisper", "roar", ...],
  "components": {
    "volume": ["silent", "quiet", "soft", ...],
    "nature": ["metallic", "wooden", "liquid", ...],
    "combat": ["clang", "crash", "clash", ...],
    "ambient": ["rustle", "whisper", "murmur", ...],
    "intensity": ["gentle", "harsh", "sharp", ...]
  },
  "patterns": [
    "sound",
    "volume + sound",
    "nature + sound",
    "intensity + sound"
  ]
}
```

**File Type:** Hybrid (Pattern Generation + Component Library)  
**Purpose:** Generate sound descriptions AND provide sound components  
**Issues:**
- ❌ Token `"sound"` doesn't match any component key
- ❌ Missing metadata
- ⚠️ Duplication: `items` contains sounds, but no matching component

**Recommended Standard Structure:**
```json
{
  "components": {
    "base_sound": ["echoing", "whisper", "roar", "clang", ...],
    "volume": ["silent", "quiet", "soft", "loud", ...],
    "nature": ["metallic", "wooden", "liquid", "magical", ...],
    "combat": ["clang", "crash", "clash", "thud", ...],
    "ambient": ["rustle", "whisper", "murmur", "chirp", ...],
    "intensity": ["gentle", "harsh", "sharp", "rhythmic", ...]
  },
  "patterns": [
    "base_sound",
    "volume + base_sound",
    "nature + base_sound",
    "intensity + base_sound",
    "combat",
    "ambient"
  ],
  "metadata": {
    "description": "Sound descriptions for combat, environment, and atmosphere",
    "version": "1.0",
    "type": "pattern_generation"
  }
}
```

**Note:** Moved `items` to `components.base_sound`, added combat/ambient as standalone options

**Action Required:** ⚠️ Fix patterns (add base_sound component), remove items, add metadata

---

### 6. textures.json

**Current Structure:**
```json
{
  "items": ["rough", "smooth", "slimy", ...],
  "components": {
    "surface_quality": ["rough", "smooth", "polished", ...],
    "temperature": ["cold", "icy", "frozen", ...],
    "moisture": ["dry", "parched", "damp", ...],
    "hardness": ["soft", "plush", "yielding", ...],
    "organic": ["leathery", "scaly", "feathery", ...]
  },
  "patterns": [
    "texture",
    "surface_quality + moisture",
    "temperature + texture",
    "hardness + surface_quality"
  ]
}
```

**File Type:** Hybrid (Pattern Generation + Component Library)  
**Purpose:** Generate texture descriptions AND provide texture components  
**Issues:**
- ❌ Token `"texture"` doesn't match any component key
- ❌ Missing metadata
- ⚠️ Duplication: `items` array overlaps with `components.surface_quality`

**Recommended Standard Structure:**
```json
{
  "components": {
    "surface_quality": ["rough", "smooth", "polished", "jagged", ...],
    "temperature": ["cold", "icy", "warm", "hot", ...],
    "moisture": ["dry", "damp", "wet", "slimy", ...],
    "hardness": ["soft", "firm", "hard", "brittle", ...],
    "organic": ["leathery", "scaly", "furry", "silky", ...]
  },
  "patterns": [
    "surface_quality",
    "surface_quality + moisture",
    "temperature + surface_quality",
    "hardness + surface_quality",
    "organic"
  ],
  "metadata": {
    "description": "Texture descriptions for items and environment",
    "version": "1.0",
    "type": "pattern_generation"
  }
}
```

**Note:** Removed `items` array, replaced `"texture"` token with `"surface_quality"`

**Action Required:** ⚠️ Fix patterns (replace texture with surface_quality), remove items, add metadata

---

### 7. time_of_day.json

**Current Structure:**

```json
{
  "items": ["dawn", "sunrise", "morning", ...],
  "components": {
    "periods": ["dawn", "morning", "midday", ...],
    "modifiers": ["early", "late", "deep", ...],
    "descriptors": ["first light", "golden hour", ...]
  },
  "patterns": [
    "period",
    "modifier + period",
    "descriptor"
  ]
}
```

**File Type:** Hybrid (Pattern Generation + Component Library)  
**Purpose:** Generate time descriptions AND provide time components  
**Issues:**

- ❌ Missing metadata
- ⚠️ Duplication: `items` and `components.periods` overlap
- ✅ Patterns are clean and parseable

**Recommended Standard Structure:**

```json
{
  "components": {
    "period": ["dawn", "morning", "midday", "afternoon", ...],
    "modifier": ["early", "late", "deep", "high", "dead"],
    "descriptor": ["first light", "golden hour", "gloaming", ...]
  },
  "patterns": [
    "period",
    "modifier + period",
    "descriptor"
  ],
  "metadata": {
    "description": "Time of day descriptions for narrative and environment",
    "version": "1.0",
    "type": "pattern_generation"
  }
}
```

**Note:** Removed `items` array, made component keys singular to match patterns

**Action Required:** ✅ Standardize - Remove items, fix component keys, add metadata

---

### 8. verbs.json

**Current Structure:**
```json
{
  "items": ["attacks", "strikes", "slashes", ...],
  "components": {
    "combat_offensive": ["attacks", "strikes", "slashes", ...],
    "combat_defensive": ["blocks", "parries", "dodges", ...],
    "magic": ["casts", "conjures", "summons", ...],
    "healing": ["heals", "mends", "restores", ...],
    "movement": ["moves", "walks", "runs", ...],
    "stealth": ["sneaks", "creeps", "stalks", ...],
    "interaction": ["opens", "closes", "examines", ...],
    "communication": ["speaks", "shouts", "whispers", ...]
  },
  "patterns": [
    "verb",
    "adverb + verb",
    "verb + preposition"
  ]
}
```

**File Type:** Hybrid (Pattern Generation + Component Library)  
**Purpose:** Generate action descriptions AND provide verb components  
**Issues:**
- ❌ Token `"verb"` doesn't match any component key
- ❌ Token `"adverb"` component doesn't exist!
- ❌ Token `"preposition"` component doesn't exist!
- ❌ Missing metadata
- ⚠️ Patterns are completely broken - none match components

**Recommended Standard Structure:**

**Option A - Fix Patterns (Add Missing Components):**
```json
{
  "components": {
    "combat_offensive": ["attacks", "strikes", "slashes", ...],
    "combat_defensive": ["blocks", "parries", "dodges", ...],
    "magic": ["casts", "conjures", "summons", ...],
    "healing": ["heals", "mends", "restores", ...],
    "movement": ["moves", "walks", "runs", ...],
    "stealth": ["sneaks", "creeps", "stalks", ...],
    "interaction": ["opens", "closes", "examines", ...],
    "communication": ["speaks", "shouts", "whispers", ...],
    "adverb": ["quickly", "slowly", "carefully", "violently", ...],
    "preposition": ["at", "toward", "against", "through", ...]
  },
  "patterns": [
    "combat_offensive",
    "combat_defensive",
    "magic",
    "adverb + combat_offensive",
    "combat_offensive + preposition"
  ],
  "metadata": {
    "description": "Action verbs for combat, magic, and interactions",
    "version": "1.0",
    "type": "pattern_generation"
  }
}
```

**Option B - Component Library (Recommended):**
```json
{
  "components": {
    "combat_offensive": ["attacks", "strikes", "slashes", "stabs", ...],
    "combat_defensive": ["blocks", "parries", "dodges", "evades", ...],
    "magic": ["casts", "conjures", "summons", "channels", ...],
    "healing": ["heals", "mends", "restores", "revives", ...],
    "movement": ["moves", "walks", "runs", "sprints", ...],
    "stealth": ["sneaks", "creeps", "stalks", "prowls", ...],
    "interaction": ["opens", "closes", "examines", "takes", ...],
    "communication": ["speaks", "shouts", "whispers", "murmurs", ...]
  },
  "metadata": {
    "description": "Categorized action verbs for game mechanics",
    "version": "1.0",
    "type": "component_library"
  }
}
```

**Decision Needed:** Are these verbs for pattern generation or just categorized reference data?

**Recommendation:** **Component Library** - Verbs are better used as reference data. The current patterns are too vague.

**Action Required:** ⚠️ Decide purpose, remove items/patterns OR fix patterns completely, add metadata

---

### 9. weather.json

**Current Structure:**

```json
{
  "items": [...],
  "components": {
    "precipitation": [...],
    "wind": [...],
    "sky_condition": [...],
    "temperature": [...],
    "severity": [...],
    "special": [...]
  },
  "patterns": [
    "condition",
    "temperature + precipitation",
    "severity + condition",
    "wind + precipitation"
  ]
}
```

**File Type:** Hybrid (Pattern Generation + Component Library)  
**Purpose:** Generate weather descriptions AND provide weather components  
**Issues:**

- ❌ Token `"condition"` doesn't match any component key
- ❌ Missing metadata
- ⚠️ Unclear pattern structure

**Recommended Standard Structure:**

```json
{
  "components": {
    "precipitation": ["clear", "rainy", "snowy", "sleeting", ...],
    "wind": ["calm", "breezy", "windy", "gusty", ...],
    "sky_condition": ["clear", "cloudy", "overcast", ...],
    "temperature": ["freezing", "cold", "mild", "warm", ...],
    "severity": ["mild", "moderate", "severe", "extreme", ...],
    "special": ["stormy", "thunderous", "lightning", "blizzard", ...]
  },
  "patterns": [
    "precipitation",
    "temperature + precipitation",
    "severity + precipitation",
    "wind + precipitation",
    "special"
  ],
  "metadata": {
    "description": "Weather condition descriptions for environment and gameplay",
    "version": "1.0",
    "type": "pattern_generation"
  }
}
```

**Action Required:** ⚠️ Fix patterns (remove `condition` token), add metadata

---

## Standard File Type Definitions

### Type 1: Component Library

**Purpose:** Provide categorized lists of values for use in other files  
**Pattern Generation:** NO  
**Runtime Usage:** Loaded as reference data, NOT for procedural generation

**Structure:**

```json
{
  "components": {
    "category1": ["Value1", "Value2", ...],
    "category2": ["Value1", "Value2", ...],
    "category3": ["Value1", "Value2", ...]
  },
  "metadata": {
    "description": "Human-readable description",
    "version": "1.0",
    "type": "component_library"
  }
}
```

**Examples:**

- adjectives.json
- materials.json

---

### Type 2: Pattern Generation (Hybrid)

**Purpose:** Generate procedural content using patterns AND provide components  
**Pattern Generation:** YES  
**Runtime Usage:** Patterns executed to generate dynamic descriptions

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
    "type": "pattern_generation"
  }
}
```

**Rules:**

- NO `items` array (use components directly)
- Component keys MUST be singular (match pattern tokens exactly)
- Patterns MUST NOT have comments
- All pattern tokens MUST match component keys

**Examples:**

- colors.json (after fixes)
- time_of_day.json
- weather.json
- smells.json

---

## Migration Plan

### Phase 1: Review Remaining Files (sounds, textures, verbs)

- [ ] Read and audit sounds.json
- [ ] Read and audit textures.json
- [ ] Read and audit verbs.json
- [ ] Classify each as Component Library or Pattern Generation

### Phase 2: Standardize Component Libraries

- [ ] adjectives.json - Add wrappers and metadata
- [ ] materials.json - Add wrappers and metadata
- [ ] (Any others identified in Phase 1)

### Phase 3: Fix Pattern Generation Files

- [ ] colors.json - Fix patterns, decide purpose, standardize
- [ ] time_of_day.json - Remove items, fix component keys, add metadata
- [ ] weather.json - Fix patterns, add metadata
- [ ] smells.json - Fix patterns, remove items, add metadata
- [ ] (Any others identified in Phase 1)

### Phase 4: Update ContentBuilder

- [ ] Add support for "component_library" type
- [ ] Distinguish between Component Library and Pattern Generation in UI
- [ ] Validate pattern generation files (no items array, tokens match components)

---

## Key Decisions Needed

### 1. colors.json Purpose

**Question:** Should colors.json generate color names OR just provide components?

**Option A - Pattern Generation:**

- Generates: "dark red", "bright blue", "crimson", etc.
- Use case: Procedurally describe item colors

**Option B - Component Library:**

- Provides: Lists of colors for other files to use
- Use case: Reference data only

**Recommendation:** Option A (Pattern Generation) - More useful for dynamic descriptions

---

### 2. Remove items Arrays from Pattern Generation Files

**Question:** Should we remove `items` arrays from pattern generation files?

**Current:** Files like colors.json, smells.json have both `items` and `components`  
**Issue:** Duplication and confusion  
**Recommendation:** YES - Remove items, use components directly

**Benefits:**

- ✅ Eliminates duplication
- ✅ Clearer purpose (components ARE the data)
- ✅ Simpler structure
- ✅ Matches names.json pattern (no items, base token resolves from types.json)

---

## Summary Table

| File | Type | Issues | Action |
|------|------|--------|--------|
| adjectives.json | Component Library | Missing wrappers | ✅ Standardize |
| materials.json | Component Library | Missing wrappers | ✅ Standardize |
| **verbs.json** | **Should be Component Library** | **Broken patterns, missing components** | ⚠️ **Fix completely** |
| colors.json | Pattern Generation | Bad patterns, mismatch | ⚠️ Fix patterns |
| time_of_day.json | Pattern Generation | Duplication, plural keys | ✅ Standardize |
| weather.json | Pattern Generation | Bad token | ⚠️ Fix patterns |
| smells.json | Pattern Generation | Bad patterns | ⚠️ Fix patterns |
| sounds.json | Pattern Generation | Token mismatch, no base component | ⚠️ Fix patterns |
| textures.json | Pattern Generation | Token mismatch | ⚠️ Fix patterns |

**Status:**
- ✅ Ready to standardize: 3 files (adjectives, materials, time_of_day)
- ⚠️ Need pattern fixes: 6 files (colors, weather, smells, sounds, textures, verbs)
- � Total: 9 files

---

## Next Steps

1. ✅ **Review remaining 3 files** (sounds, textures, verbs) - COMPLETE
2. **Decide on colors.json purpose** (Pattern Generation recommended)
3. **Decide on verbs.json purpose** (Component Library recommended)
4. **Standardize Component Libraries** (adjectives, materials, verbs)
5. **Fix Pattern Generation files** (colors, time_of_day, weather, smells, sounds, textures)
6. **Update PATTERN_COMPONENT_STANDARDS.md** with file type definitions
7. **Update ContentBuilder** to support both types

## Key Findings

### Good News ✅

- **Clear taxonomy emerged:** 2 distinct file types with different purposes
- **3 files are clean:** adjectives.json and materials.json just need wrappers
- **Standard structures work:** Pattern Generation structure is sound

### Bad News ⚠️

- **6 of 9 files have broken patterns:**
  - Invalid tokens that don't match component keys
  - Comments in pattern strings (unparseable!)
  - Missing base components (sounds, textures)
  
- **verbs.json is completely broken:**
  - Patterns reference non-existent components (adverb, preposition)
  - Should probably be a Component Library instead

### Critical Issues to Fix

1. **Broken Pattern Tokens:**
   - colors.json: `"material (gemstone/metal colors)"` - has comments!
   - smells.json: `"smell + smell (combination)"` - has comments!
   - sounds.json: `"sound"` - no matching component
   - textures.json: `"texture"` - no matching component
   - weather.json: `"condition"` - no matching component
   - verbs.json: `"verb"`, `"adverb"`, `"preposition"` - none exist!

2. **Structural Issues:**
   - All Pattern Generation files have unnecessary `items` arrays
   - Component keys need to be singular (not plural)
   - Missing metadata everywhere

## Recommendations

### Immediate Actions

**Priority 1: Fix Broken Patterns**

1. Remove ALL pattern comments (comments must go in metadata/documentation)
2. Create missing base components (base_sound, surface_quality, etc.)
3. Fix all token mismatches

**Priority 2: Convert verbs.json**

- Convert to Component Library (no patterns)
- Remove broken patterns and items array
- Keep categorized verb lists

**Priority 3: Standardize Simple Files**

- Add wrappers and metadata to adjectives.json, materials.json
- Quick wins to establish pattern

### Long-Term Strategy

**File Type Standards:**

1. **Component Library** (adjectives, materials, verbs)
   - Simple categorized lists
   - No pattern generation
   - Used as reference data

2. **Pattern Generation** (colors, sounds, textures, time_of_day, weather, smells)
   - Generate procedural descriptions
   - Used for dynamic content
   - Can also serve as component source

**Ready to proceed?**

**Suggested First Step:** Fix verbs.json and the two simple Component Libraries (adjectives, materials) as proof of concept, then tackle the Pattern Generation files.
