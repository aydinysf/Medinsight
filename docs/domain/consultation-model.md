# Consultation Model

**Bounded Context:** Case Aggregate (alt bileşen)
**Durum:** Onaylandı

## Amaç

Bir doktorun bir Case'e dahil olduğu süreci — mesajlaşma, AI analiz incelemesi,
klinik not ve tedavi planı üretimi — tek bir tutarlı model altında toplamak.

## Şema

```
Consultation
- Id
- CaseId
- DoctorId
- Status          (Pending | Active | Completed)
- StartedAt
- CompletedAt

Message
- Id
- ConsultationId
- SenderUserId
- Content           (şifreli saklanır)
- SentAt

ClinicalNote
- Id
- ConsultationId
- Content
- CreatedAt

Treatment (Case'in doğrudan alt bileşeni, Consultation üzerinden üretilir)
- Id
- CaseId
- ConsultationId
- Description
- CreatedAt
```

## Gerçek Zamanlılık

Mesajlaşma SignalR hub üzerinden gerçek zamanlı yürür; REST endpoint'i sadece geçmiş
mesaj sorgusu ve bağlantı kesintisi durumunda fallback için kullanılır.

## Tedavi Planı — Health Route Tetikleyicisi

`Treatment` oluşturulduğunda, bu **zorunlu olarak** bir `HealthRouteSnapshot`
üretir (bkz. `health-route-versioning.md`). Bu bağlantı Case invariant'larından
biridir — tedavi planı olup da rotası güncellenmeyen bir Case olamaz.

## Yetki Sınırları

Doktor `Consultation` üzerinden mesaj gönderebilir, klinik not yazabilir, tedavi
planı oluşturabilir — ancak Case'in idari durumunu (Draft, Closed) değiştiremez.
Bu ayrımın gerekçesi `case-aggregate-root.md` içinde detaylandırılmıştır.

## İlişkili Dosyalar

- Case içindeki konumu: `case-aggregate-root.md`
- Doktor güven ağırlığı: `reviewer-profile.md`
- Doktor dashboard: `../frontend/doctor-dashboard.md`
