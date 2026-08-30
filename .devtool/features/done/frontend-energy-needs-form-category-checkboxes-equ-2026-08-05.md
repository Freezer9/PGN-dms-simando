---
id: "frontend-energy-needs-form-category-checkboxes-equ-2026-08-05"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-30T00:00:00.000Z"
completedAt: "2026-08-30T00:00:00.000Z"
labels: ["epic-energy", "frontend"]
order: "aG"
---
# Frontend energy needs form (category checkboxes + equipment table)

Build the energy needs section of the form: category checkboxes with capacity/usage inputs, plus the dynamic per-equipment table with an "insert row" action.

## Context
Depends on both backend cards above. Should auto-calculate "Jumlah Kebutuhan Energi" total shown in the spec, if derivable from equipment rows.

## Acceptance Criteria
- [x] Checkbox group for Listrik/Steam/Panas/Dingin/Lainnya with per-type capacity/usage inputs
- [x] Dynamic equipment table (add/remove row) for Steam Boiler/Thermal Oil Heater/Dryer/other
- [x] Computed total energy requirement displayed
