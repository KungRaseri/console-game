# NPC Catalog Creation Summary

**Date**: 2025-12-18
**Status**: Phase 1 - Data Files (Task 1/7 Complete)

---

## ✅ Completed: npcs/catalog.json

### What Was Created

Created a comprehensive `catalog.json` file that combines:
- **14 Backgrounds** from `personalities/backgrounds.json`
- **49 Occupations** from `occupations/occupations.json`
- **Shop economy configuration** for 32 merchant occupations
- **Bank gold formulas** based on occupation and background

### File Statistics

**Total Size**: ~40KB
**Structure Version**: 4.0
**Component Keys**: `["backgrounds", "occupations"]`

#### Backgrounds Breakdown (14 total)
- **common_folk** (7 items): Commoner, Orphan, Farmer, Refugee, Drifter, Ex-convict, StreetUrchin
- **military** (5 items): FormerSoldier, Mercenary, Deserter, VeteranKnight, WarRefugee
- **nobility** (1 item): NobleBorn
- **mystical** (1 item): MagicallyGifted

#### Occupations Breakdown (49 total)

**Craftsmen** (10 merchants, shopChance 0.8-0.95):
- Blacksmith, Carpenter, Leatherworker, Jeweler, Weaponsmith, Armorer, Tailor, Cobbler, Potter, Glassblower

**Merchants** (6 merchants, shopChance 0.9-0.95):
- GeneralMerchant, TravelingMerchant, JunkDealer, Pawnbroker, Antiquarian, SilkMerchant

**Service** (5 merchants, shopChance 0.8-1.0):
- TavernKeeper, Innkeeper, Barber, Bathhouse, Undertaker

**Professionals** (3 merchants, shopChance 0.85):
- Scribe, Cartographer, Engineer

**Magical** (6 merchants, shopChance 0.75-0.9):
- Wizard, Alchemist, Enchanter, Herbalist, Astrologer, RuneCrafter

**Religious** (2 merchants, shopChance 0.7):
- Priest, Monk

**Adventurers** (4 non-merchants):
- BountyHunter, TreasureHunter, Explorer, Archaeologist

**Criminal** (5 non-merchants):
- Thief, Smuggler, Fence, Assassin, CriminalBoss

**Common** (6 non-merchants):
- Farmer, Fisherman, Miner, Hunter, LaborerDockworker, Servant

**Military** (2 non-merchants):
- Guard, CityWatch

### Key Features Implemented

#### 1. Separated Backgrounds and Occupations
```json
{
  "backgrounds": {
    "common_folk": { "items": [...] },
    "military": { "items": [...] }
  },
  "occupations": {
    "craftsmen": { "items": [...] },
    "merchants": { "items": [...] }
  }
}
```

**Why**: Allows NPCs to have BOTH background AND occupation (e.g., "Former Soldier Blacksmith")

#### 2. Shop Economy Configuration

**For 32 merchant occupations**, each has:

```json
{
  "shopType": "smithy",
  "shopChance": 0.8,  // 80% have shops, 20% are traveling/retired
  "shopInventory": {
    "coreItems": [...],      // Always available
    "dynamicCategories": [...], // Random daily refresh
    "economy": {
      "acceptsPlayerItems": true,
      "buyPriceMultiplier": 0.4,
      "resellPriceMultiplier": 0.8,
      "playerItemDecayDaily": 0.1,
      "playerItemDecayDays": 7,
      "maxPlayerItems": 10,
      "bankGoldFormula": "baseGold * 10 + background.startingGold * 5"
    }
  }
}
```

**Example - Blacksmith**:
- Core items: Longsword (infinite), Chainmail (2 per day)
- Dynamic: 1d6+2 random weapons/armor daily
- Accepts player-sold weapons/armor
- Buys at 40%, resells at 80% (40% markup)
- Items decay 10% per day, removed after 7 days
- Bank gold: (3d10 * 10) + (background gold * 5) = ~165-302g

