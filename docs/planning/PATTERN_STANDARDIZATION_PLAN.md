# Pattern System Standardization Plan

**Date:** December 16, 2025  
**Status:** ✅ Phase 1 Complete → 🚀 Phase 2 In Progress

## Overview

Standardize the pattern system across all JSON data files, update ContentBuilder to enforce the standard, then implement runtime pattern execution in the game.

## Phase 1: Define the Standard ✅

### Component Key Naming Convention

**Simple, intuitive keys that match pattern tokens exactly.**

#### Key Categories

1. **Prefixes** (describe the base item)
   - `material` - Physical material (Iron, Steel, Wood, Leather)
   - `quality` - Craftsmanship level (Fine, Superior, Masterwork)
   - `descriptive` - Special attributes (Ancient, Enchanted, Cursed)
   - `size` - Size modifiers (Small, Large, Massive)
   - `color` - Color descriptors (Red, Blue, Golden)
   - `origin` - Origin/creator (Elven, Dwarven, Dragon)
   - `condition` - State/age (Worn, Pristine, Ruined)

2. **Suffixes** (add-ons, typically start with "of")
   - `enchantment` - Magical properties (of Slaying, of Power)
   - `title` - Named items (of the Dragon, of the Hero)
   - `purpose` - Intended use (of Defense, of Battle)

3. **Special Tokens**
   - `base` - Resolves to items array
   - `item` - Alias for base

4. **Categories** (organizational, nested objects)
   - `weapon_types` - {swords: [], axes: [], ...}
   - `armor_types` - {helmets: [], chest: [], ...}
   - `enemy_types` - {beasts: [], undead: [], ...}

### Pattern Syntax

```
pattern ::= token | token " + " pattern
token   ::= "base" | component_key
```

**Examples:**

- `"base"` → "Longsword"
- `"material + base"` → "Iron Longsword"
- `"quality + material + base"` → "Fine Steel Longsword"
- `"base + enchantment"` → "Longsword of Slaying"
- `"descriptive + base + title"` → "Ancient Longsword of the Dragon"

### Pattern Execution Rules

1. Tokens are evaluated left to right
2. Each token picks a random value from its component array
3. Special token `base` picks from items array
4. Results are joined with spaces
5. If a token's component is empty, skip it (graceful degradation)

## Phase 2: Standardize JSON Files ✅ COMPLETE

**All 113 JSON files have been standardized** - December 16, 2025

### Completion Summary

| Category | Files | Status | Details |
|----------|-------|--------|---------|
| **General** | 9 | ✅ Complete | Component libraries, pattern generation, config |
| **Items** | 17 | ✅ Complete | Weapons, armor, consumables, materials, enchantments |
| **Enemies** | 59 | ✅ Complete | 13 enemy types with full trait systems |
| **NPCs** | 14 | ✅ Complete | Names, occupations, personalities, dialogue |
| **Quests** | 14 | ✅ Complete | Templates, objectives, rewards, locations |
| **TOTAL** | **113** | **✅ 100%** | All files standardized with metadata |

### What Was Standardized

**Every JSON file now includes:**

1. **Metadata Block** with auto-generated fields:
   - `description` - Human-readable file purpose
   - `version` - Schema version number
   - `last_updated` - Timestamp (YYYY-MM-DD)
   - `type` - File type classification
   - Auto-generated counts (component_keys, pattern_tokens, total_items, etc.)

2. **Structure Standardization** - All files follow one of these patterns:
   - **Pattern Generation** (names.json) - components + patterns for procedural generation
   - **Item/Enemy Catalogs** (types.json) - type-level traits + item arrays with stats
   - **Prefix/Suffix Modifiers** - rarity-organized stat modifiers
   - **Component Libraries** - categorized reference data (no patterns)
   - **Configuration Files** - game settings and rules

3. **Weight-Based Rarity** - All components and items have `rarityWeight` values:
   - Components contribute to emergent rarity calculation
   - No hardcoded rarity tiers in most files
   - Rarity emerges from combined component weights
   - Configured via `general/rarity_config.json`

4. **Consistent Naming** - Component keys match pattern tokens exactly:
   - Singular keys (material, not materials)
   - Semantic names (descriptive, not prefix_desc)
   - Universal components (material, quality, descriptive) work across categories
   - Category-specific components only where needed

### Files Structure Examples

**Pattern Generation (names.json):**
```json
{
  "metadata": {
    "description": "Weapon name generation with pattern-based system",
    "version": "3.0",
    "last_updated": "2025-12-16",
    "type": "pattern_generation",
    "component_keys": ["material", "quality", "descriptive"],
    "pattern_tokens": ["base", "material", "quality"],
    "total_patterns": 9
  },
  "components": {
    "material": ["Iron", "Steel"],
    "quality": ["Fine", "Superior"]
  },
  "patterns": [
    "base",
    "material + base",
    "quality + material + base"
  ]
}
```

