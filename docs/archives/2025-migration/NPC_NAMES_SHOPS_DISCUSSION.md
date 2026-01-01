# NPC Deep Dive Discussion: Names & Occupations

**Date**: 2025-12-18
**Context**: Option A approved, now refining details

---

## ✅ Confirmed: Option A Structure

```
npcs/
├── .cbconfig.json
├── catalog.json        ← backgrounds{} + occupations{} separate
├── traits.json         ← personality_traits{} + quirks{}
├── names.json          ← Pattern-based generation
└── dialogue/
    ├── greetings.json
    ├── farewells.json
    ├── rumors.json
    └── styles.json
```

---

## Discussion Topic 2: Names.json - Title Auto-Matching

### The Question
Should titles in names.json automatically filter by social class/occupation?

**Example:**
```json
{
  "value": "Sir",
  "rarityWeight": 40,
  "socialClass": "noble",  // ← Should this restrict to noble backgrounds?
  "gender": "male"
}
```

### Option 2A: Titles Match Social Class (AUTOMATIC FILTERING)

**Implementation:**
```json
// names.json
{
  "components": {
    "title": [
      // Noble titles
      { "value": "Sir", "rarityWeight": 40, "gender": "male", "socialClass": "noble" },
      { "value": "Lady", "rarityWeight": 40, "gender": "female", "socialClass": "noble" },
      { "value": "Lord", "rarityWeight": 50, "gender": "male", "socialClass": "noble" },
      
      // Craftsman titles
      { "value": "Master", "rarityWeight": 30, "gender": "male", "socialClass": "craftsman" },
      { "value": "Mistress", "rarityWeight": 30, "gender": "female", "socialClass": "craftsman" },
      
      // Military titles
      { "value": "Captain", "rarityWeight": 35, "occupation": "military" },
      { "value": "Sergeant", "rarityWeight": 25, "occupation": "military" },
      
      // Religious titles
      { "value": "Brother", "rarityWeight": 25, "gender": "male", "occupation": "priest" },
      { "value": "Sister", "rarityWeight": 25, "gender": "female", "occupation": "priest" },
      
      // Criminal titles
      { "value": "Boss", "rarityWeight": 50, "occupation": "criminal_leader" },
      
      // Scholar titles
      { "value": "Professor", "rarityWeight": 40, "occupation": "scholar" },
      { "value": "Sage", "rarityWeight": 45, "occupation": "wizard" }
    ]
  }
}
```

**Generator Logic:**
```csharp
// When generating name:
var npc = new NPC {
    Background = "Noble-born",
    Occupation = "Knight",
    Gender = "male"
};

// Filter titles that match
var validTitles = names.components.title.Where(t =>
    (t.socialClass == null || t.socialClass == npc.Background.SocialClass) &&
    (t.occupation == null || t.occupation == npc.Occupation.Type) &&
    (t.gender == null || t.gender == npc.Gender)
);

// Result: Can use "Sir", "Lord" but NOT "Master", "Brother"
```

**Pros:**
- ✅ Realistic: "Sir" only for nobles, "Master" only for craftsmen
- ✅ Automatic enforcement of social norms
- ✅ Prevents weird combinations like "Sir Bob the Thief"

