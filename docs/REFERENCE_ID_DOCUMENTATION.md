# Reference ID Documentation Standard

**Date**: January 1, 2026  
**Purpose**: Comprehensive documentation for all model reference ID properties to support Godot integration

## Overview

All domain models now have **rich XML documentation** on reference ID properties that includes:
- ✅ Clear description of the property's purpose
- ✅ **C# resolution pattern** with complete code example
- ✅ **GDScript/Godot resolution pattern** with complete code example  
- ✅ Explanation of why IDs are used instead of full objects
- ✅ Example ID values in v4.1 reference format

This documentation appears in **IntelliSense/autocomplete** when working with the C# DLLs in Godot.

---

## Updated Models

### 1. CharacterClass
**File**: `RealmEngine.Shared/Models/CharacterClass.cs`

#### Properties Documented:
- `StartingAbilityIds` - Abilities character begins with
- `StartingEquipmentIds` - Equipment character starts with
- `ClassProgression.AbilityUnlocks` - Abilities unlocked per level

#### Breaking Changes:
- ❌ `StartingAbilities` → ✅ `StartingAbilityIds`
- ❌ `StartingEquipment` → ✅ `StartingEquipmentIds`

#### Fixed Equipment References:
All starting equipment now uses proper v4.1 reference format:
```csharp
// ❌ OLD (incorrect):
"Iron Sword", "Wooden Shield"

// ✅ NEW (correct v4.1):
"@items/weapons/swords:longsword", "@items/armor/shields:wooden-shield"
```

---

### 2. Enemy
**File**: `RealmEngine.Shared/Models/Enemy.cs`

#### Properties Documented:
- `AbilityIds` - Combat abilities enemy can use
- `LootTableIds` - Loot tables rolled on death

#### Resolution Use Case:
```csharp
// C#: Load abilities when combat starts
var abilities = await abilityRepository.GetByIdsAsync(enemy.AbilityIds);

// GDScript: Load abilities for AI
for ability_id in enemy.AbilityIds:
    var ability = await ability_service.get_by_id(ability_id)
    enemy.combat_abilities.append(ability)
```

---

### 3. NPC
**File**: `RealmEngine.Shared/Models/NPC.cs`

#### Properties Documented:
- `DialogueIds` - Dialogue lines for conversations
- `AbilityIds` - Abilities if NPC becomes hostile
- `InventoryIds` - Shop inventory or possessions

#### Resolution Use Case:
```csharp
// C#: Load dialogue when player talks to NPC
var dialogueLines = await dialogueRepository.GetByIdsAsync(npc.DialogueIds);

// GDScript: Show shop inventory
for item_id in npc.InventoryIds:
    var item = await item_service.get_by_id(item_id)
    shop_items.append(item)
```

---

### 4. Quest
**File**: `RealmEngine.Shared/Models/Quest.cs`

#### Properties Documented:
- `ItemRewardIds` - Items given on quest completion
- `AbilityRewardIds` - Abilities unlocked on completion
- `ObjectiveLocationIds` - Locations for quest objectives
- `ObjectiveNpcIds` - NPCs involved in quest
- `ObjectiveEnemyIds` - Enemies to defeat for quest

#### Resolution Use Case:
```csharp
// C#: Award rewards on quest completion
var rewardItems = await itemRepository.GetByIdsAsync(quest.ItemRewardIds);
character.Inventory.AddRange(rewardItems);

// GDScript: Complete quest
for item_id in quest.ItemRewardIds:
    var item = await item_service.get_by_id(item_id)
    player.inventory.add_item(item)
```

---

### 5. Item
**File**: `RealmEngine.Shared/Models/Item.cs`

#### Properties Documented:
- `EnchantmentIds` - Enchantments that can be applied
- `MaterialIds` - Materials item can be crafted from
- `RequiredItemIds` - Items needed for crafting/upgrades

#### Resolution Use Case:
```csharp
// C#: Apply material during crafting
var materials = await materialRepository.GetByIdsAsync(item.MaterialIds);
var selectedMaterial = materials.RandomElement();
item.Material = selectedMaterial.Name;

// GDScript: Craft with material
var material_id = item.MaterialIds.pick_random()
var material = await material_service.get_by_id(material_id)
item.apply_material_traits(material.traits)
```

---

### 6. Ability
**File**: `RealmEngine.Shared/Models/Ability.cs`

#### Properties Documented:
- `RequiredItemIds` - Items needed to use ability (spell components)
- `RequiredAbilityIds` - Prerequisite abilities that must be learned first

