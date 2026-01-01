# NPC Shops & Economy Deep Dive

**Date**: 2025-12-18
**Context**: Exploring shop inventory systems and merchant economy

---

## 🎯 Confirmed Decisions

### Names/Titles (Topic 2):
- ✅ Soft filtering with weight multipliers
- ✅ Titles start as **decorative only** (gameplay bonuses added later)
- ✅ Earning/losing titles supported (future feature)
- ✅ Multiple titles allowed ("Captain Sir Aldric")
- ✅ Pattern-based title exclusion supported

### Shops (Topic 4):
- ✅ Tight coupling: Occupations define shop inventory
- ✅ Personality traits affect prices (Greedy/Generous)
- ✅ Specializations supported ("Weapon Specialist Blacksmith")

---

## 🛒 Discussion Topic 4.1: Background Effects on Shop Quality

### The Core Question
Should an NPC's background modify their shop's inventory quality and prices?

---

### Scenario Examples

#### Example 1: The Noble-born Blacksmith

**Background:** Noble-born
**Occupation:** Blacksmith

**Option A: Background Affects Shop Stats**
```json
// Base blacksmith inventory
{
  "categories": ["weapons.melee", "armor.heavy"],
  "itemCount": "2d6+5",
  "baseQuality": 1.0,
  "basePrice": 1.0
}

// Noble-born modifiers applied
{
  "finalQuality": 1.5,      // +50% quality (better materials)
  "finalPrice": 1.3,        // +30% price (higher overhead)
  "rarityBonus": 15,        // More rare items appear
  "shopAppearance": "Ornate workshop with fine decorations"
}
```

**Narrative:**
> "Aldric Ironforge comes from a noble family. His workshop is filled with the finest tools money can buy. His weapons are of exceptional quality, but his prices reflect his noble upbringing and expensive tastes."

**Pros:**
- ✅ **Realistic:** Noble has better resources/connections
- ✅ **Variety:** Two blacksmiths feel different (noble vs commoner)
- ✅ **Player choice:** Cheap vs quality trade-off
- ✅ **Storytelling:** Background matters mechanically

**Cons:**
- ❌ Noble backgrounds always "better" (imbalanced?)
- ❌ May create "optimal" backgrounds to seek out
- ❌ Complex: need modifiers for each background

---

**Option B: Background Affects Shop Specialization (Not Quality)**
```json
// Noble-born blacksmith
{
  "specialization": "ceremonial_weapons",
  "categories": ["weapons.swords", "armor.light"],  // Noble weapons
  "uniqueItems": ["Dueling Rapier", "Parade Armor"],
  "priceModifier": 1.3,
  "qualityModifier": 1.0  // Same quality, different selection
}

// Commoner blacksmith
{
  "specialization": "practical_weapons",
  "categories": ["weapons.axes", "weapons.hammers", "armor.heavy"],
  "uniqueItems": ["Woodcutter's Axe", "Work Hammer"],
  "priceModifier": 1.0,
  "qualityModifier": 1.0  // Same quality, different selection
}
```

**Narrative:**
> "Both blacksmiths craft excellent weapons, but Aldric (noble) specializes in elegant dueling swords and parade armor, while Gareth (commoner) focuses on practical axes and heavy work gear."

**Pros:**
- ✅ Balanced: no "better" background, just different
- ✅ Variety: distinct inventory selection
- ✅ Player choice: style preference (elegant vs practical)
- ✅ Realism: backgrounds influence interests, not skill

**Cons:**
- Need to define specializations for each background
- More complex inventory generation

---

**Option C: Background Affects Backstory Only (No Mechanical Effect)**
```json
// Both blacksmiths identical mechanically
{
  "categories": ["weapons.melee", "armor.heavy"],
  "qualityModifier": 1.0,
  "priceModifier": 1.0
}

// But different dialogue/appearance
{
  "background": "Noble-born",
  "dialogueStyle": "formal",
  "shopDescription": "An elegant workshop with fine decorations"
}
```

**Narrative:**
> "Aldric may come from nobility, but he's just as skilled as any other blacksmith. His shop looks fancier and he speaks more formally, but his prices and quality are the same."

**Pros:**
- ✅ Simple: no balance issues
- ✅ Fair: all blacksmiths equal mechanically
- ✅ Flavor: background adds story, not stats

**Cons:**
- ❌ Less variety in gameplay
- ❌ Background feels less impactful

---

### 🎨 Hybrid Approach: Minor Modifiers + Specialization

