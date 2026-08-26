---
id: "add-administrative-location-entities-provinsikotak-2026-08-05"
status: "backlog"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-05T09:00:00.000Z"
completedAt: null
labels: ["epic-location", "backend", "data-model"]
order: "a3"
---

# Add administrative location entities (Provinsi/Kota/Kecamatan/Kelurahan)

Model Indonesia's administrative location hierarchy (Provinsi -> Kota/Kabupaten -> Kecamatan -> Kelurahan/Desa) as proper entities, replacing the current free-text `Address` field on `Subscription`.

## Context
Spec (Form A1) requires cascading Provinsi/Kota/Kecamatan/Kelurahan dropdowns plus a free-text street address. Current implementation (`Api/Data/Entities.cs`) only has a single free-text `Address` string and optional `Latitude`/`Longitude` — no structured administrative hierarchy exists.

## Acceptance Criteria
- [ ] `Provinsi`, `Kota`, `Kecamatan`, `Kelurahan` entities added to `Api/Data/Entities.cs` with parent FKs
- [ ] `Subscription` gets FK to `KelurahanId` (or equivalent) in addition to the existing free-text `Alamat` line
- [ ] EF Core migration created and applied
- [ ] DTOs added to `Shared/Models.cs`
