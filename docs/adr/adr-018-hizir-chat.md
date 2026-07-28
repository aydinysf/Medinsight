# ADR-018: Hızır Chat — Vaka Kapsamlı Senkron Sohbet

## Durum

Kabul edildi — 2026-07-28

## Bağlam

Analiz Raporu §8 Hızır'ı hastanın "sağlık yolculuğunda yol arkadaşı" olarak tanımlar.
WP-LLM ile gerçek LLM sağlayıcısı bağlandı (Ai:Provider); MVP'de Hızır tek yönlüydü
(analiz mesajı + karşılama). Hastanın Hızır'a serbestçe soru sorabilmesi gerekiyor.

## Karar

1. **Vaka kapsamlı sohbet.** Chat, `POST /cases/{id}/hizir-chat` ile vakaya bağlıdır.
   Gerekçe: PII-minimize bağlam (MemoryContextBuilder) ve kaynak izlenebilirliği
   vaka üzerinden doğal olarak sağlanır; vakasız "genel sohbet" kapsam dışıdır (Post-MVP).
2. **Senkron istek/yanıt.** Analizden farklı olarak sohbet outbox'tan geçmez; LLM
   çağrısı HTTP isteği içinde yapılır (2-5 sn kabul edilebilir). Domain event
   üretilmez — sohbet trafiği timeline/audit'i boğmamalıdır. KVKK denetimi için
   mesajlar `hizir_chat_messages` tablosunda kalıcıdır (append-only davranış,
   Case aggregate'i altında).
3. **`ILlmClient.ChatAsync` genişletmesi.** Analizin JSON çıktı sözleşmesinden
   bağımsız, geçmişli (history) serbest metin sohbeti için ikinci arayüz metodu.
   Prompt-injection savunması korunur: belge içeriği ve kullanıcı mesajları asla
   sistem talimatına karışmaz; bağlam ilk user dönüşünde `[BAĞLAM]` etiketiyle gider.
4. **Guardrail'ler sohbete de uygulanır.** Yanıt `Guardrails.EnforceScope`'tan geçer
   (tanı/doz kalıpları → zorunlu yönlendirme metni). Sistem talimatı CDSS sınırlarını
   (tanı yok, doz yok, acil belirtide 112'ye yönlendir) persona diliyle içerir.
5. **Erişim ve limit.** Yalnızca vaka üyeleri (+ Admin); mevcut `messages` rate
   limit politikası uygulanır. Kapalı vakada sohbet devam edebilir (hasta geçmişe
   dönük soru sorabilir).

## Sonuçlar

- Case aggregate'ine `HizirChatMessage` koleksiyonu eklenir (ADR-001 ile uyumlu:
  tek aggregate root). Migration: `AddHizirChatMessages`.
- Stub sağlayıcıda sohbet deterministik yer tutucu yanıt verir (dev/test).
- Sohbet içeriği, konsültasyon mesajları gibi column-level şifreleme teknik borcuna
  dahildir (security-architecture.md, WP8).
- Post-MVP: vakasız genel sohbet, streaming yanıt (SSE), çok dilli destek.
