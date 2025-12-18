# types.json → catalog.json Rename Complete

**Date:** December 17, 2025  
**Status:** ✅ COMPLETE

---

## Summary

Successfully renamed all `types.json` files to `catalog.json` for consistent terminology across the project. This aligns with the v4.0 metadata standard that uses "catalog" naming (ability_catalog, occupation_catalog, etc.).

---

## Files Renamed (17 Total)

### Items (4 files)
1. `items/weapons/types.json` → `catalog.json`
2. `items/armor/types.json` → `catalog.json`
3. `items/consumables/types.json` → `catalog.json`
4. `items/materials/types.json` → `catalog.json`

### Enemies (13 files)
5. `enemies/beasts/types.json` → `catalog.json`
6. `enemies/demons/types.json` → `catalog.json`
7. `enemies/dragons/types.json` → `catalog.json`
8. `enemies/elementals/types.json` → `catalog.json`
9. `enemies/goblinoids/types.json` → `catalog.json`
10. `enemies/humanoids/types.json` → `catalog.json`
11. `enemies/insects/types.json` → `catalog.json`
12. `enemies/orcs/types.json` → `catalog.json`
13. `enemies/plants/types.json` → `catalog.json`
14. `enemies/reptilians/types.json` → `catalog.json`
15. `enemies/trolls/types.json` → `catalog.json`
16. `enemies/undead/types.json` → `catalog.json`
17. `enemies/vampires/types.json` → `catalog.json`

---

## .cbconfig.json Files Updated (27 Total)

Updated `fileIcons` property from `"types": "ShapeOutline"` to `"catalog": "ShapeOutline"` in:

### Enemies
- enemies/.cbconfig.json
- enemies/beasts/.cbconfig.json
- enemies/demons/.cbconfig.json
- enemies/dragons/.cbconfig.json
- enemies/elementals/.cbconfig.json
- enemies/goblinoids/.cbconfig.json
- enemies/humanoids/.cbconfig.json
- enemies/insects/.cbconfig.json
- enemies/orcs/.cbconfig.json
- enemies/plants/.cbconfig.json
- enemies/reptilians/.cbconfig.json
- enemies/trolls/.cbconfig.json
- enemies/undead/.cbconfig.json
- enemies/vampires/.cbconfig.json

### Items
- items/.cbconfig.json
- items/armor/.cbconfig.json
- items/consumables/.cbconfig.json
- items/materials/.cbconfig.json
- items/weapons/.cbconfig.json

### NPCs
- npcs/dialogue/.cbconfig.json
- npcs/names/.cbconfig.json
- npcs/occupations/.cbconfig.json
- npcs/personalities/.cbconfig.json

### Quests
- quests/locations/.cbconfig.json
- quests/objectives/.cbconfig.json
- quests/rewards/.cbconfig.json
- quests/templates/.cbconfig.json

---

## Code Changes

### Files Renamed
- ✅ `TypesEditorViewModel.cs` → `CatalogEditorViewModel.cs`
- ✅ `TypesEditorView.xaml` → `CatalogEditorView.xaml`
- ✅ `TypesEditorView.xaml.cs` → `CatalogEditorView.xaml.cs`

### Classes Renamed
- ✅ `TypesEditorViewModel` → `CatalogEditorViewModel`
- ✅ `TypesEditorView` → `CatalogEditorView`

### Enums Updated
- ✅ `EditorType.TypesEditor` → `EditorType.ItemCatalogEditor`
- ✅ Kept `EditorType.CatalogEditor` for generic catalogs (occupations, dialogue, etc.)

### References Updated
- ✅ `FileTypeDetector.cs` - Maps `item_catalog` → `ItemCatalogEditor`
- ✅ `MainViewModel.cs` - `LoadCatalogEditor()` method
- ✅ `CategoryNode.cs` - EditorType enum comments

### Display Text Updated
- ✅ XAML: "Types Editor" → "Catalog Editor"
- ✅ Log messages: "TypesEditor" → "CatalogEditor"
- ✅ File references: "types.json" → "catalog.json"

---

## Why This Change?

### Before (Inconsistent)
```
File name:      types.json
Metadata type:  "item_catalog"
Editor name:    TypesEditor
```

