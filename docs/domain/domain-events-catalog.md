# Domain Events Catalog

**Durum:** Onaylandı — event-storming.md ile birlikte okunmalı

Bu dosya, sistemdeki her domain event'in payload'ını ve subscriber'larını içerir.
`event-storming.md` akışı, bu event'lerin birbirine nasıl bağlandığını gösterir.

## Belge ve Kalite

### DocumentClassified
```
{ documentId, caseId, documentType: "DicomFile" | "TextualReport" |
  "ScannedReport" | "PhotoDocument", classifiedAt }
```
**Subscriber:** DICOM Grouping (DicomFile ise), Document Quality Engine

### DocumentClassificationFailed
```
{ documentId, caseId, reason, failedAt }
```
**Subscriber:** Notification Engine

### DICOMStudyGrouped
```
{ studyId, caseId, seriesList: [{ seriesId, modality, sliceCount }], groupedAt }
```
**Subscriber:** Document Quality Engine, Routing

### RoutingDecided
```
{ documentId, caseId, route: "TextExtraction" | "RadiologyInference" | "StorageOnly",
  decidedAt }
```
**Subscriber:** Text Extraction Service veya Radiology Inference Service

### DocumentUploaded
```
{ documentId, caseId, documentType, uploadedByUserId, uploadedAt }
```
**Subscriber:** Document Quality Engine

### DocumentQualityScored
```
{ documentId, caseId, overallScore, criteriaScores: {...}, failureReasons: [] }
```
**Subscriber:** AI Analysis Engine (skor yeterliyse), Notification Engine (yetersizse)

## AI Analiz

### AIAnalysisRequested
```
{ caseId, documentIds: [], requestedAt }
```
**Subscriber:** AI Analysis Engine

### AIAnalysisCompleted
```
{ analysisId, caseId, modelVersion, promptVersion, confidenceScore, findings: [] }
```
**Subscriber:** Health Route Engine, Doctor Review Queue, Notification Engine

### AIAnalysisReviewed
```
{ analysisId, doctorId, decision: "Approved" | "Corrected", correctionNotes? }
```
**Subscriber:** Reviewer Profile, Learning Loop pipeline

## Sağlık Rotası

### HealthRouteSnapshotCreated
```
{ snapshotId, caseId, previousVersionId, versionNumber, status, nextStep,
  riskLevel, triggeredBy: "AI" | "Doctor" | "Patient" | "System",
  triggerSourceId, reason, createdAt }
```
**Subscriber:** Timeline Engine, Notification Engine

## Longitudinal Karşılaştırma

### ComparisonSuggested
```
{ caseId, baselineStudyId, followUpStudyId, modality, suggestedAt }
```
**Subscriber:** Notification Engine (doktora bildirim)

### StudyComparisonCompleted
```
{ comparisonId, caseId, textSummary, measurementDelta?, completedAt }
```
**Subscriber:** Timeline Engine, Health Route Engine (doktor tedavi planı
güncellemesi üzerinden dolaylı olarak)

### EscalationSuggested
```
{ caseId, reason: "HighRiskWithUnvalidatedFinding" | "DoctorRequested",
  suggestedAt }
```
**Subscriber:** Doctor Review Queue (önceliklendirme), Notification Engine.
MVP'de vendor API çağrısı tetiklemez (bkz. `../adr/adr-014-third-party-escalation-trigger.md`).

## Doktor Eşleştirme ve Konsültasyon

### DoctorMatchRequested
```
{ caseId, requiredSpecialty, patientLocation? }
```
**Subscriber:** Doctor Matching Engine

### DoctorMatchResultReady
```
{ caseId, results: [{ doctorId, score, scoreBreakdown }] }
```
**Subscriber:** Notification Engine

### ConsultationStarted
```
{ consultationId, caseId, doctorId, startedAt }
```
**Subscriber:** Timeline Engine

### ConsultationMessageSent
```
{ messageId, consultationId, senderUserId, sentAt }
```
**Subscriber:** Notification Engine (içerik taşımaz — gizlilik)

### TreatmentPlanCreated
```
{ treatmentId, caseId, consultationId, createdByDoctorId, createdAt }
```
**Subscriber:** Health Route Engine (zorunlu snapshot tetikler)

### DoctorAvailabilityChanged
```
{ doctorId, previousStatus, newStatus, changedBy: "System" | "Doctor",
  overrideExpiresAt?, changedAt }
```
**Subscriber:** Doctor Matching Engine (sonraki öneri sorgularında kullanılır)

## Doktor Doğrulama

### DoctorVerificationSubmitted
```
{ verificationId, doctorId, documentType, submittedAt }
```
**Subscriber:** Admin review kuyruğu

### DoctorVerified / DoctorVerificationRejected
```
{ verificationId, doctorId, verifiedByAdminId, verifiedAt, rejectionReason? }
```
**Subscriber:** Notification Engine

## Case Yaşam Döngüsü

### CaseStatusChanged
```
{ caseId, fromStatus, toStatus, changedAt, reason? }
```
**Subscriber:** Timeline Engine, Audit Service

### CaseClosed / CaseReopened
```
{ caseId, closedAt } / { caseId, reopenedAt, reason }
```
**Subscriber:** Timeline Engine, Notification Engine

## Genel Kural

Her event, aşağıdaki ortak zarfı taşır (event envelope):

```
{ eventId, eventType, occurredAt, caseId, causationId, correlationId }
```

`causationId`, bu event'i hangi command'ın veya event'in tetiklediğini; `correlationId`,
tüm zincirin aynı iş akışına ait olduğunu izlemek için kullanılır — dağıtık sistemde
debug edilebilirlik için zorunludur.

## İlişkili Dosyalar

- Akış görünümü: `event-storming.md`
- API event contract'ları: `../api/event-contracts.md`
