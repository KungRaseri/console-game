# XAML Files Comprehensive Audit Report
**Date**: December 26, 2025  
**Purpose**: Identify obsolete views, missing AutomationIDs, and guide cleanup

---

## Part 1: XAML Files Status

### ✅ **ACTIVE VIEWS** (3 files - Currently Used in Switch Statement)

1. **NameListEditorView.xaml** ✅
   - EditorType: `NameListEditor`
   - ViewModel: `NameListEditorViewModel`
   - Loaded by: `LoadNameListEditor()` (line 127)
   - **Status**: ACTIVE - Used in production

2. **CatalogEditorView.xaml** ✅
   - EditorType: `CatalogEditor`
   - ViewModel: `CatalogEditorViewModel`
   - Loaded by: `LoadCatalogEditor()` (line 193)
   - **Status**: ACTIVE - Used in production

3. **AbilitiesEditorView.xaml** ✅
   - EditorType: `AbilitiesEditor`
   - ViewModel: `AbilitiesEditorViewModel`
   - Loaded by: `LoadAbilitiesEditor()` (line 227)
   - **Status**: ACTIVE - Used in production

---

### ⚠️ **ORPHANED VIEWS** (8 files - Have ViewModels + Methods BUT NOT in Switch)

These have fully implemented ViewModels and loader methods, but are **NEVER CALLED** because they're not in the switch statement:

4. **ItemEditorView.xaml** ⚠️
   - ViewModel: `ItemEditorViewModel` (exists)
   - Method: `LoadItemEditor()` (line 105) - **DEAD CODE**
   - Problem: No EditorType in switch statement
   - **Recommendation**: DELETE (unless we add EditorType to switch)

5. **HybridArrayEditorView.xaml** ⚠️
   - ViewModel: `HybridArrayEditorViewModel` (exists)
   - Method: `LoadHybridArrayEditor()` (line 149) - **DEAD CODE**
   - Problem: No EditorType in switch statement
   - **Recommendation**: DELETE

6. **NamesEditorView.xaml** ⚠️
   - ViewModel: `NamesEditorViewModel` (exists)
   - Method: `LoadNamesEditor()` (line 171) - **DEAD CODE**
   - Problem: No EditorType in switch statement
   - **Recommendation**: DELETE

7. **GenericCatalogEditorView.xaml** ⚠️
   - ViewModel: `GenericCatalogEditorViewModel` (exists)
   - Method: `LoadGenericCatalogEditor()` (line 246) - **DEAD CODE**
   - Problem: No EditorType in switch statement
   - **Recommendation**: DELETE OR add to switch

8. **QuestCatalogEditorView.xaml** ⚠️
   - ViewModel: `QuestCatalogEditorViewModel` (exists)
   - Method: `LoadQuestCatalogEditor()` (line 271) - **DEAD CODE**
   - Problem: No EditorType in switch statement
   - **Recommendation**: DELETE OR add to switch (v4.0 quest system?)

9. **QuestDataEditorView.xaml** ⚠️
   - ViewModel: `QuestDataEditorViewModel` (exists)
   - Method: `LoadQuestDataEditor()` (line 297) - **DEAD CODE**
   - Problem: No EditorType in switch statement
   - **Recommendation**: DELETE OR add to switch

10. **NameCatalogEditorView.xaml** ⚠️
    - ViewModel: `NameCatalogEditorViewModel` (exists)
    - Method: `LoadNameCatalogEditor()` (line 374) - **DEAD CODE**
    - Problem: No EditorType in switch statement
    - **Recommendation**: DELETE

11. **FlatItemEditorView.xaml** ⚠️
    - ViewModel: `FlatItemEditorViewModel` (exists - from docs)
    - Method: Likely exists but not in switch
    - Problem: No EditorType in switch statement
    - **Recommendation**: DELETE

---

### ✅ **UTILITY VIEWS** (3 files - Used for Dialogs/Windows)

12. **MainWindow.xaml** ✅
    - Main application window
    - **Status**: ACTIVE - Core UI

13. **ReferenceSelectorDialog.xaml** ✅
    - Dialog for selecting references
    - **Status**: ACTIVE - Referenced by editors

14. **PreviewWindow.xaml** ✅
    - Preview dialog
    - Opened by: `ShowPreview()` command (line 323)
    - **Status**: ACTIVE

15. **App.xaml** ✅
    - Application resources
    - **Status**: ACTIVE - Required

---

## Part 2: Decision Matrix

### Option A: AGGRESSIVE CLEANUP (Recommended)
**Delete ALL orphaned views** - Keep only what's in the switch statement

**Files to DELETE** (8 total):
- ❌ `ItemEditorView.xaml` + `.xaml.cs`
- ❌ `HybridArrayEditorView.xaml` + `.xaml.cs`
- ❌ `NamesEditorView.xaml` + `.xaml.cs`
- ❌ `GenericCatalogEditorView.xaml` + `.xaml.cs`
- ❌ `QuestCatalogEditorView.xaml` + `.xaml.cs`
- ❌ `QuestDataEditorView.xaml` + `.xaml.cs`
- ❌ `NameCatalogEditorView.xaml` + `.xaml.cs`
- ❌ `FlatItemEditorView.xaml` + `.xaml.cs`

**Also DELETE corresponding**:
- ViewModels (8 files)
- ViewModel Tests (8 files)
- Loader methods in MainViewModel.cs (8 methods)

**Total Cleanup**: ~50 files removed

**Pros**:
- Clean codebase
- No dead code
- Easier to maintain
- Tests only cover active code

**Cons**:
- Lose work if we want those editors later
- Need to re-implement if needed

---

### Option B: SELECTIVE KEEP (Conservative)
Keep quest editors (v4.0 feature?) and generic catalog editor (reusable?)

