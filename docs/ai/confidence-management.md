# Confidence Management

**Durum:** Onaylandı
**İlgili ADR:** [adr-004-confidence-threshold-branching.md](../adr/adr-004-confidence-threshold-branching.md)

## Confidence Skoru Nedir

`AIAnalysisCompleted` event'inin taşıdığı `confidenceScore` alanı, AI'ın kendi
çıkarımına ne kadar güvendiğinin sayısal ifadesidir (0-1 arası). Bu skor modelden
gelir; MedInsight tarafında hesaplanmaz, ama **yorumlanır**.

## Eşik Kontrolü

```
AIAnalysisCompleted
        │
        ▼
Confidence < Threshold?
   │              │
  Evet            Hayır
   │              │
   ▼              ▼
Paralel:      Normal akışa
- DoctorReviewPriorityRaised    devam eder
- PatientNotification
   (bkz. adr-004)
```

## Eşik Değeri Nereden Gelir

MVP'de `Threshold` tek bir global config değeridir. Bu, bilinen bir sınırlamadır
(bkz. Blueprint v3.0 Bölüm 23, Technical Debt) — ideal olarak branş bazlı olmalı,
çünkü bazı branşlarda (örn. nadir hastalıklar) AI'ın doğası gereği daha düşük
confidence üretmesi normaldir ve bu, o branşın "güvenilmez" olduğu anlamına gelmez.

Branş bazlı kalibrasyon, `learning-loop.md`'deki döngünün olgunlaşmasını bekliyor.

## Hastaya Nasıl İletilir

Teknik skor (`0.62` gibi bir sayı) hastaya **asla doğrudan gösterilmez** —
`hizir-personality.md`'deki "Belirsizlik Yönetimi" kuralı gereği bu sayı, doğal
dile çevrilir: "Bu konuda daha emin olabilmek için doktorunun görüşü gerekiyor."

## Doktora Nasıl İletilir

Doktor dashboard'unda (bkz. `../frontend/doctor-dashboard.md`) confidence skoru
sayısal olarak gösterilir çünkü doktor bunu klinik karar verirken bir girdi olarak
kullanır — bu, hastaya yönelik sadeleştirmenin aksine, teknik hassasiyet gerektirir.

## Confidence ile Reviewer Credibility Arasındaki Fark

Confidence, **AI'ın kendi çıktısına güveni**dir. Reviewer credibility
(`../domain/reviewer-profile.md`), **bir doktorun onayının ağırlığı**dır. Bu ikisi
karıştırılmamalıdır: düşük confidence'lı bir AI analizi, yüksek tecrübeli bir
doktor tarafından onaylandığında, nihai güven düzeyi ikisinin bir bileşimidir —
ama bu bileşim şu an otomatik hesaplanmaz, doktorun onayı confidence skorunu
geriye dönük değiştirmez, sadece `HealthRouteSnapshot`'ın `TriggeredBy: Doctor`
olarak işaretlenmesini sağlar.

## İlişkili Dosyalar

- ADR: `../adr/adr-004-confidence-threshold-branching.md`
- Learning Loop: `learning-loop.md`
- Event detayları: `../domain/domain-events-catalog.md`
