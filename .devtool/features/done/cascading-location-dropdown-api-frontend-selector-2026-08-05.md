---
id: "cascading-location-dropdown-api-frontend-selector-2026-08-05"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-30T00:00:00.000Z"
completedAt: "2026-08-30T00:00:00.000Z"
labels: ["epic-location", "api", "frontend"]
order: "a5"
---
# Cascading location dropdown API + frontend selector for subscription form

Expose the location hierarchy via API and build a cascading dropdown selector for the subscription registration form.

## Context
No location-lookup endpoints exist today (only `GET /api/subscriptions/regions` and `/areas` for PGN's internal org structure, which is a separate concept). This card wires the new administrative-location data into both the API and the Blazor UI.

## Acceptance Criteria
- [x] `GET /api/locations/provinsi`, `/kota?provinsiId=`, `/kecamatan?kotaId=`, `/kelurahan?kecamatanId=` endpoints added
- [x] Frontend cascading select component (Provinsi -> Kota -> Kecamatan -> Kelurahan) built and reused wherever company location is entered
- [x] Wired into the subscription creation/edit form
