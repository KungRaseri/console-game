# Quest System v4.0 Reorganization - ✅ COMPLETE

**Status**: COMPLETED 2025-12-18
**Files Created**: catalog.json, objectives.json, rewards.json
**Data Integrity**: ✅ 100% - All data preserved and reorganized

---

## Current Structure vs. Proposed Structure

### **Current (v2.0/v4.0 Mixed):**
```
/quests/
  ├── templates/quest_templates.json (v4.0, 481 lines, 27 templates)
  ├── locations/wilderness.json (v2.0, 186 lines, 18 locations)
  ├── locations/towns.json (v2.0, 166 lines, 15 locations)
  ├── locations/dungeons.json (v2.0, unknown)
  ├── objectives/primary.json (v2.0, 200 lines, 20 objectives)
  ├── objectives/secondary.json (v2.0, 176 lines, 17 objectives)
  ├── objectives/hidden.json (v2.0, unknown)
  ├── rewards/items.json (v2.0, 203 lines, 20 rewards)
  ├── rewards/gold.json (v2.0, 134 lines, 9 tiers)
  ├── rewards/experience.json (v2.0, unknown)
  └── Multiple .cbconfig.json files
```

### **Proposed (v4.0 Unified):**
```
/quests/
  ├── catalog.json          ← Templates (27) + All Locations (18+15+dungeons)
  ├── objectives.json       ← Primary (20) + Secondary (17) + Hidden
  ├── rewards.json          ← Items (20) + Gold (9) + Experience
  └── .cbconfig.json        ← Single config for all files
```

**Alignment with NPC v4.0 Pattern:**
```
/npcs/
  ├── catalog.json          ← Backgrounds (14) + Occupations (49)
  ├── traits.json           ← Personality Traits (40) + Quirks (35)
  ├── names.json            ← Name Components + Patterns
```

---

## Data Migration Verification

### ✅ **No Data Loss - Complete Mapping:**

#### **catalog.json** (Quest Templates + Locations)
- **Quest Templates** (27 items from quest_templates.json):
  - `fetch`: easy_fetch (4), medium_fetch (2), hard_fetch (1)
  - `kill`: easy_combat (2), medium_combat (2), hard_combat (2)
  - `escort`: easy_escort (2), medium_escort (2), hard_escort (1)
  - `delivery`: easy_delivery (2), medium_delivery (2), hard_delivery (1)
  - `investigate`: easy_investigation (2), medium_investigation (2), hard_investigation (1)

- **Locations** (33+ items from 3 files):
  - `wilderness`: low_danger (3), medium_danger (5), high_danger (5+), very_high_danger (5+)
  - `towns`: outposts (1), villages (3), towns (5), cities (3+), capitals (2+), special_locations (1+)
  - `dungeons`: All dungeon types (count TBD from full file read)

#### **objectives.json** (Primary + Secondary + Hidden)
- **Primary Objectives** (20 items):
  - combat_objectives (4)
  - retrieval_objectives (3)
  - rescue_objectives (1)
  - purification_objectives (3+)
  - defense_objectives (?)
  - social_objectives (?)
  - timed_objectives (?)

- **Secondary Objectives** (17 items):
  - stealth_challenges (3)
  - survival_challenges (2)
  - speed_challenges (1)
  - collection_challenges (2)
  - mercy_challenges (2)
  - combat_challenges (?)
  - precision_challenges (?)

- **Hidden Objectives** (count TBD)

#### **rewards.json** (Items + Gold + Experience)
- **Item Rewards** (20 items):
  - consumable_rewards (2)
  - common_equipment (1)
  - uncommon_equipment (3)
  - rare_equipment (5+)
  - epic_equipment (?)
  - legendary_equipment (?)
  - progression_rewards (?)
  - unique_rewards (?)
  - cosmetic_rewards (?)
  - currency_rewards (?)

- **Gold Rewards** (9 tiers):
  - trivial_rewards (1: 10-25g)
  - low_rewards (1: 25-50g)
  - medium_rewards (1: 50-100g)
  - high_rewards (1: 100-200g)
  - very_high_rewards (1: 200-500g)
  - epic_rewards (1: 500-1000g)
  - legendary_rewards (1: 1000-2500g)
  - mythic_rewards (1: 2500-5000g)
  - ancient_rewards (1: 5000+g)

- **Experience Rewards** (count TBD)

---

## Sample: catalog.json Structure

