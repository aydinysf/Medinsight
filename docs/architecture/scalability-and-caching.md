# Scalability ve Caching

**Durum:** Yeni — v3.0 sonrası genişletme (CTO madde 11)

## Ölçeklenebilirlik Yaklaşımı

Sistem, bounded context'ler arasında zaten gevşek bağlı (loosely coupled)
olduğu için (bkz. `bounded-contexts-overview.md`), her motor bağımsız olarak
yatay ölçeklendirilebilir. Öncelikli ölçeklenme adayları:

| Motor | Neden Öncelikli |
|---|---|
| Document Quality Engine | 800+ dosya toplu yüklemede en yoğun işlem hacmi |
| AI Analysis / Hızır | Model çağrıları doğası gereği yavaş, kuyruk birikebilir |
| Timeline Service | Append-only yazma yoğun, okuma da sık (her case açılışında) |

## Caching Stratejisi

| Veri | Cache Türü | TTL / Invalidation |
|---|---|---|
| Doctor Matching sonucu | Kısa süreli (case bazlı) | Yeni `AIFindings` geldiğinde invalidate |
| ReviewerProfile | Orta süreli | Her `AIAnalysisReviewed` event'inde invalidate |
| HealthRoute (current) | Kısa süreli read-through cache | Her yeni snapshot'ta invalidate |
| Medical Knowledge Graph (Post-MVP) | Uzun süreli | Sadece editöryel güncellemede invalidate |

Cache invalidation her zaman event tetiklemeli (bkz.
`../domain/domain-events-catalog.md`), zamanlanmış (TTL-only) invalidation'a
güvenilmemelidir — çünkü sağlık verisinde bayat veri göstermek (örn. eski bir
Health Route durumu) kabul edilemez.

## Performans Öncelikleri

1. **800+ dosya toplu yükleme** — chunked/resumable upload, arka planda
   queue-based processing (bkz. Blueprint v3.0 Bölüm 15.4).
2. **Doktor dashboard öncelik kuyruğu** — sık okunan, nadiren yazılan bir read
   model; agresif cache'lenebilir.
3. **AI analiz kuyruğu** — confidence eşiği altında kalan analizler öncelikli
   işlenmeli (bkz. `../ai/confidence-management.md`).

## Yatay Ölçekleme Sınırları

`HealthRouteSnapshot` ve `TimelineEntry` gibi append-only tablolar zamanla
büyüyecektir (bkz. Blueprint v3.0 Bölüm 23, Technical Debt). Bu tablolar için
partition stratejisi (örn. case_id bazlı veya tarih bazlı) ölçeklenme planının
bir parçası olmalıdır, ancak henüz uygulanmamıştır.

## İlişkili Dosyalar

- Observability: `observability.md`
- Health Route şeması: `../domain/health-route-versioning.md`
- Timeline şeması: `timeline-service.md`
