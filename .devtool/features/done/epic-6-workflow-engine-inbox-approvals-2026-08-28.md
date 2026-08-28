---
id: "epic-6-workflow-engine-inbox-approvals-2026-08-28"
status: "done"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-28T00:00:00.000Z"
modified: "2026-08-28T12:00:00.000Z"
completedAt: "2026-08-28T12:00:00.000Z"
labels: ["backend", "frontend", "workflow", "tasks"]
order: "a5"
---

# Epic 6: Workflow Engine, Inbox & Approval Actions

Implement the approval workflow engine endpoints, user tasks inbox (Tugas Aktif), stuck step monitoring (Tugas Tertahan), action dialogs for Setuju/Revisi/Tolak, and action history audit logs.

## User Stories & Scope

- [x] **Story 6.1:** Build Backend Tasks & Workflow APIs (`GET /api/tasks/inbox`, `GET /api/tasks/region`, `GET /api/tasks/blocked`, `GET /api/tasks/history`, `GET /api/tasks/summary`, `POST /api/workflow/steps/{stepId}/act`, `POST /api/workflow/steps/{stepId}/reassign`).
- [x] **Story 6.2:** Build Frontend Approvals Inbox (`/tasks`): Pending action cards, quick-preview summary drawer, and action dialogs for **Setuju**, **Revisi** (with reason & return target), and **Tolak** with TanStack Form.
- [x] **Story 6.3:** Build Frontend Tugas Tertahan (`/tasks/blocked`) for Regional Admin / SysAdmin monitoring of bottlenecks.
- [x] **Story 6.4:** Build Frontend Action History & Audit Log (`/tasks/history`): Filterable timeline with user, step, action, and comment history.

## Acceptance Criteria

1. Tasks Inbox shows only records currently at the user's turn (`assigned_user_id` or role assignment step).
2. Action dialogs require reason comments for `Revisi` and `Tolak` actions.
3. `Revisi` action correctly moves the workflow back to the target step (e.g. Sales Area for editing).
4. Full audit trail is logged into `StatusEvent` and viewable in `/tasks/history`.
