# Database Migrations

**Durum:** Onaylandı

## Kural

Her migration, hem `up` hem `down` script'ine sahip olmalıdır (bkz. `ci-cd.md`
migration stratejisi). Bir migration production'a çıktıktan sonra asla
düzenlenmez — hatalı bir migration için yeni bir düzeltme migration'ı yazılır.

## Append-Only Tablolar İçin Özel Dikkat

`HealthRouteSnapshot`, `TimelineEntry` ve `AuditLog` gibi append-only tablolarda
(bkz. `../domain/health-route-versioning.md`, `../architecture/timeline-service.md`,
`../architecture/audit-service.md`) migration'lar özellikle dikkatli olmalıdır —
bu tablolarda veri kaybına yol açabilecek bir `down` migration'ı (örn. kolon
silme) production'da çalıştırılmadan önce mutlaka veri yedeği alınmalıdır.

## Şema Değişikliği ile Domain Dokümanı İlişkisi

Bir migration, `docs/domain/erd-identity-case.md` veya `erd-ai-clinical.md`'de
tanımlı bir şemayı değiştiriyorsa, bu dosyalar aynı PR içinde güncellenmelidir
(bkz. `git-strategy.md` "Domain Değişikliği Kuralı").

## İlişkili Dosyalar

- ERD referansları: `../domain/erd-identity-case.md`, `../domain/erd-ai-clinical.md`
- CI/CD: `ci-cd.md`
