---
id: "master-data-document-number-counter-entity-2026-08-07"
status: "done"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-07T20:03:28.000Z"
completedAt: "2026-08-07T20:03:28.000Z"
labels: ["master-data", "phase-0"]
order: "a00B"
---

# Master data: Document Number Counter entity

`document_number_counter` (document_type, scope_key, period_key, next_seq) — a locked counter row for official correspondence numbers (KK0, Nota Dinas), distinct from the company `Nomor`'s lock-free sequence because gaps in official numbers invite questions. See `docs/domain/master-data.md#format-penomoran-dokumen`.
