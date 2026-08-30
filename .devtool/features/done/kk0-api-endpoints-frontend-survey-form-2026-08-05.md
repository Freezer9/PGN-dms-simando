---
id: "kk0-api-endpoints-frontend-survey-form-2026-08-05"
status: "done"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-30T00:00:00.000Z"
completedAt: "2026-08-30T00:00:00.000Z"
labels: ["epic-kk0", "api", "frontend"]
order: "aW"
---
# KK0 API endpoints + frontend survey form

Expose CRUD for the KK0 survey and build the corresponding Blazor form.

## Context
Depends on both entity cards above. Likely surfaced at the Survey workflow stage, parallel to (not replacing) the Survey-stage document upload that already exists.

## Acceptance Criteria
- [x] `GET/POST/PUT /api/subscriptions/{id}/kk0` endpoints added
- [x] Frontend KK0 survey form, including the fuel usage table and process description free-text field
