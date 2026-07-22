# Rate Limiting ve Idempotency

**Durum:** Onaylandı

## Rate Limiting

Sağlık verisi taşıyan bir API için rate limiting iki farklı amaca hizmet eder:
kötüye kullanımı önlemek ve meşru ama yoğun trafiği (800+ dosya toplu yükleme)
boğmadan yönetmek.

| Endpoint Grubu | Limit Stratejisi | Gerekçe |
|---|---|---|
| `POST /cases/{id}/documents` | Kullanıcı bazlı yüksek limit + eşzamanlı upload sayısı sınırlı (örn. 10) | Toplu yükleme meşru senaryo; istek sayısı değil eşzamanlılık asıl risk |
| `GET` endpoint'leri | IP + kullanıcı bazlı standart limit | Normal kullanım yüksek limit gerektirmez |
| `POST /consultations/{id}/messages` | Kullanıcı bazlı orta limit + burst koruması | Mesajlaşma insan hızında olur |
| `POST /admin/doctor-verifications/{id}/approve` | Admin bazlı düşük limit | Az sayıda admin var, anomali kolay yakalanır |

429 yanıtlarında `Retry-After` header'ı zorunludur.

## Idempotency

`Idempotency-Key` header'ı ile aynı isteğin tekrar gönderilmesi durumunda işlem
tekrar çalıştırılmaz; ilk isteğin sonucu döndürülür.

```
POST /api/v1/cases/{caseId}/documents
Idempotency-Key: 3f2504e0-4f89-11d3-9a0c-0305e82c3301
```

| Endpoint | Idempotency Zorunlu mu? | Neden |
|---|---|---|
| `POST /cases` | Hayır (opsiyonel) | Yanlışlıkla iki vaka açılması düşük risk |
| `POST /documents` (toplu) | Evet | Ağ kesintisinde tekrar deneme olası |
| `POST /treatment-plan` | Evet, zorunlu | Health Route snapshot bütünlüğü kritik |
| `POST /doctor-verifications/{id}/approve` | Evet, zorunlu | Çift onay/red audit log'unu bozar |

## Idempotency'nin Çözemediği Şey: Race Condition

Idempotency key'ler yalnızca **aynı isteğin** tekrarını çözer; farklı isteklerin
yarış durumunu çözmez. Örnek: iki admin aynı anda aynı doktoru onaylamaya
çalışırsa, bu optimistic concurrency (`row_version` alanı) ile ayrı ele alınmalıdır.

## İlişkili Dosyalar

- API tasarımı: `api-design.md`
- Health Route bütünlüğü: `../domain/health-route-versioning.md`
