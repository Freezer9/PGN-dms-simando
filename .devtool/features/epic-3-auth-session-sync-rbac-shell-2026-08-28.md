---
id: "epic-3-auth-session-sync-rbac-shell-2026-08-28"
status: "backlog"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-28T00:00:00.000Z"
modified: "2026-08-28T00:00:00.000Z"
completedAt: null
labels: ["backend", "frontend", "auth", "rbac"]
order: "a2"
---

# Epic 3: Authentication, Session Sync & RBAC Shell

Implement backend cookie-based authentication, user session sync, frontend login/password change workflows, and role-based app shell navigation.

## User Stories & Scope

- [ ] **Story 3.1:** Implement Backend Auth API (`POST /api/auth/login`, `POST /api/auth/logout`, `GET /api/auth/me`, `POST /api/auth/change-password`).
- [ ] **Story 3.2:** Build Frontend Auth Pages (`/sign-in`, `/change-password`, `/access-denied`) using **TanStack Form**, shadcn/ui components, and Zod validation.
- [ ] **Story 3.3:** Build React Auth Context & Route Guards: enforce authentication, redirect to `/change-password` when `mustChangePassword == true`.
- [ ] **Story 3.4:** Build App Shell (`_auth` layout): responsive sidebar, topbar, role badges, breadcrumb tracking, user profile menu, and sign-out action.
- [ ] **Story 3.5:** Implement dynamic navigation filtering matching the backend `NavigationMenuBuilder` based on the user's active `EffectivePermissions`.

## Acceptance Criteria

1. Users can sign in by email + password; successful login issues secure HttpOnly authentication cookie.
2. Users with `must_change_password == true` are locked to `/change-password` until updated.
3. App shell displays active user profile, scope label ("Seluruh Region", "Region X", or "Area Y"), and role badges.
4. Navigation menu items dynamically appear/hide according to the user's evaluated capabilities (matching backend RBAC matrix).
5. Unauthorized route access redirects cleanly to `/sign-in` or `/access-denied`.
