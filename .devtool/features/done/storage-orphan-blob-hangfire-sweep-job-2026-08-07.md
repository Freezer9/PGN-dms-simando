---
id: "storage-orphan-blob-hangfire-sweep-job-2026-08-07"
status: "done"
priority: "low"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-10T15:00:00.000Z"
completedAt: "2026-08-10T15:00:00.000Z"
labels: ["storage", "infra"]
order: "a00x"
---

# Storage: orphan-blob Hangfire sweep job

Daily job listing blobs with no matching `attachment` row, older than 24 hours, calling `DeleteOrphanAsync`. Age threshold matters — without it the sweep races in-flight uploads.
