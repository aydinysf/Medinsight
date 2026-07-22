# Study Comparison — Longitudinal Karşılaştırma

**Bounded Context:** Case Aggregate (alt bileşen, DICOMStudies'e bağlı)
**Durum:** Onaylandı
**İlgili ADR:** [adr-013-longitudinal-comparison-model.md](../adr/adr-013-longitudinal-comparison-model.md)

## Amaç

Bir hastanın aynı Case içinde birden fazla zamanda çekilmiş aynı modaliteden
(örn. iki farklı tarihli MR) görüntülerini karşılaştırıp "değişim var mı, ne
yönde" sorusuna cevap üretmek.

## Neden Ayrı Bir Kavram

`case-aggregate-root.md`'de `DICOMStudies` bir Case'e bağlı birden fazla çalışma
barındırabilir, ama iki çalışma arasındaki **ilişkiyi** (biri diğerinin takibi
mi) hiçbir yapı temsil etmiyordu. `StudyComparison`, bu ilişkiyi ve ondan
üretilen sentezi taşıyan ayrı bir varlıktır.

## Tetikleme Modeli — Hibrit

```
Yeni DICOMStudyGrouped (bkz. ingestion-pipeline.md)
        │
        ▼
Aynı Case + aynı Modality + (varsa) aynı BodyPartExamined
ile eşleşen önceki bir DICOMStudy var mı?
        │
       Evet
        │
        ▼
ComparisonSuggested  (doktora bildirim: "önceki MR ile karşılaştırmak ister misin?")
        │
        ▼
👤 Doktor onaylar → RequestStudyComparison (komut)
        │
        ▼
StudyComparison oluşturulur, Status: Requested
        │
        ▼
Karşılaştırma üretimi (aşağıda) çalışır
        │
        ▼
StudyComparisonCompleted
```

Sistem hiçbir zaman doktor onayı olmadan bir karşılaştırmayı "tamamlanmış"
olarak sunmaz — otomatik olan sadece **öneri**, üretim doktor tetiklemesiyle
başlar. Bu, Doctor Matching Engine'in "öneri sunar, atama yapmaz" ilkesiyle
(bkz. `doctor-matching-engine.md`) aynı felsefeyi paylaşır.

## Şema

```
StudyComparison
- Id
- CaseId
- BaselineStudyId       (daha eski çalışma)
- FollowUpStudyId        (daha yeni çalışma)
- RequestedByDoctorId
- Status                 (Suggested | Requested | Completed)
- TextSummary
- MeasurementDelta        (jsonb — aşağıda detaylı)
- CreatedAt
- CompletedAt
```

## Karşılaştırmanın İki Ayrı Güven Katmanı

Bu tasarımın en kritik noktası: karşılaştırma çıktısı **tek bir güven
seviyesinde değildir**, iki farklı kaynaktan gelir ve bunlar asla birbirine
karıştırılmaz.

### 1. TextSummary — Metin Sentezi (LLMTextAnalysis kaynaklı)

Hızır, her iki çalışmaya ait **radyoloğun yazılı raporlarını** (varsa) veya
`AIFindings.Source = LLMTextAnalysis` bulgularını karşılaştırıp doğal dilde bir
özet üretir: "Önceki rapor 2.1cm belirtirken, güncel rapor 2.4cm belirtiyor."
Bu, iki metni sentezlemektir — görüntüyü yeniden yorumlamak değil. Kaynak
izlenebilirliği ilkesi burada da geçerlidir (bkz.
`../ai/guardrails-and-boundaries.md`): özet, hangi iki rapordan üretildiğine
referans verir.

### 2. MeasurementDelta — Sayısal/Görsel Ölçüm (OpenSourceImageModel kaynaklı)

Eğer her iki çalışma da Radiology Inference Service'ten (bkz.
`../architecture/radiology-inference-service.md`) geçtiyse, iki segmentasyon
çıktısı karşılaştırılıp bir boyut/konum farkı hesaplanabilir. **Bu veri, ADR-010'daki
tüm sınırlamaları aynen miras alır**: `disclaimer` zorunludur, confidence
hesabına girmez, `DifferentialDiagnosis`'u besleyemez, doktor dashboard'unda
"deneysel" etiketiyle ayrı gösterilir. TextSummary ile aynı görsel ağırlıkta
sunulmaz.

```
MeasurementDelta (jsonb örneği)
{
  "source": "OpenSourceImageModel",
  "disclaimer": "Bu ölçüm karşılaştırması deneysel bir modelden üretilmiştir...",
  "findings": [
    { "structure": "kitle", "baselineSizeMm": 21, "followUpSizeMm": 24, "deltaMm": 3 }
  ]
}
```

## Görsel Karşılaştırma (Viewer İçinde)

`../architecture/dicom-viewer.md`'de benimsenen OHIF Viewer, yan yana
karşılaştırma (comparison/hanging protocol) özelliğini **zaten** destekler —
bu, ADR-012'deki "mevcut olanı yeniden icat etme" ilkesinin doğrudan
faydasıdır. MedInsight'ın eklediği tek şey, `StudyComparison` kaydını
viewer'ın hangi iki study'yi yan yana açacağını belirlemek için kullanmaktır.

## Health Route İlişkisi

Bir `StudyComparisonCompleted` event'i, klinik açıdan anlamlı bir değişim
gösteriyorsa (örn. `MeasurementDelta` belirli bir eşiği aşıyorsa veya
`TextSummary` bir kötüleşme/iyileşme belirtiyorsa), bir `HealthRouteSnapshot`
tetikleyebilir (`TriggeredBy: AI`, `TriggerSourceId: StudyComparison.Id`) —
ama bu otomatik değildir, doktorun tedavi planı güncellemesi üzerinden geçer
(bkz. `health-route-versioning.md`).

## İlişkili Dosyalar

- Case yapısı: `case-aggregate-root.md`
- Görüntü analizi ve sınırları: `../architecture/radiology-inference-service.md`
- Viewer: `../architecture/dicom-viewer.md`
- ADR: `../adr/adr-013-longitudinal-comparison-model.md`
