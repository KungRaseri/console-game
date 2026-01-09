namespace RealmEngine.Core.Settings;

/// <summary>
/// Logging configuration settings
/// </summary>
public class LoggingSettings
{
    /// <summary>Gets or sets the minimum log level (Verbose, Debug, Information, Warning, Error, Fatal).</summary>
    public string LogLevel { get; set; } = "Information";
    /// <summary>Gets or sets a value indicating whether logs are written to file.</summary>
    public bool LogToFile { get; set; } = true;
    /// <summary>Gets or sets a value indicating whether logs are written to console.</summary>
    public bool LogToConsole { get; set; } = true;
    /// <summary>Gets or sets the path where log files are stored.</summary>
    public string LogPath { get; set; } = "logs/";
    /// <summary>Gets or sets the number of days to retain log files before deletion.</summary>
    public int RetainDays { get; set; } = 7;
    /// <summary>Gets or sets a value indicating whether structured logging is enabled.</summary>
    public bool EnableStructuredLogging { get; set; } = true;
}