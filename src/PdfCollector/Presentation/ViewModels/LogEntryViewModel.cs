using PdfCollector.Core.Models;

namespace PdfCollector.Presentation.ViewModels;

public class LogEntryViewModel : ViewModelBase
{
    public LogEntryViewModel(LogEntry entry)
    {
        TimeDisplay = entry.TimeDisplay;
        Message = entry.Message;
        Level = entry.Level;
    }

    public string TimeDisplay { get; private set; }
    public string Message { get; private set; }
    public LogLevel Level { get; private set; }
}