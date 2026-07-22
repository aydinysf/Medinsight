# Document Quality Engine

**Bounded Context:** Document Quality Engine
**Durum:** Onaylandı — v3.0 itibarıyla 10 kriterli yapıya genişletildi

## Amaç

Case'e yüklenen her belge, AI analiz kuyruğuna girmeden önce dosya bazlı bir kalite
kontrolünden geçer. Bu motor, "bu belge analiz edilebilir mi" sorusuna nesnel bir
cevap üretir — öznel yargı değil, ölçülebilir skor.

## Genişletilmiş Kalite Kriterleri

İlk tasarımda üç kritere dayanıyordu (okunabilirlik, çözünürlük, eksiksizlik). Gerçek
belge çeşitliliği (DICOM, PDF, fotoğraf, tarama) bunu yetersiz bıraktı. Genişletilmiş
kriter seti:

| Kriter | Uygulandığı belge türü | Ölçüm yöntemi |
|---|---|---|
| Resolution | Görüntü, DICOM | DPI / piksel yoğunluğu eşiği |
| Orientation | Görüntü, taranan PDF | Otomatik döndürme tespiti (90°/180° sapma) |
| Contrast | Görüntü, DICOM | Histogram analizi |
| Artifact | Görüntü, DICOM | Bulanıklık, gürültü, kırpılma tespiti |
| Missing Pages | Çok sayfalı PDF | Sayfa numarası süreklilik kontrolü |
| Duplicated Files | Tüm türler | Hash bazlı tekrar tespiti |
| DICOM Integrity | DICOM | Zorunlu metadata alanlarının (PatientID, StudyDate, Modality) varlığı |
| OCR Score | Taranan PDF, fotoğraf | OCR motorunun kendi güven skoru |
| Language Detection | Metin içeren belgeler | Türkçe/İngilizce/diğer tespiti — yanlış dilde yüklenen belge işaretlenir |
| Completeness | Tüm türler | Beklenen alan/bölüm kontrolü (örn. imza, tarih, hasta bilgisi) |

## Neden Tek Bir "QualityScore" Değil

Önceki tasarımda tek bir skor (`OverallScore`) vardı. Bu, hangi kriterin başarısız
olduğunu gizler. Genişletilmiş modelde her kriter ayrı bir alt-skor üretir ve
`FailureReasons` dizisi hangi kriterlerin eşiğin altında kaldığını açıkça listeler.
Bu, hastaya "belgeniz kalitesiz" demek yerine "belgenizin 3. sayfası eksik görünüyor"
diyebilmeyi sağlar — kullanıcı deneyimi açısından fark yaratan bir detay.

## Skor Ağırlıklandırma

Belge türüne göre kriterlerin ağırlığı değişir:

- **DICOM çalışmaları**: DICOM Integrity ve Resolution en yüksek ağırlığı taşır.
- **Taranan PDF (doktor notu)**: OCR Score ve Missing Pages öncelikli.
- **Fotoğraf (cilt lezyonu vb.)**: Contrast, Artifact ve Orientation öncelikli.

Ağırlıklar konfigürasyondan okunur, kod değişikliği gerektirmeden güncellenebilir olmalıdır.

## Büyüme Beklentisi

Bu motorun kapsamı zamanla genişleyecektir — CTO değerlendirmesinde net şekilde
belirtildiği gibi, bu "çok büyüyecek" bir bileşendir. Yeni kriter eklemek
(örn. Video Quality, Audio Clarity gelecekte ses/video girişleri için) mevcut
skorlama mimarisini bozmamalıdır; bu yüzden her kriter bağımsız bir "quality check
plugin" olarak modellenmelidir, tek bir monolitik fonksiyon olarak değil.

## İlişkili Dosyalar

- Case içindeki konumu: `case-aggregate-root.md`
- Tetiklediği event'ler: `domain-events-catalog.md` (`DocumentUploaded`, `DocumentQualityScored`)
- Akış diyagramı: Blueprint v3.0, Bölüm 15.4