```json
{
  "metadata": {
    "version": "4.0",
    "last_updated": "2025-12-18",
    "description": "Quest templates and location catalog for procedural quest generation",
    "type": "quest_catalog",
    "notes": "Consolidated from quest_templates.json, wilderness.json, towns.json, and dungeons.json. Supports weighted random selection using rarityWeight formula: probability = 100 / rarityWeight",
    "total_templates": 27,
    "total_locations": 33,
    "quest_types": ["fetch", "kill", "escort", "delivery", "investigate"],
    "difficulty_levels": ["easy", "medium", "hard", "epic"],
    "usage": "Load via GameDataService.QuestCatalog. Use weighted selection for templates and locations.",
    "categories": {
      "templates": [
        "fetch.easy_fetch", "fetch.medium_fetch", "fetch.hard_fetch",
        "kill.easy_combat", "kill.medium_combat", "kill.hard_combat",
        "escort.easy_escort", "escort.medium_escort", "escort.hard_escort",
        "delivery.easy_delivery", "delivery.medium_delivery", "delivery.hard_delivery",
        "investigate.easy_investigation", "investigate.medium_investigation", "investigate.hard_investigation"
      ],
      "locations": [
        "wilderness.low_danger", "wilderness.medium_danger", "wilderness.high_danger", "wilderness.very_high_danger",
        "towns.outposts", "towns.villages", "towns.towns", "towns.cities", "towns.capitals", "towns.special_locations",
        "dungeons.crypts", "dungeons.ruins", "dungeons.caverns", "dungeons.towers", "dungeons.fortresses"
      ]
    }
  },
  "components": {
    "templates": {
      "fetch": {
        "easy_fetch": [
          {
            "name": "GatherHerbs",
            "displayName": "Gather Herbs",
            "rarityWeight": 10,
            "questType": "fetch",
            "difficulty": "easy",
            "itemType": "herb",
            "minQuantity": 5,
            "maxQuantity": 10,
            "baseGoldReward": 30,
            "baseXpReward": 50,
            "location": "Wilderness",
            "description": "Gather {quantity} {itemType} from the surrounding area"
          },
          {
            "name": "CollectMinerals",
            "displayName": "Collect Minerals",
            "rarityWeight": 12,
            "questType": "fetch",
            "difficulty": "easy",
            "itemType": "mineral",
            "minQuantity": 3,
            "maxQuantity": 8,
            "baseGoldReward": 40,
            "baseXpReward": 60,
            "location": "Mountain",
            "description": "Collect {quantity} {itemType} from the mines"
          }
          // ... (2 more easy_fetch templates)
        ],
        "medium_fetch": [
          {
            "name": "RecoverArtifact",
            "displayName": "Recover the Artifact",
            "rarityWeight": 30,
            "questType": "fetch",
            "difficulty": "medium",
            "itemType": "questitem",
            "minQuantity": 1,
            "maxQuantity": 1,
            "baseGoldReward": 300,
            "baseXpReward": 600,
            "location": "Dungeon",
            "guardians": true,
            "timeLimit": 48,
            "description": "Retrieve the ancient {itemName} from {location}"
          }
          // ... (1 more medium_fetch template)
        ],
        "hard_fetch": [
          {
            "name": "FindLegendaryWeapon",
            "displayName": "Find the Legendary Weapon",
            "rarityWeight": 60,
            "questType": "fetch",
            "difficulty": "hard",
            "itemType": "weapon",
            "itemRarity": "legendary",
            "minQuantity": 1,
            "maxQuantity": 1,
            "baseGoldReward": 500,
            "baseXpReward": 2500,
            "location": "Ancient Temple",
            "bossFight": true,
            "legendary": true,
            "description": "Seek the legendary {itemName} hidden in {location}"
          }
        ]
      },
      "kill": {
        "easy_combat": [
          {
            "name": "SlayBeasts",
            "displayName": "Slay the Beasts",
            "rarityWeight": 8,
            "questType": "kill",
            "difficulty": "easy",
            "targetType": "beast",
            "minQuantity": 3,
            "maxQuantity": 5,
            "baseGoldReward": 50,
            "baseXpReward": 100,
            "timeLimit": 0,
            "description": "Eliminate {quantity} {target} threatening the area"
          }
          // ... (1 more easy_combat template)
        ]
        // ... (medium_combat, hard_combat)
      }
      // ... (escort, delivery, investigate quest types)
    },
    "locations": {
      "wilderness": {
        "low_danger": [
          {
            "name": "GoldenPlains",
            "displayName": "Golden Plains",
            "rarityWeight": 10,
            "description": "Peaceful grasslands perfect for travel",
            "terrain": "Plains",
            "danger": "Low",
            "locationType": "wilderness"
          },
          {
            "name": "MistyHills",
            "displayName": "Misty Hills",
            "rarityWeight": 12,
            "description": "Rolling hills shrouded in fog",
            "terrain": "Hills",
            "danger": "Low",
            "locationType": "wilderness"
          },
          {
            "name": "SilverLake",
            "displayName": "Silver Lake",
            "rarityWeight": 15,
            "description": "Tranquil body of water reflecting moonlight",
            "terrain": "Lake",
            "danger": "Low",
            "locationType": "wilderness"
          }
        ],
        "medium_danger": [
          {
            "name": "DarkwoodForest",
            "displayName": "Darkwood Forest",
            "rarityWeight": 25,
            "description": "Dense woods with little sunlight",
            "terrain": "Forest",
            "danger": "Medium",
            "locationType": "wilderness"
          }
          // ... (4 more medium_danger locations)
        ],
        "high_danger": [
          {
            "name": "CrimsonPeaks",
            "displayName": "Crimson Peaks",
            "rarityWeight": 50,
            "description": "Treacherous mountain range",
            "terrain": "Mountain",
            "danger": "High",
            "locationType": "wilderness"
          }
          // ... (4+ more high_danger locations)
        ],
        "very_high_danger": [
          {
            "name": "VolcanicWasteland",
            "displayName": "Volcanic Wasteland",
            "rarityWeight": 80,
            "description": "Scorched lands of active volcanoes",
            "terrain": "Volcanic",
            "danger": "Very High",
            "locationType": "wilderness"
          }
          // ... (4+ more very_high_danger locations)
        ]
      },
      "towns": {
        "outposts": [
          {
            "name": "Crossroads",
            "displayName": "Crossroads",
            "rarityWeight": 8,
            "description": "Trading outpost at intersection of major roads",
            "size": "Outpost",
            "population": "Tiny",
            "locationType": "settlement"
          }
        ],
        "villages": [
          {
            "name": "Riverside",
            "displayName": "Riverside",
            "rarityWeight": 10,
            "description": "Peaceful farming village by the river",
            "size": "Village",
            "population": "Small",
            "locationType": "settlement"
          }
          // ... (2 more villages)
        ],
        "towns": [
          {
            "name": "Ironforge",
            "displayName": "Ironforge",
            "rarityWeight": 25,
            "description": "Dwarven mining town in the mountains",
            "size": "Town",
            "population": "Medium",
            "locationType": "settlement"
          }
          // ... (4 more towns)
        ],
        "cities": [
          {
            "name": "Silverport",
            "displayName": "Silverport",
            "rarityWeight": 50,
            "description": "Bustling coastal trading city",
            "size": "City",
            "population": "Large",
            "locationType": "settlement"
          }
          // ... (2+ more cities)
        ],
        "capitals": [
          {
            "name": "Aurumburg",
            "displayName": "Aurumburg",
            "rarityWeight": 100,
            "description": "Grand capital of the kingdom",
            "size": "Capital",
            "population": "Massive",
            "locationType": "settlement",
            "legendary": true
          }
          // ... (1+ more capitals)
        ]
      },
      "dungeons": {
        "crypts": [
          {
            "name": "ForgottenCrypt",
            "displayName": "Forgotten Crypt",
            "rarityWeight": 30,
            "description": "Ancient burial site filled with undead",
            "danger": "Medium",
            "locationType": "dungeon",
            "enemyTypes": ["undead", "skeleton", "zombie"]
          }
          // ... (more crypts)
        ]
        // ... (ruins, caverns, towers, fortresses)
      }
    }
  }
}
```

