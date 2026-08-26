---
id: "roleassignment-entity-multi-role-scoped-2026-08-07"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-07T09:00:00.000Z"
completedAt: "2026-08-07T09:00:00.000Z"
labels: ["rbac", "domain"]
order: "a002"
---

# RoleAssignment entity (multi-role, scoped)

Many-to-many user↔role assignment carrying its own `area_id`/`region_id` scope, per `docs/design/roles-permissions.md#multi-role-users`. Supports a person holding more than one role (e.g. Sales Area in one Area + Reviewer at Region level).

Files: `Simando.Domain/Security/RoleAssignment.cs`, `Infrastructure/.../RoleAssignmentConfiguration.cs`.
