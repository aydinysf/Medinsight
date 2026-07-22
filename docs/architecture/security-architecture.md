# Security Architecture

**Durum:** Onaylandı

## Katmanlı Savunma

```
Ağ katmanı           — TLS, WAF, rate limiting
  └─ Kimlik katmanı     — JWT, RBAC, kaynak bazlı yetki
       └─ Uygulama katmanı  — Guardrails, idempotency, audit log
            └─ Veri katmanı    — at-rest şifreleme, KVKK sınıflandırma
```

## Kimlik Doğrulama ve Yetkilendirme

JWT bearer token, claim'lerde `userId` ve `role` taşır. Yetkilendirme iki
katmanlıdır:

1. **Rol bazlı** — `../domain/case-aggregate-root.md` ve Blueprint v3.0 Bölüm 6.4'teki
   yetki tablosuna göre (Patient/Caregiver/Doctor/Admin).
2. **Kaynak bazlı** — bir doktor sadece kendi `Consultation`'larına erişebilir;
   rol doğru olsa bile "başka birinin vakasına erişim" senaryosu bu katmanda
   engellenir.

## Veri Koruma

| Katman | Uygulama |
|---|---|
| At-rest | Mesaj içeriği, klinik not, tedavi planı column-level şifrelenir |
| In-transit | TLS 1.2+ zorunlu, iç servisler arası dahil |
| Veri sınıflandırması | KVKK'nın özel nitelikli kişisel veri tanımına giren sağlık verisi (MR, laboratuvar, tanı notu) ayrı hassasiyet seviyesinde işlenir |

## AI Çağrılarına Özel Güvenlik

Bkz. `../ai/guardrails-and-boundaries.md` — prompt injection savunması ve PII
minimizasyonu bu dosyada detaylandırılmıştır.

## Doktor Kimlik Güvenliği

QR/diploma sahteciliği riski, otomatik onay olmaması ve belge+QR çapraz
kontrolüyle azaltılır (bkz. `../domain/doctor-verification.md` ve
`../adr/adr-007-qr-based-doctor-verification.md`).

## Secrets Yönetimi

Uygulama secret'ları (DB connection string, AI API anahtarları) kod
repository'sinde asla saklanmaz; ortam bazlı bir secrets manager (örn. Azure Key
Vault benzeri bir çözüm) üzerinden enjekte edilir. Bu, `../backend/ci-cd.md`
pipeline'ının bir parçasıdır.

## İlişkili Dosyalar

- Audit: `audit-service.md`
- AI güvenliği: `../ai/guardrails-and-boundaries.md`
- Rate limiting: `rate-limiting-idempotency.md`
