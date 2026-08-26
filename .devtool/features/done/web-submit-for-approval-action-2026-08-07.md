---
id: "web-submit-for-approval-action-2026-08-07"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-09T00:00:00.000Z"
completedAt: "2026-08-09T00:00:00.000Z"
labels: ["nol", "workflow", "phase-5"]
order: "a00t"
---

# Web: submit-for-approval action

The `[ Ajukan untuk Persetujuan ]` button on `/companies/{id}` — see `web-record-hub-shell-2026-08-07` for the combined implementation. Wired to `IWorkflowService.StartAsync`, which already snapshotted the chain and notified the Area Head (built earlier this session). Confirmation dialog first (`DialogService.ConfirmAsync`), then the action bar refreshes in place.

**Also fixed here**: `StartAsync` itself had no authorization checks before this task — see the note on `web-record-hub-shell-2026-08-07` for the full story. It now rejects a non-creator, a creator missing `Capability.SubmitForApproval`, or a creator scoped outside the record's Area, all covered by new tests in `WorkflowServiceTests.cs`.
