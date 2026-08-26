---
id: "audit-append-only-statusevent-trigger-2026-08-07"
status: "done"
priority: "critical"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-07T21:05:22.000Z"
completedAt: "2026-08-07T21:05:22.000Z"
labels: ["audit", "infra"]
order: "a00V"
---

# Audit: append-only status_event trigger

Database trigger rejecting `UPDATE`/`DELETE` on `status_event` — application-level discipline alone is not acceptable for a table backing signed commercial documents. Current status is a projection of this log; if they disagree, the log wins.
