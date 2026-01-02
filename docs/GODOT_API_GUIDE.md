# RealmEngine Godot API Guide

**Last Updated**: January 1, 2026  
**Version**: 1.0  
**For**: RealmEngine/RealmForge Integration

## Overview

This guide provides the correct API usage for all RealmEngine generators when called from Godot/GDScript.

---

## ✅ Working Generators

### 1. **LocationGenerator** - World Locations

```gdscript
# Valid Categories: "towns", "dungeons", "wilderness"
var locations = await generators.Locations.GenerateLocationsAsync("towns", 5)
var locations = await generators.Locations.GenerateLocationsAsync("dungeons", 3)
var locations = await generators.Locations.GenerateLocationsAsync("wilderness", 2)

# Generate specific location by name
var crossroads = await generators.Locations.GenerateLocationByNameAsync("towns", "Crossroads")
```

**Available Categories**:
- `towns` - Outposts, villages, towns, cities, capitals
- `dungeons` - Crypts, caves, ruins, fortresses
- `wilderness` - Forests, mountains, swamps, deserts

---

### 2. **OrganizationGenerator** - Guilds, Factions, Shops

```gdscript
# Valid Categories: "guilds", "factions", "shops", "businesses"
var guilds = await generators.Organizations.GenerateOrganizationsAsync("guilds", 5)
var factions = await generators.Organizations.GenerateOrganizationsAsync("factions", 3)
var shops = await generators.Organizations.GenerateOrganizationsAsync("shops", 2)
var businesses = await generators.Organizations.GenerateOrganizationsAsync("businesses", 2)

# Generate specific organization by name
var fighters_guild = await generators.Organizations.GenerateOrganizationByNameAsync("guilds", "fighters_guild")
```

**Available Categories**:
- `guilds` - Combat guilds, mage guilds, trade guilds, etc.
- `factions` - Political/social organizations with alignments
- `shops` - Weapon smiths, armorers, general stores, alchemists
- `businesses` - Taverns, inns, services

---

### 3. **NpcGenerator** - Non-Player Characters

```gdscript
# Valid Categories: 11 NPC types
var merchants = await generators.Npcs.GenerateNpcsAsync("merchants", 5)
var criminals = await generators.Npcs.GenerateNpcsAsync("criminal", 3)
var mages = await generators.Npcs.GenerateNpcsAsync("magical", 2)
var soldiers = await generators.Npcs.GenerateNpcsAsync("military", 4)

# Generate specific NPC by name
var general_merchant = await generators.Npcs.GenerateNpcByNameAsync("merchants", "GeneralMerchant")
```

**Available Categories**:
- `common` - Farmers, laborers, villagers
- `criminal` - Thieves, smugglers, ex-criminals
- `magical` - Wizards, apprentices, hedge wizards
- `merchants` - Traders, shopkeepers
- `military` - Soldiers, guards, officers
- `noble` - Lords, knights, courtiers
- `professional` - Craftsmen, healers, scholars
- `religious` - Priests, clerics, monks
- `service` - Innkeepers, barkeeps, servants
- `uncommon` - Rare/unique NPCs
- `rare` - Very rare/legendary NPCs

**Note**: Fixed in v1.0 - `background_types` now supported (criminal/magical work correctly)

---

### 4. **QuestGenerator** - Quest Templates

```gdscript
# Valid Quest Types: "investigate", "fetch", "escort", "kill", "delivery"
var investigation_quests = await generators.Quests.GenerateQuestsAsync("investigate", 3)
var fetch_quests = await generators.Quests.GenerateQuestsAsync("fetch", 5)
var escort_quests = await generators.Quests.GenerateQuestsAsync("escort", 2)
var kill_quests = await generators.Quests.GenerateQuestsAsync("kill", 4)
var delivery_quests = await generators.Quests.GenerateQuestsAsync("delivery", 3)

# Generate specific quest by name
var find_clues = await generators.Quests.GenerateQuestByNameAsync("investigate", "FindClues")
```

**Available Quest Types**:
- `investigate` - Clue finding, mystery solving, investigation
- `fetch` - Herb gathering, artifact retrieval, item collection
- `escort` - NPC protection, convoy escort
- `kill` - Monster slaying, bandit elimination
- `delivery` - Package delivery, message running

**❌ INVALID Quest Types** (these are NOT quest types):
- ~~`main`~~ - Does not exist
- ~~`side`~~ - Does not exist
- ~~`objectives`~~ - Separate catalog for quest objective templates
- ~~`rewards`~~ - Separate catalog for quest reward templates

---

### 5. **DialogueGenerator** - NPC Dialogue

