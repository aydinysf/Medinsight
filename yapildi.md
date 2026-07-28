# Yapılanlar

## 2026-07-21

- [Docs] MVP yol haritası oluşturuldu (9 iş paketi, 5 milestone, Gantt)
  - Dosya: docs/business/roadmap.md
  - Not: project_arch dokümantasyon sentezinden türetildi; hedef 21 Tem – 16 Eki 2026. Commit henüz yapılmadı.

- [Config] WP0: Dokümanlar docs/ altına taşındı, ADR-015 yazıldı, çözüm yapısı hizalandı
  - Dosya: docs/** (60 md, overlay sırasıyla en güncel kopyalar), docs/adr/adr-015-dotnet-9-and-solution-structure.md, MedInsight.sln, Dockerfile, README.md, .gitignore
  - Not: Shared + Reporting kaldırıldı; AIOrchestration + TimelineService eklendi (→ Domain); project_arch/ gitignore'a alındı (ham arşiv). Build 0 uyarı / 0 hata.

## 2026-07-23

- [Feature] WP1: Çekirdek omurga — Case aggregate + domain events + outbox + Timeline + Identity
  - Dosya: src/MedInsight.Domain/{Common,Cases,Identity}/**, src/MedInsight.TimelineService/**, src/MedInsight.Infrastructure/{Persistence,Repositories,Timeline}/**, src/MedInsight.Api/{Controllers,Middleware}/**, tests/MedInsight.Domain.Tests/**
  - Not: Case tek aggregate root (ADR-001), 7 durumlu state machine (Draft→...→Closed, Reopen→FollowUp); her geçiş CaseStatusChanged üretir. Domain event zarfı + outbox (jsonb) + OutboxProcessor (at-least-once, idempotent handler). Timeline pasif abone (ADR-006). Identity: users/patients/doctors/caregivers + case_members. Tablolar snake_case (EFCore.NamingConventions). Kavram bazlı Domain klasörleri. API /api/v1 önekine geçti; DomainException→409. Migration reset: InitialSchema. 18 domain testi geçti; uçtan uca smoke test (hasta→vaka→outbox→timeline) doğrulandı.
  - Teknik not: Domain event'lerde ctor + [JsonConstructor] yerine required init property kullanıldı — nullable CaseId zarfı ctor parametresine bağlanamıyordu (STJ "must bind" hatası)

- [Test] Application handler testleri eklendi (in-memory fake repolar)
  - Dosya: tests/MedInsight.Application.Tests/**

- [Feature] WP2: JWT kimlik doğrulama + iki katmanlı yetkilendirme (ADR-016)
  - Dosya: docs/adr/adr-016-mvp-authentication.md, docs/domain/erd-identity-case.md (users: +role, +password_hash), src/MedInsight.Domain/Identity/User.cs, src/MedInsight.Application/{Abstractions/Auth,Auth,Common,Cases,Patients}/**, src/MedInsight.Infrastructure/Auth/**, src/MedInsight.Api/{Auth,Controllers,Middleware}/**, Migrations/AddAuthFieldsToUsers
  - Not: POST /api/v1/auth/login → JWT (userId+role claim); rol katmanı [Authorize(Roles)], kaynak katmanı handler'larda ICurrentUser ile (vaka üyeliği/sahiplik). PasswordHasher = ASP.NET Identity PBKDF2. ForbiddenAccessException→403. Canlı test: 401 (tokensiz/yanlış parola), 403 (başka profil/vaka/başkası adına), 200 (sahibi). 37 birim testi geçti. Jwt:Key dev değeri appsettings'te — üretimde secrets manager (docs/architecture/security-architecture.md)

- [Feature] WP3 dilim 1: Belge alma hattı — MinIO + bulk upload + sınıflandırma + kalite motoru
  - Dosya: docker-compose.yml (minio servisi, host 9500/9501), src/MedInsight.Application/{Abstractions/Storage,Documents,Ingestion,Quality}/**, src/MedInsight.Infrastructure/Storage/**, src/MedInsight.Api/Controllers/DocumentsController.cs, src/MedInsight.TimelineService/Handlers/IngestionTimelineHandlers.cs, Migrations/AddDocumentFileMetadataAndIdempotency
  - Not: POST /api/v1/cases/{id}/documents (multipart bulk, 202 Accepted, Idempotency-Key destekli); IObjectStorage soyutlaması → MinioObjectStorage (S3-uyumlu, sağlayıcı değişimi config); kural tabanlı DocumentClassifier (DICM magic, PDF metin katmanı sezgiseli, görüntü MIME); Quality Engine plugin mimarisi (DuplicatedFiles/Completeness/DicomIntegrity, eşik config'ten); yeterli skor CollectingData→AIAnalysis geçişini tetikler; sınıflandırılamayan dosya ClassificationFailed + event (sessiz yok sayma yok). 55 birim testi. Uçtan uca canlı test: 3 dosya → doğru sınıf/skor/timeline (11 kayıt), idempotent tekrar → belge sayısı sabit
  - Teknik not: EF Guid PK'ları varsayılan "store-generated" saydığından, izlenen Case'e eklenen yeni belgeler UPDATE sanılıyordu (DbUpdateConcurrencyException) — tüm Guid Id'ler ValueGeneratedNever yapıldı; Case sorgusu AsSplitQuery'e alındı
  - Kalan (dilim 2): DICOM gruplama (bekleme penceresi, fo-dicom), Text Extraction + IOcrProvider/Tesseract, RoutingDecided, resumable upload

- [Feature] WP3 dilim 2: DICOM gruplama + Routing + Text Extraction (ADR-011)
  - Dosya: src/MedInsight.Dicom/** (fo-dicom FoDicomMetadataReader — proje ilk gerçek işini aldı, → Application referansı ADR-015'e işlendi), src/MedInsight.Domain/Cases/{DicomStudy,DicomSeries,Case,MedicalDocument}.cs + Events/PipelineEvents.cs, src/MedInsight.Application/{Abstractions/{Dicom,TextExtraction},Ingestion/PipelineHandlers.cs}, src/MedInsight.Infrastructure/{Ingestion/DicomGroupingWindowProcessor.cs,TextExtraction/**}, Migrations/AddDicomGroupingAndTextExtraction
  - Not: DICOM gruplama StudyInstanceUID/SeriesInstanceUID üzerinden bul-veya-oluştur + bekleme penceresi (config: Ingestion:DicomGroupingWindowSeconds, dev 8sn/prod 120sn) → DICOMStudyGrouped. RoutingDecided: TextualReport/ScannedReport→TextExtraction, DicomFile→RadiologyInference, Photo→StorageOnly. Text Extraction: PdfPig (metin katmanlı PDF), IOcrProvider soyutlaması (Tesseract implementasyonu hazır, Ocr:Provider config; dev varsayılanı Stub — tessdata kurulumu gerekince Tesseract'a çevrilir). 73 birim testi. E2E: fo-dicom ile üretilmiş 3 gerçek DICOM (2 seri) + gerçek PDF → doğru gruplama (2 seri/3 kesit), doğru rotalar, PDF'ten metin çıkarıldı
  - Kalan (dilim 3+): resumable/chunked upload, DICOM Integrity'nin zorunlu tag kontrolü, OCR Score kriterinin kalite motoruna bağlanması

- [Feature] WP3 dilim 3 (WP3 kapandı): resumable upload + DICOM Integrity tag kontrolü + OCR Score + kriter ağırlıkları
  - Dosya: src/MedInsight.Application/{Documents/UploadDocuments.cs,Quality/**,Abstractions/Dicom}, src/MedInsight.Dicom/FoDicomMetadataReader.cs, docs/architecture/ingestion-pipeline.md (Resumable Yükleme bölümü eklendi), appsettings.json (Quality:Weights)
  - Not: Resumable = SHA-256 içerik hash dedup — kesilen batch tekrarında aynı dosya yeniden işlenmez, alreadyExisted döner (canlı doğrulandı: 2. batch'te mükerrer yok). DicomIntegrity artık PatientID/StudyDate/Modality zorunlu tag'lerini fo-dicom ile kontrol ediyor. OcrScoreCriterion: OCR güven skoru kaliteye bağlandı (Stub sağlayıcıda uygulanmaz). Kriterler async + ağırlıklar config'ten (DicomIntegrity=3, OcrScore=2 — doküman önceliklerine göre). 73 birim testi
  - Post-MVP'ye kalan: chunk bazlı tekil büyük dosya (tus benzeri), Missing Pages/Resolution/Contrast görüntü kriterleri

## 2026-07-25

- [Feature] WP4: AI Orkestrasyon (Hızır) + Sağlık Rotası
  - Dosya: src/MedInsight.AIOrchestration/** (7 katman: Intent/Planner/AgentSelector/ToolInvoker/MemoryContext/Reasoning/ResponseComposer + Guardrails + StubLlmClient + handler'lar), src/MedInsight.Domain/Cases/{HealthRoute,AiAnalysis,Case}.cs + Events/AiEvents.cs, src/MedInsight.Application/{HealthRoutes,Analyses}/**, Migrations/AddAiAnalysisAndHealthRoute
  - Not: Case açılışında ilk rota snapshot'ı (ADR-002 Git modeli: append-only zincir, PreviousVersionId, tek current). AIAnalysisRequested kalite geçişinde otomatik. Guardrails 3 kapı: confidence eşiği (Ai:ConfidenceThreshold=0.6, ADR-004 → DoctorReviewPriorityRaised), kapsam kontrolü (tanı/doz regex → zorunlu yönlendirme metni), kaynak izlenebilirliği (belgeye dayanmayan bulgu + ona dayanan tanı adayı elenir). ADR-010 domain'de zorlanıyor: OpenSourceImageModel bulgusu DifferentialDiagnosis'u besleyemez + zorunlu disclaimer. PII: modele yalnız klinik veri; belge içeriği yalnızca context alanında (prompt-injection savunması yapısal). ILlmClient soyutlaması — MVP'de deterministik StubLlmClient (tanı adayı üretmez), gerçek sağlayıcı config ile bağlanacak. Yeni endpoint'ler: GET cases/{id}/analyses, /health-route, /health-route/snapshots. 89 birim testi
  - Teknik not: StubLlmClient bölme hatası — Windows CRLF nedeniyle context section split başarısızdı, normalize edildi

- [Docs] Gerçek LLM entegrasyonu ertelenmiş iş olarak kayda alındı
  - Dosya: src/MedInsight.AIOrchestration/DependencyInjection.cs (TODO(llm-provider) — 3 adımlı talimat), docs/business/roadmap.md (WP-LLM paketi)
  - Not: ClaudeLlmClient : ILlmClient + Ai:Provider config anahtarı + secrets manager. WP8'den önce yapılacak.

- [Feature] WP5 dilim A: Doktor doğrulama (ADR-007) + ReviewerProfile + müsaitlik (ADR-009) + Doctor Matching (ADR-003)
  - Dosya: src/MedInsight.Domain/Identity/{Doctor,DoctorVerification,ReviewerProfile}.cs + Events/DoctorEvents.cs, src/MedInsight.Application/{Doctors,Admin,Matching}/**, src/MedInsight.Infrastructure/Repositories/DoctorRepository.cs, src/MedInsight.Api/Controllers/{DoctorsController,AdminController}.cs, Migrations/AddDoctorVerificationAndMatching
  - Not: Doktor Pending kayıt → belge+QR yükleme (QR parse admin'e ÖNERİ, otomatik onay yok) → admin approve/reject (approve'da Idempotency-Key ZORUNLU, 400 dönüyor). DoctorVerified event'i ReviewerProfile'ı otomatik açıyor. Müsaitlik: EffectiveStatus = ManualOverride ?? ComputedStatus; Computed asla Away üretmez; süresi dolan override yok sayılır. Matching: 5 faktör (Specialty=5, Location=3 nötr-MVP, Availability=3, Experience=1, ResponseSpeed=1 — Matching:Weights config), max 5 öneri, Away hariç, Busy "yoğun" etiketiyle seçilebilir, ScoreBreakdown açıklanabilir. Admin seed: config'ten (dev: admin@medinsight.local). 109 birim testi + canlı E2E (7 senaryo)
  - Dilim B (sırada): Consultation + SignalR mesajlaşma + tedavi planı (zorunlu snapshot) + AIAnalysisReviewed + escalation (ADR-014)

- [Feature] WP5 dilim B (WP5 kapandı): Consultation + SignalR + tedavi planı + AIAnalysisReviewed + escalation
  - Dosya: src/MedInsight.Domain/Cases/{Consultation,Case,AiAnalysis}.cs + Events/ConsultationEvents.cs, src/MedInsight.Application/Consultations/**, src/MedInsight.Api/{Hubs/ConsultationHub.cs,Controllers/ConsultationsController.cs}, Migrations/AddConsultationAndTreatment
  - Not: Konsültasyon: hasta (Manage) doğrulanmış doktoru davet eder → doktor Contribute üyesi, ActiveCaseCount event'le artar/azalır (ADR-009 bağlantısı). Mesaj event'i içerik TAŞIMAZ (gizlilik); canlı akış SignalR /hubs/consultations (JWT query token), REST geçmiş/fallback. Tedavi planı: invariant 2 — zorunlu Doctor snapshot'ı + DoctorReview→Treatment→(kontrol tarihi varsa) FollowUp. AIAnalysisReviewed → ReviewerProfile.RecordReview (Corrected için not zorunlu — Learning Loop). Escalation (ADR-014): otomatik koşul (High/Critical tanı + OpenSource bulgu) + manuel doktor talebi; MVP'de öncelik High + timeline notu, vendor çağrısı yok. 122 birim testi + canlı E2E (9 adım: konsültasyon→mesajlaşma→Corrected inceleme→ReviewerProfile 1/1.0→escalation 202→plan→rota v3 Doctor→FollowUp→sayaç 0)
  - Teknik borç: mesaj/not/plan içeriği için at-rest column-level şifreleme (security-architecture.md) — TODO(security) işaretli

- [Feature] WP7: Çapraz kesenler — Audit + Notification + Rate Limiting + OpenTelemetry
  - Dosya: src/MedInsight.Infrastructure/{Audit,Notifications}/**, src/MedInsight.Application/{Abstractions/Notifications,Notifications}/**, src/MedInsight.Api/{Program.cs,Controllers/{AdminController,NotificationsController}.cs}, Migrations/AddAuditAndNotifications (trigger dahil)
  - Not: AUDIT: açık-jenerik AuditEventHandler<T> — HER domain event otomatik audit kaydı (unutmak yapısal olarak imkansız, audit-service.md); Id=EventId (idempotent); aktör payload'dan çözülür; UPDATE/DELETE **DB trigger'ıyla** engelli (canlı doğrulandı: INSERT geçti, UPDATE/DELETE reddedildi); GET /api/v1/admin/audit-logs (Admin). NOTIFICATION: INotificationService + LogNotificationService (Simulated — TODO(notification-provider)); kanal kuralları: kritik→Push+SMS, doğrulama→Push+Email, diğer→Push; 6 abone (sınıflandırma hatası, AI tamam [Hızır persona metni], öncelik [ADR-004 hasta dalı], yeni mesaj [içeriksiz], doktor onay/ret); GET /api/v1/users/me/notifications. RATE LIMIT: global 300/dk, uploads eşzamanlı 10, messages 30/dk sliding, admin-approve 10/dk; 429 + Retry-After zorunlu (canlı: 31. mesaj → 429, Retry-After: 30). OTEL: AspNetCore+HttpClient+Npgsql tracing, MedInsight.Outbox ActivitySource (correlationId etiketli), log satırlarında traceId; OTLP exporter Otel:Endpoint config'iyle. 122 test
  - Not 2: TRUNCATE trigger'ı atlar (satır trigger'ı) — prod'da ayrı kısıtlı DB rolü gerekecek (bilinen sınırlama). Bildirim metni abone kenarında üretiliyor (event payload'ında hazır gelmiyor — bilinen MVP sapması, iletim katmanı jenerik kaldı)

- [Feature] WP6 dilim A: Radiology Inference Service — bağımsız Python/FastAPI mikroservisi (ADR-010)
  - Dosya: services/radiology-inference/** (FastAPI + pydicom, stub/monai backend soyutlaması, Dockerfile), docker-compose.yml (radiology-inference servisi, host 8100), src/MedInsight.Domain/Cases/ImageFinding.cs, src/MedInsight.Application/{Abstractions/Radiology,Radiology}/**, src/MedInsight.Infrastructure/{Radiology/HttpRadiologyInferenceClient.cs,Storage (presigned URL)}, Migrations/AddImageFindings
  - Not: Monorepo yaklaşımı — ayrı repo değil, services/ klasöründe kendi Dockerfile'ıyla. Kontrat: POST /inference {studyId, dicomSeriesUrls} → findings[{modelName, modelSource:OpenSource, outputType, rawOutput, disclaimer}]; disclaimer kontrattan çıkarılamaz (hem Python hem Domain tarafında zorlanıyor). ImageFinding, AiAnalysis'ten AYRI Case bileşeni — DifferentialDiagnosis'a yapısal olarak bağlanamaz, confidence mantığına girmez, arayüzde "Deneysel" blok (GET /api/v1/cases/{id}/image-findings). DicomStudyGrouped → presigned MinIO URL'leri → Python inference → bulgu + timeline. Escalation (ADR-014) iki bacak: analiz sonrası + bulgu sonrası kontrol. Servis kapalıysa pipeline etkilenmez. Dev topolojisi: Storage:PresignEndpoint=host.docker.internal:9500. 125 test + canlı E2E (3 DICOM → "MR çalışması işlendi: 3 kesit" + disclaimer)
  - MONAI gerçek modeli: TODO(radiology-model) — requirements-ml.txt + BraTS ağırlıkları + RADIOLOGY_BACKEND=monai; yeni açık kaynak model = yeni ADR
  - Dilim B (frontend ile birlikte): OHIF Viewer + minimal WADO-RS (ADR-012)

- [Feature] FE-1: Frontend başlangıcı — React + Vite + TS (ADR-017), kimlik + hasta akışı
  - Dosya: docs/adr/adr-017-frontend-stack.md, frontend/** (Vite React 19 TS + Tailwind v4 + TanStack Query + RHF/zod + react-router + @microsoft/signalr), backend: GET /api/v1/patients/me + CORS (Cors:Origins config)
  - Not: Takım React standartları uygulandı (feature klasörleri, axios interceptor, KEYS deseni). Sayfalar: login/kayıt, Hızır ana ekranı (Analiz §8: "Merhaba X / Ben Hızır" + devam eden vaka + CDSS uyarısı), vaka listesi/oluşturma, vaka detayı 6 sekme (Rota [current+versiyon zinciri], Belgeler [toplu yükleme + durum rozetleri], Hızır Analizi [persona mesajı + bulgular + AYRI amber "Deneysel" bloğu — ADR-010 UI kuralı], Doktorlar [öneri + skor dökümü + "atama değil" notu + konsültasyon başlat], Mesajlar [SignalR canlı + REST fallback], Timeline). Polling 5sn — pipeline ilerleyişi UI'a canlı yansıyor. Browser'da uçtan uca doğrulandı: kayıt→giriş→vaka→yükleme→DoctorReview rozeti+Hızır mesajı+deneysel blok ekran görüntüsüyle. TS derleme temiz, konsol hatasız
  - FE-2 (sırada): doktor paneli (inceleme kuyruğu, analiz onay/düzeltme, tedavi planı, müsaitlik), FE-3: admin + OHIF

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

## 2026-07-27

- [Feature] FE-2: Doktor paneli (backend uçları + React arayüzü)
  - Dosya: src/MedInsight.Application/Doctors/GetDoctorQueries.cs, src/MedInsight.Application/Cases/CloseReopenCase.cs, src/MedInsight.Application/Analyses/GetCaseAnalysesQuery.cs, src/MedInsight.Api/Controllers/{DoctorsController,CasesController}.cs, src/MedInsight.Infrastructure/Repositories/CaseRepository.cs, frontend/src/features/doctor/**, frontend/src/features/auth/RegisterDoctorPage.tsx, frontend/src/{main,AppLayout}.tsx, frontend/src/lib/types.ts
  - Not: Yeni uçlar — GET /doctors/me (profil+müsaitlik+doğrulama geçmişi), GET /doctors/me/cases (inceleme kuyruğu: aktif konsültasyon > ReviewPriority > tarih sıralı), POST /cases/{id}/close (doktor üye veya admin), POST /cases/{id}/reopen (Manage üyesi veya admin, gerekçe zorunlu). AiAnalysisDto'ya ReviewDecision/ReviewedByDoctorId/ReviewedAtUtc eklendi. Frontend: rol bazlı yönlendirme (Doctor → /doctor), doktor kayıt sayfası, doğrulama belgesi yükleme kartı (ADR-007 admin onayı bekleniyor durumları), müsaitlik toggle (ADR-009 manuel override + sisteme bırak), kuyruk listesi (Öncelikli rozeti), vaka detayında "Doktor Aksiyonları" sekmesi (AI onay/düzelt + zorunlu düzeltme notu, klinik not, tedavi planı + kontrol tarihi, ikinci görüş, konsültasyon tamamlama, FollowUp'ta vaka kapatma). Uçtan uca tarayıcıda doğrulandı: kayıt→doğrulama→onay→konsültasyon→analiz onayı→tedavi planı (Takipte)→kapatma (Kapalı)→hasta reopen (FollowUp). 125 test geçti.
  - Teknik not: Storage:Endpoint localhost→127.0.0.1 düzeltildi — wslrelay [::1]:9500'ü dinlediğinden MinIO upload'ları sessizce kayboluyordu, sınıflandırma DocumentClassificationFailed üretiyordu (postgres'teki ::1 sorununun aynısı).

- [Docs] Roadmap'e frontend paketleri ve netleşen öncelik sırası eklendi
  - Dosya: docs/business/roadmap.md
  - Not: FE-1 ✅, FE-2 ✅, sırada WP-LLM (maliyet notuyla) → Hızır chat → FE-3 → WP8; bilinçli ertelenenler listelendi (OHIF, Study Comparison ADR-013, Caregiver, MONAI+OCR).

- [Feature] FE-3: Admin paneli (doğrulama onay/red + belge görüntüleme + audit log)
  - Dosya: frontend/src/features/admin/**, frontend/src/{main,AppLayout}.tsx, frontend/src/lib/types.ts, src/MedInsight.Application/Admin/DoctorVerificationAdmin.cs, src/MedInsight.Api/Controllers/AdminController.cs
  - Not: Rol bazlı yönlendirme Admin → /admin. İki sekme: (1) Doktor Doğrulamaları — bekleyen başvurular QR çözüm önerisiyle listelenir, Onayla (Idempotency-Key otomatik) / Reddet (gerekçe zorunlu, doktora gösterilir), "Belgeyi görüntüle" JWT korumalı blob akışıyla yeni sekmede açar; (2) Audit Log — son 50 kayıt, entityId filtresi, Detay ile metadata JSON + correlationId. Yeni uç: GET /admin/doctor-verifications/{id}/document (API üzerinden stream; presigned URL tarayıcı için kullanılmadı — dev'de PresignEndpoint yalnızca konteyner ağına açık). Tarayıcıda canlı doğrulandı: onay → doktor Verified, red → doktor tarafında gerekçe görünür, audit'te DoctorVerified/DoctorVerificationRejected kayıtları. 125 test geçti.

## 2026-07-28

- [Feature] WP-LLM: Gemini LLM istemcisi (Ai:Provider ile seçilebilir)
  - Dosya: src/MedInsight.AIOrchestration/GeminiLlmClient.cs, DependencyInjection.cs, MedInsight.AIOrchestration.csproj, src/MedInsight.Api/{Program.cs,appsettings.json,MedInsight.Api.csproj}, tests/MedInsight.AIOrchestration.Tests/GeminiLlmClientTests.cs
  - Not: ILlmClient'ın Gemini implementasyonu (generativelanguage.googleapis.com generateContent, responseMimeType=application/json). Sağlayıcı seçimi Ai:Provider = Stub | Gemini (varsayılan Stub); model Ai:Gemini:Model (gemini-2.5-flash); anahtar Ai:Gemini:ApiKey yalnızca user-secrets'tan (UserSecretsId eklendi). Prompt-injection savunması korunur: belge içeriği yalnızca user içeriğine gider, sistem talimatları + çıktı sözleşmesi sabit. Ayrıştırma savunmacı: bozuk/şema dışı yanıt bulgu üretmez, güven eşiği altına düşer → ADR-004 doktor önceliği yükselir; geçersiz kaynak guid'i null → kaynak izlenebilirliği kapısı eler. 6 birim test (fence sıyırma, clamp, bozuk JSON, eksik alan). 131 test geçti. Guardrails/persona/pipeline değişmedi — TODO(llm-provider) kapatıldı.
  - Karar: Ürün sahibi maliyet nedeniyle Gemini'den başlamayı seçti (ücretsiz katman, sentetik test verisi). Gerçek hasta verisi ücretsiz katmandan GEÇEMEZ (Google ücretsiz katman verisini ürün geliştirmede kullanabilir); pilota kadar ücretli katman veya başka sağlayıcı (ClaudeLlmClient aynı desenle eklenir).
