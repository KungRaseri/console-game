# Phase 1 Complete: Token-Aware Naming Components Infrastructure

**Date**: December 29, 2025  
**Status**: ✅ COMPLETE  
**Next Phase**: Phase 2 - Update NameComposer and Generators

---

## Overview

Successfully migrated all domain models from fixed naming properties to flexible token-aware `List<NameComponent>` structures. This enables variable-length component lists, preserves semantic meaning, and provides better debugging/UI support.

---

## What Was Implemented

### 1. ✅ Created NameComponent Foundation Class

**File**: `RealmEngine.Shared/Models/NameComponent.cs`

```csharp
public class NameComponent
{
    public string Token { get; set; } = string.Empty;  // Semantic identifier
    public string Value { get; set; } = string.Empty;  // Display text
}
```

**Purpose**: Preserves both token identity (e.g., `"material"`, `"element_prefix"`) and display value (e.g., `"Mithril"`, `"Flaming"`).

**Benefits**:
- Variable number of components (no fixed property count)
- Token-based lookup for filtering/debugging
- Godot can iterate lists easily
- Future-proof for adding new component types

---

### 2. ✅ Updated All Domain Models

#### Item Model (`RealmEngine.Shared/Models/Item.cs`)

**Added Properties**:
```csharp
public List<NameComponent> Prefixes { get; set; } = new();
public List<NameComponent> Suffixes { get; set; } = new();
```

**Added Helper Methods**:
- `GetPrefixValue(string token)` - Find prefix by token name
- `GetSuffixValue(string token)` - Find suffix by token name
- `ComposeNameFromComponents()` - Rebuild name from new structure

**Legacy Properties (Temporary - Phase 3 Removal)**:
- `QualityPrefix`, `MaterialPrefix` (marked TEMPORARY)
- `EnchantmentPrefixes`, `EnchantmentSuffixes`
- `SocketsText`

---

#### Enemy Model (`RealmEngine.Shared/Models/Enemy.cs`)

**Added Properties**:
```csharp
public List<NameComponent> Prefixes { get; set; } = new();
public List<NameComponent> Suffixes { get; set; } = new();
```

**Added Helper Methods**:
- `GetPrefixValue(string token)`
- `GetSuffixValue(string token)`
- `ComposeNameFromComponents()`

**Legacy Properties (Temporary)**:
- `SizePrefix`, `TypePrefix`, `DescriptivePrefix`
- `TitleSuffix`

---

#### Ability Model (`RealmEngine.Shared/Models/Ability.cs`)

**Added Properties**:
```csharp
public List<NameComponent> Prefixes { get; set; } = new();
// Note: Abilities typically don't have suffixes
```

**Added Helper Methods**:
- `GetPrefixValue(string token)`
- `ComposeDisplayNameFromComponents()`

**Legacy Properties (Temporary)**:
- `BaseAbilityName`, `PowerPrefix`, `SchoolPrefix`

---

#### NPC Model (`RealmEngine.Shared/Models/NPC.cs`)

**Added Properties**:
```csharp
public List<NameComponent> Prefixes { get; set; } = new();
public List<NameComponent> Suffixes { get; set; } = new();
```

**Added Helper Methods**:
- `GetPrefixValue(string token)`
- `GetSuffixValue(string token)`
- `ComposeNameFromComponents()`

**Legacy Properties (Temporary)**:
- `TitlePrefix`, `TitleSuffix`

---

## Architecture Decisions

### Positional Approach (Finalized)

**Rule**: Position relative to `{base}` token determines prefix/suffix categorization.

**Pattern Examples**:
```
{quality} {material} {base} {element_suffix}
^         ^          ^      ^
Prefix    Prefix     Base   Suffix
```

**Benefits**:
- Simple and consistent
- No explicit position metadata needed
- Convention over configuration
- Easy to understand patterns

---

### Nomenclature Convention (Simplified)

**Use `_prefix` / `_suffix` suffixes ONLY when the same concept has different linguistic forms.**

**Examples Where Suffixes ARE Needed**:
- **Enchantments**: 
  - `element_prefix` → "Flaming" (adjective)
  - `element_suffix` → "of Fire" (prepositional phrase)
- **NPCs**:
  - `title_prefix` → "Master" (title before name)
  - `title_suffix` → "the Wise" (epithet after name)

