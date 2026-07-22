# Reviewer Profile

**Bounded Context:** Consultation (alt bileşen)
**Durum:** Onaylandı

## Amaç

Bir doktorun AI analizine veya bir bulguya verdiği onayın/düzeltmenin "ağırlığını"
belirlemek. Aynı bulguyu bir onkoloji uzmanı ile bir pratisyenin onaylaması aynı
güven değerini taşımamalıdır.

## Şema

```
ReviewerProfile
- DoctorId          (1-1, Doctor ile ayrı tablo — sık güncellenen alanlar)
- Specialty
- YearsOfExperience
- CaseReviewCount
- AverageResponseTime
- CorrectionRate     (AI önerilerini ne sıklıkla düzelttiği — Learning Loop girdisi)
```

## Neden Doctor Tablosundan Ayrı

`CaseReviewCount` ve `AverageResponseTime` gibi alanlar her konsültasyonda güncellenir.
Bunları `Doctor` kimlik tablosuyla aynı satırda tutmak, kimlik bilgisi okuma
işlemleriyle sık güncelleme işlemleri arasında gereksiz lock çakışması yaratır.

## Learning Loop ile İlişki

`CorrectionRate`, AI'ın Learning Loop'unu (bkz. `../ai/learning-loop.md`) besleyen
temel sinyaldir: bir branşta doktorlar sistematik olarak AI'ı düzeltiyorsa
(`CorrectionRate` yüksekse), o branş için confidence eşiği yükseltilir.

## Doktor Puanlaması ile İlişki

MVP'de bu profil sadece iç kullanım amaçlıdır (Doctor Matching Engine'in tecrübe
faktörü). Herkese açık doktor puanı Post-MVP kapsamındadır
(bkz. `../business/doctor-economy.md`).

## İlişkili Dosyalar

- Doktor eşleştirme: `doctor-matching-engine.md`
- Learning Loop: `../ai/learning-loop.md`
- Doktor ekonomisi: `../business/doctor-economy.md`
