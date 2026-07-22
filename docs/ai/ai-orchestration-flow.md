# AI Orchestration Flow

**Durum:** Onaylandı
**Amaç:** Hızır'ın bir kullanıcı mesajını aldıktan sonra izlediği teknik akışı adım
adım tanımlamak.

## Akış

```
Patient Input (metin, ses, belge)
    │
    ▼
1. Intent Detection
    │  "Bu mesaj ne istiyor?" — belge yorumlama, genel soru, durum sorgusu,
    │  acil durum sinyali, vs.
    ▼
2. Planner
    │  Intent'e göre hangi adımların gerektiğini belirler.
    │  Örnek: "MR yorumla" intent'i → [kalite kontrolü, AI analiz, doktor eşleştirme]
    │  planını üretir.
    ▼
3. Agent Selection
    │  Planın hangi adımı hangi uzman ajana (bkz. ai-agent-ecosystem.md) gideceğine
    │  karar verir. MVP'de tek ajan (Hızır'ın kendisi); Post-MVP'de Radiology AI,
    │  Pathology AI gibi uzman ajanlara yönlendirme burada gerçekleşir.
    ▼
4. Tool Calling
    │  Seçilen ajan, ihtiyaç duyduğu araçları çağırır: Document Quality Engine,
    │  Doctor Matching Engine, Medical Knowledge Graph sorgusu, vb.
    ▼
5. Memory
    │  Context Engine, vakanın geçmişini (önceki analizler, doktor notları,
    │  hastanın daha önce belirttiği tercihler) sorguya dahil eder.
    ▼
6. Reasoning
    │  Toplanan veri + memory + tool sonuçları birleştirilip bir yanıt taslağı
    │  üretilir. Bu adım Guardrails katmanından geçer (bkz.
    │  guardrails-and-boundaries.md) — kapsam kontrolü burada uygulanır.
    ▼
7. Response
    │  Karakter katmanından geçirilir (bkz. hizir-personality.md) — ton, dil,
    │  risk iletişimi kuralları burada uygulanır.
    ▼
Patient'a yanıt
```

## Katmanlar Arası Sorumluluk Ayrımı

| Katman | Sorumluluk | Sorumlu OLMADIĞI |
|---|---|---|
| Intent Detection | Ne istendiğini anlamak | Nasıl cevaplanacağına karar vermek |
| Planner | Adım sırası belirlemek | Adımları çalıştırmak |
| Agent Selection | Doğru uzmanı seçmek | Analiz üretmek |
| Tool Calling | Dış sistemlere erişim | Sonucu yorumlamak |
| Memory | Geçmiş bağlamı sağlamak | Karar vermek |
| Reasoning | Yanıt taslağı üretmek | Karakteri uygulamak |
| Response | Karakteri uygulamak | İçeriği değiştirmek |

Bu ayrım önemli çünkü her katman bağımsız test edilebilir olmalı — örneğin
Reasoning katmanı yanlış bir tıbbi çıkarım yapsa bile, Response katmanı bunu asla
"kesin" bir dille sunmaz (Guardrails + Personality çift kontrolü).

## Neden Bu Kadar Katmanlı

MVP'de Hızır tek bir genel amaçlı model çağrısı gibi görünebilir, ama bu yedi
adımın hepsi mevcuttur — sadece Agent Selection adımı MVP'de her zaman "Hızır'ın
kendisi"ni seçer. Bu tasarım, `ai-agent-ecosystem.md`'deki çoklu ajan geleceğine
geçişte Orchestrator'ın (Planner + Agent Selection) yeniden yazılmasını değil,
sadece genişletilmesini gerektirir.

## İlişkili Dosyalar

- Uzman ajan geleceği: `ai-agent-ecosystem.md`
- Guardrails detayları: `guardrails-and-boundaries.md`
- Karakter uygulaması: `hizir-personality.md`
- Confidence yönetimi: `confidence-management.md`
