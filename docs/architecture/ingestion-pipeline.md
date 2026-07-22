# Ingestion Pipeline

**Durum:** Onaylandı
**Amaç:** 800+ dosyalık toplu yüklemenin, kalite kontrolünden önce nasıl
sınıflandırıldığını, DICOM serilerinin nasıl gruplandığını ve her belge türünün
hangi işleme yoluna yönlendirildiğini tanımlamak.

## Neden Ayrı Bir Doküman

`document-quality-engine.md`, bir belgenin **kalitesini** ölçer ama belgenin
**ne olduğunu** belirlemez — kalite kriterlerinin hangilerinin uygulanacağı
(DICOM Integrity mi, OCR Score mu) zaten bir tür bilgisi gerektirir. Bu doküman,
kalite kontrolünden önce gelen adımı tanımlar.

## Uçtan Uca Akış

```
800 dosya yüklenir (DocumentUploaded × N)
        │
        ▼
1. Classification (sınıflandırma)
        │
        ▼
2. DICOM Grouping (sadece DICOM dosyaları için)
        │
        ▼
3. Document Quality Engine  (bkz. document-quality-engine.md)
        │
        ▼
4. Routing (işleme yolu belirleme)
        │
   ┌────┼────────────────┐
   ▼                      ▼
Text Extraction      Radiology Inference Service
(PDF/rapor)           (bkz. radiology-inference-service.md)
   │                      │
   ▼                      ▼
LLMTextAnalysis       AIFindings (Source: OpenSourceImageModel)
(Hızır)
```

## 1. Classification

Kural bazlı, deterministik bir sınıflandırmadır — bu adımda AI kullanılmaz,
çünkü dosya türü tespiti (MIME type, dosya uzantısı, DICOM magic number varlığı)
belirsizlik içermeyen bir problemdir.

| Girdi | Tespit Yöntemi | Sonuç Tür |
|---|---|---|
| `.dcm` uzantısı veya DICOM magic number (`DICM` header) | Binary header kontrolü | `DicomFile` |
| `.pdf`, metin katmanı mevcut | PDF text layer kontrolü | `TextualReport` |
| `.pdf`, metin katmanı yok (taranmış) | PDF text layer kontrolü | `ScannedReport` |
| `.jpg`, `.png` vb. | MIME type | `PhotoDocument` |
| Ses/video (gelecek) | MIME type | MVP kapsamı dışı |

Sonuç, `MedicalDocument.DocumentType` alanına yazılır ve bir `DocumentClassified`
event'i yayınlanır (bkz. `../domain/domain-events-catalog.md`).

## 2. DICOM Grouping

Toplu yüklemede yüzlerce DICOM slice'ı tek tek dosya olarak gelir; bunların
`case-aggregate-root.md`'deki `DICOMStudies` yapısına dönüşmesi gerekir.

```
Ham DICOM dosyaları
        │
        ▼
StudyInstanceUID'ye göre grupla  → DICOMStudy
        │
        ▼
SeriesInstanceUID'ye göre grupla  → DICOMSeries (Study'nin alt kırılımı)
        │
        ▼
Her Series'in Modality tag'i okunur (MR, CT, US...)
```

Bu gruplama, dosyaların **hepsi yüklenene kadar** tamamlanamayabilir — toplu
yüklemede dosyalar farklı zamanlarda gelebilir (resumable upload). Bu nedenle
gruplama, bir "bekleme penceresi" mantığıyla çalışır: son dosya geldikten sonra
belirli bir süre (örn. 2 dakika) yeni dosya gelmezse, o ana kadar toplanan
dosyalarla grup tamamlanmış sayılır ve `DICOMStudyGrouped` event'i yayınlanır.

## 3. Document Quality Engine

Sınıflandırma ve gruplama tamamlandıktan sonra, her `MedicalDocument` veya
`DICOMSeries`, türüne uygun kalite kriterleriyle değerlendirilir (bkz.
`document-quality-engine.md`).

## 4. Routing

| DocumentType | Yönlendirilen Servis |
|---|---|
| `TextualReport` | Doğrudan Text Extraction Service (OCR gerekmez, metin zaten var) |
| `ScannedReport` | Text Extraction Service → OCR |
| `PhotoDocument` | Şimdilik sadece depolanır ve kalite kontrolünden geçer; MVP'de aktif bir analiz yolu yok (Document AI Post-MVP) |
| `DicomFile` (gruplandıktan sonra `DICOMSeries`) | Radiology Inference Service |

Routing kararı, `RoutingDecided` event'i olarak kayda geçer — bu, bir belgenin
neden belirli bir yola gittiğinin sonradan denetlenebilmesini sağlar.

## Hata Durumu — Sınıflandırılamayan Dosya

Bir dosya hiçbir kategoriye uymuyorsa (örn. bozuk dosya, desteklenmeyen format),
`DocumentClassificationFailed` event'i üretilir ve dosya "beklemede" durumuna
düşer — hastaya "bu dosya tanınamadı, farklı formatta tekrar yükler misin"
bildirimi gider. Sessizce yok sayılmaz.

## İlişkili Dosyalar

- Kalite kontrolü: `document-quality-engine.md`
- Görüntü analizi hedefi: `radiology-inference-service.md`
- Metin analizi hedefi: `text-extraction-service.md`
- Case yapısı: `../domain/case-aggregate-root.md`
- Event kataloğu: `../domain/domain-events-catalog.md`
