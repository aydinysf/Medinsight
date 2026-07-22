# Hızır — Overview

**Durum:** Onaylandı

## Tanım

Hızır, MedInsight'ın AI Agent'ıdır. Bir chatbot değil, **karar destek ajanı**dır.
Kesin tanı koymaz, reçete yazmaz, tedavi önermez. İşi, hastanın ham verisini
yapılandırılmış bir vakaya dönüştürmek ve bu vakayı doğru insana (doktor) en hazır
haliyle ulaştırmaktır.

## Görevler ve Sınırlar

| Görev | Sınır |
|---|---|
| Hastayı yönlendirme | Tanı koyamaz |
| Eksik veri tespiti ve talebi | Hangi tetkik yapılmalı diyemez |
| Ön analiz üretme | Confidence düşükse hastaya kesinlik iddia edilmez |
| Branş/doktor önerisi sunma | Atama yapmaz, sadece önerir |
| Sağlık rotasını güncelleme | Rotayı kapatamaz — o karar doktorun |

## Bu Klasördeki Diğer Dosyalar

| Dosya | İçerik |
|---|---|
| `hizir-personality.md` | Konuşma dili, empati, risk iletişimi, karakter sınırları |
| `ai-orchestration-flow.md` | Intent Detection → Planner → ... → Response teknik akışı |
| `ai-agent-ecosystem.md` | Hızır'ın orchestrator'a evrileceği çoklu-ajan geleceği |
| `medical-knowledge-graph.md` | AI'ın bilgi kaynağının graph yapısı (Post-MVP) |
| `learning-loop.md` | AI'ın doktor geri bildirimiyle zamanla iyileşmesi |
| `confidence-management.md` | Confidence skorunun hesaplanması ve eşik yönetimi |
| `guardrails-and-boundaries.md` | Teknik kapsam kontrolü, prompt injection savunması |

## İlgili Diğer Dosyalar

- Case içindeki AI bileşenleri: `../domain/case-aggregate-root.md`
  (`AIFindings`, `DifferentialDiagnosis`)
- Ana ekran deneyimi: `../wireframes/hizir-home-screen.md`
