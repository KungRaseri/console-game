# ContentBuilder UI Testing Guide

## Overview

This guide covers UI testing strategies for the WPF-based Game ContentBuilder application, including manual testing workflows, automated UI testing options, and best practices.

---

## 1. 🎯 Manual UI Testing

### Quick Visual Checklist (5 minutes)

**Purpose**: Verify UI renders correctly and responds to user actions

#### Application Launch
- [ ] Application starts without errors
- [ ] Main window appears with correct title: "Game Content Builder"
- [ ] Window is resizable and has minimize/maximize/close buttons
- [ ] Status bar at bottom shows "Ready" message
- [ ] Menu bar is visible (if applicable)

#### Layout & Structure
- [ ] Left panel shows tree view with categories
- [ ] Right panel is empty initially (shows "Select an item to edit")
- [ ] Splitter between panels works (drag to resize)
- [ ] All icons load correctly in tree view
- [ ] Tree view has proper indentation for nested items

#### Tree Navigation
- [ ] Click to expand "General" category
  - Shows 9 items with appropriate icons
- [ ] Click to expand "Items" → "Weapons"
  - Shows 3 items (Names, Prefixes, Suffixes)
- [ ] Click to expand "Enemies" category
  - Shows 13 creature type subcategories
- [ ] All 93 files are accessible through navigation
- [ ] Expanding/collapsing is smooth (no lag)
- [ ] Icons match category type (Sword for weapons, Skull for enemies, etc.)

---

## 2. 🧪 Editor-Specific UI Tests

### HybridArray Editor (58 files)

**Test File**: `general/colors.json`

#### Initial Load
- [ ] Editor appears in right panel when file is selected
- [ ] Header card displays:
  - File name ("colors")
  - Three statistic chips with counts
  - Proper Material Design styling
- [ ] Three tabs visible: Items, Components, Patterns
- [ ] Items tab selected by default
- [ ] Existing data loads correctly in lists
- [ ] Save button in footer

#### Items Tab Testing
- [ ] Input field has placeholder text
- [ ] Add button is positioned correctly
- [ ] Type "test-item" in input
  - [ ] Add button enables
  - [ ] Click Add → item appears in list
  - [ ] Input field clears after adding
  - [ ] Item count chip updates
- [ ] Hover over delete button on item
  - [ ] Button changes color/highlights
- [ ] Click delete button
  - [ ] Item removed from list
  - [ ] Count decreases
  - [ ] No confirmation dialog (immediate delete)
- [ ] List scrolls if many items
- [ ] Empty state shows helpful message/icon

#### Components Tab Testing
- [ ] Two-panel layout: groups on left, components on right
- [ ] Type "test_group" in left input
  - [ ] Click Add → group appears in left list
- [ ] Select a group from left list
  - [ ] Right panel enables
  - [ ] Shows components for selected group
- [ ] Type "component1" in right input
  - [ ] Click Add → component appears in right list
  - [ ] Component count updates
- [ ] Delete component button works
- [ ] Delete group button works
  - [ ] All components in group are removed
  - [ ] Total count updates correctly

#### Patterns Tab Testing
- [ ] Similar to Items tab layout
- [ ] Can add patterns with special characters: "{color} + {texture}"
- [ ] Patterns display correctly in list
- [ ] Delete works properly
- [ ] Count updates

#### Save Functionality
- [ ] Save button disabled when no changes
- [ ] Make any edit → Save button enables
- [ ] Click Save
  - [ ] Status message shows "Saved [filename] successfully"
  - [ ] Button disables after save (no unsaved changes)
- [ ] Make edit, switch to different file without saving
  - [ ] Should prompt for save or auto-save (check current behavior)

### FlatItem Editor (35 files)

**Test File**: `items/materials/metals.json`

#### Initial Load
- [ ] Editor displays stat block structure
- [ ] Shows list of existing metals (Iron, Steel, etc.)
- [ ] Can expand/collapse item details
- [ ] Proper styling with Material Design

#### Interaction
- [ ] Can view item properties (displayName, traits)
- [ ] Can edit existing items
- [ ] Can add new items
- [ ] Save functionality works
- [ ] Validation prevents invalid data

### NameList Editor (2 files)

**Test File**: `general/adjectives.json`

#### Initial Load
- [ ] Shows simple list of strings
- [ ] Can add new adjectives
- [ ] Can delete adjectives
- [ ] Save works correctly

---

## 3. 🔄 Cross-Editor Testing

### File Switching
- [ ] Select `general/colors.json` (HybridArray)
  - Verify editor loads
