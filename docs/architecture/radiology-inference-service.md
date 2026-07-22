# Radiology Inference Service

**Durum:** Onaylandı — MVP kapsamına dahil, bilgilendirici katman olarak
**İlgili ADR:** [adr-010-open-source-radiology-model-mvp.md](../adr/adr-010-open-source-radiology-model-mvp.md)

## Neden Ayrı Bir Servis (ve Neden Python)

.NET 8 backend'i (bkz. `../backend/tech-stack.md`), görüntü segmentasyonu/sınıflandırması
için gereken derin öğrenme ekosistemine (PyTorch, MONAI, nnU-Net) doğal bir ev sahibi
değildir. Bu nedenle görüntü çıkarımı, **ayrı, bağımsız deploy edilen bir Python
mikroservisi** olarak tasarlanır ve .NET tarafındaki AI Orchestration katmanından
(bkz. `../ai/ai-orchestration-flow.md`) bir iç API çağrısıyla tetiklenir.

```
AI Orchestration (Hızır, .NET)
        │
        │  internal API call (DICOM study reference)
        ▼
Radiology Inference Service (Python, FastAPI)
        │
        │  MONAI / nnU-Net önceden eğitilmiş model
        ▼
Ham model çıktısı (segmentasyon maskesi / sınıflandırma skoru)
        │
        ▼
AIFindings (Source = "OpenSourceImageModel")
```

## Kullanılan Kütüphaneler

- **MONAI** — model çalıştırma çerçevesi
- **nnU-Net / MONAI Model Zoo önceden eğitilmiş ağırlıklar** — örn. BraTS veri
  setiyle eğitilmiş beyin tümörü segmentasyon modeli gibi dar kapsamlı, halka açık
  modeller
- **pydicom, SimpleITK** — DICOM okuma/ön işleme

Bu modeller **klinik olarak doğrulanmamıştır** (bkz. ADR-010) — bu servisin tüm
tasarımı bu gerçeği merkeze alır.

## Girdi / Çıktı Sözleşmesi

**Girdi:** `{ studyId, dicomSeriesUrls: [] }`

**Çıktı:**
```
{
  findingId,
  modelName,            (örn. "nnUNet-BraTS-v1")
  modelSource: "OpenSource",
  outputType: "Segmentation" | "Classification",
  rawOutput,             (maske koordinatları veya sınıf skoru)
  disclaimer: "Bu bulgu klinik olarak doğrulanmamış açık kaynak bir modelden
               üretilmiştir; tek başına karar dayanağı olamaz."
}
```

`disclaimer` alanı **zorunludur** ve API sözleşmesinden çıkarılamaz — bu, bilgiyi
tüketen her arayüzün (doktor dashboard, Hızır) bu uyarıyı görmezden gelmesini
zorlaştırır.

## Zorunlu Sınırlar

1. Bu servisin çıktısı **hiçbir zaman** doğrudan `DifferentialDiagnosis`'a
   yazılmaz — sadece ayrı bir "ek bilgi" bloğu olarak `AIFindings` içinde durur
   (bkz. `../domain/case-aggregate-root.md` güncellemesi).
2. Confidence skoru hesaplamasına (bkz. `../ai/confidence-management.md`) dahil
   edilmez — bu servisin ürettiği bulgular ayrı bir "unvalidated" kategorisindedir,
   genel confidence eşiği mantığından muaftır çünkü zaten hiçbir zaman tek
   başına önceliklendirme tetiklemez.
3. Doktor dashboard'unda bu bulgular **her zaman** görsel olarak ayrı bir
   bölümde, farklı bir etiketle ("Deneysel — doğrulanmamış") gösterilir; AI'ın
   ana ön analizi ile aynı görsel ağırlıkta sunulmaz.

## Model Yönetimi

Her model, `modelName` ve versiyonuyla (`../backend/versioning.md`'deki AI model
versiyonlama kuralına tabi) kayıt altına alınır. Yeni bir açık kaynak model
entegre edilmeden önce bir ADR yazılmalıdır — bu, hangi modellerin sisteme dahil
edildiğinin denetlenebilir olmasını sağlar.

## Üçüncü Parti Servise Geçiş (Escalation)

Bu servisin çıktısı yeterli görülmeyen vakalarda (yüksek risk + belirsiz bulgu,
veya doktorun doğrudan talebi), sisteme `EscalationSuggested` sinyali düşer.
MVP'de bu, gerçek bir vendor çağrısı **tetiklemez** — sadece doktor
önceliklendirmesini artırır ve Case'e not düşer. Post-MVP'de aynı tetikleyici,
gerçek üçüncü parti API çağrısına bağlanacaktır. Tetikleme koşulları ve gerekçe
`../adr/adr-014-third-party-escalation-trigger.md`'de detaylandırılmıştır.

## İlişkili Dosyalar

- ADR: `../adr/adr-010-open-source-radiology-model-mvp.md`
- Escalation: `../adr/adr-014-third-party-escalation-trigger.md`
- Domain etkisi: `../domain/case-aggregate-root.md`
- Guardrails: `../ai/guardrails-and-boundaries.md`
- Gelecek ekosistem: `../ai/ai-agent-ecosystem.md`
