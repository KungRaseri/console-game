# NPC System Implementation Plan

**Date**: 2025-12-18
**Status**: Ready for Implementation
**Context**: Comprehensive plan based on approved design decisions

---

## ✅ Approved Design Decisions

### Structure
- ✅ **Option A**: Single `npcs/` directory structure
- ✅ Backgrounds and Occupations separated in catalog.json
- ✅ Pattern-based name generation (like weapons/enemies)
- ✅ Multiple titles allowed ("Captain Sir Aldric")

### Names/Titles
- ✅ Soft filtering with weight multipliers
- ✅ Titles are decorative (gameplay bonuses added later)
- ✅ Earning/losing titles supported (future feature)
- ✅ Pattern-based title exclusion supported

### Shops & Economy
- ✅ **Hybrid approach**: Start simple, expand later
- ✅ Occupations define shop inventory (tight coupling)
- ✅ Personality traits affect prices
- ✅ Specializations supported (future expansion)
- ✅ **Hybrid inventory**: Core items + Dynamic items
- ✅ **Player economy**: Players can sell to merchants
- ✅ **Item decay**: 7-day decay system (unsold items removed)
- ✅ **Item retention**: Sold items keep exact properties (enchantments, quality)
- ✅ **Limited merchant gold**: Based on background/occupation factors
- ✅ **Quality pricing**: RarityWeight accumulation affects prices

### Future Features (Designed, Not Implemented Yet)
- ⏭️ NPC purchasing from NPCs (background simulation)
- ⏭️ Gameplay bonuses from titles
- ⏭️ Occupation specializations ("Weapon Specialist Blacksmith")
- ⏭️ Background modifiers on shop stats

---

## 📁 Phase 1: File Structure Reorganization

### 1.1: Create New npcs/ Structure

**Target Structure:**
```
Game.Data/Data/Json/npcs/
├── .cbconfig.json
├── catalog.json           ← NEW: backgrounds{} + occupations{}
├── traits.json            ← NEW: personality_traits{} + quirks{}
├── names.json             ← NEW: pattern-based generation
└── dialogue/
    ├── .cbconfig.json
    ├── greetings.json     ← MOVED from npcs/dialogue/
    ├── farewells.json     ← MOVED
    ├── rumors.json        ← MOVED
    └── styles.json        ← MOVED (renamed from templates.json)
```

**Files to Delete (After Migration):**
```
Game.Data/Data/Json/npcs/
├── names/
│   ├── first_names.json   ← DELETE (data moved to names.json)
│   └── last_names.json    ← DELETE
├── occupations/
│   └── occupations.json   ← DELETE (data moved to catalog.json)
└── personalities/
    ├── backgrounds.json   ← DELETE (data moved to catalog.json)
    ├── personality_traits.json ← DELETE (data moved to traits.json)
    └── quirks.json        ← DELETE (data moved to traits.json)
```

---

## 📄 Phase 2: Create catalog.json

### 2.1: Structure Definition

