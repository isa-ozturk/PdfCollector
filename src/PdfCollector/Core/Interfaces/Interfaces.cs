using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PdfCollector.Core.Models;

namespace PdfCollector.Core.Interfaces;

public interface IUpdateService : IDisposable
{
    event EventHandler<UpdateInfo> UpdateAvailable;
    UpdateInfo LastUpdateInfo { get; }
    Task<UpdateInfo> CheckForUpdatesAsync();
    Task DownloadUpdateAsync(int assetId);
    void NotifyCachedUpdateAvailable();
}

public interface IPrintService
{
    Task PrintPdfsFromZipAsync(string zipPath, string printerName, IProgress<PrintProgress> progress, CancellationToken ct);
    Task<List<string>> GetAvailablePrintersAsync();
    Task<int> GetPdfCountInZipAsync(string zipPath);
}

public interface IPdfScannerService
{
    IReadOnlyList<PdfFileInfo> Scan(string rootPath);
}

public interface IZipService
{
    Task<string> CreateZipAsync(
        IReadOnlyList<PdfFileInfo> files,
        string outputDirectory,
        IProgress<ZipProgress> progress,
        CancellationToken ct);
}

public interface IFolderCleanupService
{
    int DeletePdfOnlyFolders(IReadOnlyList<PdfFileInfo> files, string rootPath);
}

public interface ILogService
{
    IReadOnlyList<LogEntry> Entries { get; }
    void Log(LogLevel level, string message);
    void Clear();
    void SaveToFile(string path);
}

public interface IHealthCheckService
{
    string SumatraPdfPath      { get; }
    bool   IsSumatraPdfAvailable { get; }
    Task<List<HealthCheckItem>> RunChecksAsync();
    Task DownloadSumatraPdfAsync(IProgress<(int percent, string status)> progress, CancellationToken ct);
}

public struct ZipProgress
{
    public int Done { get; set; }
    public int Total { get; set; }
    public string Current { get; set; }
}

public struct PrintProgress
{
    public int    Current     { get; set; }  // 1-based index of file being printed
    public int    Total       { get; set; }
    public string CurrentFile { get; set; }
    public bool   IsDone      { get; set; }
}