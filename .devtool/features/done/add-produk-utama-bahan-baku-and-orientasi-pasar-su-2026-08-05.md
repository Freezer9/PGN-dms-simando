---
id: "add-produk-utama-bahan-baku-and-orientasi-pasar-su-2026-08-05"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-30T00:00:00.000Z"
completedAt: "2026-08-30T00:00:00.000Z"
labels: ["epic-industry", "backend", "data-model"]
order: "aA"
---
# Add Produk Utama, Bahan Baku, and Orientasi Pasar sub-entities

Model the three repeatable data tables from Form A1: main products with annual capacity, raw materials (import/local with country + monthly volume), and market orientation (import/local with country + monthly volume).

## Context
None of these exist today. Each is a repeatable row-based sub-table in the spec (e.g. up to 4 raw material rows with a notes field), so each needs its own child entity with a FK back to `Subscription`.

## Acceptance Criteria
- [x] `ProdukUtama` entity (Nama, KapasitasPerTahun, SubscriptionId)
- [x] `BahanBaku` entity (Nama, ImporOrLokal, Negara, VolumePerBulan, SubscriptionId)
- [x] `OrientasiPasar` entity (Nama, ImporOrLokal, Negara, VolumePerBulan, SubscriptionId)
- [x] EF Core migration created and applied
- [x] DTOs added to `Shared/Models.cs`
