# ADR-005: Doktor Ekonomisi — Platform Komisyonu Modeli

**Durum:** Kabul edildi
**Karar tarihi:** Blueprint v3.0 oturumu

## Bağlam

MedInsight sadece hastalar için değil, doktorlar için de bir platform. Doktorun
platforma gelme motivasyonu ve gelir modeli netleştirilmeliydi.

## Karar

Platform komisyonu modeli benimsendi: doktor kendi konsültasyon ücretini serbestçe
belirler, MedInsight bu ücret üzerinden bir yüzde komisyon alır. Ödeme akışı
(escrow veya direkt) MVP kapsamı dışında bırakıldı — Post-MVP'de ele alınacak.
Doktor puanlaması MVP'de sadece iç kullanım amaçlıdır (ReviewerProfile ağırlığı);
herkese açık profil puanı Post-MVP'dedir.

## Alternatifler

1. **Konsültasyon başına sabit ücret** — Reddedildi. Doktorlar arası tecrübe ve
   branş farkını yansıtmaz; yüksek tecrübeli doktorları platforma çekmede
   dezavantaj yaratır.

2. **Aylık üyelik (sınırsız vaka)** — Reddedildi. Düşük hacimli doktorlar için
   adaletsiz; platform tarafında gelir öngörülebilirliği sağlasa da doktor
   tarafında benimsenme riski yüksek.

## Sonuç

- Komisyon **oranı** bu ADR'de belirlenmedi — pazar araştırmasıyla ayrıca
  kesinleştirilecek. Bu bilinçli bir açık nokta, uydurma bir sayı konulmadı.
- Ödeme sisteminin MVP dışında tutulması, platformun önce doktor-hasta
  eşleşmesinin ve klinik akışın güvenilirliğini kanıtlamasını sağlıyor.

## İlgili Dosyalar

- `../business/doctor-economy.md`
- `../domain/reviewer-profile.md`
