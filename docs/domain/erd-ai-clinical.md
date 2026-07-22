# ERD — AI ve Klinik Taraf

**Durum:** Onaylandı — Blueprint v3.0 Bölüm 18.2 ile aynı içerik, kaynak referans burasıdır

## Tablolar

```
AI_ANALYSES
- id PK, case_id FK, model_version, prompt_version, confidence_score

HEALTH_ROUTES
- case_id PK_FK, current_version_id, current_status

HEALTH_ROUTE_SNAPSHOTS
- id PK, case_id FK, previous_version_id, version_number,
  triggered_by, reason, created_at

CONSULTATIONS
- id PK, case_id FK, doctor_id FK, status

REVIEWER_PROFILES
- doctor_id PK_FK, specialty, years_of_experience, case_review_count

DOCTOR_VERIFICATIONS
- id PK, doctor_id FK, verification_method, verification_status
```

## İlişkiler

- `CASES 1:N AI_ANALYSES`
- `CASES 1:1 HEALTH_ROUTES`, `HEALTH_ROUTES 1:N HEALTH_ROUTE_SNAPSHOTS`
- `CASES 1:N CONSULTATIONS`
- `DOCTORS 1:1 REVIEWER_PROFILES`, `DOCTORS 1:N DOCTOR_VERIFICATIONS`

## Tasarım Notları

- `HEALTH_ROUTE_SNAPSHOTS` append-only büyür — `(case_id, created_at)` composite
  index kritik (bkz. `health-route-versioning.md`).
- `notification_preferences` ve `audit_logs` bilerek bu ERD'ye dahil edilmemiştir;
  ikisi de farklı yaşam döngüsüne ve partition stratejisine sahiptir.

## İlişkili Dosyalar

- Identity/case çekirdeği: `erd-identity-case.md`
- Health Route versiyonlama: `health-route-versioning.md`
- Reviewer Profile: `reviewer-profile.md`
