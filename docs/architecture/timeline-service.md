# Timeline Service

**Durum:** Onaylandı
**İlgili ADR:** [adr-006-timeline-as-separate-bounded-context.md](../adr/adr-006-timeline-as-separate-bounded-context.md)

## Sorumluluk

Case Engine, Health Route Engine, AI Analysis ve Consultation'daki her olayı
append-only biçimde toplamak ve zaman sıralı sunmak. Timeline **hiçbir olay
üretmez**, sadece dinler.

## Mimari Konum

```
Case Engine ──┐
Health Route ─┼──→ [domain events] ──→ Timeline Service ──→ Case Timeline (read model)
AI Analysis ──┤
Consultation ─┘
```

Timeline Service, `../domain/domain-events-catalog.md`'deki event zarfını
(`eventId`, `eventType`, `occurredAt`, `caseId`, `causationId`, `correlationId`)
dinleyen pasif bir subscriber'dır.

## Event Sourcing'e Yakınlık

Timeline Service, klasik bir event store değildir (sistemin state'i event'lerden
yeniden inşa edilmez) ama event sourcing'in okuma tarafına çok yakındır: her satır
değiştirilemez, sıralıdır ve kaynağını gösterir. Bu yakınlık bilinçlidir — ileride
tam bir event store'a geçiş gerekirse (örn. regülasyon "tüm sistem durumunun
yeniden oluşturulabilir olması" talep ederse), Timeline Service'in şeması buna
hazır bir başlangıç noktasıdır.

## Şema

```
TimelineEntry
- id PK
- case_id FK
- event_type
- occurred_at
- summary            (insan okunabilir kısa özet, örn. "MR yüklendi")
- source_event_id     (domain-events-catalog.md'deki orijinal event'e referans)
- actor_user_id        (nullable — sistem tetiklemesi olabilir)
```

## Sorgu Deseni

- **"Bu case'in timeline'ı"** → `case_id` + `occurred_at DESC` ile sayfalama.
- **"Şu tarihten sonraki olaylar"** → `occurred_at > X` filtresi.

`(case_id, occurred_at)` composite index zorunludur — bu, `health-route-versioning.md`
içindeki snapshot sorgu desenine benzer bir gereksinimdir.

## Neden Ayrı Bir Servis

Detaylı gerekçe `../adr/adr-006-timeline-as-separate-bounded-context.md`'de.
Özet: sorgu deseni diğer motorlardan temelde farklı (karar üretmez, sadece
geçmişi gösterir) ve bağımsız ölçeklendirme ihtiyacı olabilir.

## İlişkili Dosyalar

- Event akışı: `../domain/event-storming.md`
- Event kataloğu: `../domain/domain-events-catalog.md`
- ADR: `../adr/adr-006-timeline-as-separate-bounded-context.md`
