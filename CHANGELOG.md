# v1.5.0 - 18 May 2026

## ✨ Yeni Özellikler

- **Sistem Durumu (Health Check)**: Başlık çubuğuna "Sistem Durumu" butonu eklendi. Yazıcı kurulumu, SumatraPDF ve Windows PDF görüntüleyici varlığını renkli rozetlerle gösterir; yeşil / turuncu / kırmızı renk ile genel durum özetlenir
- **SumatraPDF Entegrasyonu**: PDF dosyaları artık sisteme PDF görüntüleyici kurulmadan doğrudan yazıcıya gönderilebiliyor. SumatraPDF yoksa HealthCheckWindow üzerinden otomatik indirilir (~20 MB)
- **İlk Başlangıç Uyarısı**: SumatraPDF kurulu değilse uygulama açılışında modern bir bilgi penceresi gösterilir. "Hayır, Sorma" seçilirse bir daha gösterilmez (ayar dosyasına kaydedilir)
- **Modern SumatraPDF Uyarı Penceresi**: SumatraPDF eksik uyarısı artık Windows varsayılan MessageBox yerine gradient başlıklı, üç seçenekli özel `SumatraPdfPromptWindow` ile gösteriliyor (İndir / Şimdi Değil / Hayır, Sorma)

## 🎨 Arayüz İyileştirmeleri

- **PrintWindow geniş progress bar**: Yazdırma sırasında determinate progress bar gösterilir; dosya sayacı (örn. `3 / 10`) ve geçerli dosya adı gerçek zamanlı güncellenir
- **HealthCheckWindow**: Her bileşen için renkli durum rozeti, indirme ilerleme çubuğu ve iptal butonu — HealthCheckWindow kapanınca ana pencere durumu otomatik yenilenir
- Sistem Durumu butonu sağlık durumuna göre yeşil / turuncu / kırmızı renk alıyor; başlangıçta ⏳ animasyonu gösterilir

## 🐛 Hata Düzeltmeleri

- SumatraPDF indirildikten sonra Sistem Durumu göstergesi HealthCheckWindow kapanınca otomatik güncelleniyor (önceden uygulamayı yeniden açmak gerekiyordu)

---

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
- ItemsGenerator tutarsızlığı ve thread-safe logging iyileştirildi

## 🧹 Diğer

- `TitleBarButton` stilinden MDL2 font bağımlılığı kaldırıldı, boyut 50×40 olarak güncellendi

---

# v1.3.0 - 18 May 2026

## ✨ Yeni Özellikler

- UpdateWindow yeniden tasarlandı: kenar çubuğu, sürüm listesi ve markdown render desteği eklendi

---

# v1.2.0 - 18 May 2026

## ✨ Yeni Özellikler

- GitHub Releases üzerinden otomatik güncelleme kontrolü ve tek tıkla güncelleme
- Güncelleme penceresi (`UpdateWindow`) eklendi: sürüm notları ve indirme butonu