```json
{
  "metadata": {
    "type": "npc_catalog",
    "version": "4.0",
    "description": "NPC backgrounds and occupations for character generation",
    "lastModified": "2025-12-18",
    "componentKeys": ["backgrounds", "occupations"],
    "notes": [
      "Backgrounds represent past/history (affects starting conditions)",
      "Occupations represent current job (affects skills/gold/shop)",
      "NPCs can have both background AND occupation",
      "Shop system: hybrid (core items + dynamic items)",
      "Player economy: limited gold, 7-day decay, quality pricing"
    ]
  },
  
  "backgrounds": {
    "common_folk": {
      "metadata": {
        "description": "Everyday people from humble origins",
        "socialClass": "common"
      },
      "items": [
        {
          "name": "Commoner",
          "rarityWeight": 100,
          "socialClass": "common",
          "startingGold": "1d10",
          "skillBonuses": ["endurance", "crafting"],
          "shopModifiers": {
            "qualityBonus": 0,
            "priceMultiplier": 0.95,
            "shopAppearance": "practical"
          },
          "specializationHints": ["practical", "budget", "common_goods"],
          "uniqueItems": ["Handmade Charm", "Family Heirloom"]
        },
        {
          "name": "Orphan",
          "rarityWeight": 60,
          "socialClass": "common",
          "startingGold": "1d4",
          "skillBonuses": ["survival", "stealth"],
          "backgroundStory": "Grew up on the streets, learning to fend for themselves",
          "shopModifiers": {
            "qualityBonus": -5,
            "priceMultiplier": 0.85,
            "shopAppearance": "humble"
          },
          "specializationHints": ["scavenged", "repaired", "secondhand"],
          "uniqueItems": ["Patched Cloak", "Lucky Coin"]
        }
      ]
    },
    
    "military": {
      "metadata": {
        "description": "Those with military training and combat experience",
        "socialClass": "military"
      },
      "items": [
        {
          "name": "FormerSoldier",
          "displayName": "Former Soldier",
          "rarityWeight": 50,
          "socialClass": "military",
          "startingGold": "2d10",
          "skillBonuses": ["melee_combat", "tactics", "discipline"],
          "backgroundStory": "Served in the army, now seeking a new purpose",
          "shopModifiers": {
            "qualityBonus": 15,
            "priceMultiplier": 1.0,
            "shopAppearance": "utilitarian",
            "combatItemBonus": 20
          },
          "specializationHints": ["military", "weapons", "tactical_gear"],
          "uniqueItems": ["Officer's Sword", "Battle-worn Shield", "Military Medal"]
        }
      ]
    },
    
    "nobility": {
      "metadata": {
        "description": "Born into wealth and privilege",
        "socialClass": "noble"
      },
      "items": [
        {
          "name": "NobleBorn",
          "displayName": "Noble-born",
          "rarityWeight": 80,
          "socialClass": "noble",
          "startingGold": "5d10",
          "skillBonuses": ["persuasion", "etiquette", "leadership"],
          "backgroundStory": "Raised in luxury with the finest education",
          "shopModifiers": {
            "qualityBonus": 10,
            "priceMultiplier": 1.15,
            "shopAppearance": "elegant",
            "rarityBonus": 10
          },
          "specializationHints": ["ceremonial", "light_armor", "finesse_weapons"],
          "uniqueItems": ["Dueling Rapier", "Silk-lined Gloves", "Family Crest Ring"]
        }
      ]
    }
  },
  
  "occupations": {
    "craftsmen": {
      "metadata": {
        "description": "Skilled artisans who create goods",
        "hasShop": true
      },
      "items": [
        {
          "name": "Blacksmith",
          "rarityWeight": 20,
          "socialClass": "craftsman",
          "skillBonuses": ["smithing", "crafting"],
          "baseGold": "3d10",
          "shopType": "smithy",
          "shopChance": 0.8,
          "shopInventory": {
            "coreItems": [
              { "item": "Longsword", "baseQuantity": "infinite" },
              { "item": "Chainmail", "baseQuantity": 2, "restockDaily": true }
            ],
            "dynamicCategories": ["weapons.melee", "armor.heavy"],
            "dynamicItemCount": "1d6+2",
            "dynamicRefreshDaily": true,
            "economy": {
              "acceptsPlayerItems": true,
              "acceptedCategories": ["weapons.melee", "armor.all"],
              "buyPriceMultiplier": 0.4,
              "sellPriceMultiplier": 1.0,
              "resellPriceMultiplier": 0.8,
              "playerItemDecayDaily": 0.1,
              "playerItemDecayDays": 7,
              "maxPlayerItems": 10,
              "bankGoldFormula": "baseGold * 10 + background.startingGold * 5",
              "bankGoldRestockDaily": true
            }
          },
          "canCraft": ["weapons", "armor"],
          "craftingQualityBonus": 30
        },
        {
          "name": "Apothecary",
          "rarityWeight": 25,
          "socialClass": "craftsman",
          "skillBonuses": ["healing", "herbalism"],
          "baseGold": "2d10",
          "shopType": "apothecary",
          "shopChance": 0.85,
          "shopInventory": {
            "coreItems": [
              { "item": "HealthPotion", "baseQuantity": 5, "restockDaily": true },
              { "item": "ManaPotion", "baseQuantity": 3, "restockDaily": true }
            ],
            "dynamicCategories": ["consumables.potions", "consumables.herbs"],
            "dynamicItemCount": "1d10+3",
            "dynamicRefreshDaily": true,
            "economy": {
              "acceptsPlayerItems": true,
              "acceptedCategories": ["consumables.potions", "consumables.herbs"],
              "buyPriceMultiplier": 0.4,
              "sellPriceMultiplier": 1.0,
              "resellPriceMultiplier": 0.75,
              "playerItemDecayDaily": 0.15,
              "playerItemDecayDays": 5,
              "maxPlayerItems": 15,
              "bankGoldFormula": "baseGold * 8 + background.startingGold * 3",
              "bankGoldRestockDaily": true
            }
          }
        }
      ]
    },
    
    "merchants": {
      "metadata": {
        "description": "Traders and sellers",
        "hasShop": true
      },
      "items": [
        {
          "name": "GeneralMerchant",
          "displayName": "General Merchant",
          "rarityWeight": 10,
          "socialClass": "merchant",
          "skillBonuses": ["persuasion", "appraisal"],
          "baseGold": "5d10",
          "shopType": "general_store",
          "shopChance": 0.95,
          "shopInventory": {
            "coreItems": [
              { "item": "Torch", "baseQuantity": 10, "restockDaily": true },
              { "item": "Rope", "baseQuantity": 5, "restockDaily": true }
            ],
            "dynamicCategories": ["all"],
            "dynamicItemCount": "3d10+10",
            "dynamicRefreshDaily": true,
            "varietyBonus": 50,
            "economy": {
              "acceptsPlayerItems": true,
              "acceptedCategories": ["all"],
              "buyPriceMultiplier": 0.35,
              "sellPriceMultiplier": 1.0,
              "resellPriceMultiplier": 0.85,
              "playerItemDecayDaily": 0.1,
              "playerItemDecayDays": 7,
              "maxPlayerItems": 20,
              "bankGoldFormula": "baseGold * 15 + background.startingGold * 10",
              "bankGoldRestockDaily": true
            }
          }
        }
      ]
    },
    
    "service": {
      "metadata": {
        "description": "Service providers",
        "hasShop": true
      },
      "items": [
        {
          "name": "TavernKeeper",
          "rarityWeight": 15,
          "socialClass": "common",
          "skillBonuses": ["cooking", "persuasion"],
          "baseGold": "3d10",
          "shopType": "tavern",
          "shopChance": 1.0,
          "shopInventory": {
            "coreItems": [
              { "item": "Ale", "baseQuantity": 20, "restockDaily": true },
              { "item": "Bread", "baseQuantity": 15, "restockDaily": true }
            ],
            "dynamicCategories": ["consumables.food", "consumables.drink"],
            "dynamicItemCount": "2d6+5",
            "dynamicRefreshDaily": true,
            "economy": {
              "acceptsPlayerItems": false,
              "acceptedCategories": [],
              "buyPriceMultiplier": 0.0,
              "sellPriceMultiplier": 1.0,
              "resellPriceMultiplier": 0.0,
              "bankGoldFormula": "baseGold * 12",
              "bankGoldRestockDaily": true
            }
          },
          "services": ["lodging", "rumors", "quests"]
        }
      ]
    },
    
    "non_merchants": {
      "metadata": {
        "description": "NPCs without shops",
        "hasShop": false
      },
      "items": [
        {
          "name": "Guard",
          "rarityWeight": 28,
          "socialClass": "military",
          "skillBonuses": ["melee_combat", "perception"],
          "baseGold": "2d10",
          "shopType": null,
          "shopChance": 0.0
        },
        {
          "name": "Farmer",
          "rarityWeight": 85,
          "socialClass": "common",
          "skillBonuses": ["farming", "endurance"],
          "baseGold": "1d10",
          "shopType": null,
          "shopChance": 0.0
        }
      ]
    }
  }
}
```

