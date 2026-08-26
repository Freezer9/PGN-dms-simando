---
id: "admin-bootstrap-seeding-2026-08-07"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-07T09:00:00.000Z"
completedAt: "2026-08-07T09:00:00.000Z"
labels: ["identity", "infra"]
order: "a00C"
---

# Admin bootstrap seeding

`AdminSeeder` + `seed-admin` CLI command solve the bootstrapping gap: with no directory to authenticate against, the first System Admin must be created out-of-band, with `must_change_password = true` (`docs/design/roles-permissions.md#bootstrapping-the-first-admin`).

Files: `Simando.Infrastructure/Identity/{AdminSeeder,AdminSeedResult}.cs`, `Simando.Web/Cli/SeedAdminCommand.cs`.

Remaining: go-live checklist item to deactivate the seed account once a real admin exists (tracked in "Ops — pre-go-live checklist execution").
