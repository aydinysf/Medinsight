# Yapılanlar

## 2026-07-21

- [Docs] MVP yol haritası oluşturuldu (9 iş paketi, 5 milestone, Gantt)
  - Dosya: docs/business/roadmap.md
  - Not: project_arch dokümantasyon sentezinden türetildi; hedef 21 Tem – 16 Eki 2026. Commit henüz yapılmadı.

- [Config] WP0: Dokümanlar docs/ altına taşındı, ADR-015 yazıldı, çözüm yapısı hizalandı
  - Dosya: docs/** (60 md, overlay sırasıyla en güncel kopyalar), docs/adr/adr-015-dotnet-9-and-solution-structure.md, MedInsight.sln, Dockerfile, README.md, .gitignore
  - Not: Shared + Reporting kaldırıldı; AIOrchestration + TimelineService eklendi (→ Domain); project_arch/ gitignore'a alındı (ham arşiv). Build 0 uyarı / 0 hata.

## 2026-07-05

- [Feature] MedInsight çözümü sıfırdan oluşturuldu (.NET 9, Clean Architecture, CDSS)
  - Dosya: MedInsight.sln, src/* (7 proje: Api, Domain, Application, Infrastructure, Shared, Dicom, Reporting)
  - Not: Clean Architecture referans kuralları uygulandı — Domain hiçbir şeye referans vermez; Application→Domain; Infrastructure→Application+Domain; Api→Application+Infrastructure; Reporting→Application; Dicom→Domain

- [Feature] PostgreSQL + EF Core altyapısı yapılandırıldı (entity YOK, sadece konfigürasyon)
  - Dosya: src/MedInsight.Infrastructure/Persistence/MedInsightDbContext.cs, src/MedInsight.Infrastructure/DependencyInjection.cs
  - Not: Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4; EF Core 9.0.7'ye sabitlendi (MSB3277 sürüm çakışması giderildi). Bağlantı anahtarı: ConnectionStrings:MedInsightDb

- [Feature] IAiService arayüzü eklendi (gelecekteki Python AI servisi için, implementasyonsuz)
  - Dosya: src/MedInsight.Application/Abstractions/Ai/IAiService.cs

- [Feature] Swagger/OpenAPI + Health Check yapılandırıldı
  - Dosya: src/MedInsight.Api/Program.cs
  - Not: GET /health → 200 "Healthy" (liveness, doğrulandı); GET /health/ready → PostgreSQL bağlantısını da kontrol eder; Swagger UI sadece Development ortamında

- [Config] Docker ve depo dosyaları eklendi
  - Dosya: Dockerfile (multi-stage, non-root, curl healthcheck), docker-compose.yml (api + postgres:17-alpine), .editorconfig, .gitignore, README.md, LICENSE (MIT), global.json (SDK 9.0.3xx), Directory.Build.props

- Not: Çözüm Release modunda 0 uyarı / 0 hata ile derlendi. Entity, business logic ve Patient kavramı bilinçli olarak eklenmedi.

- [Config] Git deposu oluşturuldu ve GitHub'a pushlandı
  - Not: https://github.com/aydinysf/Medinsight — main dalı, ilk commit db9bf05 (25 dosya)

- [Feature] Sprint 1 domain modeli eklendi (7 entity + 8 enum)
  - Dosya: src/MedInsight.Domain/Entities/* (Patient, MedicalCase, Study, Series, MedicalDocument, Measurement, TimelineEvent), src/MedInsight.Domain/Enums/*, src/MedInsight.Domain/Common/Entity.cs
  - Not: Tüm entity'lerde Guid Id + CreatedAtUtc, private setter, static Create fabrika metotları. Tanı/AI/öneri mantığı bilinçli olarak yok

- [Feature] Persistence katmanı: DbSet'ler, Fluent API konfigürasyonları, indeksler
  - Dosya: src/MedInsight.Infrastructure/Persistence/MedInsightDbContext.cs, Persistence/Configurations/* (7 sınıf)
  - Not: timestamptz/date/numeric(18,4)/text kolon tipleri; istenen 7 indeks (Patient.FullName, MedicalCase.PatientId, Study MedicalCaseId+StudyDateUtc, Series.StudyId, MedicalDocument.MedicalCaseId, Measurement.MedicalCaseId, TimelineEvent MedicalCaseId+EventDateUtc). Study/Series silmede Measurement/Document FK'ları SET NULL, case silmede CASCADE

- [Feature] Repository'ler + Application servisleri + DTO'lar
  - Dosya: src/MedInsight.Application/Abstractions/Repositories/*, Patients/*, MedicalCases/*; src/MedInsight.Infrastructure/Repositories/*
  - Not: IPatientRepository, IMedicalCaseRepository; CreatePatientService, CreateMedicalCaseService; record DTO'lar (DataAnnotations doğrulaması constructor parametresinde — property: hedefi MVC'de exception fırlatıyor)

- [Feature] API endpoint'leri: POST/GET /patients, POST/GET /patients/{patientId}/cases
  - Dosya: src/MedInsight.Api/Controllers/PatientsController.cs, MedicalCasesController.cs, Program.cs
  - Not: JsonStringEnumConverter eklendi; canlı test edildi (201/200/404/400 senaryoları doğrulandı)

- [DB Migration] InitialDomainModel migration'ı oluşturuldu ve uygulandı
  - Dosya: src/MedInsight.Infrastructure/Migrations/20260705173633_InitialDomainModel.cs, .config/dotnet-tools.json (dotnet-ef 9.0.7 local tool)
  - Not: Api projesine Microsoft.EntityFrameworkCore.Design eklendi; .editorconfig'e Migrations klasörü için generated_code muafiyeti eklendi

- [Config] docker-compose ve bağlantı ayarları güncellendi
  - Dosya: docker-compose.yml, src/MedInsight.Api/appsettings.json, src/MedInsight.Api/Program.cs
  - Not: Host portu 5432→5434 (5432/5433 başka container'larda dolu); localhost yerine 127.0.0.1 (::1'i wslrelay yakalıyor, "Exception while reading from stream" hatası); Database:ApplyMigrationsOnStartup bayrağı eklendi, compose'da true

- Not: Build 0 uyarı / 0 hata. Commit YAPILMADI (istenmedi).
