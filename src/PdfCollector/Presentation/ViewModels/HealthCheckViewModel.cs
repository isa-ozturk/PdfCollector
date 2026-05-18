using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using PdfCollector.Core.Interfaces;
using PdfCollector.Core.Models;
using PdfCollector.Presentation.Commands;

namespace PdfCollector.Presentation.ViewModels;

// ── Tek satır görünüm modeli ───────────────────────────────────────────────
public class HealthCheckItemViewModel : ViewModelBase
{
    private string       _name;
    private string       _description;
    private string       _detail;
    private HealthStatus _status;
    private bool         _canDownload;

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    public string Detail
    {
        get => _detail;
        set { if (SetField(ref _detail, value)) OnPropertyChanged(nameof(HasDetail)); }
    }

    public bool HasDetail => !string.IsNullOrWhiteSpace(_detail);

    public bool CanDownload
    {
        get => _canDownload;
        set => SetField(ref _canDownload, value);
    }

    public HealthStatus Status
    {
        get => _status;
        set
        {
            if (SetField(ref _status, value))
            {
                OnPropertyChanged(nameof(StatusIcon));
                OnPropertyChanged(nameof(StatusColor));
                OnPropertyChanged(nameof(StatusBgColor));
            }
        }
    }

    public string StatusIcon => _status switch
    {
        HealthStatus.Ok      => "✓",
        HealthStatus.Warning => "⚠",
        HealthStatus.Error   => "✕",
        _                    => "?"
    };

    public string StatusColor => _status switch
    {
        HealthStatus.Ok      => "#107C10",
        HealthStatus.Warning => "#CA5010",
        HealthStatus.Error   => "#C42B1C",
        _                    => "#9E9E9E"
    };

    public string StatusBgColor => _status switch
    {
        HealthStatus.Ok      => "#DFF6DD",
        HealthStatus.Warning => "#FFF4CE",
        HealthStatus.Error   => "#FDE7E9",
        _                    => "#F0F0F0"
    };

    public HealthCheckItemViewModel(HealthCheckItem item)
    {
        _name        = item.Name;
        _description = item.Description;
        _detail      = item.Detail;
        _status      = item.Status;
        _canDownload = item.CanDownload;
    }

    public void Update(HealthCheckItem item)
    {
        Name        = item.Name;
        Description = item.Description;
        Detail      = item.Detail;
        Status      = item.Status;
        CanDownload = item.CanDownload;
    }
}

// ── Ana pencere görünüm modeli ─────────────────────────────────────────────
public class HealthCheckViewModel : ViewModelBase
{
    private readonly IHealthCheckService _svc;

    private bool   _isChecking;
    private bool   _isDownloading;
    private int    _downloadProgress;
    private string _downloadStatus = string.Empty;
    private bool   _allHealthy;
    private CancellationTokenSource _cts;

    public ObservableCollection<HealthCheckItemViewModel> Items { get; } = new();

    public bool IsChecking
    {
        get => _isChecking;
        private set { if (SetField(ref _isChecking, value)) RefreshCommand.RaiseCanExecuteChanged(); }
    }

    public bool IsDownloading
    {
        get => _isDownloading;
        private set
        {
            if (SetField(ref _isDownloading, value))
            {
                OnPropertyChanged(nameof(IsIdle));
                DownloadCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsIdle => !_isDownloading;

    public int DownloadProgress
    {
        get => _downloadProgress;
        private set => SetField(ref _downloadProgress, value);
    }

    public string DownloadStatus
    {
        get => _downloadStatus;
        private set => SetField(ref _downloadStatus, value);
    }

    public bool AllHealthy
    {
        get => _allHealthy;
        private set
        {
            if (SetField(ref _allHealthy, value))
                OnPropertyChanged(nameof(OverallText));
        }
    }

    public string OverallText => AllHealthy
        ? "Tüm kontroller sağlıklı"
        : "Bazı bileşenler dikkat gerektiriyor";

    public RelayCommand RefreshCommand  { get; }
    public RelayCommand DownloadCommand { get; }
    public RelayCommand CancelCommand   { get; }

    public HealthCheckViewModel(IHealthCheckService svc)
    {
        _svc = svc;

        RefreshCommand  = new RelayCommand(RunChecks,       () => !_isChecking && !_isDownloading);
        DownloadCommand = new RelayCommand(DownloadSumatra, () => IsIdle);
        CancelCommand   = new RelayCommand(CancelDownload,  () => _isDownloading);

        RunChecks();
    }

    private async void RunChecks()
    {
        IsChecking = true;
        var results = await _svc.RunChecksAsync();

        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            if (Items.Count == results.Count)
            {
                for (int i = 0; i < results.Count; i++)
                    Items[i].Update(results[i]);
            }
            else
            {
                Items.Clear();
                foreach (var r in results)
                    Items.Add(new HealthCheckItemViewModel(r));
            }

            AllHealthy = Items.All(i => i.Status == HealthStatus.Ok);
        });

        IsChecking = false;
    }

    private async void DownloadSumatra()
    {
        IsDownloading    = true;
        DownloadProgress = 0;
        DownloadStatus   = "Hazırlanıyor...";
        _cts             = new CancellationTokenSource();

        try
        {
            var progress = new Progress<(int pct, string msg)>(p =>
            {
                DownloadProgress = p.pct;
                DownloadStatus   = p.msg;
            });

            await _svc.DownloadSumatraPdfAsync(progress, _cts.Token);
            RunChecks();
        }
        catch (OperationCanceledException)
        {
            DownloadStatus = "İndirme iptal edildi.";
        }
        catch (Exception ex)
        {
            DownloadStatus = $"Hata: {ex.Message}";
        }
        finally
        {
            IsDownloading = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private void CancelDownload() => _cts?.Cancel();
}
