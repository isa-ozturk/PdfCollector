<p align="center">
  <img src="docs/screenshot.png" alt="PdfCollector Ekran Görüntüsü" width="720"/>
</p>

<h1 align="center">PdfCollector</h1>

<p align="center">
  Alt klasörlerdeki PDF dosyalarını tek ZIP arşivinde toplayan, yazıcıya gönderen ve otomatik güncellenen Windows masaüstü uygulaması.
</p>

<p align="center">
  <a href="https://github.com/isa-ozturk/PdfCollector/releases/latest">
    <img alt="Son Sürüm" src="https://img.shields.io/github/v/release/isa-ozturk/PdfCollector?style=flat-square&color=0078D4"/>
  </a>
  <a href="https://github.com/isa-ozturk/PdfCollector/releases">
    <img alt="İndirmeler" src="https://img.shields.io/github/downloads/isa-ozturk/PdfCollector/total?style=flat-square&color=107C10"/>
  </a>
  <img alt="Platform" src="https://img.shields.io/badge/platform-Windows%2010%2F11-0078D4?style=flat-square"/>
  <img alt=".NET" src="https://img.shields.io/badge/.NET%20Framework-4.7.2-512BD4?style=flat-square"/>
  <a href="LICENSE">
    <img alt="Lisans" src="https://img.shields.io/github/license/isa-ozturk/PdfCollector?style=flat-square"/>
  </a>
</p>

---

## Özellikler

| Özellik | Açıklama |
|---|---|
| **PDF Toplama** | Seçilen klasör ve tüm alt klasörlerindeki PDF dosyalarını otomatik tarar |
| **ZIP Arşivleme** | Bulunan tüm PDF'leri tek bir `.zip` dosyasında toplar |
| **Yazıcıya Gönder** | ZIP içindeki PDF'leri kurulu yazıcılardan birine doğrudan yazdırır |
| **Klasör Temizleme** | İşlem sonrasında kaynak PDF dosyalarını isteğe bağlı olarak siler |
| **İşlem Kaydı** | Tüm adımlar gerçek zamanlı log listesinde gösterilir, dosyaya kaydedilebilir |
| **Otomatik Güncelleme** | GitHub Releases üzerinden yeni sürüm kontrolü ve tek tıkla güncelleme |

---

## Kurulum

### Hazır EXE (Önerilen)

1. [Releases](https://github.com/isa-ozturk/PdfCollector/releases/latest) sayfasından `PdfCollector.exe` dosyasını indirin.
2. Herhangi bir klasöre koyun ve çalıştırın — kurulum gerekmez.

> Uygulama, ayarlarını çalıştırıldığı dizindeki `PdfCollector.settings.json` dosyasına kaydeder.

### Gereksinimler

- Windows 10 / 11 (64-bit)
- [.NET Framework 4.7.2](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472) (Windows 10/11'de varsayılan olarak yüklüdür)

---

## Kullanım

1. **Klasör Seç** — Taranan kök klasörü seçin. Alt klasörler dahil tüm PDF'ler bulunur.
2. **Topla ve Sıkıştır** — Bulunan PDF'ler belirlenen hedefe `.zip` olarak arşivlenir.
3. **Yazdır** *(isteğe bağlı)* — ZIP oluşturulduktan sonra **Yazdır** butonuna tıklayın, yazıcı seçin ve onaylayın.
4. **Seçenekler** — Kaynak dosyaları sil, işlem kaydını dosyaya yaz, işlem bitince uygulamayı kapat gibi seçenekleri etkinleştirin.

### Otomatik Güncelleme

Uygulama her başlangıçta GitHub Releases API'sini sorgular. Yeni sürüm bulunduğunda başlık çubuğundaki güncelleme butonu aktif hale gelir; tıklandığında sürüm notlarıyla birlikte bir onay penceresi açılır. Güncelleme onaylanınca yeni EXE indirilir ve uygulama yeniden başlar.

---

## Derleme

```bash
git clone https://github.com/isa-ozturk/PdfCollector.git
cd PdfCollector
dotnet build PdfCollector.csproj
```

Çıktı: `bin\Debug\net472\PdfCollector.exe`

### Release Derleme

```bash
dotnet build PdfCollector.csproj -c Release
```

> CI/CD pipeline (`release.yml`) her `v*.*.*` etiketinde otomatik olarak Release derlemesi yapıp GitHub'a yayınlar.

---

## Proje Yapısı

```
PdfCollector/
├── Core/                       # Alan modelleri ve arayüzler (bağımlılıksız)
│   ├── Interfaces/
│   └── Models/
├── Application/                # İş mantığı, uygulama servisleri
│   ├── DTOs/
│   └── Services/
├── Infrastructure/             # Harici servis implementasyonları
│   └── Services/               # ZipService, PdfScannerService, UpdateService, PrintService…
├── Presentation/               # WPF katmanı (MVVM)
│   ├── Commands/
│   ├── Converters/
│   ├── ViewModels/
│   └── Views/
├── Themes/
│   └── Dark.xaml               # Windows 11 Fluent tasarım teması
├── .github/
│   └── workflows/
│       ├── generate-changelog.yml
│       └── release.yml
├── CHANGELOG.md
└── PdfCollector.csproj
```

---

## Teknoloji Yığını

- **Platform:** .NET Framework 4.7.2 + WPF
- **Dil:** C# 11
- **Mimari:** Clean Architecture + MVVM
- **Tema:** Windows 11 Fluent Design (özel başlık çubuğu, WindowChrome)
- **Güncelleme:** GitHub Releases API + PowerShell installer
- **CI/CD:** GitHub Actions — MSBuild ile derleme, otomatik Release yayınlama
- **Harici bağımlılık yok** — NuGet paketi kullanılmamıştır

---

## Katkı

Hata bildirimi veya özellik önerisi için [Issues](https://github.com/isa-ozturk/PdfCollector/issues) bölümünü kullanabilirsiniz.

Pull request göndermek isteyenler için:

1. Bu repoyu fork edin.
2. Yeni bir dal oluşturun: `git checkout -b feature/ozellik-adi`
3. Değişikliklerinizi commit edin: `git commit -m "feat: açıklama"`
4. Dalınızı push edin: `git push origin feature/ozellik-adi`
5. Pull request açın.

---

## Lisans

[MIT](LICENSE) — © 2026 Isa Ozturk
