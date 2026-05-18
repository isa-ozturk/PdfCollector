using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PdfCollector.Core.Interfaces;
using PdfCollector.Core.Models;

namespace PdfCollector.Infrastructure.Services;

public class PdfScannerService : IPdfScannerService
{
    public IReadOnlyList<PdfFileInfo> Scan(string rootPath)
    {
        return Directory
            .EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)
            .Where(f => Path.GetExtension(f)
                .Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            .Select(f =>
            {
                var info = new FileInfo(f);
                return new PdfFileInfo
                {
                    FullPath = f,
                    FileName = info.Name,
                    RelativePath = f.Replace(rootPath, "")
                        .TrimStart(Path.DirectorySeparatorChar),
                    SizeBytes = info.Length
                };
            })
            .ToList();
    }
}