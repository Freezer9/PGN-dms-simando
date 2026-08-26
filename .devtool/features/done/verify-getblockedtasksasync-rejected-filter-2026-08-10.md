---
id: "verify-getblockedtasksasync-rejected-filter-2026-08-10"
status: "done"
priority: "low"
assignee: null
dueDate: null
created: "2026-08-10T00:00:00.000Z"
modified: "2026-08-10T12:00:00.000Z"
completedAt: "2026-08-10T12:00:00.000Z"
labels: ["tasks", "audit"]
order: "a01x"
---

# Verify and fix GetBlockedTasksAsync query for Rejected records

`TasksService.GetBlockedTasksAsync` uses `ActiveWorkflowQuery.LoadAsync` then filters for `company.Status == RecordStatus.Rejected || waitingSince > threshold || AssignedUserId == null`. When a record is rejected, `WorkflowService` sets `WorkflowInstance.CompletedAt`. If `ActiveWorkflowQuery` filters only open (uncompleted) workflow instances, `Rejected` records will never be loaded, leaving `/tasks/blocked` empty for rejected items.

## What to do

1. Inspect `ActiveWorkflowQuery` implementation to determine if completed workflow instances for `Rejected` records are included.
2. If `ActiveWorkflowQuery` excludes completed instances, update the query or `TasksService.GetBlockedTasksAsync` to explicitly include companies with `Status == RecordStatus.Rejected`.
3. Verify `/tasks/blocked` displays both stuck active tasks and rejected tasks awaiting Regional Admin rework/discontinue.

## Acceptance criteria

- [ ] `TasksService.GetBlockedTasksAsync` reliably retrieves `Rejected` records regardless of `WorkflowInstance.CompletedAt`
- [ ] `/tasks/blocked` displays rejected records alongside stuck tasks

## References

- `TasksService.cs:40–53`
- `WorkflowService.cs:243–247`
- `ActiveWorkflowQuery.cs`
