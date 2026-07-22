# Case Aggregate Root

**Bounded Context:** Case Management
**Durum:** Onaylandı — v3.0 itibarıyla genişletildi
**İlgili ADR:** [adr-001-case-as-aggregate-root.md](../adr/adr-001-case-as-aggregate-root.md)

## Neden Case, Aggregate Root'tur

Case, MedInsight'ın merkezindeki tek gerçek tutarlılık sınırıdır (consistency boundary).
Bir hastanın sağlık yolculuğunda üretilen her veri parçası — belge, analiz, mesaj, tedavi
kararı — bir Case'e aittir ve Case dışında anlam taşımaz. Case'i aggregate root yapmak,
şu invariant'ı garanti eder:

> Case'in durumu (status, risk level) hiçbir alt bileşen doğrudan değiştirilerek bozulamaz;
> her değişiklik Case üzerinden, bir domain event üreterek geçer.

## Genişletilmiş Yapı (v3.0)

İlk tasarımda Case üç alt bileşenle sınırlıydı (Documents, AI Analysis, Consultation).
Bu, gerçek kullanımda yetersiz kaldı. Bir Case gerçekte şunları barındırır:

```
Case (Aggregate Root)
│
├── Timeline                  — append-only olay geçmişi (bkz. event-storming.md)
├── MedicalDocuments           — PDF, görüntü, genel belgeler
├── DICOMStudies                — MR/BT görüntüleme çalışmaları (DICOM metadata dahil;
│                                  aynı modaliteden birden fazla çalışma arasındaki
│                                  ilişki için bkz. study-comparison.md)
├── LaboratoryResults           — kan/tahlil sonuçları, yapılandırılmış değerler
├── AIFindings                  — AI'ın belge/görüntü bazlı ham bulguları
├── DifferentialDiagnosis       — AI + doktor tarafından üretilen olası tanı listesi
├── HealthRoute                  — güncel durum + versiyon geçmişi (bkz. health-route-versioning.md)
├── Consultations                — doktor görüşmeleri, mesajlaşma
├── Treatments                   — tedavi planları
├── FollowUps                    — kontrol randevuları, kronik takip kayıtları
├── Tasks                        — vaka üzerinde atanmış iş kalemleri (hasta veya doktor için)
├── Notes                        — serbest metin klinik notlar
├── Billing                      — konsültasyon ücretlendirme kayıtları (Post-MVP)
└── Audit                        — bu Case üzerindeki her aksiyonun değiştirilemez kaydı
```

### Neden `AIFindings` ile `DifferentialDiagnosis` ayrı?

Bu ayrım kritik bir domain kararı. `AIFindings`, AI'ın bir belgeden çıkardığı ham gözlemdir
("sol frontal lobda 2.3cm kitle") — yorum içermez. `DifferentialDiagnosis`, bu bulguların
sentezinden üretilen olası tanı adaylarıdır ve **her zaman bir confidence skoru** ile
gelir. Bu ayrım olmadan, "AI ne gördü" ile "AI ne düşünüyor" sorularının cevabı
birbirine karışır — ki bu da Hızır'ın "tanı koymaz" ilkesini (bkz. `ai/guardrails-and-boundaries.md`)
teknik olarak ihlal etmeye çok yaklaşır.

### AIFindings.Source — Bulgunun Kaynağı Ayrımı

`AIFindings`, tek bir kaynaktan gelmez. `Source` alanı iki değer alabilir:

- **`LLMTextAnalysis`** — Hızır'ın rapor metni, lab değerleri, doktor notu gibi
  yapılandırılmış/metinsel veriden ürettiği bulgu. `DifferentialDiagnosis`'u
  besleyebilir.
- **`OpenSourceImageModel`** — açık kaynak, klinik olarak doğrulanmamış bir
  görüntü modelinin (bkz. `../architecture/radiology-inference-service.md`)
  ham çıktısı. **Asla** `DifferentialDiagnosis`'u besleyemez, confidence eşiği
  mantığına dahil edilmez, arayüzde zorunlu `disclaimer` ile ayrı gösterilir
  (bkz. `../adr/adr-010-open-source-radiology-model-mvp.md`).

Bu ayrım, ileride `../ai/ai-agent-ecosystem.md`'deki Radiology AI klinik
doğrulamayla olgunlaştığında, `Source` değerinin `ValidatedImageModel` gibi yeni
bir değerle genişlemesine izin verir — mevcut şema bunu bozmadan büyür.

### Neden `Billing` Case'in içinde, ayrı bir aggregate değil?

Konsültasyon ücreti, o konsültasyonun Case'e ait olduğu gerçeğinden ayrılamaz — bir
ödeme kaydı, hangi Case için hangi Consultation'ın karşılığı olduğunu her zaman bilmelidir.
Ancak `Billing` MVP kapsamında boş bir alt bileşen olarak durur (bkz. `business/doctor-economy.md`);
şema buna hazır ama akış yok.

## Invariant'lar

Case aggregate'inin garanti ettiği kurallar:

1. Bir Case'in `HealthRoute`'u her zaman tam olarak bir tane current state'e sahiptir
   (many-to-one snapshot geçmişi olsa da).
2. Bir `Treatment` oluşturulduğunda, mutlaka bir `HealthRoute` snapshot'ı tetiklenir —
   tedavi planı olup da rotası güncellenmeyen bir Case olamaz.
3. `DifferentialDiagnosis` güncellendiğinde, ilgili `AIFindings` referansı (`SourceFindingIds`)
   boş olamaz — kaynaksız tanı adayı üretilemez (kaynak izlenebilirliği ilkesi).
4. `Audit` kaydı hiçbir zaman uygulama kodundan silinemez veya güncellenemez; sadece
   event handler'lar tarafından eklenir.
5. Case `Closed` durumundayken yeni `MedicalDocument` eklenemez — önce `FollowUp` ile
   yeniden açılmalıdır (bkz. `case-lifecycle-state-machine.md`).

## Alt Bileşenlerin Case Dışından Erişimi

Hiçbir alt bileşen (örn. `Consultation`) kendi başına, Case referansı olmadan
sorgulanamaz veya değiştirilemez. Örnek: `POST /consultations/{id}/treatment-plan`
endpoint'i bile arka planda `caseId` doğrulaması yapar — treatment plan'ı Case'in
mevcut durumundan (örn. `Closed` ise reddet) bağımsız olarak kabul etmez.

## İlişkili Dosyalar

- Kalite kontrolü: `document-quality-engine.md`
- Sağlık rotası versiyonlama: `health-route-versioning.md`
- Doktor eşleştirme: `doctor-matching-engine.md`
- Tam event akışı: `event-storming.md`
- ERD: `erd-identity-case.md`, `erd-ai-clinical.md`
