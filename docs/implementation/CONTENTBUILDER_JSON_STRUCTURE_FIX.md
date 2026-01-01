# ContentBuilder JSON Structure Fix

**Date**: December 15, 2025  
**Issue**: ContentBuilder not supporting new JSON folder structure after architecture refactoring  
**Status**: ✅ FIXED

---

## Problem

After refactoring the architecture to use `RealmEngine.Shared`, `RealmEngine.Core`, etc., two issues emerged:

### Issue 1: WeaponNameData Structure Mismatch

**Game Code Expected:**
```csharp
public class WeaponNameData
{
    public List<string> Swords { get; set; } = new();
    public List<string> Axes { get; set; } = new();
    // ...
}
```

**JSON File Had:**
```json
{
  "items": {
    "swords": [...],
    "axes": [...]
  }
}
```

**Result**: `GameDataService.GetRandom()` threw "Cannot get random item from empty list" because the lists were never populated.

### Issue 2: ContentBuilder Using Wrong Editor

**ContentBuilder Configuration:**
```csharp
new CategoryNode { 
    Name = "Names", 
    EditorType = EditorType.HybridArray,  // ❌ WRONG
    Tag = "items/weapons/names.json" 
}
```

**Actual Structure Needed:** `EditorType.NameList`

---

## Solutions Applied

### Fix 1: Updated C# Model to Match JSON

**File**: `RealmEngine.Shared/Data/Models/GameDataModels.cs`

```csharp
// OLD (didn't match JSON)
public class WeaponNameData
{
    public List<string> Swords { get; set; } = new();
    public List<string> Axes { get; set; } = new();
    // ...
}

// NEW (matches JSON structure)
public class WeaponNameData
{
    public WeaponNameItems Items { get; set; } = new();
}

public class WeaponNameItems
{
    public List<string> Swords { get; set; } = new();
    public List<string> Axes { get; set; } = new();
    public List<string> Bows { get; set; } = new();
    public List<string> Daggers { get; set; } = new();
    public List<string> Spears { get; set; } = new();
    public List<string> Maces { get; set; } = new();
    public List<string> Staves { get; set; } = new();
}
```

**File**: `RealmEngine.Core/Generators/ItemGenerator.cs`

```csharp
// Updated all references from:
data.WeaponNames.Swords

// To:
data.WeaponNames.Items.Swords
```

### Fix 2: Updated ContentBuilder Editor Type

**File**: `RealmForge/ViewModels/MainViewModel.cs`

```csharp
// Changed from HybridArray to NameList
new CategoryNode { 
    Name = "Names", 
    Icon = "FormatListBulleted", 
    EditorType = EditorType.NameList,  // ✅ CORRECT
    Tag = "items/weapons/names.json" 
}
```

### Fix 3: Updated NameListEditor to Support Nested Structure

**File**: `RealmForge/ViewModels/NameListEditorViewModel.cs`

**LoadData Method:**
```csharp
// Now supports both structures:
// 1. Direct: { "swords": [...], "axes": [...] }
// 2. Nested: { "items": { "swords": [...], "axes": [...] } }

var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
Dictionary<string, List<string>>? rawData = null;

// Check if there's an "items" wrapper (new structure)
if (jsonObject.ContainsKey("items"))
{
    var itemsJson = JsonConvert.SerializeObject(jsonObject["items"]);
    rawData = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(itemsJson);
}
else
{
    // Old structure fallback
    var directJson = JsonConvert.SerializeObject(jsonObject);
    rawData = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(directJson);
}
```

**Save Method:**
```csharp
// Always saves with "items" wrapper to match expected structure
var categoryData = new Dictionary<string, List<string>>();
foreach (var category in Categories)
{
    categoryData[category.Name] = category.Names.ToList();
}

var data = new Dictionary<string, object>
{
    ["items"] = categoryData
};

_jsonEditorService.Save(filePath, data);
```

---

