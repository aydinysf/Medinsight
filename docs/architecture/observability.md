# Observability

**Durum:** Yeni — v3.0 sonrası genişletme (CTO madde 11)

## Amaç

Sistemin çalışma zamanı davranışını (ne kadar yavaş, nerede hata veriyor, hangi
event ne zaman tetiklendi) dışarıdan gözlemlenebilir kılmak. Bu, özellikle
event-driven mimaride kritiktir — bir isteğin sonucu birden fazla asenkron
adımdan geçtiği için (bkz. `api-design.md` senkron/asenkron ayrımı), hata ayıklama
geleneksel senkron sistemlere göre daha zordur.

## Üç Sinyal

| Sinyal | Ne İzlenir | Araç |
|---|---|---|
| Logging | Yapılandırılmış (structured) log — her log satırı `correlationId` taşır | OpenTelemetry uyumlu logger |
| Tracing | Bir isteğin API'den event handler'lara kadar tüm yolculuğu | OpenTelemetry distributed tracing |
| Metrics | Endpoint gecikme süreleri, kuyruk derinliği, hata oranları | OpenTelemetry metrics + Prometheus/Grafana benzeri panel |

## Correlation ve Causation

`../domain/domain-events-catalog.md`'deki event zarfı (`correlationId`,
`causationId`) doğrudan tracing altyapısıyla eşlenir: bir `AIAnalysisCompleted`
event'inin hangi `DocumentUploaded` isteğinden kaynaklandığı, aynı
`correlationId` ile tek bir trace üzerinde görünür olmalıdır.

## Kritik İzleme Noktaları

- **Confidence eşiği altında kalan analiz oranı** — branş bazlı kalibrasyonun
  (bkz. `../ai/confidence-management.md`) ne zaman gerekli olduğunu gösterir.
- **Doktor review kuyruğu bekleme süresi** — SLA ihlali erken tespiti.
- **Belge kalite kontrolü red oranı** — Document Quality Engine'in eşiklerinin
  çok katı/gevşek olduğunu gösterebilir.
- **Guardrails ihlali denemesi sayısı** — bkz. `../ai/guardrails-and-boundaries.md`.

## Alarm Eşikleri (Taslak)

| Metrik | Uyarı Eşiği |
|---|---|
| API p95 gecikme | > 500ms |
| Doktor review kuyruğu bekleme | > 2 saat (yüksek risk vakalar için > 30 dk) |
| Event işleme hata oranı | > %1 |

## İlişkili Dosyalar

- CI/CD entegrasyonu: `../backend/ci-cd.md`
- Ölçeklenebilirlik: `scalability-and-caching.md`
