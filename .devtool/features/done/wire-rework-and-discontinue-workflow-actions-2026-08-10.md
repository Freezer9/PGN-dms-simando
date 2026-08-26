---
id: "wire-rework-and-discontinue-workflow-actions-2026-08-10"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-10T00:00:00.000Z"
modified: "2026-08-10T12:00:00.000Z"
completedAt: "2026-08-10T12:00:00.000Z"
labels: ["workflow", "audit"]
order: "a01t"
---

# Wire Rework and Discontinue actions for REJECTED records

When a record is rejected via `Tolak`, it lands in Regional Admin's queue in the `REJECTED` state. While `WorkflowTransitions.Rework` (Rejected → Draft) and `WorkflowTransitions.Discontinue` (Rejected → Discontinued) exist as pure functions, they are not exposed on `IWorkflowService` or implemented in `WorkflowService`. There is also no UI on `/tasks/blocked` to trigger them. Consequently, the `REJECTED` state is currently a dead end.

## What to do

1. **Service layer**:
   - Add `ReworkAsync(Guid companyId, string comment, CancellationToken ct)` and `DiscontinueAsync(Guid companyId, string comment, CancellationToken ct)` to `IWorkflowService` and implement in `WorkflowService`.
   - Ensure `ReworkAsync` transitions the record back to `Draft` and notifies Sales Area.
   - Ensure `DiscontinueAsync` transitions the record to `Discontinued` (terminal state) with a mandatory comment/reason.
   - Require `Capability.ReassignWorkflowStep` (or dedicated capabilities if specified) and verify Regional Admin scope.

2. **UI layer**:
   - Update `TasksBlocked.razor` (or the REJECTED queue view) to expose "Kembalikan ke Sales Area" (Rework) and "Hentikan Permohonan" (Discontinue) action buttons with comment dialogs for rejected items.

## Acceptance criteria

- [ ] `ReworkAsync` and `DiscontinueAsync` methods added to `IWorkflowService` & `WorkflowService`
- [ ] Regional Admin can rework a `Rejected` record back to `Draft` status for Sales Area
- [ ] Regional Admin can mark a `Rejected` record as `Discontinued` with a mandatory reason
- [ ] Server-side authorization and scope checks enforced for both actions
- [ ] `TasksBlocked.razor` UI wired to trigger both actions

## References

- `WorkflowTransitions.cs:37–62`
- `IWorkflowService.cs:14–46`
- `TasksBlocked.razor`
- `design/approval-workflow.md` §Tolak
