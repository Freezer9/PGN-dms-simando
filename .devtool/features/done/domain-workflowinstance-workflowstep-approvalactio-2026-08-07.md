---
id: "domain-workflowinstance-workflowstep-approvalactio-2026-08-07"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-08T15:30:00.000Z"
completedAt: "2026-08-08T15:30:00.000Z"
labels: ["workflow", "nol", "phase-5"]
order: "a00l"
---

# Domain: WorkflowInstance / WorkflowStep / ApprovalAction entities

Ordinary case data populated as a case moves — not a projection of a workflow template, because there isn't one (Regional Admin picks 2–3 reviewers per case via `Tetapkan Reviewer`, not from a preset roster). Snapshotted at submit time — resolving roles lazily would corrupt history on reassignment.

Shipped as `WorkflowInstance`/`WorkflowStep` (`src/Simando.Domain/Workflow/`) — bare entities, not `AuditableEntity`, same reasoning as `StatusEvent` (append-only-adjacent, no soft-delete). No separate `ApprovalAction` entity: `approval-workflow.md`'s `APPROVAL_ACTION` and `architecture.md`'s `status_event` turned out to be two names for the same append-only log — reused the already-built `StatusEvent` instead, adding a nullable `WorkflowStepId` FK to link a decision to the step it happened on. `WorkflowStep` rows for Reviewer1-3 are created lazily by `ChooseReviewersAsync` (once Regional Admin picks 2 or 3 reviewers), not at submit time — `StartAsync` only snapshots the Area Head/Regional Admin steps, since reviewer count isn't known until later.
