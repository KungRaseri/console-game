# ContentBuilder Editor Views - Complete Analysis

**Date:** December 17, 2025  
**Total JSON Files:** 79 files  
**Unique Metadata Types:** 26 types

---

## Executive Summary

Based on analysis of all JSON data files, we need **8 core editor views** to cover all data types effectively. Some types can share the same editor with minor variations.

---

## All Metadata Types Found

### Pattern Generation & Catalogs (Core Types)
1. **pattern_generation** (23 files) - Items, enemies, general sensory data
2. **item_catalog** (17 files) - Enemy and item type catalogs
3. **ability_catalog** (13 files) - Enemy special abilities
4. **occupation_catalog** (1 file) - NPC occupations
5. **quest_template_catalog** (1 file) - Quest templates

### NPC-Specific Types
6. **dialogue_style_catalog** (1 file) - Dialogue styles
7. **dialogue_template_catalog** (2 files) - Greetings, farewells
8. **dialogue_quick_templates** (1 file) - Quick dialogue templates
9. **rumor_template_catalog** (1 file) - Rumor templates
10. **name_catalog** (1 file) - First names
11. **surname_catalog** (1 file) - Last names
12. **personality_trait_catalog** (1 file) - Personality traits
13. **quirk_catalog** (1 file) - NPC quirks
14. **background_catalog** (1 file) - NPC backgrounds

### Quest-Specific Types
15. **quest_objectives_primary** (1 file)
16. **quest_objectives_secondary** (1 file)
17. **quest_objectives_hidden** (1 file)
18. **quest_rewards_gold** (1 file)
19. **quest_rewards_experience** (1 file)
20. **quest_rewards_items** (1 file)
21. **quest_locations_dungeons** (1 file)
22. **quest_locations_towns** (1 file)
23. **quest_locations_wilderness** (1 file)

### Specialized Types
24. **material_catalog** (1 file) - Material types
25. **component_library** (2 files) - Adjectives, verbs
26. **pattern_components** (1 file) - Material components
27. **reference_data** (1 file) - Dragon colors
28. **configuration** (1 file) - Rarity config

---

## Required Editor Views (8 Total)

### ✅ Already Implemented (2 Editors)

#### 1. **NamesEditor** (Pattern Generation)
**Status:** ✅ Implemented  
**Handles:** `pattern_generation` (23 files)

**Files Covered:**
- Items: weapons, armor, consumables, enchantments (names.json)
- Enemies: All 13 categories (names.json)
- General: colors, smells, sounds, textures, time_of_day, weather

**Features:**
- Component tree view (prefix, suffix, base, etc.)
- Pattern editor with weights
- Pattern preview
- Trait editing on components

**Structure:**
```json
{
  "metadata": { "type": "pattern_generation" },
  "components": {
    "prefix": [...],
    "suffix": [...],
    "base": [...]
  },
  "patterns": [
    { "pattern": "{prefix} {base}", "weight": 15 }
  ]
}
```

#### 2. **TypesEditor** (Item/Enemy Catalogs)
**Status:** ✅ Implemented  
**Handles:** `item_catalog`, `material_catalog` (18 files)

**Files Covered:**
- Items: weapons, armor, consumables (types.json)
- Materials: types.json
- Enemies: All 13 categories (types.json)

**Features:**
- Category tree view
- Item list with stats
- Dynamic property editor
- Trait display

**Structure:**
```json
{
  "metadata": { "type": "item_catalog" },
  "weapon_types": {
    "swords": {
      "traits": {...},
      "items": [
        { "name": "Sword", "attack": 10, ... }
      ]
    }
  }
}
```

---

### 🔨 Need to Implement (6 New Editors)

#### 3. **AbilitiesEditor** (Special Abilities/Powers)
**Priority:** HIGH  
**Handles:** `ability_catalog` (13 files)

**Files Covered:**
- Enemies: All 13 categories (abilities.json)

**Required Features:**
- List view of abilities
- Name, displayName, description fields
- Rarity selection (Common, Uncommon, Rare, Epic, Legendary)
- Add/Edit/Delete abilities
- Search/filter by name or rarity
- Optional: Effect preview/testing

**Structure:**
```json
{
  "metadata": { "type": "ability_catalog" },
  "items": [
    {
      "name": "PackHunter",
      "displayName": "Pack Hunter",
      "description": "Gains bonuses when fighting alongside allies",
      "rarity": "Common"
    }
  ]
}
```

