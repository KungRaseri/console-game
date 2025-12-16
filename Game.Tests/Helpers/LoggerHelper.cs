using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Game.Tests.Helpers;

/// <summary>
/// Helper for creating test loggers.
/// </summary>
public static class LoggerHelper
{
    /// <summary>
    /// Creates a null logger for testing (logs nothing).
    /// </summary>
    public static ILogger<T> CreateNullLogger<T>()
    {
        return NullLogger<T>.Instance;
    }

    /// <summary>
    /// Creates a logger factory that produces null loggers.
    /// </summary>
    public static ILoggerFactory CreateNullLoggerFactory()
    {
        return NullLoggerFactory.Instance;
    }
}