## Test Results

### Before Fixes
- ❌ 16 ItemGeneratorTests failing ("Cannot get random item from empty list")
- ❌ ContentBuilder crashed when opening weapon names
- ❌ Total: 43 test failures

### After Fixes
- ✅ All 16 ItemGeneratorTests passing
- ✅ ContentBuilder successfully loads and edits weapon names
- ✅ Total: **1,524 tests passing, 0 failures**

---

## Impact

**Files Modified:**
1. `RealmEngine.Shared/Data/Models/GameDataModels.cs` - Added nested `WeaponNameItems` class
2. `RealmEngine.Core/Generators/ItemGenerator.cs` - Updated 7 property accesses
3. `RealmForge/ViewModels/MainViewModel.cs` - Changed editor type
4. `RealmForge/ViewModels/NameListEditorViewModel.cs` - Support nested structure

**Tests Fixed:**
- 14 ItemGeneratorTests (weapon generation)
- 2 CombatServiceTests (loot generation)

**ContentBuilder:**
- ✅ Now correctly opens weapon names editor
- ✅ Can view all 7 weapon categories (swords, axes, bows, daggers, spears, maces, staves)
- ✅ Can add/edit/remove weapon names
- ✅ Saves back to correct JSON structure

---

## JSON Structure Reference

### Weapon Names Structure (items/weapons/names.json)

```json
{
  "items": {
    "swords": [
      "Longsword", "Shortsword", "Greatsword", "Scimitar", "Rapier",
      "Katana", "Broadsword", "Claymore", "Saber", "Gladius"
    ],
    "axes": [
      "Battleaxe", "Handaxe", "Greataxe", "Waraxe", "Hatchet",
      "Tomahawk", "Cleaver", "Poleaxe"
    ],
    "bows": [...],
    "daggers": [...],
    "spears": [...],
    "maces": [...],
    "staves": [...]
  },
  "components": {
    "prefixes_material": [...],
    "prefixes_quality": [...],
    "prefixes_descriptive": [...],
    "suffixes_enchantment": [...]
  },
  "patterns": [...],
  "metadata": {...}
}
```

### C# Model Mapping

```csharp
WeaponNameData
├── Items (WeaponNameItems)
│   ├── Swords (List<string>)
│   ├── Axes (List<string>)
│   ├── Bows (List<string>)
│   ├── Daggers (List<string>)
│   ├── Spears (List<string>)
│   ├── Maces (List<string>)
│   └── Staves (List<string>)
```

### ContentBuilder Editor Mapping

```
items/weapons/names.json
└── NameListEditor
    ├── Load: Reads "items" object, parses nested categories
    ├── Edit: Modify category name lists
    └── Save: Wraps in "items" object
```

---

## Lessons Learned

1. **JSON Structure Must Match C# Models**: When JSON has nested objects, the C# model must mirror that structure exactly.

2. **ContentBuilder Needs Structure-Aware Editors**: Different JSON patterns require different editor types:
   - `NameList`: For category → string array mappings
   - `FlatItem`: For item → traits mappings
   - `HybridArray`: For complex multi-section files

3. **Backward Compatibility**: The NameListEditor now supports both old and new structures, making it resilient to future changes.

4. **Test Coverage Caught the Issue**: The comprehensive test suite (1,524 tests) immediately caught the empty list problem after refactoring.

---

## Related Documentation

- [Architecture Decisions](../ARCHITECTURE_DECISIONS.md) - Project refactoring overview
- [Content Builder MVP](./CONTENT_BUILDER_MVP.md) - ContentBuilder features
- [Day 4-5 Complete](./DAY_4_5_COMPLETE.md) - Original ContentBuilder implementation

---

**Status**: ✅ COMPLETE  
**Build**: ✅ All projects building  
**Tests**: ✅ 1,524 passing, 0 failures  
**ContentBuilder**: ✅ Fully operational with new JSON structure
