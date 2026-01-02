# Godot Reference Resolution Guide

**For Godot C# Developers**: How to resolve v4.1 JSON reference IDs to actual game objects

## ⚠️ CRITICAL: Use the Right Service!

**DON'T use fictional services** like:
- ❌ `abilityRepository.GetByIdsAsync()` - **DOES NOT EXIST**
- ❌ `itemRepository.GetByIdsAsync()` - **DOES NOT EXIST**
- ❌ `ability_service.get_by_id()` - **DOES NOT EXIST**

**DO use the ACTUAL service:**
- ✅ `ReferenceResolverService` with `ResolveToObjectAsync(string reference)` - **THIS EXISTS!**

---

## The Actual Service: ReferenceResolverService

### Location
- **Assembly**: `RealmEngine.Data.dll`
- **Namespace**: `RealmEngine.Data.Services`
- **Class**: `ReferenceResolverService`

### Constructor
```csharp
var resolver = new ReferenceResolverService(dataCache);
```

### Key Method
```csharp
Task<JToken?> ResolveToObjectAsync(string reference)
```

**Parameters:**
- `reference` - Full v4.1 reference string (e.g., `"@abilities/active/offensive:basic-attack"`)

**Returns:**
- `JToken?` - Newtonsoft.Json token that can be deserialized to any model type
- Returns `null` if reference is invalid or optional (`?` suffix)

---

## Reference ID Format (v4.1)

All reference IDs in model properties follow this format:

```
@domain/category/subcategory:item-name[filters]?.property.nested
```

### Common Reference Patterns

| Domain | Example Reference | Resolves To |
|--------|-------------------|-------------|
| **Abilities** | `@abilities/active/offensive:basic-attack` | Ability object |
| **Items** | `@items/weapons/swords:longsword` | Item object |
| **Enemies** | `@enemies/goblinoids:goblin-warrior` | Enemy object |
| **NPCs** | `@npcs/merchants:blacksmith` | NPC object |
| **Locations** | `@world/locations/dungeons:dark-cavern` | Location object |
| **Quests** | `@quests/main-story:chapter-1` | Quest object |

### Wildcard Support
Use `:*` for random item from category:
- `@items/weapons/swords:*` - Random sword
- `@items/consumables/potions:*` - Random potion

---

## Step-by-Step Resolution Process

### C# Example (Godot)

```csharp
using RealmEngine.Data.Services;
using RealmEngine.Shared.Models;
using Newtonsoft.Json.Linq;

// Step 1: Initialize resolver with data cache
var resolver = new ReferenceResolverService(dataCache);

// Step 2: Get reference ID from model property
string abilityRefId = characterClass.StartingAbilityIds[0]; 
// e.g., "@abilities/active/offensive:basic-attack"

// Step 3: Resolve reference to JSON token
JToken? abilityJson = await resolver.ResolveToObjectAsync(abilityRefId);

// Step 4: Convert JSON token to typed object
Ability ability = abilityJson.ToObject<Ability>();

// Step 5: Use the resolved object
GD.Print($"Ability Name: {ability.DisplayName}");
GD.Print($"Mana Cost: {ability.ManaCost}");
```

### GDScript Example (Pseudo-code)

```gdscript
# Step 1: Initialize resolver
var resolver = ReferenceResolverService.new(data_cache)

# Step 2: Get reference ID
var ability_ref_id = character_class.StartingAbilityIds[0]

# Step 3: Resolve reference
var ability_data = await resolver.ResolveToObjectAsync(ability_ref_id)

# Step 4: Use the resolved data (already a dictionary in GDScript)
print("Ability Name: " + ability_data.DisplayName)
print("Mana Cost: " + str(ability_data.ManaCost))
```

---

## Complete Examples by Model Type

### 1. CharacterClass - Starting Abilities

```csharp
// CharacterClass has StartingAbilityIds: List<string>
var resolver = new ReferenceResolverService(dataCache);
var startingAbilities = new List<Ability>();

foreach (var refId in characterClass.StartingAbilityIds)
{
    // e.g., "@abilities/active/offensive:basic-attack"
    var abilityJson = await resolver.ResolveToObjectAsync(refId);
    var ability = abilityJson.ToObject<Ability>();
    startingAbilities.Add(ability);
}

// Now use the abilities
character.LearnedSkills.AddRange(startingAbilities);
```

