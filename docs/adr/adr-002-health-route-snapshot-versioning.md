# ADR-002: Health Route Snapshot Tabanlı Versiyonlama

**Durum:** Kabul edildi
**Tarih:** v1.1 tasarımı, v3.0'da Git modeline genişletildi

## Bağlam

İlk tasarımda HealthRoute tek satırlık mutable bir kayıttı (`CurrentStatus`,
`NextStep`). Bu, "bu karar ne zaman ve kim tarafından değiştirildi" sorusuna cevap
veremiyordu — sağlık verisinde denetlenebilirlik zorunluluğuyla çelişiyordu.

## Karar

HealthRoute, append-only bir `HealthRouteSnapshot` geçmişi ile desteklenen bir
current read model'e dönüştürüldü. Her yeni tetikleyici (AI analizi, doktor kararı,
hasta kararı) yeni bir snapshot üretir; hiçbir snapshot üzerine yazılmaz veya
silinmez. Bu yapı Git'in commit modeline benzetilmiştir (bkz.
`../domain/health-route-versioning.md`).

## Alternatifler

1. **Mutable tek satır + ayrı audit log** — Reddedildi. Audit log genellikle
   "yan" bir kayıt olarak ele alınır ve iş mantığı onun üzerinden çalışmaz; bu
   durumda "önceki karara dön" gibi bir işlem audit log'dan veri kurtarmayı
   gerektirirdi, ki bu kırılgan bir tasarımdır.

2. **Her değişiklik için tam branch/merge modeli** — Reddedildi (bkz. gerekçe
   `../domain/health-route-versioning.md` "Branch Senaryosu" bölümü). Sağlık
   verisinde çatallanmış gerçeklik kabul edilemez; doğrusal geçmiş yeterli ve
   daha güvenlidir.

## Sonuç

- `(case_id, created_at)` composite index zorunlu — timeline sorgusu bu tabloya
  sık erişir.
- Tablo büyüklüğü zamanla artacaktır; arşivleme/partition stratejisi henüz
  uygulanmamıştır (bkz. Blueprint v3.0 Bölüm 23, Technical Debt).

## İlgili Dosyalar

- `../domain/health-route-versioning.md`
- `../domain/domain-events-catalog.md` (`HealthRouteSnapshotCreated`)
