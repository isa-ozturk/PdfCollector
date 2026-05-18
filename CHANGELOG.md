# v1.3.0 - 18 Mayıs 2026

## ✨ Yeni Özellikler

- Güncelleme penceresi yeniden tasarlandı: sol kenar çubuğu ile sürüm listesi
- Markdown desteği: sürüm notları başlık, bullet, bold ve kod bloğu ile okunakli görünüm
- Güncelleme penceresinde sürüm karşılaştırması (mevcut → yeni pill badge)
- `MarkdownHelper` attached property ile WPF TextBlock'ta inline markdown render

## 🐞 Hata Düzeltmeleri

- Güncelleme penceresi Unicode semboller yerine SVG Path ikonları kullanıyor
- İndirme sırasında overlay modal, footer progress bar ile çakışma giderildi

---

# v1.2.0 - 18 Mayıs 2026

## ✨ Yeni Özellikler

- ZIP arşivindeki PDF'leri doğrudan yazıcıya gönderme (PrintWindow ile yazıcı seçimi)
- GitHub Releases tabanlı otomatik güncelleme sistemi
- Windows 11 Fluent Design arayüzü — mavi gradient başlık çubuğu, özel pencere kontrolleri
- Uygulama sürüm numarası footer'da gösterim

## 🐞 Hata Düzeltmeleri

- GitHub Actions release pipeline path ve izin hataları giderildi
- Proje `src/PdfCollector/` yapısına taşındı

