---
id: "fix-reassignstepasync-statusevent-action-2026-08-10"
status: "done"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-10T00:00:00.000Z"
modified: "2026-08-10T12:00:00.000Z"
completedAt: "2026-08-10T12:00:00.000Z"
labels: ["workflow", "audit"]
order: "a01u"
---

# Fix StatusEvent action in ReassignStepAsync

When Regional Admin reassigns a stuck workflow step, `WorkflowService.ReassignStepAsync` currently writes a `StatusEvent` with `Action = StatusEventAction.Setuju`. Reassigning a step is not an approval action. This misattribution corrupts the record's audit timeline display and inflates approval performance counts in `DashboardService`.

## What to do

1. Ensure `StatusEventAction.Reassign` (or `StatusEventAction.Reassigned`) exists in `StatusEventAction` enum (or `StatusEventAction.cs`).
2. Update `WorkflowService.ReassignStepAsync` to log `StatusEventAction.Reassign` instead of `StatusEventAction.Setuju`.
3. Verify that timeline UI components and `DashboardService` metrics exclude reassignments from `Setuju` / approval counts.

## Acceptance criteria

- [ ] `ReassignStepAsync` logs `StatusEventAction.Reassign` (or equivalent dedicated action enum value)
- [ ] Record audit timeline correctly distinguishes step reassignments from approvals
- [ ] Approver performance metrics in `DashboardService` count only actual `Setuju` actions

## References

- `WorkflowService.cs:309–320`
- `DashboardService.cs:134`
- `design/approval-workflow.md` §Audit trail
