using FluentAssertions;
using Game.Core.Features.Combat;
using Game.Console.UI;
using Game.Core.Abstractions;
using Game.Tests.Helpers;
using Spectre.Console.Testing;
using Game.Core.Features.Combat.Commands.AttackEnemy;
using Game.Core.Features.SaveLoad;
using Game.Core.Models;
using Game.Core.Services;
using Game.Shared.Services;
using MediatR;
using Moq;
using Xunit;

namespace Game.Tests.Features.Combat.Commands;

/// <summary>
/// Tests for AttackEnemyHandler.
/// </summary>
public class AttackEnemyHandlerTests : IDisposable
{
    private readonly Mock<CombatService> _combatServiceMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<SaveGameService> _saveGameServiceMock;
    private readonly AttackEnemyHandler _handler;
    private readonly TestConsole _testConsole;
    private readonly ConsoleUI _consoleUI;
    private readonly string _testDbPath;

    public AttackEnemyHandlerTests()
    {
        // Use unique database path for each test instance to avoid file locking
        _testDbPath = $"test_attack_{Guid.NewGuid()}.db";
        
        _testConsole = TestConsoleHelper.CreateInteractiveConsole();
        _consoleUI = new ConsoleUI(_testConsole);
        
        var apocalypseTimer = new ApocalypseTimer(_consoleUI);
        
        // Create a single mock instance to avoid creating multiple database connections
        _saveGameServiceMock = new Mock<SaveGameService>(MockBehavior.Loose, apocalypseTimer, _testDbPath);
        _combatServiceMock = new Mock<CombatService>(MockBehavior.Loose, _saveGameServiceMock.Object);
        _mediatorMock = new Mock<IMediator>();
        
        // Setup default difficulty settings
        _saveGameServiceMock
            .Setup(x => x.GetDifficultySettings())
            .Returns(DifficultySettings.Normal);
        
        _handler = new AttackEnemyHandler(_combatServiceMock.Object, _mediatorMock.Object, _saveGameServiceMock.Object);
    }

    public void Dispose()
    {
        // Dispose of the real SaveGameService instance created in the mock
        _saveGameServiceMock?.Object?.Dispose();
        
        // Clean up test database files
        try
        {
            if (File.Exists(_testDbPath))
                File.Delete(_testDbPath);
            
            var logFile = _testDbPath.Replace(".db", "-log.db");
            if (File.Exists(logFile))
                File.Delete(logFile);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task Handle_Should_Deal_Damage_To_Enemy()
    {
        // Arrange
        var player = new Character { Name = "Hero", Strength = 10 };
        var enemy = new Enemy { Name = "Goblin", Health = 50, MaxHealth = 50 };
        var combatLog = new CombatLog();
        
        _combatServiceMock
            .Setup(x => x.ExecutePlayerAttack(player, enemy, false))
            .Returns(new CombatResult { Damage = 20, IsCritical = false });

        var command = new AttackEnemyCommand 
        { 
            Player = player, 
            Enemy = enemy, 
            CombatLog = combatLog 
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Damage.Should().Be(20);
        result.IsCritical.Should().BeFalse();
        result.IsEnemyDefeated.Should().BeFalse();
        enemy.Health.Should().Be(30); // 50 - 20 = 30
        combatLog.Entries.Should().Contain(e => e.Message.Contains("Hero attacks for 20 damage"));
    }

    [Fact]
    public async Task Handle_Should_Mark_Enemy_As_Defeated_When_Health_Reaches_Zero()
    {
        // Arrange
        var player = new Character 
        { 
            Name = "Hero", 
            Strength = 20, 
            Experience = 0, 
            Gold = 0 
        };
        var enemy = new Enemy 
        { 
            Name = "Goblin", 
            Health = 10, 
            MaxHealth = 50,
            XPReward = 50,
            GoldReward = 25
        };
        var combatLog = new CombatLog();
        
        _combatServiceMock
            .Setup(x => x.ExecutePlayerAttack(player, enemy, false))
            .Returns(new CombatResult { Damage = 15, IsCritical = false });

        var command = new AttackEnemyCommand 
        { 
            Player = player, 
            Enemy = enemy, 
            CombatLog = combatLog 
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsEnemyDefeated.Should().BeTrue();
        result.ExperienceGained.Should().Be(50);
        result.GoldGained.Should().Be(25);
        player.Experience.Should().Be(50);
        player.Gold.Should().Be(25);
        enemy.Health.Should().BeLessThanOrEqualTo(0);
        combatLog.Entries.Should().Contain(e => e.Message.Contains("defeated"));
    }

    [Fact]
    public async Task Handle_Should_Mark_Critical_Hit()
    {
        // Arrange
        var player = new Character { Name = "Hero", Strength = 10 };
        var enemy = new Enemy { Name = "Goblin", Health = 50, MaxHealth = 50 };
        
        _combatServiceMock
            .Setup(x => x.ExecutePlayerAttack(player, enemy, false))
            .Returns(new CombatResult { Damage = 30, IsCritical = true });

        var command = new AttackEnemyCommand 
        { 
            Player = player, 
            Enemy = enemy 
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsCritical.Should().BeTrue();
        result.Damage.Should().Be(30);
    }

    [Fact]
    public async Task Handle_Should_Publish_Events_When_Enemy_Defeated()
    {
        // Arrange
        var player = new Character 
        { 
            Name = "Hero", 
            Strength = 20, 
            Experience = 0, 
            Gold = 0 
        };
        var enemy = new Enemy 
        { 
            Name = "Goblin", 
            Health = 5, 
            MaxHealth = 50,
            XPReward = 50,
            GoldReward = 25
        };
        
        _combatServiceMock
            .Setup(x => x.ExecutePlayerAttack(player, enemy, false))
            .Returns(new CombatResult { Damage = 10, IsCritical = false });

        var command = new AttackEnemyCommand 
        { 
            Player = player, 
            Enemy = enemy 
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(
            x => x.Publish(It.IsAny<EnemyDefeated>(), It.IsAny<CancellationToken>()), 
            Times.Once);
        _mediatorMock.Verify(
            x => x.Publish(It.IsAny<GoldGained>(), It.IsAny<CancellationToken>()), 
            Times.Once);
    }
}

