---
id: "fix-sales-dashboard-tolak-visibility-2026-08-10"
status: "done"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-10T00:00:00.000Z"
modified: "2026-08-10T12:00:00.000Z"
completedAt: "2026-08-10T12:00:00.000Z"
labels: ["dashboard", "audit"]
order: "a01v"
---

# Surface Tolak'd records on Sales Area dashboard

`DashboardService.GetSalesAreaDashboardAsync` fetches both `Revisi` and `Tolak` status events, but then filters the joined company list by `Status == RecordStatus.Draft`. Because a `Tolak` action moves a record to `RecordStatus.Rejected` (not `Draft`), all `Tolak`'d records are silently discarded. Sales Area receives no visibility on their dashboard for records rejected within their area.

## What to do

Update `DashboardService.GetSalesAreaDashboardAsync` so that the returned work widget includes both:
1. Records returned via `Revisi` (status `Draft`)
2. Records returned via `Tolak` (status `Rejected`), within the user's Area scope

## Acceptance criteria

- [ ] Returned work panel on Sales Area dashboard surfaces both `Draft` (Revisi) and `Rejected` (Tolak) records
- [ ] Each returned item displays the appropriate badge/reason (Revisi vs Tolak)
- [ ] Area-based scope filter remains properly applied

## References

- `DashboardService.cs:24–38`
- `design/frontend/02-dashboard.md` §Sales Area
