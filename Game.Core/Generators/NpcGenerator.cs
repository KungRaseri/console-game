using Bogus;
using Game.Shared.Data.Models;
using Game.Shared.Models;
using Game.Core.Models;
using Game.Core.Services;
using Game.Core.Utilities;

namespace Game.Core.Generators;

/// <summary>
/// Generates random NPCs using Bogus and v4.0 catalog-based JSON data.
/// Uses backgrounds, occupations, traits, quirks, and pattern-based names.
/// </summary>
public static class NpcGenerator
{
    private static readonly Faker<NPC> NpcFaker = new Faker<NPC>()
        .RuleFor(n => n.Id, f => Guid.NewGuid().ToString())
        .RuleFor(n => n.Age, f => f.Random.Int(18, 80))
        .RuleFor(n => n.Gold, (f, npc) => GenerateStartingGold(f, npc))
        .RuleFor(n => n.Name, (f, npc) => GeneratePatternBasedName(f, npc))
        .RuleFor(n => n.Occupation, (f, npc) => AssignBackgroundAndOccupation(f, npc))
        .RuleFor(n => n.Dialogue, f => GenerateDialogue(f))
        .RuleFor(n => n.IsFriendly, (f, npc) => 
        {
            // Apply personality traits and quirks
            AssignTraitsAndQuirks(npc, f);
            
            // Determine friendliness from personality traits
            if (npc.Traits.ContainsKey("disposition"))
            {
                var disposition = npc.Traits["disposition"].AsString();
                return disposition == "friendly";
            }
            return f.Random.Bool(0.7f); // Default 70% friendly
        });

    /// <summary>
    /// Generate a single random NPC.
    /// </summary>
    public static NPC Generate()
    {
        return NpcFaker.Generate();
    }

    /// <summary>
    /// Generate multiple random NPCs.
    /// </summary>
    public static List<NPC> Generate(int count)
    {
        return NpcFaker.Generate(count);
    }
    
    /// <summary>
    /// Helper method to create a string trait value.
    /// </summary>
    private static TraitValue Str(string value) => new TraitValue(value, TraitType.String);
    
    /// <summary>
    /// Helper method to create a number trait value.
    /// </summary>
    private static TraitValue Num(int value) => new TraitValue(value, TraitType.Number);
    
    /// <summary>
    /// Helper method to create a decimal number trait value.
    /// </summary>
    private static TraitValue Num(double value) => new TraitValue(value, TraitType.Number);
    
    /// <summary>
    /// Helper method to create a boolean trait value.
    /// </summary>
    private static TraitValue Bool(bool value) => new TraitValue(value, TraitType.Boolean);
    
    /// <summary>
    /// Assign background and occupation to NPC, applying their traits.
    /// Returns the display name of the occupation.
    /// </summary>
    private static string AssignBackgroundAndOccupation(Faker faker, NPC npc)
    {
        var catalog = GameDataService.Instance.NpcCatalog;
        
        // Step 1: Select background using weighted rarity
        var allBackgrounds = catalog.Backgrounds.Values.SelectMany(list => list).ToList();
        var selectedBackground = SelectByRarityWeight(faker, allBackgrounds);
        
        if (selectedBackground != null)
        {
            npc.Traits["background"] = Str(selectedBackground.Name);
            npc.Traits["backgroundDisplayName"] = Str(selectedBackground.DisplayName);
            npc.Traits["socialClass"] = Str(selectedBackground.SocialClass);
            npc.Traits["startingGold"] = Num(selectedBackground.StartingGold);
            
            // Apply shop modifiers if present
            if (selectedBackground.ShopModifiers != null)
            {
                if (selectedBackground.ShopModifiers.PriceMultiplier.HasValue)
                    npc.Traits["backgroundPriceMultiplier"] = Num(selectedBackground.ShopModifiers.PriceMultiplier.Value);
                if (selectedBackground.ShopModifiers.BuyPriceMultiplier.HasValue)
                    npc.Traits["backgroundBuyPriceMultiplier"] = Num(selectedBackground.ShopModifiers.BuyPriceMultiplier.Value);
            }
        }
        
        // Step 2: Select occupation using weighted rarity
        var allOccupations = catalog.Occupations.Values.SelectMany(list => list).ToList();
        var selectedOccupation = SelectByRarityWeight(faker, allOccupations);
        
        if (selectedOccupation != null)
        {
            npc.Traits["occupation"] = Str(selectedOccupation.Name);
            npc.Traits["occupationDisplayName"] = Str(selectedOccupation.DisplayName);
            npc.Traits["isMerchant"] = Bool(selectedOccupation.IsMerchant);
            
            // Store shop configuration if merchant
            if (selectedOccupation.IsMerchant && selectedOccupation.Shop != null)
            {
                npc.Traits["shopInventoryType"] = Str(selectedOccupation.Shop.InventoryType);
                npc.Traits["shopRefreshSchedule"] = Str(selectedOccupation.Shop.RefreshSchedule);
            }
            
            // Store bank gold formula if present
            if (selectedOccupation.BankGoldFormula != null)
            {
                npc.Traits["bankGoldMultiplier"] = Num(selectedOccupation.BankGoldFormula.BaseMultiplier);
                npc.Traits["bankGoldBackgroundMultiplier"] = Num(selectedOccupation.BankGoldFormula.BackgroundMultiplier);
            }
            
            return selectedOccupation.DisplayName;
        }
        
        return faker.Name.JobTitle(); // Fallback
    }
    
