# Case Lifecycle — State Machine

**Durum:** Onaylandı

## Durumlar

```
Draft → CollectingData → AIAnalysis → DoctorReview → Treatment → FollowUp → Closed
                ↑                                          │           │
                └──────────────────────────────────────────┘           │
                         (FollowUp: yeni veri geldi)                   │
                                                                         │
                                    Closed ←──────────────────────────┘
                                       │
                                       └──→ FollowUp (vaka yeniden açıldı)
```

## Geçiş Kuralları

| Geçiş | Tetikleyici | Koşul |
|---|---|---|
| Draft → CollectingData | İlk belge yüklendiğinde | — |
| CollectingData → AIAnalysis | Belge kalite kontrolünden geçtiğinde | En az bir belge `DocumentQualityScored` (yeterli) |
| AIAnalysis → DoctorReview | `AIAnalysisCompleted` | Her zaman — confidence düşükse öncelik yükselir ama durum değişimi aynı |
| DoctorReview → Treatment | `TreatmentPlanCreated` | Doktor onayı zorunlu |
| Treatment → FollowUp | Tedavi planı sonrası otomatik | Kontrol tarihi planlandığında |
| FollowUp → CollectingData | Yeni veri geldiğinde | Yeni belge veya semptom bildirimi |
| FollowUp → Closed | Doktor veya admin kapatır | Aktif takip gerekmiyor |
| Closed → FollowUp | Hasta yeni semptom bildirir | `CaseReopened` event'i |

## Neden Closed → Draft Değil, Closed → FollowUp

Bir vaka yeniden açıldığında sıfırdan başlamaz — hastanın tüm geçmiş belgeleri,
analizleri ve tedavi planları korunur. `FollowUp` durumu, "bu vaka devam eden bir
ilişkinin parçası" anlamına gelir; `Draft`'a dönmek bu sürekliliği yanlış temsil
eder.

## Durum Değişiminin Yan Etkileri

Her durum değişimi bir `CaseStatusChanged` event'i üretir (bkz.
`domain-events-catalog.md`) ve bu doğrudan Timeline'a düşer. Hiçbir durum değişimi
sessiz gerçekleşmez.

## İlişkili Dosyalar

- Case yapısı: `case-aggregate-root.md`
- Event akışı: `event-storming.md`
- UML State Diagram: `../uml/state-diagram.md`
