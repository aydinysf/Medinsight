# AI Agent Ecosystem

**Durum:** Post-MVP tasarım — mimari bugünden buna izin verecek şekilde kuruluyor

## Bugün vs. Gelecek

**Bugün (MVP):** Hızır tek bir genel amaçlı ajan. Tüm belge türlerini, tüm analiz
taleplerini kendisi işler. **İstisna:** görüntü analizi için MVP'de sınırlı,
bilgilendirici bir "Radiology AI ön hali" zaten mevcuttur — açık kaynak, klinik
olarak doğrulanmamış bir modelin (bkz. `../architecture/radiology-inference-service.md`)
sadece ek bilgi olarak sunulduğu, karar dayanağı olmayan bir katman
(bkz. `../adr/adr-010-open-source-radiology-model-mvp.md`). Bu, aşağıdaki
"gelecek" tablosundaki klinik doğrulanmış Radiology AI'dan farklıdır.

**Gelecek:** Hızır bir **orchestrator**'a evrilir — kendisi analiz üretmez, hangi
uzman ajana yönlendireceğine karar verir.

```
                        Hızır (orchestrator)
                               │
        ┌──────────┬──────────┼──────────┬──────────┐
        │           │          │          │          │
   Document AI  Radiology AI  Pathology  Laboratory  Pharmacology
                                  AI          AI          AI
        │           │          │          │          │
        └──────────┴──────────┼──────────┴──────────┘
                               │
                    Conversation AI / Doctor Matching AI
```

## Uzman Ajanların Sorumlulukları

| Ajan | Sorumluluk |
|---|---|
| Document AI | Genel belge sınıflandırma ve metin çıkarımı |
| Radiology AI | MR/BT/DICOM görüntü analizi *(MVP'de bkz. Radiology Inference Service — sınırlı, klinik doğrulaması yok)* |
| Pathology AI | Patoloji raporu yorumlama |
| Laboratory AI | Kan/tahlil sonuçlarının yorumlanması |
| Pharmacology AI | İlaç etkileşimi ve doz kontrolü (bilgilendirme amaçlı, reçete değil) |
| Conversation AI | Hastayla doğal dil etkileşimi (Hızır'ın "ses"i) |
| Doctor Matching AI | Bugünkü skorlama motorunun (bkz. `../domain/doctor-matching-engine.md`) öğrenen versiyonu |

## Orchestrator'ın Rolü Nasıl Değişir

Bugün Hızır'ın `ai-orchestration-flow.md`'deki "Agent Selection" adımı her zaman
"Hızır'ın kendisi"ni seçer. Ekosistem devreye girdiğinde, bu adım gerçek bir
yönlendirme kararı haline gelir: örneğin bir DICOM dosyası yüklendiğinde Planner,
Radiology AI'ı seçer; patoloji raporu geldiğinde Pathology AI'ı seçer.

## Ortak Bilgi Kaynağı

Tüm uzman ajanlar `medical-knowledge-graph.md`'deki aynı bilgi kaynağını paylaşır.
Bu paylaşım olmadan her ajan kendi izole bilgisiyle çalışır ve tutarsız sonuçlar
üretebilir (örn. Radiology AI ile Pathology AI aynı vakada çelişen evreleme önerisi
verebilir).

## Neden Bugün Değil

Çoklu ajan mimarisi, her ajanın kendi doğrulama/güvenlik katmanını gerektirir ve
MVP'nin karmaşıklık bütçesini aşar (bkz. `../adr/adr-008-mvp-scope-exclusions.md`).
Ancak `ai-orchestration-flow.md`'deki katmanlı tasarım, bu geçişi mimari refactor
değil, genişleme haline getirir.

## İlişkili Dosyalar

- Orchestration akışı: `ai-orchestration-flow.md`
- Bilgi kaynağı: `medical-knowledge-graph.md`
- MVP kapsam kararı: `../adr/adr-008-mvp-scope-exclusions.md`
