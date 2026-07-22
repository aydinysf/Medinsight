# Error Handling

**Durum:** Onaylandı

## Hata Kategorileri

| Kategori | Örnek | HTTP Karşılığı |
|---|---|---|
| Domain hatası (business rule ihlali) | "Closed case'e belge eklenemez" (bkz. `../domain/case-aggregate-root.md` invariant) | 409 Conflict |
| Doğrulama hatası | Eksik zorunlu alan | 400 Bad Request |
| Yetki hatası | Kaynak bazlı erişim reddi (bkz. `../architecture/security-architecture.md`) | 403 Forbidden |
| Bulunamadı | Var olmayan Case ID | 404 Not Found |
| Teknik hata | DB bağlantı kopması | 500 Internal Server Error |
| Rate limit | `../architecture/rate-limiting-idempotency.md` | 429 Too Many Requests |

## Domain Hatası ile Teknik Hatanın Ayrımı

Domain katmanı sadece domain-specific exception'lar fırlatır (örn.
`CaseAlreadyClosedException`); asla generic `Exception` veya altyapıya özgü
exception'lar (örn. EF Core exception'ları) domain katmanından dışarı sızmaz.
Bu, `coding-standards.md`'deki "domain katmanı saf kalır" ilkesinin hata
yönetimi tarafındaki karşılığıdır.

## AI Çağrısı Hataları

AI Orchestration katmanında bir model çağrısı başarısız olursa (timeout, rate
limit), bu **asla** kullanıcıya teknik hata olarak gösterilmez — Hızır'ın
karakter kurallarına uygun bir mesajla sarmalanır (bkz.
`../ai/hizir-personality.md`): "Şu an bu isteği işleyemiyorum, birazdan tekrar
dener misin?"

## Loglama

Her hata, `../architecture/observability.md`'deki `correlationId` ile loglanır.
Domain hataları `Warning` seviyesinde, teknik hatalar `Error` seviyesinde
loglanır — bu ayrım, alarm gürültüsünü azaltır (domain hataları genellikle
normal kullanıcı akışının bir parçasıdır, alarm gerektirmez).

## İlişkili Dosyalar

- Coding standards: `coding-standards.md`
- Observability: `../architecture/observability.md`
