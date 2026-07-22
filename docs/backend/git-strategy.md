# Git Strategy

**Durum:** Onaylandı

## Branch Modeli

Trunk-based development, kısa ömürlü feature branch'lerle:

```
main  ─────●───────●───────●───────●──→   (her zaman deploy edilebilir)
             \     /  \     /
              ●───●    ●───●
           feature/x  feature/y
```

- `main` her zaman yeşil (CI geçen) durumda tutulur.
- Feature branch'ler kısa ömürlü olmalı (birkaç gün, haftalar değil).
- Uzun süren geliştirmeler feature flag ile `main`'e erken merge edilir (bkz.
  `feature-flags.md`).

## Commit Mesajı Formatı

```
<tip>(<kapsam>): <özet>

<açıklama — neden bu değişiklik yapıldı>

Refs: ADR-00X (varsa)
```

Tip: `feat`, `fix`, `refactor`, `docs`, `test`, `chore`.
Kapsam: bounded context adı (`case`, `health-route`, `ai-orchestration` vb.)

## Domain Değişikliği Kuralı

Bir commit, bir domain kuralını değiştiriyorsa (örn. yeni bir invariant, yeni
bir state geçişi), commit mesajı ilgili `docs/domain/` dosyasının da bu commit'te
güncellendiğini belirtmelidir. Kod ve doküman aynı commit'te değişmelidir —
"sonra dokümanı güncelleriz" kabul edilmez (kök `README.md` disiplini).

## Pull Request Kuralı

Her PR, `coding-standards.md`'deki kod inceleme kriterlerinin dördünü de
karşılamalıdır: domain dokümanı güncel mi, ADR gerekiyorsa yazıldı mı, domain
katmanı saf mı, yetki kontrolü doğru mu.

## İlişkili Dosyalar

- CI/CD: `ci-cd.md`
- Versioning: `versioning.md`
