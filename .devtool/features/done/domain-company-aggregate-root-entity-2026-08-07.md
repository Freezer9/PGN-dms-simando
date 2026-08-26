---
id: "domain-company-aggregate-root-entity-2026-08-07"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-07T20:34:19.000Z"
completedAt: "2026-08-07T20:34:19.000Z"
labels: ["company", "phase-1"]
order: "a00W"
---

# Domain: Company aggregate root entity

The stage-1 spine every other stage's data hangs off. Fields per `docs/design/data-model.md#company`: nomor/nomor_seq, nama_perusahaan, village_id, location (geography Point), industry_type_id, area_id, current_stage, status. Index on (area_id, current_stage, status).
