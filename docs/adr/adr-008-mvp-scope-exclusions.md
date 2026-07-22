# ADR-008: MVP Kapsam Dışı Bırakılanlar

**Durum:** Kabul edildi

## Bağlam

MedInsight'ın nihai vizyonu geniş (Hospital modeli, ödeme sistemi, AI Agent
Ecosystem, e-Nabız entegrasyonu). Bunların hepsini MVP'ye dahil etmek, teslim
süresini ve karmaşıklığı kontrolsüz büyütürdü.

## Karar

Aşağıdakiler bilinçli olarak MVP dışında bırakıldı:

- Canlı görüntülü görüşme
- Sigorta entegrasyonu
- Hastane kurumsal paneli (Hospital/Department/Clinic hiyerarşisi)
- e-Nabız entegrasyonu, PACS entegrasyonu
- Ödeme sistemi (Doctor Economy'nin ödeme akışı)
- Araştırma veri seti, klinik çalışma modülü
- AI Agent Ecosystem (çoklu uzman ajan mimarisi) — Hızır tek ajan olarak kalır
- Medical Knowledge Graph — Post-MVP'de basit bir havuzdan graph yapısına evrilecek

## Gerekçe (YAGNI İlkesi)

Her biri için ayrı bir gerekçe var, ama ortak tema şu: bu özellikler gerçek bir
kullanıcı/ortak geri bildirimi olmadan tasarlanırsa spekülatif kalır. Örnek:
Hospital modelini bugünden tam şema ile kurmak, gerçek bir hastane ortağı
olmadan hangi alanların gerekli olduğunu tahmin etmek anlamına gelir.

## Sonuç

- Domain modelinde bu genişlemelere yer açan tasarım kararları (örn. Case'in
  sekiz alt bileşenli yapısı, Timeline'ın ayrı bounded context olması) bugünden
  alınmıştır — böylece MVP sonrası genişleme mimari refactor gerektirmeyecektir.
- Her kapsam dışı madde, Blueprint v3.0'da ayrı bir bölümde (Hastane Modeli,
  Doktor Ekonomisi, AI Agent Ecosystem) belgelenmiştir; "unutulmuş" değil,
  "ertelenmiş"tir.

## İlgili Dosyalar

- `../business/roadmap.md`
- Blueprint v3.0, Bölüm 6 (Kapsam ve Roller)
