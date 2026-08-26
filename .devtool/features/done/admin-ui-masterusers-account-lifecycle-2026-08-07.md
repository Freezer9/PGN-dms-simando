---
id: "admin-ui-masterusers-account-lifecycle-2026-08-07"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-08T12:00:00.000Z"
completedAt: "2026-08-08T12:00:00.000Z"
labels: ["admin-ui", "identity", "phase-0"]
order: "a00k"
---

# Admin UI: /master/users (account lifecycle)

Create user + temp password, assign role + scope, deactivate. Two-tier: System Admin (any role, any region) vs Regional Admin (Sales Area/Area Head/Reviewer, own region only — cannot appoint Regional Admin or Division Head, cannot cross regions). Sort by `last_login_at`, flag dormant accounts. No self-modification. See `docs/design/roles-permissions.md#5-identity--user-management`.

Shipped as a bespoke `IUserService` (not `IEntityService<T>` — `ApplicationUser`/`RoleAssignment` aren't `AuditableEntity`, and creation needs `UserManager<ApplicationUser>` for password hashing). The System Admin/Regional Admin escalation rule is enforced by a new `PermissionEvaluator.CanAssignRole` (closes a gap `RoleCapabilities.cs` had flagged as unimplemented), checked server-side and used to filter the role picker client-side. List sorts by `LastLoginAt` (never-logged-in first) with a summary line flagging never-logged-in and >90-day-dormant counts.

Not built: a formal audit trail for create/role-change/deactivate/reset actions — cancelled per user decision, not tracked anywhere further.
