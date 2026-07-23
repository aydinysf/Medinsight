# ERD — Identity ve Case Çekirdeği

**Durum:** Onaylandı — Blueprint v3.0 Bölüm 18.1 ile aynı içerik, kaynak referans burasıdır

## Tablolar

```
USERS
- id PK, full_name, email, status, role, password_hash (bkz. adr-016-mvp-authentication.md)

PATIENTS
- id PK, user_id FK, date_of_birth

DOCTORS
- id PK, user_id FK, specialty, license_number, verification_status

CASE_MEMBERS
- id PK, case_id FK, user_id FK, role

CASES
- id PK, patient_id FK, status, risk_level

MEDICAL_DOCUMENTS
- id PK, case_id FK, document_type, status
```

## İlişkiler

- `USERS 1:1 PATIENTS`, `USERS 1:1 DOCTORS` — bir kullanıcı hem hasta hem doktor
  olamaz (ayrı hesaplar gerekir).
- `PATIENTS 1:N CASES` — bir hasta birden fazla vakaya sahip olabilir.
- `CASES 1:N CASE_MEMBERS` — caregiver'lar bu tablo üzerinden bir case'e bağlanır.
- `CASES 1:N MEDICAL_DOCUMENTS`

## Tasarım Notları

- `CASE_MEMBERS.role` alanı `PermissionLevel` ile birlikte, caregiver'ın "belge
  silebilir mi" gibi ince yetkilerini taşır (bkz. Blueprint Bölüm 6.4 Yetki Modeli).
- `DOCTORS.verification_status`, `DoctorVerification` tablosundaki son onaylı
  kaydın özetidir — detay için `doctor-verification.md`.

## İlişkili Dosyalar

- AI/klinik taraf: `erd-ai-clinical.md`
- Case yapısı: `case-aggregate-root.md`
