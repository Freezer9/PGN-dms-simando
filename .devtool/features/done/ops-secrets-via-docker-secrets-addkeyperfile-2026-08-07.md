---
id: "ops-secrets-via-docker-secrets-addkeyperfile-2026-08-07"
status: "done"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-10T15:00:00.000Z"
completedAt: "2026-08-10T15:00:00.000Z"
labels: ["infra", "ops"]
order: "a01c"
---

# Ops: secrets via Docker secrets / AddKeyPerFile

Mounted secret files preferred over env vars where the platform offers them. Add a secret scan to CI (gitleaks) as the backstop for `git add -f`.
