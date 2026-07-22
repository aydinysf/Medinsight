# Text Extraction Service

**Durum:** Onaylandı — OCR sağlayıcısı soyutlanmış, MVP'de sağlayıcı seçimi ertelendi
**İlgili ADR:** [adr-011-ocr-provider-abstraction.md](../adr/adr-011-ocr-provider-abstraction.md)

## Amaç

`ScannedReport` türündeki belgelerden (bkz. `ingestion-pipeline.md`) metin
çıkarmak ve bu metni Hızır'ın `LLMTextAnalysis` girdisine dönüştürmek.
`TextualReport` türü zaten metin katmanına sahip olduğu için bu servisi
atlar, doğrudan metin çıkarılır.

## Sağlayıcı Soyutlaması

OCR motoru seçimi (açık kaynak vs ticari) bilinçli olarak MVP kod tabanına
kilitlenmedi. Bunun yerine bir `IOcrProvider` arayüzü tanımlanır:

```
IOcrProvider
- ExtractText(imageOrPdfBytes) → { text, confidenceScore, provider }
```

İki olası implementasyon:

| Provider | Maliyet | Kalite | Ne Zaman Tercih Edilir |
|---|---|---|---|
| `TesseractOcrProvider` | Ücretsiz, kendi sunucumuzda | Orta | Varsayılan, düşük hacimde |
| `CommercialOcrProvider` (örn. Azure/Google Vision) | Kullanım başına ücretli | Yüksek | Kalite kritikse veya hacim büyüdükçe |

Hangi provider'ın aktif olduğu **konfigürasyondan** okunur (bkz.
`../backend/feature-flags.md`'deki gibi bir switch, ama bu bir feature flag değil,
kalıcı bir yapılandırma anahtarıdır) — kod değişikliği gerektirmeden
değiştirilebilir, hatta belge türüne göre farklı provider seçilebilir (örn.
el yazısı doktor notu için ticari, standart form için açık kaynak).

## Neden Şimdi Karar Verilmedi

OCR kalitesi, gerçek kullanıcı belgeleriyle (Türkçe tıbbi el yazısı, farklı
tarayıcı kaliteleri) test edilmeden seçilirse spekülatif kalır. Soyutlama,
MVP'yi açık kaynak ile ucuza başlatıp, gerçek veriyle kalite yetersiz çıkarsa
kod değişikliği değil sadece konfigürasyon değişikliği ile ticari sağlayıcıya
geçmeyi mümkün kılar.

## Çıktı ve Güven Skoru

`ExtractText` çağrısının döndürdüğü `confidenceScore`, `document-quality-engine.md`'deki
"OCR Score" kriterine doğrudan beslenir — bu iki dosya aynı sinyali paylaşır ama
farklı amaçlarla kullanır: Document Quality Engine bunu "belge yeterince
okunabilir mi" sorusuna, Text Extraction Service ise "bu metne ne kadar
güvenilir" sorusuna cevap vermek için kullanır.

## Hata Durumu

Çıkarılan metnin güven skoru çok düşükse (provider ne olursa olsun), belge
`document-quality-engine.md`'deki "3b. Skor yetersiz" akışına düşer — hastadan
tekrar yükleme istenir, düşük kaliteli OCR çıktısı asla sessizce AI analizine
gönderilmez.

## İlişkili Dosyalar

- Yönlendirme kaynağı: `ingestion-pipeline.md`
- Kalite skoru ilişkisi: `document-quality-engine.md`
- ADR: `../adr/adr-011-ocr-provider-abstraction.md`
