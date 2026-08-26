---
id: "web-tetapkan-reviewer-action-2026-08-07"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-09T00:00:00.000Z"
completedAt: "2026-08-09T00:00:00.000Z"
labels: ["nol", "workflow", "phase-5"]
order: "a00v"
---

# Web: Tetapkan Reviewer action

Built as part of `/companies/{id}`'s action bar — see `web-record-hub-shell-2026-08-07`. `( Tetapkan Reviewer )` renders for Regional Admin only when `Company.Status == RegionalAdmin` and it's their turn, opening a `BbDialog` with a `BbFormFieldCheckboxGroup` of reviewer candidates (`IUserService.GetUsersAsync` filtered to `Role.Reviewer` holders in the actor's scope). The candidate list excludes the record's creator — P27 (testing.md) — which `ChooseReviewersAsync` was already enforcing server-side (built and tested during this session's earlier RBAC gap-fix); the dialog's candidate filtering is the UI-side mirror of that, not the only guard. 2–3 selections required before Simpan is enabled.
