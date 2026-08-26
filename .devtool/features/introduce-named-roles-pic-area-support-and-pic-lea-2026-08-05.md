---
id: "introduce-named-roles-pic-area-support-and-pic-lea-2026-08-05"
status: "backlog"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-05T09:00:00.000Z"
completedAt: null
labels: ["epic-approval", "backend", "auth"]
order: "aJ"
---

# Introduce named roles 'PIC Area Support' and 'PIC Leader Area Support'; wire AreaHead policy

Add the two missing named reviewer roles from the spec's approval chain, and wire up `AreaHead` to an actual authorization policy (it currently exists as a seedable role but grants no policy per `Frontend/Program.cs`).

## Context
Spec approval chain: Creator (Sales) -> Review 1 (PIC Area Support) -> Review 2 (PIC Leader Area Support) -> Approval (Area Head). Current roles are `SalesArea, AreaHead, AdminRegional, Reviewer, DivisionHead` — there's no "PIC Area Support"/"PIC Leader Area Support" distinction, and `AreaHead` isn't wired into any policy.

## Acceptance Criteria
- [ ] `PicAreaSupport` and `PicLeaderAreaSupport` roles added to `SimandoRoles.cs` (both `Api` and `Frontend` copies)
- [ ] `AreaHead` granted an actual authorization policy for its approval step
- [ ] Existing `Reviewer`/`DivisionHead` roles reconciled with the new named roles (decide whether they're replaced or kept as a generic fallback)