**Examples Where Suffixes NOT Needed**:
- **Enemies**: `size`, `type` (all are adjectives, form-stable)
- **Items**: `quality`, `material` (all are adjectives)
- **Abilities**: `power`, `school` (all are adjectives)

---

## Migration Strategy

### No Backward Compatibility

**Decision**: Clean break - old properties will be deleted in Phase 3.

**Rationale**:
- Avoids dual-maintenance burden
- Simpler codebase
- Forces migration to better architecture
- Game is in development (no production data to migrate)

### Temporary Legacy Properties

**Phase 1-2**: Keep old properties alongside new ones (compilation safety)  
**Phase 3**: Delete old properties and update all generators/tests

---

## Build Status

✅ **All projects compiled successfully**

```
Restore complete (1.9s)
  RealmEngine.Shared succeeded (3.1s)
  RealmEngine.Core succeeded (2.9s)
  RealmEngine.Data succeeded (1.1s)
  RealmEngine.Shared.Tests succeeded (2.2s)
  RealmEngine.Core.Tests succeeded (4.3s)
  RealmEngine.Data.Tests succeeded (3.4s)
  RealmForge succeeded (9.5s)
  RealmForge.Tests succeeded (1.7s)

Build succeeded with 5 warning(s) in 17.7s
```

**Note**: Warnings are unrelated (event handlers, nullable references).

---

## Example Usage (Future)

### C# - Accessing Components

```csharp
// Create item with components
var item = new Item
{
    Name = "Flaming Masterwork Mithril Longsword of Power",
    BaseName = "Longsword",
    Prefixes = new List<NameComponent>
    {
        new() { Token = "element_prefix", Value = "Flaming" },
        new() { Token = "quality", Value = "Masterwork" },
        new() { Token = "material", Value = "Mithril" }
    },
    Suffixes = new List<NameComponent>
    {
        new() { Token = "element_suffix", Value = "of Power" }
    }
};

// Get specific component
string? material = item.GetPrefixValue("material"); // "Mithril"

// Rebuild name
string name = item.ComposeNameFromComponents();

// Filter items by material
var mithrilItems = items.Where(i => i.GetPrefixValue("material") == "Mithril");
```

---

### Godot - Iterating Components

```gdscript
# Display item components in UI
func display_item_details(item):
    print("Base: ", item.base_name)
    
    print("Prefixes:")
    for component in item.prefixes:
        print("  [", component.token, "] = ", component.value)
    
    print("Suffixes:")
    for component in item.suffixes:
        print("  [", component.token, "] = ", component.value)

# Filter by material
func find_mithril_items(inventory):
    var result = []
    for item in inventory:
        for prefix in item.prefixes:
            if prefix.token == "material" and prefix.value == "Mithril":
                result.append(item)
    return result
```

---

## Testing Status

### Current Tests (12 passing)

**Location**: `RealmEngine.Core.Tests/Generators/Modern/ItemNamingComponentsTests.cs`

**Coverage**:
- ✅ Enchantment position bug fixed
- ✅ Quality and material prefixes populated
- ✅ BaseName set correctly
- ✅ Enchantments added as prefixes/suffixes
- ✅ Composed name matches expected format

**Note**: Tests currently use OLD properties. Will be updated in Phase 3.

---

## Next Steps: Phase 2 - Update NameComposer

### 2.1 Add ComposeNameWithComponents() Method

**File**: `RealmEngine.Core/Generators/NameComposer.cs`

**Method Signature**:
```csharp
public (string name, string baseName, List<NameComponent> prefixes, List<NameComponent> suffixes) 
    ComposeNameWithComponents(string pattern, JToken components)
```

**Algorithm**:
1. Parse pattern and find `{base}` token position
2. Resolve all tokens to values
3. Categorize tokens as prefix (before base) or suffix (after base)
4. Preserve order within each category
5. Return structured result with token-value pairs

---

### 2.2 Update ItemGenerator

**File**: `RealmEngine.Core/Generators/Modern/ItemGenerator.cs`

**Changes**:
1. Call `NameComposer.ComposeNameWithComponents()` instead of `ComposeName()`
2. Populate new `Prefixes` and `Suffixes` lists
3. Keep populating old properties temporarily (Phase 3 removal)

---

### 2.3 Verify Tests Still Pass

