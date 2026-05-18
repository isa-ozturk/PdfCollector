namespace PdfCollector.Core.Models;

public class PdfFileInfo
{
    public string FullPath { get; set; }
    public string FileName { get; set; }
    public string RelativePath { get; set; }
    public long SizeBytes { get; set; }
}