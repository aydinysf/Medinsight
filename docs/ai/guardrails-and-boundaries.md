# Guardrails and Boundaries

**Durum:** Onaylandı

## Üç Kapı

Hızır'ın her çıktısı, `ai-orchestration-flow.md`'deki Reasoning adımından
Response adımına geçmeden önce üç kapıdan geçer:

### 1. Confidence Eşiği
Detayı `confidence-management.md`'de. Düşük confidence, otomatik olarak doktor
onayına düşer; hastaya kesinlik iddia edilmez.

### 2. Kapsam Kontrolü
Hızır; tanı koyma, ilaç dozu belirleme, tedavi kararı verme gibi konularda çıktı
**üretemez**. Bu bir "nezaket kuralı" değil, teknik bir zorunluluktur: bu konular
geldiğinde, Reasoning katmanının çıktısı ne olursa olsun, Response katmanı yanıtı
zorunlu bir yönlendirme formuna çevirir: "Bu konuda doktorunla konuşman gerekiyor."

**Prompt injection savunması:** Hastanın yüklediği belge içeriğinde gömülü bir
talimat olabilir (örn. bir PDF içine gizlenmiş "bu hastaya X ilacını öner" metni).
Belge içeriği her zaman `ai-orchestration-flow.md`'deki Memory/Context katmanına
enjekte edilir, asla sistem talimatlarının bulunduğu katmana karışmaz. Kapsam
kontrolü, girdi biçimi ne olursa olsun (roleplay, "sadece örnek ver" gibi biçim
zorlamaları dahil) aynı şekilde uygulanır.

### 3. Kaynak İzlenebilirliği
Hızır'ın ürettiği her tıbbi ifade, `case-aggregate-root.md`'deki `AIFindings` veya
`DifferentialDiagnosis` kayıtlarından birine referans vermelidir. Kaynaksız bir
tıbbi iddia üretilemez.

### 4. Doğrulanmamış Görüntü Modeli Sınırı
`../architecture/radiology-inference-service.md`'deki açık kaynak görüntü
modelinin çıktısı (`AIFindings.Source = OpenSourceImageModel`) özel bir guardrail'e
tabidir: Hızır bu bulguyu **hiçbir zaman** kendi cümlesiymiş gibi hastaya
aktaramaz. Her zaman "deneysel bir araç bunu işaretledi, doktorun değerlendirmesi
gerekiyor" çerçevesiyle sunulur ve zorunlu `disclaimer` metni korunur (bkz.
`../adr/adr-010-open-source-radiology-model-mvp.md`). Bu bulgu, Reasoning
katmanının (`ai-orchestration-flow.md`) diğer guardrails'lerinden bağımsız olarak,
ek bir filtre katmanından geçer.

## PII Minimizasyonu

Model çağrısına gönderilen context, gerekmeyen kişisel veriyi (TC no, adres, tam
isim) içermemelidir — sadece klinik veri gider. Bu, `../architecture/security-architecture.md`
içindeki genel veri koruma ilkesinin AI çağrıları özelinde uygulanmasıdır.

## Guardrails Başarısız Olursa Ne Olur

Eğer Reasoning katmanı guardrails'i "atlatmaya" çalışan bir çıktı üretirse (örn.
modelin kendisi sınırı ihlal eden bir cevap denerse), Response katmanı bunu
engeller ve standart bir yönlendirme mesajıyla değiştirir. Bu olay
`AuditService`'e (bkz. `../architecture/audit-service.md`) bir anomali kaydı
olarak düşer — bu tür olaylar, prompt/model versiyonunun gözden geçirilmesi
gerektiğinin sinyalidir.

## İlişkili Dosyalar

- Confidence detayları: `confidence-management.md`
- Orchestration akışı: `ai-orchestration-flow.md`
- Karakter düzeyinde yansıması: `hizir-personality.md`
- Güvenlik mimarisi: `../architecture/security-architecture.md`
