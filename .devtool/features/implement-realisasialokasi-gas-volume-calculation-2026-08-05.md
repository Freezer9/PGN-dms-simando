---
id: "implement-realisasialokasi-gas-volume-calculation-2026-08-05"
status: "backlog"
priority: "low"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-05T09:00:00.000Z"
completedAt: null
labels: ["epic-plotting", "backend", "service"]
order: "aZ"
---

# Implement realisasi/alokasi gas volume % calculation logic

Implement the realisasi (realized) vs. alokasi (allocated) gas volume percentage calculation shown at the bottom of the spec's Plotting tab (e.g. `60/64 * 45 BBTUD`, allocation total, percentage used).

## Context
This requires clarifying the source data and formula with business — the spec example includes what looks like a day-of-month-dependent calculation with a literal `#VALUE!` error in the source spreadsheet, suggesting the original formula itself may be broken/unclear and needs to be re-derived from business rules rather than copied as-is.

## Acceptance Criteria
- [ ] Clarify the realisasi/alokasi formula and its inputs with business stakeholders (the spec's own example contains a formula error)
- [ ] Service/calculation implemented once formula is confirmed
