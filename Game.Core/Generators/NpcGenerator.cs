using Bogus;
using Game.Shared.Data.Models;
using Game.Core.Models;
using Game.Shared.Services;
using Game.Core.Utilities;

namespace Game.Core.Generators;

/// <summary>
/// Generates random NPCs using Bogus and JSON data for fantasy names and occupations.
/// </summary>
public static class NpcGenerator
{
    private static readonly Faker<NPC> NpcFaker = new Faker<NPC>()
        .RuleFor(n => n.Id, f => Guid.NewGuid().ToString())
        .RuleFor(n => n.Name, f => GenerateFantasyName(f))
        .RuleFor(n => n.Age, f => f.Random.Int(18, 80))
        .RuleFor(n => n.Occupation, (f, npc) => GenerateOccupationAndApplyTraits(f, npc))
        .RuleFor(n => n.Gold, f => f.Random.Int(10, 500))
        .RuleFor(n => n.Dialogue, f => GenerateDialogue(f))
        .RuleFor(n => n.IsFriendly, (f, npc) => 
        {
            // Apply dialogue personality traits after occupation
            ApplyDialogueTraits(npc, f);
            
            // Determine friendliness from disposition trait
            if (npc.Traits.ContainsKey("disposition"))
            {
                var disposition = npc.Traits["disposition"].AsString();
                return disposition == "friendly";
            }
            return f.Random.Bool(0.8f); // Default 80% friendly
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
    /// Generate a fantasy name using JSON data.
    /// </summary>
    private static string GenerateFantasyName(Faker faker)
    {
        var data = GameDataService.Instance.FantasyNames;
        
        // Choose gender randomly
        var useFemaleName = faker.Random.Bool();
        
        string firstName;
        if (useFemaleName && data.Female.Count > 0)
        {
            firstName = GameDataService.GetRandom(data.Female);
        }
        else if (data.Male.Count > 0)
        {
            firstName = GameDataService.GetRandom(data.Male);
        }
        else
        {
            firstName = faker.Name.FirstName();
        }
        
        var surname = data.Surnames.Count > 0
            ? GameDataService.GetRandom(data.Surnames)
            : faker.Name.LastName();
        
        return $"{firstName} {surname}";
    }
    
    /// <summary>
    /// Generate an occupation using JSON data and apply occupation-based traits.
    /// </summary>
    private static string GenerateOccupationAndApplyTraits(Faker faker, NPC npc)
    {
        var data = GameDataService.Instance.Occupations;
        
        // Pick a random category
        var categories = new List<Dictionary<string, OccupationTraitData>>
        {
            data.Merchants,
            data.Craftsmen,
            data.Professionals,
            data.Service,
            data.Nobility,
            data.Religious,
            data.Adventurers,
            data.Magical,
            data.Criminal,
            data.Common
        };
        
        // Filter out empty categories
        var validCategories = categories.Where(c => c.Count > 0).ToList();
        
        if (validCategories.Count == 0)
        {
            return faker.Name.JobTitle();
        }
        
        var selectedCategory = faker.PickRandom(validCategories);
        var occupation = faker.PickRandom(selectedCategory.Values.ToList());
        
        // Apply occupation traits to NPC
        TraitApplicator.ApplyTraits(npc, occupation.Traits);
        
        return occupation.DisplayName;
    }
    
    /// <summary>
    /// Apply dialogue personality traits to an NPC.
    /// </summary>
    private static void ApplyDialogueTraits(NPC npc, Faker faker)
    {
        var data = GameDataService.Instance.DialogueTraits;
        
        if (data.Count == 0)
            return;
        
        // Pick a random personality archetype
        var personalityOptions = data.Values.ToList();
        
        // Adjust personality selection based on occupation traits if available
        // For example, merchants might be more likely to be greedy or cunning
        if (npc.Traits.ContainsKey("merchantSkill"))
        {
            // Increase chance of greedy or cunning personality
            if (faker.Random.Bool(0.4f)) // 40% chance
            {
                var greedyOrCunning = personalityOptions
                    .Where(p => p.DisplayName == "Greedy" || p.DisplayName == "Cunning")
                    .ToList();
                if (greedyOrCunning.Count > 0)
                {
                    var personality = faker.PickRandom(greedyOrCunning);
                    TraitApplicator.ApplyTraits(npc, personality.Traits);
                    return;
                }
            }
        }
        
        // If noble or religious occupation, increase chance of noble/wise personality
        if (npc.Occupation.Contains("Lord") || npc.Occupation.Contains("Priest") || 
            npc.Occupation.Contains("Paladin") || npc.Occupation.Contains("Cleric"))
        {
            if (faker.Random.Bool(0.5f)) // 50% chance
            {
                var nobleOrWise = personalityOptions
                    .Where(p => p.DisplayName == "Noble" || p.DisplayName == "Wise" || p.DisplayName == "Humble")
                    .ToList();
                if (nobleOrWise.Count > 0)
                {
                    var personality = faker.PickRandom(nobleOrWise);
                    TraitApplicator.ApplyTraits(npc, personality.Traits);
                    return;
                }
            }
        }
        
        // Default: pick any random personality
        var randomPersonality = faker.PickRandom(personalityOptions);
        TraitApplicator.ApplyTraits(npc, randomPersonality.Traits);
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
