# ADR-010: MVP'de Açık Kaynak Görüntü Modeli — Bilgilendirici Katman Olarak

**Durum:** Kabul edildi
**Karar tarihi:** Repo dokümantasyon oturumu

## Bağlam

MedInsight'ın değer önerisi "MR yükle, yorum al" üzerine kurulu, ama gerçek
piksel-seviyeli görüntü yorumlama (segmentasyon, lezyon tespiti) klinik olarak
doğrulanmış modeller gerektirir. Üç seçenek değerlendirildi: (1) MVP'de görüntü
analizi hiç yapmamak, sadece metin/rapor yorumlamak, (2) ücretli/onaylı üçüncü
parti radyoloji AI servisi entegre etmek, (3) açık kaynak, önceden eğitilmiş
modelleri MVP'ye entegre etmek.

## Karar

Üçüncü seçenek benimsendi, ama **sıkı bir sınırla**: açık kaynak modelin çıktısı
sisteme sadece **bilgilendirici, karar verici olmayan** bir katman olarak dahil
edilir. Bu, `radiology-inference-service.md`'de tanımlanan ayrı bir Python
mikroservisi olarak uygulanır.

## Neden Bu Sınırla Kabul Edildi

Açık kaynak modellerin klinik doğrulaması olmaması gerçek bir risktir (bkz.
sohbet geçmişi — bu ADR'nin tetikleyicisi). Ancak bu riski tamamen ortadan
kaldırmak yerine **yapısal olarak sınırlamak** tercih edildi:

- Model çıktısı asla `DifferentialDiagnosis`'a girmez.
- Model çıktısı confidence eşiği mantığından muaf tutulur (zaten hiçbir zaman
  otomatik önceliklendirme tetiklemez).
- Her çıktı zorunlu bir `disclaimer` alanı taşır ve arayüzde ayrı, düşük
  görsel ağırlıkta gösterilir.

Bu, hastaya veya doktora "AI bunu söyledi" değil, "deneysel bir araç bunu
işaretledi, sen değerlendir" mesajı verir.

## Alternatifler

1. **Görüntü analizi hiç yapmamak (metin/rapor odaklı MVP)** — Daha güvenli
   ama ürünün "MR yorumlama" vaadini zayıflatıyordu; ekip bu riski bilerek
   almayı tercih etti.

2. **Ücretli/onaylı üçüncü parti servis** — Daha güvenli ve hızlı klinik kabul
   sağlardı ama MVP bütçesi ve zaman çizelgesiyle şu an uyumlu değil; bu seçenek
   Post-MVP'de tekrar değerlendirilecek (bkz. `../ai/ai-agent-ecosystem.md`).

## Sonuç

- `AIFindings` şemasına `Source` alanı eklendi: `LLMTextAnalysis` |
  `OpenSourceImageModel` (bkz. `../domain/case-aggregate-root.md`).
- Risk Register'a yeni madde: "Doğrulanmamış görüntü modelinin yanlış
  yönlendirmesi" — önlem: zorunlu disclaimer, karar dışı tutma, ayrı görsel
  sunum.
- Bu karar, ürün gerçek kullanıcılarla test edildikçe gözden geçirilmelidir;
  eğer doktorlar bu "ek bilgi"yi göz ardı etmiyorsa (yani fiilen karar
  dayanağı yapıyorsa), sınırlar yeterli değildir ve seçenek 2'ye (üçüncü
  parti onaylı servis) geçiş gerekir.

## İlgili Dosyalar

- `../architecture/radiology-inference-service.md`
- `../domain/case-aggregate-root.md`
- `../ai/guardrails-and-boundaries.md`