### 2.2: Key Features Explained

#### bankGoldFormula
```json
"bankGoldFormula": "baseGold * 10 + background.startingGold * 5"
```

**How it works:**
```
Blacksmith (baseGold: 3d10 = avg 16.5)
+ Noble-born background (startingGold: 5d10 = avg 27.5)

Bank Gold = (16.5 * 10) + (27.5 * 5)
          = 165 + 137.5
          = 302.5g

Blacksmith (baseGold: 3d10 = avg 16.5)
+ Orphan background (startingGold: 1d4 = avg 2.5)

Bank Gold = (16.5 * 10) + (2.5 * 5)
          = 165 + 12.5
          = 177.5g
```

**Result:** Noble-born merchants have more gold, orphan merchants have less

#### Quality Pricing (Using RarityWeight)

**Formula:**
```
Base Price = item.basePrice
Accumulated Rarity Weight = SUM(component.rarityWeight for all components)
Quality Modifier = AccumulatedRarityWeight / 100

Final Price = Base Price * Quality Modifier
```

**Example:**
```
Enchanted Steel Longsword
- Base: Longsword (basePrice: 100g)
- Components:
  - Enchanted (rarityWeight: 15)
  - Steel (rarityWeight: 30)
  
Accumulated Rarity = 15 + 30 = 45
Quality Modifier = 45 / 100 = 0.45
Final Price = 100 * (1 + 0.45) = 145g

Rusty Iron Longsword
- Base: Longsword (basePrice: 100g)
- Components:
  - Rusty (rarityWeight: 50)
  - Iron (rarityWeight: 40)
  
Accumulated Rarity = 50 + 40 = 90
Quality Modifier = 90 / 100 = 0.90
Final Price = 100 * (1 + 0.90) = 190g

WAIT - this seems backwards!
```

**Better Formula (Inverse Rarity):**
```
Quality Factor = 1 / (AccumulatedRarityWeight / 100)

Enchanted Steel (rarity 45) = RARE = High quality
Quality Factor = 1 / (45/100) = 1 / 0.45 = 2.22
Final Price = 100 * 2.22 = 222g ✓

Rusty Iron (rarity 90) = COMMON = Low quality
Quality Factor = 1 / (90/100) = 1 / 0.90 = 1.11
Final Price = 100 * 1.11 = 111g ✓
```

**Actually, let's reconsider:**

RarityWeight in weapons.json:
- **Low rarityWeight = RARE** (e.g., "Enchanted" = 15 = appears rarely)
- **High rarityWeight = COMMON** (e.g., "Rusty" = 50 = appears often)

So for pricing:
- Rare items should cost MORE
- Common items should cost LESS

**Final Formula:**
```
// Lower rarity weight = rarer = more expensive
PriceMultiplier = (100 / AccumulatedRarityWeight) * BaseMultiplier

Where BaseMultiplier = 1.0 for normal scaling

Enchanted Steel (rarity 45):
PriceMultiplier = (100 / 45) * 1.0 = 2.22
Final Price = 100 * 2.22 = 222g ✓ (RARE = EXPENSIVE)

Rusty Iron (rarity 90):
PriceMultiplier = (100 / 90) * 1.0 = 1.11
Final Price = 100 * 1.11 = 111g ✓ (COMMON = CHEAP)

Plain (no modifiers, assume rarity 100):
PriceMultiplier = (100 / 100) * 1.0 = 1.0
Final Price = 100 * 1.0 = 100g ✓ (NORMAL)
```

