---
id: "web-tasks-inbox-2026-08-07"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-08T17:00:00.000Z"
completedAt: "2026-08-08T17:00:00.000Z"
labels: ["tasks", "phase-5"]
order: "a00p"
---

# Web: /tasks inbox

Per-role pending steps only — genuinely this user's turn. Records visible but not actionable live under a separate "Semua di Region" tab. Not available to Sales Area (no inbox).

Shipped at `src/Simando.Web/Components/Pages/Tasks.razor`, backed by a new `ITasksService`/`TasksService` (first query service to join Company + WorkflowStep + Area/Region — no existing code did this, followed `UserService.GetUsersAsync`'s flat-query-plus-in-memory-dictionary style). Built all three tabs from `docs/design/frontend/08-tasks-and-approvals.md`, not just the two the card text named: **Menunggu Saya** (turn-filtered via `WorkflowStepAssignment.IsAssignedToStep`), **Semua di Region** (scope-filtered only, via `PermissionEvaluator.CanViewRecord`), **Riwayat Tindakan** (the actor's own past `StatusEvent` decisions) — cheap to add, and skipping it would've left the page visibly incomplete against its own wireframe.

Two deliberate deviations from the wireframe, both flagged upfront:
- **"Tahap" column shows `WorkflowStepKind`** (Area Head / Admin Regional / Reviewer 1-3 / Div. Head), not the document-stage label ("Evaluasi"/"Persetujuan") the wireframe shows — those come from `Company.CurrentStage`, which the workflow engine never writes (depends on unbuilt stage 6/7 entities).
- **`[ Tinjau ]` links to `/companies/{id}`**, which 404s today since `web-record-hub-shell-2026-08-07` is still backlog. Kept as a forward reference (several other backlog cards already assume that route) rather than disabling the button.

Filters (Tahap/Area) and sort (Terlama menunggu / Nama Perusahaan) are client-side over the already-scoped list, not new service query parameters.

Explicitly out of scope, each with its own card: `/tasks/blocked` (Tugas Tertahan), step reassignment (`web-workflow-step-reassignment-2026-08-07`), the review screen / Setuju-Revisi-Tolak action bar (`web-approval-action-bar-setujurevisitolak-2026-08-07`), the notifications bell panel.
