# Granular Naming Components Guide

**Date**: January 3, 2026  
**Status**: ✅ Implemented  
**Models Updated**: Item, Enemy, Ability, NPC

## Overview

All domain models now include **granular naming component properties** that preserve the individual parts used to build entity names. This enables:

- ✅ **Proper display in Godot UI** - Access individual name parts for tooltips, filtering, localization
- ✅ **Debugging** - See exactly which components were selected during generation
- ✅ **Localization** - Rebuild names in different orders for different languages
- ✅ **UI Features** - Filter by material, enchantment, title, etc.

## Models Updated

### Item (RealmEngine.Shared/Models/Item.cs)

```csharp
// Component Properties
public string? QualityPrefix { get; set; }          // "Masterwork", "Crude"
public string? MaterialPrefix { get; set; }         // "Mithril", "Silver"
public List<string> EnchantmentPrefixes { get; set; } // ["Flaming"]
public List<string> EnchantmentSuffixes { get; set; } // ["of Power", "of the Titan"]
public string? SocketsText { get; set; }            // "[0/2 Sockets]"

// Helper Method
public string ComposeNameFromComponents()
{
    // Order: [EnchantPrefixes] [Quality] [Material] [Base] [EnchantSuffixes] [Sockets]
    // Returns: "Flaming Masterwork Mithril Longsword of Power [0/2 Sockets]"
}
```

### Enemy (RealmEngine.Shared/Models/Enemy.cs)

```csharp
// Component Properties
public string BaseName { get; set; }           // "Wolf", "Dragon"
public string? SizePrefix { get; set; }        // "Giant", "Tiny"
public string? TypePrefix { get; set; }        // "Frost", "Shadow"
public string? DescriptivePrefix { get; set; } // "Enraged", "Elite"
public string? TitleSuffix { get; set; }       // "the Devourer", "of the Abyss"

// Helper Method
public string ComposeNameFromComponents()
{
    // Order: [Size] [Type] [Descriptive] [Base] [Title]
    // Returns: "Giant Frost Enraged Wolf the Devourer"
}
```

### Ability (RealmEngine.Shared/Models/Ability.cs)

```csharp
// Component Properties
public string? BaseAbilityName { get; set; }  // "Fireball", "Shield"
public string? PowerPrefix { get; set; }      // "Greater", "Lesser", "Supreme"
public string? SchoolPrefix { get; set; }     // "Frost", "Holy", "Shadow"

// Helper Method
public string ComposeDisplayNameFromComponents()
{
    // Order: [Power] [School] [Base]
    // Returns: "Greater Frost Fireball"
}
```

### NPC (RealmEngine.Shared/Models/NPC.cs)

```csharp
// Component Properties
public string? BaseName { get; set; }      // "Garrick", "Elara"
public string? TitlePrefix { get; set; }   // "Master", "Lord", "Apprentice"
public string? TitleSuffix { get; set; }   // "the Wise", "of Stormwind"

// Helper Method
public string ComposeNameFromComponents()
{
    // Order: [TitlePrefix] [Base] [TitleSuffix]
    // Returns: "Master Garrick the Wise"
}
```

## NameComposer Utility

Location: `RealmEngine.Core/Generators/NameComposer.cs`

### Purpose
Utility class for composing entity names from pattern-based components using names.json files.

### Usage in Generators