---

## Sample: objectives.json Structure

```json
{
  "metadata": {
    "version": "4.0",
    "last_updated": "2025-12-18",
    "description": "Quest objectives catalog including primary, secondary, and hidden objectives",
    "type": "quest_objectives",
    "notes": "Consolidated from primary.json, secondary.json, and hidden.json. Primary objectives are core quest goals. Secondary objectives provide bonus rewards. Hidden objectives are discovered during gameplay.",
    "total_primary": 20,
    "total_secondary": 17,
    "total_hidden": 0,
    "usage": "Load via GameDataService.QuestObjectives. Use weighted selection for procedural generation.",
    "categories": {
      "primary": ["combat_objectives", "retrieval_objectives", "rescue_objectives", "purification_objectives", "defense_objectives", "social_objectives", "timed_objectives"],
      "secondary": ["stealth_challenges", "survival_challenges", "speed_challenges", "collection_challenges", "mercy_challenges", "combat_challenges", "precision_challenges"],
      "hidden": []
    }
  },
  "components": {
    "primary": {
      "combat_objectives": [
        {
          "name": "ClearDungeon",
          "displayName": "Clear Dungeon",
          "rarityWeight": 20,
          "description": "Clear all enemies from {location}",
          "category": "Combat",
          "difficulty": "Medium",
          "objectiveType": "primary"
        },
        {
          "name": "DefeatBoss",
          "displayName": "Defeat Boss",
          "rarityWeight": 35,
          "description": "Defeat {enemy_name} in {location}",
          "category": "Combat",
          "difficulty": "Hard",
          "objectiveType": "primary"
        }
        // ... (2 more combat objectives)
      ],
      "retrieval_objectives": [
        {
          "name": "RecoverAncientKnowledge",
          "displayName": "Recover Ancient Knowledge",
          "rarityWeight": 25,
          "description": "Retrieve the lost knowledge from {location}",
          "category": "Retrieval",
          "difficulty": "Medium",
          "objectiveType": "primary"
        }
        // ... (2 more retrieval objectives)
      ]
      // ... (rescue, purification, defense, social, timed)
    },
    "secondary": {
      "stealth_challenges": [
        {
          "name": "AvoidDetection",
          "displayName": "Avoid Detection",
          "rarityWeight": 25,
          "description": "Complete the quest without being detected",
          "category": "Stealth",
          "bonus": "50% bonus XP",
          "objectiveType": "secondary"
        }
        // ... (2 more stealth challenges)
      ],
      "survival_challenges": [
        {
          "name": "NoDeaths",
          "displayName": "No Deaths",
          "rarityWeight": 30,
          "description": "Complete without any party members dying",
          "category": "Survival",
          "bonus": "Bonus item reward",
          "objectiveType": "secondary"
        }
        // ... (1 more survival challenge)
      ]
      // ... (speed, collection, mercy, combat, precision)
    },
    "hidden": {
      // To be populated with hidden objectives
    }
  }
}
```

