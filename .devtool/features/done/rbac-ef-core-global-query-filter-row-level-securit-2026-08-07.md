---
id: "rbac-ef-core-global-query-filter-row-level-securit-2026-08-07"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-07T21:05:22.000Z"
completedAt: "2026-08-07T21:05:22.000Z"
labels: ["rbac", "phase-0"]
order: "a00Q"
---

# RBAC: EF Core global query filter row-level security

`Company` (and everything hanging off it) scoped by `company.area_id` via a global query filter, applied automatically to every LINQ query including navigation properties and reports — never a per-screen `Where`. See `docs/build/architecture.md#row-level-security`. Depends on the `Company` entity existing.