### 2. CharacterClass - Starting Equipment

```csharp
// CharacterClass has StartingEquipmentIds: List<string>
var resolver = new ReferenceResolverService(dataCache);
var startingEquipment = new List<Item>();

foreach (var refId in characterClass.StartingEquipmentIds)
{
    // e.g., "@items/weapons/swords:longsword"
    var itemJson = await resolver.ResolveToObjectAsync(refId);
    var item = itemJson.ToObject<Item>();
    startingEquipment.Add(item);
}

// Equip items
character.Inventory.AddRange(startingEquipment);
```

### 3. Enemy - Abilities and Loot

```csharp
// Enemy has AbilityIds: List<string>
var resolver = new ReferenceResolverService(dataCache);

// Resolve enemy abilities
var enemyAbilities = new List<Ability>();
foreach (var refId in enemy.AbilityIds)
{
    // e.g., "@abilities/active/offensive:fire-breath"
    var abilityJson = await resolver.ResolveToObjectAsync(refId);
    var ability = abilityJson.ToObject<Ability>();
    enemyAbilities.Add(ability);
}

// Resolve loot table (supports wildcards)
var loot = new List<Item>();
foreach (var refId in enemy.LootTableIds)
{
    // e.g., "@items/weapons/swords:*" (wildcard = random)
    var itemJson = await resolver.ResolveToObjectAsync(refId);
    var item = itemJson.ToObject<Item>();
    if (RollForLoot()) loot.Add(item);
}
```

### 4. NPC - Dialogue and Inventory

```csharp
// NPC has DialogueIds: List<string>
var resolver = new ReferenceResolverService(dataCache);

// Resolve dialogue options
var dialogues = new List<DialogueLine>();
foreach (var refId in npc.DialogueIds)
{
    // e.g., "@npcs/dialogue/greetings:friendly"
    var dialogueJson = await resolver.ResolveToObjectAsync(refId);
    var dialogue = dialogueJson.ToObject<DialogueLine>();
    dialogues.Add(dialogue);
}

// Resolve merchant inventory
var shopItems = new List<Item>();
foreach (var refId in npc.InventoryIds)
{
    // e.g., "@items/consumables/potions:health-potion"
    var itemJson = await resolver.ResolveToObjectAsync(refId);
    var item = itemJson.ToObject<Item>();
    shopItems.Add(item);
}
```

### 5. Quest - Rewards and Objectives

```csharp
// Quest has ItemRewardIds: List<string>
var resolver = new ReferenceResolverService(dataCache);

// Resolve quest rewards
var rewardItems = new List<Item>();
foreach (var refId in quest.ItemRewardIds)
{
    // e.g., "@items/weapons/swords:magic-longsword"
    var itemJson = await resolver.ResolveToObjectAsync(refId);
    var item = itemJson.ToObject<Item>();
    rewardItems.Add(item);
}

var rewardAbilities = new List<Ability>();
foreach (var refId in quest.AbilityRewardIds)
{
    // e.g., "@abilities/active/offensive:power-strike"
    var abilityJson = await resolver.ResolveToObjectAsync(refId);
    var ability = abilityJson.ToObject<Ability>();
    rewardAbilities.Add(ability);
}

// Award to player when quest completes
character.Inventory.AddRange(rewardItems);
character.LearnedSkills.AddRange(rewardAbilities);
```

### 6. Item - Hybrid Pattern (Enchantments & Materials)

⚠️ **Special Case**: Item uses a **hybrid pattern** where:
- **EnchantmentIds** = Template references from catalog
- **Enchantments** = Resolved enhancement objects (already baked in)
- **MaterialIds** = Template references from catalog
- **Material** = Resolved material name string (already baked in)

