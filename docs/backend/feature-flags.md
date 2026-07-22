# Feature Flags

**Durum:** Onaylandı

## Amaç

`git-strategy.md`'deki trunk-based modelin çalışabilmesi için, tamamlanmamış
özellikler `main`'e merge edilebilmeli ama production'da görünür olmamalıdır.

## Kullanım Alanları

| Senaryo | Örnek |
|---|---|
| Kademeli çıkış | Doctor Matching Engine'in yeni skorlama ağırlıkları önce %10 kullanıcıya |
| Post-MVP hazırlığı | AI Agent Ecosystem'in (bkz. `../ai/ai-agent-ecosystem.md`) ilk uzman ajanı kapalı flag ile geliştirilir |
| Acil kapatma | Guardrails'te anomali tespit edilirse (bkz. `../ai/guardrails-and-boundaries.md`) ilgili AI özelliği flag ile anında kapatılabilir |

## Flag Yaşam Döngüsü

Bir flag süresiz kalmamalıdır. Her flag, ya tam açılıp koddan flag kontrolü
temizlenir, ya da özellik iptal edilip kod silinir. Kalıcı flag'ler (örn.
"Hospital rolü aktif mi" gibi yapılandırma anahtarları) bu kurala tabi değildir,
onlar konfigürasyon olarak ayrı ele alınır.

## İlişkili Dosyalar

- Git stratejisi: `git-strategy.md`
- CI/CD: `ci-cd.md`
