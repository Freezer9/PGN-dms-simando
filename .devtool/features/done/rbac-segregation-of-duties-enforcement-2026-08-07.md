---
id: "rbac-segregation-of-duties-enforcement-2026-08-07"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-08T16:00:00.000Z"
completedAt: "2026-08-08T16:00:00.000Z"
labels: ["rbac", "workflow"]
order: "a00o"
---

# RBAC: segregation-of-duties enforcement

Server-side rule: a user may never act on an approval step for a record they created, or edited at stage 7 — self-approval defeats the entire chain. Enforced in the workflow service, tested at the service layer (P26–P32 in `docs/build/testing.md`), not via UI hiding.

P26 (self-approval on own step), P29–P32 (role self-modification / `CanAssignRole` scoping) already covered by the earlier `WorkflowService`/`UserService` build. This pass closed the remaining gap: P27, "reviewer picker excludes the record's creator" — `WorkflowService.ChooseReviewersAsync` now rejects a `reviewerUserIds` list containing `Company.CreatedBy`.

**P28 (stage-7 editor self-approval) stays deferred** — `PermissionEvaluator.IsSelfApproval`'s `stage7EditorIds` param is still always `[]`, since `NolEvaluation` (the entity that would track who edited stage 7) isn't built yet. Noted on `domain-nolevaluation-entity-scenario-feed-checkpoi-2026-08-07` so it's wired in once that entity ships.