**My Recommendation:**
```json
{
  "backgrounds": {
    "noble_born": {
      "shopModifiers": {
        "qualityBonus": 10,        // Small bonus (+10%, not +50%)
        "priceMultiplier": 1.15,   // Modest increase (+15%)
        "shopAppearance": "elegant"
      },
      "specializationHints": ["ceremonial", "light_armor", "finesse_weapons"],
      "uniqueItems": ["Dueling Rapier", "Silk-lined Gloves"]
    },
    "commoner": {
      "shopModifiers": {
        "qualityBonus": 0,
        "priceMultiplier": 0.95,   // 5% cheaper
        "shopAppearance": "practical"
      },
      "specializationHints": ["practical", "heavy_armor", "work_tools"],
      "uniqueItems": ["Woodcutter's Axe", "Leather Work Gloves"]
    },
    "former_soldier": {
      "shopModifiers": {
        "qualityBonus": 15,        // Knows quality weapons
        "priceMultiplier": 1.0,    // Fair prices
        "shopAppearance": "utilitarian"
      },
      "specializationHints": ["military", "weapons", "tactical_gear"],
      "uniqueItems": ["Officer's Sword", "Battle-worn Shield"],
      "combatItemBonus": 20       // Only applies to weapons/armor
    },
    "orphan": {
      "shopModifiers": {
        "qualityBonus": -5,        // Scrappier items
        "priceMultiplier": 0.85,   // 15% cheaper (desperate for sales)
        "shopAppearance": "humble"
      },
      "specializationHints": ["scavenged", "repaired", "budget"],
      "uniqueItems": ["Repaired Dagger", "Patched Leather Armor"]
    }
  }
}
```

**Why Hybrid?**
1. **Small modifiers:** +10% quality isn't game-breaking, just noticeable
2. **Specialization hints:** Background suggests what items appear more often
3. **Unique items:** Each background gets exclusive items (not better, just different)
4. **Balanced trade-offs:** Noble = quality + price, Orphan = cheap + scrappy

**Example Generation:**
```
Noble-born Blacksmith "Aldric Ironforge"
- Base blacksmith inventory: Longsword (quality 100, price 50g)
- Background modifier: +10% quality, +15% price
- Final item: Longsword (quality 110, price 57g)
- Unique item available: Dueling Rapier (not in other blacksmiths)
- Specialization: 30% higher chance for finesse weapons
```

**Questions:**
1. Should quality bonuses apply to all items or just related categories?
   - Noble blacksmith: +10% to ALL items or just "ceremonial" weapons?
2. Should negative modifiers exist? (Orphan = -5% quality)
3. Should unique items be guaranteed or random chance?

---

## 🏪 Discussion Topic 4.4: Shop Inventory Systems

### The Core Question
How should merchant inventory work? Fixed lists, dynamic generation, or a hybrid?

---

### System Comparison

#### System A: Pure Dynamic (Randomized Each Visit)

**Implementation:**
```json
{
  "name": "Blacksmith",
  "shopInventory": {
    "categories": ["weapons.melee", "armor.heavy"],
    "itemCount": "2d6+5",
    "qualityRange": [80, 120],
    "refreshOnVisit": true
  }
}
```

**How It Works:**
- Player visits → system rolls `2d6+5` (7-17 items)
- Items randomly picked from `weapons.melee` + `armor.heavy`
- Each item has random quality (80-120%)
- **Every visit = different inventory**

**Example Visits:**
```
Visit 1 (Monday): 
  - Longsword (quality 95)
  - Chainmail (quality 110)
  - War Axe (quality 88)
  - [9 more items]

Visit 2 (Tuesday):
  - Greatsword (quality 102)
  - Dagger (quality 91)
  - Plate Armor (quality 115)
  - [8 more items]  ← Different count! (2d6+5 rolled again)
```

**Pros:**
- ✅ Infinite variety
- ✅ Incentive to visit multiple times ("check back later for better items")
- ✅ Simple to implement

**Cons:**
- ❌ Unrealistic: where did all the swords go?
- ❌ No persistence: can't "come back for that item"
- ❌ Immersion breaking: shop feels like a random loot table

---

#### System B: Pure Static (Fixed Inventory)

**Implementation:**
```json
{
  "name": "Blacksmith",
  "shopInventory": {
    "items": [
      { "item": "Longsword", "quantity": 2, "quality": 100 },
      { "item": "Chainmail", "quantity": 1, "quality": 110 },
      { "item": "War Axe", "quantity": 3, "quality": 95 }
    ],
    "refreshDaily": false
  }
}
```

**How It Works:**
- Inventory set when NPC spawns
- Same items every visit
- Quantities decrease when player buys
- Never restocks (or restocks on long timer)

