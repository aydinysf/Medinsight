# ADR-012: DICOM Viewer — Açık Kaynak Render Motoru + Kendi Bağlam Katmanı

**Durum:** Kabul edildi
**Karar tarihi:** Repo dokümantasyon oturumu

## Bağlam

Doktorun DICOM görüntülerini inceleyebilmesi için bir viewer gerekiyordu.
Seçenekler: (1) açık kaynak bir web viewer (OHIF/Cornerstone.js) gömmek,
(2) sıfırdan kendi viewer'ımızı yazmak, (3) MVP'de viewer olmadan sadece
indirilebilir dosya sunmak.

## Karar

Açık kaynak OHIF Viewer / Cornerstone.js ekosistemi çekirdek render motoru
olarak benimsendi. MedInsight kendi geliştirme çabasını, bu motorun üstüne
eklenen bağlam katmanına (Case bilgisi, AIFindings işaretleme, Hızır
bağlantısı) yoğunlaştırır.

## Gerekçe — Neden Sıfırdan Yazmadık

Karar kriteri netti: **kendi yazacağımız viewer, mevcut açık kaynak
çözümlerden daha iyi olmayacaksa, açık kaynağı kullan.** DICOM render motoru
(windowing, MPR, ölçüm araçları) yıllarca üretimde test edilmiş, klinik
ortamlarda kanıtlanmış bir problemdir. Bunu MVP sürecinde yeniden inşa etmeye
çalışmak, hem daha uzun sürer hem de muhtemelen daha düşük kalitede bir sonuç
verir — bu, kaynakların yanlış yere harcanması olurdu.

## Alternatifler

1. **Sıfırdan kendi viewer'ımızı yazmak** — Reddedildi, yukarıdaki gerekçeyle.
   Ancak bu kapı tamamen kapatılmadı: MedInsight'a özgü bir görselleştirme
   ihtiyacı (örn. Radiology Inference Service'in ürettiği segmentasyon
   maskesinin özel bir gösterimi) ortaya çıkarsa, bu OHIF plugin API'si
   üzerinden değerlendirilecek.

2. **Viewer olmadan sadece indirilebilir dosya** — Reddedildi. Doktorun DICOM
   dosyasını indirip kendi bilgisayarındaki ayrı bir programda açması, hem
   kullanıcı deneyimini böler hem de Case bağlamından (AIFindings, Timeline)
   kopuk bir inceleme deneyimi yaratır.

## Sonuç

- Object storage'ın önüne minimal bir DICOMweb (WADO-RS) servisi eklenir —
  bu tam bir PACS değildir (bkz. `../adr/adr-008-mvp-scope-exclusions.md`).
- MedInsight'ın gerçek katkısı viewer'ın kendisinde değil, onu Case'in geri
  kalanına bağlayan entegrasyon katmanındadır.

## İlgili Dosyalar

- `../architecture/dicom-viewer.md`
- `../architecture/radiology-inference-service.md`
