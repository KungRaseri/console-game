# NPC Structure Reorganization Proposal

**Date**: 2025-12-18
**Goal**: Align NPC structure with items/enemies pattern for consistency

---

## Current Pattern Analysis

### Items Structure (weapons/armor/consumables)

```
items/weapons/
├── .cbconfig.json
├── catalog.json        ← Base types with traits + items
└── names.json          ← Naming patterns
```

**catalog.json Pattern:**

```json
{
  "metadata": { "type": "item_catalog", ... },
  "weapon_types": {
    "swords": {
      "traits": {
        "damageType": "slashing",
        "slot": "mainhand",
        "category": "melee_one_handed"
      },
      "items": [
        { "name": "Longsword", "damage": "1d8", "rarityWeight": 5 },
        { "name": "Greatsword", "damage": "2d6", "rarityWeight": 10 }
      ]
    }
  }
}
```

### Enemies Structure (beasts/dragons/undead)

```
enemies/beasts/
├── .cbconfig.json
├── catalog.json        ← Beast types with traits + items
├── abilities.json      ← Special abilities/attacks
└── names.json          ← Naming patterns
```

**catalog.json Pattern:**

```json
{
  "metadata": { "type": "item_catalog", ... },
  "beast_types": {
    "wolves": {
      "traits": {
        "category": "beast",
        "size": "medium",
        "behavior": "pack",
        "damageType": "piercing"
      },
      "items": [
        { "name": "Wolf", "health": 30, "attack": 8, "rarityWeight": 5 },
        { "name": "Timber Wolf", "health": 40, "rarityWeight": 8 }
      ]
    }
  }
}
```

---

## Current NPC Structure (SCATTERED)

```
npcs/
├── dialogue/
│   ├── dialogue_styles.json
│   ├── greetings.json
│   ├── farewells.json
│   ├── rumors.json
│   └── templates.json
├── names/
│   ├── first_names.json
│   └── last_names.json
├── occupations/
│   └── occupations.json
└── personalities/
    ├── backgrounds.json
    ├── personality_traits.json
    └── quirks.json
```

**Problems:**

- ❌ No central `catalog.json` for NPC types
- ❌ Backgrounds and Occupations are separate (should be catalog-like)
- ❌ Personalities/Quirks scattered (cosmetic data)
- ❌ Dialogue files don't follow pattern
- ❌ No clear NPC "types" or categories

---

## Proposed NPC Structure (ALIGNED)

### Option A: Single NPC Category (Simpler)

```
npcs/
├── .cbconfig.json
├── catalog.json        ← NPC types with backgrounds/occupations as items
├── traits.json         ← Personality traits + quirks (cosmetic)
├── dialogue.json       ← All dialogue templates consolidated
└── names.json          ← First + last names combined
```

### Option B: Multiple NPC Categories (Like enemies)

```
npcs/
├── commoners/
│   ├── .cbconfig.json
│   ├── catalog.json    ← Farmers, merchants, laborers
│   ├── traits.json     ← Personality + quirks
│   ├── dialogue.json   ← Category-specific dialogue
│   └── names.json
├── nobles/
│   ├── .cbconfig.json
│   ├── catalog.json    ← Lords, knights, courtiers
│   ├── traits.json
│   ├── dialogue.json
│   └── names.json
├── skilled/
│   ├── .cbconfig.json
│   ├── catalog.json    ← Blacksmiths, healers, scholars
│   ├── traits.json
│   ├── dialogue.json
│   └── names.json
└── adventurers/
    ├── .cbconfig.json
    ├── catalog.json    ← Mercenaries, bounty hunters, pirates
    ├── traits.json
    ├── dialogue.json
    └── names.json
```

---

## Recommended Structure: **Option A (Unified NPC)**

**Reasoning:**

- NPCs are more homogeneous than enemies (no inherent "wolf vs dragon" split)
- Social class can be a trait, not a category
- Simpler for ContentBuilder tree navigation
- Easier to manage cross-class dialogue/traits

---

## Proposed File Structures

### 1. `npcs/catalog.json` (NEW - combines backgrounds + occupations)

