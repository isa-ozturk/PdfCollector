# Değişiklik Günlüğü

## v1.2.0 — 18 Mayıs 2026

### ✨ Yeni Özellikler
- ZIP arşivindeki PDF'leri doğrudan yazıcıya gönderme (PrintWindow ile yazıcı seçimi)
- GitHub Releases tabanlı otomatik güncelleme sistemi (başlangıçta kontrol + manuel kontrol)
- Güncelleme durumu footer butonunda anlık gösterim (⏳ / ✓ / ⬆ / ⚠)
- UpdateWindow: sürüm karşılaştırması ve sürüm notları görünümü
- Uygulama sürüm numarası footer'da gösterim (`AppVersion` binding)
- Son işlem özeti footer'da kalıcı olarak gösterilir

### 🎨 Arayüz İyileştirmeleri
- Pencere boyutu genişletildi (820×720) ve yeniden boyutlandırılabilir yapıldı
- SuccessButton (yeşil), GhostButton (transparan), UpdateButton stilleri eklendi
- Özel ComboBox stili Dark temasına entegre edildi
- Log başlığında kayıt sayacı gösterilir
- Log Temizle butonu ghost stil olarak iyileştirildi
- Kart padding ve spacing dengeleri güncellendi

### 🔧 Teknik Değişiklikler
- `AppSettingsService`: `AutoCheckForUpdates` ayarı ve `Settings` önbellek property'si eklendi
- `RelayCommand`: `RaiseCanExecuteChanged()` metodu eklendi
- `Converters.cs`: `AppInverseBoolToVisConverter` eklendi
- `Core/Interfaces`: `IUpdateService`, `IPrintService` arayüzleri eklendi
- `Core/Models`: `UpdateInfo`, `GithubReleaseInfo`, `GithubAsset` modelleri eklendi

---

## v1.1.0

### ✨ Yeni Özellikler
- İlerleme çubuğu ve gerçek zamanlı log görünümü
- Ayar kalıcılığı (JSON)
- İptal desteği (CancellationToken)
- Log dosyasına kaydetme seçeneği
- İşlem sonrası otomatik kapanma seçeneği

---

## v1.0.0

- İlk sürüm: PDF tarama, ZIP oluşturma, klasör temizleme
