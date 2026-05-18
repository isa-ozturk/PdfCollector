using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using PdfCollector.Application.DTOs;
using PdfCollector.Application.Services;
using PdfCollector.Core.Interfaces;
using PdfCollector.Core.Models;
using PdfCollector.Infrastructure.Services;
using PdfCollector.Presentation.Commands;
using PdfCollector.Presentation.Views;

namespace PdfCollector.Presentation.ViewModels;

public class MainViewModel : ViewModelBase
{
    // ─── services ────────────────────────────────────────────────────────────
    private readonly ILogService          _log;
    private readonly AppSettingsService   _settingsSvc;
    private readonly PdfCollectionService _svc;
    private readonly IPrintService        _printSvc;
    private readonly IUpdateService       _updateSvc;
    private readonly IHealthCheckService  _healthSvc;

    // ─── backing fields ───────────────────────────────────────────────────────
    private bool   _closeAfterDone;
    private CancellationTokenSource _cts;
    private bool   _deleteAfter;
    private int    _foundCount;
    private bool   _isBusy;
    private string _lastZip        = string.Empty;
    private int    _progressMax    = 1;
    private string _progressText   = string.Empty;
    private int    _progressVal;
    private bool   _saveLog;
    private string _sourceDir      = string.Empty;
    private string _statusMsg      = "Klasör seçin ve başlatın.";
    private string _lastResultSum  = string.Empty;

    // ─── update state ─────────────────────────────────────────────────────────
    private enum UpdateState { Idle, Checking, UpToDate, Available, Error }
    private UpdateState _updateState   = UpdateState.Idle;
    private string      _updateVersion = string.Empty;

    // ─── health state ─────────────────────────────────────────────────────────
    private enum HealthState { Idle, Checking, Healthy, Warning, Error }
    private HealthState _healthState = HealthState.Idle;

    public MainViewModel(
        PdfCollectionService  svc,
        ILogService           log,
        AppSettingsService    settingsSvc,
        IPrintService         printSvc,
        IUpdateService        updateSvc,
        IHealthCheckService   healthSvc)
    {
        _svc       = svc;
        _log       = log;
        _settingsSvc = settingsSvc;
        _printSvc  = printSvc;
        _updateSvc = updateSvc;
        _healthSvc = healthSvc;

        BrowseCommand       = new RelayCommand(Browse,          () => IsIdle);
        StartCommand        = new RelayCommand(Start,           () => IsIdle && HasSourceDir);
        CancelCommand       = new RelayCommand(Cancel,          () => IsBusy);
        OpenResultCommand   = new RelayCommand(OpenResult,      () => HasResult);
        PrintCommand        = new RelayCommand(OpenPrint,       () => HasResult && IsIdle);
        PrintFromZipCommand = new RelayCommand(OpenPrintFromZip, () => IsIdle);
        ClearLogCommand     = new RelayCommand(ClearLog,        () => LogEntries.Count > 0);

        LoadSettings();

        // Başlangıçta güncelleme kontrolü
        if (_settingsSvc.Settings.AutoCheckForUpdates)
            _ = StartupUpdateCheckAsync();

        // Başlangıçta sağlık kontrolü
        _ = StartupHealthCheckAsync();
    }

    // ─── public properties ────────────────────────────────────────────────────
    public string AppVersion => GetAppVersion();

    public string SourceDirectory
    {
        get => _sourceDir;
        set
        {
            if (!SetField(ref _sourceDir, value)) return;
            FoundCount   = 0;
            LastZipPath  = string.Empty;
            OnPropertyChanged(nameof(HasSourceDir));
            SaveSettings();
            ScanCountAsync(value);
        }
    }

    public bool HasSourceDir => !string.IsNullOrWhiteSpace(_sourceDir) && Directory.Exists(_sourceDir);

    public bool DeleteAfterZip
    {
        get => _deleteAfter;
        set { if (SetField(ref _deleteAfter, value)) SaveSettings(); }
    }

    public bool SaveLog
    {
        get => _saveLog;
        set { if (SetField(ref _saveLog, value)) SaveSettings(); }
    }