```csharp
// During ITEM GENERATION (not at runtime):
var resolver = new ReferenceResolverService(dataCache);

// Apply enchantments from template
var enchantments = new List<ItemEnhancement>();
foreach (var refId in item.EnchantmentIds)
{
    // e.g., "@items/enchantments/elemental:fire"
    var enchantJson = await resolver.ResolveToObjectAsync(refId);
    var enchantment = enchantJson.ToObject<ItemEnhancement>();
    enchantments.Add(enchantment);
}
item.Enchantments = enchantments; // Store resolved enchantments

// Apply material from template
if (item.MaterialIds.Any())
{
    var randomMaterialRefId = item.MaterialIds.PickRandom();
    var materialJson = await resolver.ResolveToObjectAsync(randomMaterialRefId);
    var material = materialJson.ToObject<Material>();
    item.Material = material.Name; // Store resolved name string
    item.Name = $"{material.Name} {item.BaseName}";
}

// AT RUNTIME: Use the RESOLVED properties, not the IDs!
// ✅ Use: item.Enchantments (List<ItemEnhancement>)
// ✅ Use: item.Material (string)
// ❌ Don't re-resolve: item.EnchantmentIds or item.MaterialIds
```

### 7. Ability - Prerequisites

```csharp
// Ability has RequiredItemIds: List<string>
var resolver = new ReferenceResolverService(dataCache);

// Check if player has required items
var requiredItems = new List<Item>();
foreach (var refId in ability.RequiredItemIds)
{
    // e.g., "@items/consumables/reagents:mana-crystal"
    var itemJson = await resolver.ResolveToObjectAsync(refId);
    var item = itemJson.ToObject<Item>();
    requiredItems.Add(item);
}

bool canUse = requiredItems.All(item => 
    character.Inventory.Contains(item.Name));

// Check ability prerequisites
var prerequisites = new List<Ability>();
foreach (var refId in ability.RequiredAbilityIds)
{
    // e.g., "@abilities/active/offensive:basic-attack"
    var abilityJson = await resolver.ResolveToObjectAsync(refId);
    var prereq = abilityJson.ToObject<Ability>();
    prerequisites.Add(prereq);
}

bool meetsRequirements = prerequisites.All(req =>
    character.LearnedSkills.Any(s => s.Name == req.Name));
```

---

## Error Handling

### Null Checks
Always check for null when resolving:

```csharp
var itemJson = await resolver.ResolveToObjectAsync(refId);
if (itemJson == null)
{
    GD.PrintErr($"Failed to resolve reference: {refId}");
    continue;
}

var item = itemJson.ToObject<Item>();
```

### Optional References
References ending with `?` return `null` instead of error:

```csharp
// Optional reference: "@items/weapons/swords:legendary-sword?"
var itemJson = await resolver.ResolveToObjectAsync(optionalRefId);
if (itemJson != null)
{
    // Item exists, use it
}
else
{
    // Item doesn't exist, that's OK (optional)
}
```

---

## Performance Tips

### 1. Batch Resolution
If resolving many references, collect them first:

```csharp
var resolver = new ReferenceResolverService(dataCache);
var tasks = new List<Task<JToken?>>();

foreach (var refId in character.StartingAbilityIds)
{
    tasks.Add(resolver.ResolveToObjectAsync(refId));
}

var results = await Task.WhenAll(tasks);
var abilities = results
    .Where(json => json != null)
    .Select(json => json.ToObject<Ability>())
    .ToList();
```

### 2. Cache Resolved Objects
Don't re-resolve the same reference multiple times:

```csharp
private Dictionary<string, Ability> _abilityCache = new();

public async Task<Ability> GetAbilityAsync(string refId)
{
    if (_abilityCache.TryGetValue(refId, out var cached))
        return cached;
    
    var json = await resolver.ResolveToObjectAsync(refId);
    var ability = json.ToObject<Ability>();
    _abilityCache[refId] = ability;
    return ability;
}
```

### 3. Lazy Loading
Only resolve when actually needed:

```csharp
// DON'T resolve all at startup:
// foreach (var refId in npc.DialogueIds)
//     await resolver.ResolveToObjectAsync(refId);

// DO resolve when player interacts:
public async Task OnPlayerInteract()
{
    // Resolve dialogues only when player talks to NPC
    foreach (var refId in npc.DialogueIds)
    {
        var dialogue = await GetDialogueAsync(refId);
        ShowDialogueOption(dialogue);
    }
}
```

