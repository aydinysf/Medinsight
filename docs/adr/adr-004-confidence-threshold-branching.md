# ADR-004: Confidence Eşiği — Paralel Dallanma

**Durum:** Kabul edildi

## Bağlam

AI analiz sonucu düşük güvenilirlikte (confidence) olduğunda, sistemin hem doktoru
hem hastayı doğru şekilde bilgilendirmesi gerekiyordu. Sıralı bir işlem
(önce doktora bildir, o işlemi bitirince hastaya bildir) gecikmeye ve tek noktadan
arıza riskine yol açardı.

## Karar

`AIAnalysisCompleted` event'i sonrası confidence eşik kontrolü yapılır. Eşik
altındaysa iki event **paralel ve birbirinden bağımsız** tetiklenir:
`DoctorReviewPriorityRaised` ve `PatientNotification`. Biri başarısız olursa
diğerini etkilemez.

## Alternatifler

1. **Tek bir "LowConfidenceHandling" komutu, sıralı adımlarla** — Reddedildi.
   Adımlardan biri (örn. bildirim gönderimi) geçici olarak başarısız olduğunda
   diğerinin (doktor önceliklendirmesi) de bloklanması, hasta güvenliği açısından
   kabul edilemez bir risktir.

2. **Sadece doktor tarafını önceliklendirmek, hastaya bildirim göndermemek** —
   Reddedildi. Hasta, düşük güvenilirlikli bir analizin "kesin" olduğunu
   düşünebilir; şeffaflık ilkesi gereği hasta da bilgilendirilmelidir.

## Sonuç

- Event-driven mimarinin doğal bir avantajı kullanıldı: her subscriber bağımsız
  çalışır.
- Eşik değeri config'den okunur; branş bazlı kalibrasyon Learning Loop'un
  devreye girmesini bekliyor (bkz. Blueprint v3.0 Bölüm 23, Technical Debt).

## İlgili Dosyalar

- `../ai/confidence-management.md`
- `../domain/domain-events-catalog.md`
