---
id: "notifications-inotificationchannel-in-app-implemen-2026-08-07"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-08T17:30:00.000Z"
completedAt: "2026-08-08T17:30:00.000Z"
labels: ["notifications", "phase-5"]
order: "a00q"
---

# Notifications: INotificationChannel + in-app implementation

In-app is the only live channel — every transition (submitted, setuju, revisi, tolak, reviewers assigned, NOL/RL issued) must notify. Built behind the interface so email can switch on later without touching the workflow service (T4 in testing.md protects this seam).

Shipped: `Notification` entity (`src/Simando.Domain/Notifications/`), `INotificationChannel` (one method, `SendAsync(recipientUserId, companyId, message)`, no DB types in the signature — the seam T4 protects), `InAppNotificationChannel` writing rows via `IDbContextFactory`. Wired into all three `WorkflowService` transition points (`StartAsync`, `ChooseReviewersAsync`, `ActAsync`), after each method's own `SaveChangesAsync` — intentionally not atomic with the workflow mutation itself, since the interface can't take a shared `DbContext` without breaking the channel-agnostic seam.

Recipient resolution (not specified in the docs, decided this pass): active-chain landing status → the new current step's holder(s), fanned out via `RoleAssignment` for role-resolved kinds (Area Head/Regional Admin/Division Head) or the specific `AssignedUserId` for Reviewer1-3; `Draft` (revisi from Area Head) and `IssuedNol`/`IssuedRl` → the creator; `Rejected` (tolak, any position) → Regional Admin role holders in the region, since they're the ones who manage it via Tugas Tertahan.

Backend only, per the card text — the bell panel / badge UI is `web-tugas-saya-badge-in-app-bell-panel-2026-08-07` (separate card, `high` not `critical`). No read-side query service built either; nothing consumes `Notification` rows yet, that card adds its own when built.