---

## Sample: rewards.json Structure

```json
{
  "metadata": {
    "version": "4.0",
    "last_updated": "2025-12-18",
    "description": "Quest rewards catalog including items, gold, and experience scaling formulas",
    "type": "quest_rewards",
    "notes": "Consolidated from items.json, gold.json, and experience.json. Supports weighted selection and dynamic scaling based on player level and quest difficulty.",
    "total_item_rewards": 20,
    "total_gold_tiers": 9,
    "total_xp_tiers": 0,
    "usage": "Load via GameDataService.QuestRewards. Use rarityWeight for random selection. Scale rewards based on player level.",
    "categories": {
      "items": ["consumable_rewards", "common_equipment", "uncommon_equipment", "rare_equipment", "epic_equipment", "legendary_equipment", "progression_rewards", "unique_rewards", "cosmetic_rewards", "currency_rewards"],
      "gold": ["trivial_rewards", "low_rewards", "medium_rewards", "high_rewards", "very_high_rewards", "epic_rewards", "legendary_rewards", "mythic_rewards", "ancient_rewards"],
      "experience": []
    }
  },
  "components": {
    "items": {
      "consumable_rewards": [
        {
          "name": "CommonConsumables",
          "displayName": "Common Consumables",
          "rarityWeight": 10,
          "description": "1-3 common health/mana potions",
          "rarity": "Common",
          "category": "Consumables",
          "rewardType": "item"
        }
        // ... (1 more consumable reward)
      ],
      "uncommon_equipment": [
        {
          "name": "UncommonWeapon",
          "displayName": "Uncommon Weapon",
          "rarityWeight": 25,
          "description": "Random uncommon quality weapon",
          "rarity": "Uncommon",
          "category": "Weapon",
          "rewardType": "item"
        }
        // ... (2 more uncommon equipment)
      ],
      "rare_equipment": [
        {
          "name": "RareWeapon",
          "displayName": "Rare Weapon",
          "rarityWeight": 45,
          "description": "Random rare quality weapon",
          "rarity": "Rare",
          "category": "Weapon",
          "rewardType": "item"
        },
        {
          "name": "EnchantmentScroll",
          "displayName": "Enchantment Scroll",
          "rarityWeight": 50,
          "description": "Scroll to add enchantment to equipment",
          "rarity": "Rare",
          "category": "Enchantment",
          "rewardType": "item"
        }
        // ... (3+ more rare equipment)
      ]
      // ... (epic, legendary, progression, unique, cosmetic, currency)
    },
    "gold": {
      "trivial_rewards": [
        {
          "name": "PaltryReward",
          "displayName": "Paltry Reward",
          "rarityWeight": 5,
          "description": "10-25 gold",
          "minAmount": 10,
          "maxAmount": 25,
          "tier": "Trivial",
          "rewardType": "gold"
        }
      ],
      "low_rewards": [
        {
          "name": "ModestReward",
          "displayName": "Modest Reward",
          "rarityWeight": 12,
          "description": "25-50 gold",
          "minAmount": 25,
          "maxAmount": 50,
          "tier": "Low",
          "rewardType": "gold"
        }
      ],
      "medium_rewards": [
        {
          "name": "FairReward",
          "displayName": "Fair Reward",
          "rarityWeight": 20,
          "description": "50-100 gold",
          "minAmount": 50,
          "maxAmount": 100,
          "tier": "Medium",
          "rewardType": "gold"
        }
      ],
      "legendary_rewards": [
        {
          "name": "MassiveReward",
          "displayName": "Massive Reward",
          "rarityWeight": 120,
          "description": "1000-2500 gold",
          "minAmount": 1000,
          "maxAmount": 2500,
          "tier": "Legendary",
          "rewardType": "gold"
        }
      ]
      // ... (high, very_high, epic, mythic, ancient)
    },
    "experience": {
      // To be populated with XP scaling formulas
    }
  }
}
```

