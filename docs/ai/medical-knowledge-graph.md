# Medical Knowledge Graph

**Durum:** Post-MVP tasarım — bugünden domain modelinde yer açılıyor
**Önceki hali:** Basit "Medical Knowledge" havuzu (Blueprint v3.0, Bölüm 16.2)

## Neden Havuzdan Graph'a

Bir bilgi havuzu (pool), verinin normalize edilmiş ve aranabilir olmasını sağlar
ama **ilişkileri** temsil edemez. Oysa tıbbi bilgi doğası gereği ilişkiseldir: bir
tanı, bir görüntüleme yöntemi, bir evreleme sistemi, bir mutasyon, bir tedavi ve
bu tedaviyi destekleyen kanıt birbirine bağlıdır. Bu ilişkiler bir graph yapısında
doğal olarak temsil edilir.

## Örnek Graph Yapısı

```
Glioma (Diagnosis)
   │
   ├──[görüntülenir]──→ MRI (Imaging Modality)
   │
   ├──[evrelenir]──→ WHO Grade (Staging System)
   │                      │
   │                      └──[belirler]──→ Treatment (Tedavi seçenekleri)
   │
   ├──[ilişkilidir]──→ Mutation (örn. IDH mutasyonu)
   │                      │
   │                      └──[etkiler]──→ Treatment
   │
   └──[desteklenir]──→ Evidence (klinik çalışma sonuçları)
                            │
                            └──[kaynaklanır]──→ Guidelines (örn. WHO/NCCN rehberleri)
                                                    │
                                                    └──[referans verir]──→ Literature
```

## Node ve Edge Türleri (Taslak)

| Node Türü | Örnek |
|---|---|
| Diagnosis | Glioma, Meningioma |
| ImagingModality | MRI, CT, PET |
| StagingSystem | WHO Grade I-IV |
| Mutation | IDH1, MGMT metilasyon |
| Treatment | Cerrahi, Radyoterapi, Kemoterapi |
| Evidence | Klinik çalışma sonucu |
| Guideline | WHO, NCCN, ESMO rehberleri |
| Literature | Yayınlanmış makale referansı |

| Edge Türü | Anlamı |
|---|---|
| görüntülenir | Diagnosis → ImagingModality |
| evrelenir | Diagnosis → StagingSystem |
| ilişkilidir | Diagnosis → Mutation |
| etkiler | Mutation → Treatment |
| belirler | StagingSystem → Treatment |
| desteklenir | Treatment → Evidence |
| kaynaklanır | Evidence → Guideline |
| referans verir | Guideline → Literature |

## AI Ajanlarının Graph'ı Kullanımı

`ai-agent-ecosystem.md`'deki uzman ajanlar (Radiology AI, Pathology AI vb.) bu
graph'tan **okur**, ama graph'ı doğrudan değiştirmez — graph'ın güncellenmesi
ayrı bir editöryel/klinik onay süreci gerektirir (yanlış bir edge, sistemdeki her
vakayı etkileyebilir). Bu, `case-aggregate-root.md`'deki "kaynak izlenebilirliği"
ilkesinin sistem çapında bir yansımasıdır: her AI çıkarımı, graph üzerindeki somut
bir path'e (örn. Glioma → WHO Grade → Treatment) referans verebilmelidir.

## Neden Bugün Değil

Bir knowledge graph, kürasyon (doğru node/edge'lerin klinik olarak doğrulanması)
gerektirir — bu, ham veri işleme motorlarından (Document Quality Engine gibi)
çok farklı bir çalışma gerektirir ve gerçek klinik ortaklar olmadan spekülatif
kalır (bkz. `../adr/adr-008-mvp-scope-exclusions.md`). MVP'de basit bir "Medical
Knowledge" havuzu yeterlidir; bu doküman, o havuzun ileride nasıl bir graph'a
evrileceğinin tasarım taslağıdır.

## İlişkili Dosyalar

- AI ekosistemi: `ai-agent-ecosystem.md`
- Case invariant'ları: `../domain/case-aggregate-root.md`
- MVP kapsam kararı: `../adr/adr-008-mvp-scope-exclusions.md`
