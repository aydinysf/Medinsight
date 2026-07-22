# MedInsight Documentation Repository — Manifest

Bu repo, MedInsight Blueprint v3.0'ın (Word) yerini alacak **yaşayan dokümantasyon** kaynağıdır.
Word/PDF artık ana kaynak değil; bu Markdown dosyalarından üretilen bir **çıktıdır**.

Kural: Kod, buradaki dokümanı takip eder. Doküman kodu değil.

---

## docs/business/  (11 dosya)
Ürünün "neden"i — yatırımcı ve iş tarafı için.

- executive-summary.md
- problem-statement.md
- vision-mission.md
- value-proposition.md
- competitive-analysis.md
- personas.md
- user-journey.md
- health-journey-model.md
- doctor-economy.md
- hospital-model.md
- roadmap.md

## docs/architecture/  (10 dosya)
Sistemin "nasıl"ı — katmanlar, bounded context'ler, çapraz kesen kaygılar.

- layered-architecture.md
- bounded-contexts-overview.md
- timeline-service.md  ← Madde 6: ayrı servis, event sourcing'e yakın
- notification-engine.md
- audit-service.md
- security-architecture.md
- api-design.md
- rate-limiting-idempotency.md
- observability.md  ← Madde 11: OpenTelemetry
- scalability-and-caching.md  ← Madde 11
- radiology-inference-service.md  ← Açık kaynak görüntü modeli, Python mikroservisi (MVP, bilgilendirici katman)
- ingestion-pipeline.md  ← 800+ dosya sınıflandırma, DICOM gruplama, yönlendirme
- text-extraction-service.md  ← OCR sağlayıcı soyutlaması
- dicom-viewer.md  ← Açık kaynak render motoru + MedInsight bağlam katmanı

## docs/domain/  (12 dosya)
Case merkezli iş kuralları.

- case-aggregate-root.md  ← Madde 1: 14 alt bileşenli genişletilmiş yapı
- document-quality-engine.md  ← Madde 5: 10 kalite kriteri
- health-route-versioning.md  ← Madde 7: Git benzeri versiyon modeli
- doctor-matching-engine.md
- doctor-availability.md  ← Doktor uygunluk durumu: otomatik hesap + manuel override
- doctor-verification.md
- consultation-model.md
- reviewer-profile.md
- event-storming.md  ← Madde 9: uçtan uca event akışı
- study-comparison.md  ← Longitudinal karşılaştırma: metin sentezi + deneysel ölçüm ayrımı
- domain-events-catalog.md
- case-lifecycle-state-machine.md
- erd-identity-case.md
- erd-ai-clinical.md

## docs/ai/  (8 dosya)
Hızır ve AI ekosistemi.

- hizir-overview.md
- hizir-personality.md  ← Madde 3: karakter, empati, risk iletişimi
- ai-orchestration-flow.md  ← Madde 4: Intent Detection → Planner → Agent Selection → Tool Calling → Memory → Reasoning → Response
- ai-agent-ecosystem.md
- medical-knowledge-graph.md  ← Madde 2: havuzdan graph'a yükseltme
- learning-loop.md
- confidence-management.md
- guardrails-and-boundaries.md

## docs/backend/  (10 dosya)
Madde 11'in tamamı — mühendislik disiplini.

- tech-stack.md
- coding-standards.md
- naming-conventions.md
- folder-structure.md
- git-strategy.md
- ci-cd.md
- versioning.md
- feature-flags.md
- database-migrations.md
- error-handling.md

## docs/frontend/  (5 dosya)

- design-system.md
- patient-app.md
- doctor-dashboard.md  ← Madde 8: KPI'lar eklendi
- admin-panel.md
- notification-ux.md

## docs/mobile/  (2 dosya)

- mobile-architecture.md
- offline-and-sync.md

## docs/adr/  (8 dosya)
Madde 10 — Architecture Decision Records.

- adr-001-case-as-aggregate-root.md
- adr-002-health-route-snapshot-versioning.md
- adr-003-doctor-matching-scoring-model.md
- adr-004-confidence-threshold-branching.md
- adr-005-doctor-economy-commission-model.md
- adr-006-timeline-as-separate-bounded-context.md
- adr-007-qr-based-doctor-verification.md
- adr-008-mvp-scope-exclusions.md
- adr-009-doctor-availability-model.md  ← Otomatik hesap + manuel override + Away sınırı
- adr-010-open-source-radiology-model-mvp.md  ← Açık kaynak görüntü modeli, karar dışı bilgilendirici katman
- adr-011-ocr-provider-abstraction.md  ← OCR sağlayıcı seçimi ertelendi, soyutlama ile
- adr-012-dicom-viewer-choice.md  ← Açık kaynak viewer + kendi bağlam katmanı
- adr-013-longitudinal-comparison-model.md  ← Hibrit tetikleme + ikili güven katmanı
- adr-014-third-party-escalation-trigger.md  ← Açık kaynaktan onaylı servise geçiş tetikleyicisi

## docs/uml/  (6 dosya)

- use-case-diagrams.md
- class-diagram.md
- sequence-diagram.md
- activity-diagram.md
- state-diagram.md
- erd-full.md

## docs/api/  (4 dosya)

- endpoints-overview.md
- event-contracts.md
- auth-and-security.md
- webhooks-and-integrations.md

## docs/wireframes/  (3 dosya)

- hizir-home-screen.md
- doctor-dashboard-wireframe.md
- patient-case-timeline-wireframe.md

---

**Şu an tamamlanan: 61 dosya** (domain 15, adr 14, ai 8, architecture 14, backend 10,
MANIFEST 1). "800 dosya senaryosu" gap analizindeki 7 boşluğun tamamı kapatıldı.
Kalan: frontend, mobile, uml, wireframes, api klasörleri.
