namespace PdfCollector.Application.DTOs;

public class CollectionOptions
{
    public string SourceDirectory { get; set; }
    public bool DeleteAfterZip { get; set; }
    public bool SaveLog { get; set; }
}