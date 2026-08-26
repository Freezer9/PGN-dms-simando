---
id: "web-approval-action-bar-setujurevisitolak-2026-08-07"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-09T00:00:00.000Z"
completedAt: "2026-08-09T00:00:00.000Z"
labels: ["nol", "workflow", "phase-5"]
order: "a00u"
---

# Web: approval action bar (Setuju/Revisi/Tolak)

Built as part of `/companies/{id}`'s action bar — see `web-record-hub-shell-2026-08-07`. `CompanyDetail.CanAct` (server-computed: turn + capability + scope + not-self-approval) gates the whole bar; it renders only for whoever holds the current step, matching the doc's "absent for everyone else," not a disabled state. Setuju confirms then calls `ActAsync` directly; Revisi and Tolak use `DialogService.PromptAsync` with `Required = true` to collect the comment before calling `ActAsync` — server-side, `WorkflowTransitions.RequiresComment` was already enforcing this (built earlier this session), so the dialog is belt-and-suspenders, not the only guard. Division Head at `RecordStatus.Approval` additionally gets `Tidak Layak` (issues RL) alongside Setuju (issues NOL) — a fourth `WorkflowAction` the doc's generic action-bar diagram doesn't show but `WorkflowTransitions`/testing.md's W7 requires.
