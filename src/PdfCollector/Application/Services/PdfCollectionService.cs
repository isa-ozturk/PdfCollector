using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PdfCollector.Application.DTOs;
using PdfCollector.Core.Interfaces;
using PdfCollector.Core.Models;

namespace PdfCollector.Application.Services;

public class PdfCollectionService
{
    private readonly IFolderCleanupService _cleanup;
    private readonly ILogService _log;
    private readonly IPdfScannerService _scanner;
    private readonly IZipService _zipper;

    public PdfCollectionService(
        IPdfScannerService scanner,
        IZipService zipper,
        IFolderCleanupService cleanup,
        ILogService log)
    {
        _scanner = scanner;
        _zipper = zipper;
        _cleanup = cleanup;
        _log = log;
    }

    public async Task<CollectionResult> RunAsync(
        CollectionOptions options,
        IProgress<ZipProgress> progress,
        CancellationToken ct)
    {
        var result = new CollectionResult();
        _log.Clear();

        try
        {
            _log.Log(LogLevel.Info, "Taranıyor: " + options.SourceDirectory);
            var files = _scanner.Scan(options.SourceDirectory);
            result.TotalFound = files.Count;

            if (files.Count == 0)
            {
                _log.Log(LogLevel.Warning, "Hiç PDF dosyası bulunamadı.");
                result.Success = false;
                return result;
            }

            _log.Log(LogLevel.Info, files.Count + " adet PDF dosyası bulundu.");
            _log.Log(LogLevel.Info, new string('─', 44));

            ct.ThrowIfCancellationRequested();

            // Progress wrapper: her dosyayı log'a da yaz
            var loggingProgress = new Progress<ZipProgress>(p =>
            {
                _log.Log(LogLevel.Success, string.Format("[{0,3}/{1}]  {2}",
                    p.Done, p.Total, p.Current));
                progress?.Report(p);
            });

            var zipPath = await _zipper.CreateZipAsync(
                files, options.SourceDirectory, loggingProgress, ct);

            result.ZipPath = zipPath;
            result.TotalZipped = files.Count;
            result.ZipSizeBytes = new FileInfo(zipPath).Length;

            _log.Log(LogLevel.Info, new string('─', 44));
            _log.Log(LogLevel.Success, "ZIP oluşturuldu: " + Path.GetFileName(zipPath) + " (" + result.ZipSizeDisplay + ")");

            if (options.DeleteAfterZip)
            {
                _log.Log(LogLevel.Info, "Kaynak klasörler siliniyor...");
                var deleted = _cleanup.DeletePdfOnlyFolders(files, options.SourceDirectory);
                result.TotalDeleted = deleted;
                _log.Log(LogLevel.Info, deleted + " klasör silindi.");
            }

            if (options.SaveLog)
            {
                var logPath = Path.Combine(options.SourceDirectory, "PdfCollector_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");
                _log.SaveToFile(logPath);
                _log.Log(LogLevel.Info, "Log kaydedildi: " + Path.GetFileName(logPath));
            }

            result.Success = true;
            result.CompletedAt = DateTime.Now;
            _log.Log(LogLevel.Success, "İşlem tamamlandı.");
        }
        catch (OperationCanceledException)
        {
            _log.Log(LogLevel.Warning, "İşlem iptal edildi.");
            result.Success = false;
        }
        catch (Exception ex)
        {
            _log.Log(LogLevel.Error, "Hata: " + ex.Message);
            result.Errors.Add(ex.Message);
            result.Success = false;
        }

        return result;
    }
}