# ContentBuilder Quick Reference

## All Editors (4 Total)

### 1. NameListEditor
**Files**: names.json (38 files)  
**Purpose**: Pattern-based name generation  
**Features**: Pattern editor, component tokens, rarity weights, preview

### 2. CatalogEditor
**Files**: catalog.json (61 files)  
**Purpose**: Item/enemy/ability catalogs  
**Features**: Type management, categories, items, traits, references

### 3. ComponentDataEditor ⭐ NEW
**Files**: colors.json, traits.json, objectives.json, rewards.json, etc. (20 files)  
**Purpose**: Component/data file editing  
**Features**: Key-value editing, flexible structure, metadata support, JSON preview

### 4. ConfigEditor ⭐ NEW
**Files**: .cbconfig.json (65 files)  
**Purpose**: Folder configuration  
**Features**: Display name, icon, sort order, description, icon preview

---

## Quick Actions

### Edit Component File
```
1. Navigate to file (e.g., enemies/dragons/colors.json)
2. Click file name
3. Edit items in DataGrid
4. Click "Add Item" to create new
5. Select and click "Delete" to remove
6. Click "Save" when done
```

### Edit Config File
```
1. Navigate to folder
2. Click .cbconfig.json
3. Edit Display Name, Icon, Sort Order
4. Preview icon live
5. Click "Save"
6. Refresh tree to see changes
```

---

## Common Material Design Icons

**Abilities**: AutoFix, Fire, Water, Lightning, Shield, Sword  
**Enemies**: Dragon, Ghost, Skull, Bug, Monster, Spider  
**Items**: Package, Treasure, Diamond, Coin, Gem  
**Characters**: Account, AccountGroup, Face, HumanGreeting  
**World**: Earth, Map, Castle, Tree, Mountain, Forest  
**Organizations**: Domain, Bank, Store, Factory, Building  
**Social**: EmoticonHappy, Message, Heart, Comment  
**Quests**: BookOpen, ClipboardText, Target, Flag  

**Find More**: https://pictogrammers.com/library/mdi/

---

## File Coverage

| Type | Count | Editor |
|------|-------|--------|
| catalog.json | 61 | CatalogEditor |
| names.json | 38 | NameListEditor |
| Component files | 20 | ComponentDataEditor ⭐ |
| .cbconfig.json | 65 | ConfigEditor ⭐ |
| **Total** | **184** | **100%** |

---

## Component Files List

### Abilities (1)
- abilities/keywords.json

### Enemies (6)
- enemies/dragons/colors.json
- enemies/dragons/traits.json
- enemies/undead/traits.json
- enemies/beasts/traits.json
- enemies/humanoid/traits.json
- enemies/elementals/traits.json

### NPCs (2)
- npcs/traits.json
- npcs/schedules.json

### Quests (3)
- quests/objectives.json
- quests/rewards.json
- quests/difficulties.json

### Items (3)
- items/weapons/traits.json
- items/armor/traits.json
- items/consumables/effects.json

### World (2)
- world/environments/weather.json
- world/regions/traits.json

### Organizations (1)
- organizations/ranks.json

### Social (1)
- social/emotions.json

### General (1)
- general/rarity_config.json

---

## Keyboard Shortcuts

- **Ctrl+S**: Save current editor
- **F5**: Refresh tree
- **Ctrl+N**: New file (coming soon)
- **Delete**: Delete selected item (in DataGrid)

---

## Tips

✅ **Auto-Backup**: Every save creates timestamped backup in `backups/` folder  
✅ **Change Tracking**: Save button only enabled when file is dirty  
✅ **JSON Preview**: Live preview at bottom shows exact JSON structure  
✅ **Icon Preview**: ConfigEditor shows live icon preview as you type  
✅ **Validation**: Editors validate data before saving  
✅ **Refresh**: Discard unsaved changes with Refresh button  

---

## Status: ✅ COMPLETE

**All 184 JSON files now have full CRUD support!**