- [ ] Select `items/materials/metals.json` (FlatItem)
  - Different editor loads
  - Previous editor is destroyed/cleaned up
- [ ] Rapidly switch between 5 different files
  - No crashes or memory leaks
  - Each editor loads correctly
  - No leftover data from previous file

### Multi-Edit Workflow
1. Open `general/colors.json`
2. Add item "crimson"
3. Switch to `general/textures.json`
4. Add item "rough"
5. Switch back to `general/colors.json`
6. [ ] Changes were saved or still present
7. [ ] UI state is correct

---

## 4. 🎨 Visual/UX Testing

### Material Design Compliance

#### Colors
- [ ] Primary color matches theme (check header cards)
- [ ] Accent color used for buttons (check Add/Save buttons)
- [ ] Text is readable (proper contrast)
- [ ] Disabled controls are visually distinct (grayed out)

#### Typography
- [ ] Headers use proper font sizes (larger than body)
- [ ] Body text is 14-16px (readable)
- [ ] Monospace font for code/JSON previews (if applicable)

#### Icons
- [ ] All icons from MaterialDesignInXamlToolkit render
- [ ] Icons are appropriate size (not too large/small)
- [ ] Icon colors match theme

#### Spacing
- [ ] Proper padding around cards
- [ ] List items have adequate spacing
- [ ] Buttons have proper margins
- [ ] No overlapping elements

#### Elevation/Shadows
- [ ] Cards have subtle shadow/elevation
- [ ] Hover states show visual feedback
- [ ] Selected items are highlighted

### Responsive Design

#### Window Resizing
- [ ] Resize to 600x400 (minimum)
  - UI remains usable
  - No cut-off text
  - Scrollbars appear if needed
- [ ] Resize to 1920x1080 (large)
  - UI scales properly
  - No excessive whitespace
- [ ] Resize to 1024x768 (common)
  - Optimal viewing experience

#### Panel Resizing
- [ ] Drag splitter to make left panel very narrow
  - Tree view remains functional
  - Icons still visible
- [ ] Drag splitter to make right panel very narrow
  - Editor adapts or shows minimum width
- [ ] Restore to balanced split

### Animations & Transitions
- [ ] Tree view expand/collapse has smooth animation
- [ ] Tab switching animates smoothly
- [ ] Button hover states transition smoothly
- [ ] No janky/choppy animations

---

## 5. 🤖 Automated UI Testing Options

### Option A: FlaUI (Recommended for WPF)

**Setup**:
```powershell
# Install FlaUI packages
dotnet add Game.ContentBuilder.Tests package FlaUI.Core
dotnet add Game.ContentBuilder.Tests package FlaUI.UIA3
```

**Example Test**:
```csharp
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Xunit;

public class ContentBuilderUITests : IDisposable
{
    private Application _app;
    private UIA3Automation _automation;
    private Window _mainWindow;

    public ContentBuilderUITests()
    {
        // Launch the app
        var appPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
            "Game.ContentBuilder.exe");
        _app = Application.Launch(appPath);
        _automation = new UIA3Automation();
        _mainWindow = _app.GetMainWindow(_automation);
    }

    [Fact]
    public void MainWindow_Should_Have_Correct_Title()
    {
        // Assert
        Assert.Equal("Game Content Builder", _mainWindow.Title);
    }

    [Fact]
    public void TreeView_Should_Expand_General_Category()
    {
        // Arrange
        var treeView = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CategoryTreeView"));
        
        var generalNode = treeView.FindFirstDescendant(cf => 
            cf.ByName("General"));
        
        // Act
        generalNode.AsTreeItem().Expand();
        
        // Assert
        var children = generalNode.FindAllDescendants(cf => 
            cf.ByControlType(FlaUI.Core.Definitions.ControlType.TreeItem));
        Assert.Equal(9, children.Length); // 9 files under General
    }

    [Fact]
    public void HybridArrayEditor_Should_Add_Item()
    {
        // Arrange
        var treeView = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CategoryTreeView"));
        
        // Navigate to colors.json
        var generalNode = treeView.FindFirstDescendant(cf => cf.ByName("General"));
        generalNode.AsTreeItem().Expand();
        
        var colorsNode = treeView.FindFirstDescendant(cf => cf.ByName("Colors"));
        colorsNode.Click();
        
        // Wait for editor to load
        Wait.UntilResponsive(_mainWindow);
        
        // Find input box
        var inputBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemInputBox"));
        inputBox.AsTextBox().Text = "test-color";
        
        // Find Add button
        var addButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("AddItemButton"));
        addButton.Click();
        
        // Assert
        var itemsList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemsListBox"));
        var items = itemsList.FindAllDescendants(cf => 
            cf.ByControlType(FlaUI.Core.Definitions.ControlType.ListItem));
        
        Assert.Contains(items, item => item.Name == "test-color");
    }

    public void Dispose()
    {
        _app?.Close();
        _automation?.Dispose();
    }
}
```

**Benefits**:
- Native WPF/UIA3 support
- Can interact with any UI element
- Simulates real user actions
- Works with automation IDs

**Limitations**:
- Requires app to be running
- Slower than unit tests
- Can be flaky if UI changes

### Option B: Appium (Cross-Platform)

**Setup**:
```powershell
# Install Appium for Windows
dotnet add package Appium.WebDriver
```

**Use Case**: If you need cross-platform testing (Windows/macOS/Linux)

### Option C: White Framework

**Setup**:
```powershell
dotnet add package TestStack.White
```

**Example**:
```csharp
using TestStack.White;
using TestStack.White.UIItems.WindowItems;
using Xunit;

public class WhiteUITests
{
    [Fact]
    public void CanLaunchApplication()
    {
        var app = Application.Launch("Game.ContentBuilder.exe");
        var window = app.GetWindow("Game Content Builder");
        
        Assert.NotNull(window);
        
        app.Close();
    }
}
```

### Option D: Coded UI Tests (Visual Studio)

**Setup**: Requires Visual Studio Enterprise

**Benefits**:
- Record and playback
- Visual test designer
- IDE integration

**Limitations**:
- Requires Enterprise license
- Less flexible than code-first

---

## 6. 📝 UI Testing Best Practices

### 1. Use AutomationId Properties

**In XAML**:
```xml
<TextBox x:Name="ItemInputBox" 
         AutomationProperties.AutomationId="ItemInputBox"
         AutomationProperties.Name="Item Name Input"/>

<Button x:Name="AddItemButton"
        AutomationProperties.AutomationId="AddItemButton"
        AutomationProperties.Name="Add Item"
        Content="Add"/>
```

**Benefits**:
- Makes elements easily findable in tests
- More stable than using visual tree position
- Improves accessibility

### 2. Test Structure

```csharp
// AAA Pattern: Arrange, Act, Assert
[Fact]
public void Button_Should_Enable_When_Text_Entered()
{
    // Arrange
    var inputBox = FindElement("ItemInputBox");
    var addButton = FindElement("AddItemButton");
    
    // Act
    inputBox.Text = "test";
    
    // Assert
    Assert.True(addButton.IsEnabled);
}
```

### 3. Wait Strategies

```csharp
// Bad: Thread.Sleep(1000) - brittle, wastes time
Thread.Sleep(1000);

// Good: Wait for specific condition
var timeout = TimeSpan.FromSeconds(5);
Retry.WhileException(
    () => _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("Editor")),
    timeout
);
```

### 4. Page Object Pattern

```csharp
public class MainWindowPage
{
    private readonly Window _window;
    
    public MainWindowPage(Window window)
    {
        _window = window;
    }
    
    public TreeView CategoryTree => _window.FindFirstDescendant(cf => 
        cf.ByAutomationId("CategoryTreeView")).AsTreeView();
    
    public void ExpandCategory(string categoryName)
    {
        var node = CategoryTree.FindFirstDescendant(cf => cf.ByName(categoryName));
        node.AsTreeItem().Expand();
    }
    
    public void SelectFile(string fileName)
    {
        var node = CategoryTree.FindFirstDescendant(cf => cf.ByName(fileName));
        node.Click();
    }
}

// Usage in tests
[Fact]
public void CanNavigateToColorsFile()
{
    var mainPage = new MainWindowPage(_mainWindow);
    mainPage.ExpandCategory("General");
    mainPage.SelectFile("Colors");
    // Assert editor loaded
}
```

---

## 7. 🎭 Accessibility Testing

### Keyboard Navigation
- [ ] Tab key moves focus through UI elements in logical order
- [ ] Shift+Tab moves focus backward
- [ ] Enter key activates buttons/default actions
- [ ] Space key toggles checkboxes/selects items
- [ ] Arrow keys navigate tree view
- [ ] Escape key closes dialogs/cancels operations

### Screen Reader Support
- [ ] All buttons have descriptive labels
- [ ] Input fields have associated labels
- [ ] Status messages are announced
- [ ] Tree view items are readable
- [ ] Icons have alt text/accessible names

**Testing Tools**:
- Windows Narrator (built-in)
- NVDA (free screen reader)
- Accessibility Insights for Windows

### Focus Indicators
- [ ] Focused elements have visible outline
- [ ] Focus indicator is high contrast
- [ ] Focus order makes sense
- [ ] No focus traps (can tab out of all areas)

---

## 8. 🔍 Visual Regression Testing

### Option A: Approval Tests

**Setup**:
```powershell
dotnet add package ApprovalTests
```

**Example**:
```csharp
[UseReporter(typeof(DiffReporter))]
public class VisualTests
{
    [Fact]
    public void MainWindow_Should_Match_Approved_Screenshot()
    {
        var screenshot = CaptureWindow(_mainWindow);
        Approvals.Verify(screenshot);
    }
}
```

**Workflow**:
1. First run creates baseline image
2. Subsequent runs compare against baseline
3. Differences trigger test failure
4. Review and approve legitimate changes

### Option B: Percy (Cloud Service)

**Setup**:
```powershell
dotnet add package PercyIO.Selenium
```

**Benefits**:
- Cloud-based screenshot comparison
- Visual diffs in browser
- Cross-browser testing
- Team collaboration

---

## 9. 🚀 Performance Testing

### UI Responsiveness

```csharp
[Fact]
public void LargeFile_Should_Load_Within_500ms()
{
    var stopwatch = Stopwatch.StartNew();
    
    // Select file with 1000+ items
    SelectFile("large-test-file.json");
    
    // Wait for editor to load
    WaitForEditorLoad();
    
    stopwatch.Stop();
    
    Assert.True(stopwatch.ElapsedMilliseconds < 500, 
        $"Load took {stopwatch.ElapsedMilliseconds}ms");
}

[Fact]
public void Adding_100_Items_Should_Remain_Responsive()
{
    for (int i = 0; i < 100; i++)
    {
        AddItem($"item-{i}");
    }
    
    // UI should still respond to clicks
    var addButton = FindElement("AddItemButton");
    Assert.True(addButton.IsEnabled);
}
```

### Memory Leak Testing

```csharp
[Fact]
public void Opening_100_Files_Should_Not_Leak_Memory()
{
    var initialMemory = GC.GetTotalMemory(true);
    
    for (int i = 0; i < 100; i++)
    {
        OpenRandomFile();
        CloseFile();
    }
    
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
    
    var finalMemory = GC.GetTotalMemory(true);
    var growth = finalMemory - initialMemory;
    
    // Memory growth should be minimal
    Assert.True(growth < 50_000_000, // 50 MB threshold
        $"Memory grew by {growth / 1_000_000} MB");
}
```

---

## 10. 📊 Test Coverage Tracking

### UI Test Coverage Checklist

**Core Workflows** (Must Test):
- [x] Application launch
- [x] File navigation (tree view)
- [x] HybridArray editor operations
- [x] FlatItem editor operations
- [x] NameList editor operations
- [x] Save/load cycle
- [ ] Error handling UI
- [ ] Multiple file editing

**Advanced Features** (Should Test):
- [ ] Undo/redo (if implemented)
- [ ] Search/filter (if implemented)
- [ ] Bulk operations (if implemented)
- [ ] Export/import (if implemented)

**Edge Cases** (Nice to Test):
- [ ] Very large files (1000+ items)
- [ ] Empty files
- [ ] Corrupted JSON
- [ ] Special characters in data
- [ ] Unicode support

---

## 11. 🎯 Recommended UI Testing Strategy

### Phase 1: Manual Testing (Week 1)
1. Complete visual checklist (Section 1)
2. Test all three editor types (Section 2)
3. Document any bugs found
4. Create video walkthrough of features

### Phase 2: Add AutomationIds (Week 2)
1. Add AutomationProperties to all XAML controls
2. Verify with Accessibility Insights
3. Re-test manually to ensure no regressions

### Phase 3: Automated Tests (Week 3)
1. Set up FlaUI in test project
2. Write 5-10 critical path tests:
   - Launch app
   - Navigate to file
   - Edit and save
   - Switch between editors
   - Error handling
3. Run tests in CI/CD pipeline

### Phase 4: Ongoing
1. Add new UI tests for each feature
2. Run manual visual checklist before releases
3. Monitor for flaky tests
4. Update tests when UI changes

---

## 12. 🛠️ Implementation Guide

### Step 1: Add AutomationIds to HybridArrayEditorView.xaml

