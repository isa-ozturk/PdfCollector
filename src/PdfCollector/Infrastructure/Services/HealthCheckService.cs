using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using PdfCollector.Core.Interfaces;
using PdfCollector.Core.Models;

namespace PdfCollector.Infrastructure.Services;

public class HealthCheckService : IHealthCheckService
{
    private const string DownloadUrl =
        "https://github.com/isa-ozturk/PdfCollector/releases/download/tools-v1.0/SumatraPDF.exe";

    public string SumatraPdfPath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "SumatraPDF.exe");

    public bool IsSumatraPdfAvailable => File.Exists(SumatraPdfPath);

    public Task<List<HealthCheckItem>> RunChecksAsync() => Task.Run(() =>
    {
        var items = new List<HealthCheckItem>();

        // ── 1. Yazıcı ──────────────────────────────────────────
        int printerCount = 0;
        foreach (string _ in PrinterSettings.InstalledPrinters) printerCount++;

        items.Add(new HealthCheckItem
        {
            Name        = "Yazıcı",
            Description = printerCount > 0
                ? $"{printerCount} yazıcı kurulu"
                : "Hiç yazıcı bulunamadı",
            Status      = printerCount > 0 ? HealthStatus.Ok : HealthStatus.Error,
            Detail      = printerCount == 0
                ? "Yazdırma işlemi için en az bir yazıcı kurulu olmalıdır."
                : null
        });

        // ── 2. SumatraPDF ───────────────────────────────────────
        var sumatraOk = File.Exists(SumatraPdfPath);
        items.Add(new HealthCheckItem
        {
            Name        = "SumatraPDF",
            Description = sumatraOk
                ? "PDF yazdırma motoru hazır"
                : "PDF yazdırma motoru bulunamadı",
            Status      = sumatraOk ? HealthStatus.Ok : HealthStatus.Warning,
            Detail      = sumatraOk ? null
                : "SumatraPDF olmadan yazdırma, bilgisayarda kayıtlı PDF görüntüleyiciye bağımlıdır.",
            CanDownload = !sumatraOk
        });

        // ── 3. Windows PDF görüntüleyici ────────────────────────
        string pdfAssoc = null;
        try { pdfAssoc = Registry.GetValue(@"HKEY_CLASSES_ROOT\.pdf", "", null)?.ToString(); }
        catch { /* registry erişim hatası görmezden gelinir */ }

        var hasPdfViewer = !string.IsNullOrWhiteSpace(pdfAssoc);
        items.Add(new HealthCheckItem
        {
            Name        = "Windows PDF Görüntüleyici",
            Description = hasPdfViewer
                ? "PDF için kayıtlı uygulama mevcut"
                : "PDF için kayıtlı uygulama yok",
            // SumatraPDF varsa bu kontrol önemsiz
            Status      = sumatraOk || hasPdfViewer ? HealthStatus.Ok : HealthStatus.Error,
            Detail      = (!sumatraOk && !hasPdfViewer)
                ? "SumatraPDF veya sistem PDF görüntüleyicisi olmadan yazdırma yapılamaz."
                : null
        });

        return items;
    });

    public async Task DownloadSumatraPdfAsync(
        IProgress<(int percent, string status)> progress,
        CancellationToken ct)
    {
        var dir = Path.GetDirectoryName(SumatraPdfPath)!;
        Directory.CreateDirectory(dir);

        var tempPath = SumatraPdfPath + ".tmp";

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "PdfCollector");
        client.Timeout = TimeSpan.FromMinutes(5);

        progress?.Report((0, "Sunucuya bağlanılıyor..."));

        var response = await client.GetAsync(
            DownloadUrl, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var total = response.Content.Headers.ContentLength ?? -1L;

        using (var netStream = await response.Content.ReadAsStreamAsync())
        using (var fileStream = File.Create(tempPath))
        {
            var buffer     = new byte[65536];
            long downloaded = 0;
            int  read;

            while ((read = await netStream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, read, ct);
                downloaded += read;

                if (total > 0)
                {
                    var pct = (int)(downloaded * 100L / total);
                    var mb  = downloaded / 1_048_576.0;
                    var tot = total      / 1_048_576.0;
                    progress?.Report((pct, $"İndiriliyor... {mb:F1} MB / {tot:F1} MB"));
                }
            }
        }

        ct.ThrowIfCancellationRequested();

        if (File.Exists(SumatraPdfPath)) File.Delete(SumatraPdfPath);
        File.Move(tempPath, SumatraPdfPath);

        progress?.Report((100, "İndirme tamamlandı."));
    }
}
