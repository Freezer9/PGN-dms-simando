---
id: "permission-model-scope-capability-turn-2026-08-07"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-07T09:00:00.000Z"
completedAt: "2026-08-07T09:00:00.000Z"
labels: ["rbac", "domain"]
order: "a000"
---

# Permission model: scope, capability, turn

`AccessScope`, `Capability`, `ICurrentUser` and `PermissionEvaluator` implement the three-question model (which records / what action / whose turn) from `docs/design/roles-permissions.md#1`. All three must hold before an action is permitted.

Files: `Simando.Domain/Security/{AccessScope,Capability,ICurrentUser,PermissionEvaluator}.cs`.
