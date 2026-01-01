using FluentAssertions;
using Game.Shared.Models;

namespace Game.Tests.Models;

[Trait("Category", "Unit")]
public class NPCTests
{
    [Fact]
    public void NPC_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var npc = new NPC();

        // Assert
        npc.Id.Should().NotBeNullOrEmpty();
        npc.Name.Should().Be(string.Empty);
        npc.Age.Should().Be(0);
        npc.Occupation.Should().Be(string.Empty);
        npc.Gold.Should().Be(0);
        npc.Dialogue.Should().Be(string.Empty);
        npc.IsFriendly.Should().BeTrue();
    }

    [Fact]
    public void NPC_Should_Generate_Unique_Ids()
    {
        // Arrange & Act
        var npc1 = new NPC();
        var npc2 = new NPC();

        // Assert
        npc1.Id.Should().NotBe(npc2.Id);
    }

    [Fact]
    public void NPC_Properties_Should_Be_Settable()
    {
        // Arrange
        var npc = new NPC();

        // Act
        npc.Name = "Marcus the Merchant";
        npc.Age = 45;
        npc.Occupation = "Trader";
        npc.Gold = 500;
        npc.Dialogue = "Welcome to my shop!";
        npc.IsFriendly = true;

        // Assert
        npc.Name.Should().Be("Marcus the Merchant");
        npc.Age.Should().Be(45);
        npc.Occupation.Should().Be("Trader");
        npc.Gold.Should().Be(500);
        npc.Dialogue.Should().Be("Welcome to my shop!");
        npc.IsFriendly.Should().BeTrue();
    }

    [Fact]
    public void NPC_Can_Be_Hostile()
    {
        // Arrange & Act
        var npc = new NPC { IsFriendly = false };

        // Assert
        npc.IsFriendly.Should().BeFalse();
    }
}
