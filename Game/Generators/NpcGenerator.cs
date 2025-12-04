using Bogus;
using Game.Models;

namespace Game.Generators;

/// <summary>
/// Generates random NPCs using Bogus.
/// </summary>
public static class NpcGenerator
{
    private static readonly Faker<NPC> NpcFaker = new Faker<NPC>()
        .RuleFor(n => n.Id, f => Guid.NewGuid().ToString())
        .RuleFor(n => n.Name, f => f.Name.FullName())
        .RuleFor(n => n.Age, f => f.Random.Int(18, 80))
        .RuleFor(n => n.Occupation, f => f.Name.JobTitle())
        .RuleFor(n => n.Gold, f => f.Random.Int(10, 500))
        .RuleFor(n => n.Dialogue, f => f.Lorem.Sentence())
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
}
