# Doctor Matching Engine

**Bounded Context:** Doctor Matching Engine
**Durum:** Onaylandı
**İlgili ADR:** [adr-003-doctor-matching-scoring-model.md](../adr/adr-003-doctor-matching-scoring-model.md)

## Amaç

Bir Case için branş, konum, müsaitlik, tecrübe ve yanıt hızını birleştirerek en fazla
3-5 doktoru skorlu biçimde önermek. Bu motor **atama yapmaz** — sadece önerir; nihai
seçim hasta veya doktor tarafından yapılır.

## Skorlama Faktörleri ve Ağırlıkları

| Faktör | Göreli Ağırlık | Kaynak |
|---|---|---|
| Branş uyumu | Yüksek | `DifferentialDiagnosis` içindeki önerilen branş |
| Konum / uzaklık | Orta | Hastanın konumu (varsa) |
| Müsaitlik | Orta | `doctor-availability.md` — EffectiveStatus (Available/Busy/Away) |
| Tecrübe / vaka sayısı | Düşük | `ReviewerProfile.CaseReviewCount` |
| Ortalama yanıt hızı | Düşük | `ReviewerProfile` geçmiş performansı |

Ağırlıklar konfigürasyondan okunur. Bu, ileride Hospital rolü aktif olduğunda
"kurumsal öncelik" gibi yeni bir faktörün kod değişikliği gerektirmeden eklenmesini
sağlar.

## Müsaitlik Detayı

"Müsaitlik" faktörünün nasıl hesaplandığı ve `Busy` durumundaki bir doktorun neden
tamamen filtrelenmediği (hasta yine de seçebilmeli) `doctor-availability.md`'de
detaylandırılmıştır. Özet: `Away` durumundaki doktorlar sonuç kümesinden tamamen
çıkarılır; `Busy` durumundakiler "yoğun" etiketiyle listelenmeye devam eder.

## Neden Atama Değil Öneri

Doktor eşleştirmesinin otomatik atama yerine öneri olması bilinçli bir regülasyon
kararıdır: MedInsight bir doktoru "sana bu hastayı atadık" diyerek klinik sorumluluk
üstlenmiş gibi görünmemelidir. Hasta veya doktor son kararı verir; bu, Hızır'ın genel
"karar destek, karar verici değil" ilkesiyle örtüşür (bkz. `ai/guardrails-and-boundaries.md`).

## Girdi / Çıktı

**Girdi:** `DoctorMatchRequest { CaseId, RequiredSpecialty, PatientLocation? }`

**Çıktı:** `DoctorMatchResult[] { DoctorId, Score, ScoreBreakdown }` — en fazla 5 kayıt,
skora göre azalan sırada. `ScoreBreakdown`, her faktörün skora katkısını ayrı ayrı
gösterir (açıklanabilirlik ilkesi — doktor veya hasta "neden bu doktor önerildi"
sorusuna her zaman cevap bulabilmeli).

## İlişkili Dosyalar

- Case içindeki konumu: `case-aggregate-root.md`
- Doktor güven skoru: `reviewer-profile.md`
- Müsaitlik durumu: `doctor-availability.md`
- ADR: `../adr/adr-003-doctor-matching-scoring-model.md`