**Item Catalog (types.json):**
```json
{
  "metadata": {
    "description": "Weapon type catalog with base stats",
    "version": "1.0",
    "type": "item_catalog",
    "total_weapon_types": 7,
    "total_weapons": 59
  },
  "weapon_types": {
    "swords": {
      "traits": { "damageType": "slashing", "slot": "mainhand" },
      "items": [
        { "name": "Longsword", "damage": "1d8", "rarityWeight": 5 }
      ]
    }
  }
}
```

**Standard Template (HybridArray - names.json):**
```json
{
  "metadata": {
    "description": "Description of this file",
    "version": "1.0",
    "last_updated": "2025-12-16",
    "type": "pattern_generation",
    "component_keys": ["material", "quality", "descriptive", "enchantment"],
    "pattern_tokens": ["base", "material", "quality", "descriptive", "enchantment"],
    "total_patterns": 6
  },
  "components": {
    "material": ["Iron", "Steel"],
    "quality": ["Fine", "Superior"],
    "descriptive": ["Ancient", "Enchanted"],
    "enchantment": ["of Slaying", "of Power"]
  },
  "patterns": [
    "base",
    "material + base",
    "quality + material + base",
    "descriptive + base",
    "base + enchantment",
    "material + base + enchantment"
  ]
}
```

## Phase 3: Update ContentBuilder 🚀 IN PROGRESS

**Goal:** Update the WPF ContentBuilder tool to support all 113 standardized JSON files with enhanced editing capabilities.

### Implementation Plan (Based on User Preferences)

**User Decisions:**
1. ✅ Metadata & Notes - Right-side tab panel
2. ✅ Pattern Validation - All indicators (icon, tooltip, status bar)
3. ✅ Live Examples - Below pattern input AND in side panel
4. ✅ Priority: Metadata → Validation → Live Examples → Autocomplete
5. ✅ Testing: Implement features, then add tests
6. ⏳ Weight-Based Rarity - Deferred to Phase 4

---

### 3.1 Metadata & Notes Tab (Right Panel) ⭐ PRIORITY 1

**File:** `Game.ContentBuilder/Views/HybridArrayEditorView.xaml`

**UI Layout:**
```
┌────────────────────────────────────────────────────────┐
│ Main Content (Left 70%)      │ Metadata (Right 30%)    │
│                               │                         │
│ [Items Tab]                   │ ┌─ Metadata ─────────┐ │
│ [Components Tab]              │ │ Description:        │ │
│ [Patterns Tab]                │ │ [text box]          │ │
│                               │ │                     │ │
│                               │ │ Version:            │ │
│                               │ │ [text box]          │ │
│                               │ │                     │ │
│                               │ │ ─ Auto-Generated ── │ │
│                               │ │ Last Updated:       │ │
│                               │ │ 2025-12-16          │ │
│                               │ │                     │ │
│                               │ │ Component Keys:     │ │
│                               │ │ material, quality   │ │
│                               │ │                     │ │
│                               │ │ Pattern Tokens:     │ │
│                               │ │ base, material      │ │
│                               │ │                     │ │
│                               │ │ Total Patterns: 9   │ │
│                               │ └─────────────────────┘ │
│                               │                         │
│                               │ ┌─ Notes ────────────┐ │
│                               │ │ [text box]          │ │
│                               │ │ (freeform notes)    │ │
│                               │ └─────────────────────┘ │
└────────────────────────────────────────────────────────┘
```

**Implementation:**

**ViewModel Properties:**
```csharp
// HybridArrayEditorViewModel.cs

// User-editable metadata
[ObservableProperty]
private string metadataDescription = "";

[ObservableProperty]
private string metadataVersion = "1.0";

[ObservableProperty]
private string notes = ""; // Freeform notes, saved to metadata.notes

// Auto-generated metadata (read-only, computed)
public string LastUpdated => DateTime.Now.ToString("yyyy-MM-dd");

public string ComponentKeysDisplay => string.Join(", ", 
    ComponentGroups.Select(g => g.Name).OrderBy(n => n));

public string PatternTokensDisplay
{
    get
    {
        var tokens = new HashSet<string> { "base" };
        foreach (var pattern in Patterns)
        {
            var parts = pattern.Pattern.Split(new[] { " + " }, 
                StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
                tokens.Add(part.Trim());
        }
        return string.Join(", ", tokens.OrderBy(t => t));
    }
}

public int TotalPatternsCount => Patterns.Count;
public int TotalItemsCount => Items.Count;
```

