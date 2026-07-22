# Event Storming — Uçtan Uca Vaka Akışı

**Durum:** Onaylandı
**Amaç:** DDD pratiği gereği, sistemin tüm domain event'lerini aktörler, komutlar ve
politikalarla birlikte tek bir akışta görünür kılmak.

## Gösterim

- 🟧 **Command** (kullanıcı veya sistem isteği)
- 🟨 **Event** (gerçekleşmiş olay, geçmiş zaman)
- 🟦 **Policy** (bir event'e tepki olarak tetiklenen otomatik kural)
- 🟩 **Read Model** (bir sorgunun cevapladığı görünüm)
- 👤 **Actor** (event'i tetikleyen rol)

## Ana Akış: MR Yükleyen Hastadan Vaka Kapanışına

```
👤 Patient
🟧 UploadDocument
🟨 DocumentUploaded
    │
🟦 Policy: "Belge yüklendiğinde kalite kontrolü başlat"
    │
🟨 DocumentQualityScored
    │
    ├─[skor yeterli]──────────────────────┐
    │                                      │
🟦 Policy: "Skor yetersizse hastadan       🟦 Policy: "Skor yeterliyse AI analizini
    tekrar iste"                              başlat"
    │                                      │
🟨 DocumentReuploadRequested          🟨 AIAnalysisRequested
                                            │
                                      🟨 AIAnalysisCompleted
                                            │
                        ┌───────────────────┴────────────────────┐
                        │                                         │
              🟦 Policy: "Confidence düşükse         🟦 Policy: "Her analiz Health
                 doktor review'a öncelik ver"           Route snapshot'ı tetikler"
                        │                                         │
              🟨 DoctorReviewPriorityRaised          🟨 HealthRouteSnapshotCreated
                        │                                         │
              🟨 PatientNotified                      🟦 Policy: "Yeni snapshot doktor
                 ("doktor onayı bekliyor")               eşleştirmesini tetikler"
                                                                    │
                                                          🟨 DoctorMatchRequested
                                                                    │
                                                          🟨 DoctorMatchResultReady
                                                                    │
                                                          🟨 PatientNotified
                                                             ("doktor önerileri hazır")

👤 Patient / Caregiver
🟧 SelectDoctor
🟨 ConsultationStarted
    │
👤 Doctor
🟧 ReviewAIAnalysis
🟨 AIAnalysisReviewed  (Approved | Corrected)
    │
🟦 Policy: "Doktor düzelttiyse ReviewerProfile.CorrectionRate güncellenir"
    │
🟨 ReviewerProfileUpdated
    │
👤 Doctor
🟧 WriteClinicalNote
🟨 ClinicalNoteAdded
    │
👤 Doctor
🟧 CreateTreatmentPlan
🟨 TreatmentPlanCreated
    │
🟦 Policy: "Tedavi planı zorunlu olarak yeni Health Route snapshot'ı üretir"
    │
🟨 HealthRouteSnapshotCreated  (TriggeredBy: Doctor)
    │
👤 Patient
🟧 AcknowledgeTreatmentPlan
🟨 PatientDecisionRecorded
    │
🟦 Policy: "Hasta kararı da bir Health Route snapshot'ıdır"
    │
🟨 HealthRouteSnapshotCreated  (TriggeredBy: Patient)
    │
   ... (FollowUp döngüsü — bkz. aşağıda) ...
    │
👤 Doctor / Admin
🟧 CloseCase
🟨 CaseClosed
```

## FollowUp Döngüsü

```
🟨 CaseClosed
    │
👤 Patient
🟧 ReportNewSymptom  (kronik takip veya nüks)
🟨 CaseReopened
    │
🟦 Policy: "Yeniden açılan vaka FollowUp durumuna döner, Closed'a değil"
    │
🟨 CaseStatusChanged  (Closed → FollowUp)
    │
   ... (CollectingData'dan itibaren akış tekrar başlar) ...
```

## Read Model'ler (Bu Akışın Cevapladığı Sorular)

| Read Model | Hangi event'lerden beslenir |
|---|---|
| Doktor öncelik kuyruğu | `DoctorReviewPriorityRaised` |
| Case Timeline | Tüm event'ler (append-only) |
| Health Route (current) | En son `HealthRouteSnapshotCreated` |
| Doktor performans paneli | `AIAnalysisReviewed`, `ReviewerProfileUpdated` |
| Hasta bildirim geçmişi | `PatientNotified` |

## Bu Akıştan Çıkan Mimari Kararlar

- Her state geçişi bir event üretir → Timeline Engine bunları pasif biçimde dinler
  (bkz. `../architecture/timeline-service.md`).
- Hiçbir command doğrudan bir read model'i güncellemez; her zaman bir event üzerinden
  geçer — bu CQRS ayrımını netleştirir.
- `PatientNotified` event'i, farklı policy'lerden birden fazla kez tetiklenebilir;
  Notification Engine bunları kanal tercihine göre birleştirip tekilleştirmelidir
  (bkz. `../architecture/notification-engine.md`).

## İlişkili Dosyalar

- Tam event kataloğu (payload detaylarıyla): `domain-events-catalog.md`
- Case yaşam döngüsü: `case-lifecycle-state-machine.md`