**Example Visits:**
```
Visit 1 (Monday):
  - Longsword x2 (100g each)
  - Chainmail x1 (200g)
  - War Axe x3 (75g each)

Visit 2 (Tuesday) [after buying 1 Longsword]:
  - Longsword x1 (100g)  ← Reduced quantity
  - Chainmail x1 (200g)
  - War Axe x3 (75g each)
```

**Pros:**
- ✅ Realistic: shops have actual inventory
- ✅ Persistence: can "come back later"
- ✅ Depletion: buying items feels impactful

**Cons:**
- ❌ Boring: same items forever
- ❌ Limited: might not have what player needs
- ❌ No reason to revisit (unless restocking)

---

#### System C: Hybrid (Core + Dynamic)

**Implementation:**
```json
{
  "name": "Blacksmith",
  "shopInventory": {
    "coreItems": [
      { "item": "Longsword", "alwaysAvailable": true, "quantity": "infinite" },
      { "item": "Chainmail", "alwaysAvailable": true, "quantity": 2, "restockDaily": true }
    ],
    "dynamicCategories": ["weapons.melee", "armor.heavy"],
    "dynamicItemCount": "1d6+2",
    "dynamicRefreshDaily": true
  }
}
```

**How It Works:**
- **Core items:** Always in stock (blacksmith always has basic swords/armor)
- **Dynamic items:** Random selection, changes daily/weekly
- **Player sees:** Familiar items + "what's new today?"

**Example Visits:**
```
Visit 1 (Monday):
  [CORE - Always Available]
  - Longsword (infinite stock)
  - Chainmail x2 (restocks daily)
  
  [DYNAMIC - Changes Daily]
  - Greatsword (quality 105)
  - War Hammer (quality 92)
  - Leather Armor (quality 88)
  - [4 more random items]

Visit 2 (Tuesday):
  [CORE - Always Available]
  - Longsword (infinite stock)
  - Chainmail x2 (restocked!)
  
  [DYNAMIC - NEW TODAY]
  - Battle Axe (quality 110)  ← Different items!
  - Plate Gauntlets (quality 98)
  - Iron Shield (quality 95)
  - [3 more random items]
```

**Pros:**
- ✅ Best of both: reliability + variety
- ✅ Realistic: blacksmiths have staple items + rotating stock
- ✅ Incentive to revisit: "what's new today?"
- ✅ Player-friendly: can always buy basics

**Cons:**
- More complex to implement
- Need to define core items per occupation

---

### 🔄 Advanced Hybrid: Player Interaction Economy

**My Recommendation:** Hybrid + Player Economy

```json
{
  "name": "Blacksmith",
  "shopInventory": {
    "coreItems": [
      { "item": "Longsword", "baseQuantity": "infinite" },
      { "item": "Chainmail", "baseQuantity": 2, "restockDaily": true }
    ],
    "dynamicCategories": ["weapons.melee", "armor.heavy"],
    "dynamicItemCount": "1d6+2",
    "dynamicRefreshDaily": true,
    
    // NEW: Economy system
    "economy": {
      "acceptsPlayerItems": true,
      "acceptedCategories": ["weapons.melee", "armor.all"],
      "buyPriceMultiplier": 0.4,      // Buys at 40% of base value
      "sellPriceMultiplier": 1.0,     // Sells at 100% of base value
      "resellPriceMultiplier": 0.8,   // Resells player items at 80%
      "playerItemDecayDaily": 0.1,    // 10% price drop per day
      "maxPlayerItems": 10,            // Won't buy more than 10 items
      "bankGold": "5d100",            // Shop has limited gold (250-500g)
      "bankGoldRestockDaily": true
    }
  }
}
```

**How It Works:**

#### Player Sells to Merchant
```
Player has: Greatsword (base value 100g)
Merchant offers: 40g (buyPriceMultiplier 0.4)
Player accepts → Merchant adds to inventory
```

#### Merchant's Inventory Updates
```
[CORE ITEMS]
- Longsword (infinite)
- Chainmail x2

[DYNAMIC ITEMS]
- War Axe (quality 95)
- Leather Armor (quality 88)

[PLAYER SOLD ITEMS]
- Greatsword (resell price 80g) ← 40% markup from 40g purchase
```

#### Another Player Visits
```
They see:
- Longsword (100g) [core]
- Chainmail (200g) [core]
- War Axe (75g) [dynamic]
- Greatsword (80g) ← Player A sold this!
```

