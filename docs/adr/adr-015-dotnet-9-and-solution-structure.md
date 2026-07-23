# ADR-015: .NET 9 ve Çözüm Yapısının Dokümanlarla Hizalanması

**Durum:** Kabul edildi
**Karar tarihi:** 2026-07-21, kod tabanı hizalama oturumu (WP0)

## Bağlam

`tech-stack.md` ve `layered-architecture.md` hedef platformu .NET 8 olarak
tanımlar ve `versioning.md` çerçeve gibi kritik bağımlılıklarda büyük sürüm
değişikliğinin ADR gerektirdiğini söyler. Kod tabanı ise .NET 9 üzerinde
kuruldu ve çalışır durumda (build, migration, canlı endpoint testleri geçti).

Ayrıca kod tabanındaki çözüm yapısı dokümanlardan sapmıştı: dokümanlarda hiç
geçmeyen `MedInsight.Shared` ve `MedInsight.Reporting` projeleri vardı;
dokümanların beklediği `MedInsight.AIOrchestration` ve
`MedInsight.TimelineService` projeleri yoktu.

## Karar

1. **Platform .NET 9 olarak kalır.** Dokümanların .NET 8 tercihi "olgun
   ekosistem" gerekçesine dayanıyordu; .NET 9 aynı ekosistemi (EF Core 9,
   MediatR, SignalR) destekler, STS→LTS geçiş maliyeti düşüktür ve kod tabanı
   üzerinde doğrulanmıştır. `tech-stack.md` bu ADR ile güncellenmiş sayılır.
2. **Çözüm yapısı dokümanlara hizalanır:**
   - `MedInsight.Shared` ve `MedInsight.Reporting` kaldırılır (boş, dokümansız).
   - `MedInsight.AIOrchestration` eklenir — Hızır ve AI sağlayıcı çağrılarının
     izole katmanı; Domain'e bağımlıdır, Domain ona bağımlı değildir.
   - `MedInsight.TimelineService` eklenir — pasif event abonesi (ADR-006);
     bağımsız dağıtılabilirlik hedefi korunur.
   - `MedInsight.Dicom` kalır — ingestion hattındaki DICOM ayrıştırma/gruplama
     işleri için doğal ev (`ingestion-pipeline.md`); dokümanlardaki "object
     storage Infrastructure içinde" kuralıyla çelişmez, depolama değil
     ayrıştırma sorumluluğu taşır. Application'daki `IDicomMetadataReader`
     soyutlamasını implemente ettiği için Application'a referans verir
     (Infrastructure ile aynı "abstraction'ı dışarıda implemente et" deseni).

## Alternatifler

1. **.NET 8'e geri dönmek** — Reddedildi. Çalışan ve test edilmiş bir .NET 9
   kod tabanını geri taşımanın maliyeti var, kazancı yok; dokümanların
   gerekçesi sürüme değil ekosisteme dairdi.
2. **Shared/Reporting projelerini tutmak** — Reddedildi. İçleri boş, hiçbir
   doküman sorumluluk tanımlamıyor; YAGNI (ADR-008 ilkesi). Reporting ihtiyacı
   doğarsa sorumluluğu önce dokümanda tanımlanır.

## Sonuç

- Çözüm: Domain, Application, Infrastructure, AIOrchestration,
  TimelineService, Dicom, Api (+ testler `tests/` altında, WP1'den itibaren).
- Bu ADR'den sonra yapı değişiklikleri yine önce doküman/ADR ister.

## İlgili Dosyalar

- `../backend/tech-stack.md`
- `../architecture/layered-architecture.md`
- `../backend/versioning.md`
- `../adr/adr-006-timeline-as-separate-bounded-context.md`