#### 3. Bank Gold Formulas

**Formula**: `baseGold * multiplier + background.startingGold * multiplier`

**Examples**:
- **Blacksmith**: `baseGold * 10 + background * 5`
  - Noble-born: (16.5 * 10) + (27.5 * 5) = 302g
  - Orphan: (16.5 * 10) + (2.5 * 5) = 177g
  
- **General Merchant**: `baseGold * 15 + background * 10`
  - Noble-born: (27.5 * 15) + (27.5 * 10) = 687g
  - Orphan: (27.5 * 15) + (2.5 * 10) = 437g

**Result**: Noble-born merchants have more capital, orphan merchants struggle financially

#### 4. Background Shop Modifiers

**Simple modifiers** for now (expand later):

```json
{
  "name": "NobleBorn",
  "shopModifiers": {
    "qualityBonus": 10,           // +10% item quality
    "priceMultiplier": 1.15,      // +15% prices
    "shopAppearance": "elegant"
  },
  "specializationHints": ["ceremonial", "light_armor", "finesse_weapons"],
  "uniqueItems": ["Dueling Rapier", "Silk-lined Gloves"]
}
```

**Kept simple** per user request - expand in future phases.

#### 5. Occupation Specializations

**Not fully implemented** (future feature), but hints included:

```json
{
  "name": "Blacksmith",
  "specializationHints": ["weapons", "armor", "smithing"],
  "uniqueItems": ["Masterwork Hammer", "Anvil Charm"]
}
```

Future: "Weapon Specialist Blacksmith" vs "Armor Specialist Blacksmith"

### Design Decisions Applied

✅ **Option A Structure**: Single `npcs/` directory
✅ **Hybrid Approach**: Start simple, expand later
✅ **Separated Components**: Backgrounds ≠ Occupations
✅ **Shop Chance**: Not all craftsmen have shops (80-95% chance)
✅ **Limited Gold**: Formula-based, varies by background
✅ **Quality Pricing**: Ready for rarity weight implementation
✅ **Item Decay**: 7-day system, 10% daily price drop
✅ **Item Retention**: Player-sold items keep exact properties

### Data Migration Details

#### Backgrounds (from personalities/backgrounds.json)
- ✅ All 14 backgrounds migrated
- ✅ Added `shopModifiers` to each
- ✅ Added `specializationHints`
- ✅ Added `uniqueItems` (examples)
- ✅ Moved `notes` to `metadata`
- ✅ Added `displayName` where needed

#### Occupations (from occupations/occupations.json)
- ✅ All 49 occupations migrated
- ✅ Added `shopType` for merchants
- ✅ Added `shopChance` (0.0 for non-merchants)
- ✅ Added complete `shopInventory` configuration
- ✅ Added `economy` system for 32 merchants
- ✅ Categorized into 10 groups
- ✅ Preserved all original traits
- ✅ Added `displayName` where needed (e.g., "General Merchant")

### Economy System Examples

#### Example 1: Blacksmith Shop

**Core Inventory** (always available):
- Longsword (infinite stock)
- Chainmail (2 per day, restocks daily)

**Dynamic Inventory** (random daily):
- 1d6+2 items from [weapons.melee, armor.heavy]
- Examples: Greatsword, War Axe, Plate Armor, Iron Shield

**Player Economy**:
- Accepts: weapons.melee, armor.all
- Buys at: 40% of value
- Resells at: 80% of value (40% markup)
- Decay: 10% per day for 7 days
- Max: 10 player-sold items
- Bank: ~165-302g (depends on background)

#### Example 2: Apothecary Shop

**Core Inventory**:
- Health Potion (5 per day)
- Mana Potion (3 per day)

**Dynamic Inventory**:
- 1d10+3 items from [consumables.potions, consumables.herbs]

