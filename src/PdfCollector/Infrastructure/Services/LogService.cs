using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PdfCollector.Core.Interfaces;
using PdfCollector.Core.Models;

namespace PdfCollector.Infrastructure.Services;

public class LogService : ILogService
{
    private readonly List<LogEntry> _entries = new();
    private readonly object _lock = new object();

    public IReadOnlyList<LogEntry> Entries
    {
        get { lock (_lock) return _entries.ToArray(); }
    }

    public void Log(LogLevel level, string message)
    {
        lock (_lock)
            _entries.Add(new LogEntry { Level = level, Message = message });
    }

    public void Clear()
    {
        lock (_lock)
            _entries.Clear();
    }

    public void SaveToFile(string path)
    {
        IReadOnlyList<LogEntry> snapshot;
        lock (_lock) snapshot = _entries.ToArray();

        var sb = new StringBuilder();
        sb.AppendLine("PDF Toplayici - Log Raporu");
        sb.AppendLine("Tarih: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
        sb.AppendLine(new string('=', 60));
        sb.AppendLine();
        foreach (var e in snapshot)
            sb.AppendLine(string.Format("[{0}] [{1,-7}] {2}",
                e.TimeDisplay, e.Level.ToString().ToUpper(), e.Message));
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
    }
}
