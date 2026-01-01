# ContentBuilder Auto-Generated Metadata Implementation

**Date:** December 16, 2025  
**Status:** 📋 Planning  
**Priority:** HIGH

## Overview

Implement automatic metadata generation in ContentBuilder to eliminate manual maintenance and ensure accuracy.

## Problem Statement

**Current Issues:**
- ❌ Users must manually update `componentKeys`, `patternTokens`, counts
- ❌ Easy to forget or make mistakes
- ❌ Metadata can become out of sync with actual data
- ❌ Tedious to maintain, especially during rapid editing

**Solution:**
Auto-generate metadata on save, preserving only user-defined fields.

---

## Architecture

### User-Defined Fields (Editable)

Users maintain ONLY:
- `description` - Human-written explanation
- `version` - Schema version (user increments manually for breaking changes)

### Auto-Generated Fields (Computed)

System generates:
- `lastUpdated` - Timestamp (YYYY-MM-DD)
- `componentKeys` - Extracted from `components` object
- `patternTokens` - Parsed from `patterns` array + "base"
- `totalPatterns` - Count of patterns
- `total_items` - Count of items (if applicable)
- `[category]_count` - Count of nested categories (e.g., `weapon_types: 7`)

---

## Implementation Plan

### Phase 1: Create MetadataGenerator Service

**File:** `RealmForge/Services/MetadataGenerator.cs` (NEW)

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RealmForge.Services;

public static class MetadataGenerator
{
    /// <summary>
    /// Generate metadata object with user-defined and auto-generated fields
    /// </summary>
    public static Dictionary<string, object> Generate(
        string userDescription,
        string userVersion,
        Dictionary<string, object> components,
        List<string> patterns,
        List<object>? items = null,
        Dictionary<string, object>? customFields = null)
    {
        var metadata = new Dictionary<string, object>
        {
            // User-defined fields (editable in UI)
            ["description"] = userDescription ?? "No description",
            ["version"] = userVersion ?? "1.0",
            
            // Auto-generated fields
            ["lastUpdated"] = DateTime.Now.ToString("yyyy-MM-dd"),
            ["componentKeys"] = ExtractComponentKeys(components),
            ["patternTokens"] = ExtractPatternTokens(patterns)
        };
        
        // Optional counts
        if (patterns?.Count > 0)
            metadata["totalPatterns"] = patterns.Count;
            
        if (items?.Count > 0)
            metadata["total_items"] = items.Count;
        
        // Count nested categories (e.g., weapon_types)
        foreach (var kvp in components)
        {
            if (kvp.Key.EndsWith("_types") && kvp.Value is IDictionary dict)
            {
                var typeName = kvp.Key.Replace("_types", "");
                metadata[$"{typeName}_count"] = dict.Count;
            }
        }
        
        // Merge custom fields if provided
        if (customFields != null)
        {
            foreach (var kvp in customFields)
            {
                metadata[kvp.Key] = kvp.Value;
            }
        }
        
        return metadata;
    }
    
    /// <summary>
    /// Extract component keys, excluding category types
    /// </summary>
    private static string[] ExtractComponentKeys(Dictionary<string, object> components)
    {
        return components.Keys
            .Where(k => !k.EndsWith("_types") && !k.EndsWith("_categories"))
            .OrderBy(k => k)
            .ToArray();
    }
    
    /// <summary>
    /// Extract all unique tokens from patterns, including "base"
    /// </summary>
    private static string[] ExtractPatternTokens(List<string> patterns)
    {
        var tokens = new HashSet<string> { "base" }; // Always include "base"
        
        if (patterns == null || patterns.Count == 0)
            return new[] { "base" };
        
        foreach (var pattern in patterns)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                continue;
                
            var parts = pattern.Split(new[] { " + " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var token = part.Trim();
                if (!string.IsNullOrWhiteSpace(token))
                    tokens.Add(token);
            }
        }
        
        return tokens.OrderBy(t => t).ToArray();
    }
    
