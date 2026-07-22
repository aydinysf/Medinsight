# ADR-009: Doktor Müsaitlik Modeli — Otomatik Hesaplama + Manuel Override

**Durum:** Kabul edildi
**Karar tarihi:** Repo dokümantasyon oturumu, domain/ genişletmesi sonrası

## Bağlam

Doctor Matching Engine (ADR-003), müsaitliği skorlama faktörlerinden biri olarak
tanımlamıştı ama "müsaitlik" kavramı somutlaştırılmamıştı. Sorulması gereken üç
soru vardı: (1) doktorun uygunluk durumunu kim belirler, (2) yoğun bir doktor
hastaya hiç gösterilmeli mi, (3) hasta mı doktoru seçiyor yoksa sistem mi atıyor.

Üçüncü soru zaten ADR-003 ile cevaplanmıştı: sistem önerir, hasta/caregiver seçer,
sistem atama yapmaz. Bu ADR sadece ilk iki soruyu netleştirir.

## Karar

**Hibrit model:** Sistem, doktorun açık vaka sayısını (`ActiveCaseCount`) bir
eşikle (`CapacityThreshold`) karşılaştırarak otomatik bir `ComputedStatus`
(Available | Busy) hesaplar. Doktor bu hesaplamayı `ManualOverride` ile geçersiz
kılabilir — özellikle tam kapanma (`Away`) durumu **sadece** doktorun bilinçli
kararıyla oluşabilir, sistem hiçbir zaman bir doktoru otomatik olarak "yok"
sayamaz.

Görünürlük tarafında: `Busy` doktorlar hastaya "yoğun" etiketiyle gösterilmeye
devam eder ve hasta yine de seçebilir; `Away` doktorlar sonuç kümesinden tamamen
çıkarılır.

## Alternatifler

1. **Sadece manuel (doktor kendi durumunu her zaman elle günceller)** —
   Reddedildi. Doktorlar yoğunluklarını genellikle unutup güncellemezler; bu,
   zaten dolu bir doktora sürekli yeni vaka yönlendirilmesine yol açardı.

2. **Sadece otomatik (doktorun override hakkı yok)** — Reddedildi. Doktorun
   planlı izin gibi durumları sistem vaka sayısından anlayamaz; doktora kendi
   durumunu belirtme hakkı tanınmazsa, izindeyken bile vaka önerilmeye devam
   ederdi.

3. **Busy doktorları tamamen filtrelemek (Available olmayanı hiç gösterme)** —
   Reddedildi. Bu, hastanın bilerek "biraz beklesem de bu doktoru istiyorum"
   seçimini elinden alırdı; özellikle tercih edilen/yüksek puanlı bir doktor
   geçici olarak yoğunsa, hasta bunu bilerek bekleyebilmeli.

## Sonuç

- `DoctorMatchResult` şemasına `availabilityTag` alanı eklendi.
- `Away` durumu asla `ComputedStatus` tarafından üretilemez — sadece
  `ManualOverride` ile set edilebilir; bu, yanlışlıkla bir doktorun sistemden
  "kaybolmasını" engelleyen bir güvenlik önlemidir.
- `OverrideExpiresAt` alanı, doktorun izin bitiminde manuel olarak durumu geri
  almayı unutması riskini azaltır.

## İlgili Dosyalar

- `../domain/doctor-availability.md`
- `../domain/doctor-matching-engine.md`
- `../adr/adr-003-doctor-matching-scoring-model.md`
