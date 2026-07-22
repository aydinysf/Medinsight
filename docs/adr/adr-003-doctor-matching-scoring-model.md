# ADR-003: Ağırlıklı Skorlama ile Doktor Eşleştirme

**Durum:** Kabul edildi

## Bağlam

Bir Case için doktor önerisi üretilirken hangi doktorun öncelikli gösterileceği
belirlenmeliydi. Basit bir "branşı uyan ilk müsait doktor" yaklaşımı, doktor
tecrübesi ve hasta konumu gibi faktörleri göz ardı ederdi.

## Karar

Beş faktörlü, konfigüre edilebilir ağırlıklı bir skorlama motoru kullanılır: branş
uyumu, konum/uzaklık, müsaitlik, tecrübe/vaka sayısı, ortalama yanıt hızı. Motor
**atama yapmaz**, sadece en fazla 5 doktoru skora göre sıralı önerir; nihai seçim
hasta veya doktora bırakılır.

## Alternatifler

1. **Otomatik atama** — Reddedildi. Regülasyon riski taşır: MedInsight bir doktoru
   hastaya "atamış" gibi görünmemelidir, çünkü bu klinik sorumluluk üstlenmiş
   izlenimi verir (bkz. Risk Register, "Regülasyon sınırının aşılması").

2. **Sabit sıralama kuralı (örn. sadece müsaitlik)** — Reddedildi. Tek faktörlü
   sıralama, düşük tecrübeli ama müsait bir doktoru yüksek tecrübeli ama az müsait
   olana göre öne çıkarabilirdi; bu hasta güvenini zedeler.

## Sonuç

- Ağırlıklar config'den okunur; ileride Hospital rolü aktif olduğunda kurumsal
  öncelik gibi yeni bir faktör kod değişikliği gerektirmeden eklenebilir.
- Her öneri `ScoreBreakdown` ile gelir — açıklanabilirlik zorunlu.

## İlgili Dosyalar

- `../domain/doctor-matching-engine.md`