**XAML Layout:**
```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="7*"/>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="3*"/>
    </Grid.ColumnDefinitions>
    
    <!-- Main Content (existing tabs) -->
    <TabControl Grid.Column="0">
        <!-- Items, Components, Patterns tabs -->
    </TabControl>
    
    <!-- Splitter -->
    <GridSplitter Grid.Column="1" Width="5" 
                  HorizontalAlignment="Stretch" 
                  Background="{DynamicResource MaterialDesign.Brush.Background}"/>
    
    <!-- Right Panel: Metadata & Notes -->
    <ScrollViewer Grid.Column="2" Margin="8,0,0,0">
        <StackPanel>
            <!-- Metadata Section -->
            <materialDesign:Card Padding="16" Margin="0,0,0,16">
                <StackPanel>
                    <TextBlock Text="Metadata" 
                               FontSize="16" 
                               FontWeight="Bold" 
                               Margin="0,0,0,12"/>
                    
                    <!-- User-Editable Fields -->
                    <TextBox Text="{Binding MetadataDescription, UpdateSourceTrigger=PropertyChanged}"
                             materialDesign:HintAssist.Hint="Description"
                             TextWrapping="Wrap"
                             AcceptsReturn="True"
                             MinLines="3"
                             MaxLines="5"
                             Margin="0,0,0,12"/>
                    
                    <TextBox Text="{Binding MetadataVersion, UpdateSourceTrigger=PropertyChanged}"
                             materialDesign:HintAssist.Hint="Version"
                             Margin="0,0,0,16"/>
                    
                    <!-- Auto-Generated Fields -->
                    <Separator Margin="0,0,0,12"/>
                    <TextBlock Text="Auto-Generated" 
                               FontSize="12" 
                               FontWeight="SemiBold" 
                               Opacity="0.7"
                               Margin="0,0,0,8"/>
                    
                    <TextBlock Text="{Binding LastUpdated, StringFormat='Last Updated: {0}'}" 
                               FontSize="11"
                               Margin="0,2"/>
                    <TextBlock Text="{Binding ComponentKeysDisplay, StringFormat='Components: {0}'}" 
                               FontSize="11"
                               TextWrapping="Wrap"
                               Margin="0,2"/>
                    <TextBlock Text="{Binding PatternTokensDisplay, StringFormat='Tokens: {0}'}" 
                               FontSize="11"
                               TextWrapping="Wrap"
                               Margin="0,2"/>
                    <TextBlock Text="{Binding TotalPatternsCount, StringFormat='Patterns: {0}'}" 
                               FontSize="11"
                               Margin="0,2"/>
                    <TextBlock Text="{Binding TotalItemsCount, StringFormat='Items: {0}'}" 
                               FontSize="11"
                               Margin="0,2"/>
                </StackPanel>
            </materialDesign:Card>
            
            <!-- Notes Section -->
            <materialDesign:Card Padding="16">
                <StackPanel>
                    <TextBlock Text="Notes" 
                               FontSize="16" 
                               FontWeight="Bold" 
                               Margin="0,0,0,12"/>
                    <TextBox Text="{Binding Notes, UpdateSourceTrigger=PropertyChanged}"
                             materialDesign:HintAssist.Hint="Add your notes here..."
                             TextWrapping="Wrap"
                             AcceptsReturn="True"
                             MinLines="10"
                             VerticalScrollBarVisibility="Auto"/>
                </StackPanel>
            </materialDesign:Card>
        </StackPanel>
    </ScrollViewer>
</Grid>
```

**Service:** `Game.ContentBuilder/Services/MetadataGenerator.cs` (NEW)

```csharp
public static class MetadataGenerator
{
    public static JObject Generate(
        string description,
        string version,
        string? notes,
        List<ComponentGroup> componentGroups,
        List<PatternItem> patterns,
        List<string> items)
    {
        var metadata = new JObject
        {
            ["description"] = description,
            ["version"] = version,
            ["last_updated"] = DateTime.Now.ToString("yyyy-MM-dd"),
            ["type"] = "pattern_generation", // Could be inferred
            ["component_keys"] = new JArray(
                componentGroups.Select(g => g.Name).OrderBy(n => n)),
            ["pattern_tokens"] = new JArray(ExtractTokens(patterns)),
            ["total_patterns"] = patterns.Count,
            ["total_items"] = items.Count
        };
        
        if (!string.IsNullOrWhiteSpace(notes))
            metadata["notes"] = notes;
        
        return metadata;
    }
    
    private static IEnumerable<string> ExtractTokens(List<PatternItem> patterns)
    {
        var tokens = new HashSet<string> { "base" };
        foreach (var pattern in patterns)
        {
            var parts = pattern.Pattern.Split(new[] { " + " }, 
                StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
                tokens.Add(part.Trim());
        }
        return tokens.OrderBy(t => t);
    }
}
```

