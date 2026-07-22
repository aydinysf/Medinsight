# Doctor Availability

**Bounded Context:** Doctor Matching Engine (alt bileşen)
**Durum:** Onaylandı
**İlgili ADR:** [adr-009-doctor-availability-model.md](../adr/adr-009-doctor-availability-model.md)

## Amaç

Bir doktorun o an yeni vaka alıp alamayacağını hem sistemin otomatik hesaplayabildiği
hem de doktorun kendisinin geçersiz kılabildiği (override) bir durum olarak modellemek.
Bu durum, Doctor Matching Engine'in "müsaitlik" faktörünü (bkz.
`doctor-matching-engine.md`) somutlaştırır.

## Üç Durum

| Durum | Anlamı | Hastaya Görünürlük |
|---|---|---|
| `Available` | Doktor yeni vaka alabilir | Normal listelenir |
| `Busy` | Doktor yoğun ama tamamen kapalı değil | "Yoğun" etiketiyle listelenir, **seçilebilir** |
| `Away` | Doktor tamamen kapalı (izin, tatil) | **Hiç listelenmez** |

`Busy` ile `Away` arasındaki fark bilinçli: `Busy`, hastaya "bu doktor biraz
gecikebilir ama seçebilirsin" der; `Away`, "bu doktor şu an sistemde yok" anlamına
gelir. Bunları tek bir "müsait değil" durumunda birleştirmek, hastanın gerçek bir
seçeneği (yoğun ama iyi bir doktoru bilerek seçme) kaybetmesine yol açardı.

## Şema

```
DoctorAvailability
- DoctorId              PK, FK
- ActiveCaseCount        (güncel açık Consultation sayısı)
- CapacityThreshold       (doktor başına eşik — config'den, doktor bazlı override edilebilir)
- ComputedStatus          (Available | Busy — sistem tarafından hesaplanır, Away asla otomatik hesaplanmaz)
- ManualOverride          (Available | Busy | Away | null — doktorun kendi belirlediği durum)
- OverrideExpiresAt       (nullable — örn. "3 gün sonra otomatik kalksın")
- UpdatedAt
```

## Efektif Durumun Hesaplanması

```
EffectiveStatus =
    ManualOverride ?? ComputedStatus
```

- `ManualOverride` set edilmişse, o geçerlidir (doktorun sözü sistemin
  hesaplamasından ağır basar).
- `ManualOverride` yoksa, `ComputedStatus` kullanılır.
- `ComputedStatus` **asla** `Away` üretemez — tam kapanma sadece doktorun bilinçli
  kararıyla olur. Sistem otomatik olarak bir doktoru "yok" sayamaz, sadece
  "yoğun" işaretleyebilir.

## ComputedStatus Hesaplama Mantığı

```
if ActiveCaseCount >= CapacityThreshold:
    ComputedStatus = Busy
else:
    ComputedStatus = Available
```

`ActiveCaseCount`, o doktora bağlı `Status = Active` olan `Consultation` sayısıdır
(bkz. `consultation-model.md`). Her yeni `ConsultationStarted` veya
`ConsultationCompleted` event'i bu sayacı günceller.

## Doctor Matching Engine ile İlişki

`doctor-matching-engine.md`'deki skorlama faktörlerinden "müsaitlik" artık şöyle
çalışır:

| EffectiveStatus | Sonuç Listesinde | Müsaitlik Skoru |
|---|---|---|
| Available | Listelenir | Tam puan |
| Busy | Listelenir, `"yoğun"` etiketiyle | Düşük ama sıfır değil |
| Away | **Sonuç kümesinden tamamen çıkarılır** | — |

Bu, `DoctorMatchResult` şemasına bir `AvailabilityTag` alanı ekler:
`{ doctorId, score, scoreBreakdown, availabilityTag: "Available" | "Busy" }`

## Doktor Tarafı — Override Nasıl Yapılır

Doktor dashboard'unda (bkz. `../frontend/doctor-dashboard.md`) tek dokunuşla durum
değiştirme: `Available ⇄ Busy ⇄ Away`. `Away` seçildiğinde opsiyonel bir
`OverrideExpiresAt` tarihi girilebilir (örn. izin bitiş tarihi) — bu tarih
geldiğinde `ManualOverride` otomatik olarak temizlenir ve sistem tekrar
`ComputedStatus`'a döner.

## İlişkili Dosyalar

- Skorlama motoru: `doctor-matching-engine.md`
- Consultation sayacı: `consultation-model.md`
- ADR: `../adr/adr-009-doctor-availability-model.md`