**Cons:**
- ❌ Less flexibility for creative NPCs
- ❌ Need to maintain socialClass/occupation mappings
- ❌ What about fallen nobles? (Noble-born Thief can't use "Sir"?)

---

### Option 2B: Titles Are Suggestions (SOFT FILTERING)

**Implementation:**
```json
{
  "components": {
    "title": [
      {
        "value": "Sir",
        "rarityWeight": 40,
        "gender": "male",
        "preferredSocialClass": "noble",  // ← Suggestion, not requirement
        "weightMultiplier": {
          "noble": 1.0,      // Normal weight if noble
          "common": 0.1,     // Very rare if commoner
          "criminal": 0.01   // Almost impossible if criminal
        }
      }
    ]
  }
}
```

**Generator Logic:**
```csharp
// Adjust rarityWeight based on NPC background
var adjustedWeight = title.rarityWeight * 
    title.weightMultiplier.GetValueOrDefault(npc.SocialClass, 1.0);

// Still possible to get "Sir Bob the Thief" but EXTREMELY rare
```

**Pros:**
- ✅ Flexible: allows edge cases
- ✅ Realistic distribution (mostly nobles get "Sir")
- ✅ Can have "Former Noble Thief" use "Sir" (fallen noble)

**Cons:**
- More complex logic
- Still might generate odd combinations occasionally

---

### Option 2C: No Filtering (PURE RANDOM)

**Implementation:**
```json
{
  "components": {
    "title": [
      { "value": "Sir", "rarityWeight": 40, "gender": "male" },
      { "value": "Master", "rarityWeight": 30, "gender": "male" }
      // No social class restrictions
    ]
  }
}
```

**Pros:**
- ✅ Maximum flexibility
- ✅ Simplest implementation
- ✅ Allows creative/humorous NPCs

**Cons:**
- ❌ Unrealistic: commoner farmers named "Lord Bob"
- ❌ Breaks immersion

---

### **Recommendation: Option 2B (Soft Filtering with Weight Multipliers)**

**Why:**
1. **Realistic but flexible**: Nobles usually get "Sir", but edge cases possible
2. **Handles fallen nobles**: "Noble-born Thief" can still use "Sir" (rare but possible)
3. **Handles risen commoners**: "Commoner Knight" can earn "Sir" (rare but possible)
4. **Prevents immersion breaks**: Won't see "Lord Bob the Farmer" (weight = 0.01)

**Example Generator Output:**
- **Noble Knight**: "Sir Aldric Ironforge" (high probability)
- **Commoner Blacksmith**: "Master Cole Smith" (high probability)
- **Fallen Noble Thief**: "Sir Damien Blackwood" (low but possible)
- **Commoner Farmer**: "Bob Miller" (no title, or very rarely "Master Bob")

---

### Discussion Questions:

**2.1: Should titles be purely decorative or affect gameplay?**

**Option A: Decorative Only**
- Title is just part of name string
- No mechanical effects

**Option B: Functional**
```json
{
  "value": "Sir",
  "traits": {
    "reputationBonus": 10,
    "persuasionBonus": 5,
    "merchantPriceModifier": 0.95  // 5% discount
  }
}
```
- Title grants bonuses
- NPCs react differently to titled characters

**2.2: Can titles be earned/lost in-game?**
- Start as "Bob Smith"
- Get knighted → become "Sir Bob Smith"
- Get disgraced → lose "Sir", back to "Bob Smith"

**2.3: Multiple titles?**
```json
// Pattern allows:
"{title1} {title2} {first_name} {surname} {suffix}"
// Result: "Captain Sir Aldric Ironforge the Brave"
```

**2.4: Should certain patterns exclude titles?**
```json
{
  "template": "{first_name}",  // Single name commoners
  "excludeTitles": true,       // Never use titles
  "socialClass": "common"
}
```

---

## Discussion Topic 4: Occupations & Shop Types

### The Question
Should occupations link to shop inventory/mechanics?

**Current occupations.json has:**
```json
{
  "name": "Weaponsmith",
  "traits": {
    "shopDiscount": 15,
    "weaponQuality": 30,
    "canCraftWeapons": true,
    "specialization": "weapons"
  }
}
```

### Option 4A: Occupations Define Shop Inventory (LINKED)

**Implementation:**
```json
// catalog.json - occupations
{
  "occupations": {
    "craftsmen": {
      "items": [
        {
          "name": "Blacksmith",
          "rarityWeight": 20,
          "skillBonuses": ["smithing", "crafting"],
          "baseGold": "3d10",
          "shopType": "smithy",
          "shopInventory": {
            "categories": ["weapons.melee", "armor.heavy"],
            "itemCount": "2d6+5",
            "qualityModifier": 1.2,
            "priceModifier": 1.0
          },
          "canCraft": ["weapons", "armor"],
          "craftingQualityBonus": 30
        },
        {
          "name": "Apothecary",
          "rarityWeight": 25,
          "skillBonuses": ["healing", "herbalism"],
          "baseGold": "2d10",
          "shopType": "apothecary",
          "shopInventory": {
            "categories": ["consumables.potions", "consumables.herbs"],
            "itemCount": "1d10+3",
            "qualityModifier": 1.5,
            "priceModifier": 0.95
          }
        },
        {
          "name": "Jeweler",
          "rarityWeight": 45,
          "skillBonuses": ["crafting", "appraisal"],
          "baseGold": "5d10",
          "shopType": "jewelry_shop",
          "shopInventory": {
            "categories": ["accessories.rings", "accessories.amulets"],
            "itemCount": "1d6+2",
            "qualityModifier": 2.0,
            "priceModifier": 1.3
          },
          "canEnchant": true
        }
      ]
    },
    "merchants": {
      "items": [
        {
          "name": "GeneralMerchant",
          "displayName": "General Merchant",
          "rarityWeight": 10,
          "skillBonuses": ["persuasion", "appraisal"],
          "baseGold": "5d10",
          "shopType": "general_store",
          "shopInventory": {
            "categories": ["all"],
            "itemCount": "3d10+10",
            "qualityModifier": 1.0,
            "priceModifier": 1.1,
            "varietyBonus": 50  // More diverse inventory
          }
        },
        {
          "name": "TavernKeeper",
          "rarityWeight": 15,
          "shopType": "tavern",
          "shopInventory": {
            "categories": ["consumables.food", "consumables.drink"],
            "itemCount": "2d6+5",
            "qualityModifier": 0.8,
            "priceModifier": 0.9
          },
          "services": ["lodging", "rumors", "quests"]
        }
      ]
    },
    "non_merchants": {
      "items": [
        {
          "name": "Guard",
          "rarityWeight": 28,
          "shopType": null,  // No shop
          "skillBonuses": ["melee_combat", "perception"]
        }
      ]
    }
  }
}
```

**Game Logic:**
```csharp
// When player interacts with NPC:
if (npc.Occupation.ShopType != null)
{
    var inventory = GenerateShopInventory(
        categories: npc.Occupation.ShopInventory.Categories,
        itemCount: npc.Occupation.ShopInventory.ItemCount,
        qualityModifier: npc.Occupation.ShopInventory.QualityModifier
    );
    
    OpenShopUI(npc, inventory);
}
else
{
    // Regular dialogue
    ShowDialogue(npc);
}
```

**Pros:**
- ✅ Occupation directly defines what NPC sells
- ✅ Automatic shop inventory generation
- ✅ Realistic: blacksmiths sell weapons/armor, apothecaries sell potions
- ✅ Easy to balance (modify occupation, all blacksmiths update)

**Cons:**
- ❌ Tightly couples occupation to shop mechanics
- ❌ What if NPC has occupation but no shop? (traveling blacksmith)
- ❌ What if same occupation, different inventory? (weapon specialist vs armor specialist)

---

### Option 4B: Occupations Suggest Shop Type (LOOSE COUPLING)

**Implementation:**
```json
// catalog.json
{
  "occupations": {
    "craftsmen": {
      "items": [
        {
          "name": "Blacksmith",
          "rarityWeight": 20,
          "skillBonuses": ["smithing"],
          "suggestedShopType": "smithy",  // ← Suggestion only
          "canCraft": ["weapons", "armor"]
        }
      ]
    }
  }
}
```

**Shop Config (Separate File):**
```json
// shops/shop_types.json
{
  "smithy": {
    "categories": ["weapons.melee", "armor.heavy"],
    "itemCount": "2d6+5",
    "qualityModifier": 1.2
  },
  "apothecary": {
    "categories": ["consumables.potions"],
    "itemCount": "1d10+3"
  }
}
```

**NPC Instance (Generated):**
```json
{
  "name": "Gareth Ironforge",
  "background": "Former Soldier",
  "occupation": "Blacksmith",
  "hasShop": true,           // ← Randomly determined
  "shopType": "smithy",      // ← From occupation suggestion
  "shopInventory": [...]     // ← Generated from shop_types.json
}
```

**Pros:**
- ✅ Decoupled: occupation and shop are separate concerns
- ✅ Flexible: can have non-merchant blacksmiths
- ✅ Can customize individual shops (this blacksmith specializes in swords)

**Cons:**
- More files to manage
- More complex generation logic

---

### Option 4C: Occupations Have No Shop Data (PURE SEPARATION)

**Implementation:**
```json
// catalog.json - occupations are just skills/bonuses
{
  "name": "Blacksmith",
  "skillBonuses": ["smithing", "crafting"],
  "baseGold": "3d10"
  // No shop-related data
}
```

**Shop Assignment (Separate System):**
```csharp
// When generating town NPCs:
var blacksmith = GenerateNPC(occupation: "Blacksmith");

// Decide if this blacksmith has a shop (random or based on town size)
if (ShouldHaveShop(blacksmith, townSize))
{
    blacksmith.Shop = new Shop {
        Type = "smithy",
        Inventory = GenerateInventory("smithy")
    };
}
```

**Pros:**
- ✅ Maximum flexibility
- ✅ Occupations are pure data (skills/gold only)
- ✅ Shop is runtime decision (traveling vs settled NPC)

**Cons:**
- ❌ Requires separate shop generation system
- ❌ Harder to ensure consistency

---

### **Recommendation: Option 4A (Occupations Define Shop Inventory)**

**Why:**
1. **Realistic**: Occupation = what they sell (blacksmith → weapons/armor)
2. **Simple**: One file defines both NPC stats and shop
3. **Data-driven**: Easy to balance/modify
4. **Follows items/enemies pattern**: Catalog defines everything about the type

**With Flexibility:**
```json
{
  "name": "Blacksmith",
  "shopType": "smithy",
  "shopChance": 0.8,  // 80% of blacksmiths have shops, 20% are traveling/retired
  "shopInventory": {...}
}
```

**Generator can decide:**
- Town blacksmith → has shop
- Wandering blacksmith → no shop, but can still craft/repair
- Retired blacksmith → no shop, just dialogue

---

### Discussion Questions:

**4.1: Should background affect shop inventory?**

**Example:**
- **Noble-born Blacksmith**: Higher quality items, higher prices
- **Orphan Blacksmith**: Lower quality, cheaper prices
- **Former Soldier Blacksmith**: Specializes in weapons over armor

**Implementation:**
```json
// Background modifiers
{
  "name": "Noble-born",
  "shopModifiers": {
    "qualityBonus": 0.5,
    "priceMultiplier": 1.3,
    "rarityBonus": 10  // More rare items
  }
}
```

**4.2: Should personality traits affect shop prices?**

**Example:**
- **Greedy**: 20% higher prices
- **Generous**: 10% lower prices
- **Suspicious**: Won't sell to strangers (reputation check)

**4.3: Should occupations have "specializations"?**

**Example:**
```json
{
  "name": "Blacksmith",
  "specializations": [
    {
      "name": "WeaponSpecialist",
      "rarityWeight": 40,
      "shopInventory": {
        "categories": ["weapons.melee"],  // Only weapons
        "qualityBonus": 20
      }
    },
    {
      "name": "ArmorSpecialist",
      "rarityWeight": 40,
      "shopInventory": {
        "categories": ["armor.heavy", "armor.medium"],  // Only armor
        "qualityBonus": 20
      }
    },
    {
      "name": "Generalist",
      "rarityWeight": 20,
      "shopInventory": {
        "categories": ["weapons.melee", "armor.all"]  // Both
      }
    }
  ]
}
```

**Generator picks specialization:**
- "Gareth Ironforge, Weapon Specialist" → only sells weapons
- "Marcus Smith, Armor Specialist" → only sells armor

**4.4: Dynamic vs Static Inventory?**

**Static (from catalog):**
```json
{
  "shopInventory": {
    "items": ["Longsword", "Greatsword", "Chainmail"],  // Fixed list
    "refreshDaily": false
  }
}
```

**Dynamic (generated):**
```json
{
  "shopInventory": {
    "categories": ["weapons.melee"],
    "itemCount": "2d6+5",  // Random each visit
    "refreshDaily": true
  }
}
```

**Hybrid:**
```json
{
  "shopInventory": {
    "fixedItems": ["Longsword"],  // Always has this
    "randomCategories": ["weapons.melee"],  // Plus random items
    "randomItemCount": "1d6+2"
  }
}
```

---

## Summary of Recommendations

### Names Discussion (Topic 2):
✅ **Use Soft Filtering (Option 2B)**
- Titles have `preferredSocialClass` + `weightMultiplier`
- Nobles mostly get "Sir", but fallen nobles can keep it
- Commoners rarely get titles, but risen heroes can earn them

### Shop/Occupation Discussion (Topic 4):
✅ **Occupations Define Shop Inventory (Option 4A)**
- `shopType` links to shop mechanics
- `shopChance` determines if NPC actually has shop (0.8 = 80%)
- `shopInventory` defines categories/count/quality
- Background can modify shop stats (quality/price)
- Personality traits can affect prices/availability

---

## Next Steps / More Questions:

1. **Should we implement specializations for occupations?**
   - "Blacksmith: Weapon Specialist" vs "Blacksmith: Armor Specialist"

2. **How should background + occupation combine for gold calculation?**
   ```
   FinalGold = (Occupation.BaseGold + Background.GoldModifier) * Background.WealthMultiplier
   ```

3. **Should NPCs have multiple occupations?**
   - "Blacksmith and Part-time Bard"
   - Primary occupation (has shop) + secondary (side skill)

4. **Should traits affect shop inventory quality?**
   - "Perfectionist" blacksmith → +20% quality
   - "Lazy" blacksmith → -10% quality

**What are your thoughts on these recommendations? Anything you'd like to explore further?** 🤔