    /// <summary>
    /// Generate starting gold based on background.
    /// </summary>
    private static int GenerateStartingGold(Faker faker, NPC npc)
    {
        if (npc.Traits.ContainsKey("startingGold"))
        {
            var baseGold = npc.Traits["startingGold"].AsInt();
            // Add some randomness (±20%)
            var variance = faker.Random.Int(-20, 20);
            return Math.Max(0, baseGold + (baseGold * variance / 100));
        }
        
        return faker.Random.Int(10, 100); // Default fallback
    }
    
    /// <summary>
    /// Assign personality traits and quirks to NPC.
    /// </summary>
    private static void AssignTraitsAndQuirks(NPC npc, Faker faker)
    {
        var traitsData = GameDataService.Instance.NpcTraits;
        
        // Select 1-3 personality traits using weighted rarity
        var traitCount = faker.Random.Int(1, 3);
        var allTraits = traitsData.PersonalityTraits.Values.SelectMany(list => list).ToList();
        
        for (int i = 0; i < traitCount && allTraits.Count > 0; i++)
        {
            var trait = SelectByRarityWeight(faker, allTraits);
            if (trait != null)
            {
                npc.Traits[$"personalityTrait_{i}"] = Str(trait.Name);
                npc.Traits[$"personalityTraitDisplay_{i}"] = Str(trait.DisplayName);
                
                // Apply shop modifiers from traits
                if (trait.ShopModifiers != null)
                {
                    if (trait.ShopModifiers.PriceMultiplier.HasValue)
                        npc.Traits["traitPriceMultiplier"] = Num(trait.ShopModifiers.PriceMultiplier.Value);
                    if (trait.ShopModifiers.BuyPriceMultiplier.HasValue)
                        npc.Traits["traitBuyPriceMultiplier"] = Num(trait.ShopModifiers.BuyPriceMultiplier.Value);
                    if (trait.ShopModifiers.QualityBonus.HasValue)
                        npc.Traits["traitQualityBonus"] = Num(trait.ShopModifiers.QualityBonus.Value);
                    if (trait.ShopModifiers.ReputationRequired.HasValue)
                        npc.Traits["traitReputationRequired"] = Num(trait.ShopModifiers.ReputationRequired.Value);
                }
                
                // Remove selected trait to avoid duplicates
                allTraits.Remove(trait);
            }
        }
        
        // Select 0-2 quirks using weighted rarity
        var quirkCount = faker.Random.Int(0, 2);
        var allQuirks = traitsData.Quirks.Values.SelectMany(list => list).ToList();
        
        for (int i = 0; i < quirkCount && allQuirks.Count > 0; i++)
        {
            var quirk = SelectByRarityWeight(faker, allQuirks);
            if (quirk != null)
            {
                npc.Traits[$"quirk_{i}"] = Str(quirk.Name);
                npc.Traits[$"quirkDisplay_{i}"] = Str(quirk.DisplayName);
                
                // Remove selected quirk to avoid duplicates
                allQuirks.Remove(quirk);
            }
        }
    }
    
    /// <summary>
    /// Generate a pattern-based name using soft filtering for titles.
    /// </summary>
    private static string GeneratePatternBasedName(Faker faker, NPC npc)
    {
        var namesData = GameDataService.Instance.NpcNames;
        
        if (namesData.Patterns.Count == 0 || namesData.Components.FirstName == null)
            return faker.Name.FullName(); // Fallback
        
        // Get social class for soft filtering
        var socialClass = npc.Traits.ContainsKey("socialClass") 
            ? npc.Traits["socialClass"].AsString() 
            : "common";
        
        // Select name pattern using weighted rarity
        var pattern = SelectByRarityWeight(faker, namesData.Patterns);
        if (pattern == null)
            return faker.Name.FullName(); // Fallback
        
        var template = pattern.Template;
        
        // Generate each component
        var title = GenerateTitleComponent(faker, namesData, socialClass, pattern);
        var firstName = GenerateFirstNameComponent(faker, namesData);
        var surname = GenerateSurnameComponent(faker, namesData);
        var suffix = GenerateSuffixComponent(faker, namesData);
        
        // Replace tokens in template
        var name = template
            .Replace("{title}", title)
            .Replace("{first_name}", firstName)
            .Replace("{surname}", surname)
            .Replace("{suffix}", suffix)
            .Trim();
        
        // Clean up extra spaces
        while (name.Contains("  "))
            name = name.Replace("  ", " ");
        
        return name;
    }
    
