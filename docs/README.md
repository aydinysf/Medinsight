# MedInsight — Living Documentation

Bu repo, MedInsight'ın tek gerçek kaynağıdır (single source of truth).

## Disiplin

> Kod, blueprint'i takip eder. Blueprint kodu değil.

Her yeni mimari karar önce `docs/adr/` altında bir ADR olarak yazılır, sonra ilgili
`docs/domain/` veya `docs/architecture/` dosyası güncellenir, ancak ondan sonra kod yazılır.

## Yapı

| Klasör | İçerik |
|---|---|
| `docs/business/` | Vizyon, problem, persona, roadmap — iş tarafı |
| `docs/architecture/` | Katmanlar, bounded context'ler, güvenlik, API |
| `docs/domain/` | Case merkezli iş kuralları, event modeli, ERD |
| `docs/ai/` | Hızır, AI orchestration, knowledge graph, learning loop |
| `docs/backend/` | Kod standartları, CI/CD, git stratejisi |
| `docs/frontend/` | Tasarım sistemi, ekranlar |
| `docs/mobile/` | Mobil mimari |
| `docs/adr/` | Architecture Decision Records |
| `docs/uml/` | UML diyagram kaynakları |
| `docs/wireframes/` | Ekran taslakları |
| `docs/api/` | Endpoint ve event contract referansı |

Tam dosya listesi için `docs/MANIFEST.md`.

## Word/PDF üretimi

Word ve PDF çıktıları bu Markdown dosyalarından **otomatik üretilir**, elle düzenlenmez.
MedInsight_Blueprint.docx (v3.0), bu repo'nun donmuş bir anlık görüntüsüdür (snapshot) —
bu tarihten sonraki her değişiklik önce buraya, sonra oradan üretilen çıktıya yansır.