    public bool CloseAfterDone
    {
        get => _closeAfterDone;
        set { if (SetField(ref _closeAfterDone, value)) SaveSettings(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetField(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(IsIdle));
                PrintCommand.RaiseCanExecuteChanged();
                PrintFromZipCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsIdle => !_isBusy;

    public int  ProgressValue { get => _progressVal;   private set => SetField(ref _progressVal, value); }
    public int  ProgressMax   { get => _progressMax;   private set => SetField(ref _progressMax, value); }
    public string ProgressText{ get => _progressText;  private set => SetField(ref _progressText, value); }

    public string StatusMessage
    {
        get => _statusMsg;
        private set => SetField(ref _statusMsg, value);
    }

    public string LastResultSummary
    {
        get => _lastResultSum;
        private set => SetField(ref _lastResultSum, value);
    }

    public int FoundCount
    {
        get => _foundCount;
        private set => SetField(ref _foundCount, value);
    }

    public string LastZipPath
    {
        get => _lastZip;
        private set
        {
            if (SetField(ref _lastZip, value))
            {
                OnPropertyChanged(nameof(HasResult));
                PrintCommand.RaiseCanExecuteChanged();
                OpenResultCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool HasResult => !string.IsNullOrEmpty(_lastZip) && File.Exists(_lastZip);

    public ObservableCollection<LogEntryViewModel> LogEntries { get; } = new();

    // ─── commands ─────────────────────────────────────────────────────────────
    public RelayCommand BrowseCommand       { get; }
    public RelayCommand StartCommand        { get; }
    public RelayCommand CancelCommand       { get; }
    public RelayCommand OpenResultCommand   { get; }
    public RelayCommand PrintCommand        { get; }
    public RelayCommand PrintFromZipCommand { get; }
    public RelayCommand ClearLogCommand     { get; }

    // ─── health UI ────────────────────────────────────────────────────────────
    public string HealthButtonLabel => _healthState switch
    {
        HealthState.Checking => "⏳  Kontrol ediliyor...",
        HealthState.Healthy  => "✓  Sağlıklı",
        HealthState.Warning  => "⚠  Dikkat Gerektiriyor",
        HealthState.Error    => "✕  Sorun Var",
        _                    => "🔍  Sistem Durumu"
    };

    public Brush HealthButtonColor => _healthState switch
    {
        HealthState.Healthy => new SolidColorBrush(Color.FromRgb(0x10, 0x7C, 0x10)),
        HealthState.Warning => new SolidColorBrush(Color.FromRgb(0xCA, 0x50, 0x10)),
        HealthState.Error   => new SolidColorBrush(Color.FromRgb(0xC4, 0x2B, 0x1C)),
        _                   => new SolidColorBrush(Color.FromRgb(0x4B, 0x55, 0x63))
    };

    // ─── update UI ────────────────────────────────────────────────────────────
    public string UpdateButtonLabel
    {
        get => _updateState switch
        {
            UpdateState.Checking   => "⏳  Kontrol ediliyor...",
            UpdateState.UpToDate   => "✓  Güncel",
            UpdateState.Available  => $"⬆  v{_updateVersion} mevcut",
            UpdateState.Error      => "⚠  Kontrol hatası",
            _                      => "🔄  Güncellemeleri Kontrol Et"
        };
    }

    public Brush UpdateButtonColor
    {
        get => _updateState switch
        {
            UpdateState.UpToDate  => new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E)),
            UpdateState.Available => new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6)),
            UpdateState.Error     => new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)),
            _                     => new SolidColorBrush(Color.FromRgb(0x4B, 0x55, 0x63))
        };
    }

    public bool IsUpdateButtonEnabled =>
        _updateState is UpdateState.Idle or UpdateState.UpToDate
                     or UpdateState.Available or UpdateState.Error;

    // ─── settings ─────────────────────────────────────────────────────────────
    private void LoadSettings()
    {
        var s = AppSettingsService.Load();
        _deleteAfter    = s.DeleteAfterZip;
        _saveLog        = s.SaveLog;
        _closeAfterDone = s.CloseAfterDone;

        var dir = s.LastDirectory;
        if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
            dir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);

