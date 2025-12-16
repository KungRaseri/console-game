# Pattern Format Cleanup

**Date:** 2025-12-16  
**Status:** Complete  
**Files Updated:** 13

## Summary

Cleaned up all JSON pattern arrays to remove examples in parentheses. Patterns are now pure templates that can be parsed programmatically for name/text generation.

## Changes Made

### Items Category (7 files)

1. **items/weapons/names.json**
   - ✅ Removed examples from 5 patterns
   - Example: `"material + base (Iron Longsword)"` → `"material + base"`

2. **items/armor/names.json**
   - ✅ Removed examples from 5 patterns
   - Example: `"material + piece (Iron Cuirass)"` → `"material + piece"`

3. **items/weapons/suffixes.json**
   - ✅ Removed examples from 5 patterns

4. **items/armor/suffixes.json**
   - ✅ Removed examples from 5 patterns

5. **items/armor/prefixes.json**
   - ✅ Removed examples from 5 patterns

6. **items/enchantments/prefixes.json**
   - ✅ Removed examples from 4 patterns

7. **items/enchantments/effects.json**
   - ✅ Removed examples from 4 patterns

8. **items/consumables/effects.json**
   - ✅ Removed examples from 3 patterns

### NPCs Category (3 files)

9. **npcs/names/first_names.json**
   - ✅ Removed examples from 5 patterns
   - Example: `"classical + variant (Aldric → Alaric)"` → `"classical + variant"`

10. **npcs/names/last_names.json**
    - ✅ Removed examples from 6 patterns

11. **npcs/personalities/traits.json**
    - ✅ Removed examples from 3 patterns

### General Category (1 file)

12. **general/verbs.json**
    - ✅ Removed examples from 3 patterns

## Pattern Format Standard

### Before (Incorrect)
```json
"patterns": [
  "material + base (Iron Longsword, Steel Battleaxe)",
  "quality + material + base (Fine Steel Rapier)"
]
```

### After (Correct)
```json
"patterns": [
  "material + base",
  "quality + material + base"
]
```

## Rationale

Patterns are used programmatically to generate names/text by:
1. Parsing the pattern string
2. Splitting on `" + "`
3. Replacing each token with values from component arrays

Examples in parentheses would break this parsing logic, so they've been removed to create clean, parseable templates.

## Usage Example

For pattern `"material + quality + base"`:
```csharp
// Parse: ["material", "quality", "base"]
// Replace:
//   material → "Steel" (from components.prefixes_material)
//   quality → "Fine" (from components.prefixes_quality)
//   base → "Longsword" (from items array)
// Result: "Steel Fine Longsword"
```

## Files NOT Changed

The following files have descriptive patterns (not templates) and were intentionally left unchanged:
- **npcs/dialogue/greetings.json** - Has descriptive guidelines, not templates
- **quests/rewards/experience.json** - Has formula documentation
- Enemy suffix/trait files - Have usage notes in metadata, not in patterns array
- Most files don't have patterns arrays at all

## Verification

All changes maintain valid JSON syntax and preserve the semantic meaning of each pattern while making them programmatically parseable.