```json
{
  "metadata": {
    "description": "NPC catalog with backgrounds and occupations",
    "version": "3.0",
    "last_updated": "2025-12-18",
    "type": "npc_catalog",
    "total_types": 8,
    "usage": "Base catalog for NPC generation with social class traits",
    "notes": [
      "Backgrounds + occupations combined as NPC 'types'",
      "Each type has traits that affect dialogue, stats, items",
      "Use rarityWeight for procedural generation frequency"
    ]
  },
  "npc_types": {
    "commoners": {
      "traits": {
        "socialClass": "common",
        "wealthLevel": "poor",
        "educationLevel": "basic",
        "influenceLevel": "none"
      },
      "items": [
        {
          "name": "Farmer",
          "occupation": "Farmer",
          "background": "Common Folk",
          "description": "Works the land for a living",
          "rarityWeight": 5,
          "skills": ["agriculture", "animal_handling"],
          "startingGold": "1d10",
          "typicalItems": ["pitchfork", "hoe", "simple_clothes"]
        },
        {
          "name": "Laborer",
          "occupation": "Laborer",
          "background": "Common Folk",
          "description": "Performs manual work",
          "rarityWeight": 8,
          "skills": ["strength", "endurance"],
          "startingGold": "1d6",
          "typicalItems": ["shovel", "simple_clothes"]
        }
      ]
    },
    "craftsmen": {
      "traits": {
        "socialClass": "middle",
        "wealthLevel": "moderate",
        "educationLevel": "specialized",
        "influenceLevel": "local"
      },
      "items": [
        {
          "name": "Blacksmith",
          "occupation": "Blacksmith",
          "background": "Skilled Professional",
          "description": "Forges weapons and armor",
          "rarityWeight": 20,
          "skills": ["smithing", "crafting"],
          "startingGold": "3d10",
          "typicalItems": ["hammer", "tongs", "leather_apron"]
        },
        {
          "name": "Healer",
          "occupation": "Healer",
          "background": "Skilled Professional",
          "description": "Practiced medicine and herbalism",
          "rarityWeight": 25,
          "skills": ["healing", "herbalism"],
          "startingGold": "2d10",
          "typicalItems": ["healing_kit", "herbs", "simple_robes"]
        }
      ]
    },
    "scholars": {
      "traits": {
        "socialClass": "middle",
        "wealthLevel": "comfortable",
        "educationLevel": "advanced",
        "influenceLevel": "regional"
      },
      "items": [
        {
          "name": "Wizard",
          "occupation": "Wizard",
          "background": "Magical Scholar",
          "description": "Studied arcane magic extensively",
          "rarityWeight": 45,
          "skills": ["arcane_magic", "knowledge_arcana"],
          "startingGold": "5d10",
          "typicalItems": ["staff", "spellbook", "robes"]
        },
        {
          "name": "Sage",
          "occupation": "Sage",
          "background": "Keeper of Knowledge",
          "description": "Keeper of ancient knowledge",
          "rarityWeight": 50,
          "skills": ["knowledge_history", "knowledge_religion"],
          "startingGold": "4d10",
          "typicalItems": ["books", "scrolls", "reading_glasses"]
        }
      ]
    },
    "warriors": {
      "traits": {
        "socialClass": "middle",
        "wealthLevel": "moderate",
        "educationLevel": "martial",
        "influenceLevel": "local"
      },
      "items": [
        {
          "name": "Guard",
          "occupation": "Guard",
          "background": "Former Soldier",
          "description": "Protected city or noble house",
          "rarityWeight": 28,
          "skills": ["melee_combat", "perception"],
          "startingGold": "2d10",
          "typicalItems": ["spear", "shield", "chainmail"]
        },
        {
          "name": "Mercenary",
          "occupation": "Mercenary",
          "background": "Sell-sword",
          "description": "Sold sword skills to highest bidder",
          "rarityWeight": 42,
          "skills": ["melee_combat", "survival"],
          "startingGold": "3d10",
          "typicalItems": ["sword", "leather_armor", "backpack"]
        }
      ]
    },
    "nobility": {
      "traits": {
        "socialClass": "noble",
        "wealthLevel": "wealthy",
        "educationLevel": "advanced",
        "influenceLevel": "national"
      },
      "items": [
        {
          "name": "Noble",
          "occupation": "Noble",
          "background": "Born to Privilege",
          "description": "Born into wealth and privilege",
          "rarityWeight": 60,
          "skills": ["persuasion", "etiquette"],
          "startingGold": "10d10",
          "typicalItems": ["fine_clothes", "jewelry", "signet_ring"]
        },
        {
          "name": "Knight",
          "occupation": "Knight",
          "background": "Sworn Warrior",
          "description": "Sworn warrior of noble rank",
          "rarityWeight": 55,
          "skills": ["melee_combat", "riding"],
          "startingGold": "8d10",
          "typicalItems": ["longsword", "plate_armor", "warhorse"]
        }
      ]
    },
    "criminals": {
      "traits": {
        "socialClass": "underworld",
        "wealthLevel": "variable",
        "educationLevel": "street_smart",
        "influenceLevel": "underground"
      },
      "items": [
        {
          "name": "Thief",
          "occupation": "Thief",
          "background": "Criminal",
          "description": "Lived by stealth and cunning",
          "rarityWeight": 30,
          "skills": ["stealth", "lockpicking"],
          "startingGold": "2d10",
          "typicalItems": ["dagger", "lockpicks", "dark_cloak"]
        },
        {
          "name": "Smuggler",
          "occupation": "Smuggler",
          "background": "Outlaw",
          "description": "Dealt in contraband goods",
          "rarityWeight": 38,
          "skills": ["deception", "stealth"],
          "startingGold": "3d10",
          "typicalItems": ["shortsword", "hidden_pouches"]
        }
      ]
    },
    "exiles": {
      "traits": {
        "socialClass": "outcast",
        "wealthLevel": "poor",
        "educationLevel": "variable",
        "influenceLevel": "none"
      },
      "items": [
        {
          "name": "Refugee",
          "occupation": "None",
          "background": "War Survivor",
          "description": "Fled from war or disaster",
          "rarityWeight": 25,
          "skills": ["survival"],
          "startingGold": "1d4",
          "typicalItems": ["tattered_clothes", "small_knife"]
        },
        {
          "name": "Exile",
          "occupation": "Former Noble",
          "background": "Banished",
          "description": "Banished from their homeland",
          "rarityWeight": 40,
          "skills": ["survival", "etiquette"],
          "startingGold": "1d10",
          "typicalItems": ["worn_noble_clothes", "family_heirloom"]
        }
      ]
    },
    "entertainers": {
      "traits": {
        "socialClass": "middle",
        "wealthLevel": "variable",
        "educationLevel": "artistic",
        "influenceLevel": "social"
      },
      "items": [
        {
          "name": "Bard",
          "occupation": "Entertainer",
          "background": "Performer",
          "description": "Performed for crowds as musician or actor",
          "rarityWeight": 18,
          "skills": ["performance", "persuasion"],
          "startingGold": "2d10",
          "typicalItems": ["lute", "colorful_clothes"]
        }
      ]
    }
  }
}
```

