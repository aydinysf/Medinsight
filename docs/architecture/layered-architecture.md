# Layered Architecture

**Durum:** Onaylandı

## Katmanlar

```
Web / Mobile Client
        │
        ▼
       API
        │
        ▼
   Application
        │
        ▼
     Domain
        │
        ▼
  Infrastructure
        │
        ▼
 AI Orchestration
```

## Teknoloji Kararı

.NET 8, Clean Architecture ve Domain Driven Design prensipleriyle kurulur.
PostgreSQL, event-driven domain events, object storage (belge depolama için).

## Katman Sorumlulukları

| Katman | Sorumluluk | Bağımlılık Yönü |
|---|---|---|
| API | HTTP endpoint'leri, auth, request/response şekillendirme | Application'a bağımlı |
| Application | Use case'ler (command/query handler'lar), orchestration | Domain'e bağımlı |
| Domain | Aggregate'ler, domain event'ler, iş kuralları | Hiçbir katmana bağımlı değil |
| Infrastructure | PostgreSQL erişimi, object storage, dış servis entegrasyonları | Domain'e bağımlı (interface üzerinden) |
| AI Orchestration | Hızır'ın çalıştığı izole katman (bkz. `../ai/ai-orchestration-flow.md`) | Domain'e bağımlı, ama Domain ona bağımlı değil |

## Neden AI Orchestration Ayrı Bir Katman

Bu, mimarideki en kritik izolasyon kararıdır: AI sağlayıcı (bugün bir LLM API'si,
yarın başka bir model) değişse bile Domain katmanı etkilenmemelidir. AI
Orchestration, Domain'in ürettiği event'leri dinler ve kendi sonuçlarını yine
Domain'e (event olarak) geri verir — asla Domain'in iç yapısına doğrudan erişmez.

## Bağımlılık Kuralı

Domain katmanı hiçbir dış pakete (ORM, HTTP client, AI SDK) bağımlı olamaz. Bu
kural, Domain'in birim testlerinin altyapı olmadan çalışabilmesini garanti eder.

## İlişkili Dosyalar

- Bounded context haritası: `bounded-contexts-overview.md`
- AI izolasyonu detayı: `../ai/ai-orchestration-flow.md`
- Case aggregate: `../domain/case-aggregate-root.md`
