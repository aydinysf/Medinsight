# Notification Engine

**Durum:** Onaylandı

## Sorumluluk

Domain event'lere subscribe olup kullanıcı tercihine göre push, SMS veya e-posta
kanalına yönlendirmek. Bildirimin **içeriğini üretmez** — sadece iletir.

## Akış

```
Domain Event (örn. AIAnalysisCompleted, HealthRouteSnapshotCreated,
              DoctorVerified, ConsultationMessageSent)
        │
        ▼
Notification Engine
        │
        ▼
NotificationPreference sorgusu (kullanıcı bazlı kanal tercihi)
        │
   ┌────┼────┐
   ▼    ▼    ▼
 Push  SMS  E-posta
```

## Kanal Seçim Kuralları

| Durum | Varsayılan Kanal |
|---|---|
| Genel bilgilendirme (doktor önerisi hazır) | Push |
| Kritik risk uyarısı | Push + SMS (ikisi birden) |
| Doktor doğrulama sonucu | Push + E-posta |
| Yeni mesaj | Push |

## Şema

```
NotificationPreference
- user_id PK
- push_enabled
- sms_enabled
- email_enabled
- critical_alerts_channel   (kritik uyarılar için özel tercih)

NotificationLog
- id PK
- user_id FK
- event_type
- channel
- sent_at
- delivery_status
```

## Fallback Mantığı (Bilinen Sınırlama)

MVP'de tek kanal fallback mantığı basit tutulmuştur: push başarısız olursa SMS'e
düşülmez, sadece loglanır. Kanal önceliklendirme motoru (push başarısız → SMS
dene → yine başarısız → e-postaya düş) Post-MVP'de eklenecektir (bkz. Blueprint
v3.0, Bölüm 23, Technical Debt).

## Neden İçerik Üretmez

Notification Engine'in "sadece ilet, üretme" sınırı bilinçlidir — bildirim
içeriği (örn. "doktor onayı bekliyor" mesajının tam metni) ilgili domain
(`../ai/hizir-personality.md`'deki dil kurallarına uygun olarak) tarafından
üretilir ve event payload'ında hazır gelir. Bu ayrım, Notification Engine'in
tıbbi/klinik dil kurallarından habersiz, tamamen jenerik bir iletim katmanı
olarak kalmasını sağlar.

## İlişkili Dosyalar

- Event kataloğu: `../domain/domain-events-catalog.md`
- Confidence tabanlı bildirim tetikleyicisi: `../ai/confidence-management.md`
