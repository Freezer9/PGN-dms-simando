---
id: "storage-authorising-attachment-download-controller-2026-08-07"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-09T09:20:00.000Z"
completedAt: "2026-08-09T09:20:00.000Z"
labels: ["storage", "attachments"]
order: "a00w"
---

# Storage: authorising attachment download controller

Every download streams through a controller action that re-checks scope + capability — no pre-signed S3 URLs, no Graph `downloadUrl`, ever, in either provider. See `docs/build/web-conventions.md#where-this-applies-next`.
