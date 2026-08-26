---
id: "security-break-glass-single-record-read-2026-08-07"
status: "done"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-07T09:00:00.000Z"
modified: "2026-08-10T10:15:00.000Z"
completedAt: "2026-08-10T10:15:00.000Z"
labels: ["security", "identity"]
order: "a00T"
---

# Security: break-glass single-record read

System Admin can request temporary read-only access to one record: 60-minute expiry, written reason required, `BREAK_GLASS` event on both the record timeline and audit log, in-app notification to the record's Regional Admin and Division Head. See `docs/design/roles-permissions.md#break-glass`.