**Save Integration:**
```csharp
private void SaveFile()
{
    var metadata = MetadataGenerator.Generate(
        MetadataDescription,
        MetadataVersion,
        Notes,
        ComponentGroups.ToList(),
        Patterns.ToList(),
        Items.ToList()
    );
    
    var data = new JObject
    {
        ["metadata"] = metadata,
        ["components"] = BuildComponentsObject(),
        ["patterns"] = new JArray(Patterns.Select(p => p.Pattern)),
        ["items"] = new JArray(Items) // Only if Items exist
    };
    
    File.WriteAllText(_filePath, data.ToString(Formatting.Indented));
}
```

---

### 3.2 Pattern Validation (All Indicators) ⭐ PRIORITY 2

**Service:** `Game.ContentBuilder/Services/PatternValidator.cs` (NEW)

```csharp
public enum ValidationLevel
{
    Valid,
    Warning,
    Error
}

public class ValidationResult
{
    public ValidationLevel Level { get; set; }
    public string Message { get; set; } = "";
    public List<string> InvalidTokens { get; set; } = new();
}

public static class PatternValidator
{
    public static ValidationResult Validate(
        string pattern, 
        List<ComponentGroup> componentGroups)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return new ValidationResult 
            { 
                Level = ValidationLevel.Error, 
                Message = "Pattern cannot be empty" 
            };
        
        var tokens = pattern.Split(new[] { " + " }, 
            StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .ToList();
        
        if (tokens.Count == 0)
            return new ValidationResult 
            { 
                Level = ValidationLevel.Error, 
                Message = "Pattern has no valid tokens" 
            };
        
        var componentNames = componentGroups.Select(g => g.Name).ToHashSet();
        var invalidTokens = new List<string>();
        
        foreach (var token in tokens)
        {
            // Special tokens are always valid
            if (token == "base" || token == "item")
                continue;
            
            // Check if token exists in components
            if (!componentNames.Contains(token))
                invalidTokens.Add(token);
        }
        
        if (invalidTokens.Count > 0)
        {
            return new ValidationResult
            {
                Level = ValidationLevel.Error,
                Message = $"Unknown tokens: {string.Join(", ", invalidTokens)}",
                InvalidTokens = invalidTokens
            };
        }
        
        // Check for empty components (warning, not error)
        var emptyComponents = tokens
            .Where(t => t != "base" && t != "item")
            .Where(t => componentGroups
                .FirstOrDefault(g => g.Name == t)
                ?.Items.Count == 0)
            .ToList();
        
        if (emptyComponents.Count > 0)
        {
            return new ValidationResult
            {
                Level = ValidationLevel.Warning,
                Message = $"Empty components: {string.Join(", ", emptyComponents)}"
            };
        }
        
        return new ValidationResult 
        { 
            Level = ValidationLevel.Valid, 
            Message = "✓ Pattern is valid" 
        };
    }
}
```

**ViewModel:**
```csharp
// PatternItem model
public class PatternItem : ObservableObject
{
    [ObservableProperty]
    private string pattern = "";
    
    [ObservableProperty]
    private ValidationResult? validationResult;
    
    public string ValidationIcon => ValidationResult?.Level switch
    {
        ValidationLevel.Valid => "✓",
        ValidationLevel.Warning => "⚠",
        ValidationLevel.Error => "✗",
        _ => ""
    };
    
    public Brush ValidationColor => ValidationResult?.Level switch
    {
        ValidationLevel.Valid => Brushes.Green,
        ValidationLevel.Warning => Brushes.Orange,
        ValidationLevel.Error => Brushes.Red,
        _ => Brushes.Gray
    };
}

// In HybridArrayEditorViewModel
partial void OnNewPatternInputChanged(string value)
{
    // Real-time validation as user types
    var validation = PatternValidator.Validate(value, ComponentGroups.ToList());
    NewPatternValidation = validation;
    GenerateLiveExamples(); // Trigger live examples
}

[ObservableProperty]
private ValidationResult? newPatternValidation;

[RelayCommand(CanExecute = nameof(CanAddPattern))]
private void AddPattern()
{
    if (string.IsNullOrWhiteSpace(NewPatternInput)) return;
    
    var validation = PatternValidator.Validate(
        NewPatternInput, 
        ComponentGroups.ToList());
    
    var patternItem = new PatternItem 
    { 
        Pattern = NewPatternInput,
        ValidationResult = validation
    };
    
    Patterns.Add(patternItem);
    NewPatternInput = string.Empty;
    OnPropertyChanged(nameof(TotalPatternsCount));
}

private bool CanAddPattern() => 
    !string.IsNullOrWhiteSpace(NewPatternInput) &&
    NewPatternValidation?.Level != ValidationLevel.Error;
```