```csharp
public class EnemyGenerator
{
    private readonly NameComposer _nameComposer;
    
    public EnemyGenerator(/* ... */, ILoggerFactory loggerFactory)
    {
        _nameComposer = new NameComposer(loggerFactory.CreateLogger<NameComposer>());
    }
    
    private void ApplyNameFromPattern(Enemy enemy, string category)
    {
        // Load names.json
        var namesPath = $"enemies/{category}/names.json";
        var namesFile = _dataCache.GetFile(namesPath);
        if (namesFile?.JsonData == null) return;
        
        // Select random pattern
        var patterns = namesFile.JsonData["patterns"];
        var pattern = _nameComposer.GetRandomWeightedPattern(patterns);
        var patternStr = pattern?["pattern"]?.Value<string>() ?? "base";
        
        // Compose name from pattern
        var components = namesFile.JsonData["components"];
        var name = _nameComposer.ComposeName(patternStr, components, out var componentValues);
        
        // Populate model properties
        enemy.Name = name;
        enemy.BaseName = componentValues.GetValueOrDefault("base", "");
        enemy.SizePrefix = componentValues.GetValueOrDefault("size");
        enemy.TypePrefix = componentValues.GetValueOrDefault("type");
        enemy.DescriptivePrefix = componentValues.GetValueOrDefault("descriptive");
        enemy.TitleSuffix = componentValues.GetValueOrDefault("title");
    }
}
```

## Generator Implementation Status

| Generator | Status | Notes |
|-----------|--------|-------|
| **ItemGenerator** | ✅ Implemented | Populates all component properties in BuildEnhancedName() |
| **EnemyGenerator** | ⏸️ Pending | Needs pattern-based name generation added |
| **AbilityGenerator** | ⏸️ Pending | Needs pattern-based name generation added |
| **NPCGenerator** | ⏸️ Not Created | Generator doesn't exist yet |

## Testing

### ItemNamingComponentsTests (6 tests - All Passing ✅)

```bash
dotnet test --filter "FullyQualifiedName~ItemNamingComponentsTests"
```

**Tests:**
- ✅ Generated_Items_Should_Have_BaseName_Property
- ✅ Items_With_Material_Should_Have_MaterialPrefix_Property
- ✅ Items_With_Enchantments_Should_Have_Prefix_Or_Suffix_Lists
- ✅ Items_With_Sockets_Should_Have_SocketsText_Property
- ✅ ComposeNameFromComponents_Should_Match_Generated_Name
- ✅ Generated_Items_Should_Not_Have_Suffix_Enchantments_At_Start_Of_Name

### NameComposerTests (6 tests - All Passing ✅)

```bash
dotnet test --filter "FullyQualifiedName~NameComposerTests"
```

**Tests:**
- ✅ Should_Compose_Simple_Base_Name
- ✅ Should_Compose_Name_With_Multiple_Tokens
- ✅ Should_Handle_Missing_Optional_Tokens
- ✅ Should_Select_Random_Pattern_By_Weight
- ✅ Should_Compose_Ability_Name_With_Power_And_School
- ✅ Should_Compose_NPC_Name_With_Title_Prefix_And_Suffix

## Godot Integration

### Accessing Components in GDScript

```gdscript
# Item components
var item_name = item.Name  # "Mithril Longsword of Power [0/1 Sockets]"
var base_name = item.BaseName  # "Longsword"
var material = item.MaterialPrefix  # "Mithril"
var enchants = item.EnchantmentSuffixes  # ["of Power"]
var sockets = item.SocketsText  # "[0/1 Sockets]"

# Display in tooltip
tooltip_label.text = "%s\nMaterial: %s\nEnchantments: %s" % [
    item_name,
    material if material else "None",
    ", ".join(enchants) if enchants.size() > 0 else "None"
]

# Enemy components
var enemy_name = enemy.Name  # "Giant Frost Wolf"
var base = enemy.BaseName  # "Wolf"
var size = enemy.SizePrefix  # "Giant"
var type = enemy.TypePrefix  # "Frost"

# Filter items by material
func get_items_by_material(material_name: String) -> Array:
    return items.filter(func(item): 
        return item.MaterialPrefix == material_name
    )
```

## JSON Standards

All names.json files follow v4.0 standards with pattern-based generation:

```json
{
  "metadata": {
    "version": "4.0",
    "type": "pattern_generation"
  },
  "components": {
    "size": [
      { "value": "Giant", "rarityWeight": 30 },
      { "value": "Tiny", "rarityWeight": 20 }
    ],
    "base": [
      { "value": "Wolf", "rarityWeight": 100 }
    ]
  },
  "patterns": [
    { "pattern": "base", "rarityWeight": 50 },
    { "pattern": "{size} {base}", "rarityWeight": 30 }
  ]
}
```

