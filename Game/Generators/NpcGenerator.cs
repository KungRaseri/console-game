using Bogus;
using Game.Data.Models;
using Game.Models;
using Game.Services;

namespace Game.Generators;

/// <summary>
/// Generates random NPCs using Bogus and JSON data for fantasy names and occupations.
/// </summary>
public static class NpcGenerator
{
    private static readonly Faker<NPC> NpcFaker = new Faker<NPC>()
        .RuleFor(n => n.Id, f => Guid.NewGuid().ToString())
        .RuleFor(n => n.Name, f => GenerateFantasyName(f))
        .RuleFor(n => n.Age, f => f.Random.Int(18, 80))
        .RuleFor(n => n.Occupation, f => GenerateOccupation(f))
        .RuleFor(n => n.Gold, f => f.Random.Int(10, 500))
        .RuleFor(n => n.Dialogue, f => GenerateDialogue(f))
        .RuleFor(n => n.IsFriendly, f => f.Random.Bool(0.8f)); // 80% friendly

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
    /// Generate an occupation using JSON data.
    /// </summary>
    private static string GenerateOccupation(Faker faker)
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
        return occupation.DisplayName;
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
