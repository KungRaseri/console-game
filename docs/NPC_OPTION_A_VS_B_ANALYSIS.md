# NPC Structure Discussion: Option A vs Option B

**Date**: 2025-12-18
**Updated**: Based on names.json pattern analysis & background+occupation separation

---

## Key Insights from Weapons/Enemies Pattern

### 1. **names.json Structure (Pattern Generation)**

Weapons and enemies both use **pattern-based name generation** with components:

**Weapons Pattern:**
```json
{
  "metadata": {
    "type": "pattern_generation",
    "componentKeys": ["prefix", "material", "quality", "descriptive", "suffix"],
    "patternTokens": ["base", "prefix", "material", "quality", "suffix"]
  },
  "components": {
    "prefix": [
      { "value": "Rusty", "rarityWeight": 50, "traits": {...} },
      { "value": "Enchanted", "rarityWeight": 15, "traits": {...} }
    ],
    "material": [...],
    "suffix": [...]
  },
  "patterns": [
    { "template": "{prefix} {base}", "rarityWeight": 10 },
    { "template": "{material} {base} of {suffix}", "rarityWeight": 25 }
  ]
}
```

**Enemies Pattern:**
```json
{
  "metadata": {
    "type": "pattern_generation",
    "componentKeys": ["size", "descriptive", "origin", "title"],
    "patternTokens": ["base", "size", "descriptive", "origin"]
  },
  "components": {
    "size": [
      { "value": "Dire", "rarityWeight": 25 },
      { "value": "Alpha", "rarityWeight": 30 }
    ],
    "descriptive": [...],
    "origin": [...]
  },
  "patterns": [
    { "template": "{size} {base}", "rarityWeight": 15 },
    { "template": "{descriptive} {base} from {origin}", "rarityWeight": 20 }
  ]
}
```

### 2. **catalog.json Structure (Base Types)**

**Weapons:**
- `weapon_types` → `swords` → `items` (Longsword, Greatsword)
- Types have shared `traits` (damageType, slot, category)
- Items have individual stats (damage, weight, value, rarityWeight)

**Enemies:**
- `beast_types` → `wolves` → `items` (Wolf, Timber Wolf, Frost Wolf)
- Types have shared `traits` (category, size, behavior)
- Items have individual stats (health, attack, defense, rarityWeight)

---

## Critical Realization: Background ≠ Occupation

You're absolutely right! NPCs can have BOTH:
- **Background**: Their past/history (Former Soldier, Orphan, Noble-born)
- **Occupation**: Their current job (Blacksmith, Merchant, Guard)

**Example:**
- "Former Soldier, now Blacksmith"
- "Orphan working as Thief"
- "Noble-born Wizard"
- "Refugee turned Merchant"

This means **backgrounds and occupations should be SEPARATE components**, not merged!

---

## Revised Catalog Structure

### Option 1: Backgrounds + Occupations as Separate Components (BETTER)

```json
{
  "metadata": {
    "description": "NPC catalog with backgrounds and occupations as separate components",
    "type": "npc_catalog"
  },
  "backgrounds": {
    "common_folk": {
      "traits": {
        "socialClass": "common",
        "startingWealthModifier": 0.5
      },
      "items": [
        {
          "name": "FarmRaised",
          "displayName": "Farm-Raised",
          "description": "Grew up working the land",
          "rarityWeight": 5,
          "skillBonuses": ["agriculture", "animal_handling"],
          "startingGoldModifier": -2
        },
        {
          "name": "Orphan",
          "displayName": "Orphan",
          "description": "Grew up without parents",
          "rarityWeight": 20,
          "skillBonuses": ["survival", "streetwise"],
          "startingGoldModifier": -5
        }
      ]
    },
    "noble_born": {
      "traits": {
        "socialClass": "noble",
        "startingWealthModifier": 3.0
      },
      "items": [
        {
          "name": "Noble",
          "displayName": "Noble-Born",
          "description": "Born into wealth and privilege",
          "rarityWeight": 60,
          "skillBonuses": ["etiquette", "persuasion"],
          "startingGoldModifier": 20
        }
      ]
    },
    "military": {
      "traits": {
        "socialClass": "middle",
        "startingWealthModifier": 1.0
      },
      "items": [
        {
          "name": "FormerSoldier",
          "displayName": "Former Soldier",
          "description": "Served in the military and retired or deserted",
          "rarityWeight": 30,
          "skillBonuses": ["melee_combat", "discipline"],
          "startingGoldModifier": 0,
          "startingItems": ["worn_armor", "military_medallion"]
        }
      ]
    }
  },
  "occupations": {
    "craftsmen": {
      "traits": {
        "occupationType": "skilled_labor",
        "incomeLevel": "moderate"
      },
      "items": [
        {
          "name": "Blacksmith",
          "displayName": "Blacksmith",
          "description": "Forges weapons and armor",
          "rarityWeight": 20,
          "skillBonuses": ["smithing", "crafting"],
          "baseGold": "3d10",
          "typicalItems": ["hammer", "tongs", "leather_apron"],
          "shopType": "smithy"
        },
        {
          "name": "Healer",
          "displayName": "Healer",
          "description": "Practices medicine and herbalism",
          "rarityWeight": 25,
          "skillBonuses": ["healing", "herbalism"],
          "baseGold": "2d10",
          "typicalItems": ["healing_kit", "herbs"],
          "shopType": "apothecary"
        }
      ]
    },
    "merchants": {
      "traits": {
        "occupationType": "commerce",
        "incomeLevel": "variable"
      },
      "items": [
        {
          "name": "Merchant",
          "displayName": "Merchant",
          "description": "Trades goods for profit",
          "rarityWeight": 10,
          "skillBonuses": ["persuasion", "appraisal"],
          "baseGold": "5d10",
          "shopType": "general_store"
        }
      ]
    },
    "criminal": {
      "traits": {
        "occupationType": "illegal",
        "incomeLevel": "variable"
      },
      "items": [
        {
          "name": "Thief",
          "displayName": "Thief",
          "description": "Lives by stealth and cunning",
          "rarityWeight": 30,
          "skillBonuses": ["stealth", "lockpicking"],
          "baseGold": "2d10",
          "typicalItems": ["lockpicks", "dagger"]
        }
      ]
    }
  }
}
```