This makes sense! Store in metadata:
```json
{
  "pricing": {
    "formula": "(100 / AccumulatedRarityWeight) * basePrice",
    "notes": [
      "Lower rarity weight = rarer items = higher price",
      "Higher rarity weight = common items = lower price",
      "Plain items (no components) assume rarity weight of 100"
    ]
  }
}
```

---

## 📄 Phase 3: Create traits.json

### 3.1: Structure Definition

```json
{
  "metadata": {
    "type": "npc_traits",
    "version": "4.0",
    "description": "Personality traits and quirks for NPC characterization",
    "lastModified": "2025-12-18",
    "componentKeys": ["personality_traits", "quirks"],
    "notes": [
      "Personality traits affect shop prices and dialogue style",
      "Quirks add flavor and unique characteristics",
      "NPCs can have multiple traits and quirks"
    ]
  },
  
  "personality_traits": {
    "positive": {
      "metadata": {
        "description": "Beneficial personality traits"
      },
      "items": [
        {
          "name": "Generous",
          "rarityWeight": 40,
          "description": "Tends to give discounts and help others",
          "shopModifiers": {
            "priceMultiplier": 0.9,
            "buyPriceMultiplier": 1.1
          },
          "dialogueStyle": "friendly"
        },
        {
          "name": "Honest",
          "rarityWeight": 50,
          "description": "Never cheats or lies",
          "shopModifiers": {
            "qualityGuarantee": true
          },
          "dialogueStyle": "straightforward"
        }
      ]
    },
    
    "negative": {
      "metadata": {
        "description": "Challenging personality traits"
      },
      "items": [
        {
          "name": "Greedy",
          "rarityWeight": 45,
          "description": "Always looking to maximize profit",
          "shopModifiers": {
            "priceMultiplier": 1.2,
            "buyPriceMultiplier": 0.8
          },
          "dialogueStyle": "calculating"
        },
        {
          "name": "Suspicious",
          "rarityWeight": 55,
          "description": "Doesn't trust strangers easily",
          "shopModifiers": {
            "reputationRequired": 10
          },
          "dialogueStyle": "cautious"
        }
      ]
    }
  },
  
  "quirks": {
    "physical": {
      "metadata": {
        "description": "Physical characteristics or habits"
      },
      "items": [
        {
          "name": "OneEyed",
          "displayName": "One-Eyed",
          "rarityWeight": 70,
          "description": "Has lost an eye, wears an eyepatch"
        },
        {
          "name": "Tattooed",
          "rarityWeight": 50,
          "description": "Covered in intricate tattoos"
        }
      ]
    },
    
    "behavioral": {
      "metadata": {
        "description": "Behavioral quirks and mannerisms"
      },
      "items": [
        {
          "name": "MuttersToSelf",
          "displayName": "Mutters to Self",
          "rarityWeight": 60,
          "description": "Constantly mumbling under their breath"
        },
        {
          "name": "CollectsCoins",
          "displayName": "Coin Collector",
          "rarityWeight": 65,
          "description": "Obsessively collects rare coins"
        }
      ]
    }
  }
}
```

---

## 📄 Phase 4: Create names.json

### 4.1: Structure Definition