```gdscript
# Valid Dialogue Types: "greetings", "farewells", "responses"
var greetings = await generators.Dialogue.GenerateDialogueAsync("greetings", "*", 5)
var farewells = await generators.Dialogue.GenerateDialogueAsync("farewells", "*", 3)
var responses = await generators.Dialogue.GenerateDialogueAsync("responses", "*", 10)

# Helper methods for single dialogue lines
var greeting = await generators.Dialogue.GenerateGreetingAsync("casual")
var farewell = await generators.Dialogue.GenerateFarewellAsync("formal")
var response = await generators.Dialogue.GenerateResponseAsync("shop_inquiry", "merchant")

# Generate full conversation pattern
var conversation = await generators.Dialogue.GenerateConversationAsync("friendly")
# Returns dictionary with keys: greeting, response1, response2, response3, farewell
```

**Available Dialogue Types**:
- `greetings` - NPC greeting lines organized by NPC type
- `farewells` - NPC goodbye lines
- `responses` - NPC response lines for conversations

**Style Filtering**:
- Use `"*"` for random style selection
- Or specify style: `"casual"`, `"formal"`, `"merchant"`, `"scholarly"`, etc.

**Note**: Fixed in v1.0 - Path corrected to `social/dialogue/`, hierarchical parsing added, templates array support

---

### 6. **EnemyGenerator** - Combat Enemies

```gdscript
# Valid Categories: "beasts", "undead", "dragons", "humanoid", "aberrations", etc.
var beasts = await generators.Enemies.GenerateEnemiesAsync("beasts", 10)
var undead = await generators.Enemies.GenerateEnemiesAsync("undead", 5)
var dragons = await generators.Enemies.GenerateEnemiesAsync("dragons", 1)

# Generate specific enemy by name
var wolf = await generators.Enemies.GenerateEnemyByNameAsync("beasts", "Wolf")
var skeleton = await generators.Enemies.GenerateEnemyByNameAsync("undead", "Skeleton")
```

**Available Categories**:
- `beasts` - Wolves, bears, dire animals
- `undead` - Skeletons, zombies, ghosts, vampires
- `dragons` - Various dragon types
- `humanoid` - Goblins, orcs, bandits
- `aberrations` - Eldritch horrors
- `constructs` - Golems, animated objects
- `elementals` - Fire, water, earth, air elementals
- `fey` - Sprites, pixies, dryads
- `giant` - Giants, ogres, trolls
- `monstrosity` - Griffons, chimeras

---

### 7. **ItemGenerator** - Equipment & Consumables

```gdscript
# Valid Categories: weapons, armor, consumables, materials, etc.
var weapons = await generators.Items.GenerateItemsAsync("weapons", 10)
var armor = await generators.Items.GenerateItemsAsync("armor", 5)
var potions = await generators.Items.GenerateItemsAsync("consumables", 20)

# Generate specific item by name
var iron_sword = await generators.Items.GenerateItemByNameAsync("weapons", "Iron Longsword")
```

**Available Categories**:
- `weapons` - Swords, axes, bows, staffs
- `armor` - Light, medium, heavy armor
- `consumables` - Potions, scrolls, food
- `materials` - Ores, herbs, components
- `accessories` - Rings, amulets, trinkets

---

### 8. **EnchantmentGenerator** - Magic Item Enchantments

```gdscript
# Valid Categories: "offensive", "defensive", "utility"
var offensive = await generators.Enchantments.GenerateEnchantmentsAsync("offensive", 5)
var defensive = await generators.Enchantments.GenerateEnchantmentsAsync("defensive", 3)
var utility = await generators.Enchantments.GenerateEnchantmentsAsync("utility", 2)

# Generate specific enchantment by name
var flame_blade = await generators.Enchantments.GenerateEnchantmentByNameAsync("offensive", "Flame Blade")
```

**Available Categories**:
- `offensive` - Fire damage, poison, elemental attacks
- `defensive` - Armor bonuses, resistances, shields
- `utility` - Light, detection, speed boosts

**Note**: Use `generators.Enchantments` not `generators.Items.GenerateItemsAsync("enchantments")`

---

### 9. **AbilityGenerator** - Character Abilities

```gdscript
# Valid Categories: "active", "passive", "racial", "class"
var active_abilities = await generators.Abilities.GenerateAbilitiesAsync("active", 5)
var passive_abilities = await generators.Abilities.GenerateAbilitiesAsync("passive", 3)

# Generate specific ability by name
var fireball = await generators.Abilities.GenerateAbilityByNameAsync("active", "Fireball")
```

**Available Categories**:
- `active` - Spells, attacks, actions
- `passive` - Permanent bonuses, traits
- `racial` - Species-specific abilities
- `class` - Class-specific abilities

---

## ❌ Common Mistakes

### 1. Wrong Quest Types
```gdscript
# ❌ WRONG - These quest types don't exist
var main_quests = await generators.Quests.GenerateQuestsAsync("main", 5)  # FAILS
var side_quests = await generators.Quests.GenerateQuestsAsync("side", 5)  # FAILS

# ✅ CORRECT - Use actual quest types
var investigate = await generators.Quests.GenerateQuestsAsync("investigate", 5)
var fetch = await generators.Quests.GenerateQuestsAsync("fetch", 5)
```

### 2. Wrong NPC Type for Dialogue
```gdscript
# ❌ WRONG - "dialogue" is not an NPC category
var dialogue_npcs = await generators.Npcs.GenerateNpcsAsync("dialogue", 5)  # FAILS

# ✅ CORRECT - Use DialogueGenerator instead
var greetings = await generators.Dialogue.GenerateDialogueAsync("greetings", "*", 5)
```

### 3. Wrong Enchantment API
```gdscript
# ❌ WRONG - Using Items generator for enchantments
var enchants = await generators.Items.GenerateItemsAsync("enchantments", 5)  # RETURNS EMPTY

# ✅ CORRECT - Use EnchantmentGenerator
var enchants = await generators.Enchantments.GenerateEnchantmentsAsync("offensive", 5)
```

### 4. Quest Objectives/Rewards
```gdscript
# ❌ WRONG - These are not quest types
var objectives = await generators.Quests.GenerateQuestsAsync("objectives", 5)  # FAILS
var rewards = await generators.Quests.GenerateQuestsAsync("rewards", 5)  # FAILS

# ℹ️ NOTE: objectives and rewards are supporting catalogs
# They are used INTERNALLY by the quest generator, not as quest types
# If you need custom objectives/rewards, use the quest_builder API instead
```

---

## 📊 Quick Reference Table

| Generator | Valid Categories | Common Mistake |
|-----------|-----------------|----------------|
| **Locations** | towns, dungeons, wilderness | ❌ Using "location" instead |
| **Organizations** | guilds, factions, shops, businesses | ✅ All work correctly |
| **NPCs** | 11 types (merchants, criminal, magical, etc.) | ❌ Using "dialogue" as NPC type |
| **Quests** | investigate, fetch, escort, kill, delivery | ❌ Using "main", "side", "objectives", "rewards" |
| **Dialogue** | greetings, farewells, responses | ✅ Fixed in v1.0 |
| **Enemies** | beasts, undead, dragons, humanoid, etc. | ✅ All work correctly |
| **Items** | weapons, armor, consumables, materials | ❌ Using "enchantments" (use EnchantmentGenerator) |
| **Enchantments** | offensive, defensive, utility | ✅ Use separate generator |
| **Abilities** | active, passive, racial, class | ✅ All work correctly |

---

## 🔧 Debugging Tips

### Check if Category Exists
```gdscript
# Get all available domains
var domains = data_cache.AllDomains
print("Available domains: ", domains)

# Get subdomains for a specific domain
var npc_types = data_cache.GetSubdomainsForDomain("npcs")
print("NPC types: ", npc_types)

# Get files for a domain
var quest_files = data_cache.GetFilesByDomain("quests")
print("Quest files: ", quest_files.Count)
```

### Test Generator Returns
```gdscript
var npcs = await generators.Npcs.GenerateNpcsAsync("criminal", 2)
if npcs.Count == 0:
    print("ERROR: Criminal NPCs returned empty!")
    print("Check if catalog exists: npcs/criminal/catalog.json")
else:
    print("SUCCESS: Generated ", npcs.Count, " criminal NPCs")
```

### Common Null Returns
If a generator returns null or empty:
1. **Check catalog file exists**: Use `data_cache.GetFile("domain/category/catalog.json")`
2. **Check catalog structure**: Ensure it has proper hierarchical format
3. **Check category name**: Use exact lowercase names from this guide
4. **Check generator name**: Some categories need specific generators (e.g., EnchantmentGenerator)

---

## 📝 Version History

### v1.0 (January 1, 2026)
- ✅ Fixed NpcGenerator: Now supports `background_types` (criminal/magical work)
- ✅ Fixed DialogueGenerator: Path corrected, hierarchical parsing, templates support
- ✅ Fixed LocationGenerator: Hierarchical catalog parsing
- ✅ Fixed OrganizationGenerator: Hierarchical parsing + string reputation handling
- ✅ Comprehensive test coverage: 5,179+ tests passing
- ✅ Documented all valid categories and common mistakes

---

## 🚀 Best Practices

1. **Always await async methods**
2. **Check return values for null/empty** before using
3. **Use exact category names** from this guide (case-sensitive)
4. **Use `"*"` for random selection** where applicable
5. **Cache generators** in Godot autoload for performance
6. **Use helper methods** (e.g., `GenerateGreetingAsync`) for single items
7. **Test with small counts first** (1-5 items) before generating large batches

---

## 📚 Additional Resources

- **RealmForge Tool**: Visual editor for all JSON catalogs
- **JSON Standards**: See `docs/standards/json/` for catalog format details
- **Test Coverage**: See generator test files for usage examples
- **API Reference**: IntelliSense/XML docs included in deployed DLLs

---

**End of Guide**