    private static string GenerateTitleComponent(Faker faker, NpcNamesData namesData, string socialClass, NamePattern pattern)
    {
        if (pattern.ExcludeTitles == true || namesData.Components.Title == null || namesData.Components.Title.Count == 0)
            return string.Empty;
        
        if (pattern.RequiresTitle != true && faker.Random.Bool(0.7f)) // 70% chance to skip title
            return string.Empty;
        
        // Apply soft filtering - adjust weights based on social class
        var adjustedTitles = namesData.Components.Title
            .Select(t => new
            {
                Title = t,
                AdjustedWeight = t.WeightMultiplier != null && t.WeightMultiplier.ContainsKey(socialClass)
                    ? (int)(t.RarityWeight * t.WeightMultiplier[socialClass])
                    : t.RarityWeight
            })
            .Where(t => t.AdjustedWeight > 0)
            .ToList();
        
        if (adjustedTitles.Count == 0)
            return string.Empty;
        
        var selected = SelectByRarityWeight(faker, adjustedTitles.Select(t => 
            new { Value = t.Title.Value, RarityWeight = t.AdjustedWeight }).ToList(),
            item => item.RarityWeight);
        
        return selected?.Value ?? string.Empty;
    }
    
    private static string GenerateFirstNameComponent(Faker faker, NpcNamesData namesData)
    {
        if (namesData.Components.FirstName == null)
            return faker.Name.FirstName(); // Fallback
        
        // Choose gender randomly
        var useFemale = faker.Random.Bool();
        var nameList = useFemale 
            ? namesData.Components.FirstName.Female?.Values.SelectMany(list => list).ToList()
            : namesData.Components.FirstName.Male?.Values.SelectMany(list => list).ToList();
        
        if (nameList == null || nameList.Count == 0)
            return faker.Name.FirstName(); // Fallback
        
        var selected = SelectByRarityWeight(faker, nameList);
        return selected?.Value ?? faker.Name.FirstName();
    }
    
    private static string GenerateSurnameComponent(Faker faker, NpcNamesData namesData)
    {
        if (namesData.Components.Surname == null)
            return faker.Name.LastName(); // Fallback
        
        var allSurnames = namesData.Components.Surname.Values.SelectMany(list => list).ToList();
        if (allSurnames.Count == 0)
            return faker.Name.LastName(); // Fallback
        
        var selected = SelectByRarityWeight(faker, allSurnames);
        return selected?.Value ?? faker.Name.LastName();
    }
    
    private static string GenerateSuffixComponent(Faker faker, NpcNamesData namesData)
    {
        if (namesData.Components.Suffix == null || namesData.Components.Suffix.Count == 0)
            return string.Empty;
        
        // Only 10% chance to have a suffix (epithets are rare)
        if (faker.Random.Bool(0.9f))
            return string.Empty;
        
        var selected = SelectByRarityWeight(faker, namesData.Components.Suffix);
        return selected?.Value ?? string.Empty;
    }
    
    /// <summary>
    /// Select an item from a list based on rarity weight (higher weight = rarer = less likely).
    /// </summary>
    private static T? SelectByRarityWeight<T>(Faker faker, List<T> items, Func<T, int>? weightSelector = null) where T : class
    {
        if (items.Count == 0)
            return null;
        
        // Get weights (use reflection to find RarityWeight property if no selector provided)
        var weights = new List<double>();
        foreach (var item in items)
        {
            int weight;
            if (weightSelector != null)
            {
                weight = weightSelector(item);
            }
            else
            {
                var prop = item.GetType().GetProperty("RarityWeight");
                weight = prop != null ? (int)(prop.GetValue(item) ?? 1) : 1;
            }
            
            // Convert rarity weight to selection probability (inverse relationship)
            // Higher weight = rarer = lower probability
            // Formula: probability = 100 / weight
            var probability = 100.0 / Math.Max(1, weight);
            weights.Add(probability);
        }
        
        // Normalize weights to sum to 1.0
        var totalWeight = weights.Sum();
        var normalizedWeights = weights.Select(w => w / totalWeight).ToList();
        
        // Weighted random selection
        var random = faker.Random.Double();
        var cumulativeProbability = 0.0;
        
        for (int i = 0; i < items.Count; i++)
        {
            cumulativeProbability += normalizedWeights[i];
            if (random <= cumulativeProbability)
                return items[i];
        }
        
        return items[items.Count - 1]; // Fallback
    }
    
    /// <summary>
    /// Generate dialogue using JSON templates.
    /// </summary>
    private static string GenerateDialogue(Faker faker)
    {
        var data = GameDataService.Instance.DialogueTemplates;
        
        // Combine all dialogue categories
        var allDialogue = new List<string>();
        allDialogue.AddRange(data.Greetings);
        allDialogue.AddRange(data.Merchants);
        allDialogue.AddRange(data.Quests);
        allDialogue.AddRange(data.Rumors);
        allDialogue.AddRange(data.Farewells);
        allDialogue.AddRange(data.Friendly);
        
        if (allDialogue.Count > 0)
        {
            return GameDataService.GetRandom(allDialogue);
        }
        
        return faker.Lorem.Sentence();
    }
}
