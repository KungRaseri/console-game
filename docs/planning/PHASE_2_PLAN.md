# Phase 2: NameCatalogEditor & QuestTemplateEditor

**Date:** December 17, 2025  
**Status:** 🚀 READY TO START  
**Previous Phase:** Phase 1 Complete (86% coverage, 3 editors, API standardized)

---

## Overview

Phase 2 adds **2 new editors** to bring total coverage to **90% of JSON files** (71/79 files).

### Coverage Goals
- **Current:** 44 files (55.7% by count, 86% by priority)
- **After Phase 2:** 47 files (59.5% by count, 90% by priority)
- **Files Added:** 3 files (2 name catalogs + 1 quest template)

---

## Editors to Implement

### 1. NameCatalogEditor (Priority: MEDIUM)

**Purpose:** Bulk editing of NPC name catalogs (first names, last names)

**Files Covered (2):**
- `npcs/names/first_names.json`
- `npcs/names/last_names.json`

**File Structure:**
```json
{
  "metadata": {
    "version": "4.0",
    "type": "name_catalog",
    "description": "First names for NPCs",
    "usage": "NPC generation system"
  },
  "categories": {
    "male_common": ["John", "Michael", "William"],
    "male_noble": ["Reginald", "Percival", "Edmund"],
    "female_common": ["Mary", "Elizabeth", "Sarah"],
    "female_noble": ["Arabella", "Genevieve", "Cordelia"],
    "unisex": ["Alex", "Jordan", "Morgan"]
  }
}
```

**Features:**
- **Category Management:**
  - Add/remove/rename categories
  - Category tree view (male_common, male_noble, etc.)
  - Category-level statistics (count per category)

- **Name Management:**
  - Bulk add names (multi-line text input or comma-separated)
  - Individual add/edit/delete
  - Search and filter within categories
  - Duplicate detection and validation
  - Alphabetical sorting option

- **Validation:**
  - No empty names
  - No duplicate names within a category
  - Name length limits (2-30 characters)
  - Character restrictions (letters, hyphens, apostrophes only)

- **Statistics:**
  - Total names count
  - Names per category
  - Coverage indicators (minimum names per category warning)

**UI Layout:**
```
┌─────────────────────────────────────────────────────┐
│ NameCatalogEditor: first_names.json                │
├─────────────────┬───────────────────────────────────┤
│ Categories      │ Names in [Selected Category]      │
│                 │                                   │
│ [Search...]     │ [Bulk Add] [Add] [Delete]        │
│                 │                                   │
│ ▼ Male          │ [Search...]                      │
│   • common (45) │                                   │
│   • noble (12)  │ ┌───────────────────────────────┐│
│ ▼ Female        │ │ □ Aaron                       ││
│   • common (52) │ │ □ Benjamin                    ││
│   • noble (15)  │ │ □ Charles                     ││
│ • Unisex (30)   │ │ □ Daniel                      ││
│                 │ │ ...                           ││
│ Total: 154      │ └───────────────────────────────┘│
│                 │                                   │
│                 │ Selected: 0 names                 │
└─────────────────┴───────────────────────────────────┘
```

**ViewModel:**
- `NameCatalogEditorViewModel.cs`
- Follows standardized API: `constructor(JsonEditorService, string fileName)`
- ObservableCollections: Categories, Names, SelectedCategory
- Commands: AddCategory, RemoveCategory, BulkAddNames, AddName, DeleteName
- Properties: TotalNameCount, CategoryCounts, ValidationErrors

**View:**
- `NameCatalogEditorView.xaml`
- Two-column layout (categories + names)
- Material Design components
- Bulk add dialog (multi-line TextBox)
- Validation feedback

---

### 2. QuestTemplateEditor (Priority: MEDIUM)

**Purpose:** Edit quest templates with dynamic fields based on quest type

**Files Covered (1):**
- `quests/templates/quest_templates.json`

