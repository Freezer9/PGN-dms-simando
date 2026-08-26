---
id: "fix-stage7-editor-self-approval-guard-2026-08-10"
status: "done"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-10T00:00:00.000Z"
modified: "2026-08-10T12:00:00.000Z"
completedAt: "2026-08-10T12:00:00.000Z"
labels: ["security", "workflow", "audit"]
order: "a01w"
---

# Wire stage-7 editor history to self-approval guard in ActAsync

`WorkflowService.ActAsync` invokes `PermissionEvaluator.IsSelfApproval(actorUserId, company.CreatedBy, [])`, always passing an empty collection for `stage7EditorIds`. The segregation-of-duties rule stating *"A user may never act on an approval step for a record they edited at stage 7"* is currently inoperative. If a Regional Admin user edits stage-7 evaluation data and is subsequently assigned as a Reviewer or Division Head, they are not blocked from approving.

## What to do

1. Query `status_event` (or audit log) for user IDs who edited stage-7 evaluation fields for the given company record.
2. Pass the retrieved list of stage-7 editor user IDs into `PermissionEvaluator.IsSelfApproval` inside `WorkflowService.ActAsync`.
3. If `IsSelfApproval` returns true, reject the action with a `Fail` result indicating segregation of duties violation.

## Acceptance criteria

- [ ] `ActAsync` queries and passes actual stage-7 editor user IDs to `IsSelfApproval`
- [ ] A user who edited stage-7 evaluation data is blocked from performing approval actions (`Setuju`/`Revisi`/`Tolak`/`IssueNolRl`) on that record
- [ ] Creator self-approval guard remains active alongside stage-7 editor check
- [ ] Tested: unit test verifying rejection when actor is a stage-7 editor

## References

- `WorkflowService.cs:212`
- `PermissionEvaluator.cs:81–82`
- `design/roles-permissions.md` §Segregation of duties
