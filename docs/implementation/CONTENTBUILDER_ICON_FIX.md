# ContentBuilder Icon Fix

**Date:** December 16, 2025  
**Issue:** Infinite error loop when clicking General/Colors  
**Status:** ✅ Fixed

## Problem

When navigating to General → Colors in the ContentBuilder, the application crashed with an infinite loop of error messages:

```
System.FormatException: Pattern is not a valid value for PackIconKind.
System.ArgumentException: Requested value 'Pattern' was not found.
```

## Root Cause

In `HybridArrayEditorView.xaml`, the Patterns tab was using an invalid MaterialDesign icon:

```xaml
<materialDesign:PackIcon Kind="Pattern" ... />
```

The MaterialDesign icon library doesn't have an icon named `Pattern`, causing XAML parsing to fail repeatedly during layout rendering.

## Solution

Changed the icon from `Pattern` to `ShapeOutline` (a valid MaterialDesign icon):

```xaml
<materialDesign:PackIcon Kind="ShapeOutline" Margin="0,0,8,0" VerticalAlignment="Center"/>
```

**File Changed:** `Game.ContentBuilder\Views\HybridArrayEditorView.xaml` (Line 264)

## Also Fixed

While investigating, also fixed a potential infinite property notification loop in `ComponentGroup` class by converting from source-generated properties to manual `SetProperty` calls.

**File Changed:** `Game.ContentBuilder\ViewModels\HybridArrayEditorViewModel.cs`

Changed from:
```csharp
public partial class ComponentGroup : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<string> _components = new();
}
```

To:
```csharp
public class ComponentGroup : ObservableObject
{
    private ObservableCollection<string> _components = new();
    
    public ObservableCollection<string> Components
    {
        get => _components;
        set => SetProperty(ref _components, value);
    }
}
```

## Testing

- ✅ Build successful
- ✅ No more XAML parsing errors
- ✅ Application should now load General/Colors view without errors

## Valid MaterialDesign Icon Names

For future reference, some valid pattern-related icons:
- `ShapeOutline` ✅ (used)
- `ShapePlus`
- `Shape`
- `Grid`
- `GridLarge`
- `FormatListBulleted`
- `ViewList`

See: https://pictogrammers.com/library/mdi/ for full icon list.