Run existing tests to ensure backward compatibility during migration:

```bash
dotnet test --filter "FullyQualifiedName~ItemNamingComponentsTests"
```

---

## Phase 3 Preview: Clean Break

### 3.1 Delete Old Properties

**From Item**:
- `QualityPrefix`, `MaterialPrefix`
- `EnchantmentPrefixes`, `EnchantmentSuffixes`
- `SocketsText` (move to separate property)

**From Enemy**:
- `SizePrefix`, `TypePrefix`, `DescriptivePrefix`, `TitleSuffix`

**From Ability**:
- `BaseAbilityName`, `PowerPrefix`, `SchoolPrefix`

**From NPC**:
- `TitlePrefix`, `TitleSuffix`

---

### 3.2 Update All Tests

**Files to Update**:
- `RealmEngine.Core.Tests/Generators/Modern/ItemNamingComponentsTests.cs`
- Any other tests using old naming properties

**Changes**:
```csharp
// OLD
Assert.Equal("Masterwork", item.QualityPrefix);

// NEW
Assert.Equal("Masterwork", item.GetPrefixValue("quality"));
Assert.Contains(item.Prefixes, p => p.Token == "quality" && p.Value == "Masterwork");
```

---

### 3.3 Remove Legacy ComposeNameFromComponents()

Delete `ComposeNameFromComponentsLegacy()` helper method from `Item.cs`.

---

## Success Criteria Checklist

### Phase 1 (COMPLETE) ✅
- [x] Created `NameComponent` class
- [x] Updated Item model with Prefixes/Suffixes lists
- [x] Updated Enemy model with Prefixes/Suffixes lists
- [x] Updated Ability model with Prefixes list
- [x] Updated NPC model with Prefixes/Suffixes lists
- [x] Added helper methods to all models
- [x] Build successful (no compilation errors)

### Phase 2 (PENDING) ⏸️
- [ ] Implement `NameComposer.ComposeNameWithComponents()`
- [ ] Update `ItemGenerator.BuildEnhancedName()` to use new method
- [ ] Populate new Prefixes/Suffixes lists in generator
- [ ] Verify all tests still pass

### Phase 3 (PENDING) ⏸️
- [ ] Delete old properties from all models
- [ ] Fix compilation errors
- [ ] Update all tests to use new structure
- [ ] Add new tests for helper methods
- [ ] All tests passing

### Phase 4 (OPTIONAL/FUTURE) ⏸️
- [ ] Refactor enchantments names.json to be positional
- [ ] Remove "position" metadata from enchantment components
- [ ] Update documentation

---

## Documentation

**Related Files**:
- `docs/implementation/GRANULAR_NAMING_COMPONENTS.md` - Original design doc
- `docs/implementation/GRANULAR_NAMING_SUMMARY.md` - Architecture summary
- This file - Phase 1 completion report

**Standards**:
- JSON v4.1 Reference System
- Positional naming convention
- Token-aware component structure

---

## Key Takeaways

### What Worked Well

1. **Positional approach is simple** - No metadata overhead
2. **Token preservation valuable** - Enables filtering, debugging, UI
3. **List structure flexible** - Variable components without property explosion
4. **No backward compatibility** - Clean architecture, faster progress

### Design Decisions Validated

1. **Keep BaseName separate** - Useful for unidentified items, recipe lookups
2. **Keep Sockets separate** - Not part of name composition (mechanical property)
3. **Simplify nomenclature** - Only use _prefix/_suffix where forms actually vary
4. **Temporary dual properties** - Safer migration path during development

---

## Commands Reference

```bash
# Build project
dotnet build RealmEngine.Shared/RealmEngine.Shared.csproj
dotnet build RealmEngine.Core/RealmEngine.Core.csproj

# Run tests
dotnet test --filter "FullyQualifiedName~ItemNamingComponentsTests"
dotnet test --filter "FullyQualifiedName~NameComposerTests"

# Full build
dotnet build RealmEngine.sln
```

---

## Contact for Phase 2

Ready to proceed with Phase 2: Update NameComposer and ItemGenerator to populate new structure.

**Estimated Time**: 1-2 hours  
**Risk Level**: Low (backward compatible during Phase 2)  
**Blockers**: None

---

*Phase 1 infrastructure complete! All models ready for generator updates.* 🎉