---

### 2. `npcs/traits.json` (NEW - combines personality + quirks)

```json
{
  "metadata": {
    "description": "NPC personality traits and quirks for characterization",
    "version": "3.0",
    "last_updated": "2025-12-18",
    "type": "npc_traits",
    "usage": "Cosmetic traits that add flavor to NPCs",
    "notes": [
      "Personalities affect dialogue tone but not stats",
      "Quirks are memorable behaviors that make NPCs unique",
      "NPCs typically have 1-2 personality traits + 0-1 quirks"
    ]
  },
  "personality_traits": {
    "social_positive": [
      {
        "name": "Friendly",
        "displayName": "Friendly",
        "description": "Warm and welcoming to strangers",
        "rarityWeight": 15,
        "dialogueModifiers": {
          "greetingWarmth": 85,
          "helpfulness": 75
        }
      }
    ],
    "social_negative": [
      {
        "name": "Gruff",
        "displayName": "Gruff",
        "description": "Rough-mannered but not hostile",
        "rarityWeight": 12,
        "dialogueModifiers": {
          "greetingWarmth": 30,
          "formality": "low"
        }
      }
    ]
    // ... other personality categories
  },
  "quirks": {
    "speech": [
      {
        "name": "SpeaksInRhymes",
        "displayName": "Speaks in Rhymes",
        "description": "Often talks in verse",
        "rarityWeight": 35,
        "effect": "cosmetic"
      }
    ],
    "behavioral": [
      {
        "name": "CollectsTrinkets",
        "displayName": "Collects Trinkets",
        "description": "Obsessively gathers small objects",
        "rarityWeight": 20,
        "effect": "cosmetic"
      }
    ]
    // ... other quirk categories
  }
}
```

