# Yapılanlar

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