**Files to KEEP & FIX**:
- ✅ `GenericCatalogEditorView.xaml` - Add to switch
- ✅ `QuestCatalogEditorView.xaml` - Add to switch
- ✅ `QuestDataEditorView.xaml` - Add to switch

**Files to DELETE**:
- ❌ `ItemEditorView.xaml`
- ❌ `HybridArrayEditorView.xaml`
- ❌ `NamesEditorView.xaml`
- ❌ `NameCatalogEditorView.xaml`
- ❌ `FlatItemEditorView.xaml`

**Total Cleanup**: ~30 files removed

**Pros**:
- Keep potentially useful editors
- Quest v4.0 ready

**Cons**:
- More maintenance
- Still have unused code paths

---

## Part 3: Missing AutomationIDs Audit

### MainWindow.xaml
**Missing IDs**:
- TreeView (categories tree)
- StatusBar
- Main content area
- Refresh button
- Exit button

### NameListEditorView.xaml
**Missing IDs** (from test failures):
- ✅ PatternsList - **ALREADY ADDED**
- ✅ PatternAddButton - **ALREADY ADDED**
- ✅ PatternDeleteButton - **ALREADY ADDED**
- ❌ PatternTemplateTextBox
- ❌ PatternDescriptionTextBox
- ❌ TokenBadge
- ❌ ComponentEditorPanel
- ❌ ComponentValuesList
- ❌ AddValueButton
- ❌ ComponentValueDeleteButton
- ❌ ExamplesPanel
- ❌ Save button
- ❌ All component token insertion buttons

### CatalogEditorView.xaml
**Missing IDs** (from test failures):
- ❌ CategoryListBox
- ❌ ItemsListView (might be ItemsControlInner or CustomFieldsItemsControl)
- ❌ AddCategoryButton
- ❌ DeleteCategoryButton
- ❌ AddItemButton
- ❌ DeleteItemButton
- ❌ ItemTraitsPanel
- ❌ AddTraitButton
- ❌ DeleteTraitButton
- ❌ CustomFieldsPanel
- ❌ AddCustomFieldButton
- ❌ DeleteCustomFieldButton
- ❌ MetadataPanel
- ❌ ItemNameTextBox
- ❌ ItemDescriptionTextBox
- ❌ RarityComboBox
- ❌ WeightTextBox
- ❌ CatalogVersionTextBox
- ❌ CatalogDescriptionTextBox

### AbilitiesEditorView.xaml
**Status**: Unknown - needs manual audit

### Utility Views
- ReferenceSelectorDialog.xaml - needs IDs for browsing
- PreviewWindow.xaml - probably fine (read-only view)

---

## Part 4: Obsolete Tests to Remove/Mark

### Tests for ORPHANED views (to DELETE or SKIP):
1. `ItemEditor*Tests.cs` - Delete
2. `HybridArrayEditor*Tests.cs` - Delete
3. `NamesEditor*Tests.cs` - Delete
4. `GenericCatalogEditor*Tests.cs` - Delete or keep if keeping view
5. `QuestCatalogEditor*Tests.cs` - Delete or keep
6. `QuestDataEditor*Tests.cs` - Delete or keep
7. `NameCatalogEditor*Tests.cs` - Delete
8. `FlatItemEditor*Tests.cs` - Delete

### UI Tests that test data changes (Need Refactoring):
**Already Fixed** ✅:
- `Should_Add_New_Pattern_When_Add_Button_Clicked` - Now tests button state
- `Should_Delete_Pattern_When_Delete_Button_Clicked` - Now tests button state
- All button-click tests in NameListEditor and CatalogEditor

**Still Need Review**:
- Any remaining tests asserting `.Count` changes
- Any tests asserting text changes in collections
- Any tests verifying save side effects

---

## Part 5: Recommended Action Plan

### Phase 1: Decide on Cleanup Strategy ⏳
**Decision Needed**: Option A (aggressive) or Option B (selective)?

### Phase 2: Delete Obsolete Files 🗑️
If Option A:
- Delete 8 XAML view files + code-behind
- Delete 8 ViewModel files  
- Delete 8 ViewModel test files
- Delete 8 loader methods from MainViewModel
- Update EditorType enum (remove unused values)

### Phase 3: Add AutomationIDs ⚙️
Priority order:
1. NameListEditorView.xaml (most tests)
2. CatalogEditorView.xaml (most tests)
3. AbilitiesEditorView.xaml
4. MainWindow.xaml
5. ReferenceSelectorDialog.xaml

### Phase 4: Update/Remove Tests 🧪
1. Delete tests for deleted views
2. Mark remaining data-change tests as refactored
3. Verify all AutomationIDs work

### Phase 5: Create Unit Tests 📝
Focus on ViewModels for active views:
1. NameListEditorViewModel unit tests
2. CatalogEditorViewModel unit tests
3. AbilitiesEditorViewModel unit tests

---

## Summary Statistics

### Current State:
- **Total XAML files**: 15
- **Active in switch**: 3 (20%)
- **Orphaned (dead code)**: 8 (53%)
- **Utility views**: 4 (27%)

### After Option A Cleanup:
- **Total XAML files**: 7
- **Active views**: 3
- **Utility views**: 4
- **Dead code**: 0

### Test Pass Rate:
- **Current**: 21/55 (38%)
- **After AutomationIDs**: ~40/55 (73% estimated)
- **After test cleanup**: 40/45 (89% estimated, remove 10 obsolete tests)

---

**Next Steps**: 
1. User decides: Option A or Option B?
2. Execute cleanup plan
3. Add AutomationIDs systematically
4. Remove/update tests
5. Create unit tests for business logic

