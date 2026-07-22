# Learning Loop

**Durum:** Onaylandı (kavramsal) — teknik uygulama Post-MVP

## Döngü

```
AI önerdi
   │
   ▼
Doktor düzeltti  (AIAnalysisReviewed: Corrected)
   │
   ▼
AI öğrendi
   │
   ▼
Confidence arttı
   │
   └──→ (başa dön)
```

## Her Adımın Karşılığı

| Adım | Teknik Karşılık |
|---|---|
| AI önerdi | `AIAnalysisCompleted` event'i, `DifferentialDiagnosis` üretimi |
| Doktor düzeltti | `AIAnalysisReviewed` event'i, `decision: Corrected`, `correctionNotes` |
| AI öğrendi | `ReviewerProfile.CorrectionRate` güncellenir; düzeltme verisi model/prompt iyileştirme pipeline'ına girer |
| Confidence arttı | Branş bazlı confidence eşiği kalibrasyonu (bkz. `confidence-management.md`) |

## Neden "AI Öğrendi" Adımı Belirsiz Bırakılmadı

Bu döngünün en kritik noktası "AI öğrendi" adımının ne anlama geldiğidir. İki farklı
mekanizma mümkündür ve bunlar birbirine karıştırılmamalıdır:

1. **Prompt/context iyileştirme** — Doktor düzeltmeleri toplanıp belirli
   pattern'ler (örn. "bu branşta AI sistematik olarak X'i atlıyor") tespit
   edildiğinde, Hızır'ın prompt yapısı (`hizir-personality.md` ve
   `ai-orchestration-flow.md`'deki Reasoning katmanı) güncellenir. Bu, MVP
   sonrası ilk aşamada kullanılacak yöntemdir çünkü hızlı ve düşük risklidir.

2. **Model fine-tuning** — Doktor düzeltmeleri bir eğitim veri seti haline
   getirilip modelin kendisi yeniden eğitilir. Bu çok daha yüksek maliyetli ve
   regülasyon açısından daha hassas bir yoldur (model versiyonlama ve klinik
   doğrulama gerektirir); MVP sonrası ikinci aşamadır.

Bu ayrım netleştirilmeden "AI öğrenir" demek yanıltıcıdır — bu doküman bilinçli
olarak hangi mekanizmanın ne zaman kullanılacağını ayırıyor.

## Confidence Artışının Sınırı

Confidence eşiğinin kalibrasyonu, **branş bazlı** olmalıdır, genel bir "AI artık
daha iyi" iddiası olmamalıdır. Bir branşta doktorlar sistematik düzeltme
yapıyorsa, sadece o branş için eşik ayarlanır — diğer branşların confidence
davranışı etkilenmez.

## İlişkili Dosyalar

- ReviewerProfile: `../domain/reviewer-profile.md`
- Confidence yönetimi: `confidence-management.md`
- Doktor deneyimi bağlamı: `../frontend/doctor-dashboard.md`
