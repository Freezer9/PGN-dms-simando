---
id: "master-data-unit-of-measure-unit-set-member-2026-08-07"
status: "done"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-07T20:03:28.000Z"
completedAt: "2026-08-07T20:03:28.000Z"
labels: ["master-data", "phase-0"]
order: "a005"
---

# Master data: Unit of Measure + Unit Set Member

Consolidate the 8 scattered ad-hoc unit sets (energy capacity, cooling, fuel consumption, raw-material %, diameter, gas volume, pressure) into one table with `dimension` and set membership, driving a single `<MeasureInput Set="...">` component. See `docs/domain/master-data.md#satuan-units-of-measure`.