**UI Mockup:**
```
┌─ Abilities Editor ────────────────────────────────┐
│ [Search: ____]  [Filter: All ▼]  [+ Add Ability] │
├───────────────────────────────────────────────────┤
│ ☰ Pack Hunter          [Common]     [Edit] [Del] │
│ ☰ Feral Rage          [Uncommon]    [Edit] [Del] │
│ ☰ Venomous Bite       [Uncommon]    [Edit] [Del] │
│ ☰ Regeneration        [Rare]        [Edit] [Del] │
├───────────────────────────────────────────────────┤
│ Selected: Pack Hunter                             │
│ Display Name: [Pack Hunter            ]           │
│ Description:  [Gains bonuses when...  ]           │
│ Rarity:       [Common ▼]                          │
│                                    [Save] [Cancel]│
└───────────────────────────────────────────────────┘
```

---

#### 4. **CatalogEditor** (Generic Component Catalogs)
**Priority:** HIGH  
**Handles:** Multiple catalog types (14 files)

**Files Covered:**
- NPCs: occupations.json, personality_traits.json, quirks.json, backgrounds.json, dialogue_styles.json
- NPCs Dialogue: greetings.json, farewells.json, rumors.json, templates.json
- General: adjectives.json, verbs.json
- Items: materials/names.json (pattern_components)
- Enemies: dragons/colors.json (reference_data)

**Required Features:**
- **Component-based structure** (like NamesEditor but simpler)
- Tree view for component categories
- List view for items in category
- Dynamic property editor based on item structure
- Add/Edit/Delete items and categories
- Export/Import functionality

**Supports Multiple Structures:**

**Type A - Component Catalog (Occupations, Dialogue):**
```json
{
  "metadata": { "type": "occupation_catalog" },
  "components": {
    "merchants": [
      { "name": "Blacksmith", "rarityWeight": 20, "traits": {...} }
    ],
    "craftsmen": [...]
  }
}
```

**Type B - Simple Items Catalog (Abilities, Quirks):**
```json
{
  "metadata": { "type": "quirk_catalog" },
  "items": [
    { "name": "Talkative", "description": "...", "rarityWeight": 10 }
  ]
}
```

**Type C - Simple Array (Colors, Reference Data):**
```json
{
  "metadata": { "type": "reference_data" },
  "colors": ["Red", "Blue", "Green", ...]
}
```

**UI Mockup:**
```
┌─ Catalog Editor ──────────────────────────────────┐
│ File: npcs/occupations/occupations.json           │
│ Type: occupation_catalog                          │
├──────────────┬────────────────────────────────────┤
│ Components:  │ Items in "merchants":              │
│ ▼ merchants  │ ☰ Blacksmith    [Edit] [Delete]   │
│   ▼ craftsmen│ ☰ Weaponsmith   [Edit] [Delete]   │
│   adventurers│ ☰ Armorer       [Edit] [Delete]   │
│   magical    │                  [+ Add Item]      │
├──────────────┼────────────────────────────────────┤
│              │ Selected: Blacksmith               │
│              │ Name:         [Blacksmith      ]   │
│              │ Display Name: [Blacksmith      ]   │
│              │ Rarity Weight:[20              ]   │
│              │ Social Class: [craftsman       ]   │
│              │ Traits: [Edit Traits...]           │
│              │                  [Save] [Cancel]   │
└──────────────┴────────────────────────────────────┘
```

---

#### 5. **NameCatalogEditor** (Simple Name Lists)
**Priority:** MEDIUM  
**Handles:** `name_catalog`, `surname_catalog` (2 files)

**Files Covered:**
- NPCs: first_names.json, last_names.json

**Required Features:**
- Component-based lists (male_common, female_noble, etc.)
- Bulk add/edit (comma-separated or multi-line)
- Category management
- Name validation (no duplicates)
- Statistics (total names per category)

**Structure:**
```json
{
  "metadata": { "type": "name_catalog" },
  "components": {
    "male_common": ["Aric", "Cole", "Drake", ...],
    "male_noble": ["Aldric", "Cedric", ...],
    "female_common": ["Aria", "Elara", ...],
    "female_noble": ["Beatrice", "Cordelia", ...]
  }
}
```

