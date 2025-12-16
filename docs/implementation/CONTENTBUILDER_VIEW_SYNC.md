# ContentBuilder View Synchronization

**Date:** December 16, 2025  
**Issue:** Inconsistent editor types for similar data structures  
**Status:** 🔧 In Progress

## Problem Analysis

Similar data files should use the same editor type for consistency. Currently there are mismatches:

### Current State

| Category | File | Current Editor | Data Structure |
|----------|------|---------------|----------------|
| **Items/Weapons** |
| Weapons/Names | names.json | NameList ✅ | `{ "names": [...] }` |
| Weapons/Prefixes | prefixes.json | **FlatItem** ❌ | `{ "rarity": { "item": { traits } } }` |
| Weapons/Suffixes | suffixes.json | HybridArray ✅ | `{ "items": [...] }` |
| **Items/Armor** |
| Armor/Names | names.json | HybridArray ❓ | Need to check |
| Armor/Prefixes | prefixes.json | HybridArray ✅ | `{ "items": [...] }` |
| Armor/Suffixes | suffixes.json | HybridArray ✅ | `{ "items": [...] }` |
| Armor/Materials | materials.json | FlatItem ✅ | `{ "item": { traits } }` |
| **Items/Enchantments** |
| Enchantments/Prefixes | prefixes.json | HybridArray ❓ | Need to check |
| Enchantments/Effects | effects.json | HybridArray ❓ | Need to check |
| Enchantments/Suffixes | suffixes.json | **FlatItem** ❓ | Need to check |
| **Enemies** |
| Beasts/Prefixes | prefixes.json | FlatItem | `{ "item": { traits } }` |
| Beasts/Suffixes | suffixes.json | HybridArray | `{ "items": [...] }` |
| Dragons/Prefixes | prefixes.json | FlatItem | `{ "item": { traits } }` |
| Dragons/Suffixes | suffixes.json | HybridArray | `{ "items": [...] }` |

### Editor Type Guidelines

**NameList** - Simple string arrays:
```json
{
  "names": ["item1", "item2", "item3"]
}
```

**FlatItem** - Item with traits (2-level):
```json
{
  "ItemName": {
    "displayName": "Item Name",
    "traits": { ... }
  }
}
```

**HybridArray** - Items with components/patterns:
```json
{
  "items": [ { "name": "...", "rarity": "..." } ],
  "components": { ... },
  "patterns": [ ... ]
}
```

**ItemPrefix** - Rarity-based hierarchy (3-level):
```json
{
  "common": {
    "ItemName": {
      "displayName": "Item Name",
      "traits": { ... }
    }
  },
  "rare": { ... }
}
```

## Recommended Changes

### High Priority - Clear Mismatches

1. ✅ **Weapons/Prefixes** - Change from `FlatItem` to `ItemPrefix`
   - File uses rarity-based structure: `{ "common": {...}, "uncommon": {...} }`
   - Should use `ItemPrefix` editor (3-level hierarchy)

### Medium Priority - Needs Verification

2. **Armor/Names** - Verify if HybridArray is correct
3. **Enchantments/Prefixes** - Verify structure matches editor
4. **Enchantments/Suffixes** - Verify if FlatItem is correct

### Pattern Recognition

**All Prefixes/Suffixes should follow:**
- If has rarity levels → `ItemPrefix` or `ItemSuffix`
- If flat array of items → `HybridArray`
- If item→traits only → `FlatItem`

## Implementation Plan

1. ✅ Audit all JSON files to confirm structure
2. ✅ Update MainViewModel.cs with correct EditorType assignments
3. ✅ Test each editor with its assigned files
4. ✅ Document the canonical mapping

## Files to Update

- `Game.ContentBuilder/ViewModels/MainViewModel.cs` - Category assignments
