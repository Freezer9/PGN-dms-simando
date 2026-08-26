---
id: "role-aware-navigation-menu-builder-2026-08-07"
status: "done"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-07T09:00:00.000Z"
completedAt: "2026-08-07T09:00:00.000Z"
labels: ["web", "navigation"]
order: "a00H"
---

# Role-aware navigation menu builder

`NavigationMenuBuilder` constructs the sidebar menu tree from the current user's role/capabilities rather than a static route list — the seam every future page group (Directory, Tasks, Reports, Admin) plugs into.

Files: `Simando.Application/Navigation/{NavigationMenuBuilder,NavModels}.cs`.