#### Daily Decay (Next Day)
```
[PLAYER SOLD ITEMS]
- Greatsword (resell price 72g) ← 10% decay (80g → 72g)

After 7 days unsold:
- Greatsword removed (too old, merchant sold to NPC)
```

---

### 📊 Economy System Features

#### Feature 1: Limited Merchant Gold
```json
{
  "bankGold": "5d100",  // 250-500g starting gold
  "bankGoldRestockDaily": true
}
```

**Scenario:**
```
Merchant has 300g in bank
Player tries to sell 10 items worth 400g total
Merchant says: "I can only afford 7 of these (300g worth)"
Player must choose which items to sell
```

**Next Day:**
```
Merchant bank restocks to 250-500g
Player can sell more items
```

**Prevents:** Player selling infinite items for infinite gold

---

#### Feature 2: Category Restrictions
```json
{
  "acceptedCategories": ["weapons.melee", "armor.all"],
  "rejectedCategories": ["consumables", "quest_items"]
}
```

**Scenario:**
```
Player tries to sell Health Potion to Blacksmith
Blacksmith: "Sorry, I only buy weapons and armor. Try the apothecary."
```

**Realism:** Blacksmiths don't buy potions

---

#### Feature 3: Dynamic Purchasing
```json
{
  "purchaseFromNPCs": true,
  "purchaseChanceDaily": 0.3,      // 30% chance per day
  "purchaseCategories": ["weapons.common"],
  "purchaseBudget": "2d20"         // Spends 2-40g per day
}
```

**Scenario (Background Simulation):**
```
Day 1: Blacksmith rolls 30% chance → SUCCESS
Blacksmith "purchases" from NPC adventurer:
  - Adds "Battle Axe" to inventory (cost 25g)
  - Bank gold: 300g → 275g

Player visits:
  - Sees new Battle Axe available (not player-sold, NPC-purchased)
```

**Why:** Makes shop feel alive (NPCs trade with each other, not just player)

---

### 🎮 Complete Example Scenario

**Initial State (Day 1 - Monday):**
```
Gareth Ironforge's Blacksmith Shop
Bank Gold: 350g

[CORE INVENTORY]
- Longsword (100g) - infinite stock
- Chainmail (200g) - x2 in stock

[DYNAMIC INVENTORY] (rolled 1d6+2 = 5 items)
- Greatsword (120g, quality 105)
- War Axe (75g, quality 92)
- Leather Armor (85g, quality 88)
- Iron Shield (60g, quality 100)
- Dagger (30g, quality 95)

[PLAYER-SOLD INVENTORY]
- (empty)
```

**Player Visit 1 (Monday Afternoon):**
```
Player Actions:
1. Buys Greatsword (120g)
2. Sells old Rusty Sword (base value 50g)
   - Merchant offers 20g (40% of 50g)
   - Player accepts

Shop State After:
Bank Gold: 350g - 20g + 120g = 450g
[DYNAMIC INVENTORY]
- War Axe (75g)
- Leather Armor (85g)
- Iron Shield (60g)
- Dagger (30g)
[PLAYER-SOLD INVENTORY]
- Rusty Sword (40g) ← Will resell at 80% markup
```

**Day 2 (Tuesday - Daily Refresh):**
```
Shop restocks:
1. Bank gold resets: 450g → 387g (5d100 roll)
2. Chainmail restocks: x1 → x2
3. Dynamic inventory refreshes (new roll 1d6+2 = 4 items)
4. Player-sold item decays: Rusty Sword 40g → 36g (10% decay)
5. NPC purchase check: 30% chance → FAIL (no new items purchased)

New State:
Bank Gold: 387g

[CORE INVENTORY]
- Longsword (100g) - infinite
- Chainmail (200g) - x2 ← Restocked!

[DYNAMIC INVENTORY] (NEW ITEMS)
- Battle Axe (90g, quality 110)
- Plate Gauntlets (150g, quality 98)
- Wooden Shield (45g, quality 88)
- Short Sword (65g, quality 102)

[PLAYER-SOLD INVENTORY]
- Rusty Sword (36g) ← Decayed price
```

**Different Player Visit (Tuesday):**
```
Player B walks in and sees:
- Rusty Sword (36g) ← Player A sold this yesterday!
- Battle Axe (90g) ← New dynamic item today
- Plate Gauntlets (150g)

Player B buys Rusty Sword (36g)
Merchant bank: 387g - 36g = 351g
Rusty Sword removed from inventory (sold to player)
```

---

### 🤔 Discussion Questions

#### Q1: Should merchants remember player-sold items permanently or decay them?