---

### 3. `npcs/dialogue.json` (NEW - consolidated dialogue)

**Option 3a: Simple Template Collection**

```json
{
  "metadata": {
    "description": "Dialogue templates for NPC interactions",
    "version": "3.0",
    "type": "dialogue_templates",
    "usage": "Templates selected based on NPC personality + context"
  },
  "greetings": {
    "friendly": [
      { "text": "Hello there, traveler!", "rarityWeight": 5 },
      { "text": "Welcome, friend!", "rarityWeight": 8 }
    ],
    "hostile": [
      { "text": "What do you want?", "rarityWeight": 14 },
      { "text": "State your business.", "rarityWeight": 16 }
    ]
  },
  "farewells": {
    "friendly": [
      { "text": "Safe travels!", "rarityWeight": 5 }
    ],
    "hostile": [
      { "text": "Be gone.", "rarityWeight": 14 }
    ]
  },
  "rumors": {
    "danger": [
      { "text": "Bandits have been raiding...", "rarityWeight": 10 }
    ],
    "treasure": [
      { "text": "Legend speaks of treasure...", "rarityWeight": 30 }
    ]
  }
}
```

**Option 3b: Keep Separate (Current)**

- `dialogue/greetings.json`
- `dialogue/farewells.json`
- `dialogue/rumors.json`
- `dialogue/dialogue_styles.json`

**Recommendation**: Keep separate for now, but move to `npcs/dialogue/` subfolder

---

### 4. `npcs/names.json` (COMBINED - first + last names)

```json
{
  "metadata": {
    "description": "Complete NPC name catalog - first and last names",
    "version": "3.0",
    "type": "name_catalog",
    "usage": "Provides first names and surnames for NPC generation"
  },
  "first_names": {
    "male": {
      "common": ["Aric", "Cole", "Drake"],
      "noble": ["Aldric", "Cedric", "Percival"],
      "mystical": ["Balthazar", "Eldrin"]
    },
    "female": {
      "common": ["Adeline", "Belle", "Clara"],
      "noble": ["Anastasia", "Beatrice"],
      "mystical": ["Seraphina", "Morgana"]
    }
  },
  "surnames": {
    "fantasy": ["Ironforge", "Stormwind"],
    "nordic": ["Bjornsson", "Eriksson"],
    "celtic": ["O'Brien", "MacGregor"]
  },
  "pattern_components": {
    // Generation patterns for procedural names
  }
}
```

---

## Comparison: Current vs Proposed

### Current NPC Structure

```
npcs/
├── dialogue/
│   ├── dialogue_styles.json      ← Scattered
│   ├── greetings.json
│   ├── farewells.json
│   ├── rumors.json
│   └── templates.json
├── names/
│   ├── first_names.json          ← Should combine
│   └── last_names.json
├── occupations/
│   └── occupations.json          ← Should be in catalog
└── personalities/
    ├── backgrounds.json          ← Should be in catalog
    ├── personality_traits.json   ← Should be in traits
    └── quirks.json               ← Should be in traits
```

### Proposed NPC Structure (Aligned with Items/Enemies)

```
npcs/
├── .cbconfig.json
├── catalog.json        ← Backgrounds + Occupations combined (like weapon_types)
├── traits.json         ← Personality + Quirks (cosmetic, like abilities.json)
├── names.json          ← First + Last combined (like enemies/beasts/names.json)
└── dialogue/           ← Keep subfolder for organization
    ├── .cbconfig.json
    ├── greetings.json
    ├── farewells.json
    ├── rumors.json
    └── styles.json
```