**UI Mockup:**
```
┌─ Name Catalog Editor ─────────────────────────────┐
│ File: npcs/names/first_names.json                 │
├──────────────┬────────────────────────────────────┤
│ Categories:  │ Names in "male_common": (45 names) │
│ ▶ male_common│ Aric, Cole, Drake, Heath, Nash,    │
│ ▶ male_noble │ Owen, Quinn, Stone, Wyatt, Zane,   │
│ ▶ female_com.│ Adrian, Blake, Conrad, Everett,    │
│ ▶ female_nob.│ Felix, Marcus, Roland, Sebastian   │
│              │                                     │
│              │ [Edit Names]  [Bulk Add]            │
├──────────────┴────────────────────────────────────┤
│ Bulk Add (comma-separated or one per line):       │
│ [                                              ]   │
│ [                                              ]   │
│                              [Add] [Cancel]        │
└────────────────────────────────────────────────────┘
```

---

#### 6. **QuestTemplateEditor** (Quest Templates)
**Priority:** MEDIUM  
**Handles:** `quest_template_catalog` (1 file)

**Files Covered:**
- Quests: templates/quest_templates.json

**Required Features:**
- Two-level tree: Quest Type → Difficulty
- Template property editor
- Dynamic fields based on quest type
- Preview quest text with placeholders
- Difficulty progression validation

**Structure:**
```json
{
  "metadata": { "type": "quest_template_catalog" },
  "components": {
    "fetch": {
      "easy_fetch": [
        {
          "name": "GatherHerbs",
          "questType": "fetch",
          "difficulty": "easy",
          "minQuantity": 5,
          "baseGoldReward": 30,
          "description": "Collect {quantity} herbs from {location}"
        }
      ]
    },
    "kill": { ... }
  }
}
```

**UI Mockup:**
```
┌─ Quest Template Editor ───────────────────────────┐
│ Quest Types & Difficulties:                       │
├──────────────┬────────────────────────────────────┤
│ ▼ fetch      │ Template: GatherHerbs              │
│   ▶ easy     │ Quest Type:    [fetch ▼]           │
│   ▶ medium   │ Difficulty:    [easy ▼]            │
│   ▶ hard     │ Item Type:     [consumable ▼]      │
│ ▼ kill       │ Min Quantity:  [5              ]   │
│   ▶ easy     │ Max Quantity:  [10             ]   │
│   ▶ medium   │ Base Gold:     [30             ]   │
│   ▶ hard     │ Base XP:       [50             ]   │
│ ▶ escort     │ Location:      [Forest         ]   │
├──────────────┤ Description:                       │
│              │ [Collect {quantity} healing herbs  │
│              │  from {location}                ]  │
│              │                                     │
│              │ Preview: "Collect 5-10 healing     │
│              │          herbs from Forest"        │
│              │                  [Save] [Cancel]   │
└──────────────┴────────────────────────────────────┘
```

---

#### 7. **QuestDataEditor** (Quest Objectives/Rewards/Locations)
**Priority:** LOW  
**Handles:** Quest-specific catalogs (9 files)

**Files Covered:**
- Quests: objectives/primary.json, objectives/secondary.json, objectives/hidden.json
- Quests: rewards/gold.json, rewards/experience.json, rewards/items.json
- Quests: locations/dungeons.json, locations/towns.json, locations/wilderness.json

**Required Features:**
- Simple list editor (similar to AbilitiesEditor)
- Category-aware (primary/secondary/hidden, etc.)
- Dynamic property editor
- Can probably reuse **CatalogEditor** with quest-specific templates

**Structure (varies by file):**
```json
{
  "metadata": { "type": "quest_objectives_primary" },
  "objectives": [
    {
      "name": "DefeatBoss",
      "displayName": "Defeat the Boss",
      "description": "...",
      "difficulty": "hard"
    }
  ]
}
```

**Note:** This could be a variant of **CatalogEditor** with quest-specific styling.

---

#### 8. **ConfigEditor** (Configuration Files)
**Priority:** LOW  
**Handles:** `configuration` (1 file)

**Files Covered:**
- General: rarity_config.json

**Required Features:**
- Key-value pair editor
- Type-aware input (numbers, strings, booleans, objects)
- Validation for numeric ranges
- JSON tree view for nested objects
- Import/Export JSON

**Structure:**
```json
{
  "metadata": { "type": "configuration" },
  "rarity_multipliers": {
    "Common": 1.0,
    "Uncommon": 1.5,
    "Rare": 2.5,
    "Epic": 5.0,
    "Legendary": 10.0
  },
  "weight_calculations": { ... }
}
```

