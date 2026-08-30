---
id: "add-industryproduction-classification-fields-jenis-2026-08-05"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-30T00:00:00.000Z"
completedAt: "2026-08-30T00:00:00.000Z"
labels: ["epic-industry", "backend", "data-model"]
order: "a9"
---
# Add industry/production classification fields (Jenis Produksi via BPS dropdown)

Add a structured industry/production-type classification field to the subscription, backed by a BPS-style dropdown, replacing the current total absence of any production classification.

## Context
Form A1 has "Jenis Produksi" as a dropdown sourced from BPS (Indonesia's national statistics agency) industry categories. Currently zero fields relate to production/industry type in `Entities.cs`. This overlaps with the "Directory Industri" master data (see epic-directory) — that master list should be the source for this dropdown.

## Acceptance Criteria
- [x] `IndustryCategoryId` FK added to `Subscription`, referencing the Directory Industri master table
- [x] EF Core migration created and applied
- [x] Depends on / coordinates with the Directory Industri seed data card

## Depends On
- **Seed Directory Industri master data** (epic-directory) — must land first so the Jenis Produksi dropdown has options.