**NPC Generation:**
```csharp
// Generate NPC:
1. Pick background: "Former Soldier" (military background)
2. Pick occupation: "Blacksmith" (craftsman occupation)
3. Result: "Former Soldier Blacksmith" with combined skills/gold/items
```

### Option 2: Single "NPC Types" Combining Both (SIMPLER but LESS FLEXIBLE)

```json
{
  "npc_types": {
    "soldier_blacksmith": {
      "background": "Former Soldier",
      "occupation": "Blacksmith",
      // ... combined data
    }
  }
}
```

**Problem:** Explosion of combinations (10 backgrounds × 20 occupations = 200 entries!)

---

## Revised names.json Structure (With Prefixes/Suffixes)

Following the weapons/enemies pattern:

```json
{
  "metadata": {
    "description": "NPC name generation with pattern-based system",
    "version": "4.0",
    "type": "pattern_generation",
    "supportsTraits": false,
    "componentKeys": ["title", "first_name", "surname", "suffix"],
    "patternTokens": ["title", "first_name", "surname", "suffix"],
    "totalPatterns": 12,
    "raritySystem": "weight-based"
  },
  "components": {
    "title": [
      { "value": "Sir", "rarityWeight": 40, "gender": "male", "socialClass": "noble" },
      { "value": "Lady", "rarityWeight": 40, "gender": "female", "socialClass": "noble" },
      { "value": "Lord", "rarityWeight": 50, "gender": "male", "socialClass": "noble" },
      { "value": "Master", "rarityWeight": 30, "gender": "male", "socialClass": "craftsman" },
      { "value": "Mistress", "rarityWeight": 30, "gender": "female", "socialClass": "craftsman" },
      { "value": "Captain", "rarityWeight": 35, "socialClass": "military" },
      { "value": "Brother", "rarityWeight": 25, "gender": "male", "socialClass": "religious" },
      { "value": "Sister", "rarityWeight": 25, "gender": "female", "socialClass": "religious" }
    ],
    "first_name": {
      "male": {
        "common": [
          { "value": "Aric", "rarityWeight": 5 },
          { "value": "Cole", "rarityWeight": 5 },
          { "value": "Drake", "rarityWeight": 6 }
        ],
        "noble": [
          { "value": "Aldric", "rarityWeight": 30 },
          { "value": "Cedric", "rarityWeight": 28 },
          { "value": "Percival", "rarityWeight": 35 }
        ],
        "mystical": [
          { "value": "Balthazar", "rarityWeight": 45 },
          { "value": "Eldrin", "rarityWeight": 40 }
        ]
      },
      "female": {
        "common": [
          { "value": "Adeline", "rarityWeight": 5 },
          { "value": "Belle", "rarityWeight": 5 },
          { "value": "Clara", "rarityWeight": 6 }
        ],
        "noble": [
          { "value": "Anastasia", "rarityWeight": 35 },
          { "value": "Beatrice", "rarityWeight": 30 }
        ],
        "mystical": [
          { "value": "Seraphina", "rarityWeight": 45 },
          { "value": "Morgana", "rarityWeight": 42 }
        ]
      }
    },
    "surname": {
      "fantasy": [
        { "value": "Ironforge", "rarityWeight": 20 },
        { "value": "Stormwind", "rarityWeight": 22 },
        { "value": "Brightblade", "rarityWeight": 25 }
      ],
      "nordic": [
        { "value": "Bjornsson", "rarityWeight": 15 },
        { "value": "Eriksson", "rarityWeight": 12 }
      ],
      "celtic": [
        { "value": "O'Brien", "rarityWeight": 18 },
        { "value": "MacGregor", "rarityWeight": 20 }
      ],
      "occupational": [
        { "value": "Smith", "rarityWeight": 8 },
        { "value": "Fletcher", "rarityWeight": 10 },
        { "value": "Cooper", "rarityWeight": 10 }
      ]
    },
    "suffix": [
      { "value": "the Brave", "rarityWeight": 35 },
      { "value": "the Wise", "rarityWeight": 40 },
      { "value": "the Elder", "rarityWeight": 50 },
      { "value": "the Red", "rarityWeight": 25 },
      { "value": "the Bold", "rarityWeight": 30 },
      { "value": "Dragonslayer", "rarityWeight": 60 },
      { "value": "Ironhand", "rarityWeight": 45 },
      { "value": "the Wanderer", "rarityWeight": 38 }
    ]
  },
  "patterns": [
    {
      "template": "{first_name} {surname}",
      "description": "Standard name",
      "rarityWeight": 5
    },
    {
      "template": "{title} {first_name} {surname}",
      "description": "Titled name",
      "rarityWeight": 30
    },
    {
      "template": "{first_name} {surname} {suffix}",
      "description": "Name with epithet",
      "rarityWeight": 40
    },
    {
      "template": "{title} {first_name} {surname} {suffix}",
      "description": "Full formal name",
      "rarityWeight": 60
    },
    {
      "template": "{first_name} {suffix}",
      "description": "Single name with epithet",
      "rarityWeight": 35
    },
    {
      "template": "{first_name}",
      "description": "Single name only",
      "rarityWeight": 3
    }
  ]
}
```

