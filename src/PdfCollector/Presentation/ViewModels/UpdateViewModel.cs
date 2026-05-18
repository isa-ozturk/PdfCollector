using System.Collections.Generic;
using System.Text;
using PdfCollector.Core.Models;

namespace PdfCollector.Presentation.ViewModels;

public class UpdateViewModel : ViewModelBase
{
    private string              _currentVersion;
    private string              _newVersion;
    private string              _releaseNotes;
    private int                 _assetId;
    private bool                _isDownloading;
    private string              _downloadStatus = string.Empty;
    private List<GithubReleaseInfo> _releases;
    private List<ReleaseTabItem>    _releaseTabItems;

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

    public List<ReleaseTabItem> ReleaseTabItems
    {
        get => _releaseTabItems;
        set => SetField(ref _releaseTabItems, value);
    }

    public static UpdateViewModel FromUpdateInfo(UpdateInfo info)
    {
        var vm = new UpdateViewModel
        {
            CurrentVersion = info.CurrentVersion,
            NewVersion     = info.NewVersion,
            ReleaseNotes   = info.ReleaseNotes,
            AssetId        = info.GitHubAssetId,
            Releases       = info.IntermediateReleases
        };
        vm.BuildTabItems();
        return vm;
    }

    private void BuildTabItems()
    {
        var items = new List<ReleaseTabItem>();

        // "Tüm Değişiklikler" — tüm sürümlerin notlarını birleştir
        var allSb = new StringBuilder();
        if (_releases != null)
        {
            foreach (var r in _releases)
            {
                if (!string.IsNullOrWhiteSpace(r.Body))
                    allSb.AppendLine(r.Body).AppendLine();
            }
        }
        if (allSb.Length == 0 && !string.IsNullOrWhiteSpace(_releaseNotes))
            allSb.Append(_releaseNotes);

        items.Add(new ReleaseTabItem { Header = "Tüm Değişiklikler", Content = allSb.ToString().Trim() });

        // Her sürüm için ayrı sekme
        if (_releases != null)
        {
            foreach (var r in _releases)
            {
                items.Add(new ReleaseTabItem
                {
                    Header  = r.TagName ?? r.Name ?? "?",
                    Content = r.Body ?? string.Empty
                });
            }
        }

        ReleaseTabItems = items;
    }
}
