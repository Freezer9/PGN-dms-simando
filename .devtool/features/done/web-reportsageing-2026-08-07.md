---
id: "web-reportsageing-2026-08-07"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-09T00:00:00.000Z"
completedAt: "2026-08-09T00:00:00.000Z"
labels: ["reporting", "phase-2"]
order: "a00l"
---

# Web: /reports/ageing

Plain elapsed-time table sorted by wait time descending, per record currently in the workflow. No per-step SLA threshold, no colour-coded breach. Default landing screen for Area Head/Regional Admin/Division Head. This single table is the direct answer to the client's problem statement.

`Simando.Application/Reports/{AgeingRow,IReportsService}.cs` + `Simando.Infrastructure/Reports/ReportsService.cs`, reusing `TasksService`'s active-workflow join (extracted into `Simando.Infrastructure/Workflow/ActiveWorkflowQuery.cs` so both consumers share the same Company/Area/Region/IndustryType/WaitingSince computation instead of duplicating it) plus `CompanyDetailService`'s holder-name resolution pattern for `ActorLabel` ("Reviewer 2 (Dewi)" when a specific reviewer is assigned, plain role label otherwise). Gated on `Capability.ViewAgeingReport` (already assigned to SA/AH/RA/RV, not SYS); scope check is the raw `PermissionEvaluator.CanView`, not `CanViewRecord`, since this page's capability is `ViewAgeingReport` not `ViewCompanyRecords`.

**Deviation**: "Default landing screen for Area Head/Regional Admin/Division Head" is not wired — that's `/`'s role-aware composition, owned by the separate `web-role-aware-dashboard-compositions-2026-08-07` card (a00k, still backlog). This page is reachable via the sidebar and direct URL only.

**Also built here**: a minimal `/reports` hub (`ReportsHub.razor`) — the sidebar's "Laporan" nav item already linked to `/reports` and 404'd. One real tile (Penuaan) plus four "Belum tersedia" placeholders (Corong Penjualan, Potensi Kebutuhan, Produktivitas Survei, Hasil NOL/RL), same treatment `CompanyHub.razor` gives its not-yet-built tabs. This is the hub half of `web-reports-hub-reportsfunnel-2026-08-07` (a01T) — see that card, left in backlog since its `/reports/funnel` half is still unbuilt.

New tests: `tests/Simando.Integration.Tests/Reports/ReportsServiceTests.cs` (5 tests).
