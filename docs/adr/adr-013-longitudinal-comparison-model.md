# ADR-013: Longitudinal Karşılaştırma — Hibrit Tetikleme, İkili Güven Katmanı

**Durum:** Kabul edildi
**Karar tarihi:** Repo dokümantasyon oturumu

## Bağlam

MedInsight'ın senaryosunda "AI özet + karşılaştırma" bir çıktı olarak
bekleniyordu, ama bir Case'in birden fazla zamana yayılmış çalışması arasında
karşılaştırma yapan hiçbir domain kavramı yoktu. İki soru netleştirilmeliydi:
karşılaştırma ne zaman tetiklenir, ve karşılaştırma çıktısı neyi içerir.

## Karar

**Tetikleme:** Hibrit model — aynı Case + aynı modalitede yeni bir çalışma
geldiğinde sistem otomatik bir öneri (`ComparisonSuggested`) üretir, ama asıl
karşılaştırma üretimi doktorun onayı/tetiklemesiyle başlar.

**Çıktı:** Üç bileşenli — metinsel özet, sayısal ölçüm tablosu ve viewer
içinde görsel yan yana karşılaştırma.

**En kritik karar:** Çıktının metinsel kısmı (`TextSummary`, iki radyoloji
raporunun sentezi) ile sayısal/görsel kısmı (`MeasurementDelta`, açık kaynak
görüntü modelinden) **farklı güven katmanlarında** tutulur. İkincisi,
ADR-010'daki tüm sınırlamaları (disclaimer, karar dışı tutma) miras alır.

## Alternatifler

1. **Sadece otomatik, doktor onayı olmadan** — Reddedildi. Doktor onayı
   olmadan üretilen bir "karşılaştırma sonucu", hastaya veya sisteme yanlışlıkla
   kesinmiş gibi görünebilirdi; Doctor Matching Engine'deki "öneri sunar, atama
   yapmaz" felsefesiyle tutarsız olurdu.

2. **Tamamen manuel (öneri de olmadan)** — Reddedildi. Doktorun hangi
   çalışmaların karşılaştırılabilir olduğunu (aynı modalite, aynı bölge) manuel
   taraması, otomatikleştirilebilir bariz bir iş yükü olurdu.

3. **Tek bir güven seviyesinde birleşik çıktı** — Reddedildi. Metin sentezi
   (görece güvenilir, kaynak gösterilebilir) ile açık kaynak model ölçümü
   (doğrulanmamış) aynı ağırlıkta sunulursa, ADR-010'un tüm amacı (açık kaynak
   modelin karar dayanağı olmaması) bu yeni özellik üzerinden dolaylı olarak
   ihlal edilmiş olurdu.

## Sonuç

- `StudyComparison` yeni bir domain varlığı olarak eklendi.
- `dicom-viewer.md`'deki OHIF Viewer'ın yerleşik karşılaştırma (hanging
  protocol) özelliği kullanılır — yeniden inşa edilmez (ADR-012 ile tutarlı).
- Klinik açıdan anlamlı bir karşılaştırma sonucu, doktorun tedavi planı
  güncellemesi üzerinden bir Health Route snapshot'ı tetikleyebilir, ama
  otomatik olarak tetiklemez.

## İlgili Dosyalar

- `../domain/study-comparison.md`
- `../adr/adr-010-open-source-radiology-model-mvp.md`
- `../adr/adr-012-dicom-viewer-choice.md`