## Bug Fixes

### Fixed: Enchantment Position Override (January 3, 2026)

**Problem**: Item names like "of the Titan Godforged Morning Star" where suffix enchantments appeared at the start.

**Root Cause**: ItemGenerator was unconditionally overriding enchantment position from slots, even when enchantments had position metadata in names.json.

**Solution**: Only override position for non-enchantment references (materials, etc.):

```csharp
// ItemGenerator.cs - GenerateEnchantmentsAsync()
if (!reference.StartsWith("@items/enchantments", StringComparison.OrdinalIgnoreCase) 
    && !string.IsNullOrEmpty(slotPosition))
{
    enchantment.Position = slotPosition.ToLower() == "prefix" 
        ? EnchantmentPosition.Prefix 
        : EnchantmentPosition.Suffix;
}
```

## Next Steps

### For Enemy, Ability, NPC Generators

To add component population to other generators:

1. **Inject NameComposer** in generator constructor
2. **Load names.json** for the category
3. **Select random pattern** using `GetRandomWeightedPattern()`
4. **Compose name** using `ComposeName()` which returns componentValues dictionary
5. **Populate model properties** from componentValues dictionary
6. **Fallback to catalog name** if names.json doesn't exist

### Example Implementation

```csharp
private async Task<Enemy> ConvertToEnemyAsync(JToken catalogEnemy, string category)
{
    var enemy = new Enemy
    {
        Name = GetStringProperty(catalogEnemy, "name") ?? "Unknown",
        // ... other properties
    };
    
    // Apply pattern-based name if names.json exists
    ApplyNameFromPattern(enemy, category);
    
    return enemy;
}

private void ApplyNameFromPattern(Enemy enemy, string category)
{
    try
    {
        var namesPath = $"enemies/{category}/names.json";
        if (!_dataCache.FileExists(namesPath))
        {
            // Fallback: use catalog name as BaseName
            enemy.BaseName = enemy.Name;
            return;
        }
        
        var namesFile = _dataCache.GetFile(namesPath);
        var patterns = namesFile?.JsonData?["patterns"];
        if (patterns == null) return;
        
        var pattern = _nameComposer.GetRandomWeightedPattern(patterns);
        var patternStr = pattern?["pattern"]?.Value<string>() ?? "base";
        
        var components = namesFile.JsonData["components"];
        if (components == null) return;
        
        var name = _nameComposer.ComposeName(patternStr, components, out var values);
        
        // Populate all component properties
        enemy.Name = name;
        enemy.BaseName = values.GetValueOrDefault("base", enemy.BaseName);
        enemy.SizePrefix = values.GetValueOrDefault("size");
        enemy.TypePrefix = values.GetValueOrDefault("type");
        enemy.DescriptivePrefix = values.GetValueOrDefault("descriptive");
        enemy.TitleSuffix = values.GetValueOrDefault("title");
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Error applying name pattern for {Category}", category);
        // Fallback to catalog name
        enemy.BaseName = enemy.Name;
    }
}
```

## Benefits Summary

✅ **Backward Compatible** - Name property still populated, new properties optional  
✅ **Debuggable** - Can see exact components used during generation  
✅ **Localizable** - Can rebuild names in different orders  
✅ **Filterable** - UI can filter by material, enchantment, title, etc.  
✅ **Extensible** - Easy to add more component types in the future  
✅ **Tested** - 12 comprehensive tests verify functionality  

## Related Files

- **Models**: `RealmEngine.Shared/Models/{Item,Enemy,Ability,NPC}.cs`
- **Utility**: `RealmEngine.Core/Generators/NameComposer.cs`
- **Generator**: `RealmEngine.Core/Generators/Modern/ItemGenerator.cs`
- **Tests**: `RealmEngine.Core.Tests/Generators/{ItemNamingComponentsTests,NameComposerTests}.cs`
- **Standards**: `docs/standards/json/NAMES_JSON_STANDARD.md`