**File Structure:**
```json
{
  "metadata": {
    "version": "4.0",
    "type": "quest_template_catalog",
    "description": "Quest templates by type and difficulty"
  },
  "templates": {
    "hunt": {
      "easy": [
        {
          "title": "Rat Extermination",
          "description": "Clear {count} rats from {location}",
          "objectives": ["kill_{enemy}_count"],
          "rewards": { "gold": 50, "xp": 100 }
        }
      ],
      "medium": [...],
      "hard": [...],
      "epic": [...]
    },
    "fetch": { ... },
    "escort": { ... },
    "explore": { ... }
  }
}
```

**Features:**
- **Two-Level Tree Navigation:**
  - Level 1: Quest Type (hunt, fetch, escort, explore)
  - Level 2: Difficulty (easy, medium, hard, epic)
  - Template list view for selected type+difficulty

- **Template Editing:**
  - Title, description (with placeholder support)
  - Objectives array (add/remove/edit)
  - Rewards object (gold, xp, items)
  - Dynamic fields based on quest type

- **Placeholder System:**
  - Preview with sample values
  - Syntax highlighting for {placeholders}
  - Validation of placeholder references

- **Difficulty Progression:**
  - Visual indicator (rewards scaling)
  - Difficulty comparison view
  - Template cloning across difficulties

**UI Layout:**
```
┌──────────────────────────────────────────────────────────┐
│ QuestTemplateEditor: quest_templates.json               │
├────────────────┬─────────────────────────────────────────┤
│ Quest Types    │ Template Editor                         │
│                │                                         │
│ ▼ Hunt         │ Title: [Rat Extermination_________]    │
│   • Easy (5)   │                                         │
│   • Medium (4) │ Description:                            │
│   • Hard (3)   │ ┌───────────────────────────────────┐  │
│   • Epic (2)   │ │Clear {count} rats from {location} │  │
│ ▼ Fetch        │ └───────────────────────────────────┘  │
│   • Easy (6)   │                                         │
│   • Medium (5) │ Objectives:                             │
│   ...          │ • kill_rat_count                       │
│                │ • return_to_quest_giver                │
│ Total: 45      │ [Add] [Remove]                         │
│                │                                         │
│                │ Rewards:                                │
│                │ Gold: [50___] XP: [100___]             │
│                │                                         │
│                │ [Preview] [Save] [Clone to...]         │
└────────────────┴─────────────────────────────────────────┘
```

**ViewModel:**
- `QuestTemplateEditorViewModel.cs`
- Standardized API constructor
- ObservableCollections: QuestTypes, Difficulties, Templates
- Properties: SelectedType, SelectedDifficulty, SelectedTemplate
- Commands: AddTemplate, DeleteTemplate, CloneTemplate, PreviewWithSampleData

**View:**
- `QuestTemplateEditorView.xaml`
- Two-level TreeView + template editor panel
- Placeholder syntax highlighting
- Preview panel with sample data

---

## Implementation Order

### Step 1: NameCatalogEditor (Simpler, good warmup)
1. Create ViewModel (150-200 lines)
2. Create View XAML (200-250 lines)
3. Create View code-behind (50 lines)
4. Add to MainViewModel loader
5. Create ViewModel tests (smoke + comprehensive)
6. Create UI tests
7. Test with actual data files

### Step 2: QuestTemplateEditor (More complex)
1. Create ViewModel (250-300 lines)
2. Create View XAML (300-350 lines)
3. Create View code-behind (50 lines)
4. Add to MainViewModel loader
5. Create ViewModel tests
6. Create UI tests
7. Test with actual data

---

## Technical Specifications

### NameCatalogEditor Models

```csharp
public class NameCategory
{
    public string Name { get; set; }
    public ObservableCollection<string> Names { get; set; }
    public int Count => Names.Count;
}
```

### QuestTemplateEditor Models

```csharp
public class QuestTemplate
{
    public string Title { get; set; }
    public string Description { get; set; }
    public ObservableCollection<string> Objectives { get; set; }
    public QuestRewards Rewards { get; set; }
}

public class QuestRewards
{
    public int Gold { get; set; }
    public int Experience { get; set; }
    public ObservableCollection<string> Items { get; set; }
}
```

