# Abilities Organization Plan

## Architecture Decision ✅

**Abilities remain organized by TYPE** (active/offensive, passive/defensive, etc.)
**Classes reference abilities** they grant access to via JSON references

### Benefits
- ✅ Abilities stay in logical functional categories
- ✅ Easy to find abilities by behavior/mechanics
- ✅ Abilities can be shared across classes if needed
- ✅ Simpler class definitions (just reference lists)
- ✅ Easier to browse abilities by category in tools

## Current Structure (KEEP THIS)

```
abilities/
├── active/
│   ├── offensive/    # Attack abilities
│   ├── defensive/    # Defense/mitigation abilities
│   ├── support/      # Buffs, heals, ally enhancements
│   ├── utility/      # Non-combat utility
│   ├── control/      # Crowd control
│   ├── mobility/     # Movement abilities
│   └── summon/       # Summoning abilities
├── passive/
│   ├── offensive/    # Always-on offensive bonuses
│   ├── defensive/    # Always-on defensive bonuses
│   ├── leadership/   # Party bonuses
│   ├── environmental/# Environmental interactions
│   ├── mobility/     # Movement bonuses
│   └── sensory/      # Detection/awareness
├── reactive/
│   ├── offensive/    # Triggered offensive abilities
│   ├── defensive/    # Triggered defensive abilities
│   └── utility/      # Triggered utility
└── ultimate/         # Ultimate abilities (level 20)
```

## Class Definitions (Future Implementation)

Classes will be defined in `classes/catalog.json` with ability references:

```json
{
  "class_types": {
    "warrior": {
      "items": [
        {
          "slug": "warrior",
          "startingAbilities": [
            "@abilities/active/offensive:shield-bash",
            "@abilities/active/support:second-wind",
            "@abilities/active/support:battle-cry"
          ],
          "levelUnlocks": {
            "5": ["@abilities/passive/defensive:iron-will"],
            "10": ["@abilities/active/offensive:whirlwind"],
            "20": ["@abilities/ultimate:last-stand"]
          }
        }
      ]
    }
  }
}
```

## Existing Abilities Status

### Total: 413 abilities in type-based structure ✅

**Active Abilities (191)**:
- offensive: 88 abilities
- defensive: 34 abilities
- support: 27 abilities
- utility: 28 abilities
- control: 8 abilities
- mobility: 2 abilities
- summon: 4 abilities

**Passive Abilities (147)**:
- offensive: 38 abilities
- defensive: 39 abilities
- leadership: 24 abilities
- environmental: 22 abilities
- mobility: 7 abilities
- sensory: 1 ability
- general: 16 abilities

**Reactive Abilities (36)**:
- offensive: 14 abilities
- defensive: 12 abilities
- utility: 10 abilities

**Ultimate Abilities**: 39 abilities

## Missing Player Class Abilities

Need to create 29 new abilities and add them to appropriate type folders:

### Active/Offensive (5 needed)
- `poison-strike` (Rogue)
- `arcane-missiles` (Mage)
- `power-shot` (Ranger)
- `holy-strike` (Paladin)
- `divine-smite` (Paladin)

### Active/Defensive (1 needed)
- `mana-shield` (Mage)

### Active/Support (3 needed)
- `blessing` (Cleric)
- `protective-aura` (Paladin)
- `consecration` (Paladin)

### Active/Control (1 needed)
- `frost-nova` (Mage)

### Active/Mobility (1 needed)
- `charge` (Warrior)

### Active/Utility (1 needed)
- `spell-steal` (Mage)

### Active/Summon (1 needed)
- `pet-summon` (Ranger)

### Passive/Offensive (2 needed)
- `shadow-affinity` (Rogue)
- `arcane-affinity` (Mage)

### Passive/Defensive (2 needed)
- `iron-will` (Warrior)
- `righteous-vigor` (Paladin)

### Passive/Support (1 needed)
- `divine-grace` (Cleric)

### Passive/Sensory (1 needed)
- `keen-senses` (Ranger)

### Ultimate (5 needed)
- `assassination` (Rogue)
- `meteor` (Mage)
- `divine-intervention` (Cleric)
- `arrow-storm` (Ranger)
- `judgment` (Paladin)

## Next Steps

1. ✅ Keep existing type-based structure
2. Create 29 missing class abilities in appropriate folders
3. Create classes/catalog.json with ability references
4. Ensure all ability slugs are unique
5. Document ability-to-class mapping

---

