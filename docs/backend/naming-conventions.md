# Naming Conventions

**Durum:** Onaylandı

## Genel Kural

Tüm domain terimleri, `docs/domain/` dosyalarındaki isimlendirmeyle **birebir**
eşleşmelidir. Kodda "Vaka" değil `Case`, "Sağlık Rotası" değil `HealthRoute`
kullanılır — Türkçe/İngilizce karışıklığı domain terimlerinde asla olmaz (arayüz
metinleri hariç, onlar Türkçedir).

## Sınıf ve Dosya İsimlendirme

| Tür | Kural | Örnek |
|---|---|---|
| Aggregate | Domain terimiyle birebir | `Case`, `HealthRoute` |
| Domain Event | Geçmiş zaman, `...ed`/`...Completed` | `DocumentUploaded`, `AIAnalysisCompleted` |
| Command | Emir kipi | `CreateCase`, `UploadDocument` |
| Command Handler | `{Command}Handler` | `CreateCaseHandler` |
| Query | `Get{Şey}Query` | `GetCaseTimelineQuery` |
| Event Handler | `On{Event}` | `OnDocumentUploaded` |

## Event İsimlendirme Kaynağı

Her yeni domain event eklenmeden önce `../domain/domain-events-catalog.md`
kontrol edilmeli — orada zaten tanımlı bir isimle çakışan/benzeyen bir event
üretilmemeli.

## Veritabanı Tablo İsimlendirme

`snake_case`, çoğul (`cases`, `health_route_snapshots`) — ERD dosyalarındaki
(`../domain/erd-identity-case.md`, `../domain/erd-ai-clinical.md`) isimlendirmeyle
birebir eşleşir.

## İlişkili Dosyalar

- Klasör yapısı: `folder-structure.md`
- Domain event kataloğu: `../domain/domain-events-catalog.md`