**UI Mockup:**
```
┌─ Configuration Editor ────────────────────────────┐
│ File: general/rarity_config.json                  │
├────────────────────────────────────────────────────┤
│ ▼ rarity_multipliers                              │
│   Common:     [1.0    ] (number)                  │
│   Uncommon:   [1.5    ] (number)                  │
│   Rare:       [2.5    ] (number)                  │
│   Epic:       [5.0    ] (number)                  │
│   Legendary:  [10.0   ] (number)                  │
│                                                    │
│ ▼ weight_calculations                             │
│   base_threshold:     [100  ] (number)            │
│   multiplicative:     [☑]     (boolean)           │
│                                                    │
│ [Add Section] [Add Key] [Import JSON] [Export]    │
└────────────────────────────────────────────────────┘
```

---

## Editor Priority & Implementation Order

### Phase 1: Critical (Cover 90% of files)
1. ✅ **NamesEditor** - Already implemented (23 files)
2. ✅ **TypesEditor** - Already implemented (18 files)
3. 🔨 **AbilitiesEditor** - NEW (13 files) - **HIGH PRIORITY**
4. 🔨 **CatalogEditor** - NEW (14 files) - **HIGH PRIORITY**

**After Phase 1:** 68/79 files covered (86%)

### Phase 2: Nice to Have
5. 🔨 **NameCatalogEditor** - NEW (2 files) - **MEDIUM PRIORITY**
6. 🔨 **QuestTemplateEditor** - NEW (1 file) - **MEDIUM PRIORITY**

**After Phase 2:** 71/79 files covered (90%)

### Phase 3: Specialized (Can wait)
7. 🔨 **QuestDataEditor** - NEW (9 files) - **LOW PRIORITY** (can reuse CatalogEditor)
8. 🔨 **ConfigEditor** - NEW (1 file) - **LOW PRIORITY**

**After Phase 3:** 79/79 files covered (100%)

---

## Editor Type Mapping

| Editor | Metadata Types | File Count | Status |
|--------|---------------|------------|--------|
| **NamesEditor** | pattern_generation | 23 | ✅ Done |
| **TypesEditor** | item_catalog, material_catalog | 18 | ✅ Done |
| **AbilitiesEditor** | ability_catalog | 13 | 🔨 TODO |
| **CatalogEditor** | occupation_catalog, personality_trait_catalog, dialogue_*_catalog, quirk_catalog, background_catalog, component_library, pattern_components, reference_data | 14 | 🔨 TODO |
| **NameCatalogEditor** | name_catalog, surname_catalog | 2 | 🔨 TODO |
| **QuestTemplateEditor** | quest_template_catalog | 1 | 🔨 TODO |
| **QuestDataEditor** | quest_objectives_*, quest_rewards_*, quest_locations_* | 9 | 🔨 TODO |
| **ConfigEditor** | configuration | 1 | 🔨 TODO |

---

## FileTypeDetector Updates Needed

Current `FileTypeDetector.cs` needs to recognize new types:

```csharp
public enum EditorType
{
    Unknown,
    Names,              // ✅ Implemented - pattern_generation
    Types,              // ✅ Implemented - item_catalog, material_catalog
    Abilities,          // 🔨 NEW - ability_catalog
    Catalog,            // 🔨 NEW - Multiple catalog types
    NameCatalog,        // 🔨 NEW - name_catalog, surname_catalog
    QuestTemplate,      // 🔨 NEW - quest_template_catalog
    QuestData,          // 🔨 NEW - quest_*
    Config              // 🔨 NEW - configuration
}

public EditorType DetectEditorType(string filePath)
{
    var content = File.ReadAllText(filePath);
    var metadata = JsonDocument.Parse(content).RootElement
        .GetProperty("metadata");
    
    var type = metadata.GetProperty("type").GetString();
    
    return type switch
    {
        "pattern_generation" => EditorType.Names,
        "item_catalog" or "material_catalog" => EditorType.Types,
        "ability_catalog" => EditorType.Abilities,
        "occupation_catalog" or 
        "personality_trait_catalog" or 
        "dialogue_style_catalog" or 
        "dialogue_template_catalog" or 
        "rumor_template_catalog" or 
        "quirk_catalog" or 
        "background_catalog" or 
        "component_library" or 
        "pattern_components" or 
        "reference_data" => EditorType.Catalog,
        "name_catalog" or "surname_catalog" => EditorType.NameCatalog,
        "quest_template_catalog" => EditorType.QuestTemplate,
        var s when s.StartsWith("quest_") => EditorType.QuestData,
        "configuration" => EditorType.Config,
        _ => EditorType.Unknown
    };
}
```

---

## MainViewModel Updates Needed

Add new editor loading methods:

```csharp
private void LoadAbilitiesEditor(string filePath)
{
    CurrentEditor = new AbilitiesEditorViewModel(filePath);
    CurrentEditorType = EditorType.Abilities;
}

private void LoadCatalogEditor(string filePath)
{
    CurrentEditor = new CatalogEditorViewModel(filePath);
    CurrentEditorType = EditorType.Catalog;
}

private void LoadNameCatalogEditor(string filePath)
{
    CurrentEditor = new NameCatalogEditorViewModel(filePath);
    CurrentEditorType = EditorType.NameCatalog;
}

private void LoadQuestTemplateEditor(string filePath)
{
    CurrentEditor = new QuestTemplateEditorViewModel(filePath);
    CurrentEditorType = EditorType.QuestTemplate;
}

private void LoadQuestDataEditor(string filePath)
{
    CurrentEditor = new QuestDataEditorViewModel(filePath);
    CurrentEditorType = EditorType.QuestData;
}

private void LoadConfigEditor(string filePath)
{
    CurrentEditor = new ConfigEditorViewModel(filePath);
    CurrentEditorType = EditorType.Config;
}
```

Update switch statement:
```csharp
switch (editorType)
{
    case EditorType.Names:
        LoadNamesEditor(filePath);
        break;
    case EditorType.Types:
        LoadTypesEditor(filePath);
        break;
    case EditorType.Abilities:
        LoadAbilitiesEditor(filePath);
        break;
    case EditorType.Catalog:
        LoadCatalogEditor(filePath);
        break;
    case EditorType.NameCatalog:
        LoadNameCatalogEditor(filePath);
        break;
    case EditorType.QuestTemplate:
        LoadQuestTemplateEditor(filePath);
        break;
    case EditorType.QuestData:
        LoadQuestDataEditor(filePath);
        break;
    case EditorType.Config:
        LoadConfigEditor(filePath);
        break;
    default:
        // Show placeholder or JSON viewer
        break;
}
```

---

## Recommended Implementation Order

### Sprint 1: High Priority Editors (80%+ coverage)
1. **AbilitiesEditor** (13 files) - Simple list editor
2. **CatalogEditor** (14 files) - Generic flexible editor

**Effort:** ~2-3 days per editor  
**Result:** 86% of files editable

### Sprint 2: Medium Priority Editors
3. **NameCatalogEditor** (2 files) - Simple name list manager
4. **QuestTemplateEditor** (1 file) - Hierarchical template editor

**Effort:** ~2 days per editor  
**Result:** 90% of files editable

### Sprint 3: Low Priority Editors (Polish)
5. **QuestDataEditor** (9 files) - Can reuse CatalogEditor with templates
6. **ConfigEditor** (1 file) - Key-value editor

**Effort:** ~1-2 days per editor  
**Result:** 100% of files editable

---

## Shared Components to Build

### Reusable UI Components
1. **DynamicPropertyEditor** - Edit any JSON property based on type
2. **TraitEditor** - Edit trait objects (used in multiple editors)
3. **ComponentTreeView** - Display component hierarchies
4. **ItemListView** - Draggable, searchable, filterable lists
5. **ValidationPanel** - Show validation errors/warnings
6. **JsonImportExport** - Import/export raw JSON

These can be shared across all editors to reduce code duplication.

---

## Testing Strategy

### Per Editor
1. **Load Test** - Open all files of that type
2. **Edit Test** - Modify, save, reload
3. **Add Test** - Add new items/categories
4. **Delete Test** - Remove items safely
5. **Validation Test** - Invalid data handling
6. **Export/Import Test** - JSON round-trip

### Integration
1. **Multi-File Test** - Edit multiple files in sequence
2. **Undo/Redo Test** - Change history
3. **Search Test** - Find across all data
4. **Performance Test** - Large files (1000+ items)

---

## Conclusion

**8 editors needed to fully support all 79 JSON files.**

**Current Status:**
- ✅ 2 editors implemented (41 files covered - 52%)
- 🔨 6 editors to implement (38 files remaining - 48%)

**Recommended Next Steps:**
1. Implement **AbilitiesEditor** (simple, high value)
2. Implement **CatalogEditor** (flexible, covers many types)
3. Test with real data
4. Iterate based on user feedback
5. Build remaining editors as needed

**Priority Focus:** Phase 1 gets us to 86% coverage with just 2 new editors!

---

**Created:** December 17, 2025  
**Analysis:** Complete survey of all JSON data types  
**Status:** Ready for implementation planning
