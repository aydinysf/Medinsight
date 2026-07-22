# Tech Stack

**Durum:** Onaylandı

## Backend

- **.NET 8** — Clean Architecture, Domain Driven Design
- **PostgreSQL** — birincil veritabanı
- **Object Storage** — belge/DICOM depolama (S3 uyumlu)
- **SignalR** — gerçek zamanlı mesajlaşma
- **Event bus** — domain event dağıtımı (bkz. `../domain/domain-events-catalog.md`)

## AI Katmanı

- Model sağlayıcıdan bağımsız bir AI Orchestration katmanı (bkz.
  `../ai/ai-orchestration-flow.md`) — model/prompt versiyonlama zorunlu.

## Neden .NET 8

Ekibin mevcut uzmanlığı ve Clean Architecture ile DDD pratiklerinin .NET
ekosisteminde olgun tooling'e sahip olması (MediatR benzeri CQRS kütüphaneleri,
EF Core migration altyapısı) tercih sebebidir.

## Sürüm Politikası

Framework ve kritik bağımlılık sürümleri `versioning.md`'deki genel versiyonlama
kuralına tabidir — majör sürüm yükseltmeleri bir ADR gerektirir.

## İlişkili Dosyalar

- Katman mimarisi: `../architecture/layered-architecture.md`
- Klasör yapısı: `folder-structure.md`
