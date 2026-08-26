---
id: domain-nolrequest-entity-children-2026-08-07
status: done
priority: critical
assignee: null
dueDate: null
created: '2026-08-07T09:00:00.000Z'
modified: '2026-08-09T10:00:00.000Z'
completedAt: '2026-08-09T10:00:00.000Z'
labels:
- nol
- phase-5
order: a015
---


# Domain: NolRequest entity + children

`nol_request` + `nol_request_period` (repeating contract volumes, `sama_dengan_a1` flag stored as data — reviewers examine exactly where it differs) + `nol_request_daily` (7-row weekday table, schema only, UI hidden — see "Future: daily contract basis") + `nol_request_reference` (→ reference_document). Also `registration_type` seam for the amendment/extension future feature.
