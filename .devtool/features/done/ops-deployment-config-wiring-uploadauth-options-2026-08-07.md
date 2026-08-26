---
id: "ops-deployment-config-wiring-uploadauth-options-2026-08-07"
status: "done"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-10T15:00:00.000Z"
completedAt: "2026-08-10T15:00:00.000Z"
labels: ["infra", "ops"]
order: "a01b"
---

# Ops: deployment config wiring (Upload/Auth options)

Typed `IOptions<UploadOptions>`/`IOptions<AuthOptions>` bound at startup from `appsettings.json` + env overrides. Upload size must be raised together across nginx, Kestrel and Blazor `InputFile` — three layers, one runbook item, or a 413 shows up wherever you forgot.
