# v1.4.0 - 18 May 2026

## ✨ Yeni Özellikler

- **ZIP'ten Yazdır**: Kullanıcı, daha önce oluşturulmuş herhangi bir ZIP dosyasını seçerek doğrudan yazdırma yapabilir (yeni ZIP oluşturmak zorunlu değil)

## 🎨 Arayüz İyileştirmeleri

- Ana pencere başlığı (title bar) yeniden tasarlandı: `AllowsTransparency`, drop shadow ve yuvarlak köşeler eklendi
- Pencere kontrol butonları (küçült / büyüt / kapat) Segoe MDL2 font ikonlardan SVG Path ikonlarına geçirildi
- Pencere sürükleme artık `MouseLeftButtonDown` + `DragMove()` ile yönetiliyor; `WindowChrome` kaldırıldı
- Maximize / Restore geçişinde shadow margin ve ikon otomatik güncelleniyor
- Footer yeniden tasarlandı: son işlem özeti daha okunaklı, versiyon metni renkli pill badge olarak gösteriliyor
- Tema dosyası `Dark.xaml` → `Styles.xaml` olarak yeniden adlandırıldı

## 🐛 Hata Düzeltmeleri

- PrintWindow yazıcı listesi (`ComboBox`) tıklanınca açılmıyordu — `PART_ToggleButton` eksikliği düzeltildi

## 🧹 Diğer

- `TitleBarButton` stilinden MDL2 font bağımlılığı kaldırıldı, boyut 50×40 olarak güncellendi
- update changelog and version for v1.4.0


# v1.3.0 - 18 May 2026

## ✨ Yeni Özellikler

- redesign UpdateWindow with sidebar, version list and markdown rendering
- redesign UpdateWindow with SDCardCleaner-style Fluent UI

## 🧹 Diğer

- update changelog and version for v1.3.0
- sync AssemblyVersion from CI
- update changelog and version for v1.2.0
