# ADR-017: Frontend Yığını — React + Vite + TypeScript (SPA)

**Durum:** Kabul edildi
**Karar tarihi:** 2026-07-25, WP-FE başlangıcı

## Bağlam

Backend MVP'si tamamlandı (WP1–WP7 + WP6-A); Analiz Raporu §8 "AI-first"
kullanıcı deneyimini tanımlıyor (ana ekran = Hızır) ama repo'da frontend
mimarisi dokümanı yoktu. Takımın mevcut React standartları (feature-bazlı
klasörleme, TanStack Query, react-hook-form + zod, axios) veri noktasıdır.

## Karar

1. **React 19 + Vite + TypeScript**, SPA olarak `frontend/` klasöründe
   (monorepo — radyoloji servisiyle aynı yaklaşım). SSR/Next.js gerekmez:
   ürün, arama motoru görünürlüğü değil oturum-içi uygulama deneyimidir.
2. **Takım standartları uygulanır:** TanStack Query (sunucu durumu),
   axios (+ JWT interceptor), react-hook-form + zod (formlar),
   react-router-dom (yönlendirme), feature-bazlı klasör yapısı
   (`features/{auth,cases,doctors,admin}/{components,hooks,api.ts}`).
3. **Tailwind CSS v4** — hız ve tutarlılık; bileşen kitaplığı (shadcn vb.)
   ihtiyaç doğunca ayrı kararla eklenir.
4. **Gerçek zamanlılık:** `@microsoft/signalr` istemcisi, konsültasyon
   mesajlaşması için (`/hubs/consultations`).
5. **Dil:** UI metinleri Türkçe, kod/tanımlayıcılar İngilizce
   (naming-conventions.md kuralı).
6. **Rol bazlı yönlendirme:** Patient/Caregiver → Hızır ana ekranı ve vaka
   akışı; Doctor → doktor paneli; Admin → doğrulama kuyruğu. JWT'deki `role`
   claim'i tek kaynak.
7. Fazlama: **FE-1** kimlik + hasta akışı (vaka, belge, rota, timeline),
   **FE-2** doktor paneli (inceleme kuyruğu, konsültasyon, tedavi planı),
   **FE-3** admin + OHIF viewer entegrasyonu (WP6-B / ADR-012 ile birleşir).

## Alternatifler

1. **Next.js** — Reddedildi (MVP için). SSR/SEO ihtiyacı yok; Vite daha
   hafif ve takım şablonlarıyla uyumlu (`VITE_API_URL` konvansiyonu).
2. **Blazor** — Reddedildi. Takım yetkinliği ve ekosistem tercihi React;
   OHIF (ADR-012) da React tabanlıdır, aynı yığında birleşir.

## Sonuç

- `frontend/` klasörü; `VITE_API_URL` ile API adresi.
- API'ye geliştirme CORS politikası eklenir (yalnız dev origin'leri).
- OHIF Viewer entegrasyonu (ADR-012) FE-3'te bu SPA'ya gömülür.

## İlgili Dosyalar

- `../adr/adr-012-dicom-viewer-choice.md`
- `../backend/naming-conventions.md`
- Analiz Raporu §8 (Hızır-first ana ekran)
