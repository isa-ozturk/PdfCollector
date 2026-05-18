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
    public Task<List<string>> GetAvailablePrintersAsync()
    {
        return Task.Run(() =>
        {
            var list = new List<string>();
            foreach (string name in PrinterSettings.InstalledPrinters)
                list.Add(name);
            return list;
        });
    }

    public Task<int> GetPdfCountInZipAsync(string zipPath)
    {
        return Task.Run(() =>
        {
            if (!File.Exists(zipPath)) return 0;
            using var archive = ZipFile.OpenRead(zipPath);
            return archive.Entries.Count(e =>
                e.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));
        });
    }

    public async Task PrintPdfsFromZipAsync(
        string zipPath,
        string printerName,
        IProgress<string> progress,
        CancellationToken ct)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "PdfCollector_Print_" + Guid.NewGuid().ToString("N").Substring(0, 8));
        Directory.CreateDirectory(tempDir);

        try
        {
            // ZIP'ten PDF dosyalarını çıkart
            List<string> pdfPaths = await Task.Run(() =>
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
                progress?.Report("ZIP arşivinde PDF bulunamadı.");
                return;
            }

            // Her PDF'yi yazdır
            for (int i = 0; i < pdfPaths.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var pdf  = pdfPaths[i];
                var name = Path.GetFileName(pdf);
                progress?.Report($"Yazdırılıyor ({i + 1}/{pdfPaths.Count}): {name}");

                await PrintSinglePdfAsync(pdf, printerName);

                // Yazıcı kuyruğuna teslim için bekleme
                await Task.Delay(1200, ct);
            }

            progress?.Report($"Tamamlandı  ·  {pdfPaths.Count} PDF yazıcıya gönderildi.");
        }
        finally
        {
            // Kısa bekle, sonra temizle (yazıcı kuyruğunun dosyayı okuması için)
            _ = Task.Delay(8000).ContinueWith(_ =>
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            });
        }
    }

    private static Task PrintSinglePdfAsync(string pdfPath, string printerName)
    {
        return Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WindowStyle     = ProcessWindowStyle.Hidden
                };

                if (string.IsNullOrWhiteSpace(printerName))
                {
                    psi.FileName = pdfPath;
                    psi.Verb     = "print";
                }
                else
                {
                    psi.FileName   = pdfPath;
                    psi.Verb       = "printto";
                    psi.Arguments  = $"\"{printerName}\"";
                }

                using var proc = Process.Start(psi);
                // Yazıcı işlemi başlatıldı; spooler'ın teslim alması için beklemeye gerek yok
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PrintService] {Path.GetFileName(pdfPath)} yazdırılamadı: {ex.Message}");
                throw;
            }
        });
    }
}