**Generated Names Examples:**
- "Aric Smith" (common)
- "Sir Aldric Ironforge" (noble with title)
- "Balthazar the Wise" (wizard with epithet)
- "Lady Seraphina Stormwind Dragonslayer" (legendary hero)
- "Cole Fletcher" (craftsman surname)

---

## Option A vs Option B Analysis (REVISED)

### Option A: Single `npcs/` Directory ✅ **RECOMMENDED**

```
npcs/
├── .cbconfig.json
├── catalog.json        ← Separate backgrounds + occupations
├── traits.json         ← Personality + quirks
├── names.json          ← Pattern-based with titles/surnames/suffixes
└── dialogue/
    ├── .cbconfig.json
    ├── greetings.json
    ├── farewells.json
    ├── rumors.json
    └── styles.json
```

**catalog.json Structure:**
```json
{
  "backgrounds": {
    "common_folk": { "items": [...] },
    "noble_born": { "items": [...] },
    "military": { "items": [...] }
  },
  "occupations": {
    "craftsmen": { "items": [...] },
    "merchants": { "items": [...] },
    "scholars": { "items": [...] }
  }
}
```

**Pros:**
- ✅ Background and occupation are independent (as they should be!)
- ✅ Flexible combinations (Former Soldier + Blacksmith)
- ✅ No data duplication
- ✅ Simpler file structure
- ✅ Matches weapons/enemies pattern (one category, multiple type groups)

**Cons:**
- Generator needs to pick TWO components (background + occupation)
- Need logic to ensure compatible combinations

---

### Option B: Multiple Category Directories (Like enemies/)

```
npcs/
├── commoners/
│   ├── .cbconfig.json
│   ├── catalog.json    ← Only commoner backgrounds + occupations
│   ├── traits.json
│   ├── names.json
│   └── dialogue/
├── nobles/
│   ├── .cbconfig.json
│   ├── catalog.json    ← Only noble backgrounds + occupations
│   ├── traits.json
│   ├── names.json
│   └── dialogue/
└── criminals/
    ├── catalog.json
    ├── traits.json
    ├── names.json
    └── dialogue/
```

**Pros:**
- Category-specific customization (noble names differ from commoner names)
- Each category can have unique dialogue styles
- Easier to balance rarity within social class

**Cons:**
- ❌ Data duplication (many backgrounds/occupations span multiple classes)
- ❌ More complex file structure
- ❌ Harder to manage cross-class NPCs (Noble Thief? Commoner Wizard?)
- ❌ Background/occupation still need to be separate within each catalog

---

## Recommendation: **Option A with Separated Components**

### Final Proposed Structure:

