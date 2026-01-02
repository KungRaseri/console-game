# Model Reference Audit - Object vs String ID Analysis

**Date**: January 1, 2026  
**Purpose**: Analyze all domain models to determine if they should use object references instead of string IDs

## Executive Summary

✅ **Current Pattern is CORRECT** - Models use string IDs as intended  
✅ **Character model uses proper object references** for equipped items and inventory  
✅ **String IDs are appropriate** for lazy-loading and cross-domain references

## Analysis by Model

### ✅ Character Model - **CORRECT PATTERN**
```csharp
public List<Item> Inventory { get; set; } = new();
public Item? EquippedMainHand { get; set; }
public Item? EquippedHelmet { get; set; }
```

**Why this works**: 
- Character directly owns and manages items
- Items are in-memory during gameplay
- Proper object references for immediate use

---

### ✅ CharacterClass Model - **CORRECT: Keep String IDs**

#### Current Structure
```csharp
public List<string> StartingAbilities { get; set; } = new();
public List<string> StartingEquipment { get; set; } = new();
```

#### ❌ Should NOT Change To:
```csharp
public List<Ability> StartingAbilities { get; set; } = new();
public List<Item> StartingEquipment { get; set; } = new();
```

#### ✅ Reasoning:
1. **Cross-domain references**: Classes reference abilities and items from other catalogs
2. **Lazy loading**: Starting abilities don't need to be loaded until character creation
3. **Circular dependencies**: Would create Item → Enchantment → Ability circular refs
4. **Memory efficiency**: Templates shouldn't hold full object graphs
5. **Generation pattern**: IDs are resolved at runtime via `@abilities/...` references

**Resolution Pattern in Code**:
```csharp
// CharacterClassGenerator.cs line 138
StartingAbilities = ParseAbilityReferences(data.StartingAbilities),

// At character creation time:
var abilities = await abilityRepository.GetByIdsAsync(characterClass.StartingAbilities);
```

---

### ✅ Enemy Model - **CORRECT: Keep String IDs**

#### Current Structure
```csharp
public List<string> AbilityIds { get; set; } = new();
public List<string> LootTableIds { get; set; } = new();
```

#### ✅ Reasoning:
1. **Template model**: Enemy is a template, not a live instance
2. **Combat instantiation**: Abilities loaded when combat starts
3. **Procedural generation**: Loot tables resolved at death time
4. **Memory efficiency**: 1000s of enemy templates shouldn't hold full objects

**Usage Pattern**:
```csharp
// EnemyGenerator.cs line 193
enemy.AbilityIds = resolvedAbilities;

// In combat system:
var enemyAbilities = await abilityRepository.GetByIdsAsync(enemy.AbilityIds);
```

---

### ✅ NPC Model - **CORRECT: Keep String IDs**

#### Current Structure
```csharp
public List<string> DialogueIds { get; set; } = new();
public List<string> AbilityIds { get; set; } = new();
public List<string> InventoryIds { get; set; } = new();
```

#### ✅ Reasoning:
1. **Lazy dialogue loading**: Only load dialogue when player interacts
2. **Shop inventory**: Resolved when shop UI opens
3. **Quest NPCs**: Abilities loaded if NPC becomes hostile
4. **Memory optimization**: NPCs shouldn't hold full inventory objects

**Usage Pattern**:
```csharp
// NPCGenerator.cs line 162-168
npc.DialogueIds = await ResolveReferencesAsync(dialogues);
npc.AbilityIds = await ResolveReferencesAsync(abilities);

// When player talks to NPC:
var dialogueLines = await dialogueRepository.GetByIdsAsync(npc.DialogueIds);
```

---

### ✅ Quest Model - **CORRECT: Keep String IDs**

#### Current Structure
```csharp
public List<string> ItemRewardIds { get; set; } = new();
public List<string> AbilityRewardIds { get; set; } = new();
public List<string> ObjectiveLocationIds { get; set; } = new();
public List<string> ObjectiveNpcIds { get; set; } = new();
public List<string> ObjectiveEnemyIds { get; set; } = new();
```

#### ✅ Reasoning:
1. **Lazy reward resolution**: Items created when quest completes
2. **Objective tracking**: Only load referenced data when quest active
3. **Cross-domain references**: Quests reference enemies, NPCs, locations, items
4. **Save file size**: Quest in save file shouldn't duplicate full objects

**Usage Pattern**:
```csharp
// QuestGenerator.cs line 157
quest.ItemRewardIds = await ResolveReferencesAsync(itemRewards);

// CompleteQuestCommand.cs line 29
var rewardItems = await itemRepository.GetByIdsAsync(result.Quest.ItemRewardIds);
character.Inventory.AddRange(rewardItems);
```

---

### ✅ Item Model - **CORRECT: Keep String IDs**

#### Current Structure
```csharp
public List<string> EnchantmentIds { get; set; } = new();
public List<string> MaterialIds { get; set; } = new();
public List<string> RequiredItemIds { get; set; } = new();
```

#### ✅ Reasoning:
1. **Template references**: Points to enchantment/material catalogs
2. **Generation-time resolution**: Enchantments applied when item created
3. **Crafting recipes**: Required items resolved at craft time
4. **Avoids circular refs**: Material → Item → Material would be circular

**Hybrid Pattern**:
```csharp
// Generation time (ItemGenerator):
item.EnchantmentIds = ["fire-enchantment", "strength-boost"];
var enchantments = await ResolveEnchantments(item.EnchantmentIds);
item.Enchantments = enchantments; // Baked into item

// Result: Both ID list AND resolved objects
```