**XAML:**
```xml
<!-- Patterns Tab -->
<Grid Margin="16">
    <!-- Add Pattern -->
    <materialDesign:Card Padding="16" Margin="0,0,0,16">
        <DockPanel>
            <Button DockPanel.Dock="Right" 
                    Command="{Binding AddPatternCommand}"
                    Style="{StaticResource MaterialDesignRaisedButton}"
                    Margin="8,0,0,0">
                <StackPanel Orientation="Horizontal">
                    <materialDesign:PackIcon Kind="Plus" Margin="0,0,4,0"/>
                    <TextBlock Text="Add"/>
                </StackPanel>
            </Button>
            
            <!-- Validation Indicator -->
            <Border DockPanel.Dock="Right"
                    Width="40"
                    Height="40"
                    Margin="8,0"
                    VerticalAlignment="Center"
                    ToolTip="{Binding NewPatternValidation.Message}">
                <TextBlock Text="{Binding NewPatternValidation.ValidationIcon}"
                           FontSize="24"
                           Foreground="{Binding NewPatternValidation.ValidationColor}"
                           HorizontalAlignment="Center"
                           VerticalAlignment="Center"/>
            </Border>
            
            <TextBox Text="{Binding NewPatternInput, UpdateSourceTrigger=PropertyChanged}"
                     materialDesign:HintAssist.Hint="New pattern (e.g., material + base)..."
                     Style="{StaticResource MaterialDesignOutlinedTextBox}"/>
        </DockPanel>
        
        <!-- Validation Message -->
        <TextBlock Text="{Binding NewPatternValidation.Message}"
                   Foreground="{Binding NewPatternValidation.ValidationColor}"
                   Margin="0,8,0,0"
                   FontSize="11"/>
    </materialDesign:Card>
    
    <!-- Pattern List with Validation Icons -->
    <ListBox ItemsSource="{Binding Patterns}"
             SelectedItem="{Binding SelectedPattern}">
        <ListBox.ItemTemplate>
            <DataTemplate>
                <DockPanel>
                    <TextBlock DockPanel.Dock="Left"
                               Text="{Binding ValidationIcon}"
                               Foreground="{Binding ValidationColor}"
                               FontSize="14"
                               Margin="0,0,8,0"
                               ToolTip="{Binding ValidationResult.Message}"/>
                    <TextBlock Text="{Binding Pattern}"/>
                </DockPanel>
            </DataTemplate>
        </ListBox.ItemTemplate>
    </ListBox>
</Grid>
```

---

### 3.3 Live Example Preview (Below Input + Side Panel) ⭐ PRIORITY 3

**Service Update:** `Game.ContentBuilder/Services/PatternExampleGenerator.cs`

```csharp
public static List<string> GenerateMultipleExamples(
    string pattern, 
    JArray? items, 
    JObject? components, 
    int count = 5)
{
    var examples = new List<string>();
    
    for (int i = 0; i < count; i++)
    {
        var example = GenerateExample(pattern, items, components);
        if (!string.IsNullOrEmpty(example) && example != "(no data available)")
            examples.Add(example);
    }
    
    return examples.Distinct().ToList(); // Remove duplicates
}

// Simplified token resolution (remove fuzzy matching)
private static string? ResolveToken(string token, JArray? items, JObject? components)
{
    // Special tokens
    if (token == "base" || token == "item")
    {
        if (items != null && items.Count > 0)
            return items[Random.Shared.Next(items.Count)]?.ToString();
        return null;
    }
    
    // Direct component lookup (no fuzzy matching)
    if (components?[token] is JArray componentArray && componentArray.Count > 0)
    {
        return componentArray[Random.Shared.Next(componentArray.Count)]?.ToString();
    }
    
    return null; // Token not found
}
```

**ViewModel:**
```csharp
[ObservableProperty]
private ObservableCollection<string> liveExamples = new();

private void GenerateLiveExamples()
{
    if (string.IsNullOrWhiteSpace(NewPatternInput))
    {
        LiveExamples.Clear();
        return;
    }
    
    var examples = PatternExampleGenerator.GenerateMultipleExamples(
        NewPatternInput,
        _itemsData,
        _componentsData,
        count: 5
    );
    
    LiveExamples.Clear();
    foreach (var example in examples)
        LiveExamples.Add(example);
}

[RelayCommand]
private void RefreshExamples()
{
    GenerateLiveExamples();
}
```