```
npcs/
├── .cbconfig.json
├── catalog.json        ← backgrounds{} + occupations{} as separate top-level keys
├── traits.json         ← personality_traits{} + quirks{}
├── names.json          ← Pattern-based with components{title, first_name, surname, suffix}
└── dialogue/
    ├── .cbconfig.json
    ├── greetings.json
    ├── farewells.json
    ├── rumors.json
    └── styles.json
```

### catalog.json Example:

```json
{
  "metadata": {
    "description": "NPC backgrounds and occupations catalog",
    "type": "npc_catalog",
    "componentKeys": ["backgrounds", "occupations"]
  },
  "backgrounds": {
    "common_folk": {
      "traits": { "socialClass": "common", "startingWealthModifier": 0.5 },
      "items": [
        { "name": "FarmRaised", "rarityWeight": 5, "skillBonuses": ["agriculture"] },
        { "name": "Orphan", "rarityWeight": 20, "skillBonuses": ["survival"] }
      ]
    },
    "military": {
      "traits": { "socialClass": "middle", "combatBonus": 2 },
      "items": [
        { "name": "FormerSoldier", "rarityWeight": 30, "skillBonuses": ["melee_combat"] }
      ]
    },
    "noble_born": {
      "traits": { "socialClass": "noble", "startingWealthModifier": 3.0 },
      "items": [
        { "name": "Noble", "rarityWeight": 60, "skillBonuses": ["etiquette"] }
      ]
    }
  },
  "occupations": {
    "craftsmen": {
      "traits": { "occupationType": "skilled_labor", "incomeLevel": "moderate" },
      "items": [
        { "name": "Blacksmith", "rarityWeight": 20, "skillBonuses": ["smithing"], "baseGold": "3d10" },
        { "name": "Healer", "rarityWeight": 25, "skillBonuses": ["healing"], "baseGold": "2d10" }
      ]
    },
    "merchants": {
      "traits": { "occupationType": "commerce", "incomeLevel": "variable" },
      "items": [
        { "name": "Merchant", "rarityWeight": 10, "skillBonuses": ["persuasion"], "baseGold": "5d10" }
      ]
    },
    "warriors": {
      "traits": { "occupationType": "combat", "incomeLevel": "moderate" },
      "items": [
        { "name": "Guard", "rarityWeight": 28, "skillBonuses": ["melee_combat"], "baseGold": "2d10" }
      ]
    }
  }
}
```

### NPC Generation Flow:

```csharp
// Step 1: Pick background
var background = PickFromWeightedList(catalog.backgrounds);
// Result: "FormerSoldier" with skills: [melee_combat], socialClass: "middle"

// Step 2: Pick occupation
var occupation = PickFromWeightedList(catalog.occupations);
// Result: "Blacksmith" with skills: [smithing], baseGold: "3d10"

// Step 3: Combine
var npc = new NPC {
    Background = background,
    Occupation = occupation,
    Skills = background.skills + occupation.skills, // [melee_combat, smithing]
    Gold = CalculateGold(background.modifier, occupation.baseGold),
    SocialClass = background.socialClass
};

// Step 4: Pick personality from traits.json
var personality = PickFromWeightedList(traits.personality_traits);

// Step 5: Generate name from names.json patterns
var name = GenerateNameFromPattern(names.patterns, names.components);
// Result: "Gareth Ironforge" or "Master Gareth Smith"

// Step 6: Pick dialogue based on personality
var greeting = PickDialogue(dialogue.greetings, personality);
```

---

## Key Decisions:

### 1. ✅ **Use Option A (Single npcs/ directory)**
- Simpler structure
- Follows weapons pattern (one category)
- No data duplication

### 2. ✅ **Separate backgrounds and occupations in catalog.json**
- Two top-level keys: `backgrounds{}` and `occupations{}`
- Generator picks one from each
- Allows realistic combinations (Former Soldier Blacksmith)

### 3. ✅ **Use pattern-based names.json**
- Match weapons/enemies structure
- Components: title, first_name, surname, suffix
- Patterns combine components with rarityWeight
- Examples: "Sir Aldric Ironforge", "Cole the Bold"

### 4. ✅ **Keep dialogue as subfolder**
- `npcs/dialogue/` with separate files
- Already well-organized
- Can reference personality traits from traits.json

---

## Questions for Confirmation:

1. **Agree that backgrounds and occupations should be separate?**
   - Allows combinations like "Orphan Merchant" or "Noble Wizard"

2. **Should names.json include gender-specific first names?**
   - Current proposal: `first_name.male.common`, `first_name.female.noble`

3. **Should titles in names.json auto-match social class?**
   - Example: "Sir" only for nobles, "Master" for craftsmen

4. **How to handle incompatible combinations?**
   - Should "Orphan Noble" be allowed?
   - Should generator have compatibility rules?

---

**Ready to proceed with Option A + separated backgrounds/occupations?** 🚀
