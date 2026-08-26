---
id: "nol-application-api-endpoints-frontend-form-tied-t-2026-08-05"
status: "backlog"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-05T09:00:00.000Z"
completedAt: null
labels: ["epic-nol", "api", "frontend"]
order: "aR"
---

# NOL application API endpoints + frontend form tied to PermohonanNOL stage

Expose CRUD for the NOL application and build the corresponding form, gated to the `PermohonanNOL` workflow stage.

## Context
Currently advancing a subscription to `PermohonanNOL` (via `POST /api/subscriptions/{id}/advance`) has no associated data capture. This card closes that gap end-to-end.

## Acceptance Criteria
- [ ] `GET/POST/PUT /api/subscriptions/{id}/nol-application` endpoints added
- [ ] Frontend form rendered when a subscription is at/past the `PermohonanNOL` stage
- [ ] Consider whether `advance` to `PermohonanNOL` should require the NOL application to be complete first (business rule to confirm with stakeholders)
