# Versioning

**Durum:** Onaylandı

## API Versiyonlama

`/api/v1` prefix'i ile (bkz. `../architecture/api-design.md`). Kırıcı değişiklik
gerektiğinde `/api/v2` açılır; v1 belirli bir süre paralel desteklenir.

## Domain Event Versiyonlama

Bir domain event'in payload şeması değiştiğinde (`../domain/domain-events-catalog.md`),
event adına versiyon eklenmez — bunun yerine yeni alanlar **her zaman opsiyonel**
eklenir (backward compatible). Zorunlu alan değişikliği gerekiyorsa, yeni bir
event türü tanımlanır (örn. `AIAnalysisCompletedV2`), eskisi belirli bir süre
paralel yayınlanır.

## AI Model / Prompt Versiyonlama

`../ai/confidence-management.md` ve `../domain/case-aggregate-root.md`'de
belirtildiği gibi, her `AIAnalysis` kaydı `ModelVersion` ve `PromptVersion`
taşır. Bu versiyon bilgisi asla silinmez — geçmiş bir analiz her zaman hangi
model/prompt ile üretildiğini gösterebilmelidir (klinik sorumluluk ve
Learning Loop analizi için).

## Semantic Versioning (Uygulama Sürümü)

`MAJOR.MINOR.PATCH` — MAJOR sadece bir ADR eşliğinde artırılır (mimari kırıcı
değişiklik).

## İlişkili Dosyalar

- Git stratejisi: `git-strategy.md`
- ADR süreci: `../adr/`
