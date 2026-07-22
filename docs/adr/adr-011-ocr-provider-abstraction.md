# ADR-011: OCR Sağlayıcısı Soyutlaması — Karar Ertelendi

**Durum:** Kabul edildi
**Karar tarihi:** Repo dokümantasyon oturumu

## Bağlam

Taranmış PDF/rapor belgelerinden metin çıkarmak için açık kaynak (Tesseract)
veya ticari (Azure/Google Vision gibi) bir OCR motoru seçilebilirdi. İkisi
arasında gerçek kaliteyi belirleyen şey, gerçek kullanıcı belgeleriyle
(Türkçe tıbbi el yazısı, çeşitli tarama kaliteleri) test etmektir — bu test
henüz yapılmadı.

## Karar

Şimdi bir sağlayıcı seçmek yerine bir `IOcrProvider` arayüzü tanımlandı.
MVP açık kaynak (Tesseract) ile başlar, ama sağlayıcı değişimi kod değişikliği
değil konfigürasyon değişikliği gerektirir.

## Alternatifler

1. **Hemen ticari sağlayıcı seçmek** — Reddedildi. Gerçek veri olmadan maliyet
   taahhüdü vermek erken; düşük hacimde açık kaynak yeterli olabilir.

2. **Hemen açık kaynağa kilitlenmek (soyutlama olmadan)** — Reddedildi. Kalite
   yetersiz çıkarsa, tüm OCR çağrı noktalarını değiştirmek gerekirdi;
   soyutlamanın maliyeti düşük, faydası yüksek.

## Sonuç

- Yeni bir belge türü veya provider eklendiğinde sadece `IOcrProvider`
  implementasyonu eklenir, `text-extraction-service.md`'deki çağıran kod
  değişmez.
- Provider seçimi kararı, gerçek kullanıcı verisiyle bir pilot sonrası
  netleşecek — bu netleştiğinde bu ADR "Superseded" olarak işaretlenip yeni
  bir ADR açılabilir.

## İlgili Dosyalar

- `../architecture/text-extraction-service.md`
