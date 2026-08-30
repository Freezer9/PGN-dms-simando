---
id: "seed-directory-industri-master-data-20-industry-ca-2026-08-05"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-30T00:00:00.000Z"
completedAt: "2026-08-30T00:00:00.000Z"
labels: ["epic-directory", "backend", "seed-data"]
order: "a2"
---
# Seed Directory Industri master data (20 industry categories with example products)

Seed the 20 industry category reference list (Makanan Minuman, Logam Dasar Non Baja/Baja, Fabrikasi Logam, Bahan Tekstil, Kertas, Kaca, Ceramic, CNG/LNG, Kimia, Smelter, Rubber, Plastic, Laundry, Tobacco, Wood, Farmasi, Gas Industri, Horeka, etc.) with their example products, referenced by the Jenis Produksi dropdown (epic-industry).

## Context
Currently no industry/master-data table exists at all — the only seeded lookup data is the PGN-internal Region/Area org hierarchy, which is a different concept entirely. This table is a dependency for epic-industry's Jenis Produksi dropdown.

## Acceptance Criteria
- [x] `IndustryCategory` entity (Name, ExampleProducts) added
- [x] Seeded with the 20 categories listed in the spec's "Jenis Industri" tab
- [x] EF Core migration + seeder

## Blocks
- **Add industry/production classification fields (Jenis Produksi via BPS dropdown)** — its dropdown has no options until this seed exists. Sequenced first in the build order for this reason; priority raised from medium to high to match.
