# Godot Reference Resolution Guide

**For Godot C# Developers**: How to work with fully hydrated game objects

## ✅ NEW: Fully Hydrated Objects (Recommended)

**As of v1.0**, all generators now return **fully hydrated objects by default**. You no longer need to manually resolve references!

### The Easy Way (Recommended)

```csharp
// Generators return FULLY HYDRATED objects - everything is already resolved!
var enemies = await enemyGenerator.GenerateEnemiesAsync("goblinoids", count: 3);

foreach (var enemy in enemies)
{
    // ✅ Use resolved properties directly - no ResolveToObjectAsync needed!
    GD.Print($"Enemy: {enemy.Name}");
    
    // Abilities are already resolved
    foreach (var ability in enemy.Abilities)
    {
        GD.Print($"  - Ability: {ability.DisplayName} (Cost: {ability.ManaCost})");
    }
    
    // Loot table is already resolved
    foreach (var item in enemy.LootTable)
    {
        GD.Print($"  - Loot: {item.Name} ({item.Price} gold)");
    }
}
```

### Why This Is Better

| Old Way (Manual Resolution) | New Way (Fully Hydrated) |
|------------------------------|---------------------------|
| ❌ 10+ lines of boilerplate code | ✅ 1 line - generator call |
| ❌ Easy to forget to resolve | ✅ Automatic - can't forget |
| ❌ Need to understand ReferenceResolverService | ✅ Just use the properties |
| ❌ Risk of null reference errors | ✅ Properties are populated |
| ❌ Have to loop through IDs manually | ✅ Already looped for you |

---

## ⚡ Performance Mode: Template-Only Generation

If you need **maximum performance** or are **saving to disk**, you can disable hydration:

```csharp
// Generate template only (IDs not resolved to objects)
var enemies = await enemyGenerator.GenerateEnemiesAsync("goblinoids", count: 3, hydrate: false);

// enemy.Abilities will be null
// enemy.AbilityIds will contain reference IDs like "@abilities/active/offensive:bite"

// Good for:
// - Serializing to JSON (save files)
// - Lazy loading (resolve later when needed)
// - Network transmission (smaller payload)
```

Then manually resolve when needed:
```csharp
var resolver = new ReferenceResolverService(dataCache);
var abilities = new List<Ability>();

foreach (var refId in enemy.AbilityIds)
{
    var json = await resolver.ResolveToObjectAsync(refId);
    if (json != null)
    {
        abilities.Add(json.ToObject<Ability>());
    }
}
```

---

## 📖 Complete Usage Examples

### 1. Character Creation

```csharp
// Get character class with starting abilities and equipment already resolved
var characterClass = await classGenerator.GetClassByNameAsync("fighter");

// Everything is ready to use!
var newCharacter = new Character
{
    Name = "Conan",
    Class = characterClass.Name,
    Level = 1
};

// Grant starting abilities (already resolved objects)
foreach (var ability in characterClass.StartingAbilities)
{
    newCharacter.LearnedSkills.Add(ability);
    GD.Print($"Learned: {ability.DisplayName}");
}

// Grant starting equipment (already resolved objects)
foreach (var item in characterClass.StartingEquipment)
{
    newCharacter.Inventory.Add(item);
    GD.Print($"Equipped: {item.Name}");
}
```

### 2. Combat Encounter

```csharp
// Spawn enemies with abilities already resolved
var enemies = await enemyGenerator.GenerateEnemiesAsync("undead", count: 5);

foreach (var enemy in enemies)
{
    // Spawn enemy in world
    SpawnEnemy(enemy);
    
    // AI can use abilities immediately - no resolution needed
    enemy.AI = new CombatAI(enemy.Abilities);
}

// When enemy dies
void OnEnemyDeath(Enemy enemy)
{
    // Loot table already resolved
    foreach (var item in enemy.LootTable)
    {
        if (RollForLoot())
        {
            player.Inventory.Add(item);
            ShowLootNotification(item.Name);
        }
    }
}
```

### 3. NPC Dialogue

```csharp
// Generate NPC with dialogue, abilities, and inventory already resolved
var merchant = await npcGenerator.GenerateNpcAsync("merchants");

// Show dialogue options immediately
void OnPlayerTalkToMerchant()
{
    foreach (var dialogue in merchant.Dialogues)
    {
        if (dialogue.Type == "greeting")
        {
            ShowDialogue(dialogue.Text);
            break;
        }
    }
}

// Open shop with inventory already resolved
void OnPlayerOpenShop()
{
    foreach (var item in merchant.Inventory)
    {
        AddToShopUI(item.Name, item.Price, item.Description);
    }
}
```

