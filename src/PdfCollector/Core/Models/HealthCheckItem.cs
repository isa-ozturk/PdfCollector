namespace PdfCollector.Core.Models;

public enum HealthStatus { Ok, Warning, Error }

public class HealthCheckItem
{
    public string       Name        { get; set; }
    public string       Description { get; set; }
    public string       Detail      { get; set; }
    public HealthStatus Status      { get; set; }
    public bool         CanDownload { get; set; }
}
