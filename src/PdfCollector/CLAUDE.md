# PDF Toplayıcı — CLAUDE.md

Bu dosya Claude Code'un proje hakkında bağlam edinmesi için hazırlanmıştır.

## Proje Özeti

**PDF Toplayıcı**, alt klasörlerdeki PDF dosyalarını tek bir ZIP arşivine toplayan ve arşivdeki PDF'leri
doğrudan yazıcıya gönderebilen bir WPF masaüstü uygulamasıdır.

| Alan | Değer |
|------|-------|
| Platform | Windows (.NET Framework 4.7.2 + WPF) |
| Dil | C# 11 |
| Mimari | Clean Architecture + MVVM |
| Tema | Windows 11 Fluent Design (açık, `Themes/Styles.xaml`) |
| Geliştirici | İsa Öztürk |

## Mimari Katmanlar

```
src/PdfCollector/
├── Core/
│   ├── Interfaces/
│   │   └── Interfaces.cs        # IPdfScannerService, IZipService, ILogService,
│   │                            # IFolderCleanupService, IUpdateService, IPrintService,
│   │                            # IHealthCheckService, ZipProgress, PrintProgress
│   └── Models/                  # LogEntry, PdfFileInfo, CollectionResult,
│                                # UpdateInfo, GithubReleaseInfo, HealthCheckItem, HealthStatus
├── Application/
│   ├── DTOs/CollectionOptions.cs
│   └── Services/PdfCollectionService.cs  # Orkestrasyon: tara → zip → temizle
├── Infrastructure/Services/
│   ├── PdfScannerService.cs
│   ├── ZipService.cs
│   ├── FolderCleanupService.cs
│   ├── LogService.cs
│   ├── AppSettingsService.cs    # JSON ayar kalıcılığı (DataContractJsonSerializer)
│   ├── UpdateService.cs         # GitHub Releases API güncelleme
│   ├── HealthCheckService.cs    # Yazıcı / SumatraPDF / PDF viewer kontrolü + indirme
│   └── PrintService.cs          # ZIP → SumatraPDF veya shell verb ile yazdırma
├── Presentation/
│   ├── Commands/RelayCommand.cs
│   ├── Converters/Converters.cs
│   ├── ViewModels/
│   │   ├── ViewModelBase.cs
│   │   ├── LogEntryViewModel.cs
│   │   ├── MainViewModel.cs         # Ana uygulama mantığı
│   │   ├── HealthCheckViewModel.cs  # Sistem durumu penceresi VM
│   │   └── UpdateViewModel.cs
│   └── Views/
│       ├── MainWindow.xaml(.cs)
│       ├── HealthCheckWindow.xaml(.cs)       # Sistem Durumu + SumatraPDF indirme
│       ├── SumatraPdfPromptWindow.xaml(.cs)  # İlk açılış / yazdırma öncesi uyarı
│       ├── PrintWindow.xaml(.cs)
│       └── UpdateWindow.xaml(.cs)
└── Themes/
    └── Styles.xaml              # Tüm brush ve kontrol stilleri (Windows 11 Fluent)
```

## Temel Özellikler

### ZIP Oluşturma
- `PdfScannerService.Scan()` → alt klasörleri özyinelemeli tarar
- `ZipService.CreateZipAsync()` → PDF'leri timestamp'li ZIP'e koyar
- Çakışan dosya adlarında üst klasör adı önek olarak eklenir
- `FolderCleanupService` → yalnızca PDF içeren klasörleri siler (isteğe bağlı)

### Yazdırma
- `PrintService.PrintPdfsFromZipAsync(IProgress<PrintProgress>)` → ZIP'ten geçici klasöre çıkartır
- `PrintProgress.{Current, Total, CurrentFile, IsDone}` ile dosya bazında ilerleme raporlanır
- **SumatraPDF varsa**: `SumatraPDF.exe -print-to "PrinterName" "file.pdf"` komutuyla yazdırır
- **SumatraPDF yoksa**: `Process.Start` + "printto" shell verb (fallback, güvenilir değil)
- SumatraPDF konumu: `{exe klasörü}/Tools/SumatraPDF.exe`
- Geçici dosyalar yazdırmadan 8 saniye sonra otomatik silinir

### Sistem Durumu (Health Check)
- `HealthCheckService.RunChecksAsync()` → üç bileşeni kontrol eder: yazıcı sayısı, SumatraPDF, Windows PDF viewer (`HKEY_CLASSES_ROOT\.pdf`)
- `HealthCheckService.DownloadSumatraPdfAsync(IProgress<(int,string)>)` → GitHub'dan indirir, `.tmp` → rename
- İndirme URL: `https://github.com/isa-ozturk/PdfCollector/releases/download/tools-v1.0/SumatraPDF.exe`
- `HealthCheckWindow` kapanınca `MainViewModel.RefreshHealthAsync()` tetiklenir → buton rengi güncellenir

