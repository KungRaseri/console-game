# ContentBuilder Dynamic File Tree - Testing Guide

## Quick Summary

We replaced the **350-line hardcoded file tree** in MainViewModel with a **dynamic file scanner** that automatically discovers all JSON files.

## What Changed

### Before
```csharp
private void InitializeCategories()
{
    Categories = new ObservableCollection<CategoryNode>
    {
        new CategoryNode { Name = "General", Icon = "Earth", Children = ... },
        new CategoryNode { Name = "Items", Icon = "Sword", Children = ... },
        // ... 350+ lines of manual category definitions
    };
}
```

### After
```csharp
private void InitializeCategories()
{
    // Build category tree dynamically from file system
    Categories = _fileTreeService.BuildCategoryTree();
}
```

## How It Works

1. **FileTreeService** scans `RealmEngine.Shared/Data/Json` recursively
2. For each directory, creates a CategoryNode with appropriate icon
3. For each JSON file, creates a child node with:
   - Formatted display name (snake_case → Title Case)
   - Appropriate icon based on file name
   - Detected editor type based on file content
   - Relative file path in Tag property

## Benefits

✅ **Automatically includes new files** - No code changes needed when adding JSON files  
✅ **Self-maintaining** - Tree always matches actual file structure  
✅ **Reduced code** - From ~350 lines to 4 lines (95% reduction)  
✅ **Easier to maintain** - No manual synchronization needed  
✅ **Shows new files immediately** - goblinoids/prefixes.json and others now visible  

## Testing When ContentBuilder Launches

### 1. Check Top-Level Categories
Look for these categories in the tree:
- 📁 General
- 📁 Items  
- 📁 Enemies
- 📁 NPCs
- 📁 Quests

### 2. Expand Enemies Category
You should now see **13 enemy types** (not the original 6):
- Beasts (has names, prefixes, traits, suffixes) ✅
- Demons (has names, prefixes, traits, suffixes) ✅
- Dragons (has names, prefixes, colors, traits, suffixes) ✅
- Elementals (has names, prefixes, traits, suffixes) ✅
- **Goblinoids (has prefixes, traits, suffixes) ⭐ NEW**
- Humanoids (has names, prefixes, traits, suffixes) ✅
- **Insects (has prefixes, traits, suffixes) ⭐ NEW**
- **Orcs (has prefixes, traits, suffixes) ⭐ NEW**
- **Plants (has prefixes, traits, suffixes) ⭐ NEW**
- **Reptilians (has prefixes, traits, suffixes) ⭐ NEW**
- **Trolls (has prefixes, traits, suffixes) ⭐ NEW**
- Undead (has names, prefixes, traits, suffixes) ✅
- **Vampires (has prefixes, traits, suffixes) ⭐ NEW**

### 3. Test Loading Editors

Click on each type of file to verify the correct editor loads:

**ItemPrefix Editor:**
- items/weapons/prefixes.json
- enemies/beasts/prefixes.json
- enemies/goblinoids/prefixes.json ⭐ NEW

**FlatItem Editor:**
- items/materials/metals.json
- items/materials/woods.json
- npcs/occupations/common.json

**NameList Editor:**
- npcs/names/first_names.json
- npcs/names/last_names.json

**HybridArray Editor:**
- general/colors.json
- items/weapons/names.json
- enemies/beasts/traits.json

### 4. Verify Icons

Check that icons match the content:
- 🗡️ Weapons (SwordCross)
- 🛡️ Armor (ShieldOutline)
- 💀 Enemies (Skull)
  - 🐾 Beasts (Paw)
  - 🔥 Demons (Fire)
  - 🐲 Dragons (Creation)
  - 💧 Elementals (Water)
  - 👋 Humanoids (HumanGreeting)
  - 👻 Undead (Ghost)
  - 🧛 Vampires (VampireFangs)
  - 👺 Goblinoids (MonsterGoblin)
  - 🗡️ Orcs (MonsterOrc)
  - 🐛 Insects (Bug)
  - 🌱 Plants (Sprout)
  - 🐍 Reptilians (Snake)
  - 👹 Trolls (MonsterTroll)

## Troubleshooting

### Tree is Empty
- Check that `RealmEngine.Shared/Data/Json` exists
- Check the log output in console for errors
- Verify directory path in MainViewModel.GetDataDirectory()

### File Missing from Tree
- Ensure file has `.json` extension
- Check file is in a subdirectory (files in root won't show)
- Look for errors in Serilog output

### Wrong Editor Loads
- Check FileTreeService.DetermineEditorType() logic
- File content determines editor type
- Can override by modifying detection heuristics

### Icons Don't Match
- Add icon mapping to FileTreeService.CategoryIcons or FileIcons dictionaries
- Icons use Material Design icon names

## Log Output

When ContentBuilder starts, you should see log messages like:
```
[INF] Building category tree from: C:\code\console-game\RealmEngine.Shared\Data\Json
[INF] Category tree built - 5 top-level categories, 93 total files
```

If file count doesn't match expected (~93-100 files), investigate.

## Next Steps After Testing

1. ✅ Verify all 13 enemy types appear
2. ✅ Test that new prefix files load correctly
3. ✅ Confirm metadata panels work with restructured JSON
4. 📋 Update documentation if needed
5. 📋 Consider adding file watcher for auto-refresh
6. 📋 Add search/filter capability to tree

## Related Files

- `RealmForge/Services/FileTreeService.cs` - Core scanning logic
- `RealmForge/ViewModels/MainViewModel.cs` - Updated to use service
- `docs/implementation/CONTENTBUILDER_DYNAMIC_TREE.md` - Full documentation
- `docs/implementation/PREFIX_SUFFIX_RESTRUCTURE_COMPLETE.md` - JSON changes
