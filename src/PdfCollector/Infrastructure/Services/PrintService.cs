using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PdfCollector.Core.Interfaces;

namespace PdfCollector.Infrastructure.Services;

public class PrintService : IPrintService
{
    private static string SumatraPdfPath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "SumatraPDF.exe");

    public Task<List<string>> GetAvailablePrintersAsync() => Task.Run(() =>
    {
        var list = new List<string>();
        foreach (string name in PrinterSettings.InstalledPrinters)
            list.Add(name);
        return list;
    });

    public Task<int> GetPdfCountInZipAsync(string zipPath) => Task.Run(() =>
    {
        if (!File.Exists(zipPath)) return 0;
        using var archive = ZipFile.OpenRead(zipPath);
        return archive.Entries.Count(e =>
            e.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));
    });

    public async Task PrintPdfsFromZipAsync(
        string zipPath,
        string printerName,
        IProgress<PrintProgress> progress,
        CancellationToken ct)
    {
        var tempDir = Path.Combine(
            Path.GetTempPath(),
            "PdfCollector_Print_" + Guid.NewGuid().ToString("N").Substring(0, 8));
        Directory.CreateDirectory(tempDir);

        try
        {
            var pdfPaths = await Task.Run(() =>
            {
                using var archive = ZipFile.OpenRead(zipPath);
                var entries = archive.Entries
                    .Where(e => e.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var paths = new List<string>();
                foreach (var entry in entries)
                {
                    ct.ThrowIfCancellationRequested();
                    var dest = Path.Combine(tempDir, entry.Name);
                    entry.ExtractToFile(dest, overwrite: true);
                    paths.Add(dest);
                }
                return paths;
            }, ct);

            if (pdfPaths.Count == 0)
            {
                progress?.Report(new PrintProgress { Current = 0, Total = 0, IsDone = true });
                return;
            }

            var useSumatra = File.Exists(SumatraPdfPath);

            for (int i = 0; i < pdfPaths.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var pdf  = pdfPaths[i];
                var name = Path.GetFileName(pdf);
                progress?.Report(new PrintProgress { Current = i + 1, Total = pdfPaths.Count, CurrentFile = name });

                if (useSumatra)
                    await PrintWithSumatraAsync(pdf, printerName);
                else
                    await PrintWithShellVerbAsync(pdf, printerName);

                await Task.Delay(useSumatra ? 500 : 1200, ct);
            }

            progress?.Report(new PrintProgress { Current = pdfPaths.Count, Total = pdfPaths.Count, IsDone = true });
        }
        finally
        {
            _ = Task.Delay(8000).ContinueWith(_ =>
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            });
        }
    }

    // ── SumatraPDF ile yazdır — güvenilir, PDF viewer gerektirmez ──────────
    private static Task PrintWithSumatraAsync(string pdfPath, string printerName)
    {
        return Task.Run(() =>
        {
            var args = string.IsNullOrWhiteSpace(printerName)
                ? $"-print-to-default \"{pdfPath}\""
                : $"-print-to \"{printerName}\" \"{pdfPath}\"";

            var psi = new ProcessStartInfo
            {
                FileName        = SumatraPdfPath,
                Arguments       = args,
                UseShellExecute = false,
                WindowStyle     = ProcessWindowStyle.Hidden,
                CreateNoWindow  = true
            };

            using var proc = Process.Start(psi);
            proc?.WaitForExit(15_000);
        });
    }

    // ── Sistem PDF viewer ile yazdır — fallback ────────────────────────────
    private static Task PrintWithShellVerbAsync(string pdfPath, string printerName)
    {
        return Task.Run(() =>
        {
            var psi = new ProcessStartInfo
            {
                FileName        = pdfPath,
                UseShellExecute = true,
                WindowStyle     = ProcessWindowStyle.Hidden
            };

            if (string.IsNullOrWhiteSpace(printerName))
            {
                psi.Verb = "print";
            }
            else
            {
                psi.Verb      = "printto";
                psi.Arguments = $"\"{printerName}\"";
            }

            using var proc = Process.Start(psi);
        });
    }
}
