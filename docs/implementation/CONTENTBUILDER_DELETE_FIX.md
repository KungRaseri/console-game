# ContentBuilder Delete Button Fix

**Date:** December 16, 2025  
**Status:** ✅ Complete

## Problem

Delete buttons in the HybridArrayEditorView were not deleting the correct items. The buttons were:
1. Located inside each row of the ListBox (not separate like other editors)
2. Not passing `CommandParameter` to specify which item to delete
3. Relying on `SelectedItem`/`SelectedPattern` which might not match the clicked row

This meant clicking a delete button might delete the wrong item, or no item at all if nothing was selected.

## Root Cause

The XAML had delete buttons inside `ItemTemplate`:
```xaml
<Button Command="{Binding DataContext.DeleteItemCommand, RelativeSource={RelativeSource AncestorType=ListBox}}"
        ToolTip="Delete">
    <!-- No CommandParameter! -->
</Button>
```

The ViewModel expected to use `SelectedItem`:
```csharp
[RelayCommand(CanExecute = nameof(CanDeleteItem))]
private void DeleteItem()
{
    if (SelectedItem != null)
    {
        Items.Remove(SelectedItem); // Wrong item might be deleted!
    }
}
```

## Solution

### 1. Updated ViewModel Commands to Accept Parameters

Changed all delete commands in `HybridArrayEditorViewModel.cs` to accept the item to delete as a parameter:

**Before:**
```csharp
[RelayCommand(CanExecute = nameof(CanDeleteItem))]
private void DeleteItem()
{
    if (SelectedItem != null)
    {
        Items.Remove(SelectedItem);
        SelectedItem = null;
    }
}

private bool CanDeleteItem() => SelectedItem != null;
```

**After:**
```csharp
[RelayCommand]
private void DeleteItem(string? item)
{
    if (item != null && Items.Contains(item))
    {
        Items.Remove(item);
        if (SelectedItem == item)
            SelectedItem = null;
        UpdateCounts();
        StatusMessage = $"Deleted item. Total: {Items.Count}";
    }
}
```

### 2. Updated XAML to Pass CommandParameter

Added `CommandParameter="{Binding}"` to all delete buttons:

**Before:**
```xaml
<Button Command="{Binding DataContext.DeleteItemCommand, RelativeSource={RelativeSource AncestorType=ListBox}}"
        ToolTip="Delete">
    <materialDesign:PackIcon Kind="Delete"/>
</Button>
```

**After:**
```xaml
<Button Command="{Binding DataContext.DeleteItemCommand, RelativeSource={RelativeSource AncestorType=ListBox}}"
        CommandParameter="{Binding}"
        ToolTip="Delete">
    <materialDesign:PackIcon Kind="Delete"/>
</Button>
```

### 3. Added Delete Buttons to Components

The components list didn't have delete buttons at all! Added a proper ItemTemplate with delete buttons:

**Before:**
```xaml
<ListBox.ItemTemplate>
    <DataTemplate>
        <TextBlock Text="{Binding}" TextWrapping="Wrap"/>
    </DataTemplate>
</ListBox.ItemTemplate>
```

**After:**
```xaml
<ListBox.ItemTemplate>
    <DataTemplate>
        <Grid Margin="0,4">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>
            <TextBlock Grid.Column="0" 
                       Text="{Binding}" 
                       VerticalAlignment="Center"
                       TextTrimming="CharacterEllipsis"/>
            <Button Grid.Column="1" 
                    Style="{StaticResource MaterialDesignIconButton}"
                    Command="{Binding DataContext.DeleteComponentCommand, RelativeSource={RelativeSource AncestorType=ListBox}}"
                    CommandParameter="{Binding}"
                    ToolTip="Delete">
                <materialDesign:PackIcon Kind="Delete" Foreground="{DynamicResource MaterialDesign.Brush.Validation.Error}"/>
            </Button>
        </Grid>
    </DataTemplate>
</ListBox.ItemTemplate>
```

### 4. Added DeleteComponent Command

Created a new command to delete individual components:

```csharp
[RelayCommand]
private void DeleteComponent(string? component)
{
    if (SelectedComponentGroup != null && component != null && SelectedComponentGroup.Components.Contains(component))
    {
        SelectedComponentGroup.Components.Remove(component);
        UpdateCounts();
        StatusMessage = $"Deleted component from {SelectedComponentGroup.Name}. Total: {SelectedComponentGroup.Components.Count}";
    }
}
```

### 5. Fixed Command Notifications

Also added `NotifyCanExecuteChangedFor` attributes to ensure commands are enabled/disabled correctly:

```csharp
[ObservableProperty]
[NotifyCanExecuteChangedFor(nameof(AddComponentCommand))]
[NotifyCanExecuteChangedFor(nameof(DeleteComponentGroupCommand))]
private ComponentGroup? _selectedComponentGroup;
```

## Changes Made

### Files Modified

1. **Game.ContentBuilder/ViewModels/HybridArrayEditorViewModel.cs**
   - Changed `DeleteItem()` to accept `string? item` parameter
   - Changed `DeletePattern()` to accept `PatternComponent? pattern` parameter
   - Added `DeleteComponent(string? component)` method
   - Added `NotifyCanExecuteChangedFor` attributes to `_selectedComponentGroup`
   - Removed `CanDelete*` predicates (no longer needed without CanExecute)

2. **Game.ContentBuilder/Views/HybridArrayEditorView.xaml**
   - Added `CommandParameter="{Binding}"` to Items delete button
   - Added `CommandParameter="{Binding}"` to Patterns delete button
   - Added complete ItemTemplate with delete buttons to Components list

## Testing Checklist

To verify the fix works correctly:

- [ ] Open ContentBuilder
- [ ] Navigate to Items → Weapons → Names
- [ ] **Items Tab:** Add several items, click delete on specific item → correct item removed ✅
- [ ] **Components Tab:** Select a group, add components, click delete on specific component → correct component removed ✅
- [ ] **Patterns Tab:** Add patterns, click delete on specific pattern → correct pattern removed ✅
- [ ] Save file and verify changes persist
- [ ] Reload file and verify structure is correct

## Other Editors

### Editors That Use Separate Delete Buttons (No Changes Needed)

These editors use a different UI pattern with a separate DELETE button and `SelectedItem`, which works correctly:

1. **ItemEditorViewModel** (Prefixes/Suffixes)
   - Has separate DELETE button
   - Uses `SelectedItem` binding on ListBox
   - Works correctly ✅

2. **FlatItemEditorViewModel** (Items like Potions)
   - Has separate DELETE button
   - Uses `SelectedItem` binding on ListBox
   - Works correctly ✅

3. **NameListEditorViewModel** (Name lists)
   - Has "REMOVE SELECTED" button
   - Uses `SelectedName` binding on ListBox
   - Works correctly ✅

## Technical Notes

### Why CommandParameter Works

When a button is inside a `DataTemplate`, the `DataContext` of that button is the **individual list item** (e.g., a string, PatternComponent, etc.). By using `CommandParameter="{Binding}"`, we pass that item to the command method.

The `RelativeSource` binding is needed to reach the parent ViewModel:
```xaml
Command="{Binding DataContext.DeleteItemCommand, RelativeSource={RelativeSource AncestorType=ListBox}}"
```

This says: "Find the ListBox ancestor, get its DataContext (the ViewModel), and bind to DeleteItemCommand on that ViewModel."

### Pattern: Two Delete Button Approaches

**Approach 1: Inline Delete Buttons (HybridArrayEditor)**
- Delete button in each row
- Uses `CommandParameter="{Binding}"` to pass the item
- Command signature: `void Delete(ItemType? item)`
- Doesn't require CanExecute predicate
- User doesn't need to select first

**Approach 2: Separate Delete Button (ItemEditor, FlatItemEditor, NameListEditor)**
- Single DELETE button outside the list
- Relies on `SelectedItem` binding
- Command signature: `void Delete()`
- Uses `CanExecute` predicate: `SelectedItem != null`
- User must select item first

Both approaches are valid! HybridArrayEditor uses inline buttons because it's more intuitive for rapid editing of many items.

## Related Issues Fixed

This fix also resolved:
- ✅ Add Pattern button not enabling (fixed with `NotifyCanExecuteChangedFor`)
- ✅ Add Component button not enabling when component group selected
- ✅ Missing delete functionality for individual components

## Summary

All delete operations in HybridArrayEditorView now correctly delete the **specific item clicked**, not the selected item. This makes the UI more intuitive and prevents accidental deletions of the wrong items.

**Build Status:** ✅ Successful  
**Ready for Testing:** ✅ Yes
