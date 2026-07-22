# Coding Standards

**Durum:** Onaylandı

## Genel İlkeler

- **Domain katmanı saf kalır** — hiçbir dış pakete (ORM, HTTP client) bağımlılık
  yok (bkz. `../architecture/layered-architecture.md` bağımlılık kuralı).
- **Command/Query ayrımı (CQRS)** — her use case ya bir command handler ya bir
  query handler'dır, ikisi karışmaz.
- **Immutability tercih edilir** — domain event'ler ve snapshot'lar (bkz.
  `../domain/health-route-versioning.md`) her zaman immutable kayıtlardır.

## Kod İnceleme Kriterleri

1. Yeni bir domain kuralı ekleniyorsa, önce ilgili `docs/domain/` dosyası
   güncellenmiş mi? (bkz. kök `README.md` disiplini)
2. Yeni bir mimari karar varsa, bir ADR yazıldı mı?
3. Domain katmanına dış bağımlılık sızmış mı?
4. Her yeni endpoint `../architecture/security-architecture.md`'deki yetki
   kontrolüne uyuyor mu?

## Hata Yönetimi

Detaylar `error-handling.md`'de. Özet ilke: domain hataları (business rule
ihlali) ile teknik hatalar (DB bağlantı hatası) farklı exception hiyerarşilerinde
tutulur ve API katmanında farklı HTTP status kodlarına eşlenir.

## Test Beklentisi

- Domain katmanı: %90+ birim test kapsamı hedeflenir (dış bağımlılık olmadığı
  için bu maliyetsizdir).
- Application katmanı: her command/query handler için en az bir test.
- Entegrasyon testleri: her bounded context'in event subscriber'ları için.

## İlişkili Dosyalar

- Naming: `naming-conventions.md`
- Klasör yapısı: `folder-structure.md`
- Hata yönetimi: `error-handling.md`
