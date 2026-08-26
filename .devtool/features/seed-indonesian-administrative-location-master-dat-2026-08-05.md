---
id: "seed-indonesian-administrative-location-master-dat-2026-08-05"
status: "backlog"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-05T09:00:00.000Z"
completedAt: null
labels: ["epic-location", "backend", "seed-data"]
order: "a4"
---

# Seed Indonesian administrative location master data

Populate the Provinsi/Kota/Kecamatan/Kelurahan tables with reference data so the cascading dropdowns have real options.

## Context
Depends on the entities from the previous card. The spec's example (Form A1) shows Jawa Timur -> Surabaya -> Genteng -> Gentengkali. A full national dataset (or at minimum the provinces/regencies PGN currently operates in) is needed for the dropdowns to be usable.

## Acceptance Criteria
- [ ] Seed data source identified (e.g. public Indonesian administrative dataset) and imported via migration or seeder
- [ ] `IdentitySeeder.cs` or a new seeder seeds at minimum the regions currently used by `Region`/`Area` org data
- [ ] Seed data covers Provinsi, Kota/Kabupaten, Kecamatan, Kelurahan/Desa levels
