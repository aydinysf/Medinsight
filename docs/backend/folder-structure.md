# Folder Structure

**Durum:** Onaylandı

## Çözüm Yapısı (Solution Structure)

```
MedInsight.sln
├── src/
│   ├── MedInsight.Domain/           ← saf domain, dış bağımlılık yok
│   │   ├── Cases/
│   │   ├── HealthRoutes/
│   │   ├── Consultations/
│   │   └── Events/
│   ├── MedInsight.Application/      ← command/query handler'lar
│   │   ├── Cases/Commands/
│   │   ├── Cases/Queries/
│   │   └── ...
│   ├── MedInsight.Infrastructure/   ← EF Core, object storage, dış servisler
│   ├── MedInsight.AIOrchestration/  ← Hızır ve AI çağrıları (bkz. ../ai/)
│   ├── MedInsight.Api/              ← controller'lar, middleware
│   └── MedInsight.TimelineService/  ← bağımsız deploy edilebilir (bkz.
│                                       ../architecture/timeline-service.md)
└── tests/
    ├── MedInsight.Domain.Tests/
    ├── MedInsight.Application.Tests/
    └── MedInsight.Integration.Tests/
```

## Domain Klasörü İçi Organizasyon

Her bounded context (`../architecture/bounded-contexts-overview.md`), kendi alt
klasöründe aggregate, event ve domain servisleriyle birlikte durur — teknik
katmana göre değil, iş kavramına göre gruplanır:

```
MedInsight.Domain/
├── Cases/
│   ├── Case.cs                    (aggregate root)
│   ├── CaseStatus.cs
│   ├── Events/
│   │   ├── CaseStatusChanged.cs
│   │   └── CaseClosed.cs
│   └── MedicalDocuments/
│       └── MedicalDocument.cs
```

## Neden Katman Değil Kavram Bazlı

"Tüm entity'ler `Entities/` klasöründe, tüm event'ler `Events/` klasöründe"
yaklaşımı reddedildi çünkü bir bounded context'i anlamak için geliştiricinin
birden fazla klasör arasında gezinmesini gerektirir. Kavram bazlı gruplama,
`../domain/case-aggregate-root.md` gibi dokümanlardaki yapıyla kod yapısını
birebir eşler.

## İlişkili Dosyalar

- Naming: `naming-conventions.md`
- Katman mimarisi: `../architecture/layered-architecture.md`
