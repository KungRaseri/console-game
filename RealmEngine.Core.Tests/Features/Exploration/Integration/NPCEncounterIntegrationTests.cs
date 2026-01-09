using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RealmEngine.Core.Abstractions;
using RealmEngine.Core.Features.Exploration.Commands;
using RealmEngine.Core.Features.SaveLoad;
using RealmEngine.Shared.Abstractions;
using RealmEngine.Shared.Models;
using Xunit;

namespace RealmEngine.Core.Tests.Features.Exploration.Integration;

/// <summary>
/// Integration tests for NPC encounter system.
/// Tests: Encounter NPC → Check merchant trait → Show appropriate actions.
/// </summary>
[Trait("Category", "Integration")]
public class NPCEncounterIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IMediator _mediator;
    private readonly SaveGameService _saveGameService;
    private readonly SaveGame _testSaveGame;
    private readonly NPC _testMerchant;
    private readonly NPC _testVillager;

    public NPCEncounterIntegrationTests()
    {
        var services = new ServiceCollection();

        // Mock IApocalypseTimer
        var mockTimer = new Mock<IApocalypseTimer>();
        mockTimer.Setup(t => t.GetBonusMinutes()).Returns(0);
        services.AddSingleton(mockTimer.Object);

        // Mock IGameUI
        var mockConsole = new Mock<IGameUI>();
        services.AddSingleton(mockConsole.Object);

        // Mock repository for testing
        var mockRepository = new Mock<ISaveGameRepository>();
        mockRepository.Setup(r => r.SaveGame(It.IsAny<SaveGame>()))
            .Callback<SaveGame>(s => { /* No-op for tests */ });

        // Register logging
        services.AddLogging();

        // Register MediatR with exploration command handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EncounterNPCCommand).Assembly));

        // Register core services
        services.AddSingleton(mockRepository.Object);
        services.AddSingleton<SaveGameService>();
        services.AddSingleton<ISaveGameService>(sp => sp.GetRequiredService<SaveGameService>());

        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
        _saveGameService = _serviceProvider.GetRequiredService<SaveGameService>();

        // Create test save game with NPCs
        _testMerchant = new NPC
        {
            Id = "merchant-001",
            Name = "Tharin the Blacksmith",
            Occupation = "Blacksmith",
            Age = 45,
            Gold = 5000,
            IsFriendly = true,
            Dialogue = "Welcome to my forge!",
            Traits = new Dictionary<string, TraitValue>
            {
                ["isMerchant"] = new TraitValue(true, TraitType.Boolean),
                ["shopType"] = new TraitValue("weapons", TraitType.String),
                ["shopInventoryType"] = new TraitValue("hybrid", TraitType.String)
            }
        };

        _testVillager = new NPC
        {
            Id = "villager-001",
            Name = "Elara the Farmer",
            Occupation = "Farmer",
            Age = 32,
            Gold = 50,
            IsFriendly = true,
            Dialogue = "Good day to you!",
            Traits = new Dictionary<string, TraitValue>()
        };

        _testSaveGame = new SaveGame
        {
            Character = new Character
            {
                Name = "TestHero",
                Level = 1,
                Gold = 1000
            }
        };

        _testSaveGame.KnownNPCs.Add(_testMerchant);
        _testSaveGame.KnownNPCs.Add(_testVillager);

        _saveGameService.SetCurrentSave(_testSaveGame);
    }

    [Fact]
    public async Task Should_Encounter_NPC_Successfully()
    {
        // Act
        var result = await _mediator.Send(new EncounterNPCCommand("merchant-001"));

        // Assert
        result.Success.Should().BeTrue();
        result.Npc.Should().NotBeNull();
        result.Npc!.Name.Should().Be("Tharin the Blacksmith");
        result.AvailableActions.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_Show_Trade_Action_For_Merchant()
    {
        // Act
        var result = await _mediator.Send(new EncounterNPCCommand("merchant-001"));

        // Assert
        result.Success.Should().BeTrue();
        result.AvailableActions.Should().Contain("Trade");
        result.AvailableActions.Should().Contain("Talk");
        result.AvailableActions.Should().Contain("Leave");
    }

    [Fact]
    public async Task Should_Not_Show_Trade_Action_For_Non_Merchant()
    {
        // Act
        var result = await _mediator.Send(new EncounterNPCCommand("villager-001"));

        // Assert
        result.Success.Should().BeTrue();
        result.AvailableActions.Should().NotContain("Trade");
        result.AvailableActions.Should().Contain("Talk");
        result.AvailableActions.Should().Contain("Leave");
    }

    [Fact]
    public async Task Should_Fail_When_NPC_Not_Found()
    {
        // Act
        var result = await _mediator.Send(new EncounterNPCCommand("unknown-npc"));

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task Should_Return_Correct_NPC_Details()
    {
        // Act
        var result = await _mediator.Send(new EncounterNPCCommand("villager-001"));

        // Assert
        result.Success.Should().BeTrue();
        result.Npc.Should().NotBeNull();
        result.Npc!.Name.Should().Be("Elara the Farmer");
        result.Npc.Occupation.Should().Be("Farmer");
        result.Npc.Dialogue.Should().Be("Good day to you!");
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
