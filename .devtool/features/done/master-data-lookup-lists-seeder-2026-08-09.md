---
id: "master-data-lookup-lists-seeder-2026-08-09"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-09T00:00:00.000Z"
modified: "2026-08-09T00:00:00.000Z"
completedAt: "2026-08-09T00:00:00.000Z"
labels: ["master-data", "infra"]
order: "a01r"
---

# Master data: lookup-lists seeder (Units, Fuel types, Industry types, Countries, Segments)

Go-live checklist items #4-#8 (`docs/domain/master-data.md#13-seeding-checklist`) —
the small lookup tables whose source is the doc itself (or ISO 3166-1), not PGN
input. All were built with CRUD admin screens but shipped with no seed data, same
gap `master-data-wilayah-seeder` closed for geography.

## Shipped

`MasterDataSeeder` (`src/Simando.Infrastructure/MasterData/MasterDataSeeder.cs`),
gated per-table (`AnyAsync`) so a partial prior run stays resumable:

- **Units of measure + set membership** — the 17 distinct units behind the doc's
  8 `<MeasureInput Set="...">` dropdowns (§7), each unit inserted once even where
  it backs multiple sets (e.g. `TR` backs Capacity/Cooling/EnergyUsage). Added
  `UnitDimension.Area` — `m²` (Bahan baku/pasar) had no dimension to map to.
- **Fuel types** — 13-row KK0/Lampiran 10 union list (§7), including `Lainnya`.
- **Industry types** — the 20-row curated list with example products (§5).
- **Countries** — full ISO 3166-1 (249 rows) with Indonesian names, generated via
  Python's `babel` (CLDR locale data) rather than transcribed by hand — see
  `src/Simando.Infrastructure/MasterData/CountrySeedData.cs`.
- **Segments** — the 6-tier Bronze 1 → Platinum list (§6), in tier order.

Runs via the same `seed-master-data` CLI command as the geography import (both in
one command since they're the same "go-live prerequisite" batch and neither needs
credentials/config).

Note: local dev DB already had 2 stray rows in `segment` from earlier manual admin-UI
testing (1 active "Bronze 1", 1 soft-deleted "Column Bug Test"), which blocked this
seeder's segment step there (table wasn't empty) — flagged to the user rather than
auto-deleted; not a seeder bug.
