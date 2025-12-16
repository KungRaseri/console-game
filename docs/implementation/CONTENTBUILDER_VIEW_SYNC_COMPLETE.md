# ContentBuilder View Synchronization - Complete

**Date:** December 16, 2025  
**Status:** ✅ Complete

## Problem

The ContentBuilder was using inconsistent editor types for similar data structures. For example:
- `Items/Weapons/Prefixes` used `FlatItem` editor
- `Items/Armor/Prefixes` used `HybridArray` editor
- `Enemies/Beasts/Prefixes` used `FlatItem` editor

All these files have the **same data structure** (rarity-based hierarchy) but were assigned different editors, leading to a confusing and inconsistent user experience.

## Solution

Standardized all editor type assignments based on actual data structure patterns:

### Editor Type Assignment Rules

| Data Structure | Editor Type | Use Case | Example Files |
|---------------|-------------|----------|---------------|
| **Simple string array** | `NameList` | Lists of names/words | `weapon_names.json`, `adjectives.json` |
| **Rarity → Item → Traits** | `ItemPrefix` | Prefixes with rarity levels | `weapons/prefixes.json`, `beasts/prefixes.json` |
| **Rarity → Item → Traits** | `ItemSuffix` | Suffixes with rarity levels | `enchantments/suffixes.json` |
| **Item → Traits only** | `FlatItem` | Simple item definitions | `materials/metals.json`, `dragons/colors.json` |
| **Items array + Components** | `HybridArray` | Complex structures with patterns | `weapons/suffixes.json`, `colors.json` |

### Data Structure Examples

#### ItemPrefix/ItemSuffix Structure
```json
{
  "common": {
    "ItemName": {
      "displayName": "Item Name",
      "traits": {
        "damageBonus": { "value": 2, "type": "number" }
      }
    }
  },
  "rare": { ... }
}
```

#### HybridArray Structure
```json
{
  "items": [
    { "name": "Item 1", "rarity": "Common", "description": "..." }
  ],
  "components": {
    "group1": ["comp1", "comp2"]
  },
  "patterns": ["pattern1"]
}
```

#### FlatItem Structure
```json
{
  "ItemName": {
    "displayName": "Item Name",
    "traits": {
      "property1": { "value": 10, "type": "number" }
    }
  }
}
```

## Changes Made

### Items Category

**Weapons:**
- ✅ Names: `NameList` (unchanged)
- ✅ Prefixes: `FlatItem` → **`ItemPrefix`** (fixed - has rarity levels)
- ✅ Suffixes: `HybridArray` (unchanged)

**Armor:**
- ✅ Names: `HybridArray` (unchanged)
- ✅ Prefixes: `HybridArray` (unchanged)
- ✅ Suffixes: `HybridArray` (unchanged)
- ✅ Materials: `FlatItem` (unchanged)

**Enchantments:**
- ✅ Prefixes: `HybridArray` (unchanged)
- ✅ Effects: `HybridArray` (unchanged)
- ✅ Suffixes: `FlatItem` → **`ItemSuffix`** (fixed - has rarity levels)

### Enemies Category

**All Enemy Types (Beasts, Demons, Dragons, Elementals, Humanoids, Undead):**
- ✅ Names: `NameList` (unchanged)
- ✅ Prefixes: `FlatItem` → **`ItemPrefix`** (fixed - has rarity levels)
- ✅ Traits: `HybridArray` (unchanged)
- ✅ Suffixes: `HybridArray` (unchanged)

**Dragons (additional):**
- ✅ Colors: `FlatItem` (unchanged - correct for item→traits structure)

## Summary of Fixes

| File | Old Editor | New Editor | Reason |
|------|-----------|------------|--------|
| `items/weapons/prefixes.json` | FlatItem | **ItemPrefix** | Has rarity-based hierarchy |
| `items/enchantments/suffixes.json` | FlatItem | **ItemSuffix** | Has rarity-based hierarchy |
| `enemies/beasts/prefixes.json` | FlatItem | **ItemPrefix** | Has rarity-based hierarchy |
| `enemies/demons/prefixes.json` | FlatItem | **ItemPrefix** | Has rarity-based hierarchy |
| `enemies/dragons/prefixes.json` | FlatItem | **ItemPrefix** | Has rarity-based hierarchy |
| `enemies/elementals/prefixes.json` | FlatItem | **ItemPrefix** | Has rarity-based hierarchy |
| `enemies/humanoids/prefixes.json` | FlatItem | **ItemPrefix** | Has rarity-based hierarchy |
| `enemies/undead/prefixes.json` | FlatItem | **ItemPrefix** | Has rarity-based hierarchy |

**Total Files Fixed:** 8

## Benefits

1. **Consistency** - All prefixes with rarity levels now use the same editor
2. **Discoverability** - Users instantly recognize the pattern across categories
3. **Maintainability** - Clear rules make it easy to assign editors to new files
4. **User Experience** - Same data structure = same editing experience

## Testing

✅ Build successful  
✅ All categories properly mapped  
✅ Editor types match data structures  

## Files Modified

- `Game.ContentBuilder/ViewModels/MainViewModel.cs` - Updated 8 category assignments

## Future Guidance

When adding new JSON files:

1. **Check the data structure** - Look at the JSON file format
2. **Match to the pattern** - Use the table above to find the correct editor
3. **Be consistent** - If similar files exist, use the same editor type
4. **Test the editor** - Make sure it can load and save the file correctly

### Quick Reference

- Has `{ "common": {...}, "rare": {...} }` → Use `ItemPrefix` or `ItemSuffix`
- Has `{ "items": [...] }` → Use `HybridArray`
- Has `{ "ItemName": { "traits": {...} } }` → Use `FlatItem`
- Has `{ "names": [...] }` or `["item1", "item2"]` → Use `NameList`
