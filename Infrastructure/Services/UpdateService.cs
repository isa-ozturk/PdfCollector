using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows; // System.Windows.Application
using PdfCollector.Core.Interfaces;
using PdfCollector.Core.Models;

namespace PdfCollector.Infrastructure.Services;

public class UpdateService : IUpdateService
{
    private const string GitHubApiUrl      = "https://api.github.com/repos/isa-ozturk/PdfCollector/releases";
    private const string AppName           = "PdfCollector";
    private const string UpdateFileName    = "PdfCollector_Update.exe";
    private const string UpdatePsScript    = "PdfCollector_Update.ps1";

    public event EventHandler<UpdateInfo> UpdateAvailable;
    public UpdateInfo LastUpdateInfo { get; private set; }

    public void NotifyCachedUpdateAvailable()
    {
        if (LastUpdateInfo?.IsUpdateAvailable == true)
            UpdateAvailable?.Invoke(this, LastUpdateInfo);
    }

    public async Task<UpdateInfo> CheckForUpdatesAsync()
    {
        try
        {
            if (!await IsInternetAvailableAsync())
                return new UpdateInfo { IsUpdateAvailable = false };

            var allReleases = await GetAllReleasesAsync();

            var localVersion = GetLocalVersion();

            var stableReleases = allReleases
                .Where(r => !r.Prerelease)
                .OrderByDescending(r => ParseVersion(r.TagName))
                .ToList();

            if (stableReleases.Count == 0)
                return new UpdateInfo { IsUpdateAvailable = false };

            var latestRelease = stableReleases[0];
            var serverVersion = ParseVersion(latestRelease.TagName);

            if (serverVersion <= localVersion)
                return new UpdateInfo { IsUpdateAvailable = false };

            var relevantReleases = stableReleases
                .Where(r => ParseVersion(r.TagName) > localVersion)
                .OrderByDescending(r => ParseVersion(r.TagName))
                .ToList();

            var updateInfo = new UpdateInfo
            {
                CurrentVersion       = localVersion.ToString(3),
                NewVersion           = latestRelease.TagName.TrimStart('v'),
                ReleaseNotes         = CombineReleaseNotes(relevantReleases),
                GitHubAssetId        = latestRelease.Assets?.Length > 0 ? latestRelease.Assets[0].Id : 0,
                IsUpdateAvailable    = true,
                IntermediateReleases = relevantReleases
            };

            LastUpdateInfo = updateInfo;
            UpdateAvailable?.Invoke(this, updateInfo);
            return updateInfo;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UpdateService] Kontrol hatası: {ex.Message}");
            return new UpdateInfo { IsUpdateAvailable = false };
        }
    }

    public async Task DownloadUpdateAsync(int assetId)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), UpdateFileName);

        try
        {
            var apiUrl  = $"{GitHubApiUrl}/assets/{assetId}";
            var request = (HttpWebRequest)WebRequest.Create(apiUrl);
            request.UserAgent = $"{AppName}-Updater";
            request.Accept    = "application/octet-stream";

            using var response   = await request.GetResponseAsync();
            using var stream     = response.GetResponseStream();
            using var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None);

            var buffer    = new byte[8192];
            int bytesRead;
            long totalRead    = 0;
            var  totalLength  = response.ContentLength;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                totalRead += bytesRead;
            }

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var result = System.Windows.MessageBox.Show(
                    "Güncelleme indirildi. Uygulama yeniden başlatılacak. Devam edilsin mi?",
                    "Güncelleme Hazır",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                    StartUpdateScript(tempPath);
            });
        }
        catch (Exception ex)
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                System.Windows.MessageBox.Show(
                    $"Güncelleme indirilemedi:\n{ex.Message}",
                    "Güncelleme Hatası",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error));
        }
    }

    public void Dispose() { }

    // ─── private helpers ────────────────────────────────────────────────────

    private static Version GetLocalVersion()
    {
        var v = Assembly.GetExecutingAssembly().GetName().Version;
        return new Version(v.Major, v.Minor, v.Build);
    }

    private static void StartUpdateScript(string tempPath)
    {
        var exePath      = Process.GetCurrentProcess().MainModule?.FileName;
        var psScriptPath = Path.Combine(Path.GetTempPath(), UpdatePsScript);

        var script = $@"
Start-Sleep -Seconds 1
$src = [System.Text.Encoding]::UTF8.GetString([System.Text.Encoding]::Default.GetBytes(""{tempPath}""))
$dst = [System.Text.Encoding]::UTF8.GetString([System.Text.Encoding]::Default.GetBytes(""{exePath}""))
Copy-Item -LiteralPath $src -Destination $dst -Force
Start-Process -FilePath $dst
Remove-Item -Path $src -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2
Remove-Item -Path $PSCommandPath -Force -ErrorAction SilentlyContinue
";
        File.WriteAllText(psScriptPath, script, Encoding.UTF8);

        Process.Start(new ProcessStartInfo
        {
            FileName        = "powershell.exe",
            Arguments       = $"-ExecutionPolicy Bypass -WindowStyle Hidden -File \"{psScriptPath}\"",
            UseShellExecute = true,
            WindowStyle     = ProcessWindowStyle.Hidden
        });

        System.Windows.Application.Current.Shutdown();
    }

    private async Task<List<GithubReleaseInfo>> GetAllReleasesAsync()
    {
        try
        {
            var request   = (HttpWebRequest)WebRequest.Create(GitHubApiUrl);
            request.UserAgent = $"{AppName}-Updater";

            using var response = await request.GetResponseAsync();
            using var stream   = response.GetResponseStream();
            if (stream == null) return new List<GithubReleaseInfo>();

            using var reader  = new StreamReader(stream, Encoding.UTF8, true, 4096, true);
            var       content = await reader.ReadToEndAsync();

            var ms         = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var serializer = new DataContractJsonSerializer(typeof(GithubReleaseInfo[]));
            var releases   = (GithubReleaseInfo[])serializer.ReadObject(ms);
            return releases?.ToList() ?? new List<GithubReleaseInfo>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UpdateService] Sürümler alınamadı: {ex.Message}");
            return new List<GithubReleaseInfo>();
        }
    }

    private static Version ParseVersion(string tagName)
    {
        if (string.IsNullOrEmpty(tagName)) return new Version(0, 0, 0);
        var s = tagName.TrimStart('v');
        return Version.TryParse(s, out var v) ? v : new Version(0, 0, 0);
    }

    private static string CombineReleaseNotes(List<GithubReleaseInfo> releases)
    {
        var sb    = new StringBuilder();
        var first = true;

        foreach (var r in releases)
        {
            if (!first) { sb.AppendLine(); sb.AppendLine("---"); sb.AppendLine(); }
            first = false;

            var label = r.Name ?? r.TagName;
            var date  = string.Empty;
            if (!string.IsNullOrEmpty(r.CreatedAt))
                try { date = " — " + DateTime.Parse(r.CreatedAt).ToString("dd MMMM yyyy"); }
                catch { }

            sb.AppendLine($"## {label}{date}");
            sb.AppendLine();
            sb.AppendLine(string.IsNullOrWhiteSpace(r.Body) ? "Sürüm notları yok." : r.Body.Trim());
        }

        return sb.ToString().Trim();
    }

    private static async Task<bool> IsInternetAvailableAsync()
    {
        try
        {
            var req = (HttpWebRequest)WebRequest.Create("https://www.google.com");
            req.Method = "HEAD";
            req.Timeout = 4000;
            using var _ = await req.GetResponseAsync();
            return true;
        }
        catch { return false; }
    }
}
