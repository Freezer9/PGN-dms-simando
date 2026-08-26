---
id: "application-iworkflowservice-2026-08-07"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-08T15:30:00.000Z"
completedAt: "2026-08-08T15:30:00.000Z"
labels: ["workflow", "nol", "phase-5"]
order: "a00m"
---

# Application: IWorkflowService

`StartAsync` snapshots the chain into ordered `WorkflowStep` rows on submit; `ActAsync` executes Setuju/Revisi/Tolak against a step, re-checking scope+capability+turn server-side every time. See `docs/build/architecture.md#workflow-engine`.

Shipped at `src/Simando.Application/Workflow/IWorkflowService.cs` with a third method beyond the doc's two-method signature: `ChooseReviewersAsync` (Regional Admin's "Tetapkan Reviewer" action) — without it the chain could never functionally pass Regional Admin, since reviewers have to be assigned before Reviewer1-3's steps even exist. Service-level only; the Tetapkan Reviewer UI stays a separate future task (`web-tetapkan-reviewer-action-2026-08-07`).

`ActAsync`'s guard order: current-step match → not-already-acted → `WorkflowStepAssignment.IsAssignedToStep` (role-resolved for Area Head/Regional Admin/Division Head, specific-user for Reviewer1-3) → `PermissionEvaluator.CanAct` (capability) → scope (`PermissionEvaluator.CanView` against the company's Area/Region) → self-approval (`IsSelfApproval`, creator only for now — no stage-7-editor tracking since `NolEvaluation` isn't built) → reviewers-chosen guard on Regional Admin's own Setuju. Explicitly out of scope: `Company.CurrentStage` is never written (depends on stage 6/7 entities that don't exist yet), notifications (separate downstream module per `architecture.md`), and reassignment/W17-W19 snapshotting edge cases (`web-workflow-step-reassignment-2026-08-07`).