    /// <summary>
    /// Validate that all pattern tokens exist as component keys or "base"
    /// </summary>
    public static List<string> ValidatePatternTokens(
        List<string> patterns, 
        Dictionary<string, object> components)
    {
        var errors = new List<string>();
        var validKeys = new HashSet<string>(components.Keys) { "base", "item" };
        
        foreach (var pattern in patterns)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                continue;
                
            var tokens = pattern.Split(new[] { " + " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                var cleanToken = token.Trim();
                if (!validKeys.Contains(cleanToken))
                {
                    errors.Add($"Pattern '{pattern}' uses unknown token '{cleanToken}'");
                }
            }
        }
        
        return errors;
    }
}
```

---

### Phase 2: Update HybridArrayEditorViewModel

**File:** `RealmForge/ViewModels/HybridArrayEditorViewModel.cs`

**Add Properties:**

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

public partial class HybridArrayEditorViewModel : ObservableObject
{
    // User-editable metadata fields
    [ObservableProperty]
    private string metadataDescription = "No description";
    
    [ObservableProperty]
    private string metadataVersion = "1.0";
    
    // Computed properties for UI display (read-only)
    public string LastUpdated => DateTime.Now.ToString("yyyy-MM-dd");
    
    public string ComponentKeysDisplay => Components.Keys
        .Where(k => !k.EndsWith("_types"))
        .OrderBy(k => k)
        .Aggregate((a, b) => $"{a}, {b}");
    
    public string PatternTokensDisplay
    {
        get
        {
            var tokens = new HashSet<string> { "base" };
            foreach (var pattern in Patterns)
            {
                var parts = pattern.Pattern.Split(new[] { " + " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                    tokens.Add(part.Trim());
            }
            return tokens.OrderBy(t => t).Aggregate((a, b) => $"{a}, {b}");
        }
    }
    
    public int TotalPatterns => Patterns.Count;
    public int TotalItems => Items?.Count ?? 0;
    
    // Load metadata from JSON
    private void LoadMetadata(Dictionary<string, object>? metadata)
    {
        if (metadata == null)
            return;
            
        if (metadata.TryGetValue("description", out var desc) && desc is string description)
            MetadataDescription = description;
            
        if (metadata.TryGetValue("version", out var ver) && ver is string version)
            MetadataVersion = version;
    }
    
    // Generate metadata for saving
    private Dictionary<string, object> GenerateMetadata()
    {
        var componentsDict = Components.ToDictionary(
            kvp => kvp.Key, 
            kvp => kvp.Value as object);
            
        var patternslist = Patterns.Select(p => p.Pattern).ToList();
        var itemsList = Items?.ToList<object>();
        
        return MetadataGenerator.Generate(
            MetadataDescription,
            MetadataVersion,
            componentsDict,
            patternslist,
            itemsList
        );
    }
}
```

**Update SaveFile Method:**

```csharp
private void SaveFile()
{
    try
    {
        var data = new
        {
            items = Items,
            components = Components,
            patterns = Patterns.Select(p => p.Pattern).ToList(),
            metadata = GenerateMetadata()  // Auto-generate!
        };
        
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(FilePath, json);
        
        // Notify property changed for display values
        OnPropertyChanged(nameof(LastUpdated));
        OnPropertyChanged(nameof(ComponentKeysDisplay));
        OnPropertyChanged(nameof(PatternTokensDisplay));
        OnPropertyChanged(nameof(TotalPatterns));
        OnPropertyChanged(nameof(TotalItems));
        
        ConsoleUI.ShowSuccess($"Saved {Path.GetFileName(FilePath)} with auto-generated metadata");
    }
    catch (Exception ex)
    {
        ConsoleUI.ShowError($"Failed to save file: {ex.Message}");
    }
}
```

---

### Phase 3: Update UI

**File:** `RealmForge/Views/HybridArrayEditorView.xaml`

**Add Metadata Section:**

```xml
<!-- Add after existing content, before closing Grid -->
<Expander Grid.Row="4" 
          Header="Metadata" 
          IsExpanded="True" 
          Margin="10"
          Style="{StaticResource MaterialDesignExpander}">
    <StackPanel Margin="10">
        <!-- User-Editable Fields -->
        <TextBlock Text="User-Defined Fields" 
                   FontWeight="Bold" 
                   Margin="0,0,0,10"/>
        
        <TextBox Text="{Binding MetadataDescription, UpdateSourceTrigger=PropertyChanged}"
                 materialDesign:HintAssist.Hint="Description"
                 TextWrapping="Wrap"
                 AcceptsReturn="True"
                 MinLines="2"
                 MaxLines="4"
                 Margin="0,5"/>
        
        <TextBox Text="{Binding MetadataVersion, UpdateSourceTrigger=PropertyChanged}"
                 materialDesign:HintAssist.Hint="Version"
                 Margin="0,5"/>
        
        <!-- Auto-Generated Fields -->
        <Separator Margin="0,15,0,10"/>
        
        <TextBlock Text="Auto-Generated (Read-Only)" 
                   FontWeight="Bold" 
                   Foreground="{DynamicResource MaterialDesignBody}"
                   Margin="0,0,0,10"/>
        
        <Grid>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
            </Grid.RowDefinitions>
            
            <TextBlock Grid.Row="0" Grid.Column="0" 
                       Text="Last Updated:" 
                       Margin="0,2,10,2"
                       FontWeight="SemiBold"/>
            <TextBlock Grid.Row="0" Grid.Column="1" 
                       Text="{Binding LastUpdated}" 
                       Margin="0,2"/>
            
            <TextBlock Grid.Row="1" Grid.Column="0" 
                       Text="Component Keys:" 
                       Margin="0,2,10,2"
                       FontWeight="SemiBold"/>
            <TextBlock Grid.Row="1" Grid.Column="1" 
                       Text="{Binding ComponentKeysDisplay}" 
                       TextWrapping="Wrap"
                       Margin="0,2"/>
            
            <TextBlock Grid.Row="2" Grid.Column="0" 
                       Text="Pattern Tokens:" 
                       Margin="0,2,10,2"
                       FontWeight="SemiBold"/>
            <TextBlock Grid.Row="2" Grid.Column="1" 
                       Text="{Binding PatternTokensDisplay}" 
                       TextWrapping="Wrap"
                       Margin="0,2"/>
            
            <TextBlock Grid.Row="3" Grid.Column="0" 
                       Text="Total Patterns:" 
                       Margin="0,2,10,2"
                       FontWeight="SemiBold"/>
            <TextBlock Grid.Row="3" Grid.Column="1" 
                       Text="{Binding TotalPatterns}" 
                       Margin="0,2"/>
            
            <TextBlock Grid.Row="4" Grid.Column="0" 
                       Text="Total Items:" 
                       Margin="0,2,10,2"
                       FontWeight="SemiBold"/>
            <TextBlock Grid.Row="4" Grid.Column="1" 
                       Text="{Binding TotalItems}" 
                       Margin="0,2"/>
        </Grid>
    </StackPanel>
</Expander>
```

