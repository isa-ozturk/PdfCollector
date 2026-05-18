using System.Collections.Generic;
using PdfCollector.Core.Models;

namespace PdfCollector.Presentation.ViewModels;

public class UpdateViewModel : ViewModelBase
{
    private string _currentVersion;
    private string _newVersion;
    private string _releaseNotes;
    private int    _assetId;
    private bool   _isDownloading;
    private int    _downloadProgress;
    private string _downloadStatus = string.Empty;
    private List<GithubReleaseInfo> _releases;

    public string CurrentVersion
    {
        get => _currentVersion;
        set => SetField(ref _currentVersion, value);
    }

    public string NewVersion
    {
        get => _newVersion;
        set => SetField(ref _newVersion, value);
    }

    public string ReleaseNotes
    {
        get => _releaseNotes;
        set => SetField(ref _releaseNotes, value);
    }

    public int AssetId
    {
        get => _assetId;
        set => SetField(ref _assetId, value);
    }

    public bool IsDownloading
    {
        get => _isDownloading;
        set
        {
            if (SetField(ref _isDownloading, value))
                OnPropertyChanged(nameof(IsIdle));
        }
    }

    public bool IsIdle => !_isDownloading;

    public int DownloadProgress
    {
        get => _downloadProgress;
        set => SetField(ref _downloadProgress, value);
    }

    public string DownloadStatus
    {
        get => _downloadStatus;
        set => SetField(ref _downloadStatus, value);
    }

    public List<GithubReleaseInfo> Releases
    {
        get => _releases;
        set => SetField(ref _releases, value);
    }

    public static UpdateViewModel FromUpdateInfo(UpdateInfo info) => new()
    {
        CurrentVersion = info.CurrentVersion,
        NewVersion     = info.NewVersion,
        ReleaseNotes   = info.ReleaseNotes,
        AssetId        = info.GitHubAssetId,
        Releases       = info.IntermediateReleases
    };
}
