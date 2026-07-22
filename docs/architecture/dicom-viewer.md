# DICOM Viewer

**Durum:** Onaylandı
**İlgili ADR:** [adr-012-dicom-viewer-choice.md](../adr/adr-012-dicom-viewer-choice.md)

## Karar

Açık kaynak bir DICOM web viewer (OHIF Viewer / Cornerstone.js ekosistemi)
çekirdek render motoru olarak gömülür. MedInsight'ın kendi geliştirdiği kısım,
render motorunun **üstüne** eklenen bağlam katmanıdır — DICOM piksellerini
çizme işini yeniden icat etmeyiz.

## Neden Sıfırdan Yazılmadı

OHIF, dünya çapında birçok sağlık kurumunda üretimde kullanılan, MPR
(multi-planar reconstruction), windowing/leveling, ölçüm araçları gibi
özellikleri yıllarca olgunlaşmış bir projedir. Bunu MVP'de yeniden yazmak
aylar/yıllar sürer ve muhtemelen daha düşük kalitede bir sonuç üretir. MVP
kaynaklarını burada harcamak, ürünün asıl farkını (sağlık yolculuğu yönetimi,
bkz. `../business/competitive-analysis.md`) geciktirir.

## MedInsight'ın Eklediği Değer Katmanı

```
┌─────────────────────────────────────────┐
│  MedInsight Context Layer (bizim kodumuz) │
│  - Case bilgisi yan panel                  │
│  - AIFindings'i görüntü üzerinde işaretleme │
│  - "Deneysel bulgu" disclaimer overlay'i    │
│    (bkz. radiology-inference-service.md)   │
│  - Hızır ile konuşmaya bağlantı            │
├─────────────────────────────────────────┤
│  OHIF Viewer / Cornerstone.js (açık kaynak) │
│  - DICOM render, MPR, windowing, ölçüm      │
└─────────────────────────────────────────┘
```

MedInsight'a özgü kısım, viewer'ı Case'in geri kalanına (Timeline, AIFindings,
Consultation) bağlayan entegrasyon katmanıdır — bu, "kendi viewer'ımızı
yazdık" iddiasından farklı ama gerçek bir değer üretimidir.

## Görüntü Servisi

Viewer, DICOM verisini DICOMweb (WADO-RS) protokolü üzerinden okur. Bu, object
storage'daki (bkz. `../backend/tech-stack.md`) ham DICOM dosyalarının önüne
hafif bir DICOMweb sunucu katmanı eklenmesini gerektirir — tam bir PACS
sistemi değildir (PACS entegrasyonu MVP dışıdır, bkz.
`../adr/adr-008-mvp-scope-exclusions.md`), sadece viewer'ın ihtiyaç duyduğu
standart protokolü konuşan minimal bir servis.

## Ne Zaman Kendi Çözümümüzü Değerlendiririz

Eğer MedInsight'a özgü bir görüntüleme ihtiyacı ortaya çıkarsa (örn. Radiology
Inference Service'in ürettiği segmentasyon maskesini OHIF'in desteklemediği bir
şekilde görselleştirmek gerekirse), bu OHIF'in plugin API'si üzerinden
eklenir — çekirdek render motorunu değiştirmeden. Çekirdek motoru
değiştirmeyi gerektirecek bir ihtiyaç ortaya çıkarsa, bu yeni bir ADR
gerektirir.

## İlişkili Dosyalar

- ADR: `../adr/adr-012-dicom-viewer-choice.md`
- Görüntü analizi: `radiology-inference-service.md`
- MVP kapsam kararı: `../adr/adr-008-mvp-scope-exclusions.md`