**Player Economy**:
- Accepts: consumables.potions, consumables.herbs
- Buys at: 40%
- Resells at: 75% (35% markup, lower than blacksmith)
- Decay: 15% per day for 5 days (potions expire faster!)
- Max: 15 player-sold items
- Bank: ~132-247g

#### Example 3: Tavern Keeper

**Core Inventory**:
- Ale (20 per day)
- Bread (15 per day)

**Dynamic Inventory**:
- 2d6+5 items from [consumables.food, consumables.drink]

**Player Economy**:
- Accepts: NOTHING (taverns don't buy items from adventurers)
- Services: lodging, rumors, quests
- Bank: ~300g (for services)

### Metadata Quality

All metadata follows v4.0 standards:

```json
{
  "metadata": {
    "type": "npc_catalog",
    "version": "4.0",
    "description": "...",
    "lastModified": "2025-12-18",
    "componentKeys": ["backgrounds", "occupations"],
    "notes": [
      "Backgrounds represent past/history",
      "Occupations represent current job",
      "NPCs can have both background AND occupation",
      "Shop system uses hybrid approach (core + dynamic)",
      "Player economy includes limited gold and decay"
    ]
  }
}
```

**Each category** has metadata:
```json
{
  "common_folk": {
    "metadata": {
      "description": "Everyday people from humble origins",
      "socialClass": "common"
    }
  }
}
```

### What This Enables

#### NPC Generation Examples

**Example 1: Noble-born Blacksmith**
```
Background: Noble-born
- Starting gold: 5d10 (avg 27.5g)
- Skills: persuasion, etiquette, leadership
- Shop modifier: +10% quality, +15% price

Occupation: Blacksmith
- Base gold: 3d10 (avg 16.5g)
- Skills: smithing, crafting
- Shop type: smithy

Combined:
- Name: "Sir Aldric Ironforge" (noble gets title)
- Total gold: 27.5 + 16.5 = 44g
- Shop bank: (16.5 * 10) + (27.5 * 5) = 302g
- Inventory: Core (Longsword, Chainmail) + Dynamic (1d6+2)
- Quality: +10% on all items
- Prices: +15% on all items
- Specialization: Ceremonial weapons more common
- Unique: Can sell "Dueling Rapier"
```

**Example 2: Orphan Thief**
```
Background: Orphan
- Starting gold: 1d4 (avg 2.5g)
- Skills: survival, stealth
- Shop modifier: -5% quality, -15% price

Occupation: Thief
- Base gold: 1d6 (avg 3.5g)
- Skills: stealth, lockpicking
- Shop type: null (no shop)

Combined:
- Name: "Cole" (no title, commoner single name)
- Total gold: 2.5 + 3.5 = 6g
- No shop (non-merchant occupation)
- Dialogue: streetwise, cautious
```

**Example 3: Former Soldier Apothecary**
```
Background: Former Soldier
- Starting gold: 2d10 (avg 11g)
- Skills: melee_combat, tactics, discipline
- Shop modifier: +15% quality, normal price, +20% combat items

Occupation: Apothecary
- Base gold: 2d10 (avg 11g)
- Skills: healing, herbalism
- Shop type: apothecary

Combined:
- Name: "Captain Marcus Miller" (military title)
- Total gold: 11 + 11 = 22g
- Shop bank: (11 * 8) + (11 * 3) = 121g
- Inventory: Health/Mana potions + herbs
- Quality: +15% (soldier knows good supplies)
- Specialization: Combat-oriented potions more common
- Unique: "Battle Stim Potion" (soldier background item)
```

### Files Referenced/Used

**Source Files** (read for migration):
- `Game.Data/Data/Json/npcs/personalities/backgrounds.json`
- `Game.Data/Data/Json/npcs/occupations/occupations.json`

**Created Files**:
- `Game.Data/Data/Json/npcs/catalog.json` (NEW)
- `docs/NPC_CATALOG_CREATION_SUMMARY.md` (this file)

**Documentation**:
- `docs/NPC_IMPLEMENTATION_PLAN.md` (reference)
- `docs/NPC_OPTION_A_VS_B_ANALYSIS.md` (design decisions)
- `docs/NPC_SHOPS_ECONOMY_DEEP_DIVE.md` (economy design)

---

## 📊 Statistics

### File Sizes
- **catalog.json**: ~40KB
- **backgrounds** section: ~8KB (14 items)
- **occupations** section: ~32KB (49 items)

### Coverage
- ✅ 100% of backgrounds migrated (14/14)
- ✅ 100% of occupations migrated (49/49)
- ✅ 32 merchant shops configured (65% of occupations)
- ✅ 17 non-merchant occupations (35% of occupations)

### Shop Configuration
- **32 merchants** with full economy system
- **17 non-merchants** (shopChance: 0.0)
- **Shop chance range**: 70% (Priest) to 100% (TavernKeeper)
- **Average shop chance**: 85% (most craftsmen have shops)

---

## 🎯 Next Steps

### Immediate (Continue Data Files)
1. ✅ **catalog.json** - COMPLETE
2. ⏭️ **traits.json** - Create next (combine personality_traits + quirks)
3. ⏭️ **names.json** - Pattern-based generation system
4. ⏭️ **dialogue/** - Move and fix notes placement

### After Data Files Complete
5. Update `NpcGenerator.cs` to use new structure
6. Create `ShopEconomyService.cs` for player trading
7. Write unit tests
8. Delete old files

---

## 💡 Key Insights from This Work

### 1. Background ≠ Occupation Separation Works
By separating these, we enable rich combinations:
- "Noble-born Thief" (fallen noble)
- "Orphan Wizard" (discovered magic despite poverty)
- "Former Soldier Blacksmith" (retired to craft weapons)

### 2. Shop Chance Adds Realism
Not all blacksmiths have shops:
- 80% settled with shops
- 20% traveling, retired, or working for others

### 3. Bank Gold Formula is Flexible
```
bankGoldFormula: "baseGold * X + background.startingGold * Y"
```
Can be customized per occupation:
- Merchants: high multipliers (more capital needed)
- Service: medium multipliers (steady income)
- Craftsmen: variable by profession

### 4. Economy System is Comprehensive
Player economy includes:
- Category restrictions (blacksmith won't buy potions)
- Buy/sell/resell multipliers (merchant profit)
- Decay system (old items removed)
- Limited gold (can't sell infinite loot)
- Quality pricing (rare items cost more)

### 5. Hybrid Approach Balances Simplicity & Depth
- **Simple now**: Basic shop modifiers (+10% quality)
- **Complex later**: Specializations, unique items, dynamic pricing

---

## ✅ Validation Checklist

- [x] All backgrounds migrated (14/14)
- [x] All occupations migrated (49/49)
- [x] Backgrounds and occupations separated
- [x] Shop economy configured for merchants
- [x] Bank gold formulas added
- [x] Shop chance percentages set
- [x] Core + dynamic inventory structure defined
- [x] Player economy system included
- [x] Metadata follows v4.0 standards
- [x] Notes moved to metadata
- [x] DisplayNames added where needed
- [x] Social classes preserved
- [x] Skill bonuses preserved
- [x] Original traits preserved

---

## 🎉 Summary

**Task 1 of 7 COMPLETE!** 

Created a comprehensive `catalog.json` file that:
- Combines 14 backgrounds and 49 occupations
- Separates background (past) from occupation (current job)
- Configures 32 merchant shops with full economy system
- Implements hybrid shop approach (core + dynamic items)
- Includes player trading system (buy/sell/decay)
- Uses formula-based bank gold (varies by background)
- Follows all approved design decisions
- Maintains v4.0 metadata standards

**Ready for:** Next data file creation (traits.json)

**File created:** `c:\code\console-game\Game.Data\Data\Json\npcs\catalog.json` (40KB)