        _sourceDir = dir;
        OnPropertyChanged(nameof(SourceDirectory));
        OnPropertyChanged(nameof(HasSourceDir));
        OnPropertyChanged(nameof(DeleteAfterZip));
        OnPropertyChanged(nameof(SaveLog));
        OnPropertyChanged(nameof(CloseAfterDone));
        ScanCountAsync(dir);
    }

    private void SaveSettings()
    {
        AppSettingsService.Save(new AppSettings
        {
            DeleteAfterZip           = _deleteAfter,
            SaveLog                  = _saveLog,
            CloseAfterDone           = _closeAfterDone,
            LastDirectory            = _sourceDir,
            AutoCheckForUpdates      = _settingsSvc.Settings.AutoCheckForUpdates,
            SumatraPdfPromptDeclined = _settingsSvc.Settings.SumatraPdfPromptDeclined
        });
    }

    // ─── commands impl ────────────────────────────────────────────────────────
    private void Browse()
    {
        using var dlg = new FolderBrowserDialog
        {
            Description        = "PDF dosyalarının bulunduğu klasörü seçin",
            ShowNewFolderButton = true,
            SelectedPath       = HasSourceDir ? SourceDirectory : string.Empty
        };
        if (dlg.ShowDialog() == DialogResult.OK)
            SourceDirectory = dlg.SelectedPath;
    }

    private async void Start()
    {
        try
        {
            IsBusy        = true;
            ProgressValue = 0;
            ProgressMax   = 1;
            ProgressText  = string.Empty;
            LastZipPath   = string.Empty;
            LogEntries.Clear();
            StatusMessage = "İşlem başlatıldı...";

            _cts = new CancellationTokenSource();

            var opts = new CollectionOptions
            {
                SourceDirectory = SourceDirectory,
                DeleteAfterZip  = DeleteAfterZip,
                SaveLog         = SaveLog
            };

            var progress = new Progress<ZipProgress>(p =>
            {
                ProgressMax  = p.Total;
                ProgressValue= p.Done;
                ProgressText = $"{p.Done} / {p.Total}  —  {p.Current}";
                SyncLog();
            });

            var result = await _svc.RunAsync(opts, progress, _cts.Token);
            SyncLog();
            ProgressValue = ProgressMax;

            if (result.Success)
            {
                LastZipPath     = result.ZipPath;
                FoundCount      = result.TotalFound;
                StatusMessage   = $"Tamamlandı  ·  {result.TotalZipped} PDF  ·  {result.ZipSizeDisplay}";
                LastResultSummary = $"Son işlem: {result.TotalZipped} PDF → {Path.GetFileName(result.ZipPath)}";
            }
            else
            {
                StatusMessage = "İşlem tamamlanamadı. Log kayıtlarını inceleyin.";
            }

            IsBusy = false;
            _cts?.Dispose();
            _cts = null;

            if (result.Success && CloseAfterDone)
                System.Windows.Application.Current.Shutdown();
        }
        catch (OperationCanceledException)
        {
            _log.Log(LogLevel.Warning, "İşlem iptal edildi.");
            StatusMessage = "İşlem iptal edildi.";
            SyncLog();
            IsBusy = false;
        }
        catch (Exception ex)
        {
            _log.Log(LogLevel.Error, "Beklenmeyen hata: " + ex.Message);
            _log.Log(LogLevel.Error, ex.GetType().Name + (ex.InnerException != null ? " → " + ex.InnerException.Message : string.Empty));
            SyncLog();
            StatusMessage = "Hata oluştu — log kayıtlarını inceleyin.";
            IsBusy = false;
        }
    }

    private void Cancel()
    {
        _cts?.Cancel();
        StatusMessage = "İptal ediliyor...";
    }

    private void OpenResult()
    {
        if (!string.IsNullOrEmpty(_lastZip) && File.Exists(_lastZip))
            Process.Start("explorer.exe", $"/select,\"{_lastZip}\"");
    }

    private void OpenPrint()
    {
        if (string.IsNullOrEmpty(_lastZip) || !File.Exists(_lastZip)) return;
        if (!ConfirmPrintWithoutSumatra()) return;

        var win = new PrintWindow(_printSvc, _lastZip)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        win.ShowDialog();
    }

    private void OpenPrintFromZip()
    {
        using var dlg = new OpenFileDialog
        {
            Title           = "Yazdırılacak ZIP dosyasını seçin",
            Filter          = "ZIP Dosyaları (*.zip)|*.zip",
            CheckFileExists = true
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;
        if (!ConfirmPrintWithoutSumatra()) return;

        var win = new PrintWindow(_printSvc, dlg.FileName)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        win.ShowDialog();
    }

    // SumatraPDF yoksa kullanıcıya sor; false dönerse yazdırma iptal edilir
    private bool ConfirmPrintWithoutSumatra()
    {
        if (_healthSvc.IsSumatraPdfAvailable) return true;

        var win = new SumatraPdfPromptWindow
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        win.ShowDialog();

        if (win.Result == SumatraPdfPromptResult.Download)
        {
            SumatraPdfDownloadRequested?.Invoke(this, EventArgs.Empty);
            return false; // PrintWindow açılmaz, önce indir
        }

        // "Şimdi Değil" → devam et (shell verb ile); "Hayır, Sorma" → da devam et ama kaydet
        if (win.Result == SumatraPdfPromptResult.Decline)
        {
            _settingsSvc.Settings.SumatraPdfPromptDeclined = true;
            SaveSettings();
        }

        return true; // Later veya Decline → shell verb ile devam
    }

    private void ClearLog()
    {
        _log.Clear();
        LogEntries.Clear();
    }

    // Kullanıcı SumatraPDF indirmek istediğinde MainWindow tetiklenir
    public event EventHandler SumatraPdfDownloadRequested;

    // HealthCheckWindow kapandıktan sonra göstergeyi güncelle
    public async Task RefreshHealthAsync()
    {
        SetHealthState(HealthState.Checking);
        var items    = await _healthSvc.RunChecksAsync();
        var hasError = items.Exists(i => i.Status == Core.Models.HealthStatus.Error);
        var hasWarn  = items.Exists(i => i.Status == Core.Models.HealthStatus.Warning);
        SetHealthState(hasError ? HealthState.Error : hasWarn ? HealthState.Warning : HealthState.Healthy);
    }

    // ─── update ───────────────────────────────────────────────────────────────
    public async Task CheckForUpdatesAsync()
    {
        if (_updateState == UpdateState.Checking) return;

        SetUpdateState(UpdateState.Checking);
        var info = await _updateSvc.CheckForUpdatesAsync();

        if (info.IsUpdateAvailable)
        {
            _updateVersion = info.NewVersion;
            SetUpdateState(UpdateState.Available);
        }
        else
        {
            SetUpdateState(UpdateState.UpToDate);
            // 6 saniye sonra idle'a döndür
            _ = Task.Delay(6000).ContinueWith(_ => SetUpdateState(UpdateState.Idle),
                TaskScheduler.FromCurrentSynchronizationContext());
        }
    }

    private async Task StartupUpdateCheckAsync()
    {
        await Task.Delay(2000);
        await CheckForUpdatesAsync();
    }

    private async Task StartupHealthCheckAsync()
    {
        await Task.Delay(3000); // Güncelleme kontrolünden sonra başlat
        SetHealthState(HealthState.Checking);
        var items = await _healthSvc.RunChecksAsync();

        var hasError   = items.Exists(i => i.Status == Core.Models.HealthStatus.Error);
        var hasWarning = items.Exists(i => i.Status == Core.Models.HealthStatus.Warning);

        SetHealthState(hasError ? HealthState.Error : hasWarning ? HealthState.Warning : HealthState.Healthy);

        if (!_healthSvc.IsSumatraPdfAvailable && !_settingsSvc.Settings.SumatraPdfPromptDeclined)
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(ShowStartupSumatraPrompt);
        }
    }

    private void ShowStartupSumatraPrompt()
    {
        var win = new SumatraPdfPromptWindow
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        win.ShowDialog();

        if (win.Result == SumatraPdfPromptResult.Download)
        {
            SumatraPdfDownloadRequested?.Invoke(this, EventArgs.Empty);
        }
        else if (win.Result == SumatraPdfPromptResult.Decline)
        {
            _settingsSvc.Settings.SumatraPdfPromptDeclined = true;
            SaveSettings();
        }
        // Later → bir sonraki başlangıçta tekrar sorulur
    }

    private void SetHealthState(HealthState state)
    {
        _healthState = state;
        OnPropertyChanged(nameof(HealthButtonLabel));
        OnPropertyChanged(nameof(HealthButtonColor));
    }

    private void SetUpdateState(UpdateState state)
    {
        _updateState = state;
        OnPropertyChanged(nameof(UpdateButtonLabel));
        OnPropertyChanged(nameof(UpdateButtonColor));
        OnPropertyChanged(nameof(IsUpdateButtonEnabled));
    }

    // ─── helpers ──────────────────────────────────────────────────────────────
    private async void ScanCountAsync(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return;
            var captured = path;
            var count = await Task.Run(() =>
                Directory.EnumerateFiles(captured, "*.*", SearchOption.AllDirectories)
                    .Count(f => Path.GetExtension(f).Equals(".pdf", StringComparison.OrdinalIgnoreCase)));
            if (captured == _sourceDir) FoundCount = count;
        }
        catch { }
    }

    private void SyncLog()
    {
        // Entries snapshot'ı lock altında alınıyor (LogService thread-safe)
        var entries = _log.Entries;
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            for (var i = LogEntries.Count; i < entries.Count; i++)
                LogEntries.Add(new LogEntryViewModel(entries[i]));
        });
    }

    private static string GetAppVersion()
    {
        var v = Assembly.GetExecutingAssembly().GetName().Version;
        return $"v{v.Major}.{v.Minor}.{v.Build}";
    }
}
