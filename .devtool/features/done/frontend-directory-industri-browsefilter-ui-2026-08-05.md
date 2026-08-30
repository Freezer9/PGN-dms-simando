---
id: "frontend-directory-industri-browsefilter-ui-2026-08-05"
status: "done"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-30T00:00:00.000Z"
completedAt: "2026-08-30T00:00:00.000Z"
labels: ["epic-directory", "frontend"]
order: "aX"
---
# Frontend Directory Industri browse/filter UI

Build the "Directory" workflow-stage list view: a filterable table of companies by Provinsi/Kota/Kecamatan/Kelurahan/Jenis Produksi, with a "click name to open entry form" action, matching the spec's Directory Industry tab.

## Context
Today `SubscriptionStatus.Directory` exists only as the first status value in the pipeline enum — there is no actual directory listing/browse UI. This card builds that first real screen.

## Acceptance Criteria
- [x] Table view of subscriptions/prospects filterable by location + Jenis Produksi
- [x] Clicking a row opens the entry form to complete Sales/Kawasan/Jalur assignment (feeds into epic-plotting)
