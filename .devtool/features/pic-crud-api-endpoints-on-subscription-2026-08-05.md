---
id: "pic-crud-api-endpoints-on-subscription-2026-08-05"
status: "backlog"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-05T09:00:00.000Z"
modified: "2026-08-05T09:00:00.000Z"
completedAt: null
labels: ["epic-pic", "api"]
order: "a7"
---

# PIC CRUD API endpoints on subscription

Add endpoints to create/update/list/delete PIC contacts for a subscription.

## Context
`SubscriptionsController.cs` currently has no surface for contact data. This follows the existing controller conventions (role `SalesArea` for writes, consistent with how the rest of the subscription is edited).

## Acceptance Criteria
- [ ] `GET /api/subscriptions/{id}/contacts`, `POST`, `PUT /{contactId}`, `DELETE /{contactId}` added to `SubscriptionsController.cs`
- [ ] Authorization matches existing subscription-edit role (`SalesArea`)
- [ ] Activity log entry written on contact add/edit/remove, consistent with `ActivityLogs` usage elsewhere
