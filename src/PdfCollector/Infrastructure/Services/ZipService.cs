using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using PdfCollector.Core.Interfaces;
using PdfCollector.Core.Models;

namespace PdfCollector.Infrastructure.Services;

public class ZipService : IZipService
{
    public Task<string> CreateZipAsync(
        IReadOnlyList<PdfFileInfo> files,
        string outputDirectory,
        IProgress<ZipProgress> progress,
        CancellationToken ct)
    {
        return Task.Run(() =>
        {
            var zipName = "PDF_Arsiv_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".zip";
            var zipPath = Path.Combine(outputDirectory, zipName);

            using var zipStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write);
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, false);
            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var done = 0;

            foreach (var pdf in files)
            {
                ct.ThrowIfCancellationRequested();

                var entryName = pdf.FileName;
                if (usedNames.Contains(entryName))
                {
                    var parent = Path.GetFileName(
                        Path.GetDirectoryName(pdf.FullPath));
                    entryName = parent + "_" + pdf.FileName;
                }

                usedNames.Add(entryName);

                var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);

                using (var entryStream = entry.Open())
                using (var fileStream = new FileStream(pdf.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920))
                {
                    fileStream.CopyTo(entryStream, 81920);
                }

                done++;
                progress?.Report(new ZipProgress
                {
                    Done = done,
                    Total = files.Count,
                    Current = entryName
                });
            }

            return zipPath;
        }, ct);
    }
}