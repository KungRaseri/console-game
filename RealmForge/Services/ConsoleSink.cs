using Serilog.Core;
using Serilog.Events;

namespace RealmForge.Services;

/// <summary>
/// Custom Serilog sink that writes to the global console
/// </summary>
public class ConsoleSink : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage();
        var level = logEvent.Level.ToString().ToUpper();
        var formattedMessage = $"[{level}] {message}";

        if (logEvent.Exception != null)
        {
            formattedMessage += $"\n{logEvent.Exception}";
        }

        MainWindow.AddLog(formattedMessage);
    }
}