**Status**: Structure finalized - no migration needed
**Created**: 2025-12-28
**Last Updated**: 2026-01-05

### Total Abilities by Category

```
Active Abilities (191):
├── offensive: 88 abilities
├── defensive: 34 abilities
├── support: 27 abilities
├── utility: 28 abilities
├── control: 8 abilities
├── mobility: 2 abilities
└── summon: 4 abilities

Passive Abilities (147):
├── offensive: 38 abilities
├── defensive: 39 abilities
├── leadership: 24 abilities
├── environmental: 22 abilities
├── mobility: 7 abilities
├── sensory: 1 ability
└── general: 16 abilities

Reactive Abilities (36):
├── offensive: 14 abilities
├── defensive: 12 abilities
└── utility: 10 abilities

Ultimate Abilities: 39 abilities

TOTAL: 413 abilities
```

## Classification Strategy

### 1. Player Class Abilities (48 total - 8 per class)

**Warrior** (8 abilities):
- ✅ `second-wind` (active/support) → warrior/catalog.json
- ✅ `battle-cry` (active/support) → warrior/catalog.json
- ⚠️ `charge` (likely in active/offensive or mobility) → warrior/catalog.json
- ⚠️ `shield-bash` (likely in active/offensive) → warrior/catalog.json
- ⚠️ `iron-will` (need to create passive) → warrior/catalog.json
- ⚠️ `whirlwind` (likely in active/offensive) → warrior/catalog.json
- ⚠️ `execute` (likely in active/offensive) → warrior/catalog.json
- ⚠️ `last-stand` (likely in ultimate) → warrior/catalog.json

**Rogue** (8 abilities):
- ⚠️ `backstab` (likely in active/offensive) → rogue/catalog.json
- ⚠️ `evasion` (likely in active/defensive) → rogue/catalog.json
- ⚠️ `shadow-affinity` (need to create passive) → rogue/catalog.json
- ⚠️ `poison-strike` (likely in active/offensive) → rogue/catalog.json
- ⚠️ `vanish` (likely in active/utility) → rogue/catalog.json
- ⚠️ `shadow-step` (likely in active/mobility) → rogue/catalog.json
- ⚠️ `assassination` (likely in ultimate) → rogue/catalog.json

**Mage** (8 abilities):
- ⚠️ `arcane-missiles` (likely in active/offensive) → mage/catalog.json
- ⚠️ `mana-shield` (likely in active/defensive or spells) → mage/catalog.json
- ⚠️ `arcane-affinity` (need to create passive) → mage/catalog.json
- ⚠️ `frost-nova` (likely in active/control or spells) → mage/catalog.json
- ⚠️ `blink` (likely in active/mobility or spells) → mage/catalog.json
- ⚠️ `spell-steal` (likely in active/utility) → mage/catalog.json
- ⚠️ `meteor` (likely in ultimate or spells) → mage/catalog.json

**Cleric** (8 abilities):
- ⚠️ `smite` (likely in active/offensive) → cleric/catalog.json
- ⚠️ `heal` (likely in active/support or spells) → cleric/catalog.json
- ⚠️ `divine-grace` (need to create passive) → cleric/catalog.json
- ⚠️ `divine-shield` (likely in active/defensive) → cleric/catalog.json
- ⚠️ `cleanse` (likely in active/utility) → cleric/catalog.json
- ⚠️ `blessing` (likely in active/support) → cleric/catalog.json
- ⚠️ `divine-intervention` (likely in ultimate) → cleric/catalog.json

**Ranger** (8 abilities):
- ⚠️ `power-shot` (likely in active/offensive) → ranger/catalog.json
- ⚠️ `trap` (likely in active/utility) → ranger/catalog.json
- ⚠️ `keen-senses` (need to create passive) → ranger/catalog.json
- ⚠️ `hunters-mark` (likely in active/offensive or support) → ranger/catalog.json
- ⚠️ `camouflage` (likely in active/utility) → ranger/catalog.json
- ⚠️ `pet-summon` (likely in active/summon) → ranger/catalog.json
- ⚠️ `arrow-storm` (likely in ultimate) → ranger/catalog.json

**Paladin** (8 abilities):
- ⚠️ `holy-strike` (likely in active/offensive) → paladin/catalog.json
- ⚠️ `protective-aura` (likely in active/defensive or passive/defensive) → paladin/catalog.json
- ⚠️ `righteous-vigor` (need to create passive) → paladin/catalog.json
- ⚠️ `divine-smite` (likely in active/offensive) → paladin/catalog.json
- ⚠️ `lay-on-hands` (likely in active/support) → paladin/catalog.json
- ⚠️ `consecration` (likely in active/offensive or support) → paladin/catalog.json
- ⚠️ `judgment` (likely in ultimate) → paladin/catalog.json