### 4. Quest System

```csharp
// Generate quest with all objectives and rewards resolved
var quest = await questGenerator.GenerateQuestAsync("fetch");

// Display quest info
GD.Print($"Quest: {quest.Name}");
GD.Print($"Description: {quest.Description}");

// Mark objective locations on map
foreach (var location in quest.ObjectiveLocations)
{
    AddQuestMarkerToMap(location.Name, location.Coordinates);
}

// Mark objective NPCs
foreach (var npc in quest.ObjectiveNpcs)
{
    npc.ShowQuestMarker = true;
}

// When quest completes
void OnQuestComplete(Quest quest)
{
    // Award items (already resolved)
    foreach (var item in quest.ItemRewards)
    {
        player.Inventory.Add(item);
    }
    
    // Grant abilities (already resolved)
    foreach (var ability in quest.AbilityRewards)
    {
        player.LearnedSkills.Add(ability);
        ShowAbilityUnlockedNotification(ability.DisplayName);
    }
}
```

### 5. Crafting System

```csharp
// Generate craftable item with requirements already resolved
var item = await itemGenerator.GenerateItemAsync("weapons", "legendary-sword");

// Check if player can craft
bool CanCraftItem(Item item)
{
    foreach (var requiredItem in item.RequiredItems)
    {
        if (!player.Inventory.Contains(requiredItem.Name))
        {
            ShowMissingMaterialNotification(requiredItem.Name);
            return false;
        }
    }
    return true;
}

// Craft item
void CraftItem(Item item)
{
    foreach (var requiredItem in item.RequiredItems)
    {
        player.Inventory.Remove(requiredItem.Name);
    }
    player.Inventory.Add(item);
}
```

### 6. Ability Prerequisites

```csharp
// Generate ability with prerequisites already resolved
var ability = await abilityGenerator.GenerateAbilityAsync("offensive", "power-strike");

// Check if player can learn
bool CanLearnAbility(Ability ability)
{
    // Check required items
    foreach (var requiredItem in ability.RequiredItems)
    {
        if (!player.Inventory.Contains(requiredItem.Name))
        {
            return false;
        }
    }
    
    // Check prerequisite abilities
    foreach (var prereq in ability.RequiredAbilities)
    {
        if (!player.LearnedSkills.Any(s => s.Name == prereq.Name))
        {
            ShowPrerequisiteNotification(prereq.DisplayName);
            return false;
        }
    }
    
    return true;
}
```

---

## 🎯 When to Use Each Approach

### ✅ Use Fully Hydrated (hydrate: true - default)

- **Game runtime** - Playing the game
- **UI display** - Showing item details, quest info, enemy stats
- **Combat system** - Executing abilities, rolling loot
- **NPC interactions** - Dialogue, trading, shops
- **Quest system** - Tracking objectives, awarding rewards
- **Any time you need the actual object data**

### ⚡ Use Template-Only (hydrate: false)

- **Saving games** - Serializing to JSON (smaller file size)
- **Network transmission** - Sending data over network
- **Database storage** - Storing in save files
- **Lazy loading** - Will resolve later when actually needed
- **Performance critical** - Generating 1000s of items at once

---

## 🔧 All Supported Generators

| Generator | Method | Resolved Properties |
|-----------|--------|---------------------|
| **EnemyGenerator** | `GenerateEnemiesAsync(category, count, hydrate)` | `Abilities`, `LootTable` |
| **CharacterClassGenerator** | `GetClassByNameAsync(name, hydrate)` | `StartingAbilities`, `StartingEquipment` |
| **NpcGenerator** | `GenerateNpcAsync(category, hydrate)` | `Dialogues`, `Abilities`, `Inventory` |
| **QuestGenerator** | `GenerateQuestAsync(type, hydrate)` | `ItemRewards`, `AbilityRewards`, `ObjectiveLocations`, `ObjectiveNpcs` |
| **ItemGenerator** | `GenerateItemAsync(category, name, hydrate)` | `RequiredItems` |
| **AbilityGenerator** | `GenerateAbilityAsync(type, name, hydrate)` | `RequiredItems`, `RequiredAbilities` |
| **LocationGenerator** | `GenerateLocationAsync(type, hydrate)` | `NpcObjects`, `EnemyObjects`, `LootObjects` |
| **OrganizationGenerator** | `GenerateOrganizationAsync(type, hydrate)` | `MemberObjects`, `InventoryObjects` |

---

## ⚠️ OLD APPROACH (Still Works, But Not Recommended)

The manual resolution approach using `ReferenceResolverService` still works if you need it:

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
