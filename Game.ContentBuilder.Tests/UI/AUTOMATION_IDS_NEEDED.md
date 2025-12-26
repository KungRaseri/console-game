# AutomationIds Required for UI Tests

## Status
Comprehensive UI tests for NameListEditor and CatalogEditor have been created but are currently **skipped** (`.skip` extension) because the XAML views don't have AutomationId attributes yet.

**Test Files:**
- `NameListEditor_ComprehensiveTests.cs.skip` - 27 tests
- `CatalogEditor_ComprehensiveTests.cs.skip` - 32 tests

**Current Pass Rate:** 11 passing, 39 failing (22% pass rate)

## To Re-Enable Tests
1. Add the AutomationIds listed below to the respective XAML views
2. Rename test files by removing `.skip` extension
3. Run tests: `dotnet test --filter "Editor=NameList|Editor=Catalog"`

---

## NameListEditor - Required AutomationIds

**File:** `Game.ContentBuilder\Views\NameListEditorView.xaml`

### Pattern Management
- `PatternCard` - Each pattern item in the list (ItemsControl item container)
- `PatternAddButton` - Add new pattern button
- `PatternDeleteButton` - Delete pattern button (on each pattern card)

### Pattern Details
- `TemplateTextBox` - Pattern template input field
- `DescriptionTextBox` - Pattern description field

### Component Tokens
- `ComponentInsertButton` - Component insertion buttons (e.g., "{prefix}", "{suffix}")
  - Use naming like: `ComponentInsertButton_Prefix`, `ComponentInsertButton_Suffix`, etc.

### Token Badges
- `TokenBadge` - Visual badges showing components in pattern
  - These should be clickable to open component editor

### Component Editor (Dialog/Popup)
- `ComponentEditor` - The component editor dialog/panel
- `ComponentValueList` - List of values in component editor
- `ComponentValueAddButton` - Add new value button
- `ComponentValueDeleteButton` - Delete value button

### References
- `ReferenceInsertButton` - Reference insertion buttons (materials, weapons, etc.)
  - Use naming like: `RefButton_Materials`, `RefButton_Weapons`, etc.
- `BrowseReferencesButton` - Browse references dialog button
- `BrowseReferencesDialog` - The browse dialog itself

### Examples
- `ExamplesSection` / `ExamplesPanel` - Examples display area
- Generated examples should be findable within this panel

### Status & Metadata
- `StatusBar` - Status bar showing save status

---

## CatalogEditor - Required AutomationIds

**File:** Multiple catalog editor views (GenericCatalogEditorView.xaml or specific editor views)

### Category Management
- `CategoryList` - Category list control
- `CategoryItem` - Each category in the list
- `CategoryAddButton` - Add new category button
- `CategoryDeleteButton` - Delete category button

### Item Management
- `ItemList` - Item list (appears when category selected)
- `ItemCard` / `ItemListItem` - Each item in the list
- `ItemAddButton` - Add new item button
- `ItemDeleteButton` - Delete item button

### Item Details Panel
- `ItemDetailsPanel` - The details panel that appears when item is selected
- `NameTextBox` - Item name field
- `DescriptionTextBox` - Item description field
- `RarityComboBox` - Rarity dropdown
- `RarityWeightTextBox` / `WeightTextBox` - Rarity weight field

### Traits Section
- `TraitsPanel` - Traits section container
- `TraitsList` - List of traits
- `TraitAddButton` - Add trait button
- `TraitDeleteButton` - Delete trait button

### Custom Fields Section
- `CustomFieldsPanel` - Custom fields section container
- `CustomFieldsList` - List of custom fields
- `CustomFieldAddButton` - Add custom field button
- `CustomFieldDeleteButton` - Delete custom field button

### Metadata Section
- `MetadataPanel` - Metadata section container
- `CatalogDescriptionTextBox` - Catalog-level description
- `CatalogVersionTextBox` / `VersionTextBox` - Catalog version field

### Status & Save
- `StatusBar` - Status bar for save confirmations
- Should respond to Ctrl+S keyboard shortcut

---

## Test Writing Guidelines (Going Forward)

When writing new UI tests:

