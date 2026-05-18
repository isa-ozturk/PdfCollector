using System;

namespace PdfCollector.Core.Models;

public enum LogLevel
{
    Info,
    Success,
    Warning,
    Error
}

public class LogEntry
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public LogLevel Level { get; set; }
    public string Message { get; set; }

    public string TimeDisplay => Timestamp.ToString("HH:mm:ss");
}