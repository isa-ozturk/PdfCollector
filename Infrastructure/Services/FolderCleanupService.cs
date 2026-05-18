using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PdfCollector.Core.Interfaces;
using PdfCollector.Core.Models;

namespace PdfCollector.Infrastructure.Services;

public class FolderCleanupService : IFolderCleanupService
{
    public int DeletePdfOnlyFolders(IReadOnlyList<PdfFileInfo> files, string rootPath)
    {
        var deleted = 0;

        var dirs = files
            .Select(f => Path.GetDirectoryName(f.FullPath))
            .Where(d => !string.Equals(d, rootPath, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(d => d.Length)
            .ToList();

        foreach (var dir in from dir in dirs
                 where Directory.Exists(dir)
                 let hasPdfOnly = Directory
                     .EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
                     .All(f => Path.GetExtension(f)
                         .Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                 where hasPdfOnly
                 select dir)
            try
            {
                Directory.Delete(dir, true);
                deleted++;
            }
            catch
            {
                //
            }

        // Clean empty dirs
        foreach (var dir in Directory
                     .EnumerateDirectories(rootPath, "*", SearchOption.AllDirectories)
                     .OrderByDescending(d => d.Length))
            try
            {
                if (!Directory.EnumerateFileSystemEntries(dir).Any())
                    Directory.Delete(dir);
            }
            catch
            {
                //
            }

        return deleted;
    }
}