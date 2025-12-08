using Xunit;
using FluentAssertions;
using Game.Services;
using Game.Features.Combat;
using Game.Features.SaveLoad;
using Game.Shared.Services;
using Game.Models;
using MediatR;
using Moq;
using System.Reflection;

namespace Game.Tests.Services;

/// <summary>
/// Tests for CombatOrchestrator service.
/// Note: Most combat orchestration methods are UI-dependent and async.
/// These tests focus on testable utility methods and service instantiation.
/// </summary>
public class CombatOrchestratorTests : IDisposable
{
    private readonly string _testDbFile;
    private readonly Mock<IMediator> _mockMediator;
    private readonly CombatService _combatService;
    private readonly SaveGameService _saveGameService;
    private readonly MenuService _menuService;
    private readonly GameStateService _gameStateService;
    private readonly CombatOrchestrator _combatOrchestrator;

    public CombatOrchestratorTests()
    {
        // Use unique database file for this test class
        _testDbFile = $"test-combatorchestrator-{Guid.NewGuid()}.db";

        // Create mock mediator
        _mockMediator = new Mock<IMediator>();

        // Create real dependencies with correct constructors
        _saveGameService = new SaveGameService(_testDbFile);
        _combatService = new CombatService(_saveGameService);
        _gameStateService = new GameStateService(_saveGameService);
        _menuService = new MenuService(_gameStateService, _saveGameService);

        // Create CombatOrchestrator
        _combatOrchestrator = new CombatOrchestrator(
            _mockMediator.Object,
            _combatService,
            _saveGameService,
            _menuService
        );
    }

    public void Dispose()
    {
        // Dispose services to release database connections
        _saveGameService?.Dispose();
        
        // Small delay to ensure file handles are released
        System.Threading.Thread.Sleep(100);
        
        // Clean up test database
        try
        {
            if (File.Exists(_testDbFile))
            {
                File.Delete(_testDbFile);
            }
            var logFile = _testDbFile.Replace(".db", "-log.db");
            if (File.Exists(logFile))
            {
                File.Delete(logFile);
            }
        }
        catch (IOException)
        {
            // Ignore cleanup errors - files might still be locked
        }
    }

    #region Service Instantiation Tests

    [Fact]
    public void CombatOrchestrator_Should_Be_Created_Successfully()
    {
        // Assert
        _combatOrchestrator.Should().NotBeNull();
    }

    #endregion

    #region ParseCombatAction Tests (Private Static Method)

    [Theory]
    [InlineData("⚔️ Attack", CombatActionType.Attack)]
    [InlineData("🛡️ Defend", CombatActionType.Defend)]
    [InlineData("✨ Use Item", CombatActionType.UseItem)]
    [InlineData("💨 Flee", CombatActionType.Flee)]
    [InlineData("Unknown Action", CombatActionType.Attack)] // Default case
    [InlineData("", CombatActionType.Attack)] // Empty string default
    public void ParseCombatAction_Should_Return_Correct_Action_Type(string choice, CombatActionType expected)
    {
        // Arrange - Use reflection to access private static method
        var method = typeof(CombatOrchestrator).GetMethod(
            "ParseCombatAction",
            BindingFlags.NonPublic | BindingFlags.Static
        );
        method.Should().NotBeNull("ParseCombatAction method should exist");

        // Act
        var result = (CombatActionType)method!.Invoke(null, new object[] { choice })!;

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region GenerateHealthBar Tests (Private Static Method)

    [Fact]
    public void GenerateHealthBar_Should_Generate_Full_Bar_When_At_Max_Health()
    {
        // Arrange
        var method = typeof(CombatOrchestrator).GetMethod(
            "GenerateHealthBar",
            BindingFlags.NonPublic | BindingFlags.Static
        );
        method.Should().NotBeNull("GenerateHealthBar method should exist");

        // Act
        var result = (string)method!.Invoke(null, new object[] { 100, 100, 10 })!;

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("[green]"); // Full health is green
        result.Should().Contain("██████████"); // 10 filled characters
    }

    [Fact]
    public void GenerateHealthBar_Should_Generate_Half_Bar_When_At_Half_Health()
    {
        // Arrange
        var method = typeof(CombatOrchestrator).GetMethod(
            "GenerateHealthBar",
            BindingFlags.NonPublic | BindingFlags.Static
        );

        // Act
        var result = (string)method!.Invoke(null, new object[] { 50, 100, 10 })!;

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("█████"); // 5 filled characters (50%)
        result.Should().Contain("░░░░░"); // 5 empty characters
    }

    [Fact]
    public void GenerateHealthBar_Should_Use_Yellow_Color_When_Health_Below_Half()
    {
        // Arrange
        var method = typeof(CombatOrchestrator).GetMethod(
            "GenerateHealthBar",
            BindingFlags.NonPublic | BindingFlags.Static
        );

        // Act - 40% health (between 25% and 50%)
        var result = (string)method!.Invoke(null, new object[] { 40, 100, 10 })!;

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("[yellow]"); // Mid-range health is yellow
    }

    [Fact]
    public void GenerateHealthBar_Should_Use_Red_Color_When_Health_Below_Quarter()
    {
        // Arrange
        var method = typeof(CombatOrchestrator).GetMethod(
            "GenerateHealthBar",
            BindingFlags.NonPublic | BindingFlags.Static
        );

        // Act - 20% health (below 25%)
        var result = (string)method!.Invoke(null, new object[] { 20, 100, 10 })!;

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("[red]"); // Low health is red
    }

    [Fact]
    public void GenerateHealthBar_Should_Handle_Zero_Health()
    {
        // Arrange
        var method = typeof(CombatOrchestrator).GetMethod(
            "GenerateHealthBar",
            BindingFlags.NonPublic | BindingFlags.Static
        );

        // Act
        var result = (string)method!.Invoke(null, new object[] { 0, 100, 10 })!;

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("[red]"); // Zero health is red
        result.Should().Contain("░░░░░░░░░░"); // All empty characters
    }

    [Fact]
    public void GenerateHealthBar_Should_Handle_Different_Bar_Widths()
    {
        // Arrange
        var method = typeof(CombatOrchestrator).GetMethod(
            "GenerateHealthBar",
            BindingFlags.NonPublic | BindingFlags.Static
        );

        // Act - 50% health with width of 20
        var result = (string)method!.Invoke(null, new object[] { 50, 100, 20 })!;

        // Assert
        result.Should().NotBeNullOrEmpty();
        // Should have characters for both filled and empty portions
        result.Should().Contain("█");
        result.Should().Contain("░");
    }

    #endregion

    #region Integration Note

    // Note: The following methods are NOT tested due to heavy UI dependencies:
    // - HandleCombatAsync: Main combat loop with UI interaction and async delays
    // - DisplayCombatStatusWithLog: Pure UI rendering method
    // - ExecutePlayerTurnAsync: UI-heavy with ConsoleUI calls and mediator events
    // - ExecuteEnemyTurnAsync: UI-heavy with ConsoleUI calls and mediator events
    // - UseItemInCombatMenuAsync: Requires menu interaction
    // - HandleCombatVictoryAsync: UI-heavy with level-up flow and auto-save
    // - HandleCombatDefeatAsync: UI-heavy with ConsoleUI calls
    //
    // These methods would benefit from integration tests or further refactoring
    // to separate business logic from UI concerns.

    #endregion
}