### ✅ Preferred Approach: Use Multiple Selectors
```csharp
// Try AutomationId first, fall back to Name or ControlType
var button = window.FindFirstDescendant(cf => 
    cf.ByAutomationId("AddButton")
    .Or(cf.ByName("Add"))
    .Or(cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button)
         .And(cf.ByName("Add"))));
```

### ✅ Use Hierarchical Searches
```csharp
// Find by path: Panel → List → Item
var panel = window.FindFirstDescendant(cf => cf.ByAutomationId("ItemsPanel"));
var list = panel?.FindFirstDescendant(cf => cf.ByControlType(ControlType.List));
var items = list?.FindAllChildren();
```

### ✅ Use Control Properties
```csharp
// Search by text content, tooltip, or class name
var button = window.FindFirstDescendant(cf => 
    cf.ByText("Add Item")
    .Or(cf.ByHelpText("Add a new item")));
```

### ❌ Avoid: Hard-coding AutomationIds Only
```csharp
// Don't do this - too brittle
var button = window.FindFirstDescendant(cf => cf.ByAutomationId("AddButton"));
if (button == null) throw new Exception("Button not found!");
```

### Test Structure Best Practices

1. **Use the shared UITestCollectionFixture**
   - Dramatically faster (4 min vs 45+ min)
   - All tests share one app instance

2. **Add meaningful error messages**
   ```csharp
   element.Should().NotBeNull("The add button should exist in the toolbar");
   ```

3. **Use Traits for filtering**
   ```csharp
   [Trait("Category", "UI")]
   [Trait("Editor", "NameList")]
   [Trait("Feature", "Patterns")]
   ```

4. **Add stabilization waits after actions**
   ```csharp
   addButton.Click();
   Thread.Sleep(500); // Wait for UI to update
   ```

---

## Implementation Plan

### Phase 1: NameListEditor AutomationIds
1. Add AutomationIds to pattern list items and buttons
2. Add AutomationIds to template and description fields
3. Add AutomationIds to component insertion UI
4. Add AutomationIds to examples section
5. Re-enable NameListEditor_ComprehensiveTests

### Phase 2: CatalogEditor AutomationIds
1. Add AutomationIds to category list and buttons
2. Add AutomationIds to item list and buttons
3. Add AutomationIds to item details fields
4. Add AutomationIds to traits and custom fields sections
5. Add AutomationIds to metadata section
6. Re-enable CatalogEditor_ComprehensiveTests

### Phase 3: New Editor Tests
1. Write tests for other editors (Abilities, Quests, etc.)
2. Use flexible selector approach from the start
3. Add AutomationIds to XAML as you build the UI

---

## Quick Reference: Adding AutomationId in XAML

```xaml
<!-- Button Example -->
<Button Content="Add Pattern"
        Command="{Binding AddPatternCommand}"
        AutomationProperties.AutomationId="PatternAddButton"/>

<!-- TextBox Example -->
<TextBox Text="{Binding Name}"
         AutomationProperties.AutomationId="NameTextBox"/>

<!-- ItemsControl Item Container -->
<ItemsControl ItemsSource="{Binding Patterns}">
    <ItemsControl.ItemContainerStyle>
        <Style TargetType="ContentPresenter">
            <Setter Property="AutomationProperties.AutomationId" Value="PatternCard"/>
        </Style>
    </ItemsControl.ItemContainerStyle>
</ItemsControl>

<!-- ListBox Item -->
<ListBox ItemsSource="{Binding Categories}"
         AutomationProperties.AutomationId="CategoryList">
    <ListBox.ItemContainerStyle>
        <Style TargetType="ListBoxItem">
            <Setter Property="AutomationProperties.AutomationId" Value="CategoryItem"/>
        </Style>
    </ListBox.ItemContainerStyle>
</ListBox>
```

---

## Current Status Summary

- **Fixture:** ✅ Working perfectly (shared app instance)
- **Test Coverage:** 59 comprehensive tests written
- **Blocked By:** Missing AutomationId attributes in XAML
- **Next Step:** Add AutomationIds to NameListEditorView.xaml first

**Estimated Time:** 2-3 hours per editor to add all AutomationIds and verify tests pass.
