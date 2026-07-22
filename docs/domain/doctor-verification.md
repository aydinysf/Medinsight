# Doctor Verification

**Bounded Context:** Identity & Verification
**Durum:** Onaylandı
**İlgili ADR:** [adr-007-qr-based-doctor-verification.md](../adr/adr-007-qr-based-doctor-verification.md)

## Amaç

Doktorun platforma serbest kayıt olmasını engellemek; diploma/uzmanlık belgesi ve QR
doğrulaması üzerinden admin onayına tabi bir doğrulama süreci işletmek.

## Neden Yarı Otomatik

Şu an YÖK veya TTB gibi resmi bir doğrulama kurumuyla entegrasyon yoktur. Bu nedenle
süreç şöyle işler:

1. Doktor, diploma/uzmanlık belgesini ve üzerindeki QR kodu yükler.
2. Sistem QR içeriğini parse eder (`QrParsedData` — isim, diploma no, tarih gibi
   alanlar).
3. Parse edilen veri admin'e **öneri** olarak sunulur — otomatik onay verilmez.
4. Admin, belge görseli ile parse edilen veriyi karşılaştırıp onaylar veya reddeder.

Bu tasarımda hiçbir adım tam otomatik onaya izin vermez; sahte belge riski, insan
onayı zorunluluğuyla azaltılır (bkz. Risk Register, skor 6: "Sahte doktor belgesi").

## Şema

```
DoctorVerification
- Id
- DoctorId
- DocumentType         (Diploma | UzmanlıkBelgesi | TTBSicil)
- DocumentUrl
- QrPayload             (ham QR içeriği)
- QrParsedData          (jsonb: isim, TC, diploma no, tarih)
- VerificationMethod    (Manual | QrParsed | ExternalApi)
- VerificationStatus    (Pending | Verified | Rejected)
- VerifiedByAdminId
- VerifiedAt
- RejectionReason
```

## Gelecek Genişleme

TTB/YÖK API'si açıldığında `VerificationMethod = ExternalApi` olarak eklenecek ve
otomatik doğrulama devreye girecektir — ancak bu değişiklik bile muhtemelen admin
onayını tamamen ortadan kaldırmayacak, sadece admin'in gördüğü öneri kalitesini
artıracaktır.

## İlişkili Dosyalar

- Doktor ekonomisi bağlamı: `../business/doctor-economy.md`
- ADR: `../adr/adr-007-qr-based-doctor-verification.md`