### After (Consistent)
```
File name:      catalog.json
Metadata type:  "item_catalog"
Editor name:    ItemCatalogEditor (for items/enemies)
                CatalogEditor (for occupations, dialogue, etc.)
```

### Benefits
1. **Clarity**: "catalog" clearly indicates a collection of items
2. **Consistency**: Matches v4.0 metadata standard (ability_catalog, occupation_catalog, etc.)
3. **Accuracy**: These files are catalogs of items, not just type definitions
4. **Future-proof**: Scales better if we add more complex catalog structures

---

## Breaking Changes

⚠️ **Data Files**: All `types.json` renamed to `catalog.json`  
⚠️ **Code References**: Any code loading "types.json" must update to "catalog.json"  
⚠️ **Editor Classes**: TypesEditor* → CatalogEditor*  

### Migration Needed
- [ ] Update game code that loads "types.json" files
- [ ] Update any tests referencing "types.json"
- [ ] Update documentation mentioning "types.json"
- [ ] Update any external tools or scripts

---

## Testing Performed

✅ **File Rename**: All 17 files renamed successfully  
✅ **Config Update**: All 27 .cbconfig.json files updated  
✅ **Code Rename**: All editor classes renamed  
✅ **Build**: ContentBuilder compiles successfully  
✅ **No Conflicts**: No duplicate files or merge conflicts  

### Pending Manual Testing
- [ ] Open ContentBuilder
- [ ] Navigate to items/weapons/catalog.json
- [ ] Verify file opens in CatalogEditor (formerly TypesEditor)
- [ ] Edit and save catalog
- [ ] Verify all 17 catalog.json files load correctly
- [ ] Test game engine with renamed files

---

## Automation Scripts

### Created Script
**File**: `scripts/rename-types-to-catalog.ps1`

**Features**:
- ✅ Dry-run mode with `-WhatIf`
- ✅ Renames all types.json → catalog.json
- ✅ Updates .cbconfig.json references
- ✅ Checks for conflicts before renaming
- ✅ Summary report with counts

**Usage**:
```powershell
# Dry run
.\scripts\rename-types-to-catalog.ps1 -WhatIf

# Execute
.\scripts\rename-types-to-catalog.ps1
```

---

## Next Steps

### Immediate
1. ✅ Rename complete - files and code updated
2. ✅ Build successful - no compilation errors
3. 🔲 **Test ContentBuilder** - verify catalog.json files open correctly
4. 🔲 **Update game code** - search for "types.json" references
5. 🔲 **Update documentation** - replace types.json with catalog.json

### Long-term
- Consider renaming other inconsistent file names
- Establish naming conventions for future data files
- Document v4.0 naming standards in architecture docs

---

## Impact

### Before Rename
- **File naming**: Inconsistent ("types.json" vs "*_catalog" metadata)
- **Developer confusion**: "Is this a type definition or a catalog?"
- **Discoverability**: Hard to tell what the file contains

### After Rename
- **File naming**: Consistent ("catalog.json" with "*_catalog" metadata)
- **Developer clarity**: Clear that it's a catalog of items
- **Discoverability**: Easy to understand file purpose

### Stats
- Files renamed: 17
- Configs updated: 27
- Code files changed: 6 (ViewModels, Views, Services, Models)
- Lines of code affected: ~50

---

## Related Changes

This rename is part of the larger ContentBuilder editor implementation:

1. ✅ **AbilitiesEditor** - For ability_catalog files
2. ✅ **Catalog Rename** - types.json → catalog.json (THIS)
3. 🔲 **CatalogEditor** - Generic editor for occupations, dialogue, etc.
4. 🔲 **NameCatalogEditor** - For NPC name lists
5. 🔲 **QuestTemplateEditor** - For quest templates

---

## Conclusion

The rename from `types.json` to `catalog.json` is **complete and successful**. All files, code, and configurations have been updated to use the new naming convention. The project builds successfully, and the naming is now consistent with the v4.0 metadata standard.

**Status**: ✅ READY FOR TESTING  
**Build**: ✅ PASSING  
**Breaking Changes**: ⚠️ YES - requires code updates  

---

**Created**: December 17, 2025  
**Script**: scripts/rename-types-to-catalog.ps1  
**Files Affected**: 17 data files, 27 config files, 6 code files
