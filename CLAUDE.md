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
| Tema | Karanlık mod (Tailwind renk paleti) |
| Geliştirici | İsa Öztürk |

## Mimari Katmanlar

```
PdfCollector/
├── Core/                    # Domain modelleri & arayüzler
│   ├── Interfaces/
│   │   └── Interfaces.cs    # IPdfScannerService, IZipService, ILogService,
│   │                        # IFolderCleanupService, IUpdateService, IPrintService
│   └── Models/              # LogEntry, PdfFileInfo, CollectionResult,
│                            # UpdateInfo, GithubReleaseInfo
├── Application/
│   ├── DTOs/CollectionOptions.cs
│   └── Services/PdfCollectionService.cs  # Orkestrasyon: tara → zip → temizle
├── Infrastructure/Services/ # Servis implementasyonları
│   ├── PdfScannerService.cs
│   ├── ZipService.cs
│   ├── FolderCleanupService.cs
│   ├── LogService.cs
│   ├── AppSettingsService.cs   # JSON ayar kalıcılığı
│   ├── UpdateService.cs        # GitHub Releases API güncelleme
│   └── PrintService.cs         # ZIP → yazıcı
├── Presentation/
│   ├── Commands/RelayCommand.cs
│   ├── Converters/Converters.cs
│   ├── ViewModels/
│   │   ├── ViewModelBase.cs
│   │   ├── LogEntryViewModel.cs
│   │   ├── MainViewModel.cs    # Ana uygulama mantığı
│   │   └── UpdateViewModel.cs
│   └── Views/
│       ├── MainWindow.xaml(.cs)
│       ├── UpdateWindow.xaml(.cs)
│       └── PrintWindow.xaml(.cs)
└── Themes/Dark.xaml            # Tüm stil kaynakları
```

## Temel Özellikler

### ZIP Oluşturma
- `PdfScannerService.Scan()` → alt klasörleri özyinelemeli tarar
- `ZipService.CreateZipAsync()` → PDF'leri timestamp'li ZIP'e koyar
- Çakışan dosya adlarında üst klasör adı önek olarak eklenir
- `FolderCleanupService` → yalnızca PDF içeren klasörleri siler (isteğe bağlı)

### Yazdırma
- `PrintService.PrintPdfsFromZipAsync()` → ZIP'ten geçici klasöre çıkartır
- `Process.Start` ile "printto" fiili → seçili yazıcıya gönderir
- Geçici dosyalar yazdırmadan 8 saniye sonra otomatik silinir

### Güncelleme
- GitHub API: `https://api.github.com/repos/isa-ozturk/PdfCollector-releases/releases`
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
  "AutoCheckForUpdates": true
}
```

## Tema & Stil

`Themes/Dark.xaml` dosyasında tanımlı brush'lar ve kontrol stilleri:

| Anahtar | Kullanım |
|---------|---------|
| `PrimaryButton` | Ana eylem butonu (mavi) |
| `SecondaryButton` | İkincil buton (gri kenarlık) |
| `SuccessButton` | Yeşil eylem butonu (Yazdır) |
| `GhostButton` | Transparan arka plan butonu |
| `UpdateButton` | Footer güncelleme butonu |
| `ToggleCheckBox` | Açma/kapama switch'i |
| `ModernProgressBar` | Yuvarlak köşeli ince progress bar |
| `LogListBox` | Konsol tarzı log listesi |
| `CardBorder` | İçerik kartı kenarlık stili |

## Sürümleme

Sürüm numarası `PdfCollector.csproj` dosyasındaki `<AssemblyVersion>` ile belirlenir.
GitHub Actions `release.yml` bu değeri otomatik günceller.

Sürüm formatı: `MAJOR.MINOR.PATCH` (örn. `v1.2.0`)

## Katkı & Geliştirme Kuralları

- Commit mesajları Conventional Commits formatında olmalı: `feat(...)`, `fix(...)`, `chore(...)`
- UI değişikliklerinde `Themes/Dark.xaml` stilleri korunmalı; satır içi stil kullanılmamalı
- Yeni servisler `Core/Interfaces/Interfaces.cs`'e arayüz olarak eklenmeli
- `MainViewModel` iş mantığını barındırır; code-behind yalnızca UI olaylarını yönetir
- `AppSettingsService.Settings` property'si `_current` önbelleğini kullanır; tekrar `Load()` çağrılmamalı

## GitHub Releases

Repo: `isa-ozturk/PdfCollector` (public)  
Güncellemeler aynı repo'nun Releases bölümünden indirilir.  
GitHub Actions `GITHUB_TOKEN` built-in secret'ını kullanır — ek PAT gerekmez.

GitHub Actions iş akışı:
1. `v*.*.*` etiketi itildiğinde `generate-changelog.yml` tetiklenir
2. Başarılı olunca `release.yml` tetiklenir → EXE derlenir ve aynı repo'ya Release olarak yüklenir
