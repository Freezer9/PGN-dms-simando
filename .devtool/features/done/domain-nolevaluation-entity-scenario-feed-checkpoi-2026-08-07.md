---
id: domain-nolevaluation-entity-scenario-feed-checkpoi-2026-08-07
status: done
priority: high
assignee: null
dueDate: null
created: '2026-08-07T09:00:00.000Z'
modified: '2026-08-09T10:00:00.000Z'
completedAt: '2026-08-09T10:00:00.000Z'
labels:
- evaluation
- phase-6
order: a01K
---


# Domain: NolEvaluation entity + scenario + FEED checkpoint

Stage-7 fields (capex final, pipe sizing, MRS spec, G-Size, IRR/NPV/Payback) plus `nol_evaluation_scenario` as a child table from day one (v1 UI renders one row; the seam for the future two-scenario comparison is already the schema, not a migration). `feed_status`/`feed_completed_at`/`feed_document` model the FEED checkpoint as a genuine blocker on the timeline, without modelling the FEED process itself.

**Follow-up once this ships**: wire the stage-7 editor list into `PermissionEvaluator.IsSelfApproval`'s `stage7EditorIds` param at the `WorkflowService.ActAsync` call site (`src/Simando.Infrastructure/Workflow/WorkflowService.cs`, currently passed `[]`) — closes P28 in `docs/build/testing.md`, deferred from `rbac-segregation-of-duties-enforcement-2026-08-07`.
