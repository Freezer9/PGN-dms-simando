---
id: "map-reviewstep-chain-to-named-roles-automatically-2026-08-05"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-30T00:00:00.000Z"
completedAt: "2026-08-30T00:00:00.000Z"
labels: ["epic-approval", "backend", "service"]
order: "aK"
---
# Map ReviewStep chain to named roles automatically instead of manual reviewer assignment

Change the review-chain assignment so subscriptions are automatically routed through PIC Area Support -> PIC Leader Area Support -> Area Head for the relevant area, instead of an admin manually picking arbitrary reviewer user IDs.

## Context
`WorkflowService.AssignReviewersAsync` (`Api/Services/WorkflowService.cs`) currently takes a manually-assigned, role-agnostic ordered list of reviewer IDs. This card makes the chain reflect the spec's fixed named-role sequence, looked up per the subscription's `Area`.

## Acceptance Criteria
- [x] Reviewer chain auto-populated based on who holds `PicAreaSupport`/`PicLeaderAreaSupport`/`AreaHead` for the subscription's area, removing (or making optional/override-only) the manual `assign-reviewers` step
- [x] Existing `Setuju`/`Tolak`/`Revisi` review actions unchanged

## Depends On
- **Introduce named roles 'PIC Area Support' and 'PIC Leader Area Support'** (epic-approval) — the roles must exist before the review chain can be auto-populated from them.