---

### ✅ Ability Model - **CORRECT: Keep String IDs**

#### Current Structure
```csharp
public List<string> RequiredAbilityIds { get; set; } = new();
public List<string> RequiredItemIds { get; set; } = new();
```

#### ✅ Reasoning:
1. **Prerequisite chains**: Abilities reference other abilities
2. **Item requirements**: Spellbooks, focuses, etc.
3. **Prevents circular deps**: Ability → Item → Ability would break
4. **Validation at learn time**: Check prerequisites when character learns

**Usage Pattern**:
```csharp
// AbilityGenerator.cs line 177
ability.RequiredAbilityIds = await ResolveReferencesAsync(requiredAbilities);

// When learning ability:
var prerequisites = await abilityRepository.GetByIdsAsync(ability.RequiredAbilityIds);
if (!character.HasAbilities(prerequisites))
    throw new InvalidOperationException("Prerequisites not met");
```

---

### ✅ Location Model - **CORRECT: Keep String IDs**

#### Current Structure
```csharp
public List<string> Features { get; set; } = new();
public List<string> Npcs { get; set; } = new();
public List<string> Enemies { get; set; } = new();
public List<string> Loot { get; set; } = new();
```

#### ✅ Reasoning:
1. **Area templates**: Location is a template for map generation
2. **Dynamic spawning**: NPCs/enemies spawn when player enters
3. **Procedural loot**: Generated on encounter
4. **Memory**: World shouldn't hold 1000s of NPC/Enemy objects

---

### ✅ Organization Model - **CORRECT: Keep String IDs**

#### Current Structure
```csharp
public List<string> Members { get; set; } = new();
public List<string> Services { get; set; } = new();
public List<string> Inventory { get; set; } = new();
```

#### ✅ Reasoning:
1. **Guild rosters**: Members resolved when viewing guild UI
2. **Shop inventory**: Loaded when player visits shop
3. **Dynamic membership**: Players can join/leave

---

## Design Pattern Summary

### When to Use Object References ✅
1. **Character equipment** - Direct gameplay use
2. **Character inventory** - Immediate access needed
3. **Active combat participants** - Live instances
4. **Player-managed collections** - Quest log, skill list

### When to Use String IDs ✅
1. **Cross-domain references** - Abilities → Items, Quests → Enemies
2. **Template models** - Classes, Enemies, NPCs, Locations
3. **Lazy-loaded collections** - Dialogue, shop inventory, loot tables
4. **Save file optimization** - Avoid duplicating full objects
5. **Circular dependency prevention** - Material ↔ Item, Ability ↔ Item

---

## Architecture Benefits

### Current Pattern (String IDs)
✅ **Lazy loading** - Only load what's needed  
✅ **Memory efficient** - Templates don't duplicate full objects  
✅ **Save file size** - Store IDs, not full graphs  
✅ **Flexible resolution** - Can swap implementations  
✅ **Prevents circular deps** - Clean separation  
✅ **Repository pattern** - Services resolve IDs to objects  

### If Changed to Object References ❌
❌ **Eager loading required** - All refs loaded upfront  
❌ **Memory bloat** - Every template holds full objects  
❌ **Circular dependencies** - Item → Enchantment → Ability → Item  
❌ **Save file explosion** - Full object graphs serialized  
❌ **Tight coupling** - Models depend on each other  
❌ **Initialization order problems** - Must load dependencies first  

---

## Resolution Pattern Example

### Correct Current Implementation

```csharp
// 1. JSON with @references
{
  "name": "Warrior",
  "startingAbilities": "@abilities/offensive:basic-attack @abilities/defensive:block"
}

// 2. Data model (for deserialization)
public class CharacterClassData
{
    public List<string>? StartingAbilities { get; set; }
}

// 3. Domain model (in-memory template)
public class CharacterClass
{
    public List<string> StartingAbilities { get; set; } = new(); // IDs only
}

// 4. Generator resolves references
var abilityIds = ParseAbilityReferences(data.StartingAbilities);
characterClass.StartingAbilities = abilityIds; // ["basic-attack", "block"]

// 5. Service layer resolves at use-time
var abilities = await _abilityRepo.GetByIdsAsync(characterClass.StartingAbilities);
character.LearnedSkills.AddRange(abilities);
```

### ❌ Anti-Pattern (What NOT to Do)

```csharp
// DON'T: Load full objects into templates
public class CharacterClass
{
    public List<Ability> StartingAbilities { get; set; } = new(); // ❌ Memory bloat
}

// DON'T: Eager load all dependencies
var warrior = await classRepo.GetByIdAsync("warrior");
// ^ Would need to load 20+ abilities, 50+ items, all enchantments, materials...
```

---

## Conclusion

✅ **No changes needed** - Current architecture is correct

The pattern of using **string IDs for references** and **object references for owned collections** follows best practices for:
- Entity relationship modeling
- Lazy loading optimization
- Memory management
- Save file efficiency
- Preventing circular dependencies

**Only exception**: `Character` model correctly uses `List<Item>` for inventory and `Item?` for equipment because the character **owns** these objects during gameplay.

---

## When to Revisit

Consider changing to object references ONLY if:
1. **Performance profiling** shows excessive repository lookups
2. **Gameplay code** frequently needs the same referenced objects
3. **Caching layer** is added that makes eager loading free

Current metrics suggest string IDs are optimal for this game's architecture.
