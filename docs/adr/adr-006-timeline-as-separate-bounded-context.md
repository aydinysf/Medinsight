# ADR-006: Timeline Ayrı Bir Bounded Context

**Durum:** Kabul edildi

## Bağlam

Timeline başlangıçta Case'in basit bir alt tablosu olarak düşünüldü. Ancak sorgu
deseni incelendiğinde, diğer motorlardan temelde farklı olduğu görüldü: diğer
motorlar "ne yapılmalı" sorusuna cevap üretirken, Timeline sadece "ne zaman ne
oldu" sorusuna cevap verir ve hiçbir iş kararı üretmez.

## Karar

Timeline, kendi bounded context'ine sahip bir **Timeline Engine** olarak
tasarlandı — event sourcing'e yakın bir append-only kayıt motoru. Case Engine,
Health Route Engine, AI Analysis ve Consultation'daki her olay Timeline Engine'e
akar; Timeline bu olayları üretmez, sadece toplar ve sıralı sunar.

## Alternatifler

1. **Timeline'ı Case'in basit bir alt tablosu olarak bırakmak** — Reddedildi.
   Her yeni event türü eklendiğinde Case aggregate'inin şemasını değiştirmek
   gerekirdi; bu, Case'i gereksiz yere büyütür ve Timeline'ın kendine özgü
   sorgu optimizasyonlarını (örn. zaman aralığı bazlı sayfalama) engellerdi.

2. **Genel bir "ActivityLog" tablosu, tüm sistem için ortak** — Reddedildi.
   Audit Service zaten bu amaca hizmet ediyor ama farklı bir amaçla (güvenlik/
   uyumluluk); Timeline kullanıcıya gösterilen bir deneyim, Audit Log ise
   iç/regülasyon amaçlı. İkisini birleştirmek erişim kontrolü karmaşıklığı
   yaratırdı.

## Sonuç

- Timeline Engine, event subscriber olarak çalışır — kendi write modeli yoktur,
  sadece dinlediği event'leri append-only kaydeder.
- Bu ayrım ileride Timeline'ın bağımsız ölçeklendirilmesini (örn. ayrı bir
  event store) mümkün kılar.

## İlgili Dosyalar

- `../architecture/timeline-service.md`
- `../domain/event-storming.md`