**XAML:**
```xml
<!-- Below Pattern Input -->
<materialDesign:Card Padding="16" Margin="0,0,0,16"
                     Visibility="{Binding LiveExamples.Count, 
                                  Converter={StaticResource CountToVisibilityConverter}}">
    <StackPanel>
        <DockPanel Margin="0,0,0,8">
            <TextBlock DockPanel.Dock="Left"
                       Text="Live Examples"
                       FontSize="14"
                       FontWeight="SemiBold"/>
            <Button DockPanel.Dock="Right"
                    Command="{Binding RefreshExamplesCommand}"
                    Style="{StaticResource MaterialDesignIconButton}"
                    ToolTip="Regenerate examples">
                <materialDesign:PackIcon Kind="Refresh"/>
            </Button>
        </DockPanel>
        
        <ItemsControl ItemsSource="{Binding LiveExamples}">
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <TextBlock Text="{Binding}" 
                               FontFamily="Consolas"
                               FontSize="12"
                               Margin="0,2"
                               Opacity="0.8"/>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
    </StackPanel>
</materialDesign:Card>

<!-- Also in Right Panel under Metadata -->
<materialDesign:Card Padding="16" Margin="0,0,0,16"
                     Visibility="{Binding SelectedPattern, 
                                  Converter={StaticResource NullToVisibilityConverter}}">
    <StackPanel>
        <TextBlock Text="Pattern Examples" 
                   FontSize="14" 
                   FontWeight="SemiBold"
                   Margin="0,0,0,8"/>
        <TextBlock Text="{Binding SelectedPattern.Pattern}"
                   FontFamily="Consolas"
                   FontSize="11"
                   Foreground="{DynamicResource MaterialDesign.Brush.Primary}"
                   Margin="0,0,0,12"/>
        <ItemsControl ItemsSource="{Binding SelectedPatternExamples}">
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <TextBlock Text="{Binding}" 
                               FontFamily="Consolas"
                               FontSize="11"
                               Margin="0,2"/>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
    </StackPanel>
</materialDesign:Card>
```

---

### 3.4 Token Autocomplete (Future Enhancement)

**Deferred to post-Phase 3** - Nice to have but not critical for initial release.

**Potential Implementation:**
- Use `materialDesign:AutoSuggestBox` or custom autocomplete
- Suggest available component keys while typing
- Show "base" token in suggestions
- Syntax highlighting for valid/invalid tokens

---

## Phase 4: Runtime Implementation 🎮

### Create PatternExecutor Service

**File:** `Game.Shared/Services/PatternExecutor.cs`

```csharp
public class PatternExecutor
{
    /// <summary>
    /// Execute a pattern like "quality + material + base" to generate a name
    /// </summary>
    public static string Execute(
        string pattern, 
        List<string> items, 
        Dictionary<string, List<string>> components, 
        Random random)
    {
        var tokens = pattern.Split(new[] { " + " }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim())
                            .ToArray();
        
        var parts = new List<string>();
        
        foreach (var token in tokens)
        {
            string? value = ResolveToken(token, items, components, random);
            if (!string.IsNullOrEmpty(value))
            {
                parts.Add(value);
            }
        }
        
        return parts.Count > 0 
            ? string.Join(" ", parts) 
            : items[random.Next(items.Count)]; // Fallback to random item
    }
    
    private static string? ResolveToken(
        string token, 
        List<string> items, 
        Dictionary<string, List<string>> components, 
        Random random)
    {
        // Special tokens
        if (token == "base" || token == "item")
        {
            return items.Count > 0 ? items[random.Next(items.Count)] : null;
        }
        
        // Component lookup
        if (components.TryGetValue(token, out var componentList) && componentList.Count > 0)
        {
            return componentList[random.Next(componentList.Count)];
        }
        
        // Token not found - log warning and skip
        Log.Warning("Pattern token '{Token}' not found in components", token);
        return null;
    }
}
```

### Update Data Models

**File:** `Game.Shared/Data/Models/GameDataModels.cs`

Add `Patterns` property to data classes:

```csharp
public class WeaponNameData
{
    public List<string> Items { get; set; } = new(); // Changed from WeaponNameItems
    public Dictionary<string, object> Components { get; set; } = new();
    public List<string> Patterns { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}
```

### Update Generators

**File:** `Game.Core/Generators/ItemGenerator.cs`

Replace manual name construction with pattern execution:

```csharp
private static string GenerateWeaponName(Faker f, Item item)
{
    var data = GameDataService.Instance.WeaponNames;
    
    // Pick a random pattern
    if (data.Patterns.Count > 0)
    {
        var pattern = f.PickRandom(data.Patterns);
        return PatternExecutor.Execute(
            pattern, 
            data.Items, 
            data.Components, 
            f.Random
        );
    }
    
    // Fallback to random item if no patterns
    return f.PickRandom(data.Items);
}
```

**Similar updates for:**

- `EnemyGenerator.cs` - Use patterns for enemy names
- `NpcGenerator.cs` - Use patterns for NPC names/titles

## Phase 5: Testing & Validation ✅

### Unit Tests

**File:** `Game.Tests/Services/PatternExecutorTests.cs`

```csharp
[Fact]
public void Execute_SimplePattern_GeneratesName()
{
    var items = new List<string> { "Sword", "Axe" };
    var components = new Dictionary<string, List<string>>
    {
        ["material"] = new List<string> { "Iron", "Steel" }
    };
    
    var result = PatternExecutor.Execute("material + base", items, components, new Random(42));
    
    result.Should().MatchRegex(@"^(Iron|Steel) (Sword|Axe)$");
}

[Fact]
public void Execute_InvalidToken_SkipsToken()
{
    var items = new List<string> { "Sword" };
    var components = new Dictionary<string, List<string>>();
    
    var result = PatternExecutor.Execute("invalid + base", items, components, new Random());
    
    result.Should().Be("Sword"); // Invalid token skipped
}
```

### Integration Tests

**File:** `Game.Tests/Generators/ItemGeneratorIntegrationTests.cs`

```csharp
[Fact]
public void GenerateWeaponName_UsesPatterns_FromJsonData()
{
    // Ensure JSON data is loaded
    GameDataService.Initialize("../../../Data/Json");
    
    var item = new Item { Rarity = ItemRarity.Rare };
    var faker = new Faker();
    
    // Generate 100 names, ensure they follow pattern structure
    for (int i = 0; i < 100; i++)
    {
        var name = ItemGenerator.GenerateWeaponName(faker, item);
        name.Should().NotBeNullOrEmpty();
        // Could add more specific validation
    }
}
```

### ContentBuilder Tests

**File:** `Game.ContentBuilder.Tests/Services/PatternExampleGeneratorTests.cs`

```csharp
[Fact]
public void GenerateExample_StandardPattern_CreatesValidExample()
{
    var items = new JArray { "Sword", "Axe" };
    var components = new JObject
    {
        ["material"] = new JArray { "Iron", "Steel" },
        ["quality"] = new JArray { "Fine", "Superior" }
    };
    
    var result = PatternExampleGenerator.GenerateExample(
        "quality + material + base", 
        items, 
        components
    );
    
    result.Should().MatchRegex(@"^(Fine|Superior) (Iron|Steel) (Sword|Axe)$");
}
```

## Phase 6: Documentation 📚

### Files to Create/Update

1. **GDD Update** - Add pattern system documentation
2. **Component Key Reference** - List of standard component keys
3. **Pattern Syntax Guide** - How to write patterns
4. **Migration Guide** - For updating existing JSON files
5. **ContentBuilder User Guide** - How to use pattern features

### Example Documentation

**Pattern System Guide** (`docs/guides/PATTERN_SYSTEM.md`):

```markdown
# Pattern System Guide

## Overview
The pattern system generates dynamic names by combining components using patterns.

## Basic Syntax
`token + token + ...`

## Tokens
- `base` - Random item from items array
- `material` - Random material component
- `quality` - Random quality component
- `enchantment` - Random enchantment suffix

## Examples
- `"base"` → "Longsword"
- `"material + base"` → "Iron Longsword"
- `"quality + material + base"` → "Fine Steel Longsword"

## Adding New Patterns
1. Open ContentBuilder
2. Navigate to file with patterns
3. Go to Patterns tab
4. Type pattern in text box (autocomplete available)
5. See live examples as you type
6. Click Add
7. Save file
```

## Execution Timeline

### Phase 1: Define the Standard ✅ COMPLETE

- [x] Define standard component keys and patterns
- [x] Document pattern syntax and execution rules
- [x] Create standardization templates
- [x] Completed: December 16, 2025

### Phase 2: Standardize JSON Files ✅ COMPLETE

- [x] Standardize all 113 JSON files
- [x] Add metadata to all files
- [x] Implement weight-based rarity system
- [x] Verify all patterns use correct token names
- [x] Completed: December 16, 2025

### Phase 3: ContentBuilder Updates 🚀 IN PROGRESS

**Day 1: Metadata & Notes Panel**
- [ ] Create MetadataGenerator service
- [ ] Add right-side panel to HybridArrayEditorView
- [ ] Implement user-editable fields (Description, Version, Notes)
- [ ] Implement auto-generated display (Last Updated, Component Keys, Pattern Tokens, Counts)
- [ ] Integrate with save/load functionality
- [ ] Test with existing JSON files