---

## Common Mistakes ❌

### ❌ Mistake #1: Trying to Use Repository Pattern
```csharp
// THIS DOES NOT WORK:
var abilities = await abilityRepository.GetByIdsAsync(ids);
// abilityRepository does not exist!
```

**✅ Solution:**
```csharp
// Use ReferenceResolverService:
var resolver = new ReferenceResolverService(dataCache);
foreach (var refId in ids)
{
    var json = await resolver.ResolveToObjectAsync(refId);
    var ability = json.ToObject<Ability>();
}
```

### ❌ Mistake #2: Treating IDs as Simple Strings
```csharp
// THIS DOES NOT WORK:
var ability = abilities.FirstOrDefault(a => a.Name == "basic-attack");
// IDs are full references, not simple names!
```

**✅ Solution:**
```csharp
// IDs are references:
string refId = "@abilities/active/offensive:basic-attack";
var json = await resolver.ResolveToObjectAsync(refId);
var ability = json.ToObject<Ability>();
```

### ❌ Mistake #3: Re-resolving Hybrid Properties
```csharp
// THIS IS WASTEFUL:
foreach (var refId in item.EnchantmentIds)
{
    // Don't re-resolve at runtime!
}
```

**✅ Solution:**
```csharp
// Use already-resolved properties:
foreach (var enchantment in item.Enchantments)
{
    // These are already resolved objects
    ApplyEnchantment(enchantment);
}
```

### ❌ Mistake #4: Not Handling Wildcards
```csharp
// THIS IGNORES WILDCARDS:
if (refId.Contains("*"))
{
    // Don't skip wildcard references!
}
```

**✅ Solution:**
```csharp
// ReferenceResolverService handles wildcards automatically:
var json = await resolver.ResolveToObjectAsync("@items/weapons/swords:*");
// Returns a random sword from the category
```

---

## Summary: The Only Pattern You Need

```csharp
// 1. Create resolver
var resolver = new ReferenceResolverService(dataCache);

// 2. Loop through reference IDs
foreach (var refId in model.SomeReferenceIds)
{
    // 3. Resolve to JSON
    var json = await resolver.ResolveToObjectAsync(refId);
    
    // 4. Null check
    if (json == null) continue;
    
    // 5. Convert to typed object
    var obj = json.ToObject<TheModelType>();
    
    // 6. Use the object
    DoSomethingWith(obj);
}
```

---

## When to Resolve References

| Model Type | Property | When to Resolve |
|------------|----------|-----------------|
| **CharacterClass** | StartingAbilityIds | Character creation |
| **CharacterClass** | StartingEquipmentIds | Character creation |
| **Enemy** | AbilityIds | Enemy spawns in combat |
| **Enemy** | LootTableIds | Enemy dies (loot roll) |
| **NPC** | DialogueIds | Player talks to NPC |
| **NPC** | AbilityIds | NPC becomes hostile |
| **NPC** | InventoryIds | Player opens shop |
| **Quest** | ItemRewardIds | Quest completes |
| **Quest** | AbilityRewardIds | Quest completes |
| **Quest** | ObjectiveLocationIds | Quest accepted |
| **Quest** | ObjectiveNpcIds | Quest accepted |
| **Quest** | ObjectiveEnemyIds | Never (compare IDs directly) |
| **Item** | EnchantmentIds | Item generation only |
| **Item** | MaterialIds | Item generation only |
| **Item** | RequiredItemIds | Crafting UI opened |
| **Ability** | RequiredItemIds | Ability use attempt |
| **Ability** | RequiredAbilityIds | Learn ability attempt |

---

## Need More Help?

1. **IntelliSense Documentation**: Hover over any `*Ids` property in Godot C# for full XML docs
2. **Service Source Code**: See `RealmEngine.Data/Services/ReferenceResolverService.cs`
3. **Model Source Code**: See `RealmEngine.Shared/Models/*.cs` for all models
4. **JSON Standards**: See `docs/standards/json/JSON_REFERENCE_STANDARDS.md`

---

**Last Updated**: January 1, 2026  
**Version**: v4.1 (JSON Reference System)