**Option A: Permanent (Until Sold)**
- Player-sold items stay forever until another player/NPC buys them
- No decay
- **Pros:** Simple, persistent
- **Cons:** Could clutter inventory

**Option B: Decay System (Recommended)**
- 10% price drop per day
- After 7-10 days unsold → removed (merchant "sold to NPC offscreen")
- **Pros:** Keeps inventory fresh, prevents clutter
- **Cons:** Player might return and item is gone

**Option C: Hybrid**
- High-value items (>100g) → permanent
- Low-value items (<100g) → decay after 7 days
- **Pros:** Important items persist, junk clears out
- **Cons:** More complex

---

#### Q2: Should player-sold items retain their properties?

**Example:**
```
Player sells: "Enchanted Longsword +5 Fire Damage" (unique modifiers)

Option A: Item retains enchantments
- Another player can buy "Enchanted Longsword +5 Fire Damage"
- Exact same item

Option B: Item resets to base
- Merchant strips enchantments (or doesn't understand them)
- Sells as "Longsword" (base version)
- Balances out (player can't sell god-tier items to new players)

Option C: Item degrades
- "Enchanted Longsword +3 Fire Damage" (reduced from +5)
- Merchant doesn't maintain enchantments well
```

---

#### Q3: Should merchants purchase items from NPCs? (Background simulation)

**Current Idea:**
- 30% chance per day merchant "purchases" from NPC adventurers
- Adds items to dynamic inventory
- Spends bank gold

**Pros:**
- ✅ Shop feels alive (inventory changes even without player)
- ✅ Explains where dynamic items come from (not just magic)
- ✅ Bank gold fluctuates naturally

**Cons:**
- ❌ Player can't predict inventory (less control)
- ❌ Might add items player doesn't want

**Alternative:** Only player interactions affect inventory

---

#### Q4: Should merchant bank gold be limited or infinite?

**Option A: Limited (Recommended)**
```json
{
  "bankGold": "5d100",  // 250-500g
  "bankGoldRestockDaily": true
}
```
- **Pros:** Realistic, prevents exploit (sell infinite items), forces player to visit multiple merchants
- **Cons:** Frustrating if player has lots to sell

**Option B: Infinite**
```json
{
  "bankGold": "infinite"
}
```
- **Pros:** Player-friendly, no frustration
- **Cons:** Unrealistic, allows infinite money exploit

**Option C: Hybrid**
```json
{
  "bankGold": "5d100",
  "canRequestCredit": true,  // Merchant can "order gold from bank" (takes 1 day)
}
```
- **Pros:** Limited but flexible, player can sell large hauls over multiple days
- **Cons:** More complex

---

#### Q5: Should item quality affect buy/sell prices?

**Example:**
```
Base Longsword: 100g

Player sells:
- Rusty Longsword (quality 70) → Merchant offers 28g (40% of 70g)
- Perfect Longsword (quality 120) → Merchant offers 48g (40% of 120g)

Merchant resells:
- Rusty Longsword → 56g (80% of 70g)
- Perfect Longsword → 96g (80% of 120g)
```

**Pros:**
- ✅ Realistic: quality matters
- ✅ Rewards player for selling high-quality items

**Cons:**
- More complex pricing calculations

---

## 📋 Summary of Recommendations

### Background Effects (4.1):
✅ **Hybrid Approach**
- Small modifiers (+10% quality, +15% price) - noticeable but not game-breaking
- Specialization hints (Noble → ceremonial weapons appear more often)
- Unique items per background (exclusive, not better)
- Balanced trade-offs (Noble = quality+price, Orphan = cheap+scrappy)

### Inventory System (4.4):
✅ **Hybrid + Player Economy**
- Core items (always available, infinite/restocking)
- Dynamic items (randomized daily, variety)
- Player-sold items (with decay system)
- Limited merchant gold (prevents exploits)
- Category restrictions (blacksmith won't buy potions)
- Optional: NPC purchasing (background simulation)

---

## 🚀 Next Steps

**What do you think about:**

1. **Background modifiers:** Small bonuses (+10%) or no bonuses (just specialization)?
2. **Decay system:** Should player-sold items decay over 7 days or stay forever?
3. **Enchantment retention:** Should player-sold enchanted items keep enchantments?
4. **NPC purchasing:** Should merchants buy from NPCs (background sim) or only from player?
5. **Limited gold:** Should merchant bank gold be limited (realistic) or infinite (player-friendly)?
6. **Quality pricing:** Should item quality affect buy/sell prices?

**Let me know which aspects excite you most and which need more refinement!** 🎮💰
