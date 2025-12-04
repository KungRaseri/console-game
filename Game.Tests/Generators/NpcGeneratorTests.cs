using FluentAssertions;
using Game.Generators;
using Game.Models;
using Xunit;

namespace Game.Tests.Generators;

public class NpcGeneratorTests
{
    [Fact]
    public void Generate_Should_Create_Valid_NPC()
    {
        // Act
        var npc = NpcGenerator.Generate();

        // Assert
        npc.Should().NotBeNull();
        npc.Id.Should().NotBeNullOrEmpty();
        npc.Name.Should().NotBeNullOrEmpty();
        npc.Occupation.Should().NotBeNullOrEmpty();
        npc.Age.Should().BeInRange(18, 100);
        npc.Gold.Should().BeInRange(0, 1000);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(25)]
    public void Generate_Should_Create_Requested_Number_Of_NPCs(int count)
    {
        // Act
        var npcs = NpcGenerator.Generate(count);

        // Assert
        npcs.Should().HaveCount(count);
        npcs.Should().OnlyHaveUniqueItems(n => n.Id);
    }

    [Fact]
    public void Generated_NPCs_Should_Have_Realistic_Data()
    {
        // Act
        var npcs = NpcGenerator.Generate(10);

        // Assert
        foreach (var npc in npcs)
        {
            npc.Name.Should().NotBeEmpty()
                .And.NotContain("null");
            npc.Occupation.Should().NotBeEmpty()
                .And.NotContain("null");
            npc.Age.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public void Generate_Should_Create_NPCs_With_Varied_Ages()
    {
        // Act
        var npcs = NpcGenerator.Generate(50);
        var ages = npcs.Select(n => n.Age).Distinct().ToList();

        // Assert - with 50 NPCs, we should see variety in ages
        ages.Should().HaveCountGreaterThan(10);
    }

    [Fact]
    public void Generate_Should_Create_NPCs_With_Varied_Gold()
    {
        // Act
        var npcs = NpcGenerator.Generate(50);
        var goldValues = npcs.Select(n => n.Gold).Distinct().ToList();

        // Assert - with 50 NPCs, we should see variety in gold
        goldValues.Should().HaveCountGreaterThan(10);
    }
}