```xml
<!-- Header -->
<materialDesign:Card AutomationProperties.AutomationId="EditorHeaderCard">
    <TextBlock Text="{Binding FileName}" 
               AutomationProperties.AutomationId="FileNameText"/>
    <materialDesign:Chip Content="{Binding TotalItemsCount}"
                         AutomationProperties.AutomationId="ItemsCountChip"/>
</materialDesign:Card>

<!-- Items Tab -->
<TabControl AutomationProperties.AutomationId="EditorTabControl">
    <TabItem Header="Items" AutomationProperties.AutomationId="ItemsTab">
        <TextBox x:Name="ItemInputBox"
                 AutomationProperties.AutomationId="ItemInputBox"/>
        <Button Content="Add"
                AutomationProperties.AutomationId="AddItemButton"/>
        <ListBox ItemsSource="{Binding Items}"
                 AutomationProperties.AutomationId="ItemsListBox"/>
    </TabItem>
</TabControl>

<!-- Save Button -->
<Button Content="Save Changes"
        AutomationProperties.AutomationId="SaveButton"/>
```

### Step 2: Create FlaUI Test Project

```powershell
# Add FlaUI packages to existing test project
dotnet add Game.ContentBuilder.Tests package FlaUI.Core --version 4.0.0
dotnet add Game.ContentBuilder.Tests package FlaUI.UIA3 --version 4.0.0
```

### Step 3: Write First UI Test

Create `Game.ContentBuilder.Tests/UI/BasicUITests.cs`:

```csharp
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Xunit;

namespace Game.ContentBuilder.Tests.UI;

public class BasicUITests : IDisposable
{
    private readonly Application _app;
    private readonly UIA3Automation _automation;
    private readonly Window _mainWindow;

    public BasicUITests()
    {
        // Build path to exe (adjust based on your build output)
        var exePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..",
            "Game.ContentBuilder", "bin", "Debug", "net9.0-windows",
            "Game.ContentBuilder.exe"
        );

        _app = Application.Launch(Path.GetFullPath(exePath));
        _automation = new UIA3Automation();
        _mainWindow = _app.GetMainWindow(_automation, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void Application_Should_Launch_Successfully()
    {
        Assert.NotNull(_mainWindow);
        Assert.Equal("Game Content Builder", _mainWindow.Title);
    }

    [Fact]
    public void StatusBar_Should_Show_Ready_Message()
    {
        var statusText = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("StatusMessageText"));
        
        Assert.Contains("Ready", statusText.Name);
    }

    public void Dispose()
    {
        _app?.Close();
        _automation?.Dispose();
    }
}
```

### Step 4: Run UI Tests

```powershell
# Make sure ContentBuilder is built first
dotnet build Game.ContentBuilder

# Run all tests (unit + UI)
dotnet test Game.ContentBuilder.Tests

# Run only UI tests
dotnet test Game.ContentBuilder.Tests --filter "FullyQualifiedName~UI"
```

---

## 13. 📋 Quick Test Commands

```powershell
# Build ContentBuilder
dotnet build Game.ContentBuilder

# Manual testing - launch app
dotnet run --project Game.ContentBuilder

# Run all tests (unit tests only for now)
dotnet test Game.ContentBuilder.Tests

# After adding FlaUI, run UI tests
dotnet test Game.ContentBuilder.Tests --filter "Category=UI"

# Run with verbose output to see FlaUI interactions
dotnet test Game.ContentBuilder.Tests --logger "console;verbosity=detailed"
```

---

## Summary

### Testing Pyramid for ContentBuilder

```
        /\
       /  \      E2E/Manual Tests (Slow, Comprehensive)
      /____\     
     /      \    UI Automated Tests (Medium, Critical Paths)
    /________\   
   /          \  Unit Tests (Fast, Comprehensive)
  /____________\ 
```

**Recommended Approach**:
1. ✅ **Unit Tests** (Done): Fast, test ViewModels, business logic
2. 🎯 **Manual UI Testing** (Next): Visual verification, UX feedback
3. 🤖 **Automated UI Tests** (Future): Critical path automation with FlaUI
4. 🔄 **Continuous Testing**: Run tests on every commit

### Next Steps

1. **Immediate**: Run through manual visual checklist (15 min)
2. **This Week**: Add AutomationIds to XAML controls
3. **Next Week**: Set up FlaUI and write 5 critical tests
4. **Ongoing**: Expand test coverage incrementally

**Ready to start UI testing!** Would you like me to:
- Walk you through the manual checklist?
- Add AutomationIds to the XAML files?
- Set up FlaUI for automated testing?
