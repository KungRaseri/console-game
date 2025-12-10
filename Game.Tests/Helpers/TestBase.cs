using Game.Shared.UI;
using Spectre.Console.Testing;

namespace Game.Tests.Helpers;

/// <summary>
/// Base class for tests that need TestConsole/ConsoleUI infrastructure.
/// Provides shared test console setup.
/// </summary>
public abstract class TestBase
{
    protected TestConsole TestConsole { get; }
    protected IConsoleUI ConsoleUI { get; }

    protected TestBase()
    {
        TestConsole = TestConsoleHelper.CreateInteractiveConsole();
        ConsoleUI = new ConsoleUI(TestConsole);
    }
}
