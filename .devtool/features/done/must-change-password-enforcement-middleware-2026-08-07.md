---
id: "must-change-password-enforcement-middleware-2026-08-07"
status: "done"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-07T09:00:00.000Z"
completedAt: "2026-08-07T09:00:00.000Z"
labels: ["identity", "web"]
order: "a00E"
---

# Must-change-password enforcement middleware

Forces `/change-password` on first sign-in and after an admin-issued reset, before any other route is reachable (`docs/design/roles-permissions.md#the-account-lifecycle`).

Files: `Simando.Web/Middleware/MustChangePasswordMiddleware.cs`.
