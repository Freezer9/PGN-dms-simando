---
id: "domain-equipment-table-derived-sum-2026-08-07"
status: "done"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-10T10:30:00.000Z"
completedAt: "2026-08-10T10:30:00.000Z"
labels: ["survey", "phase-3"]
order: "a00p"
---

# Domain: equipment-table derived sum

`jumlah_kebutuhan_energi` = live sum of every equipment row's `konversi_ke_gas`, stored (not computed on read) because it appears on signed documents. Recomputes on each row edit or delete (E3 in testing.md). `konversi_ke_gas` itself stays a plain manually-typed field — no conversion service, no calorific table.
