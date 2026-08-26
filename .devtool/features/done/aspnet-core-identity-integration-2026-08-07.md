---
id: "aspnet-core-identity-integration-2026-08-07"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-07T09:00:00.000Z"
completedAt: "2026-08-07T09:00:00.000Z"
labels: ["identity", "infra"]
order: "a00B"
---

# ASP.NET Core Identity integration

`ApplicationUser` (extends `IdentityUser<Guid>`) and `ApplicationUserClaimsPrincipalFactory` project role/scope onto the auth cookie's claims, per `docs/design/roles-permissions.md#5-identity--user-management`.

Files: `Simando.Infrastructure/Identity/{ApplicationUser,ApplicationUserClaimsPrincipalFactory}.cs`.