#### Resolution Use Case:
```csharp
// C#: Check prerequisites before learning
var prerequisites = await abilityRepository.GetByIdsAsync(ability.RequiredAbilityIds);
bool meetsRequirements = prerequisites.All(req => character.HasAbility(req.Id));

// GDScript: Validate prerequisites
for prereq_id in ability.RequiredAbilityIds:
    if not player.has_ability(prereq_id):
        show_error("Must learn prerequisite: " + prereq_id)
```

---

## Benefits for Godot Integration

### 1. **IntelliSense in Godot**
When using C# in Godot, hovering over any reference ID property shows:
- Full description
- Both C# and GDScript resolution examples
- Rationale for the design choice

### 2. **Self-Documenting API**
No need to read external docs - the API explains itself:
```gdscript
# Copilot/IntelliSense will show:
# "Collection of ability IDs this NPC can use (if hostile or in combat).
#  Resolution Pattern (GDScript/Godot):
#  for ability_id in npc.AbilityIds:
#      var ability = await ability_service.get_by_id(ability_id)"
var abilities = npc.AbilityIds
```

### 3. **Prevents Mistakes**
Documentation explains why you can't just use the objects directly:
- Memory efficiency
- Lazy loading patterns
- Circular dependency prevention

### 4. **Cross-Language Examples**
Both C# and GDScript examples ensure developers can work in either language.

---

## v4.1 Reference Format

All item/ability/enemy references now follow the standard:

```
@domain/category/subcategory:item-name
```

### Examples:
- `@items/weapons/swords:longsword`
- `@items/armor/shields:wooden-shield`
- `@abilities/offensive:fireball`
- `@enemies/beasts:dire-wolf`
- `@npcs/merchants:blacksmith`

### Where Used:
- CharacterClass starting equipment
- Quest rewards
- NPC inventories
- Crafting recipes
- Loot tables

---

## Testing

✅ **Build Status**: All changes compile successfully  
✅ **XML Generation**: All properties have rich documentation in XML  
✅ **IntelliSense Ready**: Documentation available via XML files  

### Verify in Godot:
1. Copy `RealmEngine.Shared.dll` and `RealmEngine.Shared.xml` to Godot project
2. Reference the DLL in your C# scripts
3. Type `characterClass.` and see autocomplete with full documentation
4. Hover over any property to see resolution patterns

---

## Migration Guide

### For Existing Code:

#### 1. Update Property Names (Breaking)
```csharp
// OLD:
var abilities = characterClass.StartingAbilities;
var equipment = characterClass.StartingEquipment;

// NEW:
var abilityIds = characterClass.StartingAbilityIds;
var equipmentIds = characterClass.StartingEquipmentIds;
```

#### 2. Update Mock Data (Fixed)
All mock character classes now use proper v4.1 references:
```csharp
// File: RealmEngine.Data/Repositories/CharacterClassRepository.cs
StartingEquipmentIds = new List<string>
{
    "@items/weapons/swords:longsword",      // ✅ Proper reference
    "@items/armor/shields:wooden-shield"    // ✅ Proper reference
}
```

#### 3. Resolution Pattern (No Change)
The way you resolve IDs remains the same:
```csharp
// Still works as before:
var items = await itemRepository.GetByIdsAsync(characterClass.StartingEquipmentIds);
```

---

## Future Enhancements

### Potential Additions:
1. **Extension Methods**: Helper methods for common resolution patterns
   ```csharp
   var abilities = await characterClass.ResolveStartingAbilitiesAsync(_abilityRepo);
   ```

2. **Validation Attributes**: Ensure IDs are valid references
   ```csharp
   [ValidReference("@items/*")]
   public List<string> StartingEquipmentIds { get; set; }
   ```

3. **Code Generation**: Auto-generate GDScript service wrappers
   ```gdscript
   # Auto-generated from C# XML docs
   class CharacterClassService extends Node:
       func resolve_starting_abilities(character_class):
           # Implementation from XML example
   ```

---

## Summary

✅ **6 models updated** with comprehensive documentation  
✅ **15+ reference ID properties** now self-documenting  
✅ **Dual examples** for both C# and GDScript  
✅ **v4.1 compliance** for all equipment references  
✅ **Zero compilation errors**  
✅ **IntelliSense ready** for Godot integration  

**Godot developers now have complete context** on how to use reference IDs without needing external documentation!