---

## Benefits of Proposed Structure

### 1. **Consistency with Items/Enemies**

- ✅ `catalog.json` → Central NPC types (like weapon_types, beast_types)
- ✅ `traits.json` → Cosmetic modifiers (like abilities.json)
- ✅ `names.json` → Naming patterns (same as enemies)

### 2. **Clearer Hierarchy**

- NPC Type (catalog) → defines stats, skills, items
- Traits (cosmetic) → add personality flavor
- Dialogue → selected based on personality
- Names → procedural generation

### 3. **Easier ContentBuilder Navigation**

```
NPCs
├── Catalog           ← NPC types editor
├── Traits            ← Personality + quirks editor
├── Names             ← Name catalog editor
└── Dialogue          ← Dialogue subfolder
    ├── Greetings
    ├── Farewells
    ├── Rumors
    └── Styles
```

### 4. **Better for Generators**

```csharp
// Generate NPC:
1. Pick type from catalog.json → get occupation, background, skills, starting gold
2. Pick traits from traits.json → get personality + quirk
3. Pick name from names.json → first + last name
4. Pick dialogue from dialogue/ → based on personality trait
```

---

## Migration Path

### Phase 1: Combine Files (Low Risk)

1. ✅ Combine `first_names.json` + `last_names.json` → `names.json`
2. ✅ Move `notes` to `metadata` in all files

### Phase 2: Create catalog.json (Medium Risk)

1. Create `npcs/catalog.json`
2. Migrate data from `backgrounds.json` + `occupations.json`
3. Structure as `npc_types` with traits + items pattern
4. Update NpcGenerator.cs to use catalog

### Phase 3: Create traits.json (Low Risk)

1. Create `npcs/traits.json`
2. Combine `personality_traits.json` + `quirks.json`
3. Update generator to read from single file

### Phase 4: Reorganize Dialogue (Low Risk)

1. Keep dialogue files separate (already well-organized)
2. Just move to `npcs/dialogue/` subfolder
3. Update ContentBuilder tree config

### Phase 5: Cleanup (Low Risk)

1. Delete old files
2. Update tests
3. Update documentation

---

## Questions for Decision

1. **NPC Categories**: Use single `npcs/` or split into `npcs/commoners/`, `npcs/nobles/`, etc.?
   - **Recommendation**: Single `npcs/` with socialClass as trait

2. **catalog.json Items**: What properties should each NPC type have?
   - occupation
   - background
   - skills
   - startingGold
   - typicalItems
   - rarityWeight
   - **Others?**

3. **Dialogue Organization**: Consolidate into single `dialogue.json` or keep separate files?
   - **Recommendation**: Keep separate, just organize in subfolder

4. **Backwards Compatibility**: Keep old files during transition or clean break?
   - **Recommendation**: Clean break, update all references at once

---

## Recommendation Summary

### ✅ **DO THIS:**

1. **Create `npcs/catalog.json`** - Combine backgrounds + occupations into NPC types
2. **Create `npcs/traits.json`** - Combine personality_traits + quirks
3. **Create `npcs/names.json`** - Combine first_names + last_names
4. **Keep `npcs/dialogue/` subfolder** - Separate files for greetings/farewells/rumors/styles

### ✅ **Structure Like:**

```
npcs/
├── .cbconfig.json
├── catalog.json        ← Like items/weapons/catalog.json
├── traits.json         ← Like enemies/beasts/abilities.json
├── names.json          ← Like enemies/beasts/names.json
└── dialogue/
    ├── .cbconfig.json
    ├── greetings.json
    ├── farewells.json
    ├── rumors.json
    └── styles.json
```

### ✅ **Benefits:**

- Matches items/enemies pattern exactly
- Easier to understand and maintain
- Better for procedural generation
- Cleaner ContentBuilder tree
- Single source of truth for NPC "types"

---

**Ready to proceed?** Let me know and I'll start creating the new structure! 🚀
