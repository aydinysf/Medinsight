# Bounded Contexts Overview

**Durum:** Onaylandı — sistemin tüm bounded context'lerinin tek referans haritası

## Kullanıcı Yüzeyleri

| Yüzey | Sorumluluk | Sahip OLMADIĞI |
|---|---|---|
| Hasta / caregiver app | Vaka oluşturma, belge yükleme, Hızır ile konuşma | Klinik karar, tedavi planı |
| Doktor dashboard | Öncelik kuyruğu, konsültasyon, klinik not | Vaka açma/kapama (idari) |
| Admin panel | Doktor doğrulama, sistem izleme | Klinik karar |

## Çekirdek Motorlar (Bounded Context'ler)

| Motor | Sorumluluk | Sahip OLMADIĞI | Referans |
|---|---|---|---|
| Case Engine | Case yaşam döngüsü, state machine | Belge kalitesi, AI çıktısı | `../domain/case-lifecycle-state-machine.md` |
| Document Quality Engine | Dosya bazlı kalite skoru | Belgenin klinik anlamı | `../domain/document-quality-engine.md` |
| Hızır — AI Orchestrator | Context toplama, guardrails, AI çağrısı | Tanı, tedavi kararı | `../ai/ai-orchestration-flow.md` |
| Health Route Engine | Snapshot üretimi, current read model | Snapshot'ı üreten kararın doğruluğu | `../domain/health-route-versioning.md` |
| Doctor Matching Engine | Skorlama, öneri sıralama | Doktor atama | `../domain/doctor-matching-engine.md` |
| Identity & Verification | Doktor QR/diploma doğrulama | Doktorun klinik yetkinliği | `../domain/doctor-verification.md` |
| Timeline Engine | Event'leri append-only kayda çevirme | Hiçbir iş kararı | `timeline-service.md` |
| Notification Engine | Kanal seçimi, iletim | Bildirimin içeriği | `notification-engine.md` |
| Audit Service | Append-only aksiyon kaydı | Hiçbir iş kararı | `audit-service.md` |

## Bounded Context'ler Arası İletişim Kuralı

Hiçbir bounded context, bir diğerinin veritabanı şemasına doğrudan erişemez.
Tüm iletişim domain event'leri üzerinden asenkron olarak yürür (bkz.
`../domain/domain-events-catalog.md`), tek istisna: senkron sorgular (örn. Case
Engine'in bir Case'in var olup olmadığını kontrol etmesi) doğrudan bir API
çağrısı üzerinden yapılabilir, ama asla yazma işlemi için değil.

## Neden Bu Ayrım

Her motorun "sahip OLMADIĞI" sütunu kadar önemlidir — bu, bir motorun
sorumluluğunu zamanla genişletip diğerinin alanına taşmasını (scope creep)
önler. Örnek: Doctor Matching Engine'in "atama yapmama" sınırı, regülasyon
riskini azaltan bilinçli bir tasarım kararıdır (bkz.
`../adr/adr-003-doctor-matching-scoring-model.md`).

## İlişkili Dosyalar

- Katman mimarisi: `layered-architecture.md`
- Case aggregate: `../domain/case-aggregate-root.md`
