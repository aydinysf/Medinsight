# ADR-014: Üçüncü Parti Radyoloji AI'a Geçiş Tetikleyicisi

**Durum:** Kabul edildi
**Karar tarihi:** Repo dokümantasyon oturumu

## Bağlam

ADR-010, açık kaynak görüntü modelini MVP'de bilgilendirici bir katman olarak
kabul etti ve bir çıkış kapısı bıraktı: "eğer bu yeterli değilse üçüncü parti
onaylı servise geçilmeli." Ancak *hangi durumda, hangi vaka için* bu geçişin
tetikleneceği tanımlanmamıştı. ADR-008 zaten üçüncü parti vendor entegrasyonunu
Post-MVP'ye bırakmıştı — bu ADR, MVP'de tetikleme *mantığının* nasıl
kurulacağını, gerçek entegrasyonun ise nasıl ertelendiğini netleştirir.

## Karar

**Tetikleme koşulu (sistem otomatik önerir, otomatik karar vermez):**

```
EscalationSuggested tetiklenir eğer:
  DifferentialDiagnosis.RiskLevel ∈ {High, Critical}
  VE
  Case'te en az bir AIFindings.Source = OpenSourceImageModel bulgusu var

VEYA

  Doktor, risk seviyesinden bağımsız olarak herhangi bir zamanda
  manuel "ikinci görüş iste" talebinde bulunur
```

**MVP davranışı:** `EscalationSuggested` sonrası doktor onaylarsa, gerçek bir
üçüncü parti API çağrısı yapılmaz (ADR-008 kapsam kararı gereği). Bunun yerine:
Case'in doktor önceliklendirmesi en üst seviyeye çıkar ve Case'e "üçüncü parti
inceleme talep edildi" notu düşülür — bu, doktora "bu vaka için ek bir uzman
görüşü faydalı olabilir" sinyalini manuel süreçle (örn. hastaneye yönlendirme)
karşılamasını sağlar.

**Post-MVP davranışı:** Aynı `EscalationSuggested` → doktor onayı akışı, gerçek
bir vendor API çağrısına bağlanır. Sonuç, `AIFindings.Source = ValidatedImageModel`
olarak Case'e eklenir (bu değer, `case-aggregate-root.md`'de zaten öngörülmüştü).

## Neden Otomatik Değil

Doctor Matching Engine (ADR-003) ve Study Comparison (ADR-013) ile aynı
felsefe: maliyet ve klinik sorumluluk doğuran bir aksiyon, sistem tarafından
sessizce tetiklenemez. Üçüncü parti servis çağrısı hem ücretli olacak (Post-MVP)
hem de "bu vaka ek incelemeye değer" gibi klinik ağırlığı olan bir sinyal
taşıyor — bu karar doktorda kalmalı.

## Alternatifler

1. **Sadece manuel (otomatik öneri olmadan)** — Reddedildi. Yüksek riskli bir
   vakada doktorun bu seçeneği fark etmemesi riski, otomatik öneriyle
   azaltılabilir.

2. **Confidence eşiğine bağlamak (ai/confidence-management.md'deki gibi)** —
   Reddedildi. Açık kaynak model çıktısı zaten confidence hesabına dahil
   değil (ADR-010); bu iki mekanizmayı birbirine bağlamak, ADR-010'un
   "karar dışı tutma" ilkesini dolaylı olarak ihlal ederdi.

## Sonuç

- `EscalationSuggested` event'i eklendi.
- MVP'de bu event'in sonucu bir vendor çağrısı değil, önceliklendirme ve
  notasyondur — bu, gerçek entegrasyon geldiğinde event yapısının
  değişmesini gerektirmez, sadece subscriber davranışı değişir.

## İlgili Dosyalar

- `../architecture/radiology-inference-service.md`
- `../adr/adr-010-open-source-radiology-model-mvp.md`
- `../adr/adr-008-mvp-scope-exclusions.md`