```json
{
  "metadata": {
    "type": "pattern_generation",
    "version": "4.0",
    "description": "Pattern-based NPC name generation system",
    "lastModified": "2025-12-18",
    "componentKeys": ["title", "first_name", "surname", "suffix"],
    "patternTokens": ["title", "first_name", "surname", "suffix"],
    "raritySystem": "weight-based",
    "supportsSoftFiltering": true,
    "notes": [
      "Titles use soft filtering with weight multipliers based on social class",
      "First names are categorized by gender and style",
      "Surnames are categorized by culture",
      "Suffixes are epithets and titles of renown",
      "Patterns determine name structure (simple to legendary)"
    ]
  },
  
  "components": {
    "title": [
      {
        "value": "Sir",
        "rarityWeight": 40,
        "gender": "male",
        "preferredSocialClass": "noble",
        "weightMultiplier": {
          "noble": 1.0,
          "military": 0.5,
          "common": 0.1,
          "criminal": 0.01
        }
      },
      {
        "value": "Lady",
        "rarityWeight": 40,
        "gender": "female",
        "preferredSocialClass": "noble",
        "weightMultiplier": {
          "noble": 1.0,
          "military": 0.3,
          "common": 0.1,
          "criminal": 0.01
        }
      },
      {
        "value": "Lord",
        "rarityWeight": 50,
        "gender": "male",
        "preferredSocialClass": "noble",
        "weightMultiplier": {
          "noble": 1.0,
          "common": 0.05,
          "criminal": 0.01
        }
      },
      {
        "value": "Master",
        "rarityWeight": 30,
        "gender": "male",
        "preferredSocialClass": "craftsman",
        "weightMultiplier": {
          "craftsman": 1.0,
          "merchant": 0.8,
          "common": 0.3,
          "noble": 0.2
        }
      },
      {
        "value": "Mistress",
        "rarityWeight": 30,
        "gender": "female",
        "preferredSocialClass": "craftsman",
        "weightMultiplier": {
          "craftsman": 1.0,
          "merchant": 0.8,
          "common": 0.3,
          "noble": 0.2
        }
      },
      {
        "value": "Captain",
        "rarityWeight": 35,
        "preferredSocialClass": "military",
        "weightMultiplier": {
          "military": 1.0,
          "noble": 0.6,
          "common": 0.2
        }
      }
    ],
    
    "first_name": {
      "male": {
        "common": [
          { "value": "Gareth", "rarityWeight": 40 },
          { "value": "Marcus", "rarityWeight": 45 },
          { "value": "Cole", "rarityWeight": 50 },
          { "value": "Finn", "rarityWeight": 55 }
        ],
        "noble": [
          { "value": "Aldric", "rarityWeight": 60 },
          { "value": "Cedric", "rarityWeight": 65 },
          { "value": "Reginald", "rarityWeight": 70 }
        ],
        "mystical": [
          { "value": "Eldrin", "rarityWeight": 75 },
          { "value": "Thalion", "rarityWeight": 80 }
        ]
      },
      "female": {
        "common": [
          { "value": "Mara", "rarityWeight": 40 },
          { "value": "Elise", "rarityWeight": 45 },
          { "value": "Brynn", "rarityWeight": 50 }
        ],
        "noble": [
          { "value": "Isolde", "rarityWeight": 60 },
          { "value": "Celestine", "rarityWeight": 65 },
          { "value": "Evangeline", "rarityWeight": 70 }
        ],
        "mystical": [
          { "value": "Lyra", "rarityWeight": 75 },
          { "value": "Seraphina", "rarityWeight": 80 }
        ]
      }
    },
    
    "surname": {
      "fantasy": [
        { "value": "Ironforge", "rarityWeight": 45 },
        { "value": "Stormwind", "rarityWeight": 50 },
        { "value": "Dragonheart", "rarityWeight": 70 }
      ],
      "nordic": [
        { "value": "Bjornson", "rarityWeight": 55 },
        { "value": "Eriksson", "rarityWeight": 60 }
      ],
      "celtic": [
        { "value": "O'Brien", "rarityWeight": 50 },
        { "value": "MacLeod", "rarityWeight": 55 }
      ],
      "occupational": [
        { "value": "Smith", "rarityWeight": 30 },
        { "value": "Miller", "rarityWeight": 35 },
        { "value": "Fletcher", "rarityWeight": 40 }
      ]
    },
    
    "suffix": [
      { "value": "the Brave", "rarityWeight": 50 },
      { "value": "the Wise", "rarityWeight": 55 },
      { "value": "Dragonslayer", "rarityWeight": 80 },
      { "value": "Ironhand", "rarityWeight": 60 },
      { "value": "the Bold", "rarityWeight": 52 }
    ]
  },
  
  "patterns": [
    {
      "template": "{first_name}",
      "rarityWeight": 30,
      "socialClass": "common",
      "description": "Simple single name for commoners"
    },
    {
      "template": "{first_name} {surname}",
      "rarityWeight": 10,
      "description": "Standard full name"
    },
    {
      "template": "{title} {first_name}",
      "rarityWeight": 40,
      "requiresTitle": true,
      "description": "Title with first name only"
    },
    {
      "template": "{title} {first_name} {surname}",
      "rarityWeight": 35,
      "requiresTitle": true,
      "description": "Formal titled name"
    },
    {
      "template": "{first_name} {surname} {suffix}",
      "rarityWeight": 60,
      "description": "Name with epithet"
    },
    {
      "template": "{title} {first_name} {surname} {suffix}",
      "rarityWeight": 85,
      "requiresTitle": true,
      "description": "Legendary full formal name with epithet"
    }
  ]
}
```

---

## 📄 Phase 5: Create .cbconfig.json Files

### 5.1: npcs/.cbconfig.json

```json
{
  "name": "NPCs",
  "description": "Non-player character data including backgrounds, occupations, traits, and names",
  "icon": "👥",
  "expanded": true,
  "children": [
    {
      "name": "Catalog",
      "file": "catalog.json",
      "description": "NPC backgrounds and occupations",
      "icon": "📋",
      "expanded": false
    },
    {
      "name": "Traits",
      "file": "traits.json",
      "description": "Personality traits and quirks",
      "icon": "🎭",
      "expanded": false
    },
    {
      "name": "Names",
      "file": "names.json",
      "description": "Pattern-based name generation",
      "icon": "✏️",
      "expanded": false
    },
    {
      "name": "Dialogue",
      "directory": "dialogue",
      "description": "NPC dialogue templates and styles",
      "icon": "💬",
      "expanded": false
    }
  ]
}
```

### 5.2: npcs/dialogue/.cbconfig.json

```json
{
  "name": "Dialogue",
  "description": "NPC dialogue greetings, farewells, rumors, and speaking styles",
  "icon": "💬",
  "expanded": false,
  "children": [
    {
      "name": "Greetings",
      "file": "greetings.json",
      "description": "NPC greeting phrases",
      "icon": "👋"
    },
    {
      "name": "Farewells",
      "file": "farewells.json",
      "description": "NPC farewell phrases",
      "icon": "👋"
    },
    {
      "name": "Rumors",
      "file": "rumors.json",
      "description": "Rumors NPCs can share",
      "icon": "🗣️"
    },
    {
      "name": "Styles",
      "file": "styles.json",
      "description": "Dialogue speaking styles",
      "icon": "🎨"
    }
  ]
}
```

