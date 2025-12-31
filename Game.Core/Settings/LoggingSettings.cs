namespace Game.Core.Settings;

/// <summary>
/// Logging configuration settings
/// </summary>
public class LoggingSettings
{
    public string LogLevel { get; set; } = "Information";
    public bool LogToFile { get; set; } = true;
    public bool LogToConsole { get; set; } = true;
    public string LogPath { get; set; } = "logs/";
    public int RetainDays { get; set; } = 7;
    public bool EnableStructuredLogging { get; set; } = true;
}