### 2. Enemy Abilities (~365 abilities)

**Characteristics:**
- Thematic names: "Venomous Bite", "Savage Roar", "Hellfire Breath"
- Animal behaviors: "Swift Pounce", "Pack Hunter"
- Monster powers: "Cursed Weapons", "Infernal Flames"
- Generic enemy actions: "Ferocious", "Primal Fury"

**Organization**: Keep in `abilities/enemy/` with type-based structure
- enemy/offensive/
- enemy/defensive/
- enemy/support/
- enemy/ultimate/

### 3. Abilities That Are Actually Spells

**Candidates for spells/ migration:**
- `mana-shield` (active/defensive) → Already in spells/alteration
- `heal` (active/support) → Already in spells/restoration
- `frost-nova` (active/control) → Could be destruction spell
- `blink` (active/mobility) → Could be mysticism spell
- `meteor` (ultimate) → Already in spells/destruction
- Any fire/ice/lightning damage abilities → Likely destruction spells

**Decision Criteria:**
- Is it learned from spellbooks/scrolls? → Spell
- Is it class-granted automatically? → Ability
- Does it use spell schools (Destruction, Restoration, etc.)? → Spell

## Migration Steps

### Phase 1: Identify Player Class Abilities ✅ NEXT
1. Search existing abilities for matches to 48 designed class abilities
2. Create mapping: existing slug → class/ability-name
3. Note missing abilities that need creation

### Phase 2: Create Class Ability Catalogs
1. Create directory structure:
   ```
   abilities/
   ├── classes/
   │   ├── warrior/catalog.json
   │   ├── rogue/catalog.json
   │   ├── mage/catalog.json
   │   ├── cleric/catalog.json
   │   ├── ranger/catalog.json
   │   └── paladin/catalog.json
   └── enemy/ (reorganized existing abilities)
   ```

2. Create each class catalog with 8 abilities
3. Add `.cbconfig.json` for each class folder

### Phase 3: Migrate Enemy Abilities
1. Create enemy/ subdirectory
2. Move existing type-based folders to enemy/
3. Update references if needed

### Phase 4: Clean Up Duplicates
1. Remove abilities migrated to spells/
2. Remove abilities migrated to classes/
3. Archive old structure

### Phase 5: Update References
1. Scan all JSON files for ability references
2. Update paths: `@abilities/active/offensive:second-wind` → `@abilities/classes/warrior:second-wind`
3. Update references to spells: `@abilities/active/control:frost-nova` → `@spells/destruction:frost-nova`

## Search Patterns for Phase 1

**Warrior Abilities:**
- `second-wind` ✅ FOUND in active/support
- `battle-cry` ✅ FOUND in active/support
- Search: "charge", "shield-bash", "iron-will", "whirlwind", "execute", "last-stand"

**Rogue Abilities:**
- Search: "backstab", "evasion", "shadow", "poison-strike", "vanish", "assassination"

**Mage Abilities:**
- Search: "arcane-missiles", "mana-shield", "arcane", "frost-nova", "blink", "spell-steal", "meteor"

**Cleric Abilities:**
- Search: "smite", "heal", "divine", "cleanse", "blessing", "intervention"

**Ranger Abilities:**
- Search: "power-shot", "trap", "keen", "hunter", "camouflage", "pet", "arrow-storm"

**Paladin Abilities:**
- Search: "holy-strike", "protective-aura", "righteous", "lay-on-hands", "consecration", "judgment"

## Notes

- Many existing abilities (especially in passive/offensive) are enemy-specific and should remain separate
- Some "ultimate" abilities in existing catalog might be enemy ultimates, not player ultimates
- Need to carefully distinguish between class abilities (automatically granted) vs spells (must be learned)
- The 413 existing abilities are primarily enemy content, not player content
- Most of the 48 player class abilities may need to be created from scratch based on the design document

## Next Actions

1. ✅ Search for each of the 48 class abilities in existing catalog
2. Create detailed mapping document
3. Begin creating class ability catalogs
4. Move enemy abilities to enemy/ subfolder
5. Update all references

---

**Status**: Phase 1 - Ready to begin detailed ability search
**Created**: Current Date
**Last Updated**: Current Date
