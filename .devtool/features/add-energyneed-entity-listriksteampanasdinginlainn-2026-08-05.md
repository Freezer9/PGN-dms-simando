---
id: "add-energyneed-entity-listriksteampanasdinginlainn-2026-08-05"
status: "backlog"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-05T09:00:00.000Z"
completedAt: null
labels: ["epic-energy", "backend", "data-model"]
order: "aE"
---

# Add EnergyNeed entity (Listrik/Steam/Panas/Dingin/Lainnya kapasitas & pemakaian)

Model the top-level energy needs checklist and capacity/usage figures from Form A1.

## Context
Spec has checkboxes for energy type (Listrik/Steam/Panas/Dingin/Lainnya) with capacity (MW/Ton-Jam/Kkal/TR) and usage (Kwh/Ton/Kkal/TR) fields per type. Currently zero energy-related fields exist anywhere in the codebase.

## Acceptance Criteria
- [ ] `EnergyNeed` entity added with energy-type flags/enum, capacity, and usage fields, FK to `Subscription`
- [ ] EF Core migration created and applied
- [ ] DTOs added to `Shared/Models.cs`
