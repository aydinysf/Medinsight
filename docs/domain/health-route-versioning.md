# Health Route Versioning — Git Modeli

**Bounded Context:** Health Route Engine
**Durum:** Onaylandı — v3.0 itibarıyla Git benzeri modele genişletildi
**İlgili ADR:** [adr-002-health-route-snapshot-versioning.md](../adr/adr-002-health-route-snapshot-versioning.md)

## Neden "Git Modeli"

HealthRoute, hastanın o anki sağlık rotasını temsil eden **current state** ile bu
duruma nasıl gelindiğini gösteren **değiştirilemez geçmiş**'ten oluşur. Bu, Git'in
"working directory + commit history" ayrımına doğrudan benzer:

```
HealthRoute (working directory / current state)
   ↑
   │ HEAD işaret eder
   │
Version 4 ← Patient decision   (en son commit)
   ↑
Version 3 ← AI changed
   ↑
Version 2 ← Doctor changed
   ↑
Version 1 ← Initial (Case açıldığında)
```

Her versiyon bir "commit" gibi düşünülebilir: kim değiştirdi (`TriggeredBy`), neden
değiştirdi (`Reason`), ne zaman (`CreatedAt`) ve önceki versiyona referans
(`PreviousVersionId`).

## Şema

```
HealthRoute (read model, "HEAD")
- CaseId
- CurrentVersionId  → en son HealthRouteSnapshot'a işaret eder
- CurrentStatus
- NextStep
- RiskLevel

HealthRouteSnapshot (append-only, "commit")
- Id
- CaseId
- PreviousVersionId   (nullable — ilk versiyon için null)
- VersionNumber        (1, 2, 3, ...)
- Status, NextStep, RiskLevel
- TriggeredBy           (AI | Doctor | Patient | System)
- TriggerSourceId       (hangi AIFindings, Consultation veya Treatment'tan geldi)
- Reason
- CreatedAt
```

## "Branch" Senaryosu Var mı?

Git'te branch, paralel gerçekliklerin bir arada var olabilmesini sağlar. Sağlık
rotasında bunun karşılığı şudur: bir doktor tedavi planı önerirken, aynı anda AI
farklı bir analiz üretebilir. Bu durumda **branch açılmaz** — bunun yerine her ikisi
de ayrı `HealthRouteSnapshot` kayıtları olarak birbirini takip eder, ve nihai
`CurrentVersionId` hangisinin "kazandığını" (genellikle doktor kararının AI önerisini
geçersiz kıldığını) gösterir. Sağlık verisinde çatallanmış gerçeklik kabul edilemez —
her zaman tek bir doğrusal, denetlenebilir geçmiş olmalıdır.

## Neden Bu Model Gerekli

Önceki tasarımda HealthRoute tek satırlık mutable bir kayıttı. Bu üç sorunu çözemiyordu:

1. **Denetlenebilirlik**: KVKK ve tıbbi sorumluluk açısından "bu karar ne zaman,
   kim tarafından değiştirildi" sorusu her zaman cevaplanabilir olmalı.
2. **Geri alma**: Bir doktor kararının hatalı olduğu sonradan anlaşılırsa, önceki
   versiyona referans vererek düzeltme yapılabilmeli (üzerine yazmadan).
3. **Timeline entegrasyonu**: Her snapshot doğrudan Timeline'a bir olay olarak
   düşer; mutable bir kayıt bunu event-sourcing uyumlu şekilde üretemez.

## Sorgu Deseni

- **"Şu an rota ne"** → `HealthRoute.CurrentVersionId` üzerinden tek okuma.
- **"Nasıl buraya geldik"** → `HealthRouteSnapshot` zincirini `PreviousVersionId`
  üzerinden geriye doğru yürü.
- **"Doktor kaç kez değişiklik yaptı"** → `TriggeredBy = Doctor` filtresiyle sayım.

## İlişkili Dosyalar

- Case içindeki konumu: `case-aggregate-root.md`
- Event kataloğu: `domain-events-catalog.md` (`HealthRouteSnapshotCreated`)
- ADR: `../adr/adr-002-health-route-snapshot-versioning.md`
