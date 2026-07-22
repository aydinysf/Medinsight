# ADR-001: Case Aggregate Root Olarak Seçildi

**Durum:** Kabul edildi
**Tarih:** v1.0 tasarımı, v3.0'da genişletildi

## Bağlam

Sistemde hasta, doktor, belge, analiz, mesaj gibi birçok varlık var. Bunlar arasında
tutarlılık sınırının nerede çizileceği belirlenmeliydi — hangi varlık "kaynak of
truth" olacak, hangi işlemler atomik olmak zorunda?

## Karar

Case, sistemin tek aggregate root'udur. Documents, AI Analyses, Consultations,
Treatments, Health Route, Timeline, Tasks, Notes, Billing ve Audit dahil tüm alt
bileşenler Case üzerinden erişilir ve değiştirilir. Hiçbir alt bileşen Case
referansı olmadan var olamaz.

## Alternatifler

1. **Patient'ı aggregate root yapmak** — Reddedildi. Bir hastanın birden fazla
   Case'i olabilir ve her Case'in kendi yaşam döngüsü, kendi doktor ekibi ve kendi
   tutarlılık kuralları vardır. Patient'ı root yapmak, aggregate'i gereksiz
   büyütür ve iki farklı Case arasında yanlışlıkla veri sızıntısına yol açabilir.

2. **Her alt bileşeni (Document, Consultation vb.) kendi aggregate'i yapmak** —
   Reddedildi. Bu, "tedavi planı oluşturulduğunda Health Route güncellenmeli"
   gibi invariant'ları uygulama katmanına yayardı; domain modelinin garanti etmesi
   gereken kurallar dağınık hale gelirdi.

## Sonuç

- Case dışından hiçbir alt bileşene doğrudan yazma izni verilmez.
- Tüm state değişiklikleri Case üzerinden bir domain event üretir.
- Bu, Case'i potansiyel olarak "şişkin" bir aggregate yapma riski taşır — bu riski
  Timeline ve Billing gibi alt bileşenleri ayrı okuma modelleri (read model) olarak
  tasarlayarak azaltıyoruz (bkz. `../architecture/timeline-service.md`).

## İlgili Dosyalar

- `../domain/case-aggregate-root.md`
