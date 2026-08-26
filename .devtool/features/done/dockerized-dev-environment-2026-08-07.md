---
id: "dockerized-dev-environment-2026-08-07"
status: "done"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-07T09:00:00.000Z"
completedAt: "2026-08-07T09:00:00.000Z"
labels: ["infra", "devops"]
order: "a00J"
---

# Dockerized dev environment

`docker-compose.dev.yml` (Postgres+PostGIS, RustFS as the S3-compatible store) plus `scripts/dev.sh` / `dev.ps1` for hot-reload (dotnet watch + Tailwind watcher together).

Note: docs describe the storage layer generically as "MinIO"; the dev/prod compose files use RustFS — same `IAttachmentStore` interface, different concrete image. Not a docs bug.