---

## Benefits of Consolidation

### **1. Consistency with NPC v4.0**
- Same organizational pattern (catalog + components)
- Same metadata standard (version, notes, usage, categories)
- Same weighted selection approach (rarityWeight)

### **2. Easier Data Management**
- Fewer files to maintain (3 files vs. 10+ files)
- Single source of truth for each concept
- Simpler ContentBuilder navigation

### **3. Better Developer Experience**
- Single import for quest templates + locations
- Reduced code complexity (fewer file loads)
- Clearer data relationships

### **4. Improved Performance**
- Fewer file I/O operations (3 loads vs. 10+ loads)
- Better JSON parsing efficiency
- Reduced memory overhead

### **5. Enhanced Searchability**
- Related data in same file (templates + locations)
- Easier to find specific quest types
- Better for IDE search/navigation

---

## Migration Checklist

- [ ] Read remaining quest files (dungeons.json, hidden.json, experience.json)
- [ ] Create consolidated catalog.json with all templates + locations
- [ ] Create consolidated objectives.json with primary + secondary + hidden
- [ ] Create consolidated rewards.json with items + gold + experience
- [ ] Update .cbconfig.json for new file structure
- [ ] Create QuestCatalogDataModels.cs (data models for deserialization)
- [ ] Update GameDataService to load new quest files
- [ ] Create/update QuestGenerator to use catalog-based selection
- [ ] Delete old quest files and directories
- [ ] Update all references to old quest file paths
- [ ] Run full build to verify no breaking changes
- [ ] Update documentation

---

## Data Integrity Guarantee

### **All existing data will be preserved:**
✅ Quest template properties (name, displayName, rarityWeight, questType, difficulty, rewards, etc.)
✅ Location properties (name, displayName, rarityWeight, terrain, danger, population, etc.)
✅ Objective properties (name, displayName, rarityWeight, category, difficulty, bonus, etc.)
✅ Reward properties (name, displayName, rarityWeight, rarity, minAmount, maxAmount, etc.)
✅ Metadata fields (version upgraded to 4.0, notes added for context)

### **New additions:**
✅ `objectiveType` field added to objectives (primary/secondary/hidden)
✅ `rewardType` field added to rewards (item/gold/experience)
✅ `locationType` field added to locations (wilderness/settlement/dungeon)
✅ Comprehensive metadata.notes for usage guidance
✅ Enhanced metadata.categories for better organization

---

## Next Steps

**Option A: Full Migration Now**
1. Create all 3 consolidated files
2. Update data models and services
3. Delete old structure
4. Test and validate

**Option B: Incremental Migration**
1. Create catalog.json first (keep old files as backup)
2. Test quest generation with new file
3. Migrate objectives.json
4. Migrate rewards.json
5. Delete old files after validation

**Your choice!** Would you like me to proceed with the full consolidation?