---

### Phase 4: Add Validation

**Add to HybridArrayEditorViewModel:**

```csharp
private void ValidatePatterns()
{
    var errors = MetadataGenerator.ValidatePatternTokens(
        Patterns.Select(p => p.Pattern).ToList(),
        Components.ToDictionary(kvp => kvp.Key, kvp => kvp.Value as object)
    );
    
    if (errors.Any())
    {
        var message = string.Join("\n", errors);
        ConsoleUI.ShowWarning($"Pattern Validation Warnings:\n{message}");
    }
    else
    {
        ConsoleUI.ShowSuccess("All patterns are valid!");
    }
}

// Call ValidatePatterns() when adding/editing patterns
```

---

## Testing Plan

### Unit Tests

**File:** `RealmForge.Tests/Services/MetadataGeneratorTests.cs`

```csharp
public class MetadataGeneratorTests
{
    [Fact]
    public void Generate_ShouldExtractComponentKeys()
    {
        var components = new Dictionary<string, object>
        {
            ["material"] = new List<string> { "Iron", "Steel" },
            ["quality"] = new List<string> { "Fine", "Superior" },
            ["weapon_types"] = new Dictionary<string, object>() // Should be excluded
        };
        
        var metadata = MetadataGenerator.Generate(
            "Test description",
            "1.0",
            components,
            new List<string>()
        );
        
        var keys = metadata["componentKeys"] as string[];
        keys.Should().Contain("material");
        keys.Should().Contain("quality");
        keys.Should().NotContain("weapon_types");
    }
    
    [Fact]
    public void Generate_ShouldExtractPatternTokens()
    {
        var patterns = new List<string>
        {
            "base",
            "material + base",
            "quality + material + base"
        };
        
        var metadata = MetadataGenerator.Generate(
            "Test",
            "1.0",
            new Dictionary<string, object>(),
            patterns
        );
        
        var tokens = metadata["patternTokens"] as string[];
        tokens.Should().Contain("base");
        tokens.Should().Contain("material");
        tokens.Should().Contain("quality");
    }
    
    [Fact]
    public void ValidatePatternTokens_ShouldDetectInvalidTokens()
    {
        var patterns = new List<string> { "invalid_token + base" };
        var components = new Dictionary<string, object>
        {
            ["material"] = new List<string>()
        };
        
        var errors = MetadataGenerator.ValidatePatternTokens(patterns, components);
        
        errors.Should().HaveCount(1);
        errors[0].Should().Contain("invalid_token");
    }
}
```

---

## Rollout Plan

### Step 1: Implement MetadataGenerator ✅
- Create service with tests
- Verify extraction logic

### Step 2: Update ViewModel ✅
- Add metadata properties
- Update save/load logic
- Add validation

### Step 3: Update UI ✅
- Add metadata expander
- Wire up bindings
- Test user flow

### Step 4: Migrate Existing Files
- Open each JSON file in ContentBuilder
- Verify description/version
- Save to regenerate metadata
- Compare before/after

### Step 5: Documentation
- Update user guide
- Add screenshots
- Document metadata fields

---

## Success Criteria

- ✅ Metadata auto-generates on every save
- ✅ User only edits description and version
- ✅ All auto-generated fields are accurate
- ✅ Pattern validation catches errors
- ✅ UI displays metadata clearly
- ✅ All existing JSON files migrated successfully

---

## Benefits Summary

| Before | After |
|--------|-------|
| Manual maintenance of componentKeys | ✅ Auto-extracted |
| Manual maintenance of patternTokens | ✅ Auto-extracted |
| Manual counting of patterns/items | ✅ Auto-counted |
| Metadata gets out of sync | ✅ Always accurate |
| Easy to forget to update | ✅ Impossible to forget |
| User maintains 7+ fields | ✅ User maintains 2 fields |

---

## Timeline

- **Day 1:** Implement MetadataGenerator service + tests
- **Day 2:** Update ViewModel and save/load logic
- **Day 3:** Update UI and wire up bindings
- **Day 4:** Testing, validation, and polish
- **Day 5:** Migrate existing files and documentation

**Total:** 5 days (1 week sprint)
