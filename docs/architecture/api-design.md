# API Design

**Durum:** Onaylandı

## Stil Kararları

- REST, resource-oriented, `/api/v1` prefix'i ile versiyonlanmış.
- Auth: JWT bearer token (bkz. `security-architecture.md`).
- Yazma işlemleri senkron + asenkron ayrımı: HTTP response sadece "kabul edildi"
  bilgisini döner (`202 Accepted`); ağır işler (kalite kontrolü, AI analiz) event
  ile arka planda yürür.
- Gerçek zamanlı mesajlaşma SignalR hub üzerinden yürür; REST sadece geçmiş
  sorgu ve fallback içindir.

## Senkron / Asenkron Ayrımı

```
İstemci isteği
      │
      ▼
API — senkron: 202 Accepted döner
      │
      ▼
Event yayınlanır
      │
   ┌──┼──┐
   ▼  ▼  ▼
Quality  AI Analysis  Notification
(asenkron, birbirinden bağımsız)
```

## Ana Endpoint Grupları

| Kaynak | Örnek Endpoint | Detay |
|---|---|---|
| Case | `POST /cases`, `GET /cases/{id}/timeline` | `../domain/case-aggregate-root.md` |
| Document | `POST /cases/{caseId}/documents` (toplu, resumable) | `../domain/document-quality-engine.md` |
| AIAnalysis | `GET /cases/{caseId}/analyses/{id}` | `../ai/confidence-management.md` |
| HealthRoute | `GET /cases/{caseId}/health-route/snapshots` | `../domain/health-route-versioning.md` |
| DoctorMatching | `GET /cases/{caseId}/doctor-matches` | `../domain/doctor-matching-engine.md` |
| Consultation | `POST /consultations/{id}/treatment-plan` | `../domain/consultation-model.md` |
| DoctorVerification | `POST /admin/doctor-verifications/{id}/approve` | `../domain/doctor-verification.md` |

Tam endpoint listesi: `../api/endpoints-overview.md`

## Neden 202 Accepted

İstemciye "kabul edildi, işleniyor" der — belge gerçekten kalite kontrolünden
geçmiş ve analiz edilmiş olmasını beklemeden. İstemci sonucu SignalR üzerinden
veya `GET /health-route` polling ile öğrenir. Bu, 800+ dosyalık toplu yüklemede
API'nin yanıt hızını korumak için kritiktir.

## İlişkili Dosyalar

- Event contract'ları: `../api/event-contracts.md`
- Rate limiting: `rate-limiting-idempotency.md`
- Güvenlik: `security-architecture.md`
