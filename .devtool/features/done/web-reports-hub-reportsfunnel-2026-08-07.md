---
id: "web-reports-hub-reportsfunnel-2026-08-07"
status: "done"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-10T09:45:00.000Z"
completedAt: "2026-08-10T09:45:00.000Z"
labels: ["reporting", "phase-7"]
order: "a01T"
---

# Web: /reports hub + /reports/funnel

Reports landing page plus the Corong Penjualan funnel visualisation.

**The hub itself already exists** — built early alongside `web-reportsageing-2026-08-07` (done) because the sidebar's "Laporan" nav link pointed at `/reports` and 404'd. `ReportsHub.razor` has one real tile (Penuaan) and four "Belum tersedia" placeholders. What's left here is only `/reports/funnel` — build the funnel query/visualisation and replace its placeholder tile with a real link, not the hub page itself.
