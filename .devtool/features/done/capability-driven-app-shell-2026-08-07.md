---
id: "capability-driven-app-shell-2026-08-07"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-07T09:00:00.000Z"
completedAt: "2026-08-07T09:00:00.000Z"
labels: ["web", "navigation"]
order: "a00G"
---

# Capability-driven app shell

`MainLayout`, `AuthLayout`, header, sidebar and breadcrumbs, rendered from the signed-in user's capabilities rather than hard-coded per role. Global `InteractiveServer` render mode on `<Routes>` (`docs/build/web-conventions.md#interactivity-is-global-not-per-page`).

Files: `Simando.Web/Components/Layout/{MainLayout,AuthLayout,ReconnectModal}.razor*`, `App.razor`.
