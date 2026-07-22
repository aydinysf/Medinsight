# CI/CD

**Durum:** Onaylandı

## Pipeline Aşamaları

```
Commit / PR
    │
    ▼
1. Build            (derleme hatası varsa dur)
    │
    ▼
2. Unit Tests        (Domain + Application katmanları)
    │
    ▼
3. Integration Tests  (bounded context event subscriber'ları)
    │
    ▼
4. Static Analysis    (lint, güvenlik taraması — secrets sızıntısı kontrolü)
    │
    ▼
5. Deploy to Staging   (main branch'e merge sonrası otomatik)
    │
    ▼
6. Smoke Tests
    │
    ▼
7. Deploy to Production (manuel onay + feature flag kontrolü)
```

## Neden Production Deploy Manuel Onaylı

Sağlık verisi taşıyan bir sistemde tam otomatik production deploy riski, hız
kazancını aşar. Manuel onay adımı, `../architecture/observability.md`'deki
metriklerin staging'de gözden geçirilmesini garanti eder.

## Secrets Yönetimi

CI/CD pipeline'ı, secret'ları (`../architecture/security-architecture.md`'de
belirtildiği gibi) asla kod içine gömmez; pipeline'ın kendisi bir secrets
manager'dan çalışma zamanında çeker.

## Migration Stratejisi

Veritabanı migration'ları (`database-migrations.md`) pipeline'ın ayrı bir
aşamasıdır ve **geri alınabilir** olmalıdır — her migration'ın bir `down` script'i
zorunludur.

## İlişkili Dosyalar

- Git stratejisi: `git-strategy.md`
- Feature flags: `feature-flags.md`
- Observability: `../architecture/observability.md`
