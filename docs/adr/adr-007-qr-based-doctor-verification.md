# ADR-007: QR Tabanlı Yarı Otomatik Doktor Doğrulama

**Durum:** Kabul edildi

## Bağlam

Doktorların platforma güvenilir biçimde kaydolması gerekiyordu, ancak şu an YÖK
veya TTB gibi resmi bir doğrulama kurumuyla entegrasyon mevcut değil.

## Karar

Doktor, diploma/uzmanlık belgesini ve üzerindeki QR kodunu yükler. Sistem QR
içeriğini parse eder ve admin'e **öneri** olarak sunar; nihai onay her durumda
admin tarafından verilir. Otomatik onay hiçbir koşulda yapılmaz.

## Alternatifler

1. **Tamamen manuel inceleme (QR kullanılmadan)** — Reddedildi. Admin'in belge
   üzerindeki tüm bilgiyi elle karşılaştırması hem yavaş hem hataya açık; QR
   parse, admin'e bir başlangıç noktası sunarak süreci hızlandırır.

2. **QR parse sonucuna göre tam otomatik onay** — Reddedildi. Sahte QR/belge
   riski (Risk Register, skor 6) bunu kabul edilemez kılıyor; insan onayı
   zorunlu tutuldu.

## Sonuç

- `VerificationMethod` alanı `Manual`, `QrParsed`, `ExternalApi` değerlerini
  taşıyabilir — TTB/YÖK API'si açıldığında üçüncü seçenek devreye girecek, ama
  admin onayı muhtemelen yine de kalkmayacak.

## İlgili Dosyalar

- `../domain/doctor-verification.md`
