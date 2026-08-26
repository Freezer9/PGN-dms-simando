---
id: "per-circuit-icurrentuser-implementation-2026-08-07"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-07T09:00:00.000Z"
completedAt: "2026-08-07T09:00:00.000Z"
labels: ["rbac", "web"]
order: "a00F"
---

# Per-circuit ICurrentUser implementation

`CurrentUser` resolves the signed-in user's scope, area/region and capabilities once per Blazor circuit, the concrete implementation the global query filters and policy handlers will depend on.

Files: `Simando.Web/Security/CurrentUser.cs`.
