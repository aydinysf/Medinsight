# Audit Service

**Durum:** Onaylandı

## Sorumluluk

Sistemdeki her kritik aksiyonu (belge silme, tedavi planı yazma, doktor onaylama,
guardrails ihlali denemesi) değiştirilemez biçimde kaydetmek. Hiçbir iş kararı
üretmez — sadece kayıt tutar.

## Timeline'dan Farkı

Timeline (`timeline-service.md`), **kullanıcıya gösterilen** bir deneyimdir — "bu
vakada ne oldu" sorusuna cevap verir. Audit Service, **iç/regülasyon amaçlı**dır —
"kim, ne zaman, hangi yetkiyle, neyi değiştirdi" sorusuna cevap verir. İkisini
birleştirmek erişim kontrolü karmaşıklığı yaratırdı (bkz.
`../adr/adr-006-timeline-as-separate-bounded-context.md` "Alternatifler").

## Şema

```
AuditLog
- id PK
- user_id FK
- action                (örn. "document.delete", "treatment_plan.create")
- entity_type
- entity_id
- timestamp
- ip_address
- metadata               (jsonb — aksiyona özel ek bilgi)
```

## Değiştirilemezlik Garantisi

`AuditLog` tablosu, uygulama seviyesinde UPDATE/DELETE izni olmayan bir rol
kullanır — bu sadece bir konvansiyon değil, veritabanı seviyesinde `GRANT`
kısıtlamasıyla uygulanır. Hiçbir servis hesabı bu tabloyu güncelleyemez veya
silemez.

## Kim Yazar

Audit kaydı, doğrudan uygulama kodundan değil, event handler'lar üzerinden
eklenir — yani her domain event, aynı zamanda bir audit kaydı üretme fırsatıdır.
Bu, geliştiricinin "audit loglamayı unutmasını" yapısal olarak imkansız kılar,
çünkü audit yazımı iş mantığının bir parçası değil, event altyapısının bir
parçasıdır.

## KVKK İlişkisi

Bu servis, KVKK denetiminde "kim, ne zaman, neye erişti" sorusuna kesin cevap
veren tek kaynaktır (bkz. `security-architecture.md`). Bu nedenle audit
kayıtlarının kendisi de erişim kontrolüne tabidir — sadece admin rolü ve
regülasyon denetimi amaçlı özel bir salt-okunur rol bu tabloyu sorgulayabilir.

## İlişkili Dosyalar

- Güvenlik mimarisi: `security-architecture.md`
- Timeline farkı: `timeline-service.md`
- Guardrails ihlali kaydı: `../ai/guardrails-and-boundaries.md`