**Day 2: Pattern Validation**
- [ ] Create PatternValidator service
- [ ] Add ValidationResult model with levels (Valid/Warning/Error)
- [ ] Update PatternItem model with validation properties
- [ ] Add real-time validation to pattern input
- [ ] Add validation icons to pattern list (✓/⚠/✗)
- [ ] Add tooltips and status messages
- [ ] Test validation with various pattern combinations

**Day 3: Live Example Preview**
- [ ] Update PatternExampleGenerator.GenerateMultipleExamples()
- [ ] Simplify token resolution (remove fuzzy matching)
- [ ] Add LiveExamples collection to ViewModel
- [ ] Add example display below pattern input
- [ ] Add example display in right panel for selected pattern
- [ ] Add refresh button for regenerating examples
- [ ] Test with real JSON data

**Day 4: Polish & Testing**
- [ ] Add keyboard shortcuts (Ctrl+S for save, etc.)
- [ ] Add loading/saving indicators
- [ ] Test all 113 files in ContentBuilder
- [ ] Fix any UI/UX issues
- [ ] Update user documentation
- [ ] Code review and cleanup

### Phase 4: Runtime Implementation 📋 PLANNED

**Day 1: PatternExecutor Service**
- [ ] Create PatternExecutor service in Game.Shared
- [ ] Implement Execute() method for pattern resolution
- [ ] Add unit tests for pattern execution
- [ ] Test with various pattern combinations

**Day 2: Update Data Models & Generators**
- [ ] Update GameDataModels with Patterns property
- [ ] Update ItemGenerator to use patterns
- [ ] Update EnemyGenerator to use patterns
- [ ] Update NpcGenerator to use patterns
- [ ] Add integration tests

**Day 3: Testing & Validation**
- [ ] Run all integration tests
- [ ] Generate test items/enemies/NPCs
- [ ] Verify names are generated correctly
- [ ] Performance testing
- [ ] Fix any issues

**Day 4: Documentation & Polish**
- [ ] Write Pattern System Guide
- [ ] Update GDD
- [ ] Create migration guide
- [ ] Final testing
- [ ] Code review

## Success Criteria

✅ **Data Standardization**

- All JSON files use consistent component keys
- All patterns use tokens that match component keys
- No hardcoded abbreviations or aliases

✅ **ContentBuilder**

- Pattern validation works
- Live examples generate correctly
- Invalid patterns show warnings
- Save/load preserves structure

✅ **Runtime**

- PatternExecutor generates valid names
- Generators use patterns from JSON
- No crashes on invalid patterns
- Graceful degradation

✅ **Quality**

- Unit test coverage >80%
- Integration tests pass
- No performance regressions
- Documentation complete

## Rollback Plan

If issues arise:

1. Keep old generator code in comments
2. Feature flag: `USE_PATTERN_EXECUTION` (default: false)
3. Gradual rollout: Enable for one category at a time
4. Monitor logs for pattern resolution failures
5. Can revert to manual generation if needed

## Notes

- Keep weapon_types, armor_types as nested objects (organizational)
- Don't validate category objects as patterns (they're data groupings)
- Pattern execution should be fast (<1ms per name)
- Cache compiled patterns if performance becomes issue
- Consider pattern weights in future (some patterns more common)

---

## Current Status Summary

### ✅ Completed (Phases 1-2)

- **Phase 1**: Standard defined with component keys and pattern syntax
- **Phase 2**: All 113 JSON files standardized with metadata and weight-based rarity

### 🚀 In Progress (Phase 3)

**ContentBuilder Implementation** - Enhancing the WPF tool with:

1. **Metadata & Notes Panel** (Right side)
   - User-editable: Description, Version, Notes
   - Auto-generated: Last Updated, Component Keys, Pattern Tokens, Counts

2. **Pattern Validation** (All indicators)
   - Real-time validation as user types
   - Visual indicators: ✓ Valid / ⚠ Warning / ✗ Error
   - Tooltips with validation messages
   - Status bar integration

3. **Live Example Preview** (Dual display)
   - Below pattern input (always visible while typing)
   - In right panel for selected pattern
   - Refresh button to regenerate examples
   - Shows 5 diverse examples

### 📋 Planned (Phase 4)

**Runtime Implementation** - Game engine integration:
- PatternExecutor service for runtime pattern resolution
- Update all generators (Items, Enemies, NPCs) to use patterns
- Comprehensive testing and validation
- Performance optimization

---

**Next Action**: Begin Phase 3 implementation starting with Metadata & Notes Panel.