### SumatraPDF Uyarı Penceresi
- `SumatraPdfPromptWindow` → üç sonuç: `SumatraPdfPromptResult.{Download, Later, Decline}`
- **İlk açılış**: `StartupHealthCheckAsync()` → SumatraPDF yoksa ve `SumatraPdfPromptDeclined == false` ise pencere açılır
- **Yazdırma öncesi**: `ConfirmPrintWithoutSumatra()` → SumatraPDF yoksa pencere açılır
- "Hayır, Sorma" → `AppSettings.SumatraPdfPromptDeclined = true` olarak kaydedilir, bir daha gösterilmez

### Güncelleme
- GitHub API: `https://api.github.com/repos/isa-ozturk/PdfCollector/releases`
- Başlangıçta otomatik kontrol (ayardan devre dışı bırakılabilir)
- PowerShell betiği ile yerinde güncelleme (uygulamayı kapatıp yenisiyle başlatır)

## Ayar Dosyası

`{exe klasörü}/PdfCollector.settings.json`

```json
{
  "DeleteAfterZip": false,
  "SaveLog": false,
  "CloseAfterDone": false,
  "LastDirectory": "C:\\...",
  "AutoCheckForUpdates": true,
  "SumatraPdfPromptDeclined": false
}
```

`AppSettingsService.Settings` property'si `_current` önbelleğini kullanır; tekrar `Load()` çağrılmamalı.  
`SaveSettings()` her çağrıda `_settingsSvc.Settings.*` üzerinden tüm alanları yazar — alan eklendikçe burası güncellenmeli.

## Tema & Stil

`Themes/Styles.xaml` dosyasında tanımlı brush'lar ve kontrol stilleri:

| Anahtar | Kullanım |
|---------|---------|
| `BgPageBrush` / `BgSurfaceBrush` | Sayfa ve kart arka planları |
| `TitleBarBrush` | Gradient mavi başlık çubuğu (LinearGradientBrush) |
| `AccentBrush` / `AccentLightBrush` | Windows mavisi ve açık tonu |
| `PrimaryButton` | Ana eylem butonu (mavi) |
| `SecondaryButton` | İkincil buton (gri kenarlık) |
| `SuccessButton` | Yeşil eylem butonu |
| `GhostButton` | Transparan arka plan butonu |
| `ToggleCheckBox` | Açma/kapama switch'i |
| `ModernProgressBar` | Yuvarlak köşeli ince progress bar |
| `LogListBox` | Konsol tarzı log listesi |
| `CardBorder` | İçerik kartı kenarlık stili |

**Pencere şablonu** (MainWindow, HealthCheckWindow, SumatraPdfPromptWindow):
- `WindowStyle="None"` + `AllowsTransparency="True"` + `Background="Transparent"`
- Dış `Border Margin="8"` → `DropShadowEffect` (shadow kırpılmaz)
- İç `Border CornerRadius="8" ClipToBounds="True"` → yuvarlak köşe + içerik kırpma

## Sürümleme

Sürüm numarası `PdfCollector.csproj` dosyasındaki `<AssemblyVersion>` ve `<FileVersion>` ile belirlenir.
GitHub Actions `release.yml` bu değeri otomatik günceller.

Sürüm formatı: `MAJOR.MINOR.PATCH` (örn. `v1.5.0`)

## Katkı & Geliştirme Kuralları

- Commit mesajları Conventional Commits formatında olmalı: `feat(...)`, `fix(...)`, `chore(...)`
- UI değişikliklerinde `Themes/Styles.xaml` stilleri korunmalı; satır içi stil kullanılmamalı
- Yeni servisler `Core/Interfaces/Interfaces.cs`'e arayüz olarak eklenmeli
- `MainViewModel` iş mantığını barındırır; code-behind yalnızca UI olaylarını yönetir
- Tüm pencereler aynı şablonu takip eder: `WindowStyle="None"` + dış shadow border + iç rounded border

## GitHub Releases

Repo: `isa-ozturk/PdfCollector` (public)  
Güncellemeler aynı repo'nun Releases bölümünden indirilir.  
SumatraPDF `tools-v1.0` tag'ine bağlı ayrı bir Release'te tutulur (uygulama versiyonlarından bağımsız).  
GitHub Actions `GITHUB_TOKEN` built-in secret'ını kullanır — ek PAT gerekmez.

GitHub Actions iş akışı:
1. `v*.*.*` etiketi itildiğinde `generate-changelog.yml` tetiklenir
2. Başarılı olunca `release.yml` tetiklenir → EXE derlenir ve aynı repo'ya Release olarak yüklenir
