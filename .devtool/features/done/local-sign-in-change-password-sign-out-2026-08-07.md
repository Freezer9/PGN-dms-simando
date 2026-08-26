---
id: "local-sign-in-change-password-sign-out-2026-08-07"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-07T09:00:00.000Z"
completedAt: "2026-08-07T09:00:00.000Z"
labels: ["identity", "web"]
order: "a00D"
---

# Local sign-in / change-password / sign-out

`AccountController` — plain MVC controller (not a Razor component) so cookie writes work correctly under interactive server render mode, per `docs/build/web-conventions.md`. Reference implementation for every future response-mutating endpoint (attachment downloads, document generation, Excel export).

Files: `Simando.Web/Controllers/AccountController.cs`, `Components/Account/Pages/{SignIn,ChangePassword}.razor`.