---

## 🔧 Phase 6: Code Updates

### 6.1: Update NpcGenerator.cs

**Location:** `Game.Core/Generators/NpcGenerator.cs`

**Key Changes:**
1. Load from new catalog.json structure
2. Separate background and occupation selection
3. Implement pattern-based name generation
4. Calculate shop inventory (core + dynamic)
5. Implement player economy system
6. Calculate bank gold using formula

**Pseudocode:**
```csharp
public NPC GenerateNPC()
{
    // 1. Select background
    var background = SelectFromWeightedList(catalog.backgrounds);
    
    // 2. Select occupation
    var occupation = SelectFromWeightedList(catalog.occupations);
    
    // 3. Determine if has shop
    bool hasShop = Random.NextDouble() < occupation.shopChance;
    
    // 4. Generate name using patterns
    var name = GenerateNameFromPattern(
        gender: Random.Choice(["male", "female"]),
        socialClass: background.socialClass,
        occupation: occupation
    );
    
    // 5. Select personality traits
    var traits = SelectTraits(count: Random.Next(1, 3));
    var quirks = SelectQuirks(count: Random.Next(0, 2));
    
    // 6. Calculate starting gold
    var gold = RollDice(background.startingGold) + RollDice(occupation.baseGold);
    
    // 7. Setup shop if applicable
    Shop shop = null;
    if (hasShop)
    {
        shop = GenerateShop(
            background: background,
            occupation: occupation,
            traits: traits
        );
    }
    
    return new NPC
    {
        Name = name,
        Background = background,
        Occupation = occupation,
        Traits = traits,
        Quirks = quirks,
        Gold = gold,
        Shop = shop
    };
}

private string GenerateNameFromPattern(string gender, string socialClass, Occupation occupation)
{
    // 1. Select pattern
    var pattern = SelectPatternFromWeightedList(nameData.patterns);
    
    // 2. Generate components
    var components = new Dictionary<string, string>();
    
    // Title (with soft filtering)
    if (pattern.template.Contains("{title}"))
    {
        var eligibleTitles = nameData.components.title
            .Where(t => t.gender == null || t.gender == gender)
            .Select(t => new {
                Title = t,
                AdjustedWeight = t.rarityWeight * t.weightMultiplier.GetValueOrDefault(socialClass, 1.0)
            });
        
        var selectedTitle = SelectFromWeightedList(eligibleTitles, t => t.AdjustedWeight);
        components["title"] = selectedTitle?.Title.value ?? "";
    }
    
    // First name
    var firstNameCategory = GetFirstNameCategory(gender, socialClass);
    components["first_name"] = SelectFromWeightedList(firstNameCategory).value;
    
    // Surname
    var surnameCategory = GetSurnameCategory(occupation);
    components["surname"] = SelectFromWeightedList(surnameCategory).value;
    
    // Suffix
    if (pattern.template.Contains("{suffix}"))
    {
        components["suffix"] = SelectFromWeightedList(nameData.components.suffix)?.value ?? "";
    }
    
    // 3. Replace pattern tokens
    var name = pattern.template;
    foreach (var kvp in components)
    {
        name = name.Replace($"{{{kvp.Key}}}", kvp.Value);
    }
    
    return name.Trim();
}

private Shop GenerateShop(Background background, Occupation occupation, List<Trait> traits)
{
    // Calculate bank gold using formula
    var baseGold = RollDice(occupation.baseGold);
    var backgroundGold = RollDice(background.startingGold);
    var bankGold = EvaluateFormula(
        occupation.shopInventory.economy.bankGoldFormula,
        new { baseGold, background.startingGold = backgroundGold }
    );
    
    // Apply trait modifiers
    var priceMultiplier = 1.0;
    var buyPriceMultiplier = occupation.shopInventory.economy.buyPriceMultiplier;
    
    foreach (var trait in traits)
    {
        if (trait.shopModifiers?.priceMultiplier != null)
            priceMultiplier *= trait.shopModifiers.priceMultiplier;
        if (trait.shopModifiers?.buyPriceMultiplier != null)
            buyPriceMultiplier *= trait.shopModifiers.buyPriceMultiplier;
    }
    
    // Generate core items
    var coreItems = occupation.shopInventory.coreItems.Select(ci => new ShopItem
    {
        ItemId = ci.item,
        Quantity = ci.baseQuantity == "infinite" ? int.MaxValue : int.Parse(ci.baseQuantity),
        RestocksDaily = ci.restockDaily,
        Source = ShopItemSource.Core
    }).ToList();
    
    // Generate dynamic items
    var dynamicItemCount = RollDice(occupation.shopInventory.dynamicItemCount);
    var dynamicItems = GenerateDynamicItems(
        categories: occupation.shopInventory.dynamicCategories,
        count: dynamicItemCount,
        background: background
    );
    
    return new Shop
    {
        Type = occupation.shopType,
        BankGold = bankGold,
        Items = coreItems.Concat(dynamicItems).ToList(),
        Economy = new ShopEconomy
        {
            AcceptsPlayerItems = occupation.shopInventory.economy.acceptsPlayerItems,
            AcceptedCategories = occupation.shopInventory.economy.acceptedCategories,
            BuyPriceMultiplier = buyPriceMultiplier,
            SellPriceMultiplier = priceMultiplier,
            ResellPriceMultiplier = occupation.shopInventory.economy.resellPriceMultiplier,
            PlayerItemDecayDaily = occupation.shopInventory.economy.playerItemDecayDaily,
            PlayerItemDecayDays = occupation.shopInventory.economy.playerItemDecayDays,
            MaxPlayerItems = occupation.shopInventory.economy.maxPlayerItems
        },
        PlayerSoldItems = new List<ShopItem>()
    };
}
```

### 6.2: Create ShopEconomy System

**Location:** `Game.Core/Services/ShopEconomyService.cs`

**Key Methods:**
```csharp
public class ShopEconomyService
{
    // Player sells item to merchant
    public bool SellToMerchant(NPC merchant, Item item, out int goldOffered)
    {
        if (!merchant.Shop.Economy.AcceptsPlayerItems)
        {
            goldOffered = 0;
            return false;
        }
        
        // Check category
        if (!merchant.Shop.Economy.AcceptedCategories.Contains("all") &&
            !merchant.Shop.Economy.AcceptedCategories.Contains(item.Category))
        {
            goldOffered = 0;
            return false;
        }
        
        // Check max items
        if (merchant.Shop.PlayerSoldItems.Count >= merchant.Shop.Economy.MaxPlayerItems)
        {
            goldOffered = 0;
            return false;
        }
        
        // Calculate price using rarity weight
        var basePrice = item.BasePrice;
        var rarityWeight = CalculateAccumulatedRarityWeight(item);
        var qualityMultiplier = 100.0 / rarityWeight;
        var itemValue = (int)(basePrice * qualityMultiplier);
        
        goldOffered = (int)(itemValue * merchant.Shop.Economy.BuyPriceMultiplier);
        
        // Check if merchant has enough gold
        if (merchant.Shop.BankGold < goldOffered)
        {
            return false;
        }
        
        return true;
    }
    
    public void CompleteSale(NPC merchant, Item item, int goldPaid)
    {
        // Deduct gold from merchant
        merchant.Shop.BankGold -= goldPaid;
        
        // Calculate resell price
        var basePrice = item.BasePrice;
        var rarityWeight = CalculateAccumulatedRarityWeight(item);
        var qualityMultiplier = 100.0 / rarityWeight;
        var itemValue = (int)(basePrice * qualityMultiplier);
        var resellPrice = (int)(itemValue * merchant.Shop.Economy.ResellPriceMultiplier);
        
        // Add to shop inventory
        merchant.Shop.PlayerSoldItems.Add(new ShopItem
        {
            Item = item,
            Quantity = 1,
            Price = resellPrice,
            Source = ShopItemSource.PlayerSold,
            DaysSinceAdded = 0
        });
    }
    
    // Daily shop updates
    public void ProcessDailyUpdate(NPC merchant)
    {
        // Restock core items
        foreach (var item in merchant.Shop.Items.Where(i => i.RestocksDaily))
        {
            item.Quantity = item.BaseQuantity;
        }
        
        // Refresh dynamic items
        var dynamicItems = GenerateDynamicItems(
            merchant.Occupation.ShopInventory.DynamicCategories,
            RollDice(merchant.Occupation.ShopInventory.DynamicItemCount),
            merchant.Background
        );
        
        // Remove old dynamic items, add new ones
        merchant.Shop.Items.RemoveAll(i => i.Source == ShopItemSource.Dynamic);
        merchant.Shop.Items.AddRange(dynamicItems);
        
        // Decay player-sold items
        foreach (var item in merchant.Shop.PlayerSoldItems)
        {
            item.DaysSinceAdded++;
            
            // Apply daily decay
            var decayAmount = (int)(item.Price * merchant.Shop.Economy.PlayerItemDecayDaily);
            item.Price = Math.Max(1, item.Price - decayAmount);
            
            // Remove if too old
            if (item.DaysSinceAdded >= merchant.Shop.Economy.PlayerItemDecayDays)
            {
                merchant.Shop.PlayerSoldItems.Remove(item);
            }
        }
        
        // Restock bank gold
        if (merchant.Occupation.ShopInventory.Economy.BankGoldRestockDaily)
        {
            var baseGold = RollDice(merchant.Occupation.BaseGold);
            var backgroundGold = RollDice(merchant.Background.StartingGold);
            merchant.Shop.BankGold = EvaluateFormula(
                merchant.Occupation.ShopInventory.Economy.BankGoldFormula,
                new { baseGold, background.startingGold = backgroundGold }
            );
        }
    }
    
    private int CalculateAccumulatedRarityWeight(Item item)
    {
        // Sum rarity weights from all components
        int total = 100; // Base weight
        
        foreach (var component in item.Components)
        {
            total += component.RarityWeight;
        }
        
        return total;
    }
}
```

---

## 📋 Phase 7: Migration Checklist

### 7.1: Data Migration Steps

- [ ] Create `npcs/catalog.json`
  - [ ] Migrate data from `personalities/backgrounds.json`
  - [ ] Migrate data from `occupations/occupations.json`
  - [ ] Add shop economy configuration
  - [ ] Add bank gold formulas
  - [ ] Fix `notes` placement (move to metadata)

