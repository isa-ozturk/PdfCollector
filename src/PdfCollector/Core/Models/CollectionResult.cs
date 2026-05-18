using System;
using System.Collections.Generic;

namespace PdfCollector.Core.Models;

public class CollectionResult
{
    public bool Success { get; set; }
    public string ZipPath { get; set; }
    public long ZipSizeBytes { get; set; }
    public int TotalFound { get; set; }
    public int TotalZipped { get; set; }
    public int TotalDeleted { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime CompletedAt { get; set; } = DateTime.Now;

    public string ZipSizeDisplay => ZipSizeBytes >= 1048576 ? $"{ZipSizeBytes / 1048576.0:F2} MB" : $"{ZipSizeBytes / 1024.0:F0} KB";
}