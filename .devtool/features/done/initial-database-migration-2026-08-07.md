---
id: "initial-database-migration-2026-08-07"
status: "done"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-07T09:00:00.000Z"
completedAt: "2026-08-07T09:00:00.000Z"
labels: ["infra", "persistence"]
order: "a00A"
---

# Initial database migration

Consolidated `InitialCreate` migration covering ASP.NET Core Identity tables, Region, Area and RoleAssignment. Superseded two earlier incremental migrations (`AddOrganisationHierarchy`, `AddIdentityAndRoleAssignment`) that were squashed before the schema stabilises.

Files: `Simando.Infrastructure/Persistence/Migrations/20260807140846_InitialCreate*.cs`.