---

## Validation Requirements

### NameCatalogEditor
- **Name Validation:**
  - Length: 2-30 characters
  - Pattern: `^[a-zA-Z'-]+$` (letters, hyphens, apostrophes)
  - No duplicates within category
  - No empty names

- **Category Validation:**
  - Name required
  - No duplicate category names
  - Minimum 1 name per category

### QuestTemplateEditor
- **Template Validation:**
  - Title required (1-100 chars)
  - Description required (1-500 chars)
  - At least 1 objective
  - Rewards: gold >= 0, xp >= 0
  - Placeholder references match available placeholders

---

## Testing Strategy

### ViewModel Tests
- **NameCatalogEditor (10-12 tests):**
  - Load categories from JSON
  - Add/remove categories
  - Bulk add names
  - Delete names
  - Duplicate detection
  - Validation errors
  - Save to JSON

- **QuestTemplateEditor (12-15 tests):**
  - Load templates by type/difficulty
  - Add/edit/delete templates
  - Clone template to different difficulty
  - Placeholder validation
  - Reward scaling
  - Save to JSON

### UI Tests
- **NameCatalogEditor (4-6 tests):**
  - Navigate to name catalog
  - Select category
  - Add name
  - Bulk add dialog
  - Delete name
  - Save button visible

- **QuestTemplateEditor (4-6 tests):**
  - Navigate to quest templates
  - Expand quest type
  - Select difficulty
  - Edit template
  - Preview with sample data
  - Save button visible

---

## File Structure

```
RealmForge/
├── ViewModels/
│   ├── NameCatalogEditorViewModel.cs (NEW)
│   └── QuestTemplateEditorViewModel.cs (NEW)
├── Views/
│   ├── NameCatalogEditorView.xaml (NEW)
│   ├── NameCatalogEditorView.xaml.cs (NEW)
│   ├── QuestTemplateEditorView.xaml (NEW)
│   └── QuestTemplateEditorView.xaml.cs (NEW)
└── Models/
    ├── NameCategory.cs (NEW)
    ├── QuestTemplate.cs (NEW)
    └── QuestRewards.cs (NEW)

RealmForge.Tests/
├── ViewModels/
│   ├── NameCatalogEditorViewModelTests.cs (NEW)
│   └── QuestTemplateEditorViewModelTests.cs (NEW)
└── UI/
    ├── NameCatalogEditorUITests.cs (NEW)
    └── QuestTemplateEditorUITests.cs (NEW)
```

---

## Estimated Effort

### NameCatalogEditor
- **ViewModel:** 1-2 hours
- **View:** 1-2 hours
- **Tests:** 1 hour
- **Total:** 3-5 hours

### QuestTemplateEditor
- **ViewModel:** 2-3 hours
- **View:** 2-3 hours
- **Tests:** 1-2 hours
- **Total:** 5-8 hours

**Phase 2 Total:** 8-13 hours

---

## Success Criteria

✅ Both editors follow standardized API pattern  
✅ All tests pass (ViewModel + UI)  
✅ Build succeeds without warnings  
✅ Can load, edit, and save actual JSON files  
✅ UI disposal works correctly (no hanging processes)  
✅ Coverage reaches 90% (47/79 files)  
✅ Documentation updated  

---

## Next Actions

1. **Start with NameCatalogEditor** (simpler, good warmup)
2. Create ViewModel with standardized constructor
3. Create View with two-column layout
4. Add to MainViewModel
5. Create tests
6. Verify with actual data
7. Move to QuestTemplateEditor
8. Update documentation

---

## Related Files

- **Planning:** `docs/planning/CONTENTBUILDER_EDITOR_VIEWS_ANALYSIS.md`
- **Progress:** `docs/implementation/CONTENTBUILDER_EDITORS_PROGRESS.md`
- **API Guide:** `docs/implementation/API_STANDARDIZATION_COMPLETE.md`
- **Test Status:** `docs/testing/AUTOMATED_UI_TESTS_STATUS.md`
