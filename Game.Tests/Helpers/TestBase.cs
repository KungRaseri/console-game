using Game.Console.UI;
using Game.Core.Abstractions;
using Spectre.Console.Testing;

namespace Game.Tests.Helpers;

/// <summary>
/// Base class for tests that need TestConsole/ConsoleUI infrastructure.
/// Provides shared test console setup.
/// </summary>
public abstract class TestBase
{
    protected TestConsole TestConsole { get; }
    protected IGameUI ConsoleUI { get; }

    protected TestBase()
    {
        TestConsole = TestConsoleHelper.CreateInteractiveConsole();
        ConsoleUI = (IGameUI)new ConsoleUI(TestConsole);
    }
}
