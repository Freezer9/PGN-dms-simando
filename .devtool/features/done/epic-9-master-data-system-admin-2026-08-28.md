---
id: "epic-9-master-data-system-admin-2026-08-28"
status: "done"
priority: "medium"
assignee: null
dueDate: null
created: "2026-08-28T00:00:00.000Z"
modified: "2026-08-29T01:10:00.000Z"
completedAt: "2026-08-29T01:10:00.000Z"
labels: ["backend", "frontend", "admin", "master-data"]
order: "a8"
---

# Epic 9: Master Data & System Administration

Implement system administration features: user management, role assignments, organisation hierarchy (Regions/Areas), master data lookup CRUDs, and emergency break-glass procedures.

## User Stories & Scope

- [x] **Story 9.1:** Backend Master Data CRUD APIs (`/api/admin/users`, `/api/admin/organisation`, `/api/admin/master/*`).
- [x] **Story 9.2:** Frontend User Management (`/admin/users`): User list table, modal for creating users with TanStack Form, assigning roles/areas/regions, password reset, activate/deactivate.
- [x] **Story 9.3:** Frontend Organisation Management (`/admin/organisation`): Region and Area tree view and editing with TanStack Form.
- [x] **Story 9.4:** Frontend Master Data Lookup Tables (`/admin/master/*`): Reusable CRUD grid component for Industry Types, Segments, Fuel Types, Units, Meter Sizes, MRS Specs, Reason Categories, Reference Docs.
- [x] **Story 9.5:** Emergency Admin Tools: Break-glass activation modal and audit log (`/admin/break-glass`), Stuck step reassignment (`/admin/stuck-steps`).

## Acceptance Criteria

1. System Admin can manage users, assign single or multiple active roles, and set area/region scope constraints.
2. Organisation tree accurately reflects regions and child areas.
3. Master data lookup entities support full CRUD with duplicate-name prevention and soft-delete/deactivate protections.
4. Break-glass emergency access logs full reason and duration into audit history.
