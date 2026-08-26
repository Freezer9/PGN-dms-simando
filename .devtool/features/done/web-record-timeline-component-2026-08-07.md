---
id: "web-record-timeline-component-2026-08-07"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-09T00:00:00.000Z"
completedAt: "2026-08-09T00:00:00.000Z"
labels: ["record-hub", "phase-2"]
order: "a00s"
---

# Web: record timeline component

Built as the right rail of `/companies/{id}` — see `web-record-hub-shell-2026-08-07` for the combined implementation notes. `ICompanyDetailService.GetTimelineAsync` reads `StatusEvent` newest-first; the page pins a synthesized "current step, still open" row above it (role + live ageing, from `CompanyDetail.HolderLabel`/`StatusSince`) since an open step has no `StatusEvent` row of its own yet. Comments render inline; collapsed to 8 with a client-side "Lihat semua" toggle (no separate route).

**Deviation**: no synthetic "reviewer ditetapkan (2)" entry — `ChooseReviewersAsync` doesn't write a `StatusEvent`, so that mockup detail isn't reproducible from real data without inventing a new audit row for a non-status-changing operation.
