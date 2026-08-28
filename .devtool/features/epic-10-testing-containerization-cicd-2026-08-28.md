---
id: "epic-10-testing-containerization-cicd-2026-08-28"
status: "backlog"
priority: "high"
assignee: null
dueDate: null
created: "2026-08-28T00:00:00.000Z"
modified: "2026-08-28T00:00:00.000Z"
completedAt: null
labels: ["devops", "testing", "docker", "e2e"]
order: "a9"
---

# Epic 10: Testing, Containerization & CI/CD

Configure Docker compose multi-container orchestration, backend integration tests with Testcontainers, frontend Vitest unit tests, and Playwright E2E smoke tests.

## User Stories & Scope

- [ ] **Story 10.1:** Update Dockerfiles & `docker-compose.yml`: multi-stage build for .NET Web API and Nginx container for React static assets (or combined reverse-proxy setup).
- [ ] **Story 10.2:** Migrate/Update Backend Integration Tests (`Simando.Integration.Tests`) to test API controllers using `WebApplicationFactory<Program>` with real PostGIS & MinIO Testcontainers.
- [ ] **Story 10.3:** Setup Frontend Vitest testing for custom hooks, validation schemas, and TanStack Table filters.
- [ ] **Story 10.4:** Setup Playwright E2E test suite covering sign-in, company registration, survey completion, and approval flow.

## Acceptance Criteria

1. `docker compose up --build` runs the entire stack (PostgreSQL + PostGIS, S3 storage, .NET 10 Web API, React SPA via reverse proxy).
2. `dotnet test` runs and passes all unit and integration tests against real Testcontainers.
3. Playwright E2E test passes the full end-to-end sales pipeline happy path from prospect creation through survey and approval to NOL issuance.