- [ ] Create `npcs/traits.json`
  - [ ] Migrate data from `personalities/personality_traits.json`
  - [ ] Migrate data from `personalities/quirks.json`
  - [ ] Add shop modifier fields
  - [ ] Fix `notes` placement

- [ ] Create `npcs/names.json`
  - [ ] Migrate first names from `names/first_names.json`
  - [ ] Migrate last names from `names/last_names.json`
  - [ ] Add pattern-based structure
  - [ ] Add title components with soft filtering
  - [ ] Add suffix components
  - [ ] Define name patterns
  - [ ] Fix `notes` placement

- [ ] Create `npcs/dialogue/` directory
  - [ ] Move `dialogue/greetings.json`
  - [ ] Move `dialogue/farewells.json`
  - [ ] Move `dialogue/rumors.json`
  - [ ] Rename `dialogue/templates.json` to `styles.json`
  - [ ] Fix `notes` placement in all dialogue files
  - [ ] Create `dialogue/.cbconfig.json`

- [ ] Create `.cbconfig.json` files
  - [ ] Create `npcs/.cbconfig.json`
  - [ ] Create `npcs/dialogue/.cbconfig.json`

- [ ] Delete old files
  - [ ] Delete `npcs/names/` directory
  - [ ] Delete `npcs/occupations/` directory
  - [ ] Delete `npcs/personalities/` directory

### 7.2: Code Migration Steps

- [ ] Update `NpcGenerator.cs`
  - [ ] Load new catalog structure
  - [ ] Implement separate background/occupation selection
  - [ ] Implement pattern-based name generation
  - [ ] Implement soft filtering for titles
  - [ ] Implement shop generation (core + dynamic)
  - [ ] Calculate bank gold using formula

- [ ] Create `ShopEconomyService.cs`
  - [ ] Implement `SellToMerchant()`
  - [ ] Implement `CompleteSale()`
  - [ ] Implement `ProcessDailyUpdate()`
  - [ ] Implement rarity weight price calculation
  - [ ] Implement item decay system

- [ ] Update models
  - [ ] Add `Shop` model with economy fields
  - [ ] Add `ShopItem` model with source tracking
  - [ ] Add `ShopEconomy` model
  - [ ] Add `ShopItemSource` enum (Core, Dynamic, PlayerSold)

- [ ] Update ContentBuilder
  - [ ] Update tree configuration for new structure
  - [ ] Update `NameCatalogEditorViewModel` for pattern-based names
  - [ ] Test loading new files

### 7.3: Testing Steps

- [ ] Unit tests
  - [ ] Test background selection
  - [ ] Test occupation selection
  - [ ] Test name pattern generation
  - [ ] Test soft filtering (title weight multipliers)
  - [ ] Test shop inventory generation (core + dynamic)
  - [ ] Test bank gold formula calculation
  - [ ] Test player item sale
  - [ ] Test item decay
  - [ ] Test rarity weight pricing

- [ ] Integration tests
  - [ ] Test full NPC generation
  - [ ] Test shop daily updates
  - [ ] Test player economy flow
  - [ ] Test ContentBuilder loading

- [ ] Manual testing
  - [ ] Generate 100 NPCs, verify variety
  - [ ] Test shop interactions
  - [ ] Test item decay over 7 days
  - [ ] Verify bank gold limits work

---

## 🎯 Implementation Priority

### Phase 1: Core Structure (HIGH PRIORITY)
1. Create catalog.json (backgrounds + occupations)
2. Create traits.json
3. Create names.json
4. Move dialogue files
5. Create .cbconfig files

### Phase 2: Basic Code (HIGH PRIORITY)
1. Update NpcGenerator for new catalog structure
2. Implement pattern-based name generation
3. Update ContentBuilder configuration

### Phase 3: Shop System (MEDIUM PRIORITY)
1. Implement core + dynamic inventory
2. Calculate bank gold using formulas
3. Test shop generation

### Phase 4: Player Economy (LOW PRIORITY - Future)
1. Create ShopEconomyService
2. Implement sell/buy mechanics
3. Implement decay system
4. Implement rarity weight pricing

### Phase 5: Testing & Cleanup (ONGOING)
1. Write unit tests
2. Update existing tests
3. Delete old files
4. Update documentation

---

## 📊 Success Metrics

### Data Quality
- ✅ All `notes` moved to metadata
- ✅ All files follow v4.0 schema
- ✅ Backgrounds and occupations separated
- ✅ Pattern-based names work correctly

### Code Quality
- ✅ NPC generation produces varied results
- ✅ Shop inventory has core + dynamic items
- ✅ Bank gold calculated correctly
- ✅ All tests passing

### Player Experience
- ✅ NPC names feel natural and varied
- ✅ Shops have reliable core items + interesting variety
- ✅ Merchant gold feels balanced
- ✅ Price differences based on quality are noticeable

---

## 🚀 Next Steps

**Immediate Action:**
1. Create catalog.json structure
2. Begin migrating background data
3. Begin migrating occupation data

**Would you like me to:**
- Start creating the catalog.json file with migrated data?
- Create the names.json structure?
- Begin updating NpcGenerator.cs?

**Let me know which phase you'd like to tackle first!** 🎮